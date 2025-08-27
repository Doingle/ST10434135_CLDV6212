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

        //constructor to initialize the OrderService with TableStorageService and QueueService dependencies
        public OrderService(TableStorageService tableService, QueueService queueService)
        {
            _tableService = tableService;
            _queueService = queueService;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method creates a new order, reduces product stock, and returns the created order or null if stock is insufficient
        public async Task<Order?> CreateOrderAsync(string customerId, string productId, int quantity)
        {
            //get product
            var product = await _tableService.GetEntityAsync<Product>(_productsTable, "PRODUCT", productId);
            if (product == null || product.Stock < quantity)
                return null; // Not enough stock

            //reduce stock
            product.Stock -= quantity;
            await _tableService.UpdateEntityAsync(_productsTable, product);

            //create order
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

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method retrieves all orders from the Orders table
        public async Task<List<Order>> GetOrdersAsync()
        {
            return await _tableService.GetEntitiesAsync<Order>(_ordersTable);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //tjis method retrieves a specific order by its ID
        public async Task<Order?> GetOrderAsync(string orderId)
        {
            return await _tableService.GetEntityAsync<Order>(_ordersTable, "ORDER", orderId);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method updates orders (general update, not status)
        public async Task UpdateOrderAsync(Order order)
        {
            //ensure CreatedOn is set if missing
            if (order.CreatedOn == default)
                order.CreatedOn = DateTime.UtcNow;

            //always update UpdatedOn timestamp
            order.UpdatedOn = DateTime.UtcNow;

            await _tableService.UpdateEntityAsync(_ordersTable, order);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //update Status (reallocate stock if cancelled)
        public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus)
        {
            var order = await _tableService.GetEntityAsync<Order>(_ordersTable, "ORDER", orderId);
            if (order == null) return false;

            //ensure status is actually changing
            var oldStatus = order.Status; 

            if (newStatus == "Cancelled" && oldStatus != "Cancelled")
            {
                //reallocate stock if order is being cancelled
                var product = await _tableService.GetEntityAsync<Product>(_productsTable, "PRODUCT", order.ProductId);
                if (product != null)
                {
                    product.Stock += order.Quantity;
                    await _tableService.UpdateEntityAsync(_productsTable, product);
                }
            }

            order.Status = newStatus;
            order.UpdatedOn = DateTime.UtcNow;

            //save once
            await _tableService.UpdateEntityAsync(_ordersTable, order);

            //inject queue message about status change
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

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //delete order (reallocate stock if not cancelled)
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

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //get all orders with customer and product names populated
        //adjusted to handle missing customers/products gracefully and avoid multiple table queries per order
        public async Task<List<Order>> GetAllOrdersWithDetailsAsync()
        {
            var orders = await _tableService.GetAllEntitiesAsync<Order>(_ordersTable);

            var result = new List<Order>();

            //this approach fetches customer and product details for each order individually
            foreach (var order in orders)
            {
                try
                {
                    var customer = await _tableService.GetEntityAsync<Customer>(_customersTable, "CUSTOMER", order.CustomerId!);
                    order.CustomerName = customer?.Name ?? "Unknown";
                }
                catch
                {
                    order.CustomerName = "Unknown"; 
                }

                try
                {
                    var product = await _tableService.GetEntityAsync<Product>(_productsTable, "PRODUCT", order.ProductId!);
                    order.ProductName = product?.Name ?? "Unknown";
                }
                catch
                {
                    order.ProductName = "Unknown"; 
                }

                result.Add(order);
            }

            return result;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method retrieves all customers and products for populating dropdowns in the order creation form
        public async Task<(List<Customer>, List<Product>)> GetDropdownDataAsync()
        {
            var customers = await _tableService.GetAllEntitiesAsync<Customer>(_customersTable);
            var products = await _tableService.GetAllEntitiesAsync<Product>(_productsTable);
            return (customers, products);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------
        //this method retrieves all orders and populates customer and product names in a single pass to optimize performance
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = await _tableService.GetAllEntitiesAsync<Order>(_ordersTable);
            var customers = await _tableService.GetAllEntitiesAsync<Customer>(_customersTable);
            var products = await _tableService.GetAllEntitiesAsync<Product>(_productsTable);

            //populate names using in memory lookups to avoid multiple table queries
            foreach (var order in orders)
            {
                order.CustomerName = customers.FirstOrDefault(c => c.RowKey == order.CustomerId)?.Name ?? "Unknown";
                order.ProductName = products.FirstOrDefault(p => p.RowKey == order.ProductId)?.Name ?? "Unknown";
            }

            return orders;
        }


    }
}
//----------------------------------------------------------------EOF-----------------------------------------------------------------\\