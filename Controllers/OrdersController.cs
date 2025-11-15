using ABCRetailers.Models;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    public class OrdersController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;

        public OrdersController(TableStorageService tableStorageService, QueueService queueService)
        {
            _tableStorageService = tableStorageService;
            _queueService = queueService;
        }


        public async Task<IActionResult> Index()
        {
            var orders = await _tableStorageService.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> PlaceOrder()
        {
            var customers = await _tableStorageService.GetCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();
            //Check for null or empty lists 
            if (customers == null || customers.Count == 0)
            {
                //No customers found
                ModelState.AddModelError("", "No Customers found. Please add customers first");
                return View();
            }
            if(products == null || products.Count == 0)
            {
                //No products found
                ModelState.AddModelError("", "No products found ! Please add products first");
                return View();
            }
            ViewData["Customer"] = customers;
            ViewData["Product"] = products;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            if (ModelState.IsValid)
            {
                //Table Service
                order.Order_Date = DateTime.SpecifyKind(order.Order_Date,DateTimeKind.Utc);
                order.PartitionKey = "OrderPartition";
                order.RowKey = "OrderRowkey";

                await _tableStorageService.AddOrderAsync(order);
                //MessageQueue
                string message = $"New order by Customer: {order.Customer_Id}" + $"of the product {order.Product_Id}" + $" in the size: {order.Order_Size}" + $" on {order.Order_Date}";
                await _queueService.SendMessage(message);
                return RedirectToAction("Index");
            }
            //Reload customers and products list if validation fails
            var customers = await _tableStorageService.GetCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();
            ViewData["Customer"] = customers;
            ViewData["Product"] = products;
            return View (order);
        }
    }


}
