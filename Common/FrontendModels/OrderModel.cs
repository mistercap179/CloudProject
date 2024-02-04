using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.FrontendModels
{
    [DataContract]
    public class OrderModel
    {
        [DataMember]
        public string OrderId { get; set; } = String.Empty;
        [DataMember]
        public string UserId { get; set; } = String.Empty;
        [DataMember]
        public string Products { get; set; } = String.Empty;
        [DataMember]
        public double TotalPrice { get; set; }
    }
}
