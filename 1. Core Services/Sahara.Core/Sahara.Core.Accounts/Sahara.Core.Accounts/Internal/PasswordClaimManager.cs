using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Accounts.TableEntities;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;

namespace Sahara.Core.Accounts.Internal
{
    internal class PasswordClaimManager
    {
        internal static string PasswordClaimTableName = "passwordclaims";

        internal static string StorePasswordClaim(string accountID, string storagePartition, string email)
        {

            //Clear any claims for the email on the account if exists:
            ClearClaimIfExist(accountID, storagePartition, email);

            var passworClaim = new PasswordClaimTableEntity(accountID, storagePartition);
            passworClaim.Email = email;

            TableOperation operation = TableOperation.Insert((passworClaim as TableEntity));
            passworClaim.cloudTable.Execute(operation);

            return passworClaim.PasswordClaimKey;
        }



        internal static IEnumerable<PasswordClaimTableEntity> GetPasswordClaims(string accountId, string storagePartition)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + PasswordClaimTableName);

            cloudTable.CreateIfNotExists();

            var query = new TableQuery<PasswordClaimTableEntity>();

            var passwordClaims = cloudTable.ExecuteQuery(query);

            return passwordClaims;

        }

        internal static PasswordClaimTableEntity GetPasswordClaim(string accountId, string storagePartition, string passwordClaimKey)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + PasswordClaimTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<PasswordClaimTableEntity> query = new TableQuery<PasswordClaimTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, passwordClaimKey));

            PasswordClaimTableEntity passwordClaim = cloudTable.ExecuteQuery(query).FirstOrDefault();

            return passwordClaim;

        }


        internal static bool DeletePasswordClaim(string accountID, string storagePartition, string passwordClaimKey)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountID) + PasswordClaimTableName);

            cloudTable.CreateIfNotExists();

            var passwordClaim = GetPasswordClaim(accountID, storagePartition, passwordClaimKey);

            cloudTable.Execute(TableOperation.Delete(passwordClaim));

            return true;
        }


        private static bool ClearClaimIfExist(string accountID, string storagePartition, string email)
        {
            bool result = false;

            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 6);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountID) + PasswordClaimTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<PasswordClaimTableEntity> query = new TableQuery<PasswordClaimTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, email));

            var passwordClaim = cloudTable.ExecuteQuery(query).FirstOrDefault();

            if (passwordClaim != null)
            {
                DeletePasswordClaim(accountID, storagePartition, passwordClaim.PasswordClaimKey);
            }

            return result;
        }
    }
}
