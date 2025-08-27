namespace ST10434135_CLDV6212.Models
{
    // Model for messages to be sent to the queue
    public class QueueMessage
    {
        //event type logging what kind of event occurred (e.g., "OrderCreated", "ProductUpdated")
        public string EventType { get; set; } = string.Empty;
        //entity id to identify the specific entity involved in the event (e.g., OrderId, ProductId, CustomerId)
        public string EntityId { get; set; } = string.Empty;
        //nullable extra related id to provide additional context if needed (e.g., CustomerId for an order event)
        public string? RelatedId { get; set; }
        //message to provide a human readable description of the event
        public string Message { get; set; } = string.Empty;
        //timestamp to log when the event occurred, defaulting to the current UTC time
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
//----------------------------------------------------------------EOF-----------------------------------------------------------------\\
