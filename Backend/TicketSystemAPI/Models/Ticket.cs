namespace TicketSystemAPI.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "Open";
        public int CreatedBy { get; set; }
        public int? AssignedTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        
        // Additional properties for display
        public string? CreatedByName { get; set; }
        public string? AssignedToName { get; set; }
    }

    public class CreateTicketRequest
    {
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public int CreatedBy { get; set; }
    }

    public class UpdateTicketRequest
    {
        public int TicketId { get; set; }
        public int? AssignedTo { get; set; }
        public string? Status { get; set; }
        public int UpdatedBy { get; set; }
        public string? Comment { get; set; }
    }

    public class TicketDetailsResponse
    {
        public Ticket? Ticket { get; set; }
        public List<TicketStatusHistory>? StatusHistory { get; set; }
        public List<TicketComment>? Comments { get; set; }
    }
}
