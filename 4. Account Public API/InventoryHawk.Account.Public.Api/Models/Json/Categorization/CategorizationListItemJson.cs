using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Categorization
{
    public class CategorizationListItemJson
    {
        //public string id { get; set; }
        public string name { get; set; }
        public string nameKey { get; set; }
        public string fullyQualifiedName { get; set; }
        //public string description { get; set; }

        public IDictionary<string, object> images { get; set; }
    }

}