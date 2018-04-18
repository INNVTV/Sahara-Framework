using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Imaging_API.Models
{
    public class ImagePreviewResponse
    {
        public bool isSuccess { get; set; }
        public string ImageID { get; set; }
        public string FileName { get; set; }
        public string SourceFile { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
    }
}