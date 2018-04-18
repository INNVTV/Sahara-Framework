using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Billing.Models
{
    [DataContract]
    public class BalanceTransaction
    {
        [DataMember]
        public string BalanceTransactionID { get; set; }

        [DataMember]
        public Decimal Amount { get; set; }
        [DataMember]
        public Decimal Fee { get; set; }
        [DataMember]
        public Decimal Net { get; set; }

        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public DateTime Created { get; set; }
        [DataMember]
        public DateTime Available { get; set; }
    }
}
