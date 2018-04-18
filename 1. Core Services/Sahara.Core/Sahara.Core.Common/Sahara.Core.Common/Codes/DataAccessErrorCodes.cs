using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Codes
{
    [DataContract]
    public enum DataAccessErrorCode
    {
        [DataMember]
        Constraint,

        //[DataMember]
        //Request,

        [DataMember]
        Other
    }
}
