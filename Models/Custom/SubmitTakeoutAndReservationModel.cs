using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
// imports
using System.ComponentModel.DataAnnotations;

namespace Uling_RestaurantManagementSystem.Models.Custom
{
    public class SubmitTakeoutAndReservationModel
    {
        public int order_id { get; set; }
        public string customer_name { get; set; }
        public string contact_number { get; set; }
        public string email { get; set; }
        public List<string> selectedItems { get; set; }
        public string reservation_date { get; set; }
        public string time_start { get; set; }
        public string time_end { get; set; }
        public int? heads { get; set; }
        public string payment_url { get; set; }
    }
}