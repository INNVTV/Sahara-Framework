using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace PlatformAdminSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //Initialize Local Environment
            EnvironmentSettings.CurrentEnvironment.Site = ConfigurationManager.AppSettings["Environment"];

            //Require HTTPS if on stage or production:
            if (EnvironmentSettings.CurrentEnvironment.Site.ToLower() == "stage" || EnvironmentSettings.CurrentEnvironment.Site.ToLower() == "production" || EnvironmentSettings.CurrentEnvironment.Site.ToLower() == "release" || EnvironmentSettings.CurrentEnvironment.Site.ToLower() == "staging")
            {
                GlobalFilters.Filters.Add(new RequireHttpsAttribute());
            }

            //Initialize Shared Client Key for WCF Calls
            Common.SharedClientKey = ConfigurationManager.AppSettings["SharedClientKey"];

            #region Communicate with CoreServices and get static settings

            var platformSettingsServiceClient = new PlatformSettingsService.PlatformSettingsServiceClient(); // <-- We only use PlatformSettingsServiceClient in EnviornmentSettings because it is ONLY used at application startup:
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                #region Get & Initialize Settings from CoreServices

                #region PLATFORM SETTINGS SERVICE

                platformSettingsServiceClient.Open();
                var platformSettingsResult = platformSettingsServiceClient.GetCorePlatformSettings(Common.SharedClientKey);


                //Apply settings as dictated by CORE-SERVICES
                EnvironmentSettings.CurrentEnvironment.CoreServices = platformSettingsResult.Environment.Current;

                //Static wrapper for centralized CoreServices settings
                CoreServices.PlatformSettings = platformSettingsResult;

                //Redis Multiplexer Configurations

                //Because the  ConnectionMultiplexer  does a lot, it is designed to be shared and reused between callers.
                //You should not create a  ConnectionMultiplexer  per operation. It is fully thread-safe and ready for this usage.
                //In all the subsequent examples it will be assumed that you have a  ConnectionMultiplexer  instance stored away for re-use.

                // We need to turn on Admin mode to allow for the flushing and other administrative duties:
                ConfigurationOptions redisConf = ConfigurationOptions.Parse(platformSettingsResult.Redis.Unsecure);
                redisConf.AllowAdmin = true;

                //ConfigurationOptions redisConf_PlatformManager = ConfigurationOptions.Parse(platformSettingsResult.Redis.PlatformManager_Unsecure);
                //redisConf_PlatformManager.AllowAdmin = true;

                //ConfigurationOptions redisConf_AccountManager = ConfigurationOptions.Parse(platformSettingsResult.Redis.AccountManager_Unsecure);
                //redisConf_AccountManager.AllowAdmin = true;

                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(redisConf);
                //IDatabase cache = con.GetDatabase();

                CoreServices.RedisConnectionMultiplexers.RedisMultiplexer = ConnectionMultiplexer.Connect(redisConf);

                //CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer = ConnectionMultiplexer.Connect(redisConf_PlatformManager);
                //CoreServices.RedisConnectionMultiplexers.AccountManager_Multiplexer = ConnectionMultiplexer.Connect(redisConf_AccountManager);

                #endregion

                #region  ACCOUNT MANAGEMENT SERVICE 

                accountManagementServiceClient.Open();
                //We load up roles at startup to avoid making multiple calls in angular views just to populate the list. If roles update website must be restarted to get the latest data.
                CoreServices.Accounts.UserRoles = accountManagementServiceClient.GetAccountUserRoles(Common.SharedClientKey);

                #endregion

                #region  PLATFORM MANAGEMENT SERVICE

                platformManagementServiceClient.Open();
                //We load up roles at startup to avoid making multiple calls in angular views just to populate the list. If roles update website must be restarted to get the latest data.
                CoreServices.Platform.UserRoles = platformManagementServiceClient.GetPlatformUserRoles(Common.SharedClientKey);

                #endregion

                #endregion

                //Close the connections
                WCFManager.CloseConnection(platformSettingsServiceClient);
                WCFManager.CloseConnection(platformManagementServiceClient);
                WCFManager.CloseConnection(accountManagementServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connections & manage the exception
                WCFManager.CloseConnection(platformSettingsServiceClient, exceptionMessage, currentMethodString);
                WCFManager.CloseConnection(platformManagementServiceClient, exceptionMessage, currentMethodString);
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
                EnvironmentSettings.CurrentEnvironment.CoreServices = "error" + exceptionMessage;
            }

            #endregion

            //Register bundles last so options can be set by environment settings:
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            /*
            if (EnvironmentSettings.CurrentEnvironment.Site == "release")
            {
                BundleTable.EnableOptimizations = true;
            }
            else{
                BundleTable.EnableOptimizations = false;
            }
            */

            #region Check if platform is initialized/exists

            var platformInitializationServiceClient = new PlatformInitializationService.PlatformInitializationServiceClient();
            try
            {
                platformInitializationServiceClient.Open();
                var isInitialized = platformInitializationServiceClient.IsPlatformInitialized();

                CoreServices.Platform.Initialized = isInitialized;

                //Close the connection
                WCFManager.CloseConnection(platformInitializationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformInitializationServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

        }
    }
}
