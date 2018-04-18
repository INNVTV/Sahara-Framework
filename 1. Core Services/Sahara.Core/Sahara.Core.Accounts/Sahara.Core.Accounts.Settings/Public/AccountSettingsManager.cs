using Microsoft.Azure.Documents.Client;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Newtonsoft.Json;
using Sahara.Core.Accounts.DocumentModels;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts.Settings.Internal;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Common.ResponseTypes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Settings
{
    public static class AccountSettingsManager
    {
        public static AccountSettingsDocumentModel GetAccountSettings(Account account, bool useCachedVersion = true)
        {

            AccountSettingsDocumentModel settingsDocument = null;

            #region Create Cache & HashField

            string redisHashField = string.Empty;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            redisHashField = AccountSettingsHash.Fields.Document(account.AccountNameKey);


            #endregion

            if(useCachedVersion)
            {
                #region Get settings from cache

                try
                {
                    var redisValue = cache.HashGet(AccountSettingsHash.Key(), redisHashField);
                    if (redisValue.HasValue)
                    {
                        settingsDocument = JsonConvert.DeserializeObject<AccountSettingsDocumentModel>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }

            if(settingsDocument == null)
            {
                #region Get settings from DocumentDB 

                /**/
                //Get the DocumentDB Client
                //var client = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient;
                //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
                //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

                //Build a collection Uri out of the known IDs
                //(These helpers allow you to properly generate the following URI format for Document DB:
                //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);

                string sqlQuery = "SELECT * FROM Settings s WHERE s.id ='AccountSettings'";

                var settingsResults = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<AccountSettingsDocumentModel>(collectionUri.ToString(), sqlQuery);

                //var accountCollection = client.Crea

                //applicationImages = result.ToList();
                settingsDocument = settingsResults.AsEnumerable().FirstOrDefault();


                #endregion

                #region Get settings from storage partition (Retired, now using DocumentDB)

                /*

                //Get storage partition for this account and create a BLOB client:
                CloudBlobClient blobClient = Sahara.Core.Settings.Azure.Storage.GetStoragePartitionAccount(account.StoragePartition).CreateCloudBlobClient();

                //Create and set retry policy
                IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 8);
                blobClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

                //CloudBlobContainer blobContainer = blobClient.GetContainerReference(account.AccountID.ToString());
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(account.AccountNameKey);


                //Get reference to the text blob or create if not exists. 
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference("settings/" + "accountSettings.json");

                //Deserialize new version of settings document
                settingsDocument = JsonConvert.DeserializeObject<AccountSettingsDocumentModel>(blockBlob.DownloadText());
                */

                #endregion

                if (settingsDocument != null)
                {
                    #region Set into cache

                    try
                    {
                        cache.HashSet(AccountSettingsHash.Key(),
                            AccountSettingsHash.Fields.Document(redisHashField),
                            JsonConvert.SerializeObject(settingsDocument), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }

                    #endregion
                }
            }

            return settingsDocument;
        }


        /// <summary>
        /// We update ALL settings as one huge document. Makign it easier to add properties as the application(s) evolve)
        /// </summary>
        /// <param name="account"></param>
        /// <param name="accountSettingsDocumentModel"></param>
        /// <returns></returns>
        public static DataAccessResponseType UpdateAccountSettings(Account account, AccountSettingsDocumentModel accountSettingsDocumentModel)
        {
            var response = new DataAccessResponseType();

            try
            {

                //Get the DocumentDB Client
                //var client = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient;
                //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
                //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

                //Build a collection Uri out of the known IDs
                //(These helpers allow you to properly generate the following URI format for Document DB:
                //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
                //Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
                Uri documentUri = UriFactory.CreateDocumentUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, accountSettingsDocumentModel.Id);

                //Replace document:
                var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(documentUri, accountSettingsDocumentModel).Result;

                response.isSuccess = true;

                #region Storage Version (Retired & Moved Back to DocumentDB)


                /*

                    //Get storage partition for this account and create a BLOB client:
                    CloudBlobClient blobClient = Sahara.Core.Settings.Azure.Storage.GetStoragePartitionAccount(account.StoragePartition).CreateCloudBlobClient();

                    //Create and set retry policy
                    IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 8);
                    blobClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

                    //CloudBlobContainer blobContainer = blobClient.GetContainerReference(account.AccountID.ToString());
                    CloudBlobContainer blobContainer = blobClient.GetContainerReference(account.AccountNameKey);


                    //Get reference to the text blob or create if not exists. 
                    CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference("settings/" + "accountSettings.json");

                    //Serialize new version of settings document
                    blockBlob.UploadText(JsonConvert.SerializeObject(accountSettingsDocumentModel));
               */

                #endregion

            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            if(response.isSuccess)
            {
                if (response.isSuccess)
                {
                    //Clear Category Caches:
                    Caching.InvalidateSettingsCache(account.AccountNameKey);
                }
            }

            return response;
        }
    }
}
