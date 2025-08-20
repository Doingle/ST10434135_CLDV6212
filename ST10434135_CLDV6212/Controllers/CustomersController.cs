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


        public CustomersController(TableStorageService tableService)
        {
            _tableService = tableService;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetCustomersAsync();
            return View(customers);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        //create action
        //---------------------------------------------------------------------------

        //create post method for creating a customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Assign system-managed keys
                customer.PartitionKey = "CUSTOMER";
                customer.RowKey = Guid.NewGuid().ToString();

                await _tableService.AddCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }

            // If invalid, show validation errors
            return View(customer);
        }





        //details, edit, and delete methods for customers
        //---------------------------------------------------------------------------

        // GET: Details
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
            return View(customer);
        }

        // GET: Customers/Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
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
                await _tableService.UpdateCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Update failed: {ex.Message}");
                return View(customer);
            }
        }




        // GET: Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
            return View(customer);
        }

        // POST: DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            await _tableService.DeleteCustomerAsync(partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}

