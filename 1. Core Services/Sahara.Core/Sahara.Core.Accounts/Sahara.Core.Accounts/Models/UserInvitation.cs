using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Models
{
    [DataContract]
    public class UserInvitation
    {
        [DataMember]
        public string InvitationKey { get; set; }

        [DataMember]
        public string AccountID { get; set; }

        [DataMember]
        public string AccountName { get; set; }

        [DataMember]
        public string AccountNameKey { get; set; }

        [DataMember]
        public string AccountLogoUrl { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Role { get; set; }

        [DataMember]
        public bool Owner { get; set; }

    }
}
