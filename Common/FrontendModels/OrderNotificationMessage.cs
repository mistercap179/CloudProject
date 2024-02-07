using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.FrontendModels
{
    [DataContract]
    public class OrderNotificationMessage
    {
        [DataMember]
        public string OrderId { get; set; } = string.Empty;

        [DataMember]
        public string Message { get; set; } = string.Empty;
    }
}
