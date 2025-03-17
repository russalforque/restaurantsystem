using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

// imports
using Uling_RestaurantManagementSystem.Models.SQL;
using Uling_RestaurantManagementSystem.Models.Custom;
using Microsoft.Ajax.Utilities;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class CashierDashboardController : Controller
    {
        private db_urmsEntities db = new db_urmsEntities();

        private bool IsUserAuthorized(int userType)
        {
            if (userType != 2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private ActionResult NotAuthorized(int userType)
        {
            string actionName = "";
            string controllerName = "";

            /*
                User Types

                1 ---> Manager
                2 ---> Cashier
                3 ---> Cook
            */

            switch (userType)
            {
                case 1:
                    actionName = "LoadManagerDashboard";
                    controllerName = "ManagerDashboard";
                    break;
                case 3:
                    actionName = "LoadCookDashboard";
                    controllerName = "CookDashboard";
                    break;
                default:
                    actionName = "SignIn";
                    controllerName = "Main";
                    break;
            }
            return RedirectToAction(actionName, controllerName);
        }

        [HttpGet]
        public ActionResult LoadCashierDashboard()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            int orderCount = db.tbl_orders
                .Count(o =>
                    // fetch orders that doesn't have any corresponding payment and reservation record
                    !o.tbl_payment.Any() &&
                    !o.tbl_reservations.Any() &&
                    // get counter order type orders only
                    o.order_type == "Counter" &&
                    o.order_status != "Cancelled" &&
                    o.order_status != "Declined"
                );

            CashierDashboardDataModel cashierDashboardDataModel = new CashierDashboardDataModel
            {
                OrderCount = orderCount
            };

            ViewBag.CurrentPage = "cashier__dashboard";
            return View(cashierDashboardDataModel);
        }
    }
}