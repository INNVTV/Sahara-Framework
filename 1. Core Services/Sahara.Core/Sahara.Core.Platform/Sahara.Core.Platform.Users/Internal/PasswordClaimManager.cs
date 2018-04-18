using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Platform.Users.TableEntities;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;

namespace Sahara.Core.Platform.Users.Internal
{
    internal class PasswordClaimManager
    {
        internal static string PasswordClaimTableName = "passwordclaims";

        internal static string StorePasswordClaim(string email)
        {

            //Clear any claims for the email if exists:
            ClearClaimIfExist(email);

            var passworClaim = new PasswordClaimTableEntity();
            passworClaim.Email = email;

            TableOperation operation = Microsoft.WindowsAzure.Storage.Table.TableOperation.Insert((passworClaim as TableEntity));
            passworClaim.cloudTable.Execute(operation);

            return passworClaim.PasswordClaimKey;
        }

        internal static IEnumerable<PasswordClaimTableEntity> GetPasswordClaims()
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(PasswordClaimTableName);

            cloudTable.CreateIfNotExists();

            var query = new TableQuery<PasswordClaimTableEntity>();

            var passwordClaims = cloudTable.ExecuteQuery(query);

            return passwordClaims;
        }

        internal static PasswordClaimTableEntity GetPasswordClaim(string passwordClaimKey)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(PasswordClaimTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<PasswordClaimTableEntity> query = new TableQuery<PasswordClaimTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, passwordClaimKey));

            PasswordClaimTableEntity passwordClaim = cloudTable.ExecuteQuery(query).FirstOrDefault();

            return passwordClaim;

        }


        internal static bool DeletePasswordClaim(string passwordClaimKey)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(PasswordClaimTableName);

            cloudTable.CreateIfNotExists();

            var passwordClaim = GetPasswordClaim(passwordClaimKey);

            cloudTable.Execute(TableOperation.Delete(passwordClaim));

            return true;
        }


        private static bool ClearClaimIfExist(string email)
        {
            bool result = false;

            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(PasswordClaimTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<PasswordClaimTableEntity> query = new TableQuery<PasswordClaimTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, email));

            var passwordClaim = cloudTable.ExecuteQuery(query).FirstOrDefault();

            if (passwordClaim != null)
            {
                DeletePasswordClaim(passwordClaim.PasswordClaimKey);
            }

            return result;
        }
    }
}
