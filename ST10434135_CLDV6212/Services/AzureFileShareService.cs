using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace ST10434135_CLDV6212.Services
{
    public class FileShareService
    {
        private readonly ShareClient _shareClient;

        public FileShareService(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("AzureStorage");
            _shareClient = new ShareClient(connectionString, "contracts"); // your Azure File Share name
            _shareClient.CreateIfNotExists();
        }

        public async Task<string> UploadFileAsync(IFormFile file, string uniqueFileName)
        {
            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(uniqueFileName);

            using (var stream = file.OpenReadStream())
            {
                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
            }

            return fileClient.Uri.ToString();
        }

        //Download file as a stream, updated as upon download the process (application) was terminated without any error
        public async Task<Stream> DownloadFileAsync(string uniqueFileName)
        {
            if (string.IsNullOrEmpty(uniqueFileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(uniqueFileName));

            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(uniqueFileName);

            var download = await fileClient.DownloadAsync();

            // Copy Azure stream into memory
            var memoryStream = new MemoryStream();
            await download.Value.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset pointer for ASP.NET to read
            return memoryStream;
        }


        public async Task DeleteFileAsync(string uniqueFileName)
        {
            if (string.IsNullOrEmpty(uniqueFileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(uniqueFileName));

            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(uniqueFileName);

            await fileClient.DeleteIfExistsAsync();
        }
    }
    }
