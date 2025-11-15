using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models
{
    //Order & inventory management using queues
    public class Order : ITableEntity
    {
        //In the order you need when the order was placed , who  placed it and what sized jersey did they get,how many they got maybe
        [Key]
        public int Order_ID { get; set; }
        public string? PartitionKey { get; set; } 
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
        //Validation Sample
        [Required(ErrorMessage = "Please select a Customer Profile")]
        public int Customer_Id { get; set; } //Fk to the customer who made the order

        [Required(ErrorMessage ="Please select a product")]
        public int Product_Id { get; set; } //Fk to the product

        [Required(ErrorMessage ="Please select the date")]

        public DateTime Order_Date { get; set; }

        [Required(ErrorMessage = "Please enter the Size")]

        public string? Order_Size { get; set; }


    }
}
