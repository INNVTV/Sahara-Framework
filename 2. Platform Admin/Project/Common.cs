using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlatformAdminSite
{
    public static class Common
    {
        public static string SharedClientKey = "";

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


    }
}