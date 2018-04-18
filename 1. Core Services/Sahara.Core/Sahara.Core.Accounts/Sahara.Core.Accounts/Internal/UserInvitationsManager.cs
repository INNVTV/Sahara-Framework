using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Accounts.TableEntities;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;

namespace Sahara.Core.Accounts.Internal
{
    internal static class UserInvitationsManager
    {
        internal static string UserInvitationsTableName = "invitedusers";

        internal static string StoreInvitedUser(string accountID, string storagePartition, string email, string firstName, string lastName, string roleName, bool isOwner)
        {

            var invitedAccountUser = new UserInvitationsTableEntity(accountID, storagePartition);
            invitedAccountUser.Email = email;
            invitedAccountUser.FirstName = firstName;
            invitedAccountUser.LastName = lastName;
            invitedAccountUser.Role = roleName;
            invitedAccountUser.Owner = isOwner;

            TableOperation operation = TableOperation.Insert((invitedAccountUser as TableEntity));
            invitedAccountUser.cloudTable.Execute(operation);

            return invitedAccountUser.InvitationKey;
        }

        internal static UserInvitationsTableEntity GetInvitedUser(string accountID, string storagePartition, string invitationKey)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountID) + UserInvitationsTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<UserInvitationsTableEntity> query = new TableQuery<UserInvitationsTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, invitationKey));

            UserInvitationsTableEntity invitedAccountUser = cloudTable.ExecuteQuery(query).FirstOrDefault();

            return invitedAccountUser;
            
        }

        internal static IEnumerable<UserInvitationsTableEntity> GetInvitedUsers(string accountID, string storagePartition)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountID) + UserInvitationsTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<UserInvitationsTableEntity> query = new TableQuery<UserInvitationsTableEntity>();

            return cloudTable.ExecuteQuery(query);
        }

        internal static bool DeleteInvitedUser(string accountID, string storagePartition, string invitationKey)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountID) + UserInvitationsTableName);

            cloudTable.CreateIfNotExists();

            var invitedAccountUser = GetInvitedUser(accountID, storagePartition, invitationKey);

            cloudTable.Execute(TableOperation.Delete(invitedAccountUser));

            return true;
        }


        internal static int GetInvitedUsersCount(string accountID, string storagePartition)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountID) + UserInvitationsTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<UserInvitationsTableEntity> query = new TableQuery<UserInvitationsTableEntity>();

            return cloudTable.ExecuteQuery(query).Count();
        }

        internal static bool DoesEmailExist(string accountID, string storagePartition, string email)
        {
            bool result = false;

            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountID) + UserInvitationsTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<UserInvitationsTableEntity> query = new TableQuery<UserInvitationsTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, email));

            if(cloudTable.ExecuteQuery(query).Count() > 0)
            {
                result = true;
            }

            return result;
        }
        
    }
}
