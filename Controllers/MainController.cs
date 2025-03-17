using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
// imports
using Uling_RestaurantManagementSystem.Models.SQL; // Required for Custom models/classes
using Uling_RestaurantManagementSystem.Models.Custom; // Required for Custom models/classes


namespace Uling_RestaurantManagementSystem.Controllers
{
    public class MainController : Controller
    {
        private db_urmsEntities db = new db_urmsEntities();

        private bool IsUserLoggedIn(int userType)
        {
            if (userType == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private ActionResult AlreadyLoggedIn(int userType)
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
                case 3:
                    actionName = "LoadCookDashboard";
                    controllerName = "CookDashboard";
                    break;
                default:
                    break;
            }
            return RedirectToAction(actionName, controllerName);
        }

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.CurrentPage = "public__home";
            return View();
        }

        [HttpGet]
        public ActionResult SignIn()
        {
            // Check if the user is already logged in.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (IsUserLoggedIn(userType)) { return AlreadyLoggedIn(userType); }

            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(SignInModel credentials)
        {
            // validate model data from the view
            if(!ModelState.IsValid)
            {
                // Return the model with the data and display validation errors.
                return View(credentials);
            }

            var user = db.tbl_users
                .Where(
                    x => x.email.Equals(credentials.Email) && 
                    x.password.Equals(credentials.Password)
                ).SingleOrDefault();

            if (user != null)
            {
                // store user details to session
                Session["user_id"] = user.user_id.ToString();
                Session["user_type"] = user.usertype.ToString();
                Session["email"] = user.email.ToString();
                Session["firstname"] = user.firstname.ToString();
                Session["lastname"] = user.lastname.ToString();

                string actionName = "";
                string controllerName = "";

                /*
                    User Types
                
                    1 ---> Manager
                    2 ---> Cashier
                    3 ---> Cook
                */

                switch (user.usertype)
                {
                    case 1:
                        actionName = "LoadManagerDashboard";
                        controllerName = "ManagerDashboard";
                        break;
                    case 2:
                        actionName = "LoadCashierDashboard";
                        controllerName = "CashierDashboard";
                        break;
                    case 3:
                        actionName = "LoadCookDashboard";
                        controllerName = "CookDashboard";
                        break;
                    default:
                        break;
                }

                return RedirectToAction(actionName, controllerName);
            }
            else
            {
                var viewBagMessage = new ViewBagMessageModel
                {
                    Key = "invalid_credentials",
                    Message = "Invalid email or password"
                };

                ViewBag.ViewBagMessage = viewBagMessage;
                return View(credentials);
            }
        }

        [HttpPost]
        public ActionResult SignOut()
        {
            // Clear all session data
            Session.Clear();

            // Optionally, you can abandon the session
            Session.Abandon();

            // Redirect to the signin page or home page
            return RedirectToAction("SignIn", "Main");
        }

        public ActionResult Menu()
        {
            // Fetch all active menu items, grouping them by category
            var menuItems = db.tbl_menu_items
                              .Where(item => item.is_available == true) // Assuming 'is_active' is a column in your table
                              .ToList();

            // Manually order categories: Main Course, Desserts, then Drinks
            var orderedMenuItems = new List<IGrouping<string, tbl_menu_items>>();

            orderedMenuItems.AddRange(menuItems.Where(item => item.category == "Main Course")
                                                .GroupBy(item => item.category));
            orderedMenuItems.AddRange(menuItems.Where(item => item.category == "Desserts") // Changed to match the frontend
                                                .GroupBy(item => item.category));
            orderedMenuItems.AddRange(menuItems.Where(item => item.category == "Drinks") // Changed to plural if necessary
                                                .GroupBy(item => item.category));

            // Add any other categories if they exist
            orderedMenuItems.AddRange(menuItems.Where(item => item.category != "Main Course"
                                                            && item.category != "Desserts"
                                                            && item.category != "Drinks")
                                                .GroupBy(item => item.category));

            // Return the data to the view
            ViewBag.CurrentPage = "public__menu";
            return View(orderedMenuItems);
        }

        [HttpGet]
        public ActionResult Order()
        {
            IEnumerable<tbl_menu_items> menuItems = db.tbl_menu_items.ToList();

            ViewBag.CurrentPage = "public__order";
            return View(menuItems);
        }

        [HttpPost]
        public JsonResult AddToCart(int menu_item_id, int quantity)
        {
            var existingOrderItems = Session["OrderItems"] as List<OrderItemModel> ?? new List<OrderItemModel>();
            tbl_menu_items menuItem = db.tbl_menu_items.Find(menu_item_id);

            if(menuItem == null)
            {
                return Json(new { success = false, message = "Menu item not found" });
            }

            OrderItemModel orderItemModel = new OrderItemModel
            {
                OrderItemId = 0,
                OrderId = 0,
                MenuItemId = menu_item_id,
                MenuName = menuItem.name,
                Quantity = quantity,
                LineTotal = Convert.ToDecimal(menuItem.price) * quantity
            };

            existingOrderItems.Add(orderItemModel);

            Session["OrderItems"] = existingOrderItems;
            return Json(new { success = true, orderItemModel });
        }

        [HttpPost]
        public JsonResult RemoveToCart(int menu_item_id)
        {
            var existingOrderItems = Session["OrderItems"] as List<OrderItemModel> ?? new List<OrderItemModel>();

            // Collect items to remove
            var itemsToRemove = new List<OrderItemModel>();

            foreach (var item in existingOrderItems)
            {
                if (item.MenuItemId == menu_item_id)
                {
                    itemsToRemove.Add(item);
                }
            }

            // Remove collected items
            foreach (var item in itemsToRemove)
            {
                existingOrderItems.Remove(item);
            }

            Session["OrderItems"] = existingOrderItems;
            return Json(new { success = true });
        }

        [HttpGet]
        public ActionResult LoadPlaceOrder(int order_id)
        {
            tbl_orders order = db.tbl_orders.Find(order_id);
            List<tbl_order_items> orderItems = db.tbl_order_items
                .Where(oi => oi.order_id == order_id)
                .ToList();

            if(order == null)
            {
                return HttpNotFound();
            }

            PlaceOrderModel placeOrderModel = new PlaceOrderModel
            {
                Order = order,
                OrderItems = orderItems
            };

            return View(placeOrderModel);
        }

        [HttpPost]
        public ActionResult PlaceOrder(string order_type, string customer_name = null, string contact_number = null, string email = null)
        {
            var existingOrderItems = Session["OrderItems"] as List<OrderItemModel> ?? new List<OrderItemModel>();

            if(existingOrderItems == null || existingOrderItems.Count == 0)
            {
                return HttpNotFound();
            }

            // check request parameters
            string customerName = !string.IsNullOrEmpty(customer_name) ? customer_name : "-";
            string contactNumber = !string.IsNullOrEmpty(contact_number) ? contact_number : "-";
            string emailAddress = !string.IsNullOrEmpty(email) ? email : "-";

            DateTime today = DateTime.Now;
            tbl_orders newOrder = new tbl_orders
            {
                order_type = "Online",
                customer_name = customerName,
                contact_number = contactNumber,
                email = emailAddress,
                date_ordered = today,
                order_status = "Pending"
            };

            db.tbl_orders.Add(newOrder);

            decimal orderTotalAmount = 0;

            // re-populate the order items using the main context (tbl_order_items) / model
            List<tbl_order_items> orderItems = new List<tbl_order_items>();
            foreach (var item in existingOrderItems)
            {
                tbl_order_items newOrderItem = new tbl_order_items
                {
                    order_id = newOrder.order_id,
                    menu_item_id = item.MenuItemId,
                    quantity = item.Quantity,
                    line_price = item.LineTotal
                };

                orderItems.Add(newOrderItem);
                orderTotalAmount += item.LineTotal;
            }

            // loop over the order items (orderItems) and save each item to db
            orderItems.ForEach(oi => db.tbl_order_items.Add(oi));

            // update the order total_amount
            newOrder.total_amount = orderTotalAmount;

            db.SaveChanges();

            Session.Remove("OrderItems");

            if (order_type == "takeout")
            {
                //TempData["PlaceOrderMessage"] = "orderSuccessful";
                //return RedirectToAction("Menu", "Main");
                return RedirectToAction("LoadUploadPayment", "Main", new { order_id = newOrder.order_id });
            }
            else if (order_type == "reservationtakeout")
            {
                return RedirectToAction("LoadPlaceOrderTakeOut", "Main", new { order_id = newOrder.order_id });
            }
            else
            {
                return RedirectToAction("LoadPlaceOrder", "Main", new { order_id = newOrder.order_id });
            }
        }

        [HttpPost]
        public ActionResult SubmitReservation(int order_id, string customer_name, string email, string contact_number, DateTime reservation_date, TimeSpan time_start, TimeSpan time_end, int heads, string note)
        {
            // Check if the reservation date and time are available
            //bool isAvailable = !db.tbl_reservations.Any(r =>
            //    r.order_id != order_id &&
            //    r.reservation_date == reservation_date &&
            //    ((r.time_start < time_end && r.time_end > time_start) ||  // Overlaps
            //     (r.time_start == time_start && r.time_end == time_end))); // Exact match

            //if (!isAvailable)
            //{
            //    return Json(new { success = false, message = "The selected date and time are not available for reservation." });
            //}

            // update the order record particularly the customer detail fields
            tbl_orders order = db.tbl_orders.Find(order_id);
            order.customer_name = customer_name;
            order.contact_number = contact_number;
            order.email = email;

            tbl_reservations newReservation = new tbl_reservations
            {
                order_id = order_id,
                reservation_date = reservation_date,
                time_start = time_start,
                time_end = time_end,
                heads = heads,
                note = note,
                reservation_status = "Pending"
            };

            db.tbl_reservations.Add(newReservation);
            db.SaveChanges();

            return Json(new { success = true, message = "Reservation submitted successfully. Will keep in touch with you soon via email or phone." });
        }

        [HttpGet]
        public ActionResult LoadUploadPayment(int order_id)
        {
            tbl_orders order = db.tbl_orders.Find(order_id);

            if (order == null)
            {
                return HttpNotFound();
            }

            List<tbl_order_items> orderItems = db.tbl_order_items
                .Where(oi => oi.order_id == order_id)
                .ToList();

            tbl_reservations reservation = db.tbl_reservations
                .Where(r => r.order_id == order_id)
                .SingleOrDefault();

            PlaceOrderModel placeOrderModel = new PlaceOrderModel
            {
                Order = order,
                OrderItems = orderItems,
                Reservation = reservation
            };

            return View(placeOrderModel);
        }

        [HttpPost]
        public JsonResult UploadPayment(int order_id, decimal amount, string payment_url)
        {
            try
            {
                // Create the initial payment record for the current order
                tbl_payment payment = new tbl_payment
                {
                    payment_method = "GCash",
                    order_id = order_id,
                    amount = amount,
                    payment_url = payment_url,
                    payment_status = "Pending"
                };

                db.tbl_payment.Add(payment);

                // Retrieve the customer details from the original order
                var originalOrder = db.tbl_orders.FirstOrDefault(o => o.order_id == order_id);

                if (originalOrder != null)
                {
                    string customerName = originalOrder.customer_name;
                    string contactNumber = originalOrder.contact_number;
                    string email = originalOrder.email;

                    // Find other orders associated with this customer that need the same payment_url
                    var otherOrders = db.tbl_orders.Where(o => o.customer_name == customerName && o.contact_number == contactNumber && o.email == email && o.order_id != order_id).ToList();

                    foreach (var otherOrder in otherOrders)
                    {
                        // Create a payment entry for each of the other orders
                        tbl_payment otherPayment = new tbl_payment
                        {
                            payment_method = "GCash",
                            order_id = otherOrder.order_id,
                            amount = amount,
                            payment_url = payment_url,
                            payment_status = "Pending"
                        };

                        db.tbl_payment.Add(otherPayment);
                    }
                }

                // Commit the changes to the database
                db.SaveChanges();

                return Json(new { success = true, message = "payment_success" });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) if necessary
                return Json(new { success = false, message = "Internal Server Error" });
            }
        }

        [HttpGet]
        public ActionResult LoadPlaceOrderTakeOut(int order_id)
        {
            tbl_orders order = db.tbl_orders.Find(order_id);
            List<tbl_order_items> orderItems = db.tbl_order_items
                .Where(oi => oi.order_id == order_id)
                .ToList();

            if (order == null)
            {
                return HttpNotFound();
            }

            PlaceOrderModel placeOrderModel = new PlaceOrderModel
            {
                Order = order,
                OrderItems = orderItems
            };

            return View(placeOrderModel);
        }

        [HttpPost]
        public JsonResult SubmitTakeoutAndReservation(SubmitTakeoutAndReservationModel model)
        {
            try
            {
                // Create Reservation Order
                tbl_orders reservationOrder = new tbl_orders
                {
                    order_type = "Online", // Consistent for filtering
                    customer_name = model.customer_name,
                    contact_number = model.contact_number,
                    email = model.email,
                    date_ordered = DateTime.Now,
                    order_status = "Pending"
                };
                db.tbl_orders.Add(reservationOrder);
                db.SaveChanges();

                if (!string.IsNullOrEmpty(model.reservation_date))
                {
                    tbl_reservations reservation = new tbl_reservations
                    {
                        order_id = reservationOrder.order_id,
                        reservation_date = DateTime.Parse(model.reservation_date),
                        time_start = TimeSpan.Parse(model.time_start),
                        time_end = TimeSpan.Parse(model.time_end),
                        heads = model.heads ?? 1,
                        note = "Reservation created alongside takeout",
                        reservation_status = "Pending"
                    };
                    db.tbl_reservations.Add(reservation);
                }

                // Create Takeout Order
                tbl_orders takeoutOrder = new tbl_orders
                {
                    order_type = "Online", // Required for LoadTakeOutOrders
                    customer_name = model.customer_name,
                    contact_number = model.contact_number,
                    email = model.email,
                    date_ordered = DateTime.Now,
                    order_status = "Pending"
                };
                db.tbl_orders.Add(takeoutOrder);
                db.SaveChanges();

                // Add Order Items for Takeout (selected items)
                List<int> selectedItemIds = model.selectedItems.Select(int.Parse).ToList();
                var selectedItems = db.tbl_order_items
                    .Where(oi => oi.order_id == model.order_id && selectedItemIds.Contains(oi.menu_item_id))
                    .ToList();

                foreach (var item in selectedItems)
                {
                    tbl_order_items takeoutOrderItem = new tbl_order_items
                    {
                        order_id = takeoutOrder.order_id,
                        menu_item_id = item.menu_item_id,
                        quantity = item.quantity,
                        line_price = item.line_price
                    };
                    db.tbl_order_items.Add(takeoutOrderItem);
                }

                // Add Unselected Items to Reservation Order
                var unselectedItems = db.tbl_order_items
                    .Where(oi => oi.order_id == model.order_id && !selectedItemIds.Contains(oi.menu_item_id))
                    .ToList();

                foreach (var item in unselectedItems)
                {
                    tbl_order_items reservationOrderItem = new tbl_order_items
                    {
                        order_id = reservationOrder.order_id,
                        menu_item_id = item.menu_item_id,
                        quantity = item.quantity,
                        line_price = item.line_price
                    };
                    db.tbl_order_items.Add(reservationOrderItem);
                }

                //// Placeholder for Uploaded Payment URL
                //string paymentUrl = model.payment_url; // Ensure the client uploads and passes this correctly.

                //// Add Payment Record for Takeout
                //tbl_payment takeoutPayment = new tbl_payment
                //{
                //    payment_method = "GCash", // Ensure compatibility
                //    order_id = takeoutOrder.order_id,
                //    amount = selectedItems.Sum(si => si.line_price),
                //    payment_url = paymentUrl, // Assign uploaded payment URL
                //    payment_status = "Pending"
                //};
                //db.tbl_payment.Add(takeoutPayment);

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Takeout and reservation orders successfully submitted.",
                    redirectUrl = Url.Action("LoadUploadPayment", "Main", new { order_id = reservationOrder.order_id })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });

            }
        }
    }
}