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
    public class ManagerDashboardController : Controller
    {
        private db_urmsEntities db = new db_urmsEntities();

        private bool IsUserAuthorized(int userType)
        {
            if (userType != 1)
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
                case 2:
                    actionName = "LoadCashierDashboard";
                    controllerName = "CashierDashboard";
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
        public ActionResult LoadManagerDashboard()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            int pendingreservationCount = db.tbl_orders
                .Where(o =>
                    o.tbl_reservations.Any() &&
                    o.tbl_payment.Any() &&
                    // Fetch only online orders & corresponding payment record payment method must not be CASH
                    o.order_type == "Online" &&
                    o.order_status == "Pending" &&
                    o.tbl_payment.FirstOrDefault().payment_method != "Cash"
                )
                .Count();

            int acceptedreservationCount = db.tbl_orders
                .Where(o =>
                    o.tbl_reservations.Any() &&
                    o.tbl_payment.Any() &&
                    // Fetch only online orders & corresponding payment record payment method must not be CASH
                    o.order_type == "Online" &&
                    (o.order_status == "Accepted" ||
                    o.order_status == "Preparation" ||
                    o.order_status == "Ready") &&
                    o.tbl_payment.FirstOrDefault().payment_method != "Cash"
                )
                .Count();

            int pendingtakeOutOrdersCount = db.tbl_orders
                .Where(o => 
                    o.tbl_payment.Any() &&
                    // Fetch only orders that doesn't have any corresponding reservation orders
                    !o.tbl_reservations.Any() &&
                    // Fetch only online orders & corresponding payment record payment method must not be CASH
                    o.order_type == "Online" &&
                    o.order_status == "Pending" &&
                    o.tbl_payment.FirstOrDefault().payment_method != "Cash"
                )
                .Count();

            int acceptedtakeOutOrdersCount = db.tbl_orders
                .Where(o =>
                    o.tbl_payment.Any() &&
                    // Fetch only orders that doesn't have any corresponding reservation orders
                    !o.tbl_reservations.Any() &&
                    // Fetch only online orders & corresponding payment record payment method must not be CASH
                    o.order_type == "Online" &&
                    (o.order_status == "Accepted" ||
                    o.order_status == "Preparation" ||
                    o.order_status == "Ready") &&
                    o.tbl_payment.FirstOrDefault().payment_method != "Cash"
                )
                .Count();

            int accountCount = db.tbl_users
                .Where(u => 
                    u.usertype == 2 ||
                    u.usertype == 3
                )
                .Count();

            ManagerDashboardDataModel managerDashboardDataModel = new ManagerDashboardDataModel
            {
                PendingReservationCount = pendingreservationCount,
                AcceptedReservationCount = acceptedreservationCount,
                PendingTakeOutOrdersCount = pendingtakeOutOrdersCount,
                AcceptedTakeOutOrdersCount = acceptedtakeOutOrdersCount,
                AccountCount = accountCount
            };

            ViewBag.CurrentPage = "manager__dashboard";
            return View(managerDashboardDataModel);
        }
    }
}