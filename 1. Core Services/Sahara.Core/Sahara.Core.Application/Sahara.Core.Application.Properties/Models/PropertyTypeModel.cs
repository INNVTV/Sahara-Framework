using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties.Models
{
    [DataContract]
    public class PropertyTypeModel
    {
        [DataMember]
        public Guid PropertyTypeID;

        [DataMember]
        public string PropertyTypeName;

        [DataMember]
        public string PropertyTypeNameKey;

        [DataMember]
        public string PropertyTypeDescription;
    }
}
