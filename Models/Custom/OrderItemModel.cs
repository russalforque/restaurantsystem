using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uling_RestaurantManagementSystem.Models.Custom
{
    public class OrderItemModel
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public string MenuName { get; set; }
        public decimal Unit_Price { get; set; }
        public int Quantity { get; set; }
        public int DiscountId { get; set; }
        public decimal LineTotal { get; set; }
    }
}