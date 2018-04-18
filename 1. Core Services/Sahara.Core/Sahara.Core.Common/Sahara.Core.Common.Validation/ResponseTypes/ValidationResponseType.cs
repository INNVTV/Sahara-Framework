using System.Runtime.Serialization;

namespace Sahara.Core.Common.Validation.ResponseTypes
{
    [DataContract]
    public class ValidationResponseType
    {
        [DataMember]
        public bool isValid { get; set; }

        [DataMember]
        public string validationMessage { get; set; }
    }
}
