using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Search.Models.Product
{
    [DataContract]
    public class ProductSearchModel
    {
        [DataMember]
        public string id { get; set; } //<-- Search Key
         
        //[DataMember]
        //public string AccountID { get; set; }

        //[DataMember]
        //public string AccountNameKey { get; set; }

        //[DataMember]
        //public string DocumentType { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string nameKey { get; set; }

        [DataMember]
        public string locationPath { get; set; }



        
        [DataMember]
        public string categoryNameKey { get; set; }

        [DataMember]
        public string subcategoryNameKey { get; set; }

        [DataMember]
        public string subsubcategoryNameKey { get; set; }

        [DataMember]
        public string subsubsubcategoryNameKey { get; set; }
        



        [DataMember]
        public string categoryName { get; set; }

        [DataMember]
        public string subcategoryName { get; set; }

        [DataMember]
        public string subsubcategoryName { get; set; }

        [DataMember]
        public string subsubsubcategoryName { get; set; }


        [DataMember]
        public DateTimeOffset dateCreated { get; set; }

        [DataMember]
        public string fullyQualifiedName { get; set; }

        //[DataMember]
        //public string title { get; set; }

        [DataMember]
        public int orderId { get; set; }

        [DataMember]
        public bool visible { get; set; }

        [DataMember]
        public string[] tags { get; set; }

        //These are now dynamic
        //[DataMember]
        //public string[] Properties { get; set; } //<-- Converted to a list of strings with Name:Value encoding

        //[DataMember]
        //public string[] thumbnails { get; set; } //<-- Converted to a list of strings with Name:Value encoding
    }


}
