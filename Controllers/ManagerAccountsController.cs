using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
// imports
using Uling_RestaurantManagementSystem.Models.SQL;

namespace Uling_RestaurantManagementSystem.Controllers
{
    public class ManagerAccountsController : Controller
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
        public ActionResult LoadManagerAccounts()
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            List<tbl_users> users = db.tbl_users
                .Where(u =>
                    u.usertype == 2 ||
                    u.usertype == 3
                )
                .ToList();

            ViewBag.CurrentPage = "manager__accounts";
            return View(users);
        }

        [HttpPost]
        public ActionResult AddAccount(string firstname, string middlename, string lastname, int usertype, string email, string password)
        {
            tbl_users user = new tbl_users
            {
                firstname = firstname,
                middlename = middlename,
                lastname = lastname,
                usertype = usertype,
                email = email,
                password = password
            };

            db.tbl_users.Add(user);
            db.SaveChanges();

            return Json(new { success = true, message = "New account has ben added successfully" });
        }

        [HttpGet]
        public ActionResult ViewAccountDetails(int user_id)
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            tbl_users user = db.tbl_users.Find(user_id);

            ViewBag.CurrentPage = "manager__accounts";
            return View(user);
        }

        [HttpGet]
        public ActionResult LoadEditAccount(int user_id)
        {
            // Check if the user is authorized.
            var userType = Convert.ToInt32(Session["user_type"]);
            if (!IsUserAuthorized(userType)) { return NotAuthorized(userType); }

            tbl_users user = db.tbl_users.Find(user_id);

            ViewBag.CurrentPage = "manager__accounts";
            return View(user);
        }

        [HttpPost]
        public ActionResult UpdateAccount(int user_id, string firstname, string middlename, string lastname, int usertype, string email, string password) 
        {
            tbl_users user = db.tbl_users.Find(user_id);
            
            if(user == null)
            {
                return HttpNotFound();
            }

            user.firstname = firstname;
            user.middlename = middlename;
            user.lastname = lastname;
            user.usertype = usertype;
            user.email = email;
            user.password = password;
            db.SaveChanges();

            return Json(new { success = true, message = "Account details has been updated successfully" });
        }

        [HttpPost]
        public ActionResult DeleteAccount(int user_id)
        {
            tbl_users user = db.tbl_users
                .Where(u => u.user_id == user_id)
                .FirstOrDefault();

            if (user == null)
            {
                return HttpNotFound();
            }

            db.tbl_users.Remove(user);
            db.SaveChanges();

            return Json(new { success = true, message = "Account has been deleted successfully" });
        }
    }
}