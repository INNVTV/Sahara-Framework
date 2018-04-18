using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Types
{
    [DataContract]
    public enum ProductPropertyUpdateType
    {
        [EnumMember]
        Replace,

        [EnumMember]
        Append
    }
}
