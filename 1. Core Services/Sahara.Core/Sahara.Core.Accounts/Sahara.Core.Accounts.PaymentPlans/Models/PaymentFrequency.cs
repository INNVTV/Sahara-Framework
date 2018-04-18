using System;
using System.Runtime.Serialization;
namespace Sahara.Core.Accounts.PaymentPlans.Models
{
    [Serializable]
    [DataContract]
    public class PaymentFrequency
    {
        [DataMember]
        public int PaymentFrequencyMonths { get; set; } //<-- ID

        [DataMember]
        public string PaymentFrequencyName { get; set; }
        [DataMember]
        public string Interval { get; set; }
        [DataMember]
        public int IntervalCount { get; set; }
        [DataMember]
        public decimal PriceBreak { get; set; }

    }
}
