using Azure;
using Azure.Data.Tables;

namespace ST10434135_CLDV6212.Models
{
    public class Contract : ITableEntity
    {
        //set partition key and row key as required by ITableEntity
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }

        //custom properties such as: A file's original name as on user PC, a stored name, a URL to access the file, and an upload timestamp
        public string? FileName { get; set; }       
        public string? StoredFileName { get; set; }
        public string? FileUrl { get; set; }        
        public DateTime UploadedOn { get; set; }

        //properties required by ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

}
//----------------------------------------------------------------EOF-----------------------------------------------------------------\\
