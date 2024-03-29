﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Remoting;
using Common.Models;
using Common.FrontendModels;

namespace Common
{
    [ServiceContract]
    public interface IOrder : IService
    {
        [OperationContract]
        Task<List<OrderModel>> GetOrders(string userId);

        [OperationContract]
        Task<bool> Prepare(CreateOrderModel order);

        [OperationContract]
        Task<bool> Commit(CreateOrderModel order);

        [OperationContract]
        Task<string> CreateOrder(Order order);
    }
}
