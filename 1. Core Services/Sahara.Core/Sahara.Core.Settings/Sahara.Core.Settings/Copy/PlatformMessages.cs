using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Copy
{
    public class PlatformMessages
    {
        #region BASE SETTINGS

        #region Account: Registration

        public static class AccountRegistration
        {
            public static string SuccessMessage = "Thank you for registering! We will contact you as soon as possible.";
            
        }


        #endregion

        #region Account: Subscriptions, Payments & Credit Card Communications



        #region Subscription Creations/Updates

        public static class SubscriptionCreated
        {
        }

        public static class SubscriptionUpgraded
        {
            
        }

        public static class SubscriptionDowngraded
        {
            
        }






        #endregion

        #region Credit Card Updates & Subscription Charges

        public static class CreditCardUpdated
        {
 
        }

        public static class SubscriptionCharged
        {
        }

        #endregion

        #endregion

        #region Account: User Invitations & Password Claims


        #endregion

        #region Platform: User Invitations & Password Claims


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
