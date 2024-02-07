using Common.FrontendModels;
using Common.Models;
using Microsoft.ServiceFabric.Data.Collections;
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
    public interface IProduct : IService 
    {
        [OperationContract]
        Task<List<ProductModel>> GetProducts();

        [OperationContract]
        Task<bool> Prepare(CreateOrderModel order);

        [OperationContract]
        Task<bool> Commit(CreateOrderModel order);

        [OperationContract]
        Task<bool> Rollback(CreateOrderModel order);

        [OperationContract]
        Task<Product> GetProductById(string productID, IReliableDictionary<string, Product> productDictionary);
        
        [OperationContract]
        Task<List<Product>> GetProductsRollBack();

    }
}
