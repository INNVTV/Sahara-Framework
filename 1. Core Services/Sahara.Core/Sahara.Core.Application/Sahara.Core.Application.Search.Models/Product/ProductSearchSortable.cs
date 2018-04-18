using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Search.Models.Product
{
    [DataContract]
    public class ProductSearchSortable
    {
        [DataMember]
        public string SortLabel { get; set; }

        [DataMember]
        public string OrderByString { get; set; }
    }
}
