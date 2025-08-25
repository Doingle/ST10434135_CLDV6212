using System.Text.Json;
using Azure.Storage.Queues;
using ST10434135_CLDV6212.Models;

namespace ST10434135_CLDV6212.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(IConfiguration config)
        {
            var conn = config.GetConnectionString("AzureStorage");
            _queueClient = new QueueClient(conn, "systemevents");
            _queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync(QueueMessage message)
        {
            try
            {
                // Ensure timestamp is current
                message.Timestamp = DateTime.UtcNow;

                var json = JsonSerializer.Serialize(message);
                await _queueClient.SendMessageAsync(json);
            }
            catch (Exception ex)
            {
                // Log and swallow — don’t crash app
                Console.WriteLine($"Queue send failed: {ex.Message}");
            }
        }
    }
}
