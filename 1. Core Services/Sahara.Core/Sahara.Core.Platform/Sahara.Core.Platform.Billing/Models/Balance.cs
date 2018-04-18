using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Billing.Models
{
    [DataContract]
    public class Balance
    {
        [DataMember]
        public List<Decimal> Available { get; set; }

        [DataMember]
        public List<Decimal> Pending { get; set; }
    }
}
