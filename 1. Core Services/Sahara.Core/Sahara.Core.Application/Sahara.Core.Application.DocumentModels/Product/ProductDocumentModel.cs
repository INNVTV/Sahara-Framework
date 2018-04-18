using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.DocumentModels.Product
{
    [DataContract]
    public class ProductDocumentModel
    {
        [DataMember]
        [JsonProperty(PropertyName = "id")] //<-- Required for all Documents
        public string Id { get; set; }

        [JsonProperty(PropertyName = "_self")]
        public string SelfLink { get; internal set; }

        //----------- Isolate Tenant Products via AccountID & DocumentType -------------------------

        //[DataMember]
        //public string AccountID { get; set; }

        //[DataMember]
        //public string AccountNameKey { get; set; }

        [DataMember]
        public string DocumentType { get; set; }


        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string NameKey { get; set; }

        [DataMember]
        public string LocationPath { get; set; } //<-- categorynameshort/subcategorynameshort/subsubcategorynameshort/subsubsubcategorynameshort

        [DataMember]
        public string FullyQualifiedName { get; set; }  //<-- categorynameshort/subcategorynameshort/subsubcategorynameshort/subsubsubcategorynameshort/productnameshort

        [DataMember]
        public DateTimeOffset DateCreated { get; set; }

        //------- Category Data (Names)----------

        [DataMember]
        public string CategoryName { get; set; }

        [DataMember]
        public string SubcategoryName { get; set; }

        [DataMember]
        public string SubsubcategoryName { get; set; }

        [DataMember]
        public string SubsubsubcategoryName { get; set; }

        //------- Category Data (NameKeys)----------

        [DataMember]
        public string CategoryNameKey { get; set; }

        [DataMember]
        public string SubcategoryNameKey { get; set; }

        [DataMember]
        public string SubsubcategoryNameKey { get; set; }

        [DataMember]
        public string SubsubsubcategoryNameKey { get; set; }

        // ----------------------------------


        //[DataMember]
        //public string Title { get; set; }

        //[DataMember]
        //public string Description { get; set; }

        //[DataMember]
        //public string FilePath { get; set; }

        [DataMember]
        public int OrderID { get; set; }

        [DataMember]
        public bool Visible { get; set; }

        //-------------- Properties -------------------------------

        [DataMember]
        public Dictionary<string, string> Properties { get; set; } //<-- strings, paragraphs, numbers, datetime, etc....

        [DataMember]
        public Dictionary<string, List<string>> Predefined { get; set; } //<-- predefined

        [DataMember]
        public Dictionary<string, List<Swatch>> Swatches { get; set; } //<-- swatches, allows for multiple swatch options

        [DataMember]
        public Dictionary<string, PropertyLocationValue> Locations { get; set; } //<-- locations

        //---------------------------------------------------------

        //-------------- Collections -------------------------------

        [DataMember]
        public List<string> Tags { get; set; } //<-- Searchable tags

        //---------------------------------------------------------

        //-------------- Image Data -------------------------------

        //[DataMember]
        //public List<ImageGroup> Images;

        // Images > Tablet > DetailMain
        // Images > Tablet > Thumbnail

        //---------------------------------------------------------
    }

    [DataContract]
    public class PropertyLocationValue
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Address1 { get; set; }

        [DataMember]
        public string Address2 { get; set; }

        [DataMember]
        public string City { get; set; }

        [DataMember]
        public string State { get; set; }

        [DataMember]
        public string PostalCode { get; set; }

        [DataMember]
        public string Country { get; set; }

        [DataMember]
        public string Lat { get; set; }

        [DataMember]
        public string Long { get; set; }

        //[DataMember]
        //public string GeographyPoint { get; set; }
    }


    [DataContract]
    public class Swatch
    {
        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public string Image { get; set; }

        [DataMember]
        public string ImageMedium { get; set; }

        [DataMember]
        public string ImageSmall { get; set; }
    }
}
