using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Account
{
    public class ContactSettingsJson
    {       
        public bool showPhoneNumber { get; set; }
        public bool showAddress { get; set; }
        public bool showEmail { get; set; }

        public ContactDetailsJson contactDetails { get; set;}

    }

    public class ContactDetailsJson
    {
        public string phoneNumber { get; set; }

        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }

        public string email { get; set; }
    }
}