using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sahara.Core.Platform.Partitioning.Models
{
    [Serializable]
    [DataContract]
    public class SqlPartition
    {
        public SqlPartition()
        {
            MaxTenants = Sahara.Core.Settings.Platform.Partitioning.MaximumTenantsPerAccountDatabasePartition;
            //Tenants = new List<Account>();
            //Schemas = new List<String>();
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int TenantCount { get; set; }
        [DataMember]
        public int MaxTenants { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        //[DataMember]
        //public List<String> Schemas;

        //[DataMember]
        //public List<Account> Tenants;
    }
}
