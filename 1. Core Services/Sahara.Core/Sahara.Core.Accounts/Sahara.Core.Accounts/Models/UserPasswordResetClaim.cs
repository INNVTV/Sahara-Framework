using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Models
{
    [DataContract]
    public class UserPasswordResetClaim
    {
        [DataMember]
        public string PasswordClaimKey { get; set; }

        [DataMember]
        public string Email { get; set; }

    }
}
