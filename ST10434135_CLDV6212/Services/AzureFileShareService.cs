using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace ST10434135_CLDV6212.Services
{
    //this class manages file operations in Azure File Share
    public class FileShareService
    {
        //initialize ShareClient to interact with Azure File Share
        private readonly ShareClient _shareClient;

        //constructor to initialize ShareClient with connection string and share name
        public FileShareService(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("AzureStorage");
            _shareClient = new ShareClient(connectionString, "contracts"); 
            _shareClient.CreateIfNotExists();
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //upload file to Azure File Share and return the file URL
        public async Task<string> UploadFileAsync(IFormFile file, string uniqueFileName)
        {
            //validate input parameters
            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(uniqueFileName);

            //upload file to Azure File Share
            using (var stream = file.OpenReadStream())
            {
                //create file in Azure File Share with the same size as the uploaded file
                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
            }

            //return the URL of the uploaded file
            return fileClient.Uri.ToString();
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //Download file as a stream, updated as upon download the process MVC app was terminated without any error
        //this method facilitates downloading a file from Azure File Share and returns it as a Stream
        public async Task<Stream> DownloadFileAsync(string uniqueFileName)
        {
            //this if statement checks if the provided file name is null or empty and throws an exception if it is
            if (string.IsNullOrEmpty(uniqueFileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(uniqueFileName));

            //get the root directory client and the specific file client for the requested file
            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(uniqueFileName);

            //download the file from Azure File Share
            var download = await fileClient.DownloadAsync();

            //copy the downloaded content to a MemoryStream and return it
            var memoryStream = new MemoryStream();
            await download.Value.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0; 
            return memoryStream;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method deletes a file from Azure File Share based on the provided unique file name
        public async Task DeleteFileAsync(string uniqueFileName)
        {
            //this if statement checks if the provided file name is null or empty and throws an exception if it is
            if (string.IsNullOrEmpty(uniqueFileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(uniqueFileName));

            //get the root directory client and the specific file client for the requested file
            var directory = _shareClient.GetRootDirectoryClient();
            var fileClient = directory.GetFileClient(uniqueFileName);

            //delete the file from Azure File Share if it exists
            await fileClient.DeleteIfExistsAsync();
        }
    }
    }
//----------------------------------------------------------------EOF-----------------------------------------------------------------\\
