using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ST10434135_CLDV6212.Models;

namespace ST10434135_CLDV6212.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        // inject QueueService to send messages on data changes
        private readonly QueueService _queueService;

        public BlobStorageService(IConfiguration configuration, QueueService queueService)
        {
            _queueService = queueService;

            var connectionString = configuration.GetConnectionString("AzureStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);

            // use container name: productimages
            _containerClient = blobServiceClient.GetBlobContainerClient("productimages");
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        // Upload product image and return URL
        public async Task<(string imageUrl, string blobName)> UploadProductImageAsync(string rowKey, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (null, null);

            var extension = Path.GetExtension(file.FileName);
            var blobName = $"products/{rowKey}{extension}";

            var blobClient = _containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            await _queueService.SendMessageAsync(new QueueMessage
            {
                EventType = "ProductImageUploaded",
                EntityId = rowKey,
                Message = $"Image uploaded for product {rowKey} (blob: {blobName})",
                RelatedId = blobName 
            });


            return (blobClient.Uri.ToString(), blobName);
        }

        // Delete product image by blob name (e.g. "products/rowKey.jpg")
        public async Task DeleteProductImageAsync(string blobName)
        {
            if (!string.IsNullOrEmpty(blobName))
            {
                await _containerClient.DeleteBlobIfExistsAsync(blobName);
            }
        }



        // ✅ NEW: Delete product image by full URL
        public async Task DeleteProductImageByUrlAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            var uri = new Uri(imageUrl);
            // Blob name is everything after the container name
            var blobName = string.Join("", uri.Segments.Skip(2)); // skip "/productimages/"

            await _containerClient.DeleteBlobIfExistsAsync(blobName);
        }
    }
}
