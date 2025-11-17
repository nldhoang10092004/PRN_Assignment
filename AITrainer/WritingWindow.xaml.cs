using Repository;
using Repository.DAO;
using Repository.Models;
using Service;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WPFAPP
{
    public partial class WritingWindow : Page
    {
        private readonly AiIeltsDbContext _db;
        private readonly WritingQuestionDAO _questionDao;
        private AIWritingService? _aiService;

        private int _userId;
        private WritingQuestion? _currentQuestion;

        public WritingWindow()
        {
            InitializeComponent();

            _db = new AiIeltsDbContext();
            _questionDao = new WritingQuestionDAO(_db);

            // TODO: lấy userId thật theo session
            _userId = AppSession.CurrentUser.UserId; 

            SubmitButton.IsEnabled = false;
        }

        // Tạo service 1 lần, dùng lại
        private async Task<bool> EnsureAiServiceAsync()
        {
            if (_aiService != null) return true;

            try
            {
                _aiService = await AIWritingService.CreateAsync(_userId);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot use AI Writing.\n" + ex.Message,
                    "API Key error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        // Generate Topic
        private async void GenerateTopic_Click(object sender, RoutedEventArgs e)
        {
            if (!await EnsureAiServiceAsync()) return;

            try
            {
                GenerateButton.IsEnabled = false;
                GenerateButton.Content = "Generating...";
                TopicTextBox.Text = "Generating topic, please wait...";

                var topic = await _aiService!.GenerateWritingPromptAsync();

                TopicTextBox.Text = topic;
                _currentQuestion = await _questionDao.SaveQuestionAsync(topic);

                UpdateSubmitButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to generate topic.\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TopicTextBox.Text = "Failed to generate topic. Try again.";
            }
            finally
            {
                GenerateButton.IsEnabled = true;
                GenerateButton.Content = "✏ Generate";
            }
        }

        private void EssayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = EssayTextBox.Text ?? string.Empty;
            var words = text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            WordCountTextBlock.Text = $"Words: {words.Length}";
            UpdateSubmitButtonState();
        }

        private void UpdateSubmitButtonState()
        {
            var hasEssay = !string.IsNullOrWhiteSpace(EssayTextBox.Text);
            var hasTopic = _currentQuestion != null && !string.IsNullOrWhiteSpace(TopicTextBox.Text);
            SubmitButton.IsEnabled = hasEssay && hasTopic;
        }

        // Submit

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (!await EnsureAiServiceAsync()) return;

            if (_currentQuestion == null)
            {
                MessageBox.Show("Please generate a topic first.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var essay = EssayTextBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(essay))
            {
                MessageBox.Show("Please write your essay before submitting.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SubmitButton.IsEnabled = false;
                SubmitButton.Content = "Grading...";

                var (score, feedback) = await _aiService!.GradeWritingAsync(essay);

                await _questionDao.SaveAnswerAsync(
                    _currentQuestion.Id,
                    essay,
                    score,
                    feedback
                );
              
                var scorePage = new WritingScoreWindow(
                    _currentQuestion.Content,
                    score,
                    feedback
                );

                NavigationService?.Navigate(scorePage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to grade your essay.\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SubmitButton.Content = "SUBMIT";
                UpdateSubmitButtonState();
            }
        }
    }
}
