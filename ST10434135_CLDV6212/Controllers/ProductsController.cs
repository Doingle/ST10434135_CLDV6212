using Microsoft.AspNetCore.Mvc;
using ST10434135_CLDV6212.Models;
using ST10434135_CLDV6212.Services;

namespace ST10434135_CLDV6212.Controllers
{
    public class ProductsController : Controller
    {
        //inject TableStorageService and BlobStorageService
        private readonly TableStorageService _tableService;
        private readonly BlobStorageService _blobService;

        //specify table name
        private const string TableName = "Products";

        //constructor injection
        public ProductsController(TableStorageService tableService, BlobStorageService blobService)
        {
            _tableService = tableService;
            _blobService = blobService;
        }

        //--------------------------------------------------------------------------------------------------------------------------
        //this is the GET : Index method to list all products
        public async Task<IActionResult> Index()
        {
            var products = await _tableService.GetEntitiesAsync<Product>(TableName);
            return View(products);
        }

        //--------------------------------------------------------------------------------------------------------------------------
        //this is the GET : Create method to show the create form
        public IActionResult Create()
        {
            return View();
        }

        //--------------------------------------------------------------------------------------------------------------------------
        //this is the POST : Create method to handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            //make sure PartitionKey and RowKey are set for new product
            product.PartitionKey = "PRODUCT";
            product.RowKey = Guid.NewGuid().ToString();

            //this if block handles image upload and validation for create action
            if (imageFile != null && imageFile.Length > 0)
            {
                var (imageUrl, blobName) = await _blobService.UploadProductImageAsync(product.RowKey, imageFile);
                product.ImageUrl = imageUrl;
                product.BlobName = blobName;
            }
            //if no image is provided, add a model error
            else
            {
                ModelState.AddModelError("BlobName", "An image is required.");
            }

            //validate the model state and add the product to the table if valid
            if (ModelState.IsValid)
            {
                await _tableService.AddEntityAsync("Products", product);
                return RedirectToAction(nameof(Index));
            }

            //if we got this far in the create action, something failed, redisplay form and preserve user input XD
            return View(product);
        }

        //--------------------------------------------------------------------------------------------------------------------------
        //this is the GET : Edit method to show the edit form
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            //retrieve the product to edit based on partitionKey and rowKey
            var product = await _tableService.GetEntityAsync<Product>(TableName, partitionKey, rowKey);
            //if product is null, return 404 not found
            if (product == null) return NotFound();

            //otherwise, return the edit view with the product data
            return View(product);
        }

        //--------------------------------------------------------------------------------------------------------------------------
        //this is the POST : Edit method to handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile imageFile)
        {
            //this if block handles image upload and validation for edit action
            if (ModelState.IsValid)
            {
                //if a new image file is provided, upload it and update the product's ImageUrl and BlobName. for now, a new image must be uploaded when editing
                //set strict requirement for image upload during edit to ensure product always has an image
                if (imageFile != null && imageFile.Length > 0)
                {
                    var (imageUrl, blobName) = await _blobService.UploadProductImageAsync(product.RowKey, imageFile);
                    product.ImageUrl = imageUrl;
                    product.BlobName = blobName;
                }

                //update the product entity in the table storage
                await _tableService.UpdateEntityAsync(TableName, product);
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        //--------------------------------------------------------------------------------------------------------------------------
        //this is the GET : Delete method to show the delete confirmation page
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            //retrieve the product to delete based on partitionKey and rowKey
            var product = await _tableService.GetEntityAsync<Product>(TableName, partitionKey, rowKey);
            if (product == null) return NotFound();

            //return the delete view with the product data
            return View(product);
        }

        //--------------------------------------------------------------------------------------------------------------------------
        //this is the POST : DeleteConfirmed method to handle the actual deletion
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            //retrieve the product to delete to get the BlobName for image deletion
            var product = await _tableService.GetEntityAsync<Product>(TableName, partitionKey, rowKey);

            //this if block handles deletion of the associated image from blob storage if it exists. it deletes the blob using the stored BlobName
            if (product != null && !string.IsNullOrEmpty(product.BlobName))
            {
                await _blobService.DeleteProductImageAsync(product.BlobName);
            }

            //delete the product entity from the table storage
            await _tableService.DeleteEntityAsync(TableName, partitionKey, rowKey);

            return RedirectToAction(nameof(Index));
        }
    }
}
//--------------------------------------------------------------------------------------EOF--------------------------------------------------------------------------------\\
