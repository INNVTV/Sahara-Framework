using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Formats.Models
{
    public class ImageFormatModel
    {
        // ParentType ----------

        [DataMember]
        public string ImageFormatGroupTypeNameKey;

        // Parent ----------

        //[DataMember]
        //public string ImageGroupName;

        [DataMember]
        public string ImageFormatGroupNameKey;

        // Self ----------

        [DataMember]
        public Guid ImageFormatID;

        [DataMember]
        public string ImageFormatName;

        [DataMember]
        public string ImageFormatNameKey;

        [DataMember]
        public int Height;

        [DataMember]
        public int Width;

        [DataMember]
        public int OrderID;

        [DataMember]
        public bool Listing; //<-- If true we merge this into search, category and other listing reults for products

        [DataMember]
        public bool Visible;

        [DataMember]
        public bool Gallery;

        [DataMember]
        public bool AllowDeletion;

        /*
        [DataMember]
        public Size AspectRatio;

        public string GetAspectRatio()
        {
            return AspectRatio.X + ":" + AspectRatio.Y;
        }
        */
    }
}
