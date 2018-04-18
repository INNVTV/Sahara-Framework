using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Models.Partitions
{
    [Serializable]
    [DataContract]
    public class SearchPartition
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Plan { get; set; }

        [DataMember]
        public int TenantCount { get; set; }

        [DataMember]
        public int MaxTenants { get; set; } //<-- Derived Property from Settings
    }
}
