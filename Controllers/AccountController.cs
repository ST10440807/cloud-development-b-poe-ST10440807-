using Microsoft.EntityFrameworkCore;
using ABCRetailers.Data;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailers.Controllers
{
    //Controller to manage login
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        //GEt : Account Login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.
                Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Set session
            HttpContext.Session.SetInt32("UserId", user.User_Id);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.Role);

            // Verify password
            if (!_passwordService.VerifyPassword(user, user.Password, model.Password))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            if (user.Customer != null)
            {
                HttpContext.Session.SetInt32("CustomerId", user.Customer.Customer_Id);
                HttpContext.Session.SetString("CustomerName", user.Customer.Customer_Name);
                TempData["SuccessMessage"] = $"Welcome back, {user.Customer.Customer_Name}!";
            }
            else
            {
                TempData["SuccessMessage"] = $"Welcome back!";
            }

            // Redirect based on role
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        


        public IActionResult Index()
        {
            return View();
        }
    }
}
