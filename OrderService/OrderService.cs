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
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace OrderService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class OrderService : StatefulService,IOrder
    {
        private readonly string connectionString;
        private readonly DbHelper<Order> orderTable;
        public OrderService(StatefulServiceContext context)
            : base(context)
        {
            this.connectionString = "DefaultEndpointsProtocol=https;AccountName=onlineshopcloud;AccountKey=KtS5e92B2XV5R3h23jZo78TN17DzlKbATEvfrjUR9j7xcI2z25QUx0gSUy+H4NvGzI/Au+LPuZdj+AStx/qZJQ==;EndpointSuffix=core.windows.net";

            this.orderTable = new DbHelper<Order>("Order", this.connectionString);
        }

        public async Task<List<OrderModel>> GetOrders(string userId)
        {
            var orderDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Order>>("orderDictionary");
            var orderList = new List<OrderModel>();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await orderDictionary.CreateEnumerableAsync(tx);

                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default))
                    {
                        var current = enumerator.Current;
                        var order = current.Value;
                        if (order.UserId == userId)
                        {
                            var orderModel = new OrderModel
                            { 
                                OrderId = order.OrderId,
                                UserId = order.OrderId,
                                Products = order.Products,
                                TotalPrice = order.TotalPrice
                            };

                            orderList.Add(orderModel);
                        }
                    }
                }
            }

            return orderList;
        }


        private async Task Initialization()
        {
            try
            {
                var productDictionary = await this.StateManager.GetOrAddAsync
                        <IReliableDictionary<string, Order>>("orderDictionary");

                var allOrders = this.orderTable.GetAllEntitiesAsync("Order").Result;

                using (var tx = this.StateManager.CreateTransaction())
                {
                    foreach (var order in allOrders)
                    {
                        await productDictionary.AddOrUpdateAsync(tx, order.OrderId, order, (key, oldValue) => order);
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

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");
            await this.Initialization();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);
                    await this.Initialization();
                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
