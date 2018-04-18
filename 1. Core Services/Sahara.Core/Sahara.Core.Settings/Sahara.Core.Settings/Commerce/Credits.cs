using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Commerce
{
    public static class Credits
    {
        #region BASE SETINGS

        public static int creditsPerDollar = 5; //<-- The amount of credits a user recieves for every dollar when purchasing credits

        #endregion

        #region INITIALIZE

        internal static void Initialize()
        {
            #region Initialize Settings

            switch (Environment.Current.ToLower())
            {

                #region Production

                case "production":

                    //Keep base setting

                    break;

                #endregion


                #region Stage

                case "stage":

                    //Keep base setting

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    //Keep base setting

                    break;

                case "local":

                    //Keep base setting

                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }
}
