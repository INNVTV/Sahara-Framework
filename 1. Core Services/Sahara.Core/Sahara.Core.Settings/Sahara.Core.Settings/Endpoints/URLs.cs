using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Endpoints
{
    public static class URLs
    {
        #region BASE SETINGS

        //Master Domains
        public static string MasterDomain = string.Empty;
        public static string PlatformDomain = string.Empty;

        public static string AccountManagementDomain = string.Empty;
        public static string AccountServiceDomain = string.Empty;
        public static string AccountSiteDomain = string.Empty;

        public static string ApiDomain = string.Empty;

        //Subdomains (Sites)
        public static string RegistrationUrl = string.Empty;

        //Management Links
        public static string AccountUserAcceptInvitationUrl = string.Empty;
        public static string AccountUserPasswordClaimUrl = string.Empty;
        public static string AccountManagementUpdateCardUrl = string.Empty;
        public static string AccountManagementCreateSubscriptionUrl = string.Empty;
        public static string AccountManagementUpgradeSubscriptionUrl = string.Empty;

        //Subdomains (Platforms)
        public static string PlatformUserAcceptInvitationUrl = string.Empty;
        public static string PlatformUserPasswordClaimUrl = string.Empty;

        //Subdomains (APIs)
        public static string RegistrationApiEndpoint = string.Empty;

        //Centralized Services
        public static string ImagingApiUrl = string.Empty;

        //Legal, Privacy, Etc...
        public static string ServiceAgreement = string.Empty;
        public static string PrivacyPolicy = string.Empty;
        public static string TermsAndConditions = string.Empty;
        public static string AcceptableUse = string.Empty;
        public static string AboutUs = string.Empty;

        #endregion

        #region INITIALIZE

        internal static void Initialize()
        {
            #region Initialize Settings

            switch (Environment.Current.ToLower())
            {

                #region Production

                case "production":

                    MasterDomain = "[Config_Domain].com";
                    PlatformDomain = "platform.[Config_Domain].com";
                    ApiDomain = "[Config_Domain].net";

                    AccountManagementDomain = "[Config_Domain].com"; 
                    AccountServiceDomain = "[Config_Domain].co";
                    AccountSiteDomain = "[Config_Domain].net";


                    AssignURLs();

                    break;

                #endregion


                #region Stage

                case "stage":

                    MasterDomain = "";
                    PlatformDomain = "platform.";
                    ApiDomain = "";

                    AccountManagementDomain = ""; 
                    AccountServiceDomain = "";
                    AccountSiteDomain = "";

                    AssignURLs();

                    break;

                #endregion


                #region Local/Debug

                case "debug":

                    MasterDomain = "";
                    PlatformDomain = "platform..com";
                    ApiDomain = "";

                    AccountManagementDomain = ".com"; 
                    AccountServiceDomain = "";
                    AccountSiteDomain = "";


                    AssignURLs();

                    break;

                case "local":

                    MasterDomain = "[Config_Domain].com";
                    PlatformDomain = "platform.[Config_Domain].com";
                    ApiDomain = "[Config_Domain].net";

                    AccountManagementDomain = "[Config_Domain].com"; 
                    AccountServiceDomain = "[Config_Domain].co";
                    AccountSiteDomain = "[Config_Domain].net";

                    AssignURLs();

                    break;

                    #endregion

            }

            #endregion

        }

        #endregion

        #region PRIVATE

        private static void AssignURLs()
        {
            //Accounts:
            RegistrationUrl = "https://register." + MasterDomain;

            //API Endpoints
            RegistrationApiEndpoint = "https://registration." + ApiDomain;
            ImagingApiUrl = "https://imaging." + ApiDomain;

            //Account Users:
            AccountUserAcceptInvitationUrl = AccountManagementDomain + "/invitations";
            AccountUserPasswordClaimUrl = AccountManagementDomain + "/password/reset";

            //Account Management:
            AccountManagementUpdateCardUrl = AccountManagementDomain + "/account/updatecard";
            AccountManagementCreateSubscriptionUrl = AccountManagementDomain + "/account/subscribe";
            AccountManagementUpgradeSubscriptionUrl = AccountManagementDomain + "/account/upgrade";

            //Platform Users:
            PlatformUserAcceptInvitationUrl = "https://" + PlatformDomain + "/invitations";
            PlatformUserPasswordClaimUrl = "https://" + PlatformDomain + "/password/reset";

            //Legal, Privacy, Etc...
            ServiceAgreement = "https://" + MasterDomain + "/legal/sla";
            TermsAndConditions = "https://" + MasterDomain + "/legal/tc";            
            AcceptableUse = "https://" + MasterDomain + "/legal/aup";
            PrivacyPolicy = "https://" + MasterDomain + "/trust/privacy";
            AboutUs = "https://" + MasterDomain + "/about";
        }

        #endregion
    }
}
