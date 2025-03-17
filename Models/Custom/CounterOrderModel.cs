using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Uling_RestaurantManagementSystem.Models.SQL;

namespace Uling_RestaurantManagementSystem.Models.Custom
{
    public class CounterOrderModel
    {
        public tbl_orders Order { get; set; }
        public List<tbl_order_items> OrderItems { get; set; }
    }
}