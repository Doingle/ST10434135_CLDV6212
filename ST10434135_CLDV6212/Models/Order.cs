using Azure;
using Azure.Data.Tables;

namespace ST10434135_CLDV6212.Models
{
    public class Order : ITableEntity
    {
        //set partition key and row key as required by ITableEntity
        public string? PartitionKey { get; set; }   
        public string? RowKey { get; set; }

        //unique order identifiers
        public string? CustomerId { get; set; }     
        public string? ProductId { get; set; }
        //keep track of quantity ordered, for stock management on order status change
        public int Quantity { get; set; }
        //order status to track progress, default to "Created"
        public string Status { get; set; } = "Created";

        //timestamps for order creation and last update
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        // Required for ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        //non persisted properties for display purposes only
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
    }
}
//----------------------------------------------------------------EOF-----------------------------------------------------------------\\
