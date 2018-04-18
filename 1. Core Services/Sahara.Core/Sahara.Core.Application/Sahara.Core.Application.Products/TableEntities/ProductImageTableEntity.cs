//using Microsoft.WindowsAzure.Storage.RetryPolicies;
//using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Products.TableEntities
{/* Depricated
    public class ProductImageTableEntity : TableEntity
    {
        public ProductImageTableEntity()
        {

        }

        public ProductImageTableEntity(string accountId, string productId, string imageGroupNameKey, string imageFormatNameKey)
        {
            //Genrate ProductImageKey
            var rowKey = imageGroupNameKey + "-" + imageFormatNameKey;

            PartitionKey = productId;
            RowKey = rowKey;

            //Create the cloudtable instance and  name for the entity operate against:
            var cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ProductImagesTableStorage.ProductImagesTableName);
            cloudTable.CreateIfNotExists();
        }

        public string ProductId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }


        public string ProductImageKey
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }

        public CloudTable cloudTable { get; set; }
    }*/
}
