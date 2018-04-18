using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Application.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Tags.TableEntities
{
    /// <summary>
    /// A generic representation of the various internal log entities for public consumption. used as a return type for log retreival methods 
    /// Used only for log retreival, NOT for log creation
    /// </summary>
    public class TagTableEntity : TableEntity
    {
        public TagTableEntity()
        {
        }

        public TagTableEntity(string accountId, string storagePartition, string tagName)
        {
            //Generate Tag Id
            //TagID = Guid.NewGuid().ToString();
            TagID = tagName.ToLower();//.Replace(" ", "");

           // TagNameKey = tagName.ToLower();
            TagName = tagName;

            //Create the cloudtable instance and  name for the entity operate against:
            //var cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


            cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.TagsTableName);
            cloudTable.CreateIfNotExists();

            //RowKey = string.Format("{0:d19}+{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
        }

        public string TagName
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string TagID
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        //public string TagName { get; set; }

        public CloudTable cloudTable { get; set; }
    }
}
