using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Platform.Users.TableEntities;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;

namespace Sahara.Core.Platform.Users.Internal
{
    internal static class PlatformInvitationsManager
    {
        internal static string PlatformInvitationsTableName = "invitedplatformusers";

        internal static string StoreInvitedUser(string email, string firstName, string lastName, string roleName)
        {

            var invitedPlatformUser = new PlatformInvitationsTableEntity();
            invitedPlatformUser.Email = email;
            invitedPlatformUser.FirstName = firstName;
            invitedPlatformUser.LastName = lastName;
            invitedPlatformUser.Role = roleName;

            TableOperation operation = TableOperation.Insert((invitedPlatformUser as TableEntity));
            invitedPlatformUser.cloudTable.Execute(operation);

            return invitedPlatformUser.InvitationKey;
        }

        internal static PlatformInvitationsTableEntity GetInvitedUser(string invitationKey)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(PlatformInvitationsTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<PlatformInvitationsTableEntity> query = new TableQuery<PlatformInvitationsTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, invitationKey));

            PlatformInvitationsTableEntity invitedAccountUser = cloudTable.ExecuteQuery(query).FirstOrDefault();

            return invitedAccountUser;

        }

        internal static IEnumerable<PlatformInvitationsTableEntity> GetInvitedUsers()
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(PlatformInvitationsTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<PlatformInvitationsTableEntity> query = new TableQuery<PlatformInvitationsTableEntity>();

            return cloudTable.ExecuteQuery(query);
        }

        internal static bool DeleteInvitedUser(string invitationKey)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(PlatformInvitationsTableName);

            cloudTable.CreateIfNotExists();

            var invitedAccountUser = GetInvitedUser(invitationKey);

            cloudTable.Execute(TableOperation.Delete(invitedAccountUser));

            return true;
        }


        internal static int GetInvitedUsersCount()
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(PlatformInvitationsTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<PlatformInvitationsTableEntity> query = new TableQuery<PlatformInvitationsTableEntity>();

            return cloudTable.ExecuteQuery(query).Count();
        }

        internal static bool DoesEmailExist(string email)
        {
            bool result = false;

            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(PlatformInvitationsTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<PlatformInvitationsTableEntity> query = new TableQuery<PlatformInvitationsTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, email));

            if (cloudTable.ExecuteQuery(query).Count() > 0)
            {
                result = true;
            }

            return result;
        }

    }
}
