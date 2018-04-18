using Sahara.Core.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Billing.Models
{
    [DataContract]
    public class Charge
    {
        [DataMember]
        public string ChargeID { get; set; }

        [DataMember]
        public string InvoiceID { get; set; }

        [DataMember]
        public Invoice Invoice { get; set; }


        [DataMember]
        public string BalanceTransactionID { get; set; }

        //[DataMember]
        //public BalanceTransaction BalanceTransaction { get; set; }



        [DataMember]
        public string StripeCustomerID { get; set; }

        //[DataMember]
        //public Account Account { get; set; } //<-- Only used when getting ALL invoices by PlatformAdmin
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public string AccountID { get; set; }

        [DataMember]
        public string Description { get; set; }
        [DataMember]

        public string CardBrand { get; set; }
        [DataMember]
        public string CardLast4 { get; set; }

        [DataMember]
        public Decimal Amount { get; set; }

        [DataMember]
        public Decimal TotalRefunded { get; set; }


        [DataMember]
        public DateTime Date { get; set; }


        [DataMember]
        public bool Paid { get; set; }
        [DataMember]
        public bool Prorated { get; set; }
        [DataMember]
        public bool Refunded { get; set; }
        [DataMember]
        public bool Failure { get; set; }
        [DataMember]
        public string FailureCode { get; set; }
        [DataMember]
        public string FailureMessage { get; set; }


    }
}
