using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Records.TableEntities
{

    public class ImageRecordTableEntity : TableEntity
    {
        
        public ImageRecordTableEntity()
        {

        }

        [DataMember]
        public string ObjectId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        [DataMember]
        public string ImageKey
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        [DataMember]
        public string ImageGroup { get; set; }
        [DataMember]
        public string ImageGroupKey { get; set; }

        [DataMember]
        public string ImageFormat { get; set; }
        [DataMember]
        public string ImageFormatKey { get; set; }

        //[DataMember]
        //public string Id { get; set; }

        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string StoragePath { get; set; }
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string ContainerName { get; set; }

        [DataMember]
        public string BlobPath { get; set; }

        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Width { get; set; }
    }
}
