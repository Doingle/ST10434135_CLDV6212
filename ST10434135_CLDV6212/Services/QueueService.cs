using System.Text.Json;
using Azure.Storage.Queues;
using ST10434135_CLDV6212.Models;

namespace ST10434135_CLDV6212.Services
{
    //this class manages sending messages to an Azure Storage Queue
    public class QueueService
    {
        //initialize QueueClient to interact with Azure Storage Queue
        private readonly QueueClient _queueClient;

        //constructor to initialize QueueClient with connection string and queue name
        public QueueService(IConfiguration config)
        {
            var conn = config.GetConnectionString("AzureStorage");
            _queueClient = new QueueClient(conn, "systemevents");
            _queueClient.CreateIfNotExists();
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method sends a message to the Azure Storage Queue
        public async Task SendMessageAsync(QueueMessage message)
        {
            //this try catch block logs any exceptions that occur during the message sending process
            try
            {
                //ensure message has a timestamp which is set to current UTC time
                message.Timestamp = DateTime.UtcNow;

                var json = JsonSerializer.Serialize(message);
                await _queueClient.SendMessageAsync(json);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Queue send failed: {ex.Message}");
            }
        }
    }
}
//----------------------------------------------------------------EOF-----------------------------------------------------------------\\
