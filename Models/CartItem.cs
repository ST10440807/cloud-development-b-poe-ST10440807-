using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABCRetailers.Models
{
    [Table("CartItems")]
    public class CartItem
    {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int CartItem_Id { get; set; }

            [Required]
            [ForeignKey("Cart")]
            public int Cart_Id { get; set; }

            [Required]
            [ForeignKey("Product")]
            public int Product_Id { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
            public int Quantity { get; set; }

            [Required]
            [StringLength(10)]
            public string Size { get; set; }

            public DateTime AddedDate { get; set; } = DateTime.UtcNow;

            // Navigation properties
            public virtual Cart Cart { get; set; }
            public virtual Product Product { get; set; }
        }
    }

