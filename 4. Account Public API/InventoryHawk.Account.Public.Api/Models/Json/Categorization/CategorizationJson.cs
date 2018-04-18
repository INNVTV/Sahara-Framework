using InventoryHawk.Account.Public.Api.Models.Json.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Categorization
{ 

    public class CategoryJson
    {
        //public string id { get; set; }
        public string name { get; set; }
        public string nameKey { get; set; }
        public string fullyQualifiedName { get; set; }
        public string description { get; set; }

        public IDictionary<string, object> images { get; set; }
        public List<CategorizationListItemJson> subcategories { get; set; }

        public List<IDictionary<string, object>> items { get; set; }

    }

    public class SubcategoryJson
    {

        //public string id { get; set; }
        public string name { get; set; }
        public string nameKey { get; set; }
        public string fullyQualifiedName { get; set; }
        public string description { get; set; }   

        public IDictionary<string, object> images { get; set; }
        public List<CategorizationListItemJson> subsubcategories { get; set; }

        public List<IDictionary<string, object>> items { get; set; }

    }

    public class SubsubcategoryJson
    {

        //public string id { get; set; }
        public string name { get; set; }
        public string nameKey { get; set; }
        public string fullyQualifiedName { get; set; }
        public string description { get; set; }

        public IDictionary<string, object> images { get; set; }
        public List<CategorizationListItemJson> subsubsubcategories { get; set; }

        public List<IDictionary<string, object>> items { get; set; }

    }

    public class SubsubsubcategoryJson
    {
        
        //public string id { get; set; }
        public string name { get; set; }
        public string nameKey { get; set; }
        public string fullyQualifiedName { get; set; }
        public string description { get; set; }

        public IDictionary<string, object> images { get; set; }

        public List<IDictionary<string, object>> items { get; set; }

    }
}