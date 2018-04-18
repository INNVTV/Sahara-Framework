using InventoryHawk.Account.Public.Api.ApplicationApiKeysService;
using InventoryHawk.Account.Public.Api.ApplicationImageRecordsService;
using InventoryHawk.Account.Public.Api.Controllers;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace InventoryHawk.Account.Public.Api
{
    public static class Common
    {
        public static string SharedClientKey = "";

        public static ObjectCache SearchIndexCache = MemoryCache.Default;
        public static int SearchIndexClientCacheTimeInMinutes = 15;

        public static int ApiKeysCacheTimeInMinutes = 6; //<-- Takes 3-6 minutes for any new api keys (or deleted keys) to resolve

        //Public Vars
        //public static int AbsoluteCacheTimeInSeconds = 120;        
        public static int CategorizationCacheTimeInMinutes = 1;
        public static int ProductCacheTimeInMinutes = 1;
        public static int AccountSettingsCacheTimeInMinutes = 1;

        public static int PropertiesCacheTimeInMinutes = 1;
        public static int PropertyTypeCacheTimeInMinutes = 40; //<-- We hold onto this longer as it's not very subject to change
        public static int SearchFacetsCacheTimeInMinutes = 1;
        public static int SearchFieldsCacheTimeInMinutes = 1;

        public static int SearchResultsCacheTimeInMinutes = 2;

        public static int TagsListCacheTimeInMinutes = 2;

        //Private Vars
        private static int LocalAccountCacheTimeInHours = 48; //<-- We cache the account locally for 2 days to avoid a Redis or WCF call on a hot account

        public static AccountManagementService.Account GetAccountObject(string accountNameKey)
        {
            AccountManagementService.Account account = null;

            #region (Plan A) Get Account from Local Cache

            bool localCacheEmpty = false;
            string localCacheKey = accountNameKey + ":account";

            try
            {
                account = (AccountManagementService.Account)HttpRuntime.Cache[localCacheKey];
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
                    //First we attempt to get the account from the Redis Cache

                    IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                    string hashKey = "accountbyname:" + accountNameKey;
                    string hashField = "model";

                    try
                    {
                        var redisValue = cache.HashGet(hashKey, hashField);
                        if (redisValue.HasValue)
                        {
                            account = JsonConvert.DeserializeObject<AccountManagementService.   Account>(redisValue);
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
                HttpRuntime.Cache.Insert(localCacheKey, account, null, DateTime.Now.AddHours(Common.LocalAccountCacheTimeInHours), TimeSpan.Zero);

                #endregion
            }

            return account;

        }


        public static bool ValidateApiKey(string accountNameKey, string apiKey)     //, string loggingLabel = null, string loggingDetails = null)
        {
            var result = false;

            List<string> apiKeyStrings = null;

            string localCacheKey = accountNameKey + ":apiKeys";

            #region (Plan A) Get list of api strings from local cache

            try
            {
                apiKeyStrings = (List<string>)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {

            }

            #endregion

            if(apiKeyStrings == null)
            {

                #region (Plan B) Get List of API strings from API Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashApiKey = accountNameKey + ":apicache";
                string hashApiField = "apikeys";

                try
                {
                    var redisApiValue = cache.HashGet(hashApiKey, hashApiField);

                    if (redisApiValue.HasValue)
                    {
                        apiKeyStrings = JsonConvert.DeserializeObject<List<string>>(redisApiValue);
                    }
                }
                catch
                {

                }

                #endregion

                if (apiKeyStrings == null)
                {
                    List<ApiKeyModel> apiKeys = null;

                    #region (Plan C) Get list of API keys from Redis Cache

                    try
                    {
                        

                        string hashMainKey = accountNameKey + ":apikeys";
                        string hashMainField = "list";

                        try
                        {
                            var redisValue = cache.HashGet(hashMainKey, hashMainField);

                            if (redisValue.HasValue)
                            {
                                apiKeys = JsonConvert.DeserializeObject<List<ApiKeyModel>>(redisValue);
                            }
                        }
                        catch
                        {

                        }


                    }
                    catch (Exception e)
                    {
                        var error = e.Message;
                        //TODO: Log: error message for Redis call
                    }


                    #endregion

                    if(apiKeys == null)
                    {
                        #region (Plan D) Get list of API keys from WCF

                        var applicationApiKeysServiceClient = new ApplicationApiKeysService.ApplicationApiKeysServiceClient();

                        try
                        {
                            applicationApiKeysServiceClient.Open();

                            apiKeys = applicationApiKeysServiceClient.GetApiKeys(accountNameKey, Common.SharedClientKey).ToList();

                            WCFManager.CloseConnection(applicationApiKeysServiceClient);

                        }
                        catch (Exception e)
                        {
                            #region Manage Exception

                            string exceptionMessage = e.Message.ToString();

                            var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                            string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                            // Abort the connection & manage the exception
                            WCFManager.CloseConnection(applicationApiKeysServiceClient, exceptionMessage, currentMethodString);

                            #endregion
                        }

                        #endregion
                    }

                    //Convert to list of strings
                    apiKeyStrings = new List<string>();
                    foreach(var key in apiKeys)
                    {
                        apiKeyStrings.Add(key.ApiKey.ToString());
                    }

                    //Cache into API specific cache
                    try
                    {
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(apiKeyStrings), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }
                }

                //Cache locally
                HttpRuntime.Cache.Insert(localCacheKey, apiKeyStrings, null, DateTime.Now.AddMinutes(Common.ApiKeysCacheTimeInMinutes), TimeSpan.Zero);
            }

            if(apiKeyStrings != null)
            {
                if (apiKeyStrings.Contains(apiKey.ToLower()))
                {
                    result = true;

                    #region LOG USE OF API KEY HERE

                    #endregion
                }

                return result;
            }
            else
            {
                return result;
            }
            
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

        public static string GetApiPathAndQuery(Uri url)
        {
            try
            {
                string patAndQuery = url.PathAndQuery;
                return patAndQuery;                               
            }
            catch
            {

            }

            return string.Empty;
        }

    }
}