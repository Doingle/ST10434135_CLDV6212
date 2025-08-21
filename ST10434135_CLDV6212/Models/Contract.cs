using Azure;
using Azure.Data.Tables;

namespace ST10434135_CLDV6212.Models
{
    public class Contract : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }

        public string? FileName { get; set; }       // The actual filename
        public string? StoredFileName { get; set; }
        public string? FileUrl { get; set; }        // Direct access URL
        public DateTime UploadedOn { get; set; }   // Audit tracking

        // Required for ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

}
