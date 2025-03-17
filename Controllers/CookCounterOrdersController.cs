using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Uling_RestaurantManagementSystem.Models.SQL;
using Uling_RestaurantManagementSystem.Models.Custom;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class CookCounterOrdersController : Controller
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
        public ActionResult LoadCookCounterOrders()
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

            List<CounterOrderModel> counterOrders = new List<CounterOrderModel>();

            foreach (var order in orders)
            {
                List<tbl_order_items> orderItems = db.tbl_order_items
                    .Where(oi =>
                        oi.order_id == order.order_id
                    )
                    .ToList();

                counterOrders.Add(new CounterOrderModel
                {
                    Order = order,
                    OrderItems = orderItems
                });
            }

            ViewBag.CurrentPage = "cook__counter_orders";
            return View(counterOrders);
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

            return RedirectToAction("LoadCookCounterOrders", "CookCounterOrders");
        }
    }
}