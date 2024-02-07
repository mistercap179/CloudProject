using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.DB;
using Common.FrontendModels;
using Common.Models;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ProductService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ProductService : StatefulService,IProduct
    {
        private readonly string connectionString;
        private readonly DbHelper<Product> productTable;
        public ProductService(StatefulServiceContext context)
            : base(context)
        {
            this.connectionString = "DefaultEndpointsProtocol=https;AccountName=onlineshopcloud;AccountKey=KtS5e92B2XV5R3h23jZo78TN17DzlKbATEvfrjUR9j7xcI2z25QUx0gSUy+H4NvGzI/Au+LPuZdj+AStx/qZJQ==;EndpointSuffix=core.windows.net";

            this.productTable = new DbHelper<Product>("Product", this.connectionString);

        }

        public async Task<List<ProductModel>> GetProducts()
        {

            var productDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Product>>("productDictionary");
            var productList = new List<ProductModel>();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await productDictionary.CreateEnumerableAsync(tx);

                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default))
                    {
                        var current = enumerator.Current;
                        var product = current.Value;
                        if (product.Quantity > 0)
                        {
                            var productModel = new ProductModel
                            {
                                ProductId = product.ProductId,
                                Name = product.Name,
                                Description = product.Description,
                                Price = product.Price,
                                Quantity = product.Quantity,
                                Category = product.Category
                            };

                            productList.Add(productModel);
                        }
                    }
                }
            }

            return productList;

        }

        public async Task<List<Product>> GetProductsRollBack()
        {

            var productDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Product>>("productDictionary");
            var productList = new List<Product>();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await productDictionary.CreateEnumerableAsync(tx);

                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default))
                    {
                        var current = enumerator.Current;
                        var product = current.Value;
                        productList.Add(product);
                    }
                }
            }

            return productList;

        }

        public async Task RefreshDateBase()
        {
            try
            {
                var productDictionary = await this.StateManager.GetOrAddAsync
                                    <IReliableDictionary<string, Product>>("productDictionary");

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var enumerable = await productDictionary.CreateEnumerableAsync(tx);
                    var enumerator = enumerable.GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        Product product = enumerator.Current.Value;

                        await this.productTable.InsertOrUpdateEntityAsync(product);
                    }

                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        private async Task Initialization()
        {
            try
            {
                var productDictionary = await this.StateManager.GetOrAddAsync
                                   <IReliableDictionary<string, Product>>("productDictionary");

                var allProducts = this.productTable.GetAllEntitiesAsync("Product").Result;

                using (var tx = this.StateManager.CreateTransaction())
                {
                    foreach (var product in allProducts)
                    {
                        await productDictionary.AddOrUpdateAsync(tx, product.ProductId, product, (key, oldValue) => product);
                    }

                    await tx.CommitAsync();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
           
        }

        private async Task CopyDict()
        {
            try
            {
                var productDictionary = await this.StateManager.GetOrAddAsync
                                   <IReliableDictionary<string, Product>>("productDictionaryCopy");

                var allProducts = this.productTable.GetAllEntitiesAsync("Product").Result;

                using (var tx = this.StateManager.CreateTransaction())
                {
                    foreach (var product in allProducts)
                    {
                        await productDictionary.AddOrUpdateAsync(tx, product.ProductId, product, (key, oldValue) => product);
                    }

                    await tx.CommitAsync();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            await this.Initialization();
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    await this.RefreshDateBase();
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public async Task<bool> Commit(CreateOrderModel order)
        {
            try
            {
                await this.CopyDict();

                var productDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Product>>("productDictionary");

                using (var tx = this.StateManager.CreateTransaction())
                {
                    foreach (var cartItem in order.CartItems)
                    {
                        ConditionalValue<Product> result = await productDictionary.TryGetValueAsync(tx, cartItem.ProductId);

                        if (result.HasValue)
                        {
                            var existingProduct = result.Value;

                            int newQuantity = existingProduct.Quantity - cartItem.Quantity;

                            Product updatedProduct = new Product
                            {
                                ProductId = existingProduct.ProductId,
                                Quantity = newQuantity,
                                Price = existingProduct.Price,
                                Description = existingProduct.Description,
                                Name = existingProduct.Name,
                                Category = existingProduct.Category
                            };

                            updatedProduct.PartitionKey = "Product";
                            updatedProduct.RowKey = updatedProduct.ProductId;

                            await productDictionary.AddOrUpdateAsync(tx, updatedProduct.ProductId, updatedProduct, (key, oldValue) => updatedProduct);
                        }
                        else
                        {
                            
                            return false;
                        }
                    }

                   
                    await tx.CommitAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška prilikom commit-a: {ex.Message}");
            }
        }


        public async Task<bool> Prepare(CreateOrderModel order)
        {
            var productDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Product>>("productDictionary");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await productDictionary.CreateEnumerableAsync(tx);

                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    foreach (var cartItem in order.CartItems)
                    {
                        bool found = false;

                        while (await enumerator.MoveNextAsync(default))
                        {
                            var current = enumerator.Current;
                            var product = current.Value;

                            if (cartItem.ProductId == product.ProductId)
                            {
                                found = true;

                                if (cartItem.Quantity <= product.Quantity)
                                {
                                    break;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }

                        if (!found)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
        }

        public async Task<bool> Rollback(CreateOrderModel order)
        {
            try
            {
                var reliableDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Product>>("productDictionaryCopy");
                var productDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Product>>("productDictionary");

                using (var tx = this.StateManager.CreateTransaction())
                {
                    await productDictionary.ClearAsync();

                    var enumerator = (await reliableDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var current = enumerator.Current;
                        await productDictionary.AddOrUpdateAsync(tx, current.Key, current.Value, (key, oldValue) => current.Value);
                    }

                    await tx.CommitAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Greška prilikom rollback-a: {ex.Message}");
            }
        }

        public async Task<Product> GetProductById(string productID, IReliableDictionary<string, Product> productDictionary)
        {
            try
            {
                using (var tx = this.StateManager.CreateTransaction())
                {
                    var enumerable = await productDictionary.CreateEnumerableAsync(tx);
                    var enumerator = enumerable.GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var product = enumerator.Current.Value;

                        if (product.ProductId == productID)
                        {
                            return product;
                        }
                    }

                    return null;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
