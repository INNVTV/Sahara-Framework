using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Imaging_API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
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

            #region Communicate with CoreServices and get static settings for this client to work with

            //Each client can get necessary settings and endpoint information from CoreServices for each enviornment.
            //This happens when the appliation is first initialized, and is stored in a static class for performance
            //If CoreServices are updated, a restart of each client may be necessary to get updated settings

            var platformSettingsServiceClient = new PlatformSettingsService.PlatformSettingsServiceClient(); // <-- We only use PlatformSettingsServiceClient in EnviornmentSettings because it is ONLY used at application startup:

            //Get & Initialize Settings from CoreServices:

            try
            {

                // PLATFORM SETTINGS SERVICE --------------------------------------

                platformSettingsServiceClient.Open();
                var platformSettingsResult = platformSettingsServiceClient.GetCorePlatformSettings(Common.SharedClientKey);



                CoreServices.PlatformSettings = platformSettingsResult;
                EnvironmentSettings.CurrentEnvironment.CoreServices = platformSettingsResult.Environment.Current;

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
                EnvironmentSettings.CurrentEnvironment.CoreServices = "error: " + exceptionMessage;
            }

            #endregion


            //Register bundles last so options can be set by environment settings:
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
