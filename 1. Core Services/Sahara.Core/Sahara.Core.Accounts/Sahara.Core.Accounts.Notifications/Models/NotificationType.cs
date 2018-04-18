using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Notifications.Models
{
    [DataContract]
    public enum NotificationType
    {
        [EnumMember]
        Information,

        [EnumMember]
        Success,

        [EnumMember]
        Warning,

        [EnumMember]
        Alert
    }
}
