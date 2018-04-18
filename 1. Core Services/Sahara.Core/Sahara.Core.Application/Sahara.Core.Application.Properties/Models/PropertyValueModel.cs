using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties.Models
{
    [DataContract]
    public class PropertyValueModel
    {
        [DataMember]
        public Guid PropertyValueID;

        [DataMember]
        public Guid PropertyID;

        [DataMember]
        public string PropertyValueName;

        [DataMember]
        public string PropertyValueNameKey;

        [DataMember]
        public int OrderID;

        [DataMember]
        public bool Visible;

        [DataMember]
        public DateTime CreatedDate;
    }
}
