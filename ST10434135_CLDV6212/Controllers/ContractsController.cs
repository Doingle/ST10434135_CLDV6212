using Microsoft.AspNetCore.Mvc;
using ST10434135_CLDV6212.Services;
using ST10434135_CLDV6212.Models;

namespace ST10434135_CLDV6212.Controllers
{
    public class ContractsController : Controller
    {
        private readonly TableStorageService _tableService;
        private readonly FileShareService _fileShareService;
        private const string TableName = "Contracts";

        public ContractsController(TableStorageService tableService, FileShareService fileShareService)
        {
            _tableService = tableService;
            _fileShareService = fileShareService;
        }

        // GET: Contracts
        public async Task<IActionResult> Index()
        {
            var contracts = await _tableService.GetEntitiesAsync<Contract>(TableName);
            return View(contracts);
        }

        // GET: Upload
        public IActionResult Upload() => View();

        // POST: Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile contractFile)
        {
            if (contractFile != null && contractFile.Length > 0)
            {
                var rowKey = Guid.NewGuid().ToString();
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(contractFile.FileName)}";

                var fileUrl = await _fileShareService.UploadFileAsync(contractFile, uniqueFileName);

                var contract = new Contract
                {
                    PartitionKey = "CONTRACT",
                    RowKey = rowKey,
                    FileName = contractFile.FileName,   
                    StoredFileName = uniqueFileName,    
                    FileUrl = fileUrl,
                    UploadedOn = DateTime.UtcNow
                };

                await _tableService.AddEntityAsync(TableName, contract);
                return RedirectToAction(nameof(Index));
            }

            return View();
        }



        // GET: Contracts/Download
        //downloads buffered into memory to avoid process termination without error, unscalable implementation, however, works for small contract files
        //which is what the scenario is limited to. If errors occur, this issue should be revisited.
        public async Task<IActionResult> Download(string partitionKey, string rowKey)
        {
            var contract = await _tableService.GetEntityAsync<Contract>(TableName, partitionKey, rowKey);
            if (contract == null) return NotFound();

            using var stream = await _fileShareService.DownloadFileAsync(contract.StoredFileName);

            //copy stream to byte array which is buffered in memory
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            return File(fileBytes, "application/octet-stream", contract.FileName);
        }






        // GET: Delete
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var contract = await _tableService.GetEntityAsync<Contract>(TableName, partitionKey, rowKey);
            return contract == null ? NotFound() : View(contract);
        }

        // POST: DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            var contract = await _tableService.GetEntityAsync<Contract>(TableName, partitionKey, rowKey);
            if (contract != null)
            {
                await _fileShareService.DeleteFileAsync(contract.StoredFileName);
                await _tableService.DeleteEntityAsync(TableName, partitionKey, rowKey);
            }
            return RedirectToAction(nameof(Index));
        }



    }
}
