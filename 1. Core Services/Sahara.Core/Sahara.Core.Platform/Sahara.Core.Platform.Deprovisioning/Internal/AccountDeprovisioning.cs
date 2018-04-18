using System;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using Sahara.Core.Platform.Partitioning.Public;
using System.Diagnostics;
using Sahara.Core.Common.DocumentModels;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.Azure.Search;

namespace Sahara.Core.Platform.Deprovisioning.Internal
{
    internal static class AccountDeprovisioning
    {
        //------------ Delete Account And Users ------

        internal static DataAccessResponseType DeleteAllAccountUsers(Account account)
        {
            var response = new DataAccessResponseType();

            foreach (AccountUser user in account.Users)
            {
                //We ignore verifications, all users will be purged:
                AccountUser outUser = null;
                AccountUserManager.DeleteUser(user.Id, false, out outUser);
            }

            return response;
        }

        internal static DataAccessResponseType DeleteAccount(Account account)
        {
            var response = new DataAccessResponseType();

            var SqlDeleteStatements = new Sql.Statements.DeleteStatements(account.AccountID.ToString(), account.SqlPartition);

            SqlDeleteStatements.DeleteAccount();

            return response;
        }

        //------------ Destroy Data ------

        internal static DataAccessResponseType DestroySqlSchemaAndTables(Account account)
        {
            var response = new DataAccessResponseType();

            try
            {
                return Sql.Statements.StoredProcedures.DestroySchema(account.SchemaName, account.SqlPartition);
            }
            catch(Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;

                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to destroy SQL schema and tables for : " + account.AccountName,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                PlatformLogManager.LogActivity(CategoryType.Error, ActivityType.Error_Exception, "Error destroying schema for: '" + account.AccountName + "' on: ' " + account.SqlPartition + "'", "AccountID: '" + account.AccountID + "' AccountNameKey: '" + account.AccountNameKey + "' Error: '" + e.Message + "'", account.AccountID.ToString(), account.AccountName);
            }

            return response;
        }

        internal static DataAccessResponseType DestroyTableStorageData(Account account)
        {
            var response = new DataAccessResponseType { isSuccess = false };

            //Loop through all tables named by schema for this account and delete
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(account.StoragePartition).CreateCloudTableClient();

            //Create and set retry policy
            IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 16);
            //IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

            IEnumerable<CloudTable> tables = cloudTableClient.ListTables(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(account.AccountID.ToString()));

            foreach(CloudTable table in tables)
            {
                try
                {
                    table.Delete();  
                }
                catch
                {
                    response.isSuccess = false;

                    PlatformLogManager.LogActivity(
                        CategoryType.ManualTask,
                        ActivityType.ManualTask_TableStorage,
                        "Table(s) Deletion Failed for schema" + Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(account.AccountID.ToString()),
                        "Please delete all tables for schema '" + Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(account.AccountID.ToString()) + "' manually.",
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString()
                    );
                }
                              
            }

            return response;
        }

        internal static DataAccessResponseType DestroyBlobStorageData(Account account)
        {
            var response = new DataAccessResponseType { isSuccess = true };

            //CloudBlobClient blobClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudBlobClient();
            CloudBlobClient blobClient = Sahara.Core.Settings.Azure.Storage.GetStoragePartitionAccount(account.StoragePartition).CreateCloudBlobClient();

            //Create and set retry policy
            IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 8);
            blobClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

            CloudBlobContainer blobContainer = blobClient.GetContainerReference(account.AccountID.ToString());
            //CloudBlobContainer blobContainer = blobClient.GetContainerReference(account.AccountNameKey);

            try
            {
                blobContainer.DeleteIfExists();
            }
            catch
            {
                response.isSuccess = false;

                PlatformLogManager.LogActivity(
                        CategoryType.ManualTask,
                        ActivityType.ManualTask_BlobStorage,
                        "Blob Container Deletion Failed",
                        "Please delete the top level container '" + account.AccountID.ToString() + "' manually.",
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString()
                    );
            }
            

            return response;
        }

        internal static DataAccessResponseType DestroyDocumentCollection(Account account)
        {
            var response = new DataAccessResponseType { isSuccess = false };

            try
            {
                // Create new stopwatch & begin timing tasks
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                #region Connect to the DocumentDB Client & Get Collection Object

                //Get the DocumentDB Client
                //var client = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient;
                //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
                //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            
                //Build a collection Uri out of the known IDs
                //(These helpers allow you to properly generate the following URI format for Document DB:
                //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
                
                #endregion

                #region Delete the document collection for this account

                    var result = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.DeleteDocumentCollectionAsync(collectionUri.ToString()).Result;
                        
                    #endregion

            }
            catch
            {
                #region Manual Task Instructions (if failure occurs)

                response.isSuccess = false;

                PlatformLogManager.LogActivity(
                        CategoryType.ManualTask,
                        ActivityType.ManualTask_DocumentDB,
                        "Deletion of Document Collection for closed account '" + account.AccountName + "' failed.",
                        "DocumentPartition: '" + account.DocumentPartition + "' partition for: '" + account.AccountName + "'. Please manually delete the '" + account.DocumentPartition + "' Document Collection",
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString()
                    );

                #endregion
            }

            return response;
        }

        internal static DataAccessResponseType DestroySearchIndexes(Account account)
        {
            var response = new DataAccessResponseType { isSuccess = false };

            try
            {
                // Create new stopwatch & begin timing tasks
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();


                #region Delete the search indexes for this account

                //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
                //searchServiceClient.Indexes.Delete(account.ProductSearchIndex);

                //Using partition for this account:
                SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(account.SearchPartition);
                searchServiceClient.Indexes.Delete(account.ProductSearchIndex);

                #endregion

            }
            catch
            {
                #region Manual Task Instructions (if failure occurs)

                response.isSuccess = false;

                PlatformLogManager.LogActivity(
                        CategoryType.ManualTask,
                        ActivityType.ManualTask_DocumentDB,
                        "Deletion of Search Indexes for closed account '" + account.AccountName + "' failed.",
                        "Search Indexes for: '" + account.AccountNameKey + "' will need to be manually deleted in the Azure portal.",
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString()
                    );

                #endregion
            }

            return response;
        }
    }
}
