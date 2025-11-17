using Repository;
using Repository.DAO;
using Repository.Models;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace WPFAPP
{
    public partial class SettingScreen : Page
    {
        private readonly APIKeyDAO _apiKeyDao;
        private readonly int _userId = AppSession.CurrentUser.UserId;  // tạm hardcode, sau đổi theo user login

        public SettingScreen()
        {
            InitializeComponent();
            _apiKeyDao = new APIKeyDAO(new AiIeltsDbContext());

            Loaded += SettingScreen_Loaded;
        }

        private async void SettingScreen_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var key = await _apiKeyDao.GetApiKeyAsync(_userId);

                if (key != null)
                {
                    txtGptApi.Text = key.ChatGptkey ?? "";
                    txtDeepgramApi.Text = key.DeepgramKey ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load API key: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string gptKey = txtGptApi.Text.Trim();
            string dgKey = txtDeepgramApi.Text.Trim();

            if (string.IsNullOrEmpty(gptKey) || string.IsNullOrEmpty(dgKey))
            {
                MessageBox.Show("Both API keys are required.",
                    "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _apiKeyDao.SaveApiKeyAsync(_userId, dgKey, gptKey);

                MessageBox.Show("API keys saved successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving API keys: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Home_Click(object sender, MouseButtonEventArgs e)
        {
            var w = new HomePage();
            NavigationService?.Navigate(w);
        }


        private void Profile_Click(object sender, MouseButtonEventArgs e)
        {
            var p = new Profile(); 
            NavigationService?.Navigate(p);
        }
    }
}