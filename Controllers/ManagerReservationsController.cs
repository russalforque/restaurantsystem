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
    public class ManagerReservationsController : Controller
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
        public ActionResult LoadManagerReservation()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            List<tbl_orders> reservationOrders = db.tbl_orders
                .Where(o =>
                    o.tbl_reservations.Any() &&
                    o.tbl_payment.Any()
                )
                .OrderBy(o => o.date_ordered)
                .ToList();

            ViewBag.CurrentPage = "manager__reservations";
            return View(reservationOrders);
        }

        [HttpGet]
        public ActionResult ViewReservationDetails(int reservation_id)
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            tbl_reservations reservation = db.tbl_reservations.Find(reservation_id);

            if(reservation == null)
            {
                return HttpNotFound();
            }

            tbl_orders order = db.tbl_orders
                .Where(o => o.order_id == reservation.order_id)
                .FirstOrDefault();

            if (order == null)
            {
                return HttpNotFound();
            }

            List<tbl_order_items> orderItems = db.tbl_order_items
                .Where(oi => oi.order_id == order.order_id)
                .ToList();

            PlaceOrderModel placeOrderModel = new PlaceOrderModel
            {
                Order = order,
                OrderItems = orderItems,
                Reservation = reservation
            };

            ViewBag.CurrentPage = "manager__reservations";
            return View(placeOrderModel);
        }

        [HttpPost]
        public ActionResult ChangeReservationStatus(int reservation_id, string reservation_status)
        {
            tbl_reservations reservation = db.tbl_reservations.Find(reservation_id);

            if (reservation == null)
            {
                return HttpNotFound();
            }

            tbl_orders order = db.tbl_orders
                .Where(o => o.order_id == reservation.order_id)
                .FirstOrDefault();

            tbl_payment payment = db.tbl_payment
                .Where(p => p.order_id == order.order_id)
                .FirstOrDefault();

            string orderStatus = "";

            switch (reservation_status)
            {
                case "Accepted":
                    orderStatus = "Accepted";
                    break;
                case "Declined":
                    orderStatus = "Declined";
                    break;
                default:
                    break;
            }

            order.order_status = orderStatus;
            payment.payment_status = orderStatus;
            reservation.reservation_status = reservation_status;
            db.SaveChanges();

            TempData["statusChangeMessage"] = "success";
            return RedirectToAction("ViewReservationDetails", "ManagerReservations", new { reservation_id });
        }

        [HttpPost]
        public ActionResult CancelReservation(int reservation_id)
        {
            try
            {
                tbl_reservations reservation = db.tbl_reservations.Find(reservation_id);

                if(reservation == null)
                {
                    return HttpNotFound();
                }

                tbl_orders order = db.tbl_orders
                    .Where(o => o.order_id == reservation.order_id)
                    .SingleOrDefault();

                tbl_payment payment = db.tbl_payment
                    .Where(p => p.order_id == order.order_id)
                    .SingleOrDefault();

                order.order_status = "Cancelled";
                payment.payment_status = "Cancelled";
                reservation.reservation_status = "Cancelled";

                db.SaveChanges();

                return Json(new { success = true, message = "Reservation successfully cancelled." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Internal Server Error" });
                throw;
            }
        }
    }
}