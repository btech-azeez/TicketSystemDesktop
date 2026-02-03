using System.Windows;
using TicketSystemDesktop.Models;
using TicketSystemDesktop.Services;

namespace TicketSystemDesktop.Views
{
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService;

        public LoginWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            this.MouseLeftButtonDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); };
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            lblMessage.Text = "";

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                lblMessage.Text = "Please enter username";
                return;
            }

            if (string.IsNullOrEmpty(txtPassword.Password))
            {
                lblMessage.Text = "Please enter password";
                return;
            }

            btnLogin.IsEnabled = false;
            btnLogin.Content = "Logging in...";

            var loginRequest = new LoginRequest
            {
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Password
            };

            var response = await _apiService.Login(loginRequest);

            if (response.Success && response.User != null)
            {
                var mainWindow = new MainWindow(response.User, _apiService);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                lblMessage.Text = response.Message;
                btnLogin.IsEnabled = true;
                btnLogin.Content = "Login";
            }
        }
    }
}
