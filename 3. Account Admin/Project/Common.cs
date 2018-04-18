using AccountAdminSite.AccountManagementService;
using Microsoft.Azure.Search;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace AccountAdminSite
{
    public static class Common
    {

        public static string SharedClientKey = "";

        public static ObjectCache SearchIndexCache = MemoryCache.Default;
        public static int SearchIndexClientCacheTimeInMinutes = 15;


        public static int LocalAccountCacheTimeInSeconds = 300;
        
        public static Account GetAccountObject(string accountNameKey)
        {
            Account account = null;

            #region (Plan A) Get Account from Local Cache

            bool localCacheEmpty = false;
            string localCacheKey = accountNameKey + ":account";

            try
            {
                account = (Account)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (account == null)
            {
                localCacheEmpty = true;

                #region (Plan B) Get Account from Redis Cache

                try
                {
                    //First we attempt to get the user from the Redis Cache

                    IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                    string hashKey = "accountbyname:" + accountNameKey;
                    string hashField = "model";

                    try
                    {
                        var redisValue = cache.HashGet(hashKey, hashField);

                        if (redisValue.HasValue)
                        {
                            account = JsonConvert.DeserializeObject<Account>(redisValue);
                        }
                    }
                    catch
                    {

                    }

                }
                catch (Exception e)
                {
                    var error = e.Message;
                }

                #endregion
            }
            if (account == null)
            {
                #region (Plan C) Get Account from WCF

                //If a failure occurs, or the redis cache is empty we get the user from the WCF service
                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    account = accountManagementServiceClient.GetAccount(accountNameKey, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(accountManagementServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            if (localCacheEmpty)
            {
                #region store Account into local cache

                //store string in local cache for a few moments:
                HttpRuntime.Cache.Insert(localCacheKey, account, null, DateTime.Now.AddSeconds(Common.LocalAccountCacheTimeInSeconds), TimeSpan.Zero);

                #endregion
            }

            return account;

        }

        //Refresh Platform Settings
        public static void RefreshPlatformSettings()
        {
            #region Communicate with CoreServices and get updated subset of static settings for this client to work with

            var platformSettingsServiceClient = new PlatformSettingsService.PlatformSettingsServiceClient(); // <-- We only use PlatformSettingsServiceClient in EnviornmentSettings because it is ONLY used at application startup:

            try
            {

                platformSettingsServiceClient.Open();
                var platformSettingsResult = platformSettingsServiceClient.GetCorePlatformSettings(Common.SharedClientKey);

                CoreServices.PlatformSettings = platformSettingsResult;

                //Close the connections
                WCFManager.CloseConnection(platformSettingsServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connections & manage the exceptions
                WCFManager.CloseConnection(platformSettingsServiceClient, exceptionMessage, currentMethodString);

                #endregion

                platformSettingsServiceClient.Close();
            }

            #endregion

        }

        

        public static string GetSubDomain(Uri url)
        {
            try
            {
                string host = url.Host;
                if (host.Split('.').Length > 2)
                {
                    int firstIndex = host.IndexOf(".");
                    string subdomain = host.Substring(0, firstIndex);

                    if (subdomain != "www" && !String.IsNullOrEmpty(subdomain))
                    {
                        return subdomain;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    if (EnvironmentSettings.CurrentEnvironment.Site == "local")
                    {
                        return ConfigurationManager.AppSettings["LocalDebugAccount"];
                    }
                }
            }
            catch
            {
                
            }

            return string.Empty;
        }


        //public static Dictionary<String, SearchIndexClient> SearchIndexes;


        //Static Search Indexes
        //public static List<SearchIndexHopper> SearchIndexes; //<-- Stores a static list of all Search indexes ready for use


    }
    /*
    public class SearchIndexHopper
    {
        public string IndexName;
        public ISearchIndexClient SearchIndexClient;
    }*/
}