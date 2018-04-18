using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Tags.TableEntities;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Tags
{
    internal static class Internal
    {
        internal static string TagsTableName = "applicationtags";

        internal static DataAccessResponseType StoreTag(TagTableEntity tagEntity, string storagePartition)
        {
            var response = new DataAccessResponseType();

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


            TableOperation operation = TableOperation.Insert((tagEntity as TableEntity));

            try
            {
                var tableResult = tagEntity.cloudTable.Execute(operation);
                response.isSuccess = true; //tableResult.;
            }
            catch
            {
                response.isSuccess = false; //tableResult.;
                response.ErrorMessage = "Tag exists";
            }
                

            return response;
        }

        internal static IEnumerable<TagTableEntity> RetrieveTags(string accountId, string storagePartition)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + TagsTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<TagTableEntity> query = new TableQuery<TagTableEntity>();

            return cloudTable.ExecuteQuery(query);
        }

        internal static DataAccessResponseType DeleteTag(Account account, string tagName)
        {
            var response = new DataAccessResponseType();

            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(account.StoragePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(account.AccountID.ToString()) + TagsTableName);

            cloudTable.CreateIfNotExists();

            try
            {
                TagTableEntity tagEntity = cloudTable.CreateQuery<TagTableEntity>().Where(t => t.PartitionKey == tagName).FirstOrDefault();

                TableResult deleteResult = cloudTable.Execute(TableOperation.Delete(tagEntity));

                response.isSuccess = true;
            }
            catch(Exception e)
            {
                PlatformLogManager.LogActivity(
                    CategoryType.Error,
                    ActivityType.Error_Exception,
                    e.Message,
                    "Exception while attempting to delete tag '" + tagName +"'",
                    account.AccountID.ToString(),
                    account.AccountName,
                    null,
                    null,
                    null,
                    null,
                    System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                    null
                );
            }

            return response;
        }

        /*
        internal static int RetrieveTagCount(string accountId)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + TagsTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<TagTableEntity> query = new TableQuery<TagTableEntity>();

            return cloudTable.ExecuteQuery(query).Count();
        }*/
    }
}
