using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties.Models
{
    [DataContract]
    public class PropertySwatchModel
    {
        [DataMember]
        public Guid PropertySwatchID;

        [DataMember]
        public Guid PropertyID;

        [DataMember]
        public string PropertySwatchImage;

        [DataMember]
        public string PropertySwatchImageMedium;

        [DataMember]
        public string PropertySwatchImageSmall;

        [DataMember]
        public string PropertySwatchLabel;

        [DataMember]
        public string PropertySwatchNameKey;

        [DataMember]
        public int OrderID;

        [DataMember]
        public bool Visible;

        [DataMember]
        public DateTime CreatedDate;
    }
}
