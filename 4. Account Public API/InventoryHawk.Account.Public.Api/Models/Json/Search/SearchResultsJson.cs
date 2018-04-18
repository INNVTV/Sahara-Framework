using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Search
{
    public class SearchResultsJson
    {
        public long? count { get; set; }
        public int returned { get; set; }
        public string range { get; set; }
        public int remaining { get; set; }

        public string executionType { get; set; }
        public string executionTime { get; set; }

        public List<IDictionary<string, object>> items { get; set; }
    }
}