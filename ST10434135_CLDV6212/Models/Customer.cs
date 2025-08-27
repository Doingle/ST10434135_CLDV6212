using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ST10434135_CLDV6212.Models
{
    public class Customer : ITableEntity
    {
        //system managed keys for Azure Table Storage
        //bind never to prevent model binding issues
        //made nullable to allow for model binding and setting default values

        //set partition key and row key as required by ITableEntity
        public string? PartitionKey { get; set; } = "Customers";

        public string? RowKey { get; set; } = Guid.NewGuid().ToString();

        //custom properties such as: Customer Name, Email, Phone, all required fields
        [Required]
        public string? Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string? Email { get; set; } = string.Empty;

        [Required, Phone]
        public string? Phone { get; set; } = string.Empty;

        //properties required by ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

}
//----------------------------------------------------------------EOF-----------------------------------------------------------------\\
