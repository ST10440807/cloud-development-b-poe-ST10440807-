using Azure;
using Azure.Data.Tables;

namespace ABCRetailers.Models
{
    //Implement blob storage for product images ,also use table storage for info
    public class Product : ITableEntity
    {
        //Product info(we are selling football jerseys )
        public int Product_Id { get; set; } 

        public string? Product_Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        //Jersey related info finalize & dont forget to add it in views 
        public string? Size { get; set; }

        public double? Price { get; set; }
        
        public int Quantity { get; set; }

        //ITablEntity implementation
        public string? PartitionKey { get; set; } //
        public string? RowKey { get; set; }

        public ETag ETag { get; set; } //

        public DateTimeOffset? Timestamp { get; set; }
    }
}
