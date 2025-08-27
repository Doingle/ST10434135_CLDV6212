using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ST10434135_CLDV6212.Models
{
    public class Product : ITableEntity
    {
        //required properties for ITableEntity
        public string PartitionKey { get; set; } = "PRODUCT";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        //setting needed fields for a product listing, set required fields and validation
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        //price set to double to allow for decimal places in a way which is supported by azure table storage, range set to prevent negative prices
        [Required]
        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        //keep track of stock, range set to prevent negative stock values, and change with orders
        public int Stock { get; set; }

        //image URL to facilitate display of product images in the product listing, nullable as not all products may have images
        public string? ImageUrl { get; set; }

        //blob name to facilitate deletion of images from blob storage when products are deleted
        public string? BlobName { get; set; }

    }
}
//----------------------------------------------------------------EOF-----------------------------------------------------------------\\