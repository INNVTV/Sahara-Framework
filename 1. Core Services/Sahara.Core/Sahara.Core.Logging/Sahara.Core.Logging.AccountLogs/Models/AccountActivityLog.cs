using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Logging.AccountLogs.Models
{
    /// <summary>
    /// Public version of entity exposed by WCF to clients. All log results are transformed into this type.
    /// </summary>
    [DataContract]
    public class AccountActivityLog
    {
        [DataMember]
        public string Category { get; set; }
        [DataMember]
        public string Activity { get; set; }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Details { get; set; }

        [DataMember]
        public string UserID { get; set; }
        [DataMember]
        public string UserEmail { get; set; }
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string ObjectID { get; set; }

        [DataMember]
        public string ObjectName { get; set; }

        [DataMember]
        public string Object { get; set; }

        [DataMember]
        public string IPAddress { get; set; }

        [DataMember]
        public string Origin { get; set; }

        [DataMember]
        public DateTimeOffset Timestamp { get; set; }
    }
}
