using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Models
{
    [Serializable]
    [DataContract]
    public class AccountCreditCardInfo
    {
        [DataMember]
        public string CardName;
        [DataMember]
        public string CardBrand;

        [DataMember]
        public string Last4;
        [DataMember]
        public string ExpirationMonth;
        [DataMember]
        public string ExpirationYear;

        [DataMember]
        public string ExpirationDescription;
        [DataMember]
        public string CardDescription;
    }
}
