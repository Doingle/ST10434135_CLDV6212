using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using ST10434135_CLDV6212.Models;
using ST10434135_CLDV6212.Services;

namespace ST10434135_CLDV6212.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TableStorageService _tableService;

        // specify which table this controller uses
        private const string TableName = "Customers";

        public CustomersController(TableStorageService tableService)
        {
            _tableService = tableService;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetEntitiesAsync<Customer>(TableName);
            return View(customers);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Assign system-managed keys
                customer.PartitionKey = "CUSTOMER";
                customer.RowKey = Guid.NewGuid().ToString();

                await _tableService.AddEntityAsync(TableName, customer);
                return RedirectToAction(nameof(Index));
            }

            // If invalid, show validation errors
            return View(customer);
        }

        // GET: Customers/Details
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetEntityAsync<Customer>(TableName, partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // GET: Customers/Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetEntityAsync<Customer>(TableName, partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            try
            {
                await _tableService.UpdateEntityAsync(TableName, customer);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Update failed: {ex.Message}");
                return View(customer);
            }
        }

        // GET: Customers/Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetEntityAsync<Customer>(TableName, partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            await _tableService.DeleteEntityAsync(TableName, partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
