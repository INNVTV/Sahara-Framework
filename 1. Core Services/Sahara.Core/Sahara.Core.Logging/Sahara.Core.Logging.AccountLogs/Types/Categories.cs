using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Logging.AccountLogs.Types
{
    [DataContract]
    public enum CategoryType
    {
        [EnumMember]
        Authentication,

        [EnumMember]
        Account,

        [EnumMember]
        ApiKeys,

        [EnumMember]
        AccountUser,

        //[EnumMember]
        //Credits,

        //[EnumMember]
        //Schema,

        [EnumMember]
        Inventory,

        [EnumMember]
        Sales,

        [EnumMember]
        Customers,

        //[EnumMember]
        //Products,

        //[EnumMember]
        //ApplicationImage,
    }
}
