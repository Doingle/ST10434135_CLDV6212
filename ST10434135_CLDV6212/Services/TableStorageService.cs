using Azure;
using Azure.Data.Tables;
using ST10434135_CLDV6212.Models;

namespace ST10434135_CLDV6212.Services
{

    //Generalized the service to allow for multiple tables within an individual storage account
    public class TableStorageService
    {

        private readonly TableServiceClient _serviceClient;

        // inject QueueService to send messages on data changes
        private readonly QueueService _queueService;

        public TableStorageService(IConfiguration configuration, QueueService queueService)
        {
            var connString = configuration.GetConnectionString("AzureStorage");
            _queueService = queueService;
            _serviceClient = new TableServiceClient(connString);
        }

        // Get a client for any table
        private TableClient GetTableClient(string tableName)
        {
            var client = _serviceClient.GetTableClient(tableName);
            client.CreateIfNotExists();
            return client;
        }

        // Generic Add
        public async Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            var client = GetTableClient(tableName);
            await client.AddEntityAsync(entity);

            //injection of queue service. add a message into the queue after adding an entity
            await _queueService.SendMessageAsync(new QueueMessage
            {
                EventType = "EntityCreated",
                EntityId = entity.RowKey,
                Message = $"{typeof(T).Name} created"
            });

        }

        // Generic Get All
        public async Task<List<T>> GetEntitiesAsync<T>(string tableName) where T : class, ITableEntity, new()
        {
            var client = GetTableClient(tableName);
            var results = new List<T>();
            await foreach (var item in client.QueryAsync<T>())
            {
                results.Add(item);
            }
            return results;
        }

        // Generic Get Single
        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var client = GetTableClient(tableName);
            var response = await client.GetEntityAsync<T>(partitionKey, rowKey);
            return response.Value;
        }

        // Generic Update
        public async Task UpdateEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            if (string.IsNullOrEmpty(entity.PartitionKey) || string.IsNullOrEmpty(entity.RowKey))
                throw new ArgumentException("PartitionKey and RowKey must not be null or empty.");

            var client = GetTableClient(tableName);
            await client.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);

            //injection of queue service. add a message into the queue after updating an entity
            await _queueService.SendMessageAsync(new QueueMessage
            {
                EventType = "EntityUpdated",
                EntityId = entity.RowKey,
                Message = $"{typeof(T).Name} updated"
            });

        }

        // Generic Delete
        public async Task DeleteEntityAsync(string tableName, string partitionKey, string rowKey)
        {
            var client = GetTableClient(tableName);
            await client.DeleteEntityAsync(partitionKey, rowKey);

            await _queueService.SendMessageAsync(new QueueMessage
            {
                EventType = "EntityDeleted",
                EntityId = rowKey,
                Message = $"Entity deleted"
            });

        }

        public async Task<List<T>> GetAllEntitiesAsync<T>(string tableName) where T : class, ITableEntity, new()
        {
            var tableClient = GetTableClient(tableName);
            var result = new List<T>();

            await foreach (var entity in tableClient.QueryAsync<T>())
            {
                result.Add(entity);
            }

            return result;
        }

    }
}
