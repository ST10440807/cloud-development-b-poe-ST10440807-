using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models
{
    //Store customer info  using azure table storage
    public class Customer : ITableEntity
    {
        [Key]
        public int Customer_Id { get; set; } 
        public string? Customer_Name { get; set; }

        //Login information for coming parts of assignment
        public string? email { get; set; }
        public string? password { get; set; }

        //ITableEntity implementaion
        public string PartitionKey { get; set; } //
        public string RowKey { get; set; }

        public ETag ETag { get; set; } //

        public DateTimeOffset? Timestamp { get; set; }


        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual Cart Cart { get; set; }

    }
}
