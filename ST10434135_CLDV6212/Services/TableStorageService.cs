using Azure;
using Azure.Data.Tables;
using ST10434135_CLDV6212.Models;

namespace ST10434135_CLDV6212.Services
{
    public class TableStorageService
    {

        private readonly TableClient _tableClient;

        public TableStorageService(IConfiguration configuration)
        {
            var connString = configuration.GetConnectionString("AzureStorage");
            var serviceClient = new TableServiceClient(connString);
            _tableClient = serviceClient.GetTableClient("Customers");
            _tableClient.CreateIfNotExists(); // ensure table exists
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await _tableClient.AddEntityAsync(customer);
        }


        public async Task<List<Customer>> GetCustomersAsync()
        {
            var results = new List<Customer>();
            await foreach (var customer in _tableClient.QueryAsync<Customer>())
            {
                results.Add(customer);
            }
            return results;
        }

        public async Task<Customer> GetCustomerAsync(string partitionKey, string rowKey)
        {
            var response = await _tableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
            return response.Value;
        }




        public async Task UpdateCustomerAsync(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must not be null or empty.");
            }

            await _tableClient.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace);
        }





        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

    }

}

