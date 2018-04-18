using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Formats.Models
{
    public class ImageFormatGroupTypeModel
    {
        [DataMember]
        public Guid ImageFormatGroupTypeID;

        [DataMember]
        public string ImageFormatGroupTypeName;

        [DataMember]
        public string ImageFormatGroupTypeNameKey;

    }
}
