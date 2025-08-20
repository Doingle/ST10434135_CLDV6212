using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ST10434135_CLDV6212.Models
{
    public class Customer : ITableEntity
    {
        // System-managed keys for Azure Table Storage
        // Bind never to prevent model binding issues
        // Made nullable to allow for model binding
        
        public string? PartitionKey { get; set; } = "Customers";

        public string? RowKey { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string? Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string? Email { get; set; } = string.Empty;

        [Required, Phone]
        public string? Phone { get; set; } = string.Empty;

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

}
