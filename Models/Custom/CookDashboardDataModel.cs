using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uling_RestaurantManagementSystem.Models.Custom
{
    public class CookDashboardDataModel
    {
        public int CounterOrders { get; set; }
        public int ReservationOrders { get; set; }
        public int TakeOutOrders { get; set; }
    }
}