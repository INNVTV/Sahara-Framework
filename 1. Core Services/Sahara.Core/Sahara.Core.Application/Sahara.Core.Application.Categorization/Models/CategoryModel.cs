using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Categorization.Models
{
    [DataContract]
    public class CategoryModel
    {
        [DataMember]
        public Guid CategoryID;

        [DataMember]
        public string CategoryName;

        [DataMember]
        public string CategoryNameKey;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;

        [DataMember]
        public string Description;
   
        /*
        [DataMember]
        public string ShortDescription;

        [DataMember]
        public string LongDescription;
        */

        [DataMember]
        public List<SubcategoryListModel> Subcategories = new List<SubcategoryListModel>();

        [DataMember]
        public int OrderID;

        [DataMember]
        public bool Visible;

        [DataMember]
        public DateTime CreatedDate;
    }

    [DataContract]
    public class CategoryParentModel
    {
        [DataMember]
        public Guid CategoryID;

        [DataMember]
        public string CategoryName;

        [DataMember]
        public string CategoryNameKey;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;

        [DataMember]
        public bool Visible;

    }
}
