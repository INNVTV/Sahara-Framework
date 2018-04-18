using System;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Platform.Partitioning;
using Sahara.Core.Platform.Provisioning.Internal;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using System.Text;
using Sahara.Core.Accounts.DocumentModels;
using Sahara.Core.Platform.Partitioning.Public;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.IO;
using Sahara.Core.Application.Search;
using System.Collections.Generic;
using System.Linq;
using Sahara.Core.Platform.Partitioning.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Newtonsoft.Json;
using Sahara.Core.Settings.Models.Partitions;
using Sahara.Core.Common.Services.CloudFlare.Public;

namespace Sahara.Core.Platform.Provisioning
{
    public static class ProvisioningManager
    {
        public static DataAccessResponseType ProvisionAccount(Account account)
        {
            var response = new DataAccessResponseType();

            #region Pre Provisioning Verification

            bool _documentPartitioning = true;      //<-- If false will skip provisioning DocumentDB resources for accounts
            bool _searchPartitioning = true;      //<-- If false will skip provisioning Search resources for accounts
            bool _storagePartitioning = true;      //<-- If false will skip provisioning Search resources for accounts
            bool _sqlPartitioning = true;           //<-- If false will skip provisioning a SQL Location and SchemeName for accounts

            StoragePartition storagePartition = null; //<-- Chosen partition for this account
            SearchPartition searchPartition = null; //<-- Chosen partition for this account

            //Make sure account isn't already provisioned
            if (account.Provisioned)
            {
                
                response.isSuccess = false;
                response.ErrorMessage = "Account is already provisioned!";

                return response;
            }
            if (account.StripeSubscriptionID == null || account.StripeCustomerID == null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This account has not been assigned a payment plan or a Stripe CustomerID";

                return response;
            }

            //If Account object is passed in without users get all/initial user(s):
            if (account.Users == null)
            {
                account.Users = AccountUserManager.GetUsers(account.AccountID.ToString());
            }

            #region Ensure that there is a storage partition available and select next available spot

            if(_storagePartitioning)
            {
                var storagePartitions = StoragePartitioningManager.GetStoragePartitions();
                

                //Sort with lowest tenant count at the top:
                storagePartitions = storagePartitions.OrderBy(o => o.TenantCount).ToList();

                if(storagePartitions.Count > 0)
                {
                    if (storagePartitions[0].TenantCount >= Settings.Platform.Partitioning.MaximumTenantsPerStorageAccount)
                    {
                        response.isSuccess = false;
                        response.ErrorMessage = "There are no storage partitions available for this account! Please create one before attempting to provision.";

                        //Reset account to inactive so you can restart partitioning sequence after partition hopper has additional partitions added
                        AccountManager.UpdateAccountActiveState(account.AccountID.ToString(), false);

                        return response;
                    }
                    else
                    {
                        //Assign storage partition:
                        storagePartition = storagePartitions[0];
                    }
                }
                else
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "There are no storage partitions available on this platform! Cannot provision any accounts!";

                    //Reset account to inactive so you can restart partitioning sequence after partition hopper has additional partitions added
                    AccountManager.UpdateAccountActiveState(account.AccountID.ToString(), false);

                    return response;
                }

            }



            #endregion

            #region Ensure that there is a search partition available and select next available spot

            if(_searchPartitioning)
            {
                //Get search plan type for this plan tier
                string searchPlan = account.PaymentPlan.SearchPlan;

                //Get list of search partitions available with this plan type
                var searchPartitions = SearchPartitioningManager.GetSearchPartitions(searchPlan);

                int maxTenantsAllowed = Int32.Parse((searchPlan.Substring(searchPlan.LastIndexOf("-") + 1)));

                /* MAx Tenatnts are now pulled from the SarchPlan name
                 * 
                int maxTenantsAllowed = 0;

                if(searchPlan == "Basic")
                {
                    maxTenantsAllowed = Settings.Platform.Partitioning.MaximumTenantsPerBasicSearchServiceShared;
                }
                else if (searchPlan == "Basic-Dedicated")
                {
                    maxTenantsAllowed = Settings.Platform.Partitioning.MaximumTenantsPerBasicSearchServiceDedicated;
                }
                else if(searchPlan == "S1")
                {
                    maxTenantsAllowed = Settings.Platform.Partitioning.MaximumTenantsPerS1SearchServiceShared;
                }
                else if (searchPlan == "S1-Dedicated")
                {
                    maxTenantsAllowed = Settings.Platform.Partitioning.MaximumTenantsPerS1SearchServiceDedicated;
                }
                else if (searchPlan == "S2")
                {
                    maxTenantsAllowed = Settings.Platform.Partitioning.MaximumTenantsPerS2SearchServiceShared;
                }
                else if (searchPlan == "S2-Dedicated")
                {
                    maxTenantsAllowed = Settings.Platform.Partitioning.MaximumTenantsPerS2SearchServiceDedicated;
                }
                else if(searchPlan == "Free")
                {
                    maxTenantsAllowed = Settings.Platform.Partitioning.MaximumTenantsPerFreeSearchService;
                }
                */

                //Sort with lowest tenant count at the top:
                searchPartitions = searchPartitions.OrderBy(o => o.TenantCount).ToList();

                if(searchPartitions.Count > 0)
                {
                    if (searchPartitions[0].TenantCount >= maxTenantsAllowed)
                    {
                        response.isSuccess = false;
                        response.ErrorMessage = "There are no '" + searchPlan + "' search partitions available for this account! Please create one before attempting to provision.";

                        //Reset account to inactive so you can restart partitioning sequence after partition hopper has additional partitions added
                        AccountManager.UpdateAccountActiveState(account.AccountID.ToString(), false);

                        return response;
                    }
                    else
                    {
                        //Assign storage partition:
                        searchPartition = searchPartitions[0];
                    }
                }
                else
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "There are no '" + searchPlan + "' search partitions available on this platform! Cannot provision any accounts!";

                    //Reset account to inactive so you can restart partitioning sequence after partition hopper has additional partitions added
                    AccountManager.UpdateAccountActiveState(account.AccountID.ToString(), false);

                    return response;
                }
            }





            #endregion

            #endregion

            #region Account Partitioning

            #region Document Database Partitioning (REMOVED)

            if (_documentPartitioning)
                {
                //Connect to the document client & get the database selfLink
                //var client = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient;

                //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync().ConfigureAwait(false);
                //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

                //var dataBaseSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;

                //STEP 1: Get or create the next available document partition for the 'Free' tier
                var partitioningResult = DocumentPartitioningManager.CreateDocumentCollectionAccountPartition(account.AccountNameKey, Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient, Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId);

                    if (partitioningResult.isSuccess == true)
                    {
                        DocumentCollection nextAvailablePartitionCollection = (DocumentCollection)partitioningResult.ResponseObject;

                    #region STEP 4: Add Account Settings Document for this account on the collection

                    var accountSettingsDocumentCreated = false;
                    Exception accountSettingsException = null;

                    try
                    {
                        var accountSettingsDocument = new AccountSettingsDocumentModel { Id = "AccountSettings" };

                        accountSettingsDocument.ContactSettings = new ContactSettingsModel();
                        accountSettingsDocument.ContactSettings.ContactInfo = new ContactInfoModel();
                        accountSettingsDocument.SalesSettings = new SalesSettingsModel();

                        //Default LeadLabels
                        accountSettingsDocument.SalesSettings.LeadLabels = new List<string>();
                        accountSettingsDocument.SalesSettings.LeadLabels.Add("New");
                        accountSettingsDocument.SalesSettings.LeadLabels.Add("Archive");
                        accountSettingsDocument.SalesSettings.LeadLabels.Add("Deleted");

                        accountSettingsDocument.Theme = "Light"; //<-- Default Theme
                        accountSettingsDocument.SalesSettings.ButtonCopy = "I'm interested!"; //<-- Default Theme
                        accountSettingsDocument.SalesSettings.DescriptionCopy = "Fill out our contact form and a member of our team will contact you directly.";



                        Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentAsync(nextAvailablePartitionCollection.SelfLink, accountSettingsDocument).ConfigureAwait(false);

                                accountSettingsDocumentCreated = true;
                            }
                            #region Manage Exception & Create Manual Instructions

                            catch (DocumentClientException de)
                            {
                                accountSettingsException = de.GetBaseException();

                            }
                            catch (Exception e)
                            {
                                accountSettingsException = e;
                            }

                            if (!accountSettingsDocumentCreated)
                            {
                                #region Log Exception

                                if (accountSettingsException != null)
                                {
                                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                                        accountSettingsException,
                                        "creating an account settings document into a partition during account provisioning",
                                        System.Reflection.MethodBase.GetCurrentMethod(),
                                        account.AccountID.ToString(),
                                        account.AccountName
                                    );
                                }


                                #endregion

                                #region Manual Instructions

                                //Not successfull, All tasks within 'GetNextAvailableDocumentPartition' must be run manually
                                PlatformLogManager.LogActivity(
                                    CategoryType.ManualTask,
                                    ActivityType.ManualTask_DocumentDB,
                                    "AccountSettingsDocumentModel file creation failed during account provisioning",
                                    "Please create the 'AccountSettingsDocumentModel' document for '" + account.AccountName + "' within the '" + nextAvailablePartitionCollection.Id + "' collection manually.",
                                    account.AccountID.ToString(),
                                    account.AccountName,
                                    null,
                                    null,
                                    null,
                                    null,
                                    System.Reflection.MethodBase.GetCurrentMethod().ToString()
                                );

                                #endregion
                            }

                            #endregion

                        #endregion
                        
                    }
                    else
                    {
                        #region Manual Instructions

                        //Not successfull, All tasks within 'GetNextAvailableDocumentPartition' must be run manually
                        PlatformLogManager.LogActivity(
                            CategoryType.ManualTask,
                            ActivityType.ManualTask_Other,
                            "Document partitioning failed during account provisioning",
                            "Please run all tasks under 'DocumentPartitioningManager.GetNextAvailableDocumentPartition('Free', client, dataBaseSelfLink)' as Well as 'if (partitioningResult.isSuccess == true)' manually. This may include creating a new DocumentPartition, updating account DocumentPartitionId and creating an AccountPropertiesDocument for this account into the new partition.",
                            account.AccountID.ToString(),
                            account.AccountName,
                            null,
                            null,
                            null,
                            null,
                            System.Reflection.MethodBase.GetCurrentMethod().ToString()
                        );

                        #endregion
                    }

                #region Depricated DocumentDB Code
                /*
                try
                {
                    DocumentClient client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
                    client.OpenAsync(); //<-- By default, the first request will have a higher latency because it has to fetch the address routing table. In order to avoid this startup latency on the first request, you should call OpenAsync() once during initialization as follows.


                    //Generate Account Database
                    Database accountDatabase = client.CreateDatabaseAsync(new Database { Id = account.AccountID.ToString() }).Result;


                    //Generate "AccountProperties" Collection on the database
                    DocumentCollection accountPropertiesCollection = client.CreateDocumentCollectionAsync(accountDatabase.SelfLink, new DocumentCollection { Id = "AccountProperties" }).Result;


                    //Generate "SelfLinkReferences" Document within AccountProperties" collection
                    Document selfLinkReferencesDocument = client.CreateDocumentAsync(accountPropertiesCollection.SelfLink, new SelfLinkReferencesDocumentModel { Id = "SelfLinkReferences" }).Result;


                    //Store all the SelfLinks
                    var documentUpdateResults = Sql.Statements.UpdateStatements.UpdateDocumentDatabaseLinks(account.AccountID.ToString(), accountDatabase.SelfLink, accountPropertiesCollection.SelfLink, selfLinkReferencesDocument.SelfLink);
                    if (documentUpdateResults)
                    {

                    }
                    else
                    {

                        var errorMessage = "DocumentDB Selflink insertion into the '" + account.AccountName + "' account has failed";
                        var errorDetails = "AccountID: '" + account.AccountID + "' Error: 'DocumentDB resources have been provisioned, but an error occured when updating database columns for the account'";

                        //Log Errors
                        PlatformLogManager.LogActivity(
                                CategoryType.Error,
                                ActivityType.Error_Other,
                                errorMessage,
                                errorDetails,
                                account.AccountID.ToString(),
                                account.AccountName
                            );

                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = errorMessage };
                    }
                }
                catch (Exception e)
                {
                    #region Handle Exception

                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to partition DocumentDB resources for the '" + account.AccountName + "' account during provisioning.",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        account.AccountID.ToString(),
                        account.AccountName
                    );

                    #endregion
                }
                 */

                #endregion

            }

            #endregion

            #region Storage Partitioning

            if (_storagePartitioning)
            {

                /* No longer need to set anything up (Back to document db)

                //Create setings JSON doc in storage (DocumentDB is now OFF)
                var accountSettingsDocument = new AccountSettingsDocumentModel { Id = "AccountSettings" };

                accountSettingsDocument.ContactSettings = new ContactSettingsModel();
                accountSettingsDocument.ContactSettings.ContactInfo = new ContactInfoModel();
                accountSettingsDocument.SalesSettings = new SalesSettingsModel();

                //Default LeadLabels
                accountSettingsDocument.SalesSettings.LeadLabels = new List<string>();
                accountSettingsDocument.SalesSettings.LeadLabels.Add("New");
                accountSettingsDocument.SalesSettings.LeadLabels.Add("Archive");
                accountSettingsDocument.SalesSettings.LeadLabels.Add("Deleted");

                accountSettingsDocument.Theme = "Light"; //<-- Default Theme
                accountSettingsDocument.SalesSettings.ButtonCopy = "I'm interested!"; //<-- Default Theme
                accountSettingsDocument.SalesSettings.DescriptionCopy = "Fill out our contact form and a member of our team will contact you directly.";

                //Save to designated storage account
                CloudStorageAccount storageAccount;
                StorageCredentials storageCredentials = new StorageCredentials(storagePartition.Name, storagePartition.Key);
                storageAccount = new CloudStorageAccount(storageCredentials, false);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                //Create and set retry policy
                IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(400), 6);
                blobClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

                //Creat/Connect to the Blob Container for this account
                blobClient.GetContainerReference(account.AccountNameKey).CreateIfNotExists(BlobContainerPublicAccessType.Blob); //<-- Create and make public


                CloudBlobContainer blobContainer = blobClient.GetContainerReference(account.AccountNameKey);

                //Get reference to the text blob or create if not exists. 
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference("settings/" + "accountSettings.json");

                blockBlob.UploadText(JsonConvert.SerializeObject(accountSettingsDocument));

                //Save to storage
                //Convert final BMP to byteArray
                //Byte[] finalByteArray;

                //finalByteArray = outStream.ToArray();

                //blockBlob.UploadFromByteArray(finalByteArray, 0, finalByteArray.Length);

                */

            }

            #endregion

            #region SQL Partitioning

            if (_sqlPartitioning)
            {
                try
                {
                    // 1. Get and assign the next available database partition for this account to be provisioned into:
                    var getAndAssignPartitionResponse = SqlPartitioningManager.GetAndAssignNextAvailableAccountSqlPartition(account.AccountID.ToString());

                    if (getAndAssignPartitionResponse.isSuccess)
                    {
                        string DatabasePartitionName = getAndAssignPartitionResponse.SuccessMessage;

                        // 2. Run creation scripts to provision accounts schema to the selected partition:
                        var generateAccountSchemaResponse = AccountProvisioning.GenerateAccountSchema(account.AccountID.ToString(), DatabasePartitionName);

                        if (generateAccountSchemaResponse.isSuccess)
                        {
                            generateAccountSchemaResponse.SuccessMessage = DatabasePartitionName; //<-- Return the name of the database partition name

                        }
                        else
                        {
                            return generateAccountSchemaResponse;
                        }

                    }
                    else
                    {
                        return getAndAssignPartitionResponse;
                    }
                }
                catch(Exception e)
                {
                    #region Handle Exception

                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to partition SQL for the '" + account.AccountName + "' account during provisioning.",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        account.AccountID.ToString(),
                        account.AccountName
                    );

                    #endregion
                }
            }




            #endregion

            #region Search Partitioning

            if(_searchPartitioning)
            {
                //Create an Product Search Index for this account on the selected search partition ---------------------
                var searchIndexCreated = ProductSearchManager.CreateProductSearchIndex(account.AccountNameKey, searchPartition.Name, searchPartition.Key);
            }

            #endregion

            #endregion

            #region Post Partitioning Tasks

            // 1. Mark the Account as Provisioned, Active and assign a ProvisioningDate:
            var result = Sql.Statements.UpdateStatements.UpdateProvisiongStatus(account.AccountID.ToString(), true, true, storagePartition.Name, searchPartition.Name);


            if (result)
            {
                // 1. Create a platform user account SO we can log into the account for management purposes:
                AccountUserManager.CreateAccountUser(account.AccountID.ToString(), "platformadmin@[Config_PlatformEmail]", "Platform", "Admin", "[Config_PlatformPassword_AzureKeyVault]", Settings.Accounts.Users.Authorization.Roles.PlatformAdmin, true, null, true);

                // 2. Invalidated/Update the cache for this account
                AccountManager.UpdateAccountDetailCache(account.AccountNameKey);

                // 3. Email the creator with sucessful provisioning message and login info:
                /*
                EmailManager.Send(
                        account.Users[0].Email, //<-- Will only have the initial user
                        Settings.Endpoints.Emails.FromProvisioning,
                        Settings.Copy.EmailMessages.ProvisioningComplete.FromName,
                        Settings.Copy.EmailMessages.ProvisioningComplete.Subject,
                        String.Format(Settings.Copy.EmailMessages.ProvisioningComplete.Body, account.AccountNameKey),
                        true
                    );*/

                // 4. Send an alert to the platform admin(s):
                EmailManager.Send(
                        Settings.Endpoints.Emails.PlatformEmailAddresses,
                        Settings.Endpoints.Emails.FromProvisioning,
                        "Provisioning " + Settings.Application.Name,
                        "Account Provisioned",
                        "<b>'" + account.AccountName + "'</b> has just been provisioned.",
                        true
                    );

                // 5. Log Successfull Provisioning Activity
                PlatformLogManager.LogActivity(CategoryType.Account,
                    ActivityType.Account_Provisioned,
                    "Provisioning of '" + account.AccountName + "' has completed",
                    "AccountID: '" + account.AccountID + "'",
                    account.AccountID.ToString(), account.AccountName);

                //Register subdomains
                try
                {
                    var cloudFlareResult = CloudFlareManager.RegisterSubdomains(account.AccountNameKey);

                    if (cloudFlareResult.isSuccess == false)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogErrorAndAlertAdmins(
                            cloudFlareResult.ErrorMessage,
                            "attempting to add cloudflare subdomains for the '" + account.AccountName + "' account during provisioning.",
                            System.Reflection.MethodBase.GetCurrentMethod(),
                            account.AccountID.ToString(),
                            account.AccountName
                        );
                    }
                }
                catch(Exception e)
                {
                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to register cloudflare subdomains for the '" + account.AccountName + "' account during provisioning.",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        account.AccountID.ToString(),
                        account.AccountName
                    );
                }

                return new DataAccessResponseType { isSuccess = true };
            }
            else
            {
                var errorMessage = "Account has been fully provisioned, but an error occured when setting the Account table to Active and assigning a provisioning date";
                
                PlatformLogManager.LogActivity(CategoryType.Error,
                    ActivityType.Error_Other,
                    "Provisioning of '" + account.AccountName + "' has failed",
                    errorMessage,
                    account.AccountID.ToString(), account.AccountName);

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = errorMessage  };
            }



            #endregion

        }
    }
}
