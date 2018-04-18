using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties.Models
{
    [DataContract]
    public enum PropertyListType
    {
        [EnumMember]
        All,

        [EnumMember]
        Listings,

        [EnumMember]
        Details,

        [EnumMember]
        Featured
    }
}
