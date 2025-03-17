using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
// imports
using Uling_RestaurantManagementSystem.Models.SQL;
using Uling_RestaurantManagementSystem.Models.Custom;
using System;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class CashierOrdersController : Controller
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
        public ActionResult LoadCashierOrders()
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
                    o.order_type == "Counter" && 
                    o.order_status != "Cancelled" &&
                    o.order_status != "Declined"
                )
                .ToList();

            ViewBag.CurrentPage = "cashier__orders";
            return View(orders);
        }

        [HttpGet]
        public ActionResult ViewOrderDetails(int order_id)
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            tbl_orders order = db.tbl_orders.Find(order_id);

            if(order == null)
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
            

            ViewBag.CurrentPage = "cashier__orders";
            return View(placeOrderModel);
        }

        [HttpPost]
        public ActionResult ChangeOrderStatus(int order_id, string order_status)
        {
            tbl_orders orders = db.tbl_orders.Find(order_id);

            if(orders == null)
            {
                return HttpNotFound();
            }

            orders.order_status = order_status;
            db.SaveChanges();

            TempData["statusChangeMessage"] = "Order status changed to " + order_status;
            return RedirectToAction("ViewOrderDetails", "CashierOrders", new { order_id = order_id });
        }

        [HttpPost]
        public ActionResult ServeOrder(int orderId)
        {
            var order = db.tbl_orders.FirstOrDefault(o => o.order_id == orderId);
            if (order != null)
            {
                order.order_status = "Served";  // Update the status
                db.SaveChanges();  // Save the changes to the database
            }

            return RedirectToAction("LoadCashierOrders", "CashierOrders");
        }
    }
}