using ABCRetailers.Models;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    public class ProductController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableStorageService _tableStorageService;

        public ProductController(BlobService blobService, TableStorageService tableStorageService)
        {
            _blobService = blobService;
            _tableStorageService = tableStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetAllProductsAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file) //Upload a file when we add a product 
        {

            if (ModelState.IsValid)
            {

                if (file != null && file.Length >= 0)
                {

                    using var stream = file.OpenReadStream();
                    var imageUrl = await _blobService.UploadsAsync(stream, file.FileName);
                    product.ImageUrl = imageUrl;


                    product.PartitionKey = "ProductPartition";
                    product.RowKey = Guid.NewGuid().ToString();
                    await _tableStorageService.AddProductAsync(product);
                    return RedirectToAction("Index");
                }


            }
            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string partitionKey,string rowKey,Product product)
        {
            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                //Delete associated blob image
                await _blobService.DeleteBlobAsync(product.ImageUrl);
            }
            //Delete table entity
            await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);

            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }
    }
}
