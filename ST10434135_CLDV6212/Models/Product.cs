using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ST10434135_CLDV6212.Models
{
    public class Product : ITableEntity
    {
        // Required by Azure Table Storage
        public string PartitionKey { get; set; } = "PRODUCT";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Product-specific fields
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        public int Stock { get; set; }

        public string? ImageUrl { get; set; }

        // Blob name is used for deletion purposes
        public string? BlobName { get; set; }

    }
}