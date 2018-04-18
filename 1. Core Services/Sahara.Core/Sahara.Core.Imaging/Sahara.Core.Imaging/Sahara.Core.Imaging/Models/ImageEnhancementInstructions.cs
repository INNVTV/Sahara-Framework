using ImageProcessor.Imaging.Filters.Photo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Imaging.Models
{
    /// <summary>
    /// Instructions for enhancing a source image using ImageResizing Library, Null if not applicable.
    /// </summary>
    [DataContract]
    public class ImageEnhancementInstructions
    {
        /// <summary>
        /// -100 - 100 (Default: 0)
        /// </summary>
        [DataMember]
        public int Brightness = 0;

        /// <summary>
        /// -100 - 100 (Default: 0)
        /// </summary>
        [DataMember]
        public int Contrast = 0;


        /// <summary>
        /// 0 - 10 (Default: 0)
        /// </summary>
        [DataMember]
        public int Sharpen = 0;

        /// <summary>
        /// -100 - 100 (Default: 0)
        /// </summary>
        [DataMember]
        public int Saturation = 0;


        /// <summary>
        /// True/False (Default: false)
        /// </summary>
        [DataMember]
        public bool Sepia = false;

        /// <summary>
        /// True/False (Default: false)
        /// </summary>
        [DataMember]
        public bool Polaroid = false;

        /// <summary>
        /// True/False (Default: false)
        /// </summary>
        [DataMember]
        public bool Greyscale = false;


    }
}
