using InventoryHawk.Account.Public.Api.Models.Json.Categorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Products
{
    public class ProductDetailJson
    {
        //public int count { get; set; }
        public string executionType { get; set; }
        public string executionTime { get; set; }

        //Parent Info
        public CategorizationParentItemJson category { get; set; }
        public CategorizationParentItemJson subcategory { get; set; }
        public CategorizationParentItemJson subsubcategory { get; set; }
        public CategorizationParentItemJson subsubsubcategory { get; set; }

        public IDictionary<string, object> item { get; set; }
    }
}
