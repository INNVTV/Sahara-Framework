//using Sahara.Core.Application.Images.Formats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Records.Models
{
    public class ImageRecordModel
    {
        public ImageRecordModel()
        {
            GalleryImages = new List<ImageRecordGalleryModel>();
        }

        [DataMember]
        public string FormatName { get; set; }

        [DataMember]
        public string FormatNameKey { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string ContainerName { get; set; }

        [DataMember]
        public string BlobPath { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string FileName { get; set; }

        #region Alt Sizes

        [DataMember]
        public string BlobPath_sm { get; set; }

        [DataMember]
        public string Url_sm { get; set; }

        [DataMember]
        public string FilePath_sm { get; set; }

        [DataMember]
        public string FileName_sm { get; set; }

        [DataMember]
        public string BlobPath_xs { get; set; }

        [DataMember]
        public string Url_xs { get; set; }

        [DataMember]
        public string FilePath_xs { get; set; }

        [DataMember]
        public string FileName_xs { get; set; }

        #endregion

        [DataMember]
        public List<ImageRecordGalleryModel> GalleryImages { get; set;}

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public int Width { get; set; }

        //[DataMember]
        //public bool Gallery;

        //[DataMember]
        //public Size AspectRatio;

        //public string GetAspectRatio()
        //{
        //return AspectRatio.X + ":" + AspectRatio.Y;
        //}
    }

    public class ImageRecordGalleryModel //: ImageFormatModel <-- Removed to avoid circular references later
    {

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string BlobPath { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string FileName { get; set; }

        #region Alt Sizes

        [DataMember]
        public string Url_sm { get; set; }

        [DataMember]
        public string BlobPath_sm { get; set; }

        [DataMember]
        public string FilePath_sm { get; set; }

        [DataMember]
        public string FileName_sm { get; set; }

        //--------

        [DataMember]
        public string Url_xs { get; set; }

        [DataMember]
        public string BlobPath_xs { get; set; }

        [DataMember]
        public string FilePath_xs { get; set; }

        [DataMember]
        public string FileName_xs { get; set; }

        #endregion
    }
}
