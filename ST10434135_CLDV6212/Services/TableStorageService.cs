using Azure;
using Azure.Data.Tables;
using ST10434135_CLDV6212.Models;

namespace ST10434135_CLDV6212.Services
{

    //this class is a generalised service for managing Azure Table Storage operations for customers, products, etc.
    public class TableStorageService
    {
        //initialize TableServiceClient to interact with Azure Table Storage
        private readonly TableServiceClient _serviceClient;

        // inject QueueService to send messages on data changes
        private readonly QueueService _queueService;

        //constructor to initialize TableServiceClient with connection string from configuration
        public TableStorageService(IConfiguration configuration, QueueService queueService)
        {
            var connString = configuration.GetConnectionString("AzureStorage");
            _queueService = queueService;
            _serviceClient = new TableServiceClient(connString);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method retrieves or creates a TableClient for the specified table name
        private TableClient GetTableClient(string tableName)
        {
            var client = _serviceClient.GetTableClient(tableName);
            client.CreateIfNotExists();
            return client;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this is a generic add method that adds an entity to the specified table and sends a message to the queue
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

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this is a generic get method that retrieves all entities from the specified table
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

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this is a generic get method that retrieves a single entity by partition key and row key from the specified table
        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity, new()
        {
            var client = GetTableClient(tableName);
            var response = await client.GetEntityAsync<T>(partitionKey, rowKey);
            return response.Value;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this is a generic update method that updates an entity in the specified table and sends a message to the queue
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

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this is a generic delete method that deletes an entity by partition key and row key from the specified table and sends a message to the queue
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

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method retrieves all entities from the specified table (alternative to GetEntitiesAsync)
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
//----------------------------------------------------------------EOF-----------------------------------------------------------------\\
