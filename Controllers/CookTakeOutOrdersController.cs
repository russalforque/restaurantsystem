using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Uling_RestaurantManagementSystem.Models.SQL;
using Uling_RestaurantManagementSystem.Models.Custom;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class CookTakeOutOrdersController : Controller
    {
        private readonly db_urmsEntities db = new db_urmsEntities();

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
        public ActionResult LoadCookTakeOutOrders()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            List<tbl_orders> orders = db.tbl_orders
                .Where(o =>
                    o.tbl_payment.Any() &&
                    // Fetch only orders that doesn't have any corresponding reservation orders
                    !o.tbl_reservations.Any() &&
                    // status (only fetch the take-out orders that is already accepted and paid)
                    (o.order_status != "Pending" && o.order_status != "Declined" && o.order_status != "Cancelled") &&
                    o.tbl_payment.FirstOrDefault().payment_status == "Accepted" &&
                    // Fetch only online orders & corresponding payment record payment method must not be CASH
                    o.order_type == "Online" &&
                    o.tbl_payment.FirstOrDefault().payment_method != "Cash"
                )
                .ToList();

            List<TakeOutOrdersDataModel> takeOutOrders = new List<TakeOutOrdersDataModel>();

            foreach (var order in orders)
            {
                TakeOutOrdersDataModel takeOutOrder = new TakeOutOrdersDataModel();

                List<tbl_order_items> orderItems = db.tbl_order_items
                    .Where(oi =>
                        oi.order_id == order.order_id
                    )
                    .ToList();

                takeOutOrder.Order = order;
                takeOutOrder.OrderItems = orderItems;
                takeOutOrder.Payment = order.tbl_payment.FirstOrDefault();

                takeOutOrders.Add(takeOutOrder);
            }

            ViewBag.CurrentPage = "cook__takeout_orders";
            return View(takeOutOrders);
        }

        [HttpPost]
        public ActionResult ChangeOrderStatus(int order_id, string order_status)
        {
            tbl_orders orders = db.tbl_orders
                .Where(o => o.order_id == order_id)
                .FirstOrDefault();

            if (orders == null)
            {
                return HttpNotFound();
            }

            orders.order_status = order_status;
            db.SaveChanges();

            return RedirectToAction("LoadCookTakeOutOrders", "CookTakeOutOrders");
        }
    }
}