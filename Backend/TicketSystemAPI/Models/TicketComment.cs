namespace TicketSystemAPI.Models
{
    public class TicketComment
    {
        public int CommentId { get; set; }
        public int TicketId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public int CommentedBy { get; set; }
        public bool IsInternal { get; set; }
        public DateTime CommentDate { get; set; }
        
        // Additional property for display
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
