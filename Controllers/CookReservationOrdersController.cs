using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Uling_RestaurantManagementSystem.Models.SQL;
using Uling_RestaurantManagementSystem.Models.Custom;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class CookReservationOrdersController : Controller
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
        public ActionResult LoadCookReservationOrders()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            List<tbl_orders> orders = db.tbl_orders
                .Where(o =>
                    // get orders that have reservation and payment corresponding record
                    o.tbl_reservations.Any() &&
                    o.tbl_payment.Any() &&
                    // status (only fetch the reservations that are already verified and paid)
                    (o.order_status != "Pending" && o.order_status != "Declined" && o.order_status != "Cancelled") &&
                    // Include "Accepted", "Preparation", and "Ready" statuses for reservations
                    (o.tbl_reservations.FirstOrDefault().reservation_status == "Accepted" ||
                    o.tbl_reservations.FirstOrDefault().reservation_status == "Preparation" ||
                    o.tbl_reservations.FirstOrDefault().reservation_status == "Ready") &&
                    o.tbl_payment.FirstOrDefault().payment_status == "Accepted" &&
                    // Fetch only online orders & corresponding payment record payment method must not be CASH
                    o.order_type == "Online" &&
                    o.tbl_payment.FirstOrDefault().payment_method != "Cash"
                )
                .ToList();

            List<ReservationOrderModel> reservationOrders = new List<ReservationOrderModel>();

            foreach (var order in orders)
            {
                ReservationOrderModel reservationOrderModel = new ReservationOrderModel();

                List<tbl_order_items> orderItems = db.tbl_order_items
                    .Where(oi =>
                        oi.order_id == order.order_id
                    )
                    .ToList();

                reservationOrderModel.Reservation = order.tbl_reservations.FirstOrDefault();
                reservationOrderModel.Order = order;
                reservationOrderModel.OrderItems = orderItems;
                reservationOrderModel.Payment = order.tbl_payment.FirstOrDefault();

                reservationOrders.Add(reservationOrderModel);
            }

            ViewBag.CurrentPage = "cook__reservation_orders";
            return View(reservationOrders);
        }

        //[HttpPost]
        //public ActionResult ChangeOrderStatus(int order_id, string order_status)
        //{
        //    tbl_orders orders = db.tbl_orders
        //        .Where(o => o.order_id == order_id)
        //        .FirstOrDefault();

        //    if (orders == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    orders.order_status = order_status;
        //    db.SaveChanges();

        //    return RedirectToAction("LoadCookReservationOrders", "CookReservationOrders");

        //}

        [HttpPost]
        public ActionResult ChangeOrderStatus(int order_id, string order_status)
        {
            // Fetch the order record by ID
            tbl_orders order = db.tbl_orders
                .Where(o => o.order_id == order_id)
                .FirstOrDefault();

            if (order == null)
            {
                return HttpNotFound();
            }

            // Update the order status
            order.order_status = order_status;

            // Fetch the related reservation record
            tbl_reservations reservation = order.tbl_reservations.FirstOrDefault();

            if (reservation != null)
            {
                reservation.reservation_status = order_status;
            }

            // Save changes to the database
            db.SaveChanges();

            return RedirectToAction("LoadCookReservationOrders", "CookReservationOrders");
        }
    }
}