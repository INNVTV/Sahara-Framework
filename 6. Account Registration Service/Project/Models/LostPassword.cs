using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sahara.Api.Accounts.Registration.Models
{
    public class LostPassword
    {
        public string AccountID { get; set; }
        public string Email { get; set; }
    }
}