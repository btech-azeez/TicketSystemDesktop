using MySql.Data.MySqlClient;
using System.Data;
using TicketSystemAPI.Models;

namespace TicketSystemAPI.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        #region User Methods

        public async Task<User?> AuthenticateUser(string username, string password)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT UserId, Username, Password, FullName, Role, Email, CreatedDate, IsActive 
                         FROM Users 
                         WHERE Username = @Username AND Password = @Password AND IsActive = 1";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Username = reader.GetString("Username"),
                    FullName = reader.GetString("FullName"),
                    Role = reader.GetString("Role"),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email"),
                    CreatedDate = reader.GetDateTime("CreatedDate"),
                    IsActive = reader.GetBoolean("IsActive")
                };
            }

            return null;
        }

        public async Task<List<User>> GetAdminUsers()
        {
            var users = new List<User>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT UserId, Username, FullName, Role, Email 
                         FROM Users 
                         WHERE Role = 'Admin' AND IsActive = 1";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    UserId = reader.GetInt32("UserId"),
                    Username = reader.GetString("Username"),
                    FullName = reader.GetString("FullName"),
                    Role = reader.GetString("Role"),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString("Email")
                });
            }

            return users;
        }

        #endregion

        #region Ticket Methods

        public async Task<string> GenerateTicketNumber()
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var query = "SELECT MAX(CAST(SUBSTRING(TicketNumber, 5) AS UNSIGNED)) FROM Tickets";
            using var command = new MySqlCommand(query, connection);

            var result = await command.ExecuteScalarAsync();
            int nextNumber = 1;

            if (result != null && result != DBNull.Value)
            {
                nextNumber = Convert.ToInt32(result) + 1;
            }

            return $"TKT-{nextNumber:D5}";
        }

        public async Task<Ticket?> CreateTicket(CreateTicketRequest request)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var ticketNumber = await GenerateTicketNumber();

            var query = @"INSERT INTO Tickets (TicketNumber, Subject, Description, Priority, Status, CreatedBy) 
                         VALUES (@TicketNumber, @Subject, @Description, @Priority, 'Open', @CreatedBy);
                         SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@TicketNumber", ticketNumber);
            command.Parameters.AddWithValue("@Subject", request.Subject);
            command.Parameters.AddWithValue("@Description", request.Description);
            command.Parameters.AddWithValue("@Priority", request.Priority);
            command.Parameters.AddWithValue("@CreatedBy", request.CreatedBy);

            var ticketId = Convert.ToInt32(await command.ExecuteScalarAsync());

            // Add initial status history
            await AddStatusHistory(ticketId, null, "Open", request.CreatedBy, "Ticket created");

            return await GetTicketById(ticketId);
        }

        public async Task<List<Ticket>> GetTicketsByUser(int userId)
        {
            var tickets = new List<Ticket>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT t.*, u1.FullName as CreatedByName, u2.FullName as AssignedToName
                         FROM Tickets t
                         LEFT JOIN Users u1 ON t.CreatedBy = u1.UserId
                         LEFT JOIN Users u2 ON t.AssignedTo = u2.UserId
                         WHERE t.CreatedBy = @UserId
                         ORDER BY t.CreatedDate DESC";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tickets.Add(MapTicketFromReader(reader));
            }

            return tickets;
        }

        public async Task<List<Ticket>> GetAllTickets()
        {
            var tickets = new List<Ticket>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT t.*, u1.FullName as CreatedByName, u2.FullName as AssignedToName
                         FROM Tickets t
                         LEFT JOIN Users u1 ON t.CreatedBy = u1.UserId
                         LEFT JOIN Users u2 ON t.AssignedTo = u2.UserId
                         ORDER BY t.CreatedDate DESC";

            using var command = new MySqlCommand(query, connection);
            using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tickets.Add(MapTicketFromReader(reader));
            }

            return tickets;
        }

        public async Task<Ticket?> GetTicketById(int ticketId)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT t.*, u1.FullName as CreatedByName, u2.FullName as AssignedToName
                         FROM Tickets t
                         LEFT JOIN Users u1 ON t.CreatedBy = u1.UserId
                         LEFT JOIN Users u2 ON t.AssignedTo = u2.UserId
                         WHERE t.TicketId = @TicketId";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@TicketId", ticketId);

            using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapTicketFromReader(reader);
            }

            return null;
        }

        public async Task<bool> UpdateTicket(UpdateTicketRequest request)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            // Get current ticket status
            var ticket = await GetTicketById(request.TicketId);
            if (ticket == null) return false;

            // Check if ticket is closed
            if (ticket.Status == "Closed" && request.Status != "Closed")
            {
                return false; // Cannot modify closed tickets
            }

            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Update ticket
                var updateQuery = @"UPDATE Tickets 
                                   SET AssignedTo = @AssignedTo, 
                                       Status = @Status,
                                       LastModifiedDate = NOW()
                                   WHERE TicketId = @TicketId";

                using var updateCommand = new MySqlCommand(updateQuery, connection, transaction as MySqlTransaction);
                updateCommand.Parameters.AddWithValue("@TicketId", request.TicketId);
                updateCommand.Parameters.AddWithValue("@AssignedTo", request.AssignedTo.HasValue ? (object)request.AssignedTo.Value : DBNull.Value);
                updateCommand.Parameters.AddWithValue("@Status", request.Status ?? ticket.Status);

                await updateCommand.ExecuteNonQueryAsync();

                // Add status history if status changed
                if (!string.IsNullOrEmpty(request.Status) && request.Status != ticket.Status)
                {
                    await AddStatusHistory(request.TicketId, ticket.Status, request.Status, request.UpdatedBy, request.Comment, transaction as MySqlTransaction);
                }

                // Add comment if provided
                if (!string.IsNullOrEmpty(request.Comment))
                {
                    await AddComment(new AddCommentRequest
                    {
                        TicketId = request.TicketId,
                        CommentText = request.Comment,
                        CommentedBy = request.UpdatedBy,
                        IsInternal = true
                    }, transaction as MySqlTransaction);
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region Status History Methods

        private async Task AddStatusHistory(int ticketId, string? oldStatus, string newStatus, int changedBy, string? comments, MySqlTransaction? transaction = null)
        {
            var connection = transaction?.Connection ?? GetConnection();
            bool shouldCloseConnection = transaction == null;

            if (shouldCloseConnection && connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var query = @"INSERT INTO TicketStatusHistory (TicketId, OldStatus, NewStatus, ChangedBy, Comments) 
                         VALUES (@TicketId, @OldStatus, @NewStatus, @ChangedBy, @Comments)";

            using var command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@TicketId", ticketId);
            command.Parameters.AddWithValue("@OldStatus", string.IsNullOrEmpty(oldStatus) ? DBNull.Value : oldStatus);
            command.Parameters.AddWithValue("@NewStatus", newStatus);
            command.Parameters.AddWithValue("@ChangedBy", changedBy);
            command.Parameters.AddWithValue("@Comments", string.IsNullOrEmpty(comments) ? DBNull.Value : comments);

            await command.ExecuteNonQueryAsync();

            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }

        public async Task<List<TicketStatusHistory>> GetTicketHistory(int ticketId)
        {
            var history = new List<TicketStatusHistory>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT h.*, u.FullName as ChangedByName
                         FROM TicketStatusHistory h
                         LEFT JOIN Users u ON h.ChangedBy = u.UserId
                         WHERE h.TicketId = @TicketId
                         ORDER BY h.ChangedDate DESC";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@TicketId", ticketId);

            using var reader = (MySqlDataReader)await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                history.Add(new TicketStatusHistory 
                {
                    HistoryId = reader.GetInt32("HistoryId"),
                    TicketId = reader.GetInt32("TicketId"),
                    OldStatus = reader.IsDBNull(reader.GetOrdinal("OldStatus")) ? null : reader.GetString("OldStatus"),
                    NewStatus = reader.GetString("NewStatus"),
                    ChangedBy = reader.GetInt32("ChangedBy"),
                    ChangedDate = reader.GetDateTime("ChangedDate"),
                    Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString("Comments"),
                    ChangedByName = reader.IsDBNull(reader.GetOrdinal("ChangedByName")) ? null : reader.GetString("ChangedByName")
                });
            }

            return history;
        }

        #endregion

        #region Comment Methods

        public async Task<bool> AddComment(AddCommentRequest request, MySqlTransaction? transaction = null)
        {
            var connection = transaction?.Connection ?? GetConnection();
            bool shouldCloseConnection = transaction == null;

            if (shouldCloseConnection && connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var query = @"INSERT INTO TicketComments (TicketId, CommentText, CommentedBy, IsInternal) 
                         VALUES (@TicketId, @CommentText, @CommentedBy, @IsInternal)";

            using var command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@TicketId", request.TicketId);
            command.Parameters.AddWithValue("@CommentText", request.CommentText);
            command.Parameters.AddWithValue("@CommentedBy", request.CommentedBy);
            command.Parameters.AddWithValue("@IsInternal", request.IsInternal);

            var result = await command.ExecuteNonQueryAsync();

            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }

            return result > 0;
        }

        public async Task<List<TicketComment>> GetTicketComments(int ticketId)
        {
            var comments = new List<TicketComment>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT c.*, u.FullName as CommentedByName
                         FROM TicketComments c
                         LEFT JOIN Users u ON c.CommentedBy = u.UserId
                         WHERE c.TicketId = @TicketId
                         ORDER BY c.CommentDate DESC";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@TicketId", ticketId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                comments.Add(new TicketComment
                {
                    CommentId = reader.GetInt32("CommentId"),
                    TicketId = reader.GetInt32("TicketId"),
                    CommentText = reader.GetString("CommentText"),
                    CommentedBy = reader.GetInt32("CommentedBy"),
                    IsInternal = reader.GetBoolean("IsInternal"),
                    CommentDate = reader.GetDateTime("CommentDate"),
                    CommentedByName = reader.IsDBNull(reader.GetOrdinal("CommentedByName")) ? null : reader.GetString("CommentedByName")
                });
            }

            return comments;
        }

        #endregion

        #region Helper Methods

        private Ticket MapTicketFromReader(MySqlDataReader reader)
        {
            return new Ticket
            {
                TicketId = reader.GetInt32("TicketId"),
                TicketNumber = reader.GetString("TicketNumber"),
                Subject = reader.GetString("Subject"),
                Description = reader.GetString("Description"),
                Priority = reader.GetString("Priority"),
                Status = reader.GetString("Status"),
                CreatedBy = reader.GetInt32("CreatedBy"),
                AssignedTo = reader.IsDBNull(reader.GetOrdinal("AssignedTo")) ? null : reader.GetInt32("AssignedTo"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                LastModifiedDate = reader.GetDateTime("LastModifiedDate"),
                CreatedByName = reader.IsDBNull(reader.GetOrdinal("CreatedByName")) ? null : reader.GetString("CreatedByName"),
                AssignedToName = reader.IsDBNull(reader.GetOrdinal("AssignedToName")) ? null : reader.GetString("AssignedToName")
            };
        }

        #endregion
    }
}
