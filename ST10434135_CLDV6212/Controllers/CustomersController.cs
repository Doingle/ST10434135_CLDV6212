using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using ST10434135_CLDV6212.Models;
using ST10434135_CLDV6212.Services;

namespace ST10434135_CLDV6212.Controllers
{
    public class CustomersController : Controller
    {
        // inject the table storage service
        private readonly TableStorageService _tableService;

        //specific table name for this controller
        private const string TableName = "Customers";

        //-----------------------------------------------------------------------------------------
        // Constructor injection of the TableStorageService
        public CustomersController(TableStorageService tableService)
        {
            _tableService = tableService;
        }

        //-----------------------------------------------------------------------------------------
        //this is the GET : Customers Index action to list all customers
        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetEntitiesAsync<Customer>(TableName);
            return View(customers);
        }

        //-----------------------------------------------------------------------------------------
        //this is the GET : Customers Create action to show the create form
        public IActionResult Create()
        {
            return View();
        }

        //-----------------------------------------------------------------------------------------
        //this is the POST : Customers Create action to handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            //this if condition checks if the model is valid based on data annotations
            if (ModelState.IsValid)
            {
                //then it sets the PartitionKey and RowKey for the new customer, required for Azure Table Storage
                customer.PartitionKey = "CUSTOMER";
                customer.RowKey = Guid.NewGuid().ToString();

                await _tableService.AddEntityAsync(TableName, customer);
                return RedirectToAction(nameof(Index));
            }

            //if the model is not valid, it returns the same view with the customer data to show validation errors
            return View(customer);
        }

        //-----------------------------------------------------------------------------------------
        //this is the GET : Customers Details action to show details of a specific customer
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetEntityAsync<Customer>(TableName, partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        //-----------------------------------------------------------------------------------------
        //this is the GET : Customers Edit action to show the edit form for a specific customer
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetEntityAsync<Customer>(TableName, partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        //-----------------------------------------------------------------------------------------
        //this is the POST : Customers Edit action to handle form submission for editing a customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            //this if condition checks if the model is valid based on data annotations
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            //then it tries to update the customer entity in Azure Table Storage
            try
            {
                await _tableService.UpdateEntityAsync(TableName, customer);
                return RedirectToAction(nameof(Index));
            }
            //if there is an exception during the update, it catches the exception and adds a model error to display in the view
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Update failed: {ex.Message}");
                return View(customer);
            }
        }

        //-----------------------------------------------------------------------------------------
        //this is the GET : Customers Delete action to show the delete confirmation for a specific customer
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetEntityAsync<Customer>(TableName, partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        //-----------------------------------------------------------------------------------------
        //this is the POST : Customers Delete action to handle the actual deletion of a customer.
        //DeleteConfirmed is used to avoid method name conflicts with the GET Delete action
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            await _tableService.DeleteEntityAsync(TableName, partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
//--------------------------------------------------------------------------------EOF--------------------------------------------------------------------------------\\