using AITrainer;
using Business;
using Repository;
using Repository.Models;
using System.Windows;
using System.Windows.Controls;

namespace WPFAPP
{
    public partial class LoginWindow : Page
    {
        private readonly AccountBusiness _accountBus;

        public LoginWindow()
        {
            InitializeComponent();

            var db = new AiIeltsDbContext();
            _accountBus = new AccountBusiness(db);
        }

        /* ======================================================
         * LOGIN CLICK
         * ====================================================== */
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Password.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter username!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter password!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
  
            // Call Business
            var result = await _accountBus.LoginAsync(username, password);

            if (result == null)
            {
                MessageBox.Show("Invalid username or password!", "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Login OK
            MessageBox.Show("Login successful!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
            AppSession.CurrentUser = result;

            // Điều hướng sang trang khác (HomePage hoặc Dashboard)
            NavigationService?.Navigate(new HomePage());
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SignUpWindow());
            return;
        }
    }
}
