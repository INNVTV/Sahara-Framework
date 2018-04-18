using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Billing.Models
{
    [DataContract]
    public class Fee
    {
        [DataMember]
        public String Id { get; set; }

        [DataMember]
        public String BalanceTransactionId { get; set; }

        [DataMember]
        public String ChargeId { get; set; }

        [DataMember]
        public Decimal Amount { get; set; }

        [DataMember]
        public String Description { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public bool Refunded { get; set; }


    }
}
