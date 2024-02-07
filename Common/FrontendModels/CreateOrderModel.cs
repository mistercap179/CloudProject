using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.FrontendModels
{
    public  class CreateOrderModel
    {
        public UserModel User { get; set; } = new UserModel();
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public double TotalPrice { get; set; }
        public string Type { get; set; } = String.Empty;
    }
}
