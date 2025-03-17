using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Uling_RestaurantManagementSystem.Models.SQL;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class ManagerMenuItemsController : Controller
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

        public ActionResult LoadManagerMenuItems()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            List<tbl_menu_items> menuItems = db.tbl_menu_items.ToList();

            ViewBag.CurrentPage = "manager__menu_items";
            return View(menuItems);
        }

        [HttpPost]
        public ActionResult UpdateMenuItemStatus(int menu_item_id, int is_available)
        {
            tbl_menu_items menuItem = db.tbl_menu_items.Find(menu_item_id);

            bool isAvailable = false;
            if(is_available == 1)
            {
                isAvailable = true;
            }

            menuItem.is_available = isAvailable;

            db.SaveChanges();

            return Json(new { success = true, message = "Menu item status has been changed successfully." });
        }
    }
}