using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Search.Models.Product
{
    [DataContract]
    public class ProductSearchFacet
    {
        [DataMember]
        public string PropertyName { get; set; }

        [DataMember]
        public string PropertyType { get; set; }

        [DataMember]
        public string SearchFieldName { get; set; }

        //[DataMember]
        //public string FilterStyle { get; set; }

        [DataMember]
        public List<FacetValue> Values { get; set; }

        [DataMember]
        public bool ShowAdvanced { get; set; } //<--Added to help with Angular Calls in UI ( to show or hide advanced options)

        [DataMember]
        public string BooleanOperator { get; set; } //<--Added to help with Angular Calls in UI ( to use either "and" or "or" when combing filters)

        //[DataMember]
        //public ProductSearchFacetType Type { get; set; }

        //[DataMember]
        //public ProductSearchFacetStyle Style { get; set; }


        // ------ Settings used based on type/style combos ----

        //[DataMember]
        //public List<Int32> Range { get; set; }
    }

    [DataContract]
    public class FacetValue
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string Image { get; set; }

        [DataMember]
        public string Filter { get; set; }

        [DataMember]
        public string Count { get; set; }

        [DataMember]
        public bool Selected { get; set; } //<--Added to help with Angular UI's


    }


    /*
    [DataContract]
    public class ProductSearchFacetStyle
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string NameKey { get; set; }
    }
    */


    /*
    [DataContract]
    public class ProductSearchFacetType
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string NameKey { get; set; }
    }
*/
}
