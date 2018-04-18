using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Formats.Models
{
    public class ImageFormatGroupModel
    {
        // Parent ----------

        //[DataMember]
        //public string ImageGroupTypeName;

        [DataMember]
        public string ImageFormatGroupTypeNameKey;


        // Self ----------

        [DataMember]
        public Guid ImageFormatGroupID;

        [DataMember]
        public string ImageFormatGroupName;

        [DataMember]
        public string ImageFormatGroupNameKey;

        [DataMember]
        public bool Visible;

        [DataMember]
        public bool AllowDeletion;

        //Children ------------

        [DataMember]
        public List<ImageFormatModel> ImageFormats;
    }
}
