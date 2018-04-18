using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Categorization
{
    /// <summary>
    /// It's best practice to wrap JsonArrays into a JsonObject in case we ever need to extend our call by adding a count, execution time, or other properties
    /// </summary>
    public class CategoryDetailsJson
    {
        //public int count { get; set; }
        public string executionType { get; set; }
        public string executionTime { get; set; }

        public CategoryJson category { get; set; }
    }

    public class SubcategoryDetailsJson
    {
        //public int count { get; set; }
        public string executionType { get; set; }
        public string executionTime { get; set; }

        //Parent Info
        public CategorizationParentItemJson category { get; set; }

        //public CategorizationParentItemJson category { get; set; }

        public SubcategoryJson subcategory { get; set; }
    }

    public class SubsubcategoryDetailsJson
    {
        //public int count { get; set; }
        public string executionType { get; set; }
        public string executionTime { get; set; }

        //Parent Info
        public CategorizationParentItemJson category { get; set; }
        public CategorizationParentItemJson subcategory { get; set; }

        //public CategorizationParentItemJson category { get; set; }
        //public CategorizationParentItemJson subcategory { get; set; }

        public SubsubcategoryJson subsubcategory { get; set; }
    }

    public class SubsubsubcategoryDetailsJson
    {
        //public int count { get; set; }
        public string executionType { get; set; }
        public string executionTime { get; set; }

        //Parent Info
        public CategorizationParentItemJson category { get; set; }
        public CategorizationParentItemJson subcategory { get; set; }
        public CategorizationParentItemJson subsubcategory { get; set; }

        //public CategorizationParentItemJson category { get; set; }
        //public CategorizationParentItemJson subcategory { get; set; }
        //public CategorizationParentItemJson subsubcategory { get; set; }

        public SubsubsubcategoryJson subsubsubcategory { get; set; }
    }


}