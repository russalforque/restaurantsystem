using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Uling_RestaurantManagementSystem.Utils.Functions
{
    public class Globals
    {
        public string GetReservaitonBadgeClass(string status)
        {
            string badgeClass = "";

            switch (status)
            {
                case "Pending":
                    badgeClass = "bg-secondary";
                    break;
                case "Accepted":
                    badgeClass = "bg-warning text-dark";
                    break;
                case "Preparation":
                    badgeClass = "bg-warning text-dark";
                    break;
                case "Ready":
                    badgeClass = "bg-success";
                    break;
                case "Completed":
                    badgeClass = "bg-success";
                    break;
                case "Declined":
                    badgeClass = "bg-danger";
                    break;
                case "Cancelled":
                    badgeClass = "bg-secondary";
                    break;
                default:
                    break;
            }

            return badgeClass;
        }

        public string GetTakeOutOrderBadgeClass(string status)
        {
            string badgeClass = "";

            switch (status)
            {
                case "Pending":
                    badgeClass = "bg-secondary";
                    break;
                case "Accepted":
                    badgeClass = "bg-warning text-dark";
                    break;
                case "Preparation":
                    badgeClass = "bg-warning text-dark";
                    break;
                case "Ready":
                    badgeClass = "bg-success";
                    break;
                case "Completed":
                    badgeClass = "bg-success";
                    break;
                case "Declined":
                    badgeClass = "bg-danger";
                    break;
                case "Cancelled":
                    badgeClass = "bg-secondary";
                    break;
                default:
                    break;
            }

            return badgeClass;
        }
    }
}