using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Uling_RestaurantManagementSystem.Models.SQL;

namespace Uling_RestaurantManagementSystem.Models.Custom
{
    public class CashierPOSViewModel
    {
        public IEnumerable<tbl_menu_items> MenuItems { get; set; }
        public IEnumerable<tbl_discounts> Discounts { get; set; }
    }
}