using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Billing.Models
{
    [DataContract]
    public class DunningAttempt
    {
        [DataMember]
        public DateTimeOffset WarningDate { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Amount { get; set; }

        [DataMember]
        public string StripeSubscriptionId { get; set; }
        [DataMember]
        public string StripeChargeId { get; set; }
        [DataMember]
        public string StripeEventId { get; set; }
    }
}
