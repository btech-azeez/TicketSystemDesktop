namespace TicketSystemAPI.Models
{
    public class TicketStatusHistory
    {
        public int HistoryId { get; set; }
        public int TicketId { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public int ChangedBy { get; set; }
        public DateTime ChangedDate { get; set; }
        public string? Comments { get; set; }
        
        // Additional property for display
        public string? ChangedByName { get; set; }
    }
}
