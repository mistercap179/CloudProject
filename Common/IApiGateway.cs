using Common.FrontendModels;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IApiGateway : IService
    {
        [OperationContract]
        Task<List<OrderModel>> GetOrders(string userId);

        [OperationContract]
        Task<List<ProductModel>> GetProducts();

        [OperationContract]
        Task<UserModel> Login(string email, string password);

        [OperationContract]
        Task<UserModel> Update(Common.FrontendModels.UserModel user);

        [OperationContract]
        Task<string> Register(Common.FrontendModels.UserModel user);


    }
}
