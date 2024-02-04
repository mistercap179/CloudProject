using Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DB
{
    public class DbHelper<T> where T : TableEntity, new()
    {
        private readonly CloudTable table;

        public DbHelper(string tableName, string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(tableName);
            bool res = table.CreateIfNotExistsAsync().Result;
        }

        public async Task InsertOrUpdateEntityAsync(T entity)
        {
            var operation = TableOperation.InsertOrReplace(entity);
            await table.ExecuteAsync(operation);
        }

        public async Task<IEnumerable<User>> GetUsersByEmailAndPasswordAsync(string email, string password)
        {
            var allUsers = await GetAllEntitiesAsync("User");
            return allUsers.OfType<User>().Where(user => user.Email == email && user.Password == password);
        }

        public async Task<IEnumerable<Order>> GetOrdersByIdAsync(string userId)
        {
            var allOrders = await GetAllEntitiesAsync("Order");
            return allOrders.OfType<Order>().Where(order => order.UserId == userId);
        }

        public async Task<IEnumerable<User>> GetUsersByIdAsync(string userId)
        {
            var allUsers = await GetAllEntitiesAsync("User");
            return allUsers.OfType<User>().Where(user => user.UserId == userId);
        }

        public async Task<IEnumerable<Product>> GetProductByIdAsync(string productId)
        {
            var allProdcts = await GetAllEntitiesAsync("Product");
            return allProdcts.OfType<Product>().Where(product => product.ProductId == productId);
        }

        public async Task<IEnumerable<T>> GetAllEntitiesAsync(string partitionKey)
        {
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var queryResult = await table.ExecuteQuerySegmentedAsync(query, token: null);
            return queryResult.Results;
        }
    }
}
