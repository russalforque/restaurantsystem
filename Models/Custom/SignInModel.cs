using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
// imports
using System.ComponentModel.DataAnnotations;

namespace Uling_RestaurantManagementSystem.Models.Custom
{
    public class SignInModel
    {
        [Required(ErrorMessage = "Email field is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password field is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}