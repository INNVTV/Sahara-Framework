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
    public class CategoryTreeModel
    {
        [DataMember]
        public Guid ID;

        [DataMember]
        public string Name;

        [DataMember]
        public string NameKey;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;

        [DataMember]
        public List<SubcategoryTreeModel> Subcategories = new List<SubcategoryTreeModel>();
 
    }

    [DataContract]
    public class SubcategoryTreeModel
    {
        [DataMember]
        public Guid ID;

        [DataMember]
        public string Name;

        [DataMember]
        public string NameKey;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;

        [DataMember]
        public List<SubsubcategoryTreeModel> Subsubcategories = new List<SubsubcategoryTreeModel>();

    }

    [DataContract]
    public class SubsubcategoryTreeModel
    {
        [DataMember]
        public Guid ID;

        [DataMember]
        public string Name;

        [DataMember]
        public string NameKey;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;

        [DataMember]
        public List<SubsubsubcategoryTreeModel> Subsubsubcategories = new List<SubsubsubcategoryTreeModel>();

    }

    [DataContract]
    public class SubsubsubcategoryTreeModel
    {
        [DataMember]
        public Guid ID;

        [DataMember]
        public string Name;

        [DataMember]
        public string NameKey;

        [DataMember]
        public string LocationPath;

        [DataMember]
        public string FullyQualifiedName;

    }


}
