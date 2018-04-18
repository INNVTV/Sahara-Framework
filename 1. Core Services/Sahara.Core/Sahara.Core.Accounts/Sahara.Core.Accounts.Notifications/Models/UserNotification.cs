using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Notifications.Models
{
    [Serializable]
    [DataContract]
    public class UserNotification
    {
        [DataMember]
        public string NotificationType { get; set; } //<-- (Partial) TableName

        [DataMember]
        public string NotificationId { get; set; } //<-- RowKey

        [DataMember]
        public string Status { get; set; } //<-- PartitionKey

        //[DataMember]
        //public bool Read { get; set; } 

        [DataMember]
        public bool Expired { get; set; }

        [DataMember]
        public string NotificationMessage { get; set; }

        [DataMember]
        public double ExpirationMinutes { get; set; }


        [DataMember]
        public DateTime CreatedDateTime { get; set; }

        [DataMember]
        public DateTime ExpirationDateTime { get; set; }

        [DataMember]
        public DateTime LastUpdatedDateTime { get; set; }

    }
}
