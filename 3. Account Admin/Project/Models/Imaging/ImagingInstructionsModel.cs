using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountAdminSite.Models.Imaging
{
    /// <summary>
    /// SourceFileModel represents a source image stored in intermediary storage for immediate cropping/enhancement instruction development.
    /// These instructions are sent to CoreServices (along with the SourceFile info) for generation of final images.
    /// Core Services Custodian clears folders based on dates at set intervals.
    /// </summary>
    public class ImagingInstructionsModel
    {
        public string SourceContainerName { get; set; }
        public string ImageID { get; set; }
        public string FileName { get; set; }

        public string Type { get; set; } //<-- jpg, png or gif

        public string ObjectType { get; set; }
        public string ObjectID { get; set; }
        public string ImageGroupNameKey { get; set; }
        public string ImageFormatNameKey { get; set; }


        public int FormatWidth { get; set; }
        public int FormatHeight { get; set; }

        public int SourceWidth { get; set; }
        public int SourceHeight { get; set; }
    }

}