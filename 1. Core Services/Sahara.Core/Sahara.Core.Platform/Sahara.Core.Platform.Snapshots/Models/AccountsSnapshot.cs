using Sahara.Core.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Snapshots.Models
{
    [DataContract]
    public class AccountsSnapshot
    {
        [DataMember]
        public PlatformSnapshotAccountCounts Counts;

        [DataMember]
        public List<Account> PastDue;
        [DataMember]
        public List<Account> Unpaid;
        [DataMember]
        public List<Account> LatestRegistered;
        [DataMember]
        public List<AccountClosure> LatestClosures;


        [DataMember]
        public List<Account> ScheduledForClosure;
        [DataMember]
        public List<Account> RequiresClosureApproval;


    }


    [DataContract]
    public class PlatformSnapshotAccountCounts
    {
        [DataMember]
        public int Total;
        [DataMember]
        public int Subscribed;

        [DataMember]
        public int PaidUp; //<---Subscribed & PaidUp
        [DataMember]
        public int Unpaid; //<---Subscribed & Unpaid
        [DataMember]
        public int PastDue; //<---Subscribed & PastDue


        [DataMember]
        public int Delinquent; //<---Both unpaid & past_due

        [DataMember]
        public int Unprovisioned;
        [DataMember]
        public int Inactive;



        [DataMember]
        public int GlobalUsersCount;

        [DataMember]
        public int Signups_Last24Hours;
        [DataMember]
        public int Signups_Last3Days;
        [DataMember]
        public int Signups_Last7Days;
        [DataMember]
        public int Signups_Last30Days;
    }

    [DataContract]
    public class AccountClosure
    {
        [DataMember]
        public DateTime Timestamp;

        [DataMember]
        public string Decription;

        [DataMember]
        public Account Account;

    }
}
