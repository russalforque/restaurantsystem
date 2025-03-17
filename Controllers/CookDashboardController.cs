using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

// imports
using Uling_RestaurantManagementSystem.Models.SQL;
using Uling_RestaurantManagementSystem.Models.Custom;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class CookDashboardController : Controller
    {
        private db_urmsEntities db = new db_urmsEntities();

        private bool IsUserAuthorized(int userType)
        {
            if (userType != 3)
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
                case 2:
                    actionName = "LoadCashierDashboard";
                    controllerName = "CashierDashboard";
                    break;
                default:
                    actionName = "SignIn";
                    controllerName = "Main";
                    break;
            }
            return RedirectToAction(actionName, controllerName);
        }

        [HttpGet]
        public ActionResult LoadCookDashboard()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            int counterOrderCount = db.tbl_orders
                .Count(o =>
                    // fetch orders that doesn't have any corresponding payment and reservation record
                    !o.tbl_payment.Any() &&
                    !o.tbl_reservations.Any() &&
                    // get counter order type orders only
                    o.order_type == "Counter"
                );

            int reservationCount = db.tbl_orders
                .Where(o =>
                    o.tbl_reservations.Any() &&
                    o.tbl_payment.Any() &&
                    // status (only fetch the reservations that is already verified and paid)
                    o.order_status == "Accepted" &&
                    o.tbl_reservations.FirstOrDefault().reservation_status == "Accepted" &&
                    o.tbl_payment.FirstOrDefault().payment_status == "Accepted" &&
                    // fetch orders that type is Online and payment method is not CASH
                    o.order_type == "Online" &&
                    o.tbl_payment.FirstOrDefault().payment_method != "Cash"
                )
                .Count();

            int takeOutOrdersCount = db.tbl_orders
                .Where(o => 
                    o.tbl_payment.Any() &&
                    !o.tbl_reservations.Any() &&
                    // status (only fetch the take-out orders that is already accepted and paid)
                    o.order_status == "Accepted" &&
                    o.tbl_payment.FirstOrDefault().payment_status == "Accepted" &&
                    // fetch orders that type is Online and payment method is not CASH
                    o.order_type == "Online" &&
                    o.tbl_payment.FirstOrDefault().payment_method != "Cash"
                )
                .Count();

            CookDashboardDataModel cookDashboardDataModel = new CookDashboardDataModel
            {
                CounterOrders = counterOrderCount,
                ReservationOrders = reservationCount,
                TakeOutOrders = takeOutOrdersCount
            };

            ViewBag.CurrentPage = "cook__dashboard";
            return View(cookDashboardDataModel);
        }
    }
}