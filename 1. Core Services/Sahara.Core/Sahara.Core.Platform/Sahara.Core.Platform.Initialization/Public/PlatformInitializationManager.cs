using System;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.Stripe;
using Sahara.Core.Platform.Deprovisioning;
using Sahara.Core.Platform.Users;
using Sahara.Core.Accounts.PaymentPlans.Public;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Diagnostics;
using System.Linq;
using StackExchange.Redis;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System.Collections.Generic;

namespace Sahara.Core.Platform.Initialization
{
    public static class PlatformInitializationManager
    {

        public static bool isPlatformInitialized()
        {
            bool platformDatabaseExists = false;
            bool accountsDatabaseExists = false;

            platformDatabaseExists = Sql.Statements.VerificationStatements.DatabaseExists("Platform");
            if (platformDatabaseExists)
            {
                accountsDatabaseExists = Sql.Statements.VerificationStatements.DatabaseExists("Accounts");
                if (accountsDatabaseExists)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static DataAccessResponseType ProvisionPlatform(string FirstName, string LastName, string Email, string password)
        {
            
            // Begin timing task
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataAccessResponseType response = new DataAccessResponseType();

            if (!isPlatformInitialized())
            {
                //Generate Databases & Schemas
                PlatformInitialization platformInitialization = new PlatformInitialization();
                response = platformInitialization.InitializePlatform();

                if(response.isSuccess)
                {
                    try
                    {
                        //Create initial Platform User & Assign to SuperAdmin Role
                        var createUserResult = PlatformUserManager.CreatePlatformUser(Email, FirstName, LastName, password, Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin);
                        if (createUserResult.isSuccess)
                        {

                            //Replicate Payment Plans to Stripe Account:
                            //StripeInitialization.SeedPaymentPlans();
                            var stripePlansResponse = PaymentPlanManager.DuplicatePlansToStripe();

                            if (!stripePlansResponse.isSuccess)
                            {
                                response.isSuccess = false;
                                response.ErrorMessage = "An error occured when creating Stripe plans";
                                return response;
                            }


                            /*======================================
                                 Create AccountPartition Document Database
                           ========================================*/

                            /*  Retired ------------------*/

                            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
                            var databasename = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId;

                            Database accountDatabase = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDatabaseQuery().Where(db => db.Id == databasename).ToArray().FirstOrDefault();
                            if (accountDatabase == null)
                            {
                                //Create if not exists
                                accountDatabase = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDatabaseAsync(new Database { Id = databasename }).Result;
                            }

                            


                            /*======================================
                                 Clear all logs ()
                           ========================================*/

                            //PlatformLogManager.ClearLogs();

                            /*======================================
                                 Log Initilization of Platform
                           ========================================*/

                            stopwatch.Stop();

                            PlatformLogManager.LogActivity(
                                CategoryType.Platform,
                                ActivityType.Platform_Initialized,
                                "Platform initilized by: " + FirstName + " " + LastName + " (" + Email + ")",
                                "Initialization took " + String.Format("{0:0,0.00}", TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalMinutes) + " minute(s) to complete"
                                );


                            response.isSuccess = true;
                            response.SuccessMessage = "Platform has been successfully initialized and '" + Email + "' has been registered as the initial platform administrator.";
                        }
                    }
                    catch(Exception e)
                    {
                        
                        response.isSuccess = false;
                        response.ErrorMessage = e.Message;
                        try
                        {
                            response.ErrorMessages.Add(e.InnerException.Message);
                        }
                        catch
                        {

                        }
                    }

                }

                return response;
            }
            else
            {
                response.isSuccess = false;
                response.ErrorMessage = "Platform is already initialized.";
                response.ErrorMessages.Add("If you are attempting to create a new installation on this envionemnt: You must first clear all platfrom components manually.");

                return response;
            }
        }

        public static DataAccessResponseType PurgePlatform()
        {
            DataAccessResponseType response = new DataAccessResponseType();

            //We only allow purging on local, debug & stage 
            if(Sahara.Core.Settings.Environment.Current.ToLower() == "production")
            {
                response.isSuccess = false;
                response.ErrorMessage = "Cannot purge a production version of the platform";

                return response;
            }
            else if (Sahara.Core.Settings.Environment.Current.ToLower() != "staging" && Sahara.Core.Settings.Environment.Current.ToLower() != "stage" && Sahara.Core.Settings.Environment.Current.ToLower() != "debug" && Sahara.Core.Settings.Environment.Current.ToLower() != "test" && Sahara.Core.Settings.Environment.Current.ToLower() != "testing" && Sahara.Core.Settings.Environment.Current.ToLower() != "local")
            {
                response.isSuccess = false;
                response.ErrorMessage = "Can only purge a stage, test, debug or local version of the platform";

                return response;
            }
            else
            {
                //Deprovision All Accounts:
                DeprovisionAllAccounts();

                //Clear Stripe Plans:
                var stripeManager = new StripeManager();
                var planIDs = stripeManager.GetPlanIDs();
                foreach (string planID in planIDs)
                {
                    stripeManager.DeletePlan(planID);
                }

                //Delete All SQL Databases:
                DeleteAllSQLDatabases();

                //Clear Platform Storage Accounts:
                ClearPlatformStorageAccounts();

                /*======================================
                    Delete AccountPartition Document Database
                ========================================*/

                /*  RETIRED ---*/

                //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
                var databasename = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId;

                Database accountDatabase = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDatabaseQuery().Where(db => db.Id == databasename).ToArray().FirstOrDefault();
                if (accountDatabase != null)
                {
                    //Create if not exists
                    accountDatabase = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.DeleteDatabaseAsync(accountDatabase.SelfLink).Result;
                }




                /*======================================
                    FLUSH ALL REDIS CACHES
                ========================================*/

                var redisEndpoints = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetEndPoints(true);
                var redisServer = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetServer(redisEndpoints[0]);

                redisServer.FlushAllDatabases();


                /*
                var platformEndpoints = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetEndPoints(true);
                var platformServer = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetServer(platformEndpoints[0]);

                var accountEndpoints = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetEndPoints(true);
                var accountServer = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetServer(accountEndpoints[0]);

                platformServer.FlushAllDatabases();
                accountServer.FlushAllDatabases();
                */



                /*================================================================
                    DELETE ALL AZURE SEARCH DATASOURCES, INDEXES & INDEXERS (LEGACY USING SINGLE PLAN)
                ==================================================================

                SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;

                //Delete all search indexes
                var indexList = searchServiceClient.Indexes.List();
                foreach (Microsoft.Azure.Search.Models.Index index in indexList.Indexes)
                {
                    searchServiceClient.Indexes.Delete(index.Name);
                }

                //Delete all search indexers
                var indexerList = searchServiceClient.Indexers.List();
                foreach (Microsoft.Azure.Search.Models.Indexer indexer in indexerList.Indexers)
                {
                    searchServiceClient.Indexers.Delete(indexer.Name);
                }

                //Delete all search datasources
                var dataSourcesList = searchServiceClient.DataSources.List();
                foreach (Microsoft.Azure.Search.Models.DataSource datasource in dataSourcesList.DataSources)
                {
                    searchServiceClient.DataSources.Delete(datasource.Name);
                }*/



                /*==============================================================================
                    DELETE ALL AZURE SEARCH DATASOURCES, INDEXES & INDEXERS (LOOP THROUGH ALL)
                =============================================================================*/

                //Refresh and get list:
                var searchPartitions = Settings.Azure.Search.RefreshSearchPartitions();

                foreach(var searchPartition in searchPartitions)
                {
                    SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartition.Name);

                    //Delete all search indexes
                    var indexList = searchServiceClient.Indexes.List();
                    foreach (Microsoft.Azure.Search.Models.Index index in indexList.Indexes)
                    {
                        searchServiceClient.Indexes.Delete(index.Name);
                    }

                    //Delete all search indexers
                    var indexerList = searchServiceClient.Indexers.List();
                    foreach (Microsoft.Azure.Search.Models.Indexer indexer in indexerList.Indexers)
                    {
                        searchServiceClient.Indexers.Delete(indexer.Name);
                    }

                    //Delete all search datasources
                    var dataSourcesList = searchServiceClient.DataSources.List();
                    foreach (Microsoft.Azure.Search.Models.DataSource datasource in dataSourcesList.DataSources)
                    {
                        searchServiceClient.DataSources.Delete(datasource.Name);
                    }
                }




                /*======================================
                     SEND BACK RESULTS
                ========================================*/

                response.isSuccess = true;
                response.SuccessMessage = "Platform has been purged.";

                return response;
            }
        }

        #region private platform PURGE methods

        private static void DeprovisionAllAccounts()
        {
            try
            {
                var Accounts = AccountManager.GetAllAccounts(0, 0, "AccountNameKey", false);

                foreach (Account account in Accounts)
                {
                    try
                    {
                        var response = DeprovisioningManager.DeprovisionAccount(account);

                        if (response.isSuccess)
                        {
                            //Console.ForegroundColor = ConsoleColor.Green;
                            //Console.WriteLine(account.AccountName + " - has been deleted");
                            //Console.ResetColor();
                        }
                        else
                        {
                            //Console.ForegroundColor = ConsoleColor.Red;
                            //Console.WriteLine("There was an error deleting: " + account.AccountName);
                            //Console.ForegroundColor = ConsoleColor.DarkRed;
                            //Console.WriteLine(response.ErrorMessage);
                            //Console.ResetColor();
                        }
                    }
                    catch
                    {
                        //Console.ForegroundColor = ConsoleColor.Red;
                        //Console.WriteLine("There was an exception thrown while deleting: " + account.AccountName);
                        //Console.ForegroundColor = ConsoleColor.DarkRed;
                        //Console.WriteLine(e.Message);
                        //Console.ResetColor();
                    }
                }
            }
            catch
            {

            }


        }


        private static void DeleteAllSQLDatabases()
        {

            //var Accounts = AccountManager.GetAllAccounts(0, 0, "AccountNameKey", false);

            

            //Get all databases:
            var Databases = Sql.Statements.SelectStatements.GetAllPlatformDatabases();

            //Loop through and add each database to our query:
            foreach (string database in Databases)
            {
                try
                {
                    Sql.Statements.DeleteStatements.DeleteDatabase(database);
                }
                catch
                {

                }
                
            }
        }

        private static void ClearPlatformStorageAccounts()
        {
            //var response = new DataAccessResponseType { isSuccess = false };

            //Loop through all tables named by schema for this account and delete
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 16);
            //IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

            IEnumerable<CloudTable> tables = cloudTableClient.ListTables();

            foreach (CloudTable table in tables)
            {
                try
                {
                    table.Delete();
                }
                catch
                {
                    //response.isSuccess = false;

                }

            }

        }


        #endregion
    }
}
