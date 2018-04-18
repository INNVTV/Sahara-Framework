using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Imaging.Models
{
    /// <summary>
    /// Contains information required to access source files within the intermediate storage container
    /// As well as info for image record for the object the mage is being added to
    /// </summary>
    public class ImageProcessingManifestModel
    {
        // "intermediarystorage/12-11-2015/xxx-xxx-xxx-xxx.jpg"

        [DataMember]
        public string SourceContainerName { get; set; } //<-- Date format: "12-11-2015" (Get's cleaned up on a schedule by custodian)

        [DataMember]
        public string FileName { get; set; } //<-- ImageID + File Extension

        [DataMember]
        public string Type { get; set; } // = "jpg"; //<-- jpg, gif, png

        [DataMember]
        public int Quality { get; set; } // = 90; //<-- 70,80,90,100

        [DataMember]
        public string GroupTypeNameKey { get; set; }

        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public string GroupNameKey { get; set; }

        [DataMember]
        public string FormatNameKey { get; set; }

        [DataMember]
        public string ImageId { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }


    }
}
