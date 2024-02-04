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
    public interface IProduct : IService 
    {
        [OperationContract]
        Task<List<ProductModel>> GetProducts();
    }
}
