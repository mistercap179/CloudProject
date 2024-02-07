using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public  class Product : TableEntity
    {
        public string ProductId { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; } = String.Empty;
    }
}
