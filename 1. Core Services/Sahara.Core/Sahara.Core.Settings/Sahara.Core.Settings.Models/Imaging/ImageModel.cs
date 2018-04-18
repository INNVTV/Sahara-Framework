using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Models.Imaging
{
    /// <summary>
    /// Used to generate static classes representing image specs
    /// </summary>
    public class ImageFormatModel
    {
        public ImageFormatModel()
        {

        }


        /// <summary>
        /// Determines if we lock the size of this image one size only
        /// If SetSizes DO NOT EXIST: Image will be cropped based on aspect ratio only (one image will be used and scaled on UI)
        /// if SetSizes EXIST: Image will be cropped by set size or sizes (one or more images will be generated and named by set size)
        /// </summary>
        public List<Size> SetSizes; //<-- if no set set sizes exist we only resize an image by the aspect ratio


        public List<Size> ThumbnailSizes; //<-- if no thumbnails are set thumnails are skipped 

        /// <summary>
        /// Container or folder location for saving to blob storage
        /// </summary>
        public string ParentName;

        /// <summary>
        /// Aspec ratio used for cropping
        /// </summary>
        public Size AspectRatio;

        /// <summary>
        /// Minimum size accepted for uploading this image
        /// </summary>
        public Size MinSize;

        /// <summary>
        /// Maximum size accepted for uploading this image
        /// </summary>
        public Size MaxSize;

        /// <summary>
        /// Calculate the image pixel height based on width
        /// </summary>
        //public CalculateAspectRatioHeight()
        //{

        //}

        /// <summary>
        /// Print out the aspect ratio
        /// </summary>
        /// <returns></returns>
        public string GetAspectRatio()
        {
            return AspectRatio.X + ":" + AspectRatio.Y ;
        }

        /// <summary>
        /// print out the minimum size
        /// </summary>
        /// <returns></returns>
        public string GetMinSize()
        {
            return MinSize.X + "x" + MinSize.Y;
        }

        /// <summary>
        /// print out the maximum size
        /// </summary>
        /// <returns></returns>
        public string GetMaxSize()
        {
            return MaxSize.X + "x" + MaxSize.Y;
        }
    }


    public class Size
    {
        public int X = 0;
        public int Y = 0;

        public string GetSize()
        {
            return X + "x" + Y;
        }
    }
}
