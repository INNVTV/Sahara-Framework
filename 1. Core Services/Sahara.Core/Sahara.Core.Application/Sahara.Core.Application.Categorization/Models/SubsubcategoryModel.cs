using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Categorization.Models
{
     
    [DataContract]
    public class SubsubcategoryModel
    {
        [DataMember]
        public Guid SubsubcategoryID;

        [DataMember]
        public Guid SubcategoryID;

        [DataMember]
        public Guid CategoryID;

        [DataMember]
        public string SubsubcategoryName;

        [DataMember]
        public string SubsubcategoryNameKey;

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
        public List<SubsubsubcategoryListModel> Subsubsubcategories = new List<SubsubsubcategoryListModel>();


    }


    [DataContract]
    public class SubsubcategoryListModel
    {
        [DataMember]
        public Guid SubsubcategoryID;

        [DataMember]
        public string SubsubcategoryName;

        [DataMember]
        public string SubsubcategoryNameKey;

        [DataMember]
        public bool Visible;

        [DataMember]
        public int OrderID;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;
    }

    [DataContract]
    public class SubsubcategoryParentModel
    {
        [DataMember]
        public Guid SubsubcategoryID;

        [DataMember]
        public string SubsubcategoryName;

        [DataMember]
        public string SubsubcategoryNameKey;

        [DataMember]
        public bool Visible;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;
    }

}
