using Sahara.Core.Platform.Billing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Snapshots.Models
{
    [DataContract]
    public class BillingSnapshot
    {
        [DataMember]
        public int CreditsInCirculation;

        [DataMember]
        public decimal CreditsInCirculationDollarAmount;

        [DataMember]
        public Balance Balance;

        [DataMember]
        public List<Transfer> LatestTransfers;

        [DataMember]
        public List<Transfer> UpcomingTransfers;

    }
}
