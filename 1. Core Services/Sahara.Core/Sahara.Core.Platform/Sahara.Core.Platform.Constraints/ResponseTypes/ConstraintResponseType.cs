using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Constraints.ResponseTypes
{
    [DataContract]
    public class ConstraintResponseType
    {
        [DataMember]
        public bool isConstrained { get; set; }

        [DataMember]
        public string constraintMessage { get; set; }
    }
}
