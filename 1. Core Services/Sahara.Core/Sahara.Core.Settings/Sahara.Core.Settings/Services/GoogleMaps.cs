using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Services
{
    public static class GoogleMaps
    {
        #region BASE SETTINGS

        // ON GOOGLE DEVELOPER ACCOUNT:
        // Enable the Google Maps JAVASCRIPT API.
        // Enable the Google STATIC Maps API.
        // Enable the Google Maps EMBED API.

        //Note: Stage key is open to all
        //Note: Production key is locked down to production domains ONLY (THis is the rate limited/paid account)

        public static class Account
        {
            public static string AccountEmail = "";
            public static string ApiKey = "";
        }

        #endregion

        #region INITIALIZE

        internal static void Initialize()
        {
            #region Environment Settings

            switch (Environment.Current.ToLower())
            {

                #region Production

                case "production":

                    Account.AccountEmail = "[Config_AccountEmail]";
                    Account.ApiKey = "[Config_ApiKey]";

                    break;

                #endregion


                #region Stage

                case "stage":

                    Account.AccountEmail = "[Config_AccountEmail]";
                    Account.ApiKey = "[Config_ApiKey]";

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    Account.AccountEmail = "[Config_AccountEmail]";
                    Account.ApiKey = "[Config_ApiKey]";


                    break;

                case "local":

                    Account.AccountEmail = "[Config_AccountEmail]";
                    Account.ApiKey = "[Config_ApiKey]";

                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }


}
