using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Users.Models
{
    [DataContract]
    public class PlatformPasswordResetClaim
    {
        [DataMember]
        public string PasswordClaimKey { get; set; }

        [DataMember]
        public string Email { get; set; }

    }
}
