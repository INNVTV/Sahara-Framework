using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Account.Public.Website.Controllers
{
    public class InfrastructureController : Controller
    {
        [Route("infrastructure/refreshPlatformSettings")]
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

    }
}