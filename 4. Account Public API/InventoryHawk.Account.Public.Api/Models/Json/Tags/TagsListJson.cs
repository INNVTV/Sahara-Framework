using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Tags
{
    public class TagsListJson
    {
        public int count { get; set; }
        public string executionType { get; set; }
        public string executionTime { get; set; }

        public List<string> tags { get; set; }
    }
}