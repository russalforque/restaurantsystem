using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
// imports
using Uling_RestaurantManagementSystem.Models.SQL;
using Uling_RestaurantManagementSystem.Models.Custom;
using System.Data.Entity;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class CookManageOrdersController : Controller
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
        public ActionResult LoadCookManageOrders()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            DateTime today = DateTime.Today;

            // Define the start and end of the day
            DateTime startOfDay = today.Date;
            DateTime endOfDay = today.Date.AddDays(1).AddTicks(-1);

            List<tbl_orders> orders = db.tbl_orders
                .Where(o => 
                    o.order_status == "Accepted" || 
                    o.order_status == "Preparation" ||
                    o.order_status == "Ready"
                )
                .ToList();

            List<PlaceOrderModel> placeOrderModels = new List<PlaceOrderModel>();

            foreach (var order in orders)
            {
                List<tbl_order_items> orderItems = db.tbl_order_items
                    .Where(io => io.order_id == order.order_id)
                    .ToList();

                tbl_reservations reservation = db.tbl_reservations
                .Where(r => r.order_id == order.order_id)
                .FirstOrDefault();

                PlaceOrderModel placeOrderModel = new PlaceOrderModel
                {
                    Order = order,
                    OrderItems = orderItems,
                    Reservation = reservation
                };

                // filter the orders with reservation to only show if the reservation date is today for the cook to prepare
                // it means, the cook will only prepare the orders of reservation for today's date        
                if (reservation != null && reservation.reservation_date == today.Date)
                {
                    placeOrderModels.Add(placeOrderModel);
                }
            }

            ViewBag.CurrentPage = "cook__manage_orders";
            return View(placeOrderModels);
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

            return RedirectToAction("LoadCookManageOrders", "CookManageOrders");
        }
    }
}