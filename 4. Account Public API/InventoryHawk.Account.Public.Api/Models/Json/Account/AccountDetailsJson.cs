using InventoryHawk.Account.Public.Api.Models.Json.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Account
{    
    /// <summary>
    /// It's best practice to wrap JsonArrays into a JsonObject in case we ever need to extend our call by adding a count, execution time, or other properties
    /// </summary>
    public class AccountDetailsJson
    {
        public string executionType { get; set; }
        public string executionTime { get; set; }

        public AccountJson account { get; set; }
    }

    public class AccountJson
    {
        public string accountName { get; set; }
        public string accountNameKey { get; set; }
        public string customDomain { get; set; }

        public string paymentPlan { get; set; }
        //public string paymentPlanName { get; set; }
        //public string paymentPlanFrequency { get; set; }

        public SalesSettingsJson salesSettings { get; set; }
        public ContactSettingsJson contactSettings { get; set; }
        //public SortSettingsJson sortSettings { get; set; }
        //public ThemeSettingsJson themeSettings { get; set; }

        public IDictionary<string, object> images { get; set; }

    }

    

}