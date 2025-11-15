using Microsoft.EntityFrameworkCore;
using ABCRetailers.Models;

namespace ABCRetailers.Data
{
    //Azure db context 
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           base.OnModelCreating(modelBuilder);
            //Seeding data
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Email = "admin@gmail.com",
                Password = "Admin@123",
                Role = "Admin",
                Customer_Id = 1

            },
            new User
            {
                UserId = 2,
                Email = "customer@gmail.com",
                Password = "Customer@123",
                Role = "Customer",
                Customer_Id = 2
            });

            modelBuilder.Entity<Customer>().HasData(new Customer
            {
                Customer_Id = 1,
                Customer_Name = "Test",
                email = "customer@gmail.com",

            });
        }

        
        
    }
}
