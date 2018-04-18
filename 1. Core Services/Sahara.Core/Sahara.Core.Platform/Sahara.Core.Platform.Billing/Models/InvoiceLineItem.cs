using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Billing.Models
{
    [DataContract]
    public class InvoiceLineItem
    {
        [DataMember]
        public string Description;

        [DataMember]
        public string PlanDescription;

        [DataMember]
        public string PlanInterval;

        [DataMember]
        public int PlanIntervalCount;

        [DataMember]
        public string Amount;

        [DataMember]
        public bool? Proration;
    }
}
