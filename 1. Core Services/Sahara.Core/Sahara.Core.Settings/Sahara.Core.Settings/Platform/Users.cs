using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Platform
{
    public static class Users
    {
        #region BASE SETTING

            public static class Authentication
            {
                public static int WebApplicationCookiePersistence = 6; //<--- Hours
                public static int PasswordMinimumLength = 8;
            }

            public static class Authorization
            {

                public static class Roles
                {
                    public static string SuperAdmin =   "SuperAdmin";
                    public static string Admin =        "Admin";
                    public static string Manager =      "Manager";
                    public static string User =         "User";

                    public static List<string> GetRoles() //<---   Used to populate dropdowns and initialize platform
                    {
                        return new List<string>()
                        {
                            // In order of least to greatest (important! Order is used for validating user requests!):

                            User,               //<-------      Read Only & Basic Functions
                            Manager,            //<-------      Can manage basic data
                            Admin,              //<-------      Can Manage Most of the Platform
                            SuperAdmin          //<-------      Can Manage Everything
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
