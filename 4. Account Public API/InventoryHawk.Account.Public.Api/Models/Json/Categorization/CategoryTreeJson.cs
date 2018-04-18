using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Categorization
{
    public class CategoryTreeJson
    {
        public string executionType { get; set; }
        public string executionTime { get; set; }

        public int categoryDepth { get; set; }

        public List<CategoryTreeListItemJson> categories { get; set; }
    }

    public class CategoryTreeListItemJson
    {
        //public string id { get; set; }
        public string name { get; set; }
        public string nameKey { get; set; }
        public string fullyQualifiedName { get; set; }

        public IDictionary<string, object> images { get; set; }

        public List<SubcategoryTreeListItemJson> subcategories { get; set; }
    }

    public class SubcategoryTreeListItemJson
    {
        //public string id { get; set; }
        public string name { get; set; }
        public string nameKey { get; set; }
        public string fullyQualifiedName { get; set; }

        public IDictionary<string, object> images { get; set; }

        public List<SubsubcategoryTreeListItemJson> subsubcategories { get; set; }
    }

    public class SubsubcategoryTreeListItemJson
    {
        //public string id { get; set; }
        public string name { get; set; }
        public string nameKey { get; set; }
        public string fullyQualifiedName { get; set; }

        public IDictionary<string, object> images { get; set; }

        public List<SubsubsubcategoryTreeListItemJson> subsubsubcategories { get; set; }
    }
    public class SubsubsubcategoryTreeListItemJson
    {
        //public string id { get; set; }
        public string name { get; set; }
        public string nameKey { get; set; }
        public string fullyQualifiedName { get; set; }

        public IDictionary<string, object> images { get; set; }
    }
}