using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using Common;
using Common.Models;
using Common.FrontendModels;

namespace ApiGateway
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ApiGateway : StatelessService,IApiGateway
    {
        public ApiGateway(StatelessServiceContext context)
            : base(context)
        { }

        public async Task<List<OrderModel>> GetOrders(string userId)
        {
            IOrder orderService = ServiceProxy.Create<IOrder>(new Uri("fabric:/OnlineShopServiceFabric/OrderService"), new ServicePartitionKey(1));

            try
            {
                return await orderService.GetOrders(userId);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<List<ProductModel>> GetProducts()
        {
            IProduct prouctService = ServiceProxy.Create<IProduct>(new Uri("fabric:/OnlineShopServiceFabric/ProductService"), new ServicePartitionKey(1));

            try
            {
                return await prouctService.GetProducts();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<UserModel> Login(string email,string password)
        {
            IUser userService = ServiceProxy.Create<IUser>(new Uri("fabric:/OnlineShopServiceFabric/UserService"), new ServicePartitionKey(1));

            try
            {
                return await userService.Login(email, password);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> Register(Common.FrontendModels.UserModel user)
        {
            IUser userService = ServiceProxy.Create<IUser>(new Uri("fabric:/OnlineShopServiceFabric/UserService"), new ServicePartitionKey(1));

            try
            {

                User newUser = new User
                {
                    UserId = new Guid().ToString(),
                    AccountBalance = user.AccountBalance,
                    Email = user.Email,
                    Password = user.Password,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };

                return await userService.Register(newUser);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UserModel> Update(Common.FrontendModels.UserModel user)
        {
            IUser userService = ServiceProxy.Create<IUser>(new Uri("fabric:/OnlineShopServiceFabric/UserService"), new ServicePartitionKey(1));

            try
            {
                User updatedUser = new User
                {
                    UserId = user.UserId,
                    AccountBalance = user.AccountBalance,
                    Email = user.Email,
                    Password = user.Password,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                return await userService.Update(updatedUser);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
