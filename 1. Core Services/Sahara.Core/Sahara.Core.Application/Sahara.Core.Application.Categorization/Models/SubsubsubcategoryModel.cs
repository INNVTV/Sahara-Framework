using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Categorization.Models
{
     
    [DataContract]
    public class SubsubsubcategoryModel
    {
        [DataMember]
        public Guid SubsubsubcategoryID;

        [DataMember]
        public Guid SubsubcategoryID;

        [DataMember]
        public Guid SubcategoryID;

        [DataMember]
        public Guid CategoryID;

        [DataMember]
        public string SubsubsubcategoryName;

        [DataMember]
        public string SubsubsubcategoryNameKey;

        [DataMember]
        public bool Visible;

        [DataMember]
        public int OrderID;

        [DataMember]
        public DateTime CreatedDate;

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
        public CategoryParentModel Category;

        [DataMember]
        public SubcategoryParentModel Subcategory;

        [DataMember]
        public SubsubcategoryParentModel Subsubcategory;

    }

    [DataContract]
    public class SubsubsubcategoryListModel
    {
        [DataMember]
        public Guid SubsubsubcategoryID;

        [DataMember]
        public string SubsubsubcategoryName;

        [DataMember]
        public string SubsubsubcategoryNameKey;

        [DataMember]
        public bool Visible;

        [DataMember]
        public int OrderID;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;
    }

}
