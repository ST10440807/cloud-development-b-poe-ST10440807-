using ABCRetailers.Models;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    public class CustomerController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        public CustomerController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _tableStorageService.GetCustomersAsync();
            return View(customers);
        }

        public async Task<IActionResult> Delete(string partitionKey,string rowKey)
        {
            await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        }
        //Post method
        [HttpPost]
        public async Task<IActionResult> AddCustomer(Customer customer)
        {
            customer.PartitionKey = "CustomersPartition";
            customer.RowKey = Guid.NewGuid().ToString(); //Auto generate rowkey

            await _tableStorageService.AddCustomerAsync(customer);
            return RedirectToAction("Index");
        }
        //Get method
        [HttpGet]
        public IActionResult AddCustomer()
        {
            return View();
        }
    }
}
