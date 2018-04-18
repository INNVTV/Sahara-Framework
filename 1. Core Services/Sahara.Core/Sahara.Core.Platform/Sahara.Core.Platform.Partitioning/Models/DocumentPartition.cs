using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sahara.Core.Platform.Partitioning.Models
{
    [Serializable]
    [DataContract]
    public class DocumentPartition
    {

        [DataMember]
        public string DocumentPartitionID { get; set; }

        //[DataMember]
        //public string DocumentPartitionTierID { get; set; }

        //[DataMember]
        //public int TenantCount { get; set; }

        [DataMember]
        public DateTime CreateDate { get; set; }

        [DataMember]
        public DateTime LastUpdatedDate { get; set; }

        //[DataMember]
        //public int SequenceID { get; set; } //<-- Used To Generate DocumentPartitionID. For example [DocumentPartitionTierID]_[SequenceID] or Free_1001 or Shared_1001, Shared_1002, Dedicated_1001 

        //[DataMember]
        //public List<Account> Tenants;
    }
}
