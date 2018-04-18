using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Accounts
{
    public static class Users
    {
        #region BASE SETTINGS


        public static class Authentication
        {
            public static string WebApplicationCookieName = "sahara-authentication-cookie"; //<--- Name of cookie for web authentication
            public static int WebApplicationCookiePersistence = 48; //<--- Hours
        }

        public static class Authorization
        {
            public static class Roles
            {
                public static string PlatformAdmin =    "PlatformAdmin"; //<--Hide from Admins
                public static string Admin =            "Admin";
                public static string Manager =          "Manager";
                //public static string Sales =            "Sales";
                public static string User =             "User";

                public static List<string> GetRoles() //<--   Used to populate dropdowns and initialize platform
                {
                    return new List<string>()
                    {
                        // In order of least to greatest (important! Order is used for validating user requests!):

                        User,                  //<---     Read Only (Can browse inventory data)
                        //Sales,                 //<---     Can View/Manage Leads/Customers (Only available on certain account types)
                        Manager,               //<---     Can Manage Inventory/Category/Properties/Tag Data
                        Admin,                 //<---     Can Manage Users, Account & Billing       
                        PlatformAdmin          //<---     Can Add Properties/SearchFields, Featured Properties & Image Groups/Formats                
                    };
                }
            }
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
