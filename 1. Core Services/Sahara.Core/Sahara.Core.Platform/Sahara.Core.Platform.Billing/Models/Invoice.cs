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
    public class Invoice
    {
        [DataMember]
        public string InvoiceID { get; set; }

        [DataMember]
        public string StripeCustomerID { get; set; }

        [DataMember]
        public Account Account { get; set; } //<-- Only used when getting ALL invoices by PlatformAdmin

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public Decimal Subtotal { get; set; }
        [DataMember]
        public Decimal Total { get; set; }
        [DataMember]
        public Decimal AmountDue { get; set; }

        [DataMember]
        public Decimal StartingBalance { get; set; }
        [DataMember]
        public Decimal EndingBalance { get; set; }


        [DataMember]
        public int? Attempts { get; set; }
        [DataMember]
        public DateTime? NextAttempt { get; set; }

        [DataMember]
        public DateTime? Date { get; set; }


        [DataMember]
        public bool Paid { get; set; }



        [DataMember]
        public List<InvoiceLineItem> LineItems { get; set; }

    }
}
