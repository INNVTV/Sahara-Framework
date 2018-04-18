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
    public class StoragePartition
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string URL { get; set; }

        [DataMember]
        public string CDN { get; set; }

        [DataMember]
        public int TenantCount { get; set; }

        [DataMember]
        public int MaxTenants { get; set; } //<--- Derived property from static platform settings

    }
}
