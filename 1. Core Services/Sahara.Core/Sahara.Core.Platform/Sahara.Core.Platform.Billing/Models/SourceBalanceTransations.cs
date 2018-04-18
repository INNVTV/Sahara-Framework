using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Billing.Models
{
    [DataContract]
    public class SourceBalanceTransactions
    {
        [DataMember]
        public List<BalanceTransaction> Transactions;

        [DataMember]
        public Decimal TotalGross { get; set; }
        [DataMember]
        public Decimal TotalFees { get; set; }
        [DataMember]
        public Decimal TotalNet { get; set; }

    }
}
