using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFAPP
{
    /// <summary>
    /// Interaction logic for WritingScoreWindow.xaml
    /// </summary>
    public partial class WritingScoreWindow : Page
    {
        public WritingScoreWindow(string topic, decimal score, string feedback)
        {
            InitializeComponent();

            ScoreTextBlock.Text = $"AI Score: {score:0.0}";
            TopicTextBlock.Text = $"Topic: {topic}";
            FeedbackTextBox.Text = feedback;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new HomePage());

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new HomePage());

        }
    }
}
