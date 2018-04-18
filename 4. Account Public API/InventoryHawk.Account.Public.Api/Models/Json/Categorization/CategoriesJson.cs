using InventoryHawk.Account.Public.Api.Models.Json.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Categorization
{    
    /// <summary>
    /// It's best practice to wrap JsonArrays into a JsonObject in case we ever need to extend our call by adding a count, execution time, or other properties
    /// </summary>
    public class CategoriesJson
    {
        public int count { get; set; }
        public string executionType { get; set; }
        public string executionTime { get; set; }

        public List<CategorizationListItemJson> categories { get; set; }
    }

}