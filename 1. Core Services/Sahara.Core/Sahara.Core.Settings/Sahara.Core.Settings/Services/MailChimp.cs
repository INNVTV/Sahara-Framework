using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Services
{
    public static class MailChimp
    {
        #region BASE SETTINGS

        public static class Account
        {
            public static string ApiUrl = "";
            public static string UserName = "";
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

                    Account.ApiUrl = "";

                    break;

                #endregion


                #region Stage

                case "stage":

                    Account.ApiUrl = "";

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    Account.ApiUrl = "";


                    break;

                case "local":

                    Account.ApiUrl = "";

                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }


}
