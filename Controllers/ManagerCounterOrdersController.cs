using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uling_RestaurantManagementSystem.Models.Custom;
using Uling_RestaurantManagementSystem.Models.SQL;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class ManagerCounterOrdersController : Controller
    {
        private readonly db_urmsEntities db = new db_urmsEntities();

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

        public ActionResult LoadManagerCounterOrders()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            List<tbl_orders> orders = db.tbl_orders
                .Where(o =>
                    // fetch orders that doesn't have any corresponding payment and reservation record
                    !o.tbl_payment.Any() &&
                    !o.tbl_reservations.Any() &&
                    // get counter order type orders only
                    o.order_type == "Counter"
                )
                .ToList();

            ViewBag.CurrentPage = "manager__counter_orders";
            return View(orders);
        }

        public ActionResult ViewOrderDetails(int order_id)
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            tbl_orders order = db.tbl_orders.Find(order_id);

            if (order == null)
            {
                return HttpNotFound();
            }

            List<tbl_order_items> orderItems = db.tbl_order_items
                .Where(oi => oi.order_id == order_id)
                .ToList();

            PlaceOrderModel placeOrderModel = new PlaceOrderModel
            {
                Order = order,
                OrderItems = orderItems
            };


            ViewBag.CurrentPage = "manager__counter_orders";
            return View(placeOrderModel);
        }

        [HttpPost]
        public ActionResult CancelCounterOrder(int order_id)
        {
            tbl_orders order = db.tbl_orders.Find(order_id);

            if(order == null)
            {
                return HttpNotFound();
            }

            order.order_status = "Cancelled";
            db.SaveChanges();

            return Json(new { success = true, message = "Order cancelled successfully." });
        }
    }
}