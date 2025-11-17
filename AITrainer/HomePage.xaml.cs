using Business;
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
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private readonly HomePageBusiness _business;
        public HomePage()
        {
            _business = new HomePageBusiness();
            InitializeComponent();
            Loaded += HomePage_Loaded;
        }

        private async void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // ===== Overall band =====
                var overall = await _business.GetCurrentBandAsync();
                TxtOverallBand.Text = overall?.ToString("0.0") ?? "-";

                // ===== Writing =====
                var lastWriting = await _business.GetLatestWritingAnswerScoreAsync();

                // ===== Speaking =====
                var lastSpeaking = await _business.GetLatestSpeakingAnswerScoreAsync();
                TxtLastWritingBand.Text = lastWriting?.ToString("0.0") ?? "-";
                TxtLastSpeakingBand.Text = lastSpeaking?.ToString("0.0") ?? "-";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StartWriting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ww = new WritingWindow();
                NavigationService?.Navigate(ww);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error when opening WritingWindow",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void StartSpeaking_Click(object sender, RoutedEventArgs e)
        {
            var ww = new SpeakingWindow();
            NavigationService?.Navigate(ww);
        }
    }
}
