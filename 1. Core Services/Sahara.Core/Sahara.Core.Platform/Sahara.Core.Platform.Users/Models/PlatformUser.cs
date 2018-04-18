using System;
using System.Runtime.Serialization;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Principal;

namespace Sahara.Core.Platform.Users.Models
{
    public class PlatformUserIdentity : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Photo { get; set; }
        public bool Active { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string FullName
        {
            get { return FirstName + " " + LastName; }

        }
    }

    [Serializable]
    [DataContract]
    public class PlatformUser
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string UserName { get; set; }


        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Role { get; set; }

        [DataMember]
        public string Photo { get; set; }

        [DataMember]
        public bool Active { get; set; }

        [DataMember]
        public DateTime? CreatedDate { get; set; }
    }

    
}
