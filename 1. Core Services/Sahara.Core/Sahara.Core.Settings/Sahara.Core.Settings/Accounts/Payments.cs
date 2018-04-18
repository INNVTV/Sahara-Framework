using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Accounts
{
    public static class Payments
    {
        #region BASE SETINGS

        public static double PaymentAttemptsBeforeLockingAccounts = 6; //<-- Grace period for lapsed accounts before account is locked

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

                    //GracePeriodDays = 1;
                    PaymentAttemptsBeforeLockingAccounts = 5;

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    //GracePeriodDays = .5;
                    PaymentAttemptsBeforeLockingAccounts = 3;

                    break;

                case "local":

                    //GracePeriodDays = .25;
                    PaymentAttemptsBeforeLockingAccounts = 2;

                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }
}
