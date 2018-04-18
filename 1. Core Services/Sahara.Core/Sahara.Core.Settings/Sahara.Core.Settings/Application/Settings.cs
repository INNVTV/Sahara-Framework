using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings
{
    public static class Application
    {
        #region BASE SETTINGS

        public static string Name = "The Sahara Framework";
        public static TimeZoneInfo LocalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        
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

                    Application.Name = "Sahara-Stage";

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    Application.Name = "Sahara-Debug";

                    break;

                case "local":

                    Application.Name = "Sahara-Local";

                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }
}
