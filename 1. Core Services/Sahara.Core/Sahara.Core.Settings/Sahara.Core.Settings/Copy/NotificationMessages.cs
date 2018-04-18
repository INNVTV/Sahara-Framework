using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Copy
{
    public class NotificationMessages
    {
        #region BASE SETTINGS

        #region Account: Subscriptions, Payments & Credit Card Communications



        #region Charge Failures

        public static class ChargeFailure_Automatic
        {
            public static string NotificationMessage = "Please update your payment info. Account is scheduled to be closed due to nonpayment.";
        }



        #endregion

        #endregion

        #endregion

        #region INITIALIZE

        internal static void Initialize()
        {
            #region Initialize Settings

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
