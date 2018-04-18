using System;
using System.Linq;
using Sahara.Core.Accounts;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Platform.Custodian.Internal;
using Sahara.Core.Platform.Deprovisioning;
using Sahara.Core.Accounts.Notifications;
using Sahara.Core.Common.Services.Stripe;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;

namespace Sahara.Core.Platform.Custodian
{
    public static class CustodianManager
    {
        #region Account Tasks

        public static DataAccessResponseType DeprovisionClosedAccounts()
        {
            var response = new DataAccessResponseType();

            try
            {
                // 1. Get list of AccountID's that are past their AccountEndDate and have ClosueApproved set to TRUE. These accounts are ready for deprovisioning:
                var accountsToDeprovision = Sql.Statements.SelectStatements.SelectClosedAccountsToDeprovision();
                if (accountsToDeprovision.Count > 0)
                {
                    foreach (string accountID in accountsToDeprovision)
                    {
                        // Get the Account
                        //var account = AccountManager.GetAccountByID(accountID, false);
                        var account = AccountManager.GetAccount(accountID, false, AccountManager.AccountIdentificationType.AccountID);

                        // Delete the account, associated user(s) and all data for each account ID the open up each data partition for future accounts:
                        var deprovisioningResponse = DeprovisioningManager.DeprovisionAccount(account);

                        // Log Custodian Activity
                        //PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Scheduled_Task, 
                            //account.AccountName + " has been deprovisioned. (" + account.AccountID + ") ",
                            //"Check deprovisioning log for details.", account.AccountID.ToString(), account.AccountName);

                    }

                    response.isSuccess = true;
                    response.SuccessMessage = "Closed account(s) have been deprovisioned, see deprovisioning log for details.";
                }
                else
                {
                    // No accounts to deprovision...
                    response.isSuccess = true;
                    response.SuccessMessage = "No accounts found for deprovisioning.";

                    // Log (Commented out to make custodian less noisy)
                    //PlatformLogManager.LogActivity(CustodianLogActivity.ScheduledTask, ""No accounts found for deprovisioning.");
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to deprovision closed accounts",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            return response;
        }

        public static DataAccessResponseType SendCreditCardExpirationReminders()
        {
            var response = new DataAccessResponseType();

            try
            {

                foreach (int daysTillExpiration in Sahara.Core.Settings.Platform.Custodian.Dunning.ReminderDaysTillCardExpiration)
                {

                    var timeLeftDescription = string.Empty;

                    // 1. Get list of AccountID's that have a credit card within the window of time for this expiration reminder:
                    var accountsToRemind = Sql.Statements.SelectStatements.SelectAccountIDsForCreditCardExpirationReminders(daysTillExpiration);

                    if (accountsToRemind.Count > 0)
                    {
                        foreach (string accountID in accountsToRemind)
                        {
                            //Check email reminders log to see if this reminder epiration has already been sent to the account:
                            bool emailReminderSent = CardExpirationReminderEmailsLogManager.HasEmailBeenSent(accountID, daysTillExpiration);

                            if (!emailReminderSent)
                            {
                                //Get the account
                                //var account = AccountManager.GetAccountByID(accountID, false);
                                var account = AccountManager.GetAccount(accountID);

                                #region Generate Time Left Description

                                if (daysTillExpiration < 0)
                                {
                                    // description not used
                                }
                                else if (daysTillExpiration > 0 && daysTillExpiration <= 1)
                                {
                                    timeLeftDescription = "in about a day";
                                }
                                else if (daysTillExpiration > 1 && daysTillExpiration <= 2)
                                {
                                    timeLeftDescription = "in a couple of days";
                                }
                                else if (daysTillExpiration > 2 && daysTillExpiration <= 3)
                                {
                                    timeLeftDescription = "in a few days";
                                }
                                else if (daysTillExpiration > 3 && daysTillExpiration <= 5)
                                {
                                    timeLeftDescription = "this week";
                                }
                                else if (daysTillExpiration > 5 && daysTillExpiration <= 12)
                                {
                                    timeLeftDescription = "next week";
                                }
                                else if (daysTillExpiration > 12 && daysTillExpiration <= 15)
                                {
                                    timeLeftDescription = "in a couple of weeks";
                                }
                                else if (daysTillExpiration > 15 && daysTillExpiration <= 30)
                                {
                                    timeLeftDescription = "this month";
                                }
                                else if (daysTillExpiration > 30 && daysTillExpiration <= 62)
                                {
                                    timeLeftDescription = "next month";
                                }
                                else if (daysTillExpiration > 62 && daysTillExpiration <= 92)
                                {
                                    timeLeftDescription = "in a couple of months";
                                }
                                else if (daysTillExpiration > 92 && daysTillExpiration <= 180)
                                {
                                    timeLeftDescription = "in a few months";
                                }
                                else
                                {
                                    timeLeftDescription = "soon";
                                }

                                #endregion

                                if (daysTillExpiration < 0)
                                {
                                    //After the expiration occurs we send a more alarming message from ALERTS
                                    AccountManager.SendEmailToAccount(
                                    accountID,
                                    Settings.Endpoints.Emails.FromAlerts,
                                    Settings.Copy.EmailMessages.CreditCardExpirationMessage.FromName,
                                    Settings.Copy.EmailMessages.CreditCardExpirationMessage.Subject,
                                    String.Format(Settings.Copy.EmailMessages.CreditCardExpirationMessage.Body, account.AccountName, account.AccountNameKey),
                                    true,
                                    true
                                    );

                                    //We also send platform admins an email
                                    EmailManager.Send(
                                            Sahara.Core.Settings.Endpoints.Emails.PlatformEmailAddresses,
                                            Sahara.Core.Settings.Endpoints.Emails.FromPlatform,
                                            "Account Credit Card Expired",
                                            "An accounts credit card has expired!",
                                            "The <b>" + account.AccountName + "</b> account has an expired credit card. Please reach out to them manually to avoid potential billing issues.",
                                            true,
                                            true
                                        );
                                    
                                }
                                else
                                {
                                    //If time remains we send a friendly reminder from REMINDERS
                                    AccountManager.SendEmailToAccount(
                                    accountID,
                                    Settings.Endpoints.Emails.FromReminders,
                                    Settings.Copy.EmailMessages.CreditCardExpirationReminder.FromName,
                                    Settings.Copy.EmailMessages.CreditCardExpirationReminder.Subject,
                                    String.Format(Settings.Copy.EmailMessages.CreditCardExpirationReminder.Body, account.AccountName, timeLeftDescription, account.AccountNameKey),
                                    true,
                                    true
                                    );   
                                }
                                


                                //Log activity
                                /*
                                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_CardExpirationReminder_EmailSent,
                                    account.AccountName +
                                    " (" + account.AccountID.ToString() +
                                    ") was sent a " + daysTillExpiration + " day credit card expiration reminder.",
                                    "",account.AccountID.ToString(), account.AccountName);*/
                            }

                        }

                        response.isSuccess = true;
                    }
                    else
                    {
                        // No accounts to email...
                        response.isSuccess = true;
                        response.SuccessMessage = "No accounts to remind for ";

                    }

                }

                response.isSuccess = true;
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to send credit card expiration reminders",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            return response;
        }

        #endregion

        #region Data Cleanup Tasks

        public static DataAccessResponseType ClearIntermediaryStorage()
        {
            var response = new DataAccessResponseType();

            var daysAgo = (Sahara.Core.Settings.Platform.GarbageCollection.IntermediaryStorageContainerDaysToPurge * -1);

            try
            {
                //Clients that wish to save source files to intermediary blob storage mush use the following date format to name the container for custodial garbage collection to take place.
                var date = DateTime.UtcNow.AddDays(daysAgo);
                var containerName = date.ToShortDateString().Replace("/", "-");

                //Delete Storage Containers on Intermediary Labeled from "X" days ago
                CloudBlobClient blobClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.IntermediateStorage.CreateCloudBlobClient();

                //Create and set retry policy
                IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 8);
                blobClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;


                if (blobClient.GetContainerReference(containerName).Exists())
                {
                    blobClient.GetContainerReference(containerName).DeleteIfExists();

                    //Log Garbage Collection
                    PlatformLogManager.LogActivity(
                        CategoryType.GarbageCollection,
                        ActivityType.GarbageCollection_IntermediaryStorage,
                        "Purged the '" + containerName + "' intermediary storage container",
                        "An intermediary storage container older than " + Sahara.Core.Settings.Platform.GarbageCollection.IntermediaryStorageContainerDaysToPurge + " days has been purged"
                        );
                }


                response.isSuccess = true;


            }
            catch(Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to purge intermediary data past " + Sahara.Core.Settings.Platform.GarbageCollection.IntermediaryStorageContainerDaysToPurge + " days old",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            return response;
        }

        public static DataAccessResponseType ClearLogs()
        {
            var response = new DataAccessResponseType();

            //Clear logs
            //ToDo Seperate logs by month or year???

            response.isSuccess = true;
            //response.ErrorMessage = "Not implemented.";

            return response;
        }

        public static DataAccessResponseType ClearCreditCardExpirationRemindersLog()
        {
            var response = new DataAccessResponseType();

            try
            {
                //Clear the CreditCardExpirationRemindersLog log for records older than X days
                response.isSuccess = CardExpirationReminderEmailsLogManager.ClearReminderEmailsLog(Sahara.Core.Settings.Platform.GarbageCollection.CreditCardExpirationReminderEmailsLogDaysToPurge);

            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to clear credit card expiration reminders log",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            return response;
        }

        public static DataAccessResponseType ClearStripeWebhooksLog()
        {
            var response = new DataAccessResponseType();

            try
            {

                int amountOfDays = Sahara.Core.Settings.Platform.GarbageCollection.StripeWebhookEventLogDaysToPurge;

                CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

                //Create and set retry policy
                //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
                IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
                cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

                CloudTable cloudTable = cloudTableClient.GetTableReference("stripewebhookeventslog");

                cloudTable.CreateIfNotExists();

                TableQuery<TableEntity> query = new TableQuery<TableEntity>()
                    .Where(TableQuery.GenerateFilterConditionForDate("DateTimeUTC", QueryComparisons.LessThanOrEqual, DateTimeOffset.UtcNow.AddDays(amountOfDays * -1)));

                var stripeWebhooks = cloudTable.ExecuteQuery(query);

                int count = stripeWebhooks.Count();

                foreach (var log in stripeWebhooks)
                {
                    cloudTable.Execute(TableOperation.Delete(log));
                }

                if (count > 0)
                {
                    //Log Garbage Collection
                    PlatformLogManager.LogActivity(
                        CategoryType.GarbageCollection,
                        ActivityType.GarbageCollection_StripeEventLog,
                        "Purged " + count.ToString("#,##0") + " item(s) from the stripe webhook events logs",
                        count.ToString("#,##0") + " stripe webhook event(s) past " + amountOfDays + " days have been purged"
                        );
                }

                response.isSuccess = true;

            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to clear stripe webhooks log",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            return response;
        }

        #endregion

        #region Keep Alive Tasks

        public static DataAccessResponseType PingSites()
        {
            //Set up security channel to allow for SSL/TLS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var response = new DataAccessResponseType();


            #region Marketing site 1

            try
            {
                string siteName = "marketing site (1)";
                string url = "https://" + Settings.Endpoints.URLs.MasterDomain;

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging registration site (1): " + e.Message);
            }

            #endregion

            #region Marketing site 2

            try
            {
                string siteName = "marketing site (2)";
                string url = "https://www." + Settings.Endpoints.URLs.MasterDomain;

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging registration site (2): " + e.Message);
            }

            #endregion

            #region Registration site

            try
            {
                string siteName = "registration site";
                string url = Settings.Endpoints.URLs.RegistrationUrl;

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging registration site: " + e.Message);
            }

            #endregion


            #region Subscription site

            try
            {
                string siteName = "subscription site";
                string url = "https://subscribe." + Settings.Endpoints.URLs.MasterDomain;

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging subscription site: " + e.Message);
            }

            #endregion

            #region Platform admin

            try
            {
                string siteName = "platform admin";
                string url = "https://" + Settings.Endpoints.URLs.PlatformDomain;

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging platform admin: " + e.Message);
            }

            #endregion

            #region Account Admin site

            try
            {
                string siteName = "accounts admin site";
                string url = "https://accounts." + Settings.Endpoints.URLs.AccountManagementDomain;

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging accounts admin site: " + e.Message);
            }

            #endregion

            #region Account API

            try
            {
                string siteName = "accounts api";
                string url = "https://accounts." + Settings.Endpoints.URLs.AccountServiceDomain;

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging accounts api: " + e.Message);
            }

            #endregion

            #region Account Sites

            try
            {
                string siteName = "accounts site";
                string url = "https://accounts." + Settings.Endpoints.URLs.AccountSiteDomain;

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging accounts site: " + e.Message);
            }

            #endregion

            #region Imaging API

            try
            {
                string siteName = "imaging api";
                string url = Settings.Endpoints.URLs.ImagingApiUrl;

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging imaging api: " + e.Message);
            }

            #endregion

            #region Registration API

            try
            {
                string siteName = "registration api";
                string url = Settings.Endpoints.URLs.RegistrationApiEndpoint + "/deploymentEnvironments/local";


                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging registration api: " + e.Message);
            }

            #endregion

            #region Registration API (AccountName Validation)


            try
            {
                string siteName = "api account name validator";
                string url = Settings.Endpoints.URLs.RegistrationApiEndpoint + "/validation/accountname";

                //Post to the AccountName Validator to make this really speedy:
                var values = new Dictionary<string, string>
                {
                   { "accountname", "kaz" }
                };

                var content = new FormUrlEncodedContent(values);
                var responseString = "";

                using (var client = new HttpClient())
                {
                    var postResponse = client.PostAsync(url, content).Result;
                    responseString = postResponse.Content.ReadAsStringAsync().Result;
                }

                try
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "posted to  " + siteName + " response:" + responseString);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error posting to " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pstong to api account name validator: " + e.Message);
            }



            #endregion

            #region Webhooks API

            try
            {
                string siteName = "stripe webhooks api";
                string url = "http://webhooks." + Settings.Endpoints.URLs.ApiDomain + "/DeploymentEnvironments"; //<-- We use HTTP here only as there are no certs on cloud service.

                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                //optional
                HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                try
                {
                    Stream stream = webRequestResponse.GetResponseStream();
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_KeepAlive_Status, "Pinged " + siteName + " at:" + url);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging " + siteName + ": " + e.Message);
                }
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error pinging stripe webhooks: " + e.Message);
            }

            #endregion

            response.isSuccess = true;
            //response.ErrorMessage = "Not implemented.";

            return response;
        }

        #endregion


        #region Caching Tasks

        public static DataAccessResponseType CacheAccountData()
        {

            //Set up security channel to allow for SSL/TLS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };


            var response = new DataAccessResponseType();


            var accounts = AccountManager.GetAllAccountsByFilter("Provisioned", "1", 0, 2000, "AccountNameKey");

            //Loop through each account and call public api to preload data
            foreach(var account in accounts)
            {
                #region Account Info

                try
                {
                    string callName = "account";
                    string url = "https://" + account.AccountNameKey + "." + Settings.Endpoints.URLs.AccountServiceDomain + "/account";

                    HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                    //optional
                    HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                    try
                    {
                        Stream stream = webRequestResponse.GetResponseStream();
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_RefreshCache_Status, "Refreshing " + callName + " api cache for '" + account.AccountName + "' at:" + url);
                    }
                    catch (Exception e)
                    {
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing " + callName + " api cache for '" + account.AccountName + "': " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing account api cache for '" + account.AccountName + ": " + e.Message);
                }

                #endregion

                #region Category Tree

                try
                {
                    string callName = "categories";
                    string url = "https://" + account.AccountNameKey + "." + Settings.Endpoints.URLs.AccountServiceDomain + "/categories/tree?includeImages=true";

                    HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                    //optional
                    HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                    try
                    {
                        Stream stream = webRequestResponse.GetResponseStream();
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_RefreshCache_Status, "Refreshing " + callName + " api cache for '" + account.AccountName + "' at:" + url);
                    }
                    catch (Exception e)
                    {
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing " + callName + " api cache for '" + account.AccountName + "': " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing categories api cache for '" + account.AccountName + ": " + e.Message);
                }

                #endregion

                #region Search Query

                try
                {
                    string callName = "search";
                    string url = "https://" + account.AccountNameKey + "." + Settings.Endpoints.URLs.AccountServiceDomain + "/search/products?query=test?skip=0&take=2";

                    HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                    //optional
                    HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                    try
                    {
                        Stream stream = webRequestResponse.GetResponseStream();
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_RefreshCache_Status, "Refreshing " + callName + " api cache for '" + account.AccountName + "' at:" + url);
                    }
                    catch (Exception e)
                    {
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing " + callName + " api cache for '" + account.AccountName + "': " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing search api cache for '" + account.AccountName + ": " + e.Message);
                }

                #endregion

                #region Search Sortables

                try
                {
                    string callName = "search-sortables";
                    string url = "https://" + account.AccountNameKey + "." + Settings.Endpoints.URLs.AccountServiceDomain + "/search/sortables";

                    HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                    //optional
                    HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                    try
                    {
                        Stream stream = webRequestResponse.GetResponseStream();
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_RefreshCache_Status, "Refreshing " + callName + " api cache for '" + account.AccountName + "' at:" + url);
                    }
                    catch (Exception e)
                    {
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing " + callName + " api cache for '" + account.AccountName + "': " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing search-sortables api cache for '" + account.AccountName + ": " + e.Message);
                }

                #endregion

                #region Search Facets

                try
                {
                    string callName = "search-facets";
                    string url = "https://" + account.AccountNameKey + "." + Settings.Endpoints.URLs.AccountServiceDomain + "/search/facets";

                    HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                    //optional
                    HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                    try
                    {
                        Stream stream = webRequestResponse.GetResponseStream();
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_RefreshCache_Status, "Refreshing " + callName + " api cache for '" + account.AccountName + "' at:" + url);
                    }
                    catch (Exception e)
                    {
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing " + callName + " api cache for '" + account.AccountName + "': " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing search-facets api cache for '" + account.AccountName + ": " + e.Message);
                }

                #endregion

                #region Properties

                try
                {
                    string callName = "properties";
                    string url = "https://" + account.AccountNameKey + "." + Settings.Endpoints.URLs.AccountServiceDomain + "/properties/product";

                    HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                    //optional
                    HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                    try
                    {
                        Stream stream = webRequestResponse.GetResponseStream();
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_RefreshCache_Status, "Refreshing " + callName + " api cache for '" + account.AccountName + "' at:" + url);
                    }
                    catch (Exception e)
                    {
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing " + callName + " api cache for '" + account.AccountName + "': " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing properties api cache for '" + account.AccountName + ": " + e.Message);
                }

                #endregion

                #region Tags

                try
                {
                    string callName = "tags";
                    string url = "https://" + account.AccountNameKey + "." + Settings.Endpoints.URLs.AccountServiceDomain + "/tags";

                    HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                    //optional
                    HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                    try
                    {
                        Stream stream = webRequestResponse.GetResponseStream();
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_RefreshCache_Status, "Refreshing " + callName + " api cache for '" + account.AccountName + "' at:" + url);
                    }
                    catch (Exception e)
                    {
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing " + callName + " api cache for '" + account.AccountName + "': " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing tags api cache for '" + account.AccountName + ": " + e.Message);
                }

                #endregion

            }

            //Loop through each account and call public site to preload pages
            foreach (var account in accounts)
            {
                #region Public Site Search

                try
                {
                    string callName = "account-search";
                    string url = "https://" + account.AccountNameKey + "." + Settings.Endpoints.URLs.AccountSiteDomain + "/search";

                    HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                    //optional
                    HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                    try
                    {
                        Stream stream = webRequestResponse.GetResponseStream();
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_RefreshAccountSite_Status, "Refreshing " + callName + " site cache for '" + account.AccountName + "' at:" + url);
                    }
                    catch (Exception e)
                    {
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing " + callName + " site cache for '" + account.AccountName + "': " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing account-search site cache for '" + account.AccountName + ": " + e.Message);
                }

                #endregion


                #region Public Site Search

                try
                {
                    string callName = "account-home";
                    string url = "https://" + account.AccountNameKey + "." + Settings.Endpoints.URLs.AccountSiteDomain;

                    HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;

                    //optional
                    HttpWebResponse webRequestResponse = webRequest.GetResponse() as HttpWebResponse;

                    try
                    {
                        Stream stream = webRequestResponse.GetResponseStream();
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_RefreshAccountSite_Status, "Refreshing " + callName + " site cache for '" + account.AccountName + "' at:" + url);
                    }
                    catch (Exception e)
                    {
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing " + callName + " site cache for '" + account.AccountName + "': " + e.Message);
                    }
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Error_Custodian, "Error refreshing account-home site cache for '" + account.AccountName + ": " + e.Message);
                }

                #endregion
            }


            response.isSuccess = true;
            //response.ErrorMessage = "Not implemented.";

            return response;
        }

        #endregion
    }
}
