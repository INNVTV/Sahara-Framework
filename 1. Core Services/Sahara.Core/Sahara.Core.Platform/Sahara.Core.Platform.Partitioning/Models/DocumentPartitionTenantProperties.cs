using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Partitioning.Models
{
    [DataContract]
    public class DocumentPartitionTenantCollectionProperties
    {
        [DataMember]
        public int DocumentCount;

        [DataMember]
        public int ProductCount;
    }
}
