using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST10434135_CLDV6212.Models;
using ST10434135_CLDV6212.Services;

namespace ST10434135_CLDV6212.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderService _orderService;
        private readonly TableStorageService _tableService;
        private readonly string _customersTable = "Customers";
        private readonly string _productsTable = "Products";
        private readonly string _ordersTable = "Orders";

        public OrdersController(OrderService orderService, TableStorageService tableService)
        {
            _orderService = orderService;
            _tableService = tableService;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersWithDetailsAsync();
            return View(orders);
        }


        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            var (customers, products) = await _orderService.GetDropdownDataAsync();

            ViewData["Customers"] = customers.Select(c => new SelectListItem
            {
                Value = c.RowKey,
                Text = c.Name
            }).ToList();

            ViewData["Products"] = products.Select(p => new SelectListItem
            {
                Value = p.RowKey,
                Text = p.Name
            }).ToList();

            return View();
        }



        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string customerId, string productId, int quantity)
        {
            var order = await _orderService.CreateOrderAsync(customerId, productId, quantity);

            if (order == null)
            {
                ModelState.AddModelError("", "Not enough stock or invalid product.");
                ViewBag.Customers = await _tableService.GetEntitiesAsync<Customer>(_customersTable);
                ViewBag.Products = await _tableService.GetEntitiesAsync<Product>(_productsTable);
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var order = await _tableService.GetEntityAsync<Order>(_ordersTable, "ORDER", id);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET: Orders/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var order = await _orderService.GetOrderAsync(id);
            if (order == null) return NotFound();

            // Populate dropdowns again for Customers + Products
            ViewBag.Customers = await _tableService.GetEntitiesAsync<Customer>("customers");
            ViewBag.Products = await _tableService.GetEntitiesAsync<Product>("products");

            return View(order);
        }

        // POST: Orders/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Order updatedOrder)
        {
            if (ModelState.IsValid)
            {
                // Get the existing order from storage
                var existingOrder = await _orderService.GetOrderAsync(id);
                if (existingOrder == null)
                {
                    return NotFound();
                }

                // Preserve CreatedOn
                updatedOrder.CreatedOn = existingOrder.CreatedOn;

                // Ensure UpdatedOn is always valid UTC
                updatedOrder.UpdatedOn = DateTime.UtcNow;

                // Keep PartitionKey and RowKey
                updatedOrder.PartitionKey = existingOrder.PartitionKey;
                updatedOrder.RowKey = existingOrder.RowKey;

                await _orderService.UpdateOrderAsync(updatedOrder);
                return RedirectToAction(nameof(Index));
            }
            return View(updatedOrder);
        }



        // POST: Orders/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            await _orderService.UpdateOrderStatusAsync(id, status);
            return RedirectToAction(nameof(Index));
        }

        // POST: Orders/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _tableService.DeleteEntityAsync(_ordersTable, "ORDER", id);
            return RedirectToAction(nameof(Index));
        }
    }
}
