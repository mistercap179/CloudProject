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
    public interface IUser : IService
    {
        [OperationContract]
        Task<UserModel> Login(string email, string password);

        [OperationContract]
        Task<string> Register(User user);

        [OperationContract]
        Task<UserModel> Update(User user);

        [OperationContract]
        Task<UserModel> GetUserById(string userId);

        [OperationContract]
        Task<bool> Prepare(CreateOrderModel order);

        [OperationContract]
        Task<bool> Commit(CreateOrderModel order);

        [OperationContract]
        Task<bool> Rollback(CreateOrderModel order);

        [OperationContract]
        Task<User> GetUserByIdRollBack(string userId);

    }
}
