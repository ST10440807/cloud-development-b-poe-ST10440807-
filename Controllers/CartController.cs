using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABCRetailers.Data;
using ABCRetailers.Models;

namespace ABCRetailers.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cart/Index
        // Shows the customer's shopping cart
        public async Task<IActionResult> Index()
        {
            // Check if customer is logged in
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["ErrorMessage"] = "Please login to view your cart.";
                return RedirectToAction("Login", "Account");
            }

            // Get customer's cart with all items and product details
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Customer_Id == customerId.Value);

            // If cart doesn't exist, create it
            if (cart == null)
            {
                cart = new Cart
                {
                    Customer_Id = customerId.Value,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return View(cart);
        }

        // POST: Cart/AddToCart
        // Adds a product to the shopping cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity, string size)
        {
            // Check if customer is logged in
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                TempData["ErrorMessage"] = "Please login to add items to cart.";
                return RedirectToAction("Login", "Account");
            }

            // Validate product exists and is available
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("Index", "Products");
            }

            // Check if enough stock is available
            if (product.Quantity < quantity)
            {
                TempData["ErrorMessage"] = $"Only {product.Quantity} items available in stock.";
                return RedirectToAction("Index", "Products");
            }

            // Get or create customer's cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Customer_Id == customerId.Value);

            if (cart == null)
            {
                cart = new Cart
                {
                    Customer_Id = customerId.Value,
                    CreatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Check if this exact item (same product and size) is already in cart
            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.Product_Id == productId && ci.Size == size);

            if (existingItem != null)
            {
                // Update existing item quantity
                existingItem.Quantity += quantity;

                // Make sure we don't exceed available stock
                if (existingItem.Quantity > product.Quantity)
                {
                    existingItem.Quantity = product.Quantity;
                    TempData["ErrorMessage"] = $"Added maximum available quantity ({product.Quantity}) to cart.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"{product.Product_Name} quantity updated in cart!";
                }
            }
            else
            {
                // Add new item to cart
                var cartItem = new CartItem
                {
                    Cart_Id = cart.Cart_Id,
                    Product_Id = productId,
                    Quantity = quantity,
                    Size = size,
                    AddedDate = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
                TempData["SuccessMessage"] = $"{product.Product_Name} added to cart successfully!";
            }

            // Update cart last modified time
            cart.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Products");
        }

        // POST: Cart/UpdateQuantity
        // Updates the quantity of an item in the cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the cart item (ensure it belongs to the logged-in customer)
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartItem_Id == cartItemId && ci.Cart.Customer_Id == customerId.Value);

            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Cart item not found.";
                return RedirectToAction("Index");
            }

            // If quantity is 0 or negative, remove the item
            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
                TempData["SuccessMessage"] = "Item removed from cart.";
            }
            // Check if requested quantity exceeds available stock
            else if (cartItem.Product.Quantity < quantity)
            {
                TempData["ErrorMessage"] = $"Only {cartItem.Product.Quantity} items available in stock.";
                return RedirectToAction("Index");
            }
            else
            {
                // Update quantity
                cartItem.Quantity = quantity;
                TempData["SuccessMessage"] = "Cart updated successfully!";
            }

            // Update cart last modified time
            cartItem.Cart.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // POST: Cart/RemoveItem
        // Removes an item from the cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the cart item (ensure it belongs to the logged-in customer)
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItem_Id == cartItemId && ci.Cart.Customer_Id == customerId.Value);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                cartItem.Cart.LastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item removed from cart.";
            }

            return RedirectToAction("Index");
        }

        // GET: Cart/Checkout
        // Shows the checkout page with order summary
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get cart with items
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Customer_Id == customerId.Value);

            // Ensure cart exists and has items
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            return View(cart);
        }

        // POST: Cart/PlaceOrder
        // Creates an order from the cart items
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string shippingAddress)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate shipping address
            if (string.IsNullOrWhiteSpace(shippingAddress))
            {
                TempData["ErrorMessage"] = "Please provide a shipping address.";
                return RedirectToAction("Checkout");
            }

            // Get cart with items
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Customer_Id == customerId.Value);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            // Verify stock availability for all items
            foreach (var item in cart.CartItems)
            {
                if (item.Product.Quantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Insufficient stock for {item.Product.Product_Name}. Only {item.Product.Quantity} available.";
                    return RedirectToAction("Index");
                }
            }

            // Create the order with PENDING status
            var order = new Order
            {
                Customer_Id = customerId.Value,
                Order_Date = DateTime.UtcNow,  
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            decimal totalAmount = 0;

            // Create order items from cart items and update product quantities
            foreach (var cartItem in cart.CartItems)
            {
                // Create order item
                var orderItem = new OrderItem
                {
                    Order_ID = order.Order_ID,
                    Product_Id = cartItem.Product_Id,
                    Quantity = cartItem.Quantity,
                    Size = cartItem.Size,
                  
                };

                totalAmount += orderItem.Subtotal;

                // Reduce product quantity
                cartItem.Product.Quantity -= cartItem.Quantity;

                _context.OrderItems.Add(orderItem);
            }

            // Update order total
            order.TotalAmount = totalAmount;

            // Clear the cart
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order placed successfully! Order ID: {order.Order_ID}";
            return RedirectToAction("OrderConfirmation", new { orderId = order.Order_ID });
        }

        // GET: Cart/OrderConfirmation
        // Shows order confirmation page after successful checkout
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the order with all details (ensure it belongs to logged-in customer)
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Order_ID == orderId && o.Customer_Id == customerId.Value);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Cart/MyOrders
        // Shows customer's order history
        public async Task<IActionResult> MyOrders()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get all orders for the customer
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Customer_Id == customerId.Value)
                .OrderByDescending(o => o.Order_Date)
                .ToListAsync();

            return View(orders);
        }
    }
}