using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Platform
{
    public static class GarbageCollection
    {
        #region BASE SETTINGS

        //public static int StripeEventLogDaysToPurge;

        public static int CreditCardExpirationReminderEmailsLogDaysToPurge;
        public static int StripeWebhookEventLogDaysToPurge;

        public static int IntermediaryStorageContainerDaysToPurge; //<--- Number of days to allow an intermediary container (named by date) data to be retained before purging by custodian

        public static int PlatformActivitiesLogDaysToPurge;
        public static int AccountActivitiesLogDaysToPurge;

        #endregion

        #region INITIALIZE

        internal static void Initialize()
        {
            #region Environment Settings

            switch (Environment.Current.ToLower())
            {

                #region Production

                case "production":

                    //StripeEventLogDaysToPurge = 5;
                    CreditCardExpirationReminderEmailsLogDaysToPurge = 3;
                    StripeWebhookEventLogDaysToPurge = 5;

                    IntermediaryStorageContainerDaysToPurge = 3;

                    PlatformActivitiesLogDaysToPurge = 10;
                    AccountActivitiesLogDaysToPurge = 10;

                    break;

                #endregion


                #region Stage

                case "stage":

                    //StripeEventLogDaysToPurge = 5;
                    CreditCardExpirationReminderEmailsLogDaysToPurge = 3;
                    StripeWebhookEventLogDaysToPurge = 5;

                    IntermediaryStorageContainerDaysToPurge = 3;

                    PlatformActivitiesLogDaysToPurge = 10;
                    AccountActivitiesLogDaysToPurge = 10;

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    //StripeEventLogDaysToPurge = 5;
                    CreditCardExpirationReminderEmailsLogDaysToPurge = 3;
                    StripeWebhookEventLogDaysToPurge = 5;

                    IntermediaryStorageContainerDaysToPurge = 3;

                    PlatformActivitiesLogDaysToPurge = 10;
                    AccountActivitiesLogDaysToPurge = 10;

                    break;

                case "local":

                    //StripeEventLogDaysToPurge = 5;
                    CreditCardExpirationReminderEmailsLogDaysToPurge = 3;
                    StripeWebhookEventLogDaysToPurge = 5;

                    IntermediaryStorageContainerDaysToPurge = 3;

                    PlatformActivitiesLogDaysToPurge = 10;
                    AccountActivitiesLogDaysToPurge = 10;

                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }
}
