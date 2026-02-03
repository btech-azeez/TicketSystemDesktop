using System.Windows;
using System.Windows.Controls;
using TicketSystemDesktop.Models;
using TicketSystemDesktop.Services;

namespace TicketSystemDesktop.Views
{
    public partial class CreateTicketWindow : Window
    {
        private readonly User _currentUser;
        private readonly ApiService _apiService;

        public CreateTicketWindow(User user, ApiService apiService)
        {
            InitializeComponent();
            _currentUser = user;
            _apiService = apiService;
        }

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            lblMessage.Text = "";

            if (string.IsNullOrWhiteSpace(txtSubject.Text))
            {
                lblMessage.Text = "Please enter a subject";
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                lblMessage.Text = "Please enter a description";
                return;
            }

            btnSubmit.IsEnabled = false;
            btnSubmit.Content = "Creating...";

            var request = new CreateTicketRequest
            {
                Subject = txtSubject.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Priority = (cboPriority.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Medium",
                CreatedBy = _currentUser.UserId
            };

            var response = await _apiService.CreateTicket(request);

            if (response.Success)
            {
                MessageBox.Show("Ticket created successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                lblMessage.Text = response.Message;
                btnSubmit.IsEnabled = true;
                btnSubmit.Content = "Submit";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
