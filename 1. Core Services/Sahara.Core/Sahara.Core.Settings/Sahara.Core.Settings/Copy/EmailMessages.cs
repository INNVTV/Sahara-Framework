using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Copy
{
    public class EmailMessages
    {
        #region BASE SETTINGS

        #region Account: Registration, Provisioning, Closures & Deprovisioning

        /*
        public static class ProvisioningComplete
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your " + Application.Name + " account is ready!";
            public static string Body = "Your " + Application.Name + " account is ready!</br></br>Please login with the following link using the email address and password you created:<br></br>" +
                "http://{0}." + Endpoints.URLs.AccountManagementDomain + "</br></br>" +
                "Thank you & welcome aboard!</br></br>" +
                "The " + Application.Name + " Team";
        }*/

        public static class AccountClosureAlert
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your account has been marked for closure";
            public static string Body = "Hello {0},</br></br>Your " + Application.Name + " account has been marked for closure and will become deactivated {1}." +
                "</br></br>If you did not intend to close your account, please email us at: " + Sahara.Core.Settings.Endpoints.Emails.ToSubscriptions + " immediately to avoid account closure and data loss.</br></br>" +
                "</br></br>Thank you,</br></br>" +
                "The " + Application.Name + " Team";
        }

        public static class DeprovisioningComplete
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your " + Application.Name + " account has been closed";
            public static string Body = "Hello {0},</br></br>Your " + Application.Name + " account has been closed." +
                "</br></br>Thank you,</br></br>" +
                "The " + Application.Name + " Team";
        }



        #endregion

        #region Account: Subscriptions, Payments & Credit Card Communications


        #region Account Owners: Subscription Creations/Updates

        public static class SubscriptionCreated
        {
            public static string FromName = Application.Name;
            public static string Subject = "Thank you for subscribing!";
            public static string Body = "Hello {0},<br/><br/>" +
                "Your have been successfully subscribed to the {1} plan on " + Application.Name + "!" +
                "<br/><br/>You will receive an invoice shortly, as well as instructions on how to access your account.<br/><br/>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class SubscriptionIssues
        {
            public static string FromName = Application.Name;
            public static string Subject = "Subscription Issues";
            public static string Body = "Hello {0},<br/><br/>" +
                "There were issues subscribing you to the {1} plan on " + Application.Name +
                "<br/><br/>We will manually resolve the matter and email you next steps shortly.<br/><br/>" +
                "Thank you for your patience,<br/><br/>The " + Application.Name + " Team";
        }

        public static class SubscriptionUpgraded
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your subscription has been updated";
            public static string Body = "Hello {0},<br/><br/>" +
                "Your have been successfully upgraded to the {1} plan on " + Application.Name + "!" +
                "<br/><br/>You will be billed immediatly for the added services. You will also be prorated for any credits may you have from the remainder of your current subscription!<br/><br/>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class SubscriptionDowngraded
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your subscription has been downgraded";
            public static string Body = "Hello {0},<br/><br/>" +
                "Your have been successfully downgraded to the {1} plan on " + Application.Name + "." +
                "<br/><br/>You will be prorated for any credits you may have from the remainder of your current subscription on your next billing date.<br/><br/>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class SubscriptionFrequencyChanged
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your subscription frequency has been changed";
            public static string Body = "Hello {0},<br/><br/>" +
                "Your subscription frequency has been changed to {1} on " + Application.Name + "!" +
                "<br/><br/>You will be billed immediatly for any additional costs and prorated for any credits you have from the remainder of your current subscription frequency.<br/><br/>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class SubscriptionReactivated
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your subscription has been reactivated!";
            public static string Body = "Hello {0},<br/><br/>" +
                "Your account has been successfully reactivated to the {1} plan on " + Application.Name + "." +
                "<br/><br/>Your billing cycle will continue as before.<br/><br/>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        #endregion

        #region Account Owners: Credit Card Updates, Subscription Charges & Refunds

        public static class CreditCardAdded
        {
            public static string FromName = Application.Name;
            public static string Subject = "Credit Card Added";
            public static string Body = "Hello {0},<br/><br/>" +
                "A credit card has been successfully added on your " + Application.Name + " account" +
                "<br/><br/>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class CreditCardUpdated
        {
            public static string FromName = Application.Name;
            public static string Subject = "Credit Card Updated";
            public static string Body = "Hello {0},<br/><br/>" +
                "Your credit card has been successfully updated on your " + Application.Name + " account" +
                "<br/><br/>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class AccountCharged
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your payment for your " + Application.Name + " account was successfully processed";
            public static string Body = "Hello {0},<br/><br/>" +
                "A charge has been made on your {1} ending in {2} in the amount of <b>{3}</b>" +
                "<br/><br/><br/><strong>Here are your order details:</strong><br/><br/>{4}" +
                "<br/><br/>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class ChargeRefunded
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your refund has been authorized";
            public static string Body = "Hello {0},<br/><br/>" +
                "Your account has been authorized a refund in the amount of <b>{1}</b>" +
                "<br/><br/>Please allow a few days for the refund to be fully processed.<br/><br/>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }



        #endregion

        #region Account Owners: Payment Issues, Dunning & Account Closures

        /*==================================================================================
           Credit card expiration reminders
         ===============================================================================*/

        public static class CreditCardExpirationReminder
        {
            public static string FromName = Application.Name;
            public static string Subject = "Please update your payment information!";
            public static string Body = "Hello {0},<br/><br/>" +
                "The credit card associated with your account is set to expire {1}." +
                "<br/><br/>To avoid a lapse in your service, please update your credit card at the following link:<br/><br/>" +
                "<a href='https://{2}." + Endpoints.URLs.AccountManagementUpdateCardUrl + "'>Update Card</a></br></br>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }
        public static class CreditCardExpirationMessage
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your credit card has expired!";
            public static string Body = "Hello {0},<br/><br/>" +
                "The credit card associated with your account has expired." +
                "<br/><br/>To avoid a lapse in service and potential data loss, please update your credit card immediately at the following link:<br/><br/>" +
                "<a href='https://{1}." + Endpoints.URLs.AccountManagementUpdateCardUrl + "'>Update Card</a></br></br>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }


        /*==================================================================================
           ONE-OFF/SINGLE FAILED CHARGES (Not associated with an invoice)
         ===============================================================================*/


        public static class ChargeFailed
        {
            public static string FromName = Application.Name;
            public static string Subject = "Payment Error";
            public static string Body = "Hello {0},<br/><br/>" +
                "There was an issue when attempting to process a payment for your account.<br/><br/>" +
                "This error is due to the following:<br/><br/><strong>{2}</strong><br/><br/>" +
                "To avoid future billing issues, please update your payment information at the following link:<br/><br/>" +
                "<a href='https://{1}." + Endpoints.URLs.AccountManagementUpdateCardUrl + "'>Update Payment Information</a></br></br>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        /*==================================================================================
           DUNNING  - FAILED CHARGES (associated with an invoice)
         ===============================================================================*/

        public static class InvoicedChargeFailed_FirstDunningAttempt
        {
            public static string FromName = Application.Name;
            public static string Subject = "Payment Processing Error";
            public static string Body = "Hello {0},<br/><br/>" +
                "There was an issue when attempting to process a payment for your account.<br/><br/>" +
                "This error is due to the following:<br/><br/><strong>{2}</strong><br/><br/>" + 
                "To avoid a lapse in your service, please update your payment information at the following link:<br/><br/>" +
                "<a href='https://{1}." + Endpoints.URLs.AccountManagementUpdateCardUrl + "'>Update Payment Information</a></br></br>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class InvoicedChargeFailed_SecondDunningAttempt
        {
            public static string FromName = Application.Name;
            public static string Subject = "Second attempt to pay for your subscription has failed";
            public static string Body = "Hello {0},<br/><br/>" +
                "There was a second attempt made to resolve payment issues for your subscription.<br/><br/>" +
                "The issue with the charge is due to the following:<br/><br/><strong>{2}</strong><br/><br/>" + 
                "To avoid account closure and loss of your data please update your payment information at the following link:<br/><br/>" +
                "<a href='https://{1}." + Endpoints.URLs.AccountManagementUpdateCardUrl + "'>Update Payment Information</a></br></br>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class InvoicedChargeFailed_ThirdDunningAttempt
        {
            public static string FromName = Application.Name;
            public static string Subject = "Third attempt to pay for your subscription has failed";
            public static string Body = "Hello {0},<br/><br/>" +
                "There was a third attempt made to resolve payment issues for your subscription.<br/><br/>" +
                "This error is due to the following:<br/><br/><strong>{2}</strong><br/><br/>" + 
                "There will be one final attempt made before your account is closed. To avoid closure and unrecoverable loss of your data please update your payment information at the following link:<br/><br/>" +
                "<a href='https://{1}." + Endpoints.URLs.AccountManagementUpdateCardUrl + "'>Update Payment Information</a></br></br>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class InvoicedChargeFailed_FinalDunningAttempt
        {
            public static string FromName = Application.Name;
            public static string Subject = "FINAL NOTICE: Attempt to pay for your subscription has failed";
            public static string Body = "Hello {0},<br/><br/>" +
                "There was a final attempt made to resolve payment issues for your subscription.<br/><br/>" +
                "<br/><br/>Your account will be closed. We will attempt to contact you to resolve payment. If we cannot contact you in a timely manner, your account will be purged and your data will be lost. <br/><br/>" +
                "<br/><br/>Please email us immediately to resolve your payment issues:<br/><br/>" +
                "<a href='mailto:" + Settings.Endpoints.Emails.FromBilling + "?subject=Resolve Payment For {0}'>Email us now</a></br></br>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        public static class InvoicedChargeFailed_AccountDeactivated
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your account has been deactivated";
            public static string Body = "Hello {0},<br/><br/>" +
                "Due to payment issues with your subscription, your account has been deactvated.<br/><br/>" +
                "<br/><br/>If we cannot contact you in a timely manner to resolve payment, your account will be purged and your data will be lost. <br/><br/>" +
                "<br/><br/>Please email us immediately to reopen your account:<br/><br/>" +
                "<a href='mailto:" + Settings.Endpoints.Emails.FromBilling + "?subject=Resolve Payment For {0}'>Email us now</a></br></br>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }


        public static class FinalAccountClosureMessage
        {
            public static string FromName = Application.Name;
            public static string Subject = "Your account has been marked for closure!";
            public static string Body = "Hello {0},<br/><br/>" +
                "After multiple attempts to resolve payment issues we have deacticated your account." +
                "<br/><br/>To avoid losing your data after our short grace period, please contact us immediatly so the we can resolve the issue manually.<br/><br/>" +
                "Thank you,<br/><br/>The " + Application.Name + " Team";
        }

        #endregion

        #region Account Users: User Invitations & Password Claims

        public static class AccountUserInvitation
        {
            public static string Subject = "An invitation from {0}";
            public static string Body = "Hello {0},<br/><br/>You've been invited to join the <b>'{1}'</b> account on " + Application.Name + ".</br></br>Please create your profile with the link below:<br></br>" +
                "<a href='https://{2}." + Endpoints.URLs.AccountUserAcceptInvitationUrl + "/{3}'>Create my profile</a></br></br>" +
                "Thank you,</br></br>" +
                "The " + Application.Name + " Team";

        }


        public static class AccountUserPasswordClaim
        {
            public static string Subject = "Password Reset";
            public static string Body = "Hello {0},<br/><br/>Reset your password for the <b>'{1}'</b> account on " + Application.Name + " with the link below:<br></br>" +
                "<a href='https://{2}." + Endpoints.URLs.AccountUserPasswordClaimUrl + "/{3}'>Reset password</a></br></br>" +
                "Thank you,</br></br>" +
                "The " + Application.Name + " Team";
        }

        #endregion

        #region Platform Users: User Invitations & Password Claims

        public static class PlatformInvitation
        {
            public static string Subject = "An invitation from {0}";
            public static string Body = "Hello {0},<br/><br/>You've been invited to join the <b>" + Application.Name + "</b> platform administration system.</br></br>Please create your profile with the link below:<br></br>" +
                "<a href='" + Endpoints.URLs.PlatformUserAcceptInvitationUrl + "/{1}'>Create my profile</a></br></br>" +
                "Thank you,</br></br>" +
                "The " + Application.Name + " Team";

        }


        public static class PlatformPasswordClaim
        {
            public static string Subject = "Password Reset";
            public static string Body = "Hello {0},<br/><br/>Reset your " + Application.Name + " Platform user password with the link below:<br></br>" +
                "<a href='" + Endpoints.URLs.PlatformUserPasswordClaimUrl + "/{1}'>Reset password</a></br></br>" +
                "Thank you,</br></br>" +
                "The " + Application.Name + " Team";
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
