using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Logging.PlatformLogs.Types
{
    [DataContract]
    public enum CategoryType
    {
        [EnumMember]
        Error,

        [EnumMember]
        Platform,

        [EnumMember]
        Account,

        [EnumMember]
        Billing,

        [EnumMember]
        Imaging,

        [EnumMember]
        StripeEvent,


        [EnumMember]
        Worker,

        [EnumMember]
        Custodian,

        [EnumMember]
        Authentication,

        [EnumMember]
        PlatformUser,

        [EnumMember]
        Registration,

        [EnumMember]
        WCFHost,

        [EnumMember]
        GarbageCollection,

        [EnumMember]
        ManualTask, //<-- For manual tasks

        [EnumMember]
        DataInjection,

        [EnumMember]
        Trace, //<-- For trace statements

    }
}
