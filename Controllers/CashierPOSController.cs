using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Uling_RestaurantManagementSystem.Models.SQL;
using Uling_RestaurantManagementSystem.Models.Custom;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class CashierPOSController : Controller
    {
        private readonly db_urmsEntities db = new db_urmsEntities();

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
        public ActionResult LoadCashierPOS()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            IEnumerable<tbl_menu_items> menuItems = db.tbl_menu_items.ToList();
            IEnumerable<tbl_discounts> discounts = db.tbl_discounts.Where(d => d.is_active == 1).ToList();

            CashierPOSViewModel cashierPOSViewModel = new CashierPOSViewModel
            {
                MenuItems = menuItems,
                Discounts = discounts
            };

            ViewBag.CurrentPage = "cashier__pos";
            return View(cashierPOSViewModel);
        }

        [HttpPost]
        public JsonResult AddToCart(int menu_item_id, int quantity)
        {
            var existingOrderItems = Session["OrderItems"] as List<OrderItemModel> ?? new List<OrderItemModel>();
            tbl_menu_items menuItem = db.tbl_menu_items.Find(menu_item_id);

            if (menuItem == null)
            {
                return Json(new { success = false, message = "Menu item not found" });
            }

            int nextOrderItemId = 0;
            if (existingOrderItems.Count() > 0) {
                List<int> orderItemIdArr = new List<int>();
                foreach (var item in existingOrderItems)
                {
                    orderItemIdArr.Add(item.OrderItemId);
                }
                var maxOrderItem = orderItemIdArr.OrderByDescending(oi => oi).First();
                nextOrderItemId = Convert.ToInt32(maxOrderItem) + 1;
            }
            else
            {
                nextOrderItemId = 1;
            }

            OrderItemModel orderItemModel = new OrderItemModel
            {
                OrderItemId = nextOrderItemId,
                OrderId = 0,
                MenuItemId = menu_item_id,
                MenuName = menuItem.name,
                Unit_Price = menuItem.price,
                Quantity = quantity,
                DiscountId = 0,
                LineTotal = Convert.ToDecimal(menuItem.price) * quantity
            };

            existingOrderItems.Add(orderItemModel);

            Session["OrderItems"] = existingOrderItems;
            return Json(new { success = true, orderItemModel });
        }

        [HttpPost]
        public ActionResult ApplyDiscount(int discount_id, int order_item_id)
        {
            var existingOrderItems = Session["OrderItems"] as List<OrderItemModel> ?? new List<OrderItemModel>();
            foreach (var item in existingOrderItems)
            {
                if (item.OrderItemId == order_item_id)
                {
                    item.DiscountId = discount_id;
                }
            }

            Session["OrderItems"] = existingOrderItems;
            return Json(new { success = true, message = "Discount applied successfully" });
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

        [HttpPost]
        public ActionResult PlaceOrder(string customer_name = null, string contact_number = null, string email = null)
        {
            var existingOrderItems = Session["OrderItems"] as List<OrderItemModel> ?? new List<OrderItemModel>();

            if (existingOrderItems == null || existingOrderItems.Count == 0)
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
                order_type = "Counter",
                customer_name = customerName,
                contact_number = contactNumber,
                email = emailAddress,
                date_ordered = today,
                order_status = "Accepted"
            };

            db.tbl_orders.Add(newOrder);

            decimal orderTotalAmount = 0;

            // re-populate the order items using the main context (tbl_order_items) / model
            List<tbl_order_items> orderItems = new List<tbl_order_items>();

            // Create a list for receipt items
            List<ReceiptItemModel> receiptItemArray = new List<ReceiptItemModel>();

            foreach (var item in existingOrderItems)
            {
                tbl_discounts discount = db.tbl_discounts.Where(d => d.discount_id == item.DiscountId).FirstOrDefault();
                decimal initialLinePrice = item.LineTotal;
                decimal finalLinePrice = 0;

                tbl_order_items newOrderItem = new tbl_order_items
                {
                    order_id = newOrder.order_id,
                    menu_item_id = item.MenuItemId,
                    quantity = item.Quantity
                };

                if (discount != null && item.DiscountId != 0)
                {
                    // check for discounts
                    if (discount.discount_percentage != 0 && discount.discount_amount == 0)
                    {
                        // has discount percentage off
                        decimal discountDecimalForm = discount.discount_percentage / 100;
                        decimal discountOff = item.Unit_Price * discountDecimalForm;
                        finalLinePrice = initialLinePrice - discountOff;
                    }
                    else if (discount.discount_percentage == 0 && discount.discount_amount != 0)
                    {
                        // has discount fix amount off
                        finalLinePrice = initialLinePrice - discount.discount_amount;
                    }

                    newOrderItem.discount_id = item.DiscountId;
                }
                else {
                    // no discount
                    finalLinePrice = initialLinePrice;
                }

                newOrderItem.line_price = finalLinePrice;
                
                orderItems.Add(newOrderItem);
                orderTotalAmount += finalLinePrice;

                // insert the order item to the receipt items array
                ReceiptItemModel receiptItemModel = new ReceiptItemModel
                {
                    Item = item.MenuName,
                    Quantity = item.Quantity,
                    Unit_Price = item.Unit_Price,
                    Total_Price = finalLinePrice
                };

                receiptItemArray.Add(receiptItemModel);
            }

            // loop over the order items (orderItems) and save each item to db
            orderItems.ForEach(oi => db.tbl_order_items.Add(oi));

            // update the order total_amount
            newOrder.total_amount = orderTotalAmount;

            int userId = Convert.ToInt32(Session["user_id"] ?? 0);

            // save receipt record
            tbl_receipts receipt = new tbl_receipts
            {
                order_id = newOrder.order_id,
                receipt_date = DateTime.Now,
                sub_total = newOrder.total_amount,
                amount_due = newOrder.total_amount,
                cashier_id = userId
            };

            db.tbl_receipts.Add(receipt);

            db.SaveChanges();

            Session.Remove("OrderItems");

            tbl_users cashier = db.tbl_users.Where(u => u.user_id == userId).FirstOrDefault();

            if (cashier == null) {
                return HttpNotFound();
            }

            return Json(new { 
                success = true, 
                message = "Order submitted successfully.",
                receipt = new
                {
                    receipt.receipt_id,
                    receipt.order_id,
                    receipt.receipt_date,
                    receipt.sub_total,
                    receipt.amount_due,
                    cashier = cashier.firstname,
                    receipt.tbl_orders.customer_name,
                    receipt_items = receiptItemArray
                }
            });
        }
    }
}