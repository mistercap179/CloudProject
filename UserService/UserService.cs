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

namespace UserService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class UserService : StatefulService,IUser
    {
        private readonly string connectionString;
        private readonly DbHelper<User> userTableHelper;

        public UserService(StatefulServiceContext context) : base(context)
        {
            this.connectionString = "DefaultEndpointsProtocol=https;AccountName=onlineshopcloud;AccountKey=KtS5e92B2XV5R3h23jZo78TN17DzlKbATEvfrjUR9j7xcI2z25QUx0gSUy+H4NvGzI/Au+LPuZdj+AStx/qZJQ==;EndpointSuffix=core.windows.net";

            this.userTableHelper = new DbHelper<User>("User", this.connectionString);
        }


        public async Task<UserModel> Login(string email, string password)
        {
            var userDictionary = await this.StateManager.GetOrAddAsync
                                    <IReliableDictionary<string, User>>("userDictionary");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await userDictionary.CreateEnumerableAsync(tx);
                var enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    var user = enumerator.Current.Value;

                    if (user.Email == email && user.Password == password)
                    {
                        UserModel returnUser = new UserModel
                        {
                            UserId = user.UserId,
                            AccountBalance = user.AccountBalance,
                            Email = user.Email,
                            Password = user.Password,
                            FirstName = user.FirstName,
                            LastName = user.LastName
                        };

                        return returnUser;
                    }
                }

                return null;
            }
        }

        public async Task<string> Register(User user)
        {
            try
            {
                user.RowKey = user.UserId;
                user.PartitionKey = "User";
                await this.userTableHelper.InsertOrUpdateEntityAsync(user);
                return "Registracija uspesna";
            }
            catch (Exception ex)
            {
                return $"Greška prilikom registracije: {ex.Message}";
            }
        }

        public async Task<UserModel> Update(User user)
        {
            try
            {
                user.RowKey = user.UserId;
                user.PartitionKey = "User";

                await this.userTableHelper.InsertOrUpdateEntityAsync(user);

                UserModel returnUser = new UserModel
                {
                    UserId = user.UserId,
                    AccountBalance = user.AccountBalance,
                    Email = user.Email,
                    Password = user.Password,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };

                return returnUser;
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
                var userDictionary = await this.StateManager.GetOrAddAsync
                                   <IReliableDictionary<string, User>>("userDictionary");

                var allUsers = this.userTableHelper.GetAllEntitiesAsync("User").Result;

                using (var tx = this.StateManager.CreateTransaction())
                {
                    foreach (var user in allUsers)
                    {
                        await userDictionary.AddOrUpdateAsync(tx, user.UserId, user, (key, oldValue) => user);
                    }

                    await tx.CommitAsync();
                }

            }
            catch(Exception ex)
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
