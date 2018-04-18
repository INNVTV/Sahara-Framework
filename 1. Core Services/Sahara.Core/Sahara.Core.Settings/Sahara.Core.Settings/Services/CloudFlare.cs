using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Services
{
    public static class CloudFlare
    {
        #region BASE SETTINGS

        public static class Account
        {
            public static string apiDomain = "";
            public static string apiKey = "";
            public static string email = "";
        }

        public static class Domain_AccountAdmin
        {
            public static string ZoneId = "";
            public static string Name = "";
            public static string Domain = "";
            public static string Content = "";
        }

        public static class Domain_AccountApi
        {
            public static string ZoneId = "";
            public static string Name = "";
            public static string Domain = "";
            public static string Content = "";
        }

        public static class Domain_AccountWebsite
        {
            public static string ZoneId = "";
            public static string Name = "";
            public static string Domain = "";
            public static string Content = "";
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

                    Account.apiDomain = "https://api.cloudflare.com/client/v4";
                    Account.apiKey = "[Config_APIKey]";
                    Account.email = "[Config_AccountEmail]";

                    Domain_AccountAdmin.Name = "[Config_AccountName]";
                    Domain_AccountAdmin.Domain = "[Config_AccountDomain]";
                    Domain_AccountAdmin.ZoneId = "[Config_ZoneId]";
                    Domain_AccountAdmin.Content = "[Config_Content]";

                    Domain_AccountApi.Name = "[Config_AccountName]";
                    Domain_AccountApi.Domain = "[Config_AccountDomain]";
                    Domain_AccountApi.ZoneId = "[Config_ZoneId]";
                    Domain_AccountApi.Content = "[Config_Content]";

                    Domain_AccountWebsite.Name = "[Config_AccountName]";
                    Domain_AccountWebsite.Domain = "[Config_AccountDomain]";
                    Domain_AccountWebsite.ZoneId = "[Config_ZoneId]";
                    Domain_AccountWebsite.Content = "[Config_Content]";

                    break;

                #endregion


                #region Stage

                case "stage":

                    Account.apiDomain = "https://api.cloudflare.com/client/v4";
                    Account.apiKey = "[Config_APIKey]";
                    Account.email = "[Config_AccountEmail]";

                    Domain_AccountAdmin.Name = "";
                    Domain_AccountAdmin.Domain = "";
                    Domain_AccountAdmin.ZoneId = "";
                    Domain_AccountAdmin.Content = "";

                    Domain_AccountApi.Name = "";
                    Domain_AccountApi.Domain = "";
                    Domain_AccountApi.ZoneId = "";
                    Domain_AccountApi.Content = "";

                    Domain_AccountWebsite.Name = "";
                    Domain_AccountWebsite.Domain = "";
                    Domain_AccountWebsite.ZoneId = "";
                    Domain_AccountWebsite.Content = "";

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    Account.apiDomain = "https://api.cloudflare.com/client/v4";
                    Account.apiKey = "";
                    Account.email = "";

                    Domain_AccountAdmin.Name = "";
                    Domain_AccountAdmin.Domain = "";
                    Domain_AccountAdmin.ZoneId = "";
                    Domain_AccountAdmin.Content = "";

                    Domain_AccountApi.Name = "";
                    Domain_AccountApi.Domain = "";
                    Domain_AccountApi.ZoneId = "";
                    Domain_AccountApi.Content = "";

                    Domain_AccountWebsite.Name = "";
                    Domain_AccountWebsite.Domain = "";
                    Domain_AccountWebsite.ZoneId = "";
                    Domain_AccountWebsite.Content = "";

                    break;

                case "local":

                    Account.apiDomain = "https://api.cloudflare.com/client/v4";
                    Account.apiKey = "[Config_APIKey]";
                    Account.email = "[Config_AccountEmail]";

                    Domain_AccountAdmin.Name = "[Config_AccountName]";
                    Domain_AccountAdmin.Domain = "[Config_AccountDomain]";
                    Domain_AccountAdmin.ZoneId = "[Config_ZoneId]";
                    Domain_AccountAdmin.Content = "[Config_Content]";

                    Domain_AccountApi.Name = "[Config_AccountName]";
                    Domain_AccountApi.Domain = "[Config_AccountDomain]";
                    Domain_AccountApi.ZoneId = "[Config_ZoneId]";
                    Domain_AccountApi.Content = "[Config_Content]";

                    Domain_AccountWebsite.Name = "[Config_AccountName]";
                    Domain_AccountWebsite.Domain = "[Config_AccountDomain]";
                    Domain_AccountWebsite.ZoneId = "[Config_ZoneId]";
                    Domain_AccountWebsite.Content = "[Config_Content]";

                    break;

                    #endregion

            }

            #endregion

        }

        #endregion
    }


}
