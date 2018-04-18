using System;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Platform.Billing.Internal;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Platform.Billing.TableEntities
{
    public class AutomaticDunningAttemptsTableEntity : TableEntity
    {

        public AutomaticDunningAttemptsTableEntity()
        {
        }

        public AutomaticDunningAttemptsTableEntity(string accountId, string storagePartition, string stripeChargeId)
        {
            StripeChargeID = stripeChargeId;

            //Create the cloudtable instance and  name for the entity operate against:
            //var cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + AutomaticDunningManager.AutomaticDunningTableName);
            cloudTable.CreateIfNotExists();

            //Ordered by Newest at top
            PartitionKey = string.Format("{0:d19}+{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
        }

        public string StripeChargeID
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string StripeEventID { get; set; }
        public string StripeSubscriptionID { get; set; }
        public string ChargeAmount { get; set; }
        public string FailureMessage { get; set; }
        // public DateTime AttemptTime { get; set; }

        public CloudTable cloudTable { get; set; }

    }
}
