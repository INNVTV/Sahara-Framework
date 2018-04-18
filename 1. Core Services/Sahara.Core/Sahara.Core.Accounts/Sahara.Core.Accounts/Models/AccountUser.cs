using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Principal;

namespace Sahara.Core.Accounts.Models
{
    public class AccountUserIdentity : IdentityUser
    {

        /* NOTES =================================
         * 
         * By default Sahara auto generates the UserName to be "EmailAddress_AccountID"
         * This allows for an email address to be used on multiple accounts both as a creator as well as an account user
         * 
         * Therefore all login attempts using Email/Password must also include the AccountNameKey (derived from a subdomain or typed in by the user) or an AccountID
         * 
         * This can be easilly refactored to align with your needs.
         * 
         */

        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public override string Email { get; set; } //<-- Used as login and is concatenated with AccountID to create the UserName "email@email.com_accountId"

        public string Photo { get; set; }

        
        public bool Active { get; set; }
        public bool AccountOwner { get; set; }

        //public bool Verified { get; set; }
        //public DateTime? VerifiedDate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public Guid AccountID { get; set; }
        public string AccountName { get; set; } //<-- Has a DB Trigger to Accounts
        public string AccountNameKey { get; set; } //<-- Used as SubDomain, Has a DB Trigger to Accounts

        public string FullName
        {
            get{return FirstName + " " + LastName;}

        }

        //Optional for certain scenarios:
        //public string LoginName { get; set; } //<-- Used as login in place of Email address, and as part of UserName "loginName_accountId" (Framework wide refactoring required)
        //public string DisplayName { get; set; }

    }

    /// <summary>
    /// Serializable version for WCF calls and lists of users within Account objects (Identity objects cannot be serialized)
    /// </summary>
    [Serializable]
    [DataContract]
    public class AccountUser
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
        public string Email { get; set; } //<-- Used as login and is concatenated with AccountID to create the UserName "email@email.com_accountId"

        [DataMember]
        public bool Active { get; set; }

        [DataMember]
        public bool AccountOwner { get; set; }

        [DataMember]
        public string Role { get; set; }

        [DataMember]
        public string Photo { get; set; }

        [DataMember]
        public Guid AccountID { get; set; }
        [DataMember]
        public string AccountName { get; set; } //<-- Has a DB Trigger to Accounts
        [DataMember]
        public string AccountNameKey { get; set; } //<-- Used as SubDomain, Has a DB Trigger to Accounts


        #region Removed for now

        //[DataMember]
        //public Account Account { get; set; } // <-- The current account the user is logged into, may notalways be returned

        //[DataMember]
        //public List<UserAccount> Accounts { get; set; } // <-- List of other accounts the user may belong to

        #endregion

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public DateTime? CreatedDate { get; set; }

    }

    [Serializable]
    [DataContract]
    public class UserAccount
    {
        [DataMember]
        public string AccountID { get; set; }

        [DataMember]
        public string AccountName { get; set; }

        [DataMember]
        public string AccountNameKey { get; set; }

        [DataMember]
        public string AccountURL { get; set; }

        [DataMember]
        public bool AccountOwner { get; set; }
    }

}
