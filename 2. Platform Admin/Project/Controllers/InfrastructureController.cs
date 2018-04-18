using Newtonsoft.Json;
using PlatformAdminSite.Models.Infrastructure;
using PlatformAdminSite.PlatformManagementService;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    [Authorize]
    public class InfrastructureController : Controller
    {
        public ActionResult Index()
        {
            if (AuthenticationCookieManager.GetAuthenticationCookie().Role != "SuperAdmin")
            {
                return Content("Sorry! Your princess is in another castle...");
            }

            return View();
        }

        #region Refresh Platform Settings

        public ActionResult RefreshPlatformSettings()
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

                return Content("<b style='color:darkgreen'>Settings refreshed!</b>");
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

                return Content("<b style='color:red'>" + e.ToString() + "</b>");

            }

            #endregion


        }

        #endregion

        #region Redis Servers

        public ActionResult RedisServers()
        {
            if (AuthenticationCookieManager.GetAuthenticationCookie().Role != "SuperAdmin")
            {
                return Content("Sorry! Your princess is in another castle...");
            }

            return View();

        }

        #region Get Keys

        public ActionResult RedisServer_GetKeys()
        {
            //ViewBag.ServerName = ServerName;

            var redisServerModel = new RedisServerModel { Name = "Redis" };

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(GetRedisConfigurationString(ServerName));
            //IDatabase cache = con.GetDatabase();

            //ConnectionMultiplexer con = GetRedisConnectionMultiplexer(ServerName);

            IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

            var endpoints = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetEndPoints(true);
            var server = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetServer(endpoints[0]);


            foreach (RedisKey key in server.Keys(0))
            {
                RedisType redisKeyType = cache.KeyType(key);

                if (redisKeyType.ToString().ToLower() == "string")
                {
                    redisServerModel.StringKeys.Add(key);
                }
                else if (redisKeyType.ToString().ToLower() == "hash")
                {
                    redisServerModel.HashKeys.Add(key);
                }
            }

            //con.Close();

            return View(redisServerModel);

        }

        #endregion

        #region Get Values

        public ActionResult RedisServer_GetString(string Key)
        {
            //ViewBag.ServerName = ServerName;
            ViewBag.Key = Key;


            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(GetRedisConfigurationString(ServerName));
            IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

            RedisValue redisValue = cache.StringGet(Key);
            
            //con.Close();

            ViewBag.KeyValue = redisValue;

            return View();
        }

        public ActionResult RedisServer_GetHash(string HashKey)
        {
            //ViewBag.ServerName = ServerName;
            ViewBag.HashKey = HashKey;

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(GetRedisConfigurationString(ServerName));
            //IDatabase cache = con.GetDatabase();

            IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

            List<HashEntry> hashEntry = cache.HashGetAll(HashKey).ToList();

            //con.Close();

            return View(hashEntry);
        }


        #endregion

        #region Flush Keys

        public string RedisServer_Flush()//string ServerName)
        {
            // We need to turn on Admin mode to allow for the flush:
            //ConfigurationOptions redisConf = ConfigurationOptions.Parse(GetRedisConfigurationString(ServerName));
            //redisConf.AllowAdmin = true;

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(redisConf);
            //IDatabase cache = con.GetDatabase();

            //ConnectionMultiplexer con = GetRedisConnectionMultiplexer(ServerName);
            IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

            var endpoints = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetEndPoints(true);
            var server = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetServer(endpoints[0]);

            server.FlushAllDatabases();

            //con.Close();

            return "Redis keys flushed";

        }
        

        #endregion

        #region Private Methods

/*
        private ConnectionMultiplexer GetRedisConnectionMultiplexer(string ServerName)
        {
            return CoreServices.RedisConnectionMultiplexers.RedisMultiplexer;

            
            switch (ServerName)
            {
                case "PlatformManager":
                    return CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer;

                case "AccountManager":
                    return  CoreServices.RedisConnectionMultiplexers.AccountManager_Multiplexer;
   
                default:
                    return null;
            }

        }*/

        #endregion

        #endregion

        #region Performance Tests


        public ActionResult PerformanceTests()
        {
            if (AuthenticationCookieManager.GetAuthenticationCookie().Role != "SuperAdmin")
            {
                return Content("Sorry! Your princess is in another castle...");
            }

            return View();
        }


 
        public ActionResult PerformanceTests_Redis()
        {
            return View();
        }

        #region Redis Test iFrames

        [Route("Infrastructure/PerformanceTest/GetPlansRedis")]
        [HttpGet]
        public String PerformanceTest_GetPlansRedis()
        {

            var watch = Stopwatch.StartNew();


            List<AccountPaymentPlanService.PaymentPlan> paymentPlans = null;

            //Attempt to get the payment plan from the Redis Cache first
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(CoreServiceSettings.RedisConfigurations.AccountManager_Connection);
            //IDatabase cache = con.GetDatabase();

            IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

            string hashKey = "paymentplans";
            string hashField = "list:False:True";

            var redisValue = cache.HashGet(hashKey, hashField);

            //con.Close();

            if (redisValue.HasValue)
            {
                paymentPlans = JsonConvert.DeserializeObject<List<AccountPaymentPlanService.PaymentPlan>>(redisValue);
            }


            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            return "GetPlans (RDS): " + elapsedMs + "ms";

        }

        [Route("Infrastructure/PerformanceTest/GetPlansWCF")]
        [HttpGet]
        public String PerformanceTest_GetPlansWCF()
        {
            var watch = Stopwatch.StartNew();

            List<AccountPaymentPlanService.PaymentPlan> paymentPlans = null;

            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();
            accountPaymentPlanServiceClient.Open();

            paymentPlans = accountPaymentPlanServiceClient.GetPaymentPlans(false, true).ToList();

            //Close the connection
            WCFManager.CloseConnection(accountPaymentPlanServiceClient);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            return "GetPlans (WCF): " + elapsedMs + "ms";

        }

        #endregion


        public ActionResult PerformanceTests_WCFCaching()
        {
            return View();
        }

        #region Performance Test WCF Caching

        [Route("Infrastructure/PerformanceTest/GetAccountsListCached")]
        [HttpGet]
        public String PerformanceTest_WCFCache_GetAccountsListCached()
        {
            var watch = Stopwatch.StartNew();

            var infrastructureTestsServiceClient = new InfrastructureTestsService.InfrastructureTestsServiceClient();
            infrastructureTestsServiceClient.Open();

            var result = infrastructureTestsServiceClient.PerformanceTest_InternalCaching_GetAccounts(true, Common.SharedClientKey);

            //Close the connection
            WCFManager.CloseConnection(infrastructureTestsServiceClient);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            return "Account List (WCF: Cached): " + elapsedMs + "ms";
        }

        [Route("Infrastructure/PerformanceTest/GetAccountsListNoncached")]
        [HttpGet]
        public String PerformanceTest_WCFCache_GetAccountsListNoncached()
        {
            var watch = Stopwatch.StartNew();

            var infrastructureTestsServiceClient = new InfrastructureTestsService.InfrastructureTestsServiceClient();
            infrastructureTestsServiceClient.Open();

            var result = infrastructureTestsServiceClient.PerformanceTest_InternalCaching_GetAccounts(false, Common.SharedClientKey);

            //Close the connection
            WCFManager.CloseConnection(infrastructureTestsServiceClient);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            return "Account List (WCF: No Cache):" + elapsedMs + "ms";
        }

        #endregion

        #endregion

        #region DocumentDB_Accounts

        /*
        public ActionResult DocumentDB_Accounts()
        {
            if (AuthenticationCookieManager.GetAuthenticationCookie().Role != "SuperAdmin")
            {
                return Content("Sorry! Your princess is in another castle...");
            }

            var docuentTiers = new List<DocumentTierModel>();

            #region Get Document Partition Tiers from Redis or WCF

            List<DocumentPartitionTier> documentPartitionTiers = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer.GetDatabase();

                string hashKey = "partitiontiers";
                string hashField = "list";

                var redisValue = cache.HashGet(hashKey, hashField);

                //con.Close();

                if (redisValue.HasValue)
                {
                    documentPartitionTiers = JsonConvert.DeserializeObject<List<DocumentPartitionTier>>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (documentPartitionTiers == null)
            {
                #region (Plan B) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    documentPartitionTiers = PlatformManagementServiceClient.GetDocumentPartitionTiers().ToList();

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            #endregion

            foreach (DocumentPartitionTier tier in documentPartitionTiers)
            {
                var newDocumentTierModel = new DocumentTierModel { DocumentTierId = tier.DocumentPartitionTierID };
                newDocumentTierModel.DocumentCollections = new List<DocumentCollectionModel>();

                #region Get Partitions for this tier from WCF

                List<DocumentPartition> documentPartitions = null;

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    documentPartitions = PlatformManagementServiceClient.GetDocumentPartitions(tier.DocumentPartitionTierID).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                    
                

                #endregion

                foreach(DocumentPartition partition in documentPartitions)
                {
                    newDocumentTierModel.DocumentCollections.Add(new DocumentCollectionModel{ DocumentCollectionId = partition.DocumentPartitionID });
                }

                docuentTiers.Add(newDocumentTierModel);
            }

            return View(docuentTiers);

        }
        */
        #endregion

    }

}