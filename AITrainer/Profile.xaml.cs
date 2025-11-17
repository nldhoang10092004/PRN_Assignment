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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFAPP
{
    /// <summary>
    /// Interaction logic for Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        int userId = AppSession.CurrentUser.UserId;
        private readonly AccountBusiness _accountBusiness;
        public Profile()
        {
            _accountBusiness = new AccountBusiness(new AiIeltsDbContext());
            InitializeComponent();
            Loaded += LoadProfileDataAsync;
        }

        private async void LoadProfileDataAsync(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem AccountBusiness đã được khởi tạo chưa
            if (_accountBusiness == null) return;

            try
            {
                Account? account = await _accountBusiness.GetAccountAsync(userId);

                if (account?.UserDetail != null)
                {
                    // Hiển thị dữ liệu lên các control
                    FullNameTextBox.Text = account.UserDetail.FullName ?? string.Empty;
                    AddressTextBox.Text = account.UserDetail.Address ?? string.Empty;

                    // Chuyển đổi DateOnly? sang DateTime? một cách an toàn
                    if (account.UserDetail.Dob.HasValue)
                    {
                        DobDatePicker.SelectedDate = account.UserDetail.Dob.Value.ToDateTime(TimeOnly.MinValue);
                    }
                    else
                    {
                        DobDatePicker.SelectedDate = null;
                    }

                    // Bỏ qua logic tải avatar theo yêu cầu của bạn
                    // UpdateAvatar(account.UserDetail.AvatarUrl); 
                }
                else if (account != null)
                {
                    // Trường hợp chỉ có Account, chưa có UserDetail (UserDetail là rỗng/null)
                    System.Windows.MessageBox.Show("Tài khoản đã được tải, nhưng thông tin chi tiết chưa được thiết lập.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    System.Windows.MessageBox.Show("Không tìm thấy thông tin tài khoản.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải hồ sơ: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
            private async void ChangeAvatar_Click(object sender, RoutedEventArgs e) { }
        private async void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
                {
                    System.Windows.MessageBox.Show("Full name is required.",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create UserDetail object with updated information
                var userDetail = new UserDetail
                {
                    UserId = userId,
                    FullName = FullNameTextBox.Text.Trim(),
                    Address = AddressTextBox.Text?.Trim(),
                    Dob = DobDatePicker.SelectedDate.HasValue 
                        ? DateOnly.FromDateTime(DobDatePicker.SelectedDate.Value) 
                        : null,
                    AvatarUrl = null // Set to null or handle avatar URL if needed
                };

                // Call business layer to update profile
                bool success = await _accountBusiness.UpdateProfileAsync(userDetail);

                if (success)
                {
                    System.Windows.MessageBox.Show("Profile updated successfully!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("Failed to update profile. Please try again.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving profile: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Setting_Click(object sender, MouseButtonEventArgs e)
        {
            var s = new SettingScreen();
            NavigationService?.Navigate(s);

        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            AppSession.CurrentUser = null;
            var hp = new LoginWindow();
            NavigationService?.Navigate(hp);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var hp = new HomePage();
            NavigationService?.Navigate(hp);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var s = new SettingScreen();
            NavigationService?.Navigate(s);
        }
    }
}
