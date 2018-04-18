using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sahara.Api.Accounts.Registration
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);


            //Initialize Local Environment
            EnvironmentSettings.CurrentEnvironment.Local = ConfigurationManager.AppSettings["Environment"];

            //Require HTTPS if on stage or production:
            if (EnvironmentSettings.CurrentEnvironment.Local.ToLower() == "stage" || EnvironmentSettings.CurrentEnvironment.Local.ToLower() == "production" || EnvironmentSettings.CurrentEnvironment.Local.ToLower() == "release" || EnvironmentSettings.CurrentEnvironment.Local.ToLower() == "staging")
            {
                GlobalFilters.Filters.Add(new RequireHttpsAttribute());
            }

            //Initialize Shared Client Key for WCF Calls
            Common.SharedClientKey = ConfigurationManager.AppSettings["SharedClientKey"];

            //We only use PlatformSettingsServiceClient in EnviornmentSettings because it is ONLY used at application startup:
            var platformSettingsServiceClient = new PlatformSettingsService.PlatformSettingsServiceClient();

            //Get & Initialize Settings from CoreServices:
            try
            {
                platformSettingsServiceClient.Open();
                var platformSettingsResult = platformSettingsServiceClient.GetCorePlatformSettings(Common.SharedClientKey);


                //Close the connection
                WCFManager.CloseConnection(platformSettingsServiceClient);


                //Apply settings as dictated by CORE-SERVICES
                EnvironmentSettings.CurrentEnvironment.CoreServices = platformSettingsResult.Environment.Current;
            }
            catch(Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformSettingsServiceClient, exceptionMessage, currentMethodString);

                #endregion

            }
        }
    }
}
