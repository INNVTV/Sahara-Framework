using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Imaging.Models
{
    /// <summary>
    /// Coordinates for cropping a source image using ImageResizing Library, Null if not applicable.
    /// </summary>
    [DataContract]
    public class ImageCropCoordinates
    {
        [DataMember]
        public float Top;

        [DataMember]
        public float Left;

        [DataMember]
        public float Right;

        [DataMember]
        public float Bottom;
    }
}
