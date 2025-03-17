using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uling_RestaurantManagementSystem.Models.Custom
{
    public class ManagerDashboardDataModel
    {
        public int PendingReservationCount { get; set; }

        public int AcceptedReservationCount { get; set; }

        public int PendingTakeOutOrdersCount { get; set; }

        public int AcceptedTakeOutOrdersCount { get; set; }

        public int AccountCount { get; set; }
    }
}