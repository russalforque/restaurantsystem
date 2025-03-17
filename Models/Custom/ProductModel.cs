using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uling_RestaurantManagementSystem.Models.Custom
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int UnitPrice { get; set; }
    }
}