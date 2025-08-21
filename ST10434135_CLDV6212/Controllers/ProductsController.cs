using Microsoft.AspNetCore.Mvc;
using ST10434135_CLDV6212.Models;
using ST10434135_CLDV6212.Services;

namespace ST10434135_CLDV6212.Controllers
{
    public class ProductsController : Controller
    {
        private readonly TableStorageService _tableService;
        private readonly BlobStorageService _blobService;

        private const string TableName = "Products";

        public ProductsController(TableStorageService tableService, BlobStorageService blobService)
        {
            _tableService = tableService;
            _blobService = blobService;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _tableService.GetEntitiesAsync<Product>(TableName);
            return View(products);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            // Always set keys first
            product.PartitionKey = "PRODUCT";
            product.RowKey = Guid.NewGuid().ToString();

            // Handle image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var (imageUrl, blobName) = await _blobService.UploadProductImageAsync(product.RowKey, imageFile);
                product.ImageUrl = imageUrl;
                product.BlobName = blobName;
            }
            else
            {
                ModelState.AddModelError("BlobName", "An image is required.");
            }

            if (ModelState.IsValid)
            {
                await _tableService.AddEntityAsync("Products", product);
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }



        // GET: Edit
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await _tableService.GetEntityAsync<Product>(TableName, partitionKey, rowKey);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var (imageUrl, blobName) = await _blobService.UploadProductImageAsync(product.RowKey, imageFile);
                    product.ImageUrl = imageUrl;
                    product.BlobName = blobName;
                }


                await _tableService.UpdateEntityAsync(TableName, product);
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var product = await _tableService.GetEntityAsync<Product>(TableName, partitionKey, rowKey);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            var product = await _tableService.GetEntityAsync<Product>(TableName, partitionKey, rowKey);

            if (product != null && !string.IsNullOrEmpty(product.BlobName))
            {
                await _blobService.DeleteProductImageAsync(product.BlobName);
            }

            await _tableService.DeleteEntityAsync(TableName, partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
