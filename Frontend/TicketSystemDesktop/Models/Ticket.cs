using System;
using System.Collections.Generic;

namespace TicketSystemDesktop.Models
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

    public class TicketStatusHistory
    {
        public int HistoryId { get; set; }
        public int TicketId { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public int ChangedBy { get; set; }
        public DateTime ChangedDate { get; set; }
        public string? Comments { get; set; }
        public string? ChangedByName { get; set; }
    }

    public class TicketComment
    {
        public int CommentId { get; set; }
        public int TicketId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public int CommentedBy { get; set; }
        public bool IsInternal { get; set; }
        public DateTime CommentDate { get; set; }
        public string? CommentedByName { get; set; }
    }

    public class AddCommentRequest
    {
        public int TicketId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public int CommentedBy { get; set; }
        public bool IsInternal { get; set; }
    }
}
