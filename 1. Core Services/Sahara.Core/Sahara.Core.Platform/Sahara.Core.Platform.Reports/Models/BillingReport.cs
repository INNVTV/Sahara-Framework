using Sahara.Core.Platform.Billing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Reports.Models
{
    [DataContract]
    public class BillingReport
    {

        [DataMember]
        public bool BillingIssues; //<-- Any billing issues during this duration?



        [DataMember]
        public int ChargeCount;
        //[DataMember]
        //public int FailedChargeCount;
        [DataMember]
        public int RefundCount;
        [DataMember]
        public int TransferCount;
        [DataMember]
        public int TransferCount_Pending;
        [DataMember]
        public int TransferCount_Complete;

        [DataMember]
        public decimal TotalGross;
        [DataMember]
        public decimal TotalRefunds;
        [DataMember]
        public decimal TotalFees;
        [DataMember]
        public decimal TotalNet;
        [DataMember]
        public decimal TotalTransfers;
        [DataMember]
        public decimal TotalTransfers_Pending;
        [DataMember]
        public decimal TotalTransfers_Complete;

        [DataMember]
        public List<BalanceTransaction> BalanceTransactions;

        [DataMember]
        public List<BalanceTransaction> BalanceTransactions_Created;
        [DataMember]
        public List<BalanceTransaction> BalanceTransactions_Available;

    }
}
