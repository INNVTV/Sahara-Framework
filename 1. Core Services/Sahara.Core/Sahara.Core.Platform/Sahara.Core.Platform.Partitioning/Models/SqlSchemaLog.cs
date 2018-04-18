using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sahara.Core.Platform.Partitioning.Models
{
    [Serializable]
    [DataContract]
    public class SqlSchemaLog
    {

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public DateTime InstallDate { get; set; }
    }
}
