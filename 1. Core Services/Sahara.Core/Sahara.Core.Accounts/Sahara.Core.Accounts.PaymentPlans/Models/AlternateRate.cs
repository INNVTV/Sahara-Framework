using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.PaymentPlans.Models
{
    /// <summary>
    /// Used as a helper to describe alternate rates to the monthly option to users
    /// </summary>
    [Serializable]
    [DataContract]
    public class AlternateRate
    {
        [DataMember]
        public string FrequencyName;

        [DataMember]
        public string FrequencyInterval;

        [DataMember]
        public int FrequencyInMonths;
        
        [DataMember]
        public decimal DiscountedPrice;

        [DataMember]
        public string SavingsPercentage;

        [DataMember]
        public string Description;
    }
}
