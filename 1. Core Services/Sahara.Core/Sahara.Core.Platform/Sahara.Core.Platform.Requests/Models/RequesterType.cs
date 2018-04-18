using System.Runtime.Serialization;

namespace Sahara.Core.Platform.Requests.Models
{
    [DataContract]
    public enum RequesterType
    {
        [EnumMember]
        PlatformUser,

        [EnumMember]
        AccountUser,

        [EnumMember]
        Exempt //<---used for platform systems where authenticated users are not used. Such as registration website, etc...
    }
}
