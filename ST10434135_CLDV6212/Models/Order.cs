using Azure;
using Azure.Data.Tables;

namespace ST10434135_CLDV6212.Models
{
    public class Order : ITableEntity
    {
        public string? PartitionKey { get; set; }   // e.g. "ORDER"
        public string? RowKey { get; set; }         // Order ID (Guid)

        public string? CustomerId { get; set; }     // FK to Customers table (RowKey)
        public string? ProductId { get; set; }      // FK to Products table (RowKey)
        public int Quantity { get; set; }           // How many products ordered

        public string Status { get; set; } = "Created"; // Created, Processing, Completed, Cancelled

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        // Required for ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
    }
}
