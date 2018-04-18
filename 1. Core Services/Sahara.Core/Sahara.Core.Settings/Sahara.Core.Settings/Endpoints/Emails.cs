using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Endpoints
{
    public static class Emails
    {
        #region BASE SETTINGS

        // Important status emails are sent to these platform managers
        public static List<string> PlatformEmailAddresses
        {
            get
            {
                var emails = new List<string>();

                emails.Add("[Config_Email]");

                return emails;
            }
        }

        public const string FromRegistration = "[Config_Email]";
        public const string FromSubscriptions = "[Config_Email]";
        public const string FromInvitations = "[Config_Email]";
        public const string FromProvisioning = "[Config_Email]";
        public const string FromPasswordClaims = "[Config_Email]";
        public const string FromReminders = "[Config_Email]";
        public const string FromAlerts = "[Config_Email]";
        public const string FromBilling = "[Config_Email]";
        public const string FromRefunds = "[Config_Email]";

        public const string FromPlatform = "[Config_Email]";
        public const string FromExceptions = "[Config_Email]";

        public const string ToInfo = "[Config_Email]";
        public const string ToBilling = "[Config_Email]";
        public const string ToSubscriptions = "[Config_Email]";

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
