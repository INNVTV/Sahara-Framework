using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Platform
{
    public static class Custodian
    {
        #region BASE SETTINGS

        /// <summary>
        /// The amount of time for worker thread to sleep between custodial duties
        /// </summary>
        public static class Frequency
        {
            //public static int Length = 1800000; //<--- Milliseconds
            //public static string Description = "30 minutes";

            //Switched to 12 minute cycle to allow for new keep alive tasks
            public static int Length = 720000; //<--- Milliseconds
            public static string Description = "12 minutes";
        }

        //public static class Intermediary
        //{
            //public static int DaysToHold = 3; //<--- Number of days to allow an intermediary container (named by date) data to be retained before purging by custodian
        //}

        //public static class Logs
        //{
            //public static int ExpirationDays = 180; //<--- Number of days to allow log data to be retained before purging by custodian
            //public static int StripeEventLogRetention = -365; //<--- Number of days to retain Stripe Webhook event log data
        //}

        //public static class UnverifiedAccounts
        //{
            //public static int ExpirationDays = 40; //<--- Number of days to allow unverified accounts to sit idle in the system before purging
        //}

        public static class LapsedAccounts
        {
            //public static int WarningPeriodDays = 1; //<--- Number of days to allow an account lapse in payment before sending a warning to the user 
            //public static int GracePeriodDays = 15; //<--- Number of days to wait before purging a lapsed account
        }


        public static class Dunning
        {
            public static List<int> ReminderDaysTillCardExpiration { get; set; } //<-- Lists of days prior to credit card expiration to send reminder emails to account owners
            //public static int DaysToClearCreditCardExpirationReminderEmailsLog { get; set; } //<-- How many days old a log for this should exst before being deeted by the custodian (Must be a negative number)
        }



        #endregion

        #region INITIALIZE

        internal static void Initialize()
        {
            #region Environment Settings

            Dunning.ReminderDaysTillCardExpiration = new List<int>();

            switch (Environment.Current.ToLower())
            {

                #region Production

                case "production":

                    //Frequency.Length = 1800000; 
                    //Frequency.Description = "30 minutes";

                    //Switched to 12 minute cycle to allow for new keep alive tasks
                    Frequency.Length = 720000; //<--- Milliseconds
                    Frequency.Description = "12 minutes";


        // Remind account owners on set days prior to credit card expiration
        Dunning.ReminderDaysTillCardExpiration.Add(30);
                    Dunning.ReminderDaysTillCardExpiration.Add(20);
                    Dunning.ReminderDaysTillCardExpiration.Add(10);
                    Dunning.ReminderDaysTillCardExpiration.Add(5);
                    Dunning.ReminderDaysTillCardExpiration.Add(2);
                    Dunning.ReminderDaysTillCardExpiration.Add(1);
                    Dunning.ReminderDaysTillCardExpiration.Add(-1);
                    Dunning.ReminderDaysTillCardExpiration.Add(-3);

                    //Dunning.DaysToClearCreditCardExpirationReminderEmailsLog = -60;

                    break;

                #endregion


                #region Stage

                case "stage":

                    //Frequency.Length = 1800000; 
                    //Frequency.Description = "30 minutes";

                    //Switched to 12 minute cycle to allow for new keep alive tasks
                    Frequency.Length = 720000; //<--- Milliseconds
                    Frequency.Description = "12 minutes";

                    //UnverifiedAccounts.ExpirationDays = 20;
                    //LapsedAccounts.WarningPeriodDays = 1;
                    //LapsedAccounts.GracePeriodDays = 15;


                    // Remind account owners on set days prior to credit card expiration
                    Dunning.ReminderDaysTillCardExpiration.Add(30);
                    Dunning.ReminderDaysTillCardExpiration.Add(20);
                    Dunning.ReminderDaysTillCardExpiration.Add(10);
                    Dunning.ReminderDaysTillCardExpiration.Add(5);
                    Dunning.ReminderDaysTillCardExpiration.Add(2);
                    Dunning.ReminderDaysTillCardExpiration.Add(1);
                    Dunning.ReminderDaysTillCardExpiration.Add(-1);
                    Dunning.ReminderDaysTillCardExpiration.Add(-3);

                   // Dunning.DaysToClearCreditCardExpirationReminderEmailsLog = -60;

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    Frequency.Length = 1800000; 
                    Frequency.Description = "30 minutes"; 
                    //UnverifiedAccounts.ExpirationDays = 1;
                    //LapsedAccounts.WarningPeriodDays = 1;
                    //LapsedAccounts.GracePeriodDays = 1;

                    // Remind account owners on set days prior to credit card expiration
                    Dunning.ReminderDaysTillCardExpiration.Add(34);
                    Dunning.ReminderDaysTillCardExpiration.Add(33);
                    Dunning.ReminderDaysTillCardExpiration.Add(32);
                    Dunning.ReminderDaysTillCardExpiration.Add(31);
                    Dunning.ReminderDaysTillCardExpiration.Add(30);
                    Dunning.ReminderDaysTillCardExpiration.Add(29);
                    Dunning.ReminderDaysTillCardExpiration.Add(28);
                    Dunning.ReminderDaysTillCardExpiration.Add(20);
                    Dunning.ReminderDaysTillCardExpiration.Add(15);
                    Dunning.ReminderDaysTillCardExpiration.Add(14);
                    Dunning.ReminderDaysTillCardExpiration.Add(13);
                    Dunning.ReminderDaysTillCardExpiration.Add(12);
                    Dunning.ReminderDaysTillCardExpiration.Add(11);
                    Dunning.ReminderDaysTillCardExpiration.Add(10);
                    Dunning.ReminderDaysTillCardExpiration.Add(9);
                    Dunning.ReminderDaysTillCardExpiration.Add(8);
                    Dunning.ReminderDaysTillCardExpiration.Add(7);
                    Dunning.ReminderDaysTillCardExpiration.Add(6);
                    Dunning.ReminderDaysTillCardExpiration.Add(5);
                    Dunning.ReminderDaysTillCardExpiration.Add(4);
                    Dunning.ReminderDaysTillCardExpiration.Add(3);
                    Dunning.ReminderDaysTillCardExpiration.Add(2);
                    Dunning.ReminderDaysTillCardExpiration.Add(1);
                    Dunning.ReminderDaysTillCardExpiration.Add(-1);
                    Dunning.ReminderDaysTillCardExpiration.Add(-2);

                    //Dunning.DaysToClearCreditCardExpirationReminderEmailsLog = -7;

                    break;

                case "local":

                    Frequency.Length = 1800000; 
                    Frequency.Description = "30 minutes";
                    //UnverifiedAccounts.ExpirationDays = 1;
                    //LapsedAccounts.WarningPeriodDays = 1;
                    //LapsedAccounts.GracePeriodDays = 1;

                    // Remind account owners on set days prior to credit card expiration
                    Dunning.ReminderDaysTillCardExpiration.Add(34);
                    Dunning.ReminderDaysTillCardExpiration.Add(33);
                    Dunning.ReminderDaysTillCardExpiration.Add(32);
                    Dunning.ReminderDaysTillCardExpiration.Add(31);
                    Dunning.ReminderDaysTillCardExpiration.Add(30);
                    Dunning.ReminderDaysTillCardExpiration.Add(29);
                    Dunning.ReminderDaysTillCardExpiration.Add(28);
                    Dunning.ReminderDaysTillCardExpiration.Add(20);
                    Dunning.ReminderDaysTillCardExpiration.Add(15);
                    Dunning.ReminderDaysTillCardExpiration.Add(14);
                    Dunning.ReminderDaysTillCardExpiration.Add(13);
                    Dunning.ReminderDaysTillCardExpiration.Add(12);
                    Dunning.ReminderDaysTillCardExpiration.Add(11);
                    Dunning.ReminderDaysTillCardExpiration.Add(10);
                    Dunning.ReminderDaysTillCardExpiration.Add(9);
                    Dunning.ReminderDaysTillCardExpiration.Add(8);
                    Dunning.ReminderDaysTillCardExpiration.Add(7);
                    Dunning.ReminderDaysTillCardExpiration.Add(6);
                    Dunning.ReminderDaysTillCardExpiration.Add(5);
                    Dunning.ReminderDaysTillCardExpiration.Add(4);
                    Dunning.ReminderDaysTillCardExpiration.Add(3);
                    Dunning.ReminderDaysTillCardExpiration.Add(2);
                    Dunning.ReminderDaysTillCardExpiration.Add(1);
                    Dunning.ReminderDaysTillCardExpiration.Add(-1);
                    Dunning.ReminderDaysTillCardExpiration.Add(-2);

                    //Dunning.DaysToClearCreditCardExpirationReminderEmailsLog = -7;

                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }
}
