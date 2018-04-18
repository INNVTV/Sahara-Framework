using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.ApiKeys.Models
{
    [DataContract]
    public class ApiKeyModel
    {
        [DataMember]
        public Guid ApiKey;

        [DataMember]
        public string Name;

        [DataMember]
        public string Description;

        [DataMember]
        public DateTime CreatedDate;
    }
}
