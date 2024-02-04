using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.FrontendModels
{
    [DataContract]
    public class ProductModel
    {
        [DataMember]
        public string ProductId { get; set; } = String.Empty;
        [DataMember]
        public string Name { get; set; } = String.Empty;
        [DataMember]
        public string Description { get; set; } = String.Empty;
        [DataMember]
        public double Price { get; set; }
        [DataMember]
        public int Quantity { get; set; }
    }
}
