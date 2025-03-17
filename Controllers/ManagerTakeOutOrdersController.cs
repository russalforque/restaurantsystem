using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Uling_RestaurantManagementSystem.Models.SQL;
using Uling_RestaurantManagementSystem.Models.Custom;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class ManagerTakeOutOrdersController : Controller
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
        public ActionResult LoadTakeOutOrders()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            List<TakeOutOrdersDataModel> takeOutOrders = new List<TakeOutOrdersDataModel>();

            
            var ordersWithPayments = db.tbl_orders
                .Where(order =>
                    // Fetch orders that have at least one payment record and does not have any corresponding reservation record
                    order.tbl_payment.Any() &&
                    !order.tbl_reservations.Any() &&
                    // Fetch only online orders & corresponding payment record payment method must not be CASH
                    order.order_type == "Online" &&
                    order.tbl_payment.FirstOrDefault().payment_method != "Cash"
                )
                .OrderBy(order => order.date_ordered)
                .ToList();

            foreach (var order in ordersWithPayments)
            {
                var orderItems = db.tbl_order_items
                    .Where(oi => oi.order_id == order.order_id)
                    .ToList();

                TakeOutOrdersDataModel takeOutOrdersDataModel = new TakeOutOrdersDataModel
                {
                    Order = order,
                    OrderItems = orderItems
                };

                takeOutOrders.Add(takeOutOrdersDataModel);
            }

            ViewBag.CurrentPage = "manager__takeout_orders";
            return View(takeOutOrders);
        }

        [HttpGet]
        public ActionResult ViewTakeOutOrderDetails(int order_id)
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

            tbl_payment payment = db.tbl_payment
                .Where(p => p.order_id == order_id)
                .FirstOrDefault();

            TakeOutOrdersDataModel takeOutOrdersDataModel = new TakeOutOrdersDataModel
            {
                Order = order,
                OrderItems = orderItems,
                Payment = payment
            };

            ViewBag.CurrentPage = "manager__takeout_orders";
            return View(takeOutOrdersDataModel);
        }

        [HttpPost]
        public ActionResult UpdateTakeOutOrderStatus(int order_id, string order_status)
        {
            tbl_orders order = db.tbl_orders.Find(order_id);

            if(order == null)
            {
                return HttpNotFound();
            }

            string payment_status = "";

            switch (order_status)
            {
                case "Accepted":
                    payment_status = "Accepted";
                    break;
                case "Declined":
                    payment_status = "Declined";
                    break;
                default:
                    break;
            }

            order.order_status = order_status;

            tbl_payment payment = db.tbl_payment
                .Where(p => p.order_id == order.order_id)
                .FirstOrDefault();

            payment.payment_status = payment_status;

            db.SaveChanges();

            TempData["statusChangeMessage"] = "success";
            return RedirectToAction("ViewTakeOutOrderDetails", "ManagerTakeOutOrders", new { order_id });
        }

        [HttpPost]
        public ActionResult CancelTakeOutOrder(int order_id)
        {
            try
            {
                tbl_orders order = db.tbl_orders.Find(order_id);

                if (order == null)
                {
                    return HttpNotFound();
                }

                tbl_payment payment = db.tbl_payment
                    .Where(p => p.order_id == order_id)
                    .SingleOrDefault();

                order.order_status = "Cancelled";
                payment.payment_status = "Cancelled";

                db.SaveChanges();

                return Json(new { success = true, message = "Order successfully cancelled." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Internal Server Error" });
                throw;
            }
        }
    }
}