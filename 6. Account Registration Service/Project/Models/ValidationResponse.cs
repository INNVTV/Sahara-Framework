using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sahara.Api.Accounts.Registration.Models
{
    public class ValidationResponse
    {
        public bool valid { get; set; }
        public string message { get; set; }
    }
}