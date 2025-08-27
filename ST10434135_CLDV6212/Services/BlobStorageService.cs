using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ST10434135_CLDV6212.Models;

namespace ST10434135_CLDV6212.Services
{
    public class BlobStorageService
    {
        //initialize BlobContainerClient to interact with Azure Blob Storage
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

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method uploads a product image to Azure Blob Storage and returns the image URL and blob name
        public async Task<(string imageUrl, string blobName)> UploadProductImageAsync(string rowKey, IFormFile file)
        {
            //validate input parameters
            if (file == null || file.Length == 0)
                return (null, null);

            //generate blob name using rowKey and file extension
            var extension = Path.GetExtension(file.FileName);
            var blobName = $"products/{rowKey}{extension}";

            //get BlobClient for the specified blob name
            var blobClient = _containerClient.GetBlobClient(blobName);

            //upload file to Azure Blob Storage, overwriting if it already exists
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            //send message to queue about the new image upload
            await _queueService.SendMessageAsync(new QueueMessage
            {
                EventType = "ProductImageUploaded",
                EntityId = rowKey,
                Message = $"Image uploaded for product {rowKey} (blob: {blobName})",
                RelatedId = blobName 
            });


            return (blobClient.Uri.ToString(), blobName);
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method deletes a product image from Azure Blob Storage using the blob name, ensuring no images build up over time
        public async Task DeleteProductImageAsync(string blobName)
        {
            if (!string.IsNullOrEmpty(blobName))
            {
                await _containerClient.DeleteBlobIfExistsAsync(blobName);
            }
        }


        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method ensures that a product image can be deleted using its URL, allowing the system to clean up images upon product deletion
        public async Task DeleteProductImageByUrlAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            var uri = new Uri(imageUrl);
            //checked blob name extraction logic to ensure it correctly handles the URL structure, ensure to skip the first two segments (container name and slash)
            var blobName = string.Join("", uri.Segments.Skip(2)); 

            await _containerClient.DeleteBlobIfExistsAsync(blobName);
        }
    }
}
//----------------------------------------------------------------EOF-----------------------------------------------------------------\\
