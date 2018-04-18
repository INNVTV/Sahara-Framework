using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlatformAdminSite.Models.Authentication
{
    public class ResetPassword
    {
        public string PasswordClaimKey { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "Please include the new password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your new password.")]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }
    }
}