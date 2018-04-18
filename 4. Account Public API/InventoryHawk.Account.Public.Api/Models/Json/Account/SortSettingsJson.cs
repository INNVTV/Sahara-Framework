using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Account
{
    public class SortSettingsJson
    {
        public SortJson truncatedList { get; set; }
        public SortJson mixedList { get; set; }
        public SortJson fullList { get; set; }
    }

    public class SortJson
    {
        public string name { get; set; }
        public string value { get; set; }
    }

}