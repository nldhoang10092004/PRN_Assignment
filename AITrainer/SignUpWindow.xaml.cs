using Business;
using Repository;
using Repository.Models;
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
using WPFAPP;

namespace WPFAPP
{
    /// <summary>
    /// Interaction logic for SignUpWindow.xaml
    /// </summary>
    public partial class SignUpWindow : Page
    {
        private readonly AccountBusiness _accountBus;

        public SignUpWindow()
        {
            InitializeComponent();
            var db = new AiIeltsDbContext();
            _accountBus = new AccountBusiness(db);
        }

        private void txtOtp_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private async void btnSignUp_ClickAsync(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string username = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();
            string confirmPassword = txtConfirm.Password.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter username!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter email!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter password!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Please enter confirm password!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!confirmPassword.Equals(password))
            {
                MessageBox.Show("Confirm password must be the same with password!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result1 = await _accountBus.RegisterAsync(username, email, password);

            // Call Business
            var result = await _accountBus.LoginAsync(username, password);

            if (result == null)
            {
                MessageBox.Show("Invalid username or password!", "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Login OK
            MessageBox.Show("Sign Up successful!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);
            AppSession.CurrentUser = result;

            // Điều hướng sang trang khác (HomePage hoặc Dashboard)
            NavigationService?.Navigate(new HomePage());
        }
    }
}
