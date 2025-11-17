using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NAudio.Wave;
using Service;              // 👈 nhớ thêm namespace Service
using System.Threading.Tasks;
using Repository;
using Repository.DAO;
using Repository.Models;

namespace WPFAPP
{
    public partial class SpeakingWindow : Page
    {
        private WaveInEvent _waveIn;
        private WaveFileWriter _writer;
        private bool _isRecording = false;
        private string _currentFilePath;
        private DateTime _recordStartTime;
        private DispatcherTimer _timer;
        private SpeakingQuestionDAO _dao;
        private SpeakingQuestion _currentQuestion;
        // 👇 Thêm mấy field này
        private AISpeakingService? _speakingService;
        private readonly int _userId;

        // ✅ Nhận userId để lấy API key đúng user
        public SpeakingWindow()
        {
            InitializeComponent();

            _userId = AppSession.CurrentUser.UserId;

            // Timer để cập nhật 00:00
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            // Khởi tạo AI service async sau khi UI đã load
            Loaded += SpeakingWindow_Loaded;
            _dao = new SpeakingQuestionDAO();
        }

        // Khởi tạo AISpeakingService không bị .Result deadlock
        private async void SpeakingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _speakingService = await AISpeakingService.CreateAsync(_userId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Init Speaking AI Service failed: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        // ==========================
        // 1️⃣ Generate speaking topic (OpenAI)
        // ==========================
        private async void GenerateTopic_Click(object sender, RoutedEventArgs e)
        {
            if (_speakingService == null)
            {
                MessageBox.Show("Speaking service is not ready yet.");
                return;
            }

            try
            {
                GenerateButton.IsEnabled = false;
                TopicTextBox.Text = "Generating topic...";

                var topic = await _speakingService.GenerateSpeakingPromptAsync();
                TopicTextBox.Text = topic;
                _currentQuestion = await _dao.SaveQuestionAsync(topic);
            }
            catch (Exception ex)
            {
                TopicTextBox.Text = "Failed to generate topic.";
                MessageBox.Show("Generate topic failed: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                GenerateButton.IsEnabled = true;
            }
        }


        // Ghi âm bằng NAudio

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }

        private void StartRecording()
        {
            try
            {
                // Tạo folder lưu file
                string folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "WPFAPP_Recordings");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                _currentFilePath = Path.Combine(
                    folder,
                    $"speaking_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

                _waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(44100, 1) // 44.1kHz, mono
                };

                _waveIn.DataAvailable += WaveIn_DataAvailable;
                _waveIn.RecordingStopped += WaveIn_RecordingStopped;

                _writer = new WaveFileWriter(_currentFilePath, _waveIn.WaveFormat);

                _waveIn.StartRecording();
                _isRecording = true;

                // UI
                RecordButtonText.Text = "Stop";
                RecordStatusTextBlock.Text = "Recording...";
                SubmitButton.IsEnabled = false;
                _recordStartTime = DateTime.Now;
                TimerTextBlock.Text = "00:00";
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while starting recording: " + ex.Message);
            }
        }

        private void StopRecording()
        {
            if (_waveIn != null)
            {
                _waveIn.StopRecording();
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_writer == null) return;
            _writer.Write(e.Buffer, 0, e.BytesRecorded);
            _writer.Flush();
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            _timer.Stop();

            _waveIn.Dispose();
            _waveIn = null;

            _writer.Close();
            _writer.Dispose();
            _writer = null;

            _isRecording = false;

            // UI
            Dispatcher.Invoke(() =>
            {
                RecordButtonText.Text = "Record";
                RecordStatusTextBlock.Text = "Recording stopped.";
                SubmitButton.IsEnabled = !string.IsNullOrEmpty(_currentFilePath);
                LastFileTextBlock.Text = $"Saved: {_currentFilePath}";
            });

            if (e.Exception != null)
            {
                MessageBox.Show("Recording error: " + e.Exception.Message);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var elapsed = DateTime.Now - _recordStartTime;
            TimerTextBlock.Text = $"{elapsed.Minutes:00}:{elapsed.Seconds:00}";
        }


        // Submit: Transcribe + Grade (Deepgram + OpenAI)

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (_speakingService == null)
            {
                MessageBox.Show("Speaking service is not ready.");
                return;
            }

            if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
            {
                MessageBox.Show("No recording to submit yet.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TopicTextBox.Text))
            {
                MessageBox.Show("Please generate a topic first.");
                return;
            }

            try
            {
                SubmitButton.IsEnabled = false;
                SubmitButton.Content = "PROCESSING...";
                RecordStatusTextBlock.Text = "Uploading & grading...";

                // Đọc file audio thành byte[]
                var audioBytes = await File.ReadAllBytesAsync(_currentFilePath);

                // Gửi Deepgram để transcribe
                var transcript = await _speakingService.TranscribeAsync(audioBytes);

                if (string.IsNullOrWhiteSpace(transcript))
                {
                    MessageBox.Show("Transcription result is empty.");
                    return;
                }

                //  Gửi transcript + topic để chấm điểm
                var topic = TopicTextBox.Text;
                var (finalTranscript, score, feedback) =
                    await _speakingService.GradeSpeakingAsync(transcript, topic);
                await _dao.SaveAnswerAsync(_currentQuestion.QuestionId, finalTranscript, score, feedback);
                WritingScoreWindow scorePage = new WritingScoreWindow(topic, score, feedback);
                NavigationService?.Navigate(scorePage);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Submit failed: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SubmitButton.IsEnabled = true;
                SubmitButton.Content = "SUBMIT";
                RecordStatusTextBlock.Text = "Done.";
            }
        }
    }
}
