using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Properties
{
    //Listing
    public class PropertiesResultJson
    {
        public int count { get; set; }
        public string executionType { get; set; }
        public string executionTime { get; set; }

        public List<PropertyJson> properties { get; set; }
    }

    //Single
    public class PropertyResultJson
    {
        public string executionType { get; set; }
        public string executionTime { get; set; }

        public PropertyJson property { get; set; }
    }

    public class PropertyJson
    {
        public string propertyName { get; set; }
        public string propertyNameKey { get; set; }
        public string propertyType { get; set; }
        public string searchField { get; set; }

        //numbers
        public PropertySymbolJson symbol { get; set; }

        //swatches
        public List<PropertySwatchesJson> swatches { get; set; }
    }

    public class PropertySymbolJson
    {
        public string value { get; set; }
        public string placement { get; set; }
    }

    public class PropertySwatchesJson
    {
        public string label { get; set; }
        public string image { get; set; }
        public string imageMedium { get; set; }
        public string imageSmall { get; set; }
    }
}