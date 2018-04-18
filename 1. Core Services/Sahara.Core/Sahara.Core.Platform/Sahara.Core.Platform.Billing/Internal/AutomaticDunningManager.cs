using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Platform.Billing.TableEntities;
using System;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Platform.Billing.Internal
{
    internal static class AutomaticDunningManager
    {
        internal static string AutomaticDunningTableName = "automaticdunning";

        internal static bool StoreDunningAttempt(string accountId, string storagePartition, string stripeChargeId, string chargeAmount, string stripeSubscriptionId, string stripeEventId, string failureMessage)
        {

            var automaticDunningAttempts = new AutomaticDunningAttemptsTableEntity(accountId, storagePartition, stripeChargeId);
            automaticDunningAttempts.ChargeAmount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(chargeAmount);
            automaticDunningAttempts.StripeSubscriptionID = stripeSubscriptionId;
            automaticDunningAttempts.StripeEventID = stripeEventId;
            automaticDunningAttempts.FailureMessage = failureMessage;
            //automaticDunningAttempts.AttemptTime = DateTime.UtcNow;


            TableOperation operation = TableOperation.Insert((automaticDunningAttempts as TableEntity));
            automaticDunningAttempts.cloudTable.Execute(operation);

            return true;
        }


        internal static IEnumerable<AutomaticDunningAttemptsTableEntity> GetDunningAttempts(string accountId, string storagePartition)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + AutomaticDunningTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<AutomaticDunningAttemptsTableEntity> query = new TableQuery<AutomaticDunningAttemptsTableEntity>();

            return cloudTable.ExecuteQuery(query);
        }

        internal static int GetDunningAttemptsCount(string accountId, string storagePartition)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + AutomaticDunningTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<AutomaticDunningAttemptsTableEntity> query = new TableQuery<AutomaticDunningAttemptsTableEntity>();

            return cloudTable.ExecuteQuery(query).Count();
        }

        /// <summary>
        /// Used to destroy the dunning attempts table once payment issues have been resolved
        /// </summary>
        /// <param name="accountID"></param>
        /// <returns></returns>
        internal static bool ClearDunningAttempts(string accountID, string storagePartition)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();
            
            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 6);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountID) + AutomaticDunningTableName);

            //Delete the entire table
            cloudTable.DeleteIfExists();

            return true;
        }
        
    }
}
