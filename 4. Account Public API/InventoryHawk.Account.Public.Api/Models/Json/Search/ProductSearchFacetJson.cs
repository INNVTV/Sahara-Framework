using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Search
{

    public class ProductSearchFacetsJson
    {
        public string executionType { get; set; }
        public string executionTime { get; set; }

        public List<ProductSearchFacetJson> facets { get; set; }
    }

    public class ProductSearchFacetJson
    {        
        public string name { get; set; }
        public string key { get; set; }
        public string type { get; set; }

        public List<ProductSearchFacetJsonValue> values { get; set; }
    }

    public class ProductSearchFacetJsonValue
    {
        public string name { get; set; }
        public string filter { get; set; }
        public string count { get; set; }
        public string image { get; set; }
        //public string value { get; set; }
    }
}