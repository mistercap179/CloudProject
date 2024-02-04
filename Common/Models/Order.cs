using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Order : TableEntity
    {
        public string OrderId { get; set; } = String.Empty;
        public string UserId { get; set; } = String.Empty;
        public string Products { get; set; } = String.Empty;
        public double TotalPrice { get; }
        
    }
}
