using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AccountAdminSite.Models.Authentication
{
    public class Login
    {

        [Required(ErrorMessage = "Please specify an account name.")]
        public string AccountName { get; set; }


        [Required(ErrorMessage = "Please use your email address.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [Required(ErrorMessage = "Your password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

    }
}