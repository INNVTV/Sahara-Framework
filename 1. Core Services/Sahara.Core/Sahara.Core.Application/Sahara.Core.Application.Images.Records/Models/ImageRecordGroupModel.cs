using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Records.Models
{
    public class ImageRecordGroupModel
    {
        public ImageRecordGroupModel()
        {
            ImageRecords = new List<ImageRecordModel>();
        }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public string GroupNameKey { get; set; }

        [DataMember]
        public List<ImageRecordModel> ImageRecords { get; set; }
    }
}
