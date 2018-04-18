using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Account
{
    public class SalesSettingsJson
    {
        public bool useSalesLeads { get; set; }
        public string buttonCopy { get; set; }
        public string leadsDescription { get; set; }
    }
}