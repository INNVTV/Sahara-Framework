using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties.Models
{
    [DataContract]
    public class PropertyModel
    {
        [DataMember]
        public string PropertyTypeNameKey;

        [DataMember]
        public Guid PropertyID;

        [DataMember]
        public string PropertyName;

        [DataMember]
        public string PropertyNameKey;

        [DataMember]
        public string SearchFieldName;

        [DataMember]
        public List<PropertyValueModel> Values;

        [DataMember]
        public List<PropertySwatchModel> Swatches;

        [DataMember]
        public string Symbol;
        [DataMember]
        public string SymbolPlacement;
        [DataMember]
        public string NumericDescriptor;

        [DataMember]
        public int OrderID;

        [DataMember]
        public int FeaturedID;

        [DataMember]
        public int FacetOrderID;

        //[DataMember]
        //public int ListingOrderID;

        //[DataMember]
        //public int DetailsOrderID;

        [DataMember]
        public bool Listing;

        [DataMember]
        public bool Details;

        //[DataMember]
        //public bool Visible;

        [DataMember]
        public bool Facetable;

        [DataMember]
        public bool Appendable;

        [DataMember]
        public bool Sortable;

        //[DataMember]
        //public bool Highlighted;

        [DataMember]
        public bool AlwaysFacetable;

        [DataMember]
        public int FacetInterval;

        [DataMember]
        public DateTime CreatedDate;
    }
}
