using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST10434135_CLDV6212.Models;
using ST10434135_CLDV6212.Services;

namespace ST10434135_CLDV6212.Controllers
{
    public class OrdersController : Controller
    {
        //injected services, in the order they are defined in Program.cs
        private readonly OrderService _orderService;
        private readonly TableStorageService _tableService;
        private readonly string _customersTable = "Customers";
        private readonly string _productsTable = "Products";
        private readonly string _ordersTable = "Orders";

        //constructor to inject services
        public OrdersController(OrderService orderService, TableStorageService tableService)
        {
            _orderService = orderService;
            _tableService = tableService;
        }

        //--------------------------------------------------------------------------------------------------------
        //this is the GET: Orders Index action method to list all orders with details
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersWithDetailsAsync();
            return View(orders);
        }

        //--------------------------------------------------------------------------------------------------------
        //this is the GET: Orders/Create action method to show the create order form
        public async Task<IActionResult> Create()
        {
            // Populate dropdowns for Customers as well as Products
            var (customers, products) = await _orderService.GetDropdownDataAsync();

            // Using ViewData to pass SelectListItems to the view
            ViewData["Customers"] = customers.Select(c => new SelectListItem
            {
                Value = c.RowKey,
                Text = c.Name
            }).ToList();

            // Products dropdown
            ViewData["Products"] = products.Select(p => new SelectListItem
            {
                Value = p.RowKey,
                Text = p.Name
            }).ToList();

            return View();
        }

        //--------------------------------------------------------------------------------------------------------
        //this is the POST: Orders/Create action method to handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string customerId, string productId, int quantity)
        {
            // Validate inputs
            var order = await _orderService.CreateOrderAsync(customerId, productId, quantity);

            //this if block checks if the order creation was successful and validates stock availability
            if (order == null)
            {
                ModelState.AddModelError("", "Not enough stock or invalid product.");
                ViewBag.Customers = await _tableService.GetEntitiesAsync<Customer>(_customersTable);
                ViewBag.Products = await _tableService.GetEntitiesAsync<Product>(_productsTable);
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        //--------------------------------------------------------------------------------------------------------
        //this is the GET: Order Details action method to show order details
        public async Task<IActionResult> Details(string id)
        {
            var order = await _tableService.GetEntityAsync<Order>(_ordersTable, "ORDER", id);
            if (order == null) return NotFound();
            return View(order);
        }

        //--------------------------------------------------------------------------------------------------------
        //this is the GET: Orders Edit action method to show the edit form
        public async Task<IActionResult> Edit(string id)
        {
            //if id is null, return 404
            if (id == null) return NotFound();

            //get the order by id
            var order = await _orderService.GetOrderAsync(id);
            if (order == null) return NotFound();

            //this populates the dropdowns for Customers and Products in the edit view
            ViewBag.Customers = await _tableService.GetEntitiesAsync<Customer>("customers");
            ViewBag.Products = await _tableService.GetEntitiesAsync<Product>("products");

            //return the order to the view for editing
            return View(order);
        }

        //--------------------------------------------------------------------------------------------------------
        //this is the POST: Orders/Edit action method to handle the edit form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Order updatedOrder)
        {
            if (ModelState.IsValid)
            {
                //get the existing order to preserve CreatedOn and ensure correct keys
                var existingOrder = await _orderService.GetOrderAsync(id);
                if (existingOrder == null)
                {
                    return NotFound();
                }

                //ensure CreatedOn is never invalid
                updatedOrder.CreatedOn = existingOrder.CreatedOn;

                //always make UpdatedOn valid UTC
                updatedOrder.UpdatedOn = DateTime.UtcNow;

                //ensure PartitionKey and RowKey are correct
                updatedOrder.PartitionKey = existingOrder.PartitionKey;
                updatedOrder.RowKey = existingOrder.RowKey;

                await _orderService.UpdateOrderAsync(updatedOrder);
                return RedirectToAction(nameof(Index));
            }
            return View(updatedOrder);
        }

        //--------------------------------------------------------------------------------------------------------
        //this is the POST: Orders/UpdateStatus action method to update order status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            await _orderService.UpdateOrderStatusAsync(id, status);
            return RedirectToAction(nameof(Index));
        }

        //--------------------------------------------------------------------------------------------------------
        //this is the GET: Orders/Delete action method to show the delete confirmation page
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _tableService.DeleteEntityAsync(_ordersTable, "ORDER", id);
            return RedirectToAction(nameof(Index));
        }
    }
}
//-------------------------------------------------------------------------EOF-------------------------------------------------------------\\
