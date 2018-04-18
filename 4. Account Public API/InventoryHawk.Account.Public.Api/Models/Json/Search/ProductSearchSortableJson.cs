using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Search
{

    public class ProductSearchSortablesJsonResult
    {
        public string executionType { get; set; }
        public string executionTime { get; set; }

        public List<ProductSearchSortableJson> sortables { get; set; }
    }

    public class ProductSearchSortableJson
    {                
        public string label { get; set; }
        public string orderByString { get; set; }

        //public string name { get; set; }

    }

}