using System;
using System.Windows;

namespace WPFAPP
{
    /// <summary>
    /// Interaction logic for SpeakingPractice.xaml
    /// </summary>
    public partial class SpeakingPractice : Window
    {
        public SpeakingPractice()
        {
            InitializeComponent();
            Loaded += SpeakingPractice_Loaded;
            btnGenerate.Click += btnGenerate_Click;
        }

        private void SpeakingPractice_Loaded(object? sender, RoutedEventArgs e)
        {
            // Initial states
            txtTopicPlaceholder.Visibility = Visibility.Visible;
            txtTopic.Visibility = Visibility.Collapsed;

            txtRecordStatus.Text = "Click the Button To Start Recording or Upload your Audio File";
            txtUploadStatus.Text = string.Empty;
        }

        private void btnGenerate_Click(object? sender, RoutedEventArgs e)
        {
            // Replace this with your real generation logic.
            // Example: show generated topic and hide placeholder.
            string generated = "Describe a technology that changed your life."; // sample
            txtTopic.Text = generated;
            txtTopic.Visibility = Visibility.Visible;
            txtTopicPlaceholder.Visibility = Visibility.Collapsed;
        }
    }
}
