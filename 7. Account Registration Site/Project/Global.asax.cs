using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AccountRegistration
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
            //var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            //var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

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
                //ConfigurationOptions redisConf_PlatformManager = ConfigurationOptions.Parse(platformSettingsResult.Redis.PlatformManager_Unsecure);
                //redisConf_PlatformManager.AllowAdmin = true;

                //ConfigurationOptions redisConf_AccountManager = ConfigurationOptions.Parse(platformSettingsResult.Redis.AccountManager_Unsecure);
                //redisConf_AccountManager.AllowAdmin = true;

                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(redisConf);
                //IDatabase cache = con.GetDatabase();

                //CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer = ConnectionMultiplexer.Connect(redisConf_PlatformManager);
                //CoreServices.RedisConnectionMultiplexers.AccountManager_Multiplexer = ConnectionMultiplexer.Connect(redisConf_AccountManager);

                #endregion

                #endregion

                //Close the connections
                WCFManager.CloseConnection(platformSettingsServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connections & manage the exception
                WCFManager.CloseConnection(platformSettingsServiceClient, exceptionMessage, currentMethodString);

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

        }
    }
}
