using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Partitioning.Models
{
    [DataContract]
    public class DocumentPartitionCollectionProperties
    {
        [DataMember]
        public int DocumentCount; //<-- TO reflect accuratly must be incremented to include itself (+1) as well as every AccountProperties document ( + (partition.tenantcount * 1))

        [DataMember]
        public int ProductCount;
    }
}
