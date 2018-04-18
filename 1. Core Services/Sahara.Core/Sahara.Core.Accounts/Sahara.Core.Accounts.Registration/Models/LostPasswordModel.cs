using System.ComponentModel.DataAnnotations;

namespace Sahara.Core.Accounts.Registration.Models
{
    public class LostPasswordModel
    {
        [Required]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
    }
}
