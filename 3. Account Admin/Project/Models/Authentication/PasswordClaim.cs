using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AccountAdminSite.Models.Authentication
{
    public class PasswordClaim
    {
        public string AccountNameKey { get; set; }

        [Required(ErrorMessage = "Please include an email address.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}