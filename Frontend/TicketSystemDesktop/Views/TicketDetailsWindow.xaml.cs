using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TicketSystemDesktop.Models;
using TicketSystemDesktop.Services;

namespace TicketSystemDesktop.Views
{
    public partial class TicketDetailsWindow : Window
    {
        private readonly int _ticketId;
        private readonly User _currentUser;
        private readonly ApiService _apiService;
        private Ticket? _currentTicket;

        public TicketDetailsWindow(int ticketId, User user, ApiService apiService)
        {
            InitializeComponent();
            _ticketId = ticketId;
            _currentUser = user;
            _apiService = apiService;

            LoadTicketDetails();
        }

        private async void LoadTicketDetails()
        {
            var details = await _apiService.GetTicketDetails(_ticketId);

            if (details?.Ticket != null)
            {
                _currentTicket = details.Ticket;

                lblTicketNumber.Text = details.Ticket.TicketNumber;
                lblSubject.Text = details.Ticket.Subject;
                lblPriority.Text = details.Ticket.Priority;
                lblStatus.Text = details.Ticket.Status;
                lblCreatedDate.Text = details.Ticket.CreatedDate.ToString("MM/dd/yyyy HH:mm");
                lblAssignedTo.Text = details.Ticket.AssignedToName ?? "Unassigned";
                txtDescription.Text = details.Ticket.Description;

                // Load history and comments
                dgHistory.ItemsSource = details.StatusHistory;
                dgComments.ItemsSource = details.Comments;

                // Show appropriate action panel
                if (_currentUser.Role == "Admin")
                {
                    pnlAdminActions.Visibility = Visibility.Visible;
                    await LoadAdmins();

                    // Set current values
                    if (details.Ticket.AssignedTo.HasValue)
                    {
                        var assignedAdmin = cboAssignTo.Items.Cast<User>()
                            .FirstOrDefault(u => u.UserId == details.Ticket.AssignedTo.Value);
                        cboAssignTo.SelectedItem = assignedAdmin;
                    }

                    var statusItem = cboStatus.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == details.Ticket.Status);
                    cboStatus.SelectedItem = statusItem;

                    // Disable editing if closed
                    if (details.Ticket.Status == "Closed")
                    {
                        cboAssignTo.IsEnabled = false;
                        cboStatus.IsEnabled = false;
                        txtAdminComment.IsEnabled = false;
                        btnSave.IsEnabled = false;
                    }
                }
                else
                {
                    // User can add comments only if ticket is not closed
                    if (details.Ticket.Status != "Closed" && details.Ticket.CreatedBy == _currentUser.UserId)
                    {
                        pnlUserComment.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task LoadAdmins()
        {
            var admins = await _apiService.GetAdmins();
            cboAssignTo.ItemsSource = admins;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTicket == null) return;

            btnSave.IsEnabled = false;
            btnSave.Content = "Saving...";

            var request = new UpdateTicketRequest
            {
                TicketId = _ticketId,
                UpdatedBy = _currentUser.UserId,
                AssignedTo = (cboAssignTo.SelectedItem as User)?.UserId,
                Status = (cboStatus.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Comment = string.IsNullOrWhiteSpace(txtAdminComment.Text) ? null : txtAdminComment.Text.Trim()
            };

            var response = await _apiService.UpdateTicket(request);

            if (response.Success)
            {
                MessageBox.Show("Ticket updated successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(response.Message, "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                btnSave.IsEnabled = true;
                btnSave.Content = "Save Changes";
            }
        }

        private async void BtnAddComment_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUserComment.Text))
            {
                MessageBox.Show("Please enter a comment", "Validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnAddComment.IsEnabled = false;
            btnAddComment.Content = "Adding...";

            var request = new AddCommentRequest
            {
                TicketId = _ticketId,
                CommentText = txtUserComment.Text.Trim(),
                CommentedBy = _currentUser.UserId,
                IsInternal = false
            };

            var response = await _apiService.AddComment(request);

            if (response.Success)
            {
                MessageBox.Show("Comment added successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                txtUserComment.Clear();
                LoadTicketDetails();
            }
            else
            {
                MessageBox.Show(response.Message, "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            btnAddComment.IsEnabled = true;
            btnAddComment.Content = "Add Comment";
        }
    }
}
