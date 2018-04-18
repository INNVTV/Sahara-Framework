using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlatformAdminSite.Models.Authentication
{
    public class PasswordClaim
    {
        [Required(ErrorMessage = "Please include an email address.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}