using System.Runtime.Serialization;

namespace Sahara.Core.Platform.Requests.ResponseTypes
{
    [DataContract]
    public class RequestResponseType
    {
        [DataMember]
        public bool isApproved { get; set; }

        [DataMember]
        public string requestMessage { get; set; }
    }
}
