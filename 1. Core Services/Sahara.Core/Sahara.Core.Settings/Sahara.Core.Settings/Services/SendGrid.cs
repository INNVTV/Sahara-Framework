using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Services
{
    public static class SendGrid
    {
        #region BASE SETTINGS

        public static class Account
        {
            public static bool Active = true; //<-- Set to inactive to avoid sending mass emails when load testing registration, verification, partitioning & provisioning to avoid extra service charges
            //public static string SmptServer = "smtp.sendgrid.net";
            //public static string UserName = "";
            public static string APIKey = "[Config_ApiKey]";
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

                    break;

                #endregion


                #region Stage

                case "stage":

                    break;

                #endregion


                #region Local/Debug

                case "debug":


                    break;

                case "local":


                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }


}
