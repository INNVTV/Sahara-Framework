using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlatformAdminSite.Models.Users
{
    public class RegisterPlatformUserModel
    {

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }


        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "FirstName")]
        [MinLength(3)]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "LastName")]
        [MinLength(3)]
        public string LastName { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [MinLength(8)]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }



        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            ValidationResult result = null;


            if (Password.Length < 8)
            {
                result = new ValidationResult("Password must be at least 8 characters in length.");
            }

            if (result != null)
            {
                yield return result;
            }
        }
    }
}