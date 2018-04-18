using AccountAdminSite.ApplicationImageFormatsService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountAdminSite.Models.ImageFormat
{
    /// <summary>
    /// Used by Settings to aggregate ALL image group types, groups and formats into ONE model
    /// </summary>
    public class ImageGroupTypeSettingsModel
    {
        public string ImageGroupTypeID { get; set; }
        public string ImageGroupTypeName { get; set; }
        public string ImageGroupTypeNameKey { get; set; }
        public List<ImageFormatGroupModel> ImageGroups { get; set; }
    }
}