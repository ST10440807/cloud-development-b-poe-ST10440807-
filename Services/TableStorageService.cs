using ABCRetailers.Models;
using Azure;
using Azure.Data.Tables;
using System.Security.Cryptography.Pkcs;

namespace ABCRetailers.Services
{
    //Services for table storage implementation
    public class TableStorageService
    {
        //TableClient performs account level operations like creating and deleting tables 
        public readonly TableClient _customerTableClient; //For customer table
        public readonly TableClient _productTableClient; //For product table
        public readonly TableClient _orderTableClient; // for order table


        public TableStorageService(string connectionString)
        {
            _customerTableClient = new TableClient(connectionString, "Customer");
            _productTableClient = new TableClient(connectionString, "Product");
            _orderTableClient = new TableClient(connectionString, "orders");
        }


        //Get a list of all customers
        public async Task<List<Customer>> GetCustomersAsync()
        {
            //Loop and get list of all customers
            var customers = new List<Customer>();

            await foreach(var customer in _customerTableClient.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }
            return customers;
        }

        //Add customer
        public async Task AddCustomerAsync(Customer customer)
        {
            //Error handling 
            if (string.IsNullOrEmpty(customer.PartitionKey)|| string.IsNullOrEmpty(customer.RowKey)) //Check if rowkey and partition keys are there
            {
                throw new ArgumentException("PartitionKey and RowKey must be set");
            }
            try
            {
                await _customerTableClient.AddEntityAsync(customer); //Or try and add the customer to the table storage
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to table storage",ex);
            }
        }

        //Delete customer
        public async Task DeleteCustomerAsync(string partitionKey,string rowKey)
        {
            await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
        }


        //--------Products-----------
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();

           await foreach (var product in _productTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }
           return products;
        }

        //Add products
        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
            {
                throw new ArgumentException("PartitonKey and RowKey must be set");
            }
            try
            {
                await _productTableClient.AddEntityAsync(product);
            }
            catch(RequestFailedException ex) 
            {
                throw new InvalidOperationException("Error adding entity to table storage", ex);
            }
        }
        //Delete product
        public async Task DeleteProductAsync(string partitionKey,string rowKey)
        {
            await _productTableClient.DeleteEntityAsync(partitionKey,rowKey);
        }

        //--------Orders-----------
        public async Task AddOrderAsync(Order order)
        {
            //Add an order to the table storage
            if (string.IsNullOrEmpty(order.PartitionKey) || string.IsNullOrEmpty(order.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set");
            }
            try
            {
                await _orderTableClient.AddEntityAsync(order);
            }
            catch(RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding order to table storage",ex);
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            //Get list of all orders in table storage
            var orders = new List<Order>();
            await foreach (var order in _orderTableClient.QueryAsync<Order>())
            {
                orders.Add(order);
            }
            return orders;
        }
    }
}
