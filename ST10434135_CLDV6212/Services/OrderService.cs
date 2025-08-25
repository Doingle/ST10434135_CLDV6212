using ST10434135_CLDV6212.Models;

namespace ST10434135_CLDV6212.Services
{
    public class OrderService
    {
        // inject QueueService to send messages on data changes
        private readonly QueueService _queueService;

        //all service references, with queue service above
        private readonly TableStorageService _tableService;
        private readonly string _ordersTable = "Orders";
        private readonly string _productsTable = "Products";
        private readonly string _customersTable = "Customers";

        public OrderService(TableStorageService tableService, QueueService queueService)
        {
            _tableService = tableService;
            _queueService = queueService;
        }

        // Create Order
        public async Task<Order?> CreateOrderAsync(string customerId, string productId, int quantity)
        {
            // Get product
            var product = await _tableService.GetEntityAsync<Product>(_productsTable, "PRODUCT", productId);
            if (product == null || product.Stock < quantity)
                return null; // Not enough stock

            // Reduce stock
            product.Stock -= quantity;
            await _tableService.UpdateEntityAsync(_productsTable, product);

            // Create order
            var order = new Order
            {
                PartitionKey = "ORDER",
                RowKey = Guid.NewGuid().ToString(),
                CustomerId = customerId,
                ProductId = productId,
                Quantity = quantity,
                Status = "Created",
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };

            await _tableService.AddEntityAsync(_ordersTable, order);
            return order;
        }

        // Get all orders
        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _tableService.GetEntitiesAsync<Order>(_ordersTable);
        }

        // Get single order
        public async Task<Order?> GetOrderAsync(string orderId)
        {
            return await _tableService.GetEntityAsync<Order>(_ordersTable, "ORDER", orderId);
        }

        // Update an order (general update, not just status)
        public async Task UpdateOrderAsync(Order order)
        {
            // Ensure CreatedOn is never invalid
            if (order.CreatedOn == default)
                order.CreatedOn = DateTime.UtcNow;

            // Always make UpdatedOn valid UTC
            order.UpdatedOn = DateTime.UtcNow;

            await _tableService.UpdateEntityAsync(_ordersTable, order);
        }



        // Update Status (reallocate stock if cancelled)
        public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus)
        {
            var order = await _tableService.GetEntityAsync<Order>(_ordersTable, "ORDER", orderId);
            if (order == null) return false;

            var oldStatus = order.Status; // ✅ capture before overwriting

            if (newStatus == "Cancelled" && oldStatus != "Cancelled")
            {
                // Reallocate stock
                var product = await _tableService.GetEntityAsync<Product>(_productsTable, "PRODUCT", order.ProductId);
                if (product != null)
                {
                    product.Stock += order.Quantity;
                    await _tableService.UpdateEntityAsync(_productsTable, product);
                }
            }

            order.Status = newStatus;
            order.UpdatedOn = DateTime.UtcNow;

            // ✅ Save once
            await _tableService.UpdateEntityAsync(_ordersTable, order);

            // ✅ Push queue message with correct old/new values
            await _queueService.SendMessageAsync(new QueueMessage
            {
                EventType = "OrderStatusChanged",
                EntityId = order.RowKey!,
                Message = $"Order status changed from {oldStatus} to {newStatus}",
                RelatedId = order.PartitionKey,
                Timestamp = DateTime.UtcNow
            });

            return true;
        }


        // Delete order (optionally reallocate stock)
        public async Task DeleteOrderAsync(string orderId)
        {
            var order = await _tableService.GetEntityAsync<Order>(_ordersTable, "ORDER", orderId);
            if (order != null)
            {
                // If deleting a non-cancelled order, reallocate stock
                if (order.Status != "Cancelled")
                {
                    var product = await _tableService.GetEntityAsync<Product>(_productsTable, "PRODUCT", order.ProductId);
                    if (product != null)
                    {
                        product.Stock += order.Quantity;
                        await _tableService.UpdateEntityAsync(_productsTable, product);
                    }
                }

                await _tableService.DeleteEntityAsync(_ordersTable, order.PartitionKey!, order.RowKey!);
            }
        }

        public async Task<List<Order>> GetAllOrdersWithDetailsAsync()
        {
            var orders = await _tableService.GetAllEntitiesAsync<Order>(_ordersTable);

            var result = new List<Order>();

            foreach (var order in orders)
            {
                try
                {
                    var customer = await _tableService.GetEntityAsync<Customer>(_customersTable, "CUSTOMER", order.CustomerId!);
                    order.CustomerName = customer?.Name ?? "Unknown";
                }
                catch
                {
                    order.CustomerName = "Unknown"; // missing customer
                }

                try
                {
                    var product = await _tableService.GetEntityAsync<Product>(_productsTable, "PRODUCT", order.ProductId!);
                    order.ProductName = product?.Name ?? "Unknown";
                }
                catch
                {
                    order.ProductName = "Unknown"; // missing product
                }

                result.Add(order);
            }

            return result;
        }



        // ✅ Helper for Create dropdowns
        public async Task<(List<Customer>, List<Product>)> GetDropdownDataAsync()
        {
            var customers = await _tableService.GetAllEntitiesAsync<Customer>(_customersTable);
            var products = await _tableService.GetAllEntitiesAsync<Product>(_productsTable);
            return (customers, products);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = await _tableService.GetAllEntitiesAsync<Order>(_ordersTable);
            var customers = await _tableService.GetAllEntitiesAsync<Customer>(_customersTable);
            var products = await _tableService.GetAllEntitiesAsync<Product>(_productsTable);

            foreach (var order in orders)
            {
                order.CustomerName = customers.FirstOrDefault(c => c.RowKey == order.CustomerId)?.Name ?? "Unknown";
                order.ProductName = products.FirstOrDefault(p => p.RowKey == order.ProductId)?.Name ?? "Unknown";
            }

            return orders;
        }


    }
}
