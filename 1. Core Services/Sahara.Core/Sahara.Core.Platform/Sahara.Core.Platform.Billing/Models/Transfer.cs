using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Billing.Models
{
    [DataContract]
    public class Transfer
    {
        [DataMember]
        public Decimal Amount { get; set; }

        [DataMember]
        public string TransferID { get; set; }

        //[DataMember]
        //public BalanceTransaction BalanceTransaction { get; set; }

        [DataMember]
        public string BalanceTransactionID { get; set; }

        [DataMember]
        public string Destination { get; set; }

        [DataMember]
        public string DestinationPayment { get; set; }

        [DataMember]
        public String StatementDescription { get; set; }

        [DataMember]
        public String Description { get; set; }

        [DataMember]
        public String Type { get; set; }

        [DataMember]
        public String Status { get; set; }

        [DataMember]
        public String FailureCode { get; set; }

        [DataMember]
        public String FailureMessage { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [DataMember]
        public DateTime Created { get; set; }
    }
}
