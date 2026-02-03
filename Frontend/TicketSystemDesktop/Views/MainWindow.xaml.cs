using System.Windows;
using System.Windows.Input;
using TicketSystemDesktop.Models;
using TicketSystemDesktop.Services;

namespace TicketSystemDesktop.Views
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private readonly ApiService _apiService;

        public MainWindow(User user, ApiService apiService)
        {
            InitializeComponent();
            _currentUser = user;
            _apiService = apiService;

            lblUserInfo.Text = $"{_currentUser.FullName} ({_currentUser.Role})";

            // Show All Tickets tab for Admin
            if (_currentUser.Role == "Admin")
            {
                tabAllTickets.Visibility = Visibility.Visible;
            }

            LoadTickets();
        }

        private async void LoadTickets()
        {
            if (_currentUser.Role == "Admin")
            {
                var allTickets = await _apiService.GetAllTickets();
                dgAllTickets.ItemsSource = allTickets;
            }

            var userTickets = await _apiService.GetUserTickets(_currentUser.UserId);
            dgTickets.ItemsSource = userTickets;
        }

        private void BtnCreateTicket_Click(object sender, RoutedEventArgs e)
        {
            var createWindow = new CreateTicketWindow(_currentUser, _apiService);
            createWindow.Owner = this;
            if (createWindow.ShowDialog() == true)
            {
                LoadTickets();
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            var userTickets = await _apiService.GetUserTickets(_currentUser.UserId);
            dgTickets.ItemsSource = userTickets;
        }

        private async void BtnRefreshAll_Click(object sender, RoutedEventArgs e)
        {
            var allTickets = await _apiService.GetAllTickets();
            dgAllTickets.ItemsSource = allTickets;
        }

        private void DgTickets_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgTickets.SelectedItem is Ticket ticket)
            {
                var detailsWindow = new TicketDetailsWindow(ticket.TicketId, _currentUser, _apiService);
                detailsWindow.Owner = this;
                if (detailsWindow.ShowDialog() == true)
                {
                    LoadTickets();
                }
            }
        }

        private void DgAllTickets_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgAllTickets.SelectedItem is Ticket ticket)
            {
                var detailsWindow = new TicketDetailsWindow(ticket.TicketId, _currentUser, _apiService);
                detailsWindow.Owner = this;
                if (detailsWindow.ShowDialog() == true)
                {
                    LoadTickets();
                }
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
