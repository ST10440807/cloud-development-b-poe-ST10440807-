using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABCRetailers.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }


        //Login Model 
        //Login information
        //Email
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //password
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } = "Admin";


        // Navigation 
        [ForeignKey("Customer")]
        public int? Customer_Id { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
