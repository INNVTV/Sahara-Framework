using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Categorization.Models
{
    [DataContract]
    public class SubcategoryModel
    {
        [DataMember]
        public Guid SubcategoryID;

        [DataMember]
        public Guid CategoryID;

        [DataMember]
        public string SubcategoryName;

        [DataMember]
        public string SubcategoryNameKey;

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
        public bool Visible;

        [DataMember]
        public int OrderID;

        [DataMember]
        public DateTime CreatedDate;

        [DataMember]
        public CategoryParentModel Category;

        [DataMember]
        public List<SubsubcategoryListModel> Subsubcategories = new List<SubsubcategoryListModel>();
    }

    [DataContract]
    public class SubcategoryListModel
    {
        [DataMember]
        public Guid SubcategoryID;

        [DataMember]
        public string SubcategoryName;

        [DataMember]
        public string SubcategoryNameKey;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;

        [DataMember]
        public bool Visible;

        [DataMember]
        public int OrderID;
    }

    [DataContract]
    public class SubcategoryParentModel
    {
        [DataMember]
        public Guid SubcategoryID;

        [DataMember]
        public string SubcategoryName;

        [DataMember]
        public string SubcategoryNameKey;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;

        [DataMember]
        public bool Visible;

    }
}
