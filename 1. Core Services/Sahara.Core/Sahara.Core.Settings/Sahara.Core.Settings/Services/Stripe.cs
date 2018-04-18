using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Services
{
    public static class Stripe
    {
        #region BASE SETTINGS

        public static class Account
        {
            public static string AccountName = "";

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

                    Account.AccountName = "[Config_AccountName]";
                    Account.ApiKey = "[Config_ApiKey]"; //<-- LIVE (Production) 
                    //Account.ApiKey = "[Config_ApiKey]"; //<-- TEST (Production)

                    break;

                #endregion


                #region Stage

                case "stage":

                    Account.AccountName = "[Config_AccountName]";
                    Account.ApiKey = "[Config_ApiKey]";

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    Account.AccountName = "[Config_AccountName]";
                    Account.ApiKey = "[Config_ApiKey]";


                    break;

                case "local":

                    Account.AccountName = "[Config_AccountName]";
                    Account.ApiKey = "[Config_ApiKey]"; //<-- LIVE (Production)
                    //Account.ApiKey = "[Config_ApiKey]"; //<-- TEST (Production)

                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }


}
