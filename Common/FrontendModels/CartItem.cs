using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.FrontendModels
{
    public class CartItem
    {
        public string ProductId { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
}
