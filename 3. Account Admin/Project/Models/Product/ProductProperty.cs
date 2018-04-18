using AccountAdminSite.ApplicationProductService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountAdminSite.Models.Product
{
    public class ProductProperty
    {
        public string ValueType;
        public bool Appendable;

        public string Symbol;
        public string SymbolPlacement;

        public List<string> PredefinedValues;

        public string Name;
        public string NameKey;

        public string AssignedValue;       
        public List<string> AssignedValues;

        public List<SwatchObject> AssignedSwatches;
        public List<SwatchObject> AvailableSwatches;

        public LocationObject LocationData;

    }

    //public class PredefinedValue
    //{
        //Label
    //}

    public class SwatchObject
    {
        public string Label { get; set; }
        public string Image { get; set; }
        public string NameKey { get; set; }
    }

    public class LocationObject
    {
        public string MapUrl { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }
    }
}