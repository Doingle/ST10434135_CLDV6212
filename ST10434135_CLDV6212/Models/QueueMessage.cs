namespace ST10434135_CLDV6212.Models
{
    public class QueueMessage
    {
        public string EventType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? RelatedId { get; set; } // optional (e.g., ProductId in an Order)
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
