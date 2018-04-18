using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Sahara.Core.Accounts.Registration.Models
{
    [DataContract]
    public class RegisterNewAccountModel
    {
        [DataMember]
        [Required]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }

        [DataMember]
        [Required]
        [Display(Name = "PhoneNumber")]
        public string PhoneNumber { get; set; }

        [DataMember]
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [DataMember]
        [Required]
        [Display(Name = "FirstName")]
        public string FirstName { get; set; }

        [DataMember]
        [Required]
        [Display(Name = "LastName")]
        public string LastName { get; set; }

        [DataMember]
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataMember]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [DataMember]
        public string Origin { get; set; } //<--- WindowsTablet, Website, Etc....


    }
    
}
