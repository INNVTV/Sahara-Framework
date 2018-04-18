using System;
using System.Collections.Generic;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Notifications;
using Sahara.Core.Accounts.Notifications.Models;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Platform.Provisioning;
using Sahara.Core.Common.Services.Stripe;
using System.Text;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Platform.Billing;
using Sahara.Core.Application.DocumentModels.Application.ApplicationImages.DocumentModels;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using System.Diagnostics;
using Microsoft.Azure.Documents.Client.TransientFaultHandling.Strategies;
using System.Threading;
using Newtonsoft.Json;
using Sahara.Core.Accounts.Capacity.Public;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sahara.Core.Platform.BackgroundTasks
{
    public static class BackgroundTasksManager
    {
        #region Registration Operations

        public static DataAccessResponseType ProcessAccountProvisioning(string accountId)
        {
            //var response = ProvisioningManager.ProvisionAccount(AccountManager.GetAccountByID(accountID, true));
            var response = ProvisioningManager.ProvisionAccount(AccountManager.GetAccount(accountId, false, AccountManager.AccountIdentificationType.AccountID));

            return response;
        }


        #endregion

        #region STRIPE Webhook Operations

        public static DataAccessResponseType ProcessSuccessfulStripeChargeEvent(string stripeCustomerId, string stripeChargeId, string stripeCardId, string StripeInvoiceId, string StripeEventId)
        {
            var response = new DataAccessResponseType();

            //Get all owners for the account:
            var accountOwnerEmails = AccountManager.GetAccountOwnerEmails(stripeCustomerId);
            //Get the account
            //var account = AccountManager.GetAccountByStripeCustomerID(stripeCustomerID, false);
            var account = AccountManager.GetAccount(stripeCustomerId);

            //Get the associated Stripe Invoice:
            var stripeManager = new StripeManager();

            
            var stripeCharge = stripeManager.GetCharge(stripeChargeId);
            var stripeCard = stripeManager.GetCard(stripeCustomerId, account.StripeCardID); //<-- in case card is updated we use the latest stripe card ID

            //Generate dollar amount
            var chargeTotalAmount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(stripeCharge.Amount.ToString());

            //For HTML invoice or charge details string
            var htmlPaymentDetailsString = string.Empty;

            if (!String.IsNullOrEmpty(StripeInvoiceId))
            {
                //Charge has in invoice associated
                var stripeInvoice = stripeManager.GetInvoice(StripeInvoiceId);
                
                //Generate HTML invoice
                htmlPaymentDetailsString = Sahara.Core.Common.Methods.Billing.GenerateStripeHTMLInvoice(stripeInvoice);

            }
            else
            {
                //Generate HTML charge details (No associated invoice exists)
                htmlPaymentDetailsString = Sahara.Core.Common.Methods.Billing.GenerateStripeHTMLChargeDetails(stripeCharge);
            }

            //email all account owners a copy of the associated charge/invoice details
            EmailManager.Send(
                    accountOwnerEmails,
                    Settings.Endpoints.Emails.FromBilling,
                    Settings.Copy.EmailMessages.AccountCharged.FromName,
                    String.Format(Settings.Copy.EmailMessages.AccountCharged.Subject),
                    String.Format(Settings.Copy.EmailMessages.AccountCharged.Body, account.AccountName, stripeCard.CardBrand, stripeCard.Last4, chargeTotalAmount, htmlPaymentDetailsString),
                    true);

            //Clear Billing Issues Marker From Account
            //????/

            response.isSuccess = true;

            return response;
        }

        public static DataAccessResponseType ProcessFailedStripeRecurringChargeEvent(string stripeCustomerID, string stripeChargeId, string amount, string failureMessage, string stripeEventId)
        {
            /* FROM STRIPE DOCS (why we only focus on the charge failure since we ensure a card is always on file:
             * 
             * We'll let you know about this case with an 'invoice.payment_failed' event.
             * The payment could have failed either because your customer did not have a card on file or because the charge was declined;
             * if it was due to a decline, we'll also emit a 'charge.failed event'.
             * 
             * You'll also see a 'customer.updated' event to set the 'delinquent' flag,
             * an 'invoice.updated' event to track the payment attempts, and 'customer.subscription.updated'
             * or 'customer.subscription.deleted' depending on your retry settings.
             * 
             * */

            var response = new DataAccessResponseType();

            // 1. Get the account
            var account = AccountManager.GetAccount(stripeCustomerID);

            // 2. Update account Delinquent state
            AccountManager.UpdateAccountDelinquentStatus(account.AccountID.ToString(), true);

            // 3. Determine number of previous autmatic failures, and adjust messaging severity accordingly
            int previousFailureCount = PlatformBillingManager.GetAutomaticDunningAttemptCount(account.AccountID.ToString(), account.StoragePartition);

            //Pull count of past failures from table storage rows
            // Details shoul include:
                // TieStamp
                // Charge amount
                // Stripe ID's (charge, event, etc)


            // 4. Get all owners for the account:
            var accountOwnerEmails = AccountManager.GetAccountOwnerEmails(stripeCustomerID);

            if (previousFailureCount == 0)
            {
                //First attempt, light messagng
                //Email all account owners
                    //email all account owners a copy of the associated invoice
                    EmailManager.Send(
                            accountOwnerEmails,
                            Settings.Endpoints.Emails.FromBilling,
                            Settings.Copy.EmailMessages.InvoicedChargeFailed_FirstDunningAttempt.FromName,
                            String.Format(Settings.Copy.EmailMessages.InvoicedChargeFailed_FirstDunningAttempt.Subject),
                            String.Format(Settings.Copy.EmailMessages.InvoicedChargeFailed_FirstDunningAttempt.Body, account.AccountName, account.AccountNameKey, failureMessage),
                            true);
                

            }
            else if (previousFailureCount == 1)
            {
                //This was the second attempt, email is more sever and includes platform admins
                //Email all account owners

                    //email all account owners a copy of alert email
                    EmailManager.Send(
                            accountOwnerEmails,
                            Settings.Endpoints.Emails.FromBilling,
                            Settings.Copy.EmailMessages.InvoicedChargeFailed_SecondDunningAttempt.FromName,
                            String.Format(Settings.Copy.EmailMessages.InvoicedChargeFailed_SecondDunningAttempt.Subject),
                            String.Format(Settings.Copy.EmailMessages.InvoicedChargeFailed_SecondDunningAttempt.Body, account.AccountName, account.AccountNameKey, failureMessage),
                            true);
                

                //Email Platform Admins
                EmailManager.Send(
                            Settings.Endpoints.Emails.PlatformEmailAddresses,
                            Settings.Endpoints.Emails.FromPlatform,
                            "Second Failed Charge Warning",
                            "An account has been sent a second warning about failed charge",
                            "AccountName: <b>" + account.AccountName + "</b><br/><br/>AccountID: <b>" + account.AccountID + "</b><br/><br/>Failure Message: <b>" + failureMessage + "</b><br/>",
                            true);

            }
            else if (previousFailureCount == 2)
            {
                //This was the third attempt, email is more sever and includes platform admins
                //Email all account owners

                    //email all account owners a copy of alert email
                    EmailManager.Send(
                            accountOwnerEmails,
                            Settings.Endpoints.Emails.FromBilling,
                            Settings.Copy.EmailMessages.InvoicedChargeFailed_ThirdDunningAttempt.FromName,
                            String.Format(Settings.Copy.EmailMessages.InvoicedChargeFailed_ThirdDunningAttempt.Subject),
                            String.Format(Settings.Copy.EmailMessages.InvoicedChargeFailed_ThirdDunningAttempt.Body, account.AccountName, account.AccountNameKey, failureMessage),
                            true);
                

                //Email Platform Admins
                EmailManager.Send(
                            Settings.Endpoints.Emails.PlatformEmailAddresses,
                            Settings.Endpoints.Emails.FromPlatform,
                            "Third Failed Charge Warning",
                            "An account has been sent a third warning about failed charge",
                            "AccountName: <b>" + account.AccountName + "</b><br/><br/>AccountID: <b>" + account.AccountID + "</b><br/><br/>Failure Message: <b>" + failureMessage + "</b><br/>",
                            true);

                //Send alert notification with a 7 day expiration to the account owners (or do this on the second attempt?)
                /* NOTIFICATIONS TURNED OFF
                NotificationsManager.SendNotificationToAccount(
                    NotificationType.Alert,
                    account.AccountID.ToString(),
                    Settings.Copy.NotificationMessages.ChargeFailure_Automatic.NotificationMessage,
                    10080, //<-- 7 Days (Length of time until Stripe makes final attempt)
                    true
                    );
                    */

                // Send email to Platform Admins
            }
            else if(previousFailureCount == 3)
            {
                //This was the fourth and final attempt, after this Stripe has marked the Subscritption as 'unpaid'. Messaing is a dire warning about account closure
                //Email all account owners

                    //email all account owners a copy of alert email
                    EmailManager.Send(
                            accountOwnerEmails,
                            Settings.Endpoints.Emails.FromBilling,
                            Settings.Copy.EmailMessages.InvoicedChargeFailed_FinalDunningAttempt.FromName,
                            String.Format(Settings.Copy.EmailMessages.InvoicedChargeFailed_FinalDunningAttempt.Subject),
                            String.Format(Settings.Copy.EmailMessages.InvoicedChargeFailed_FinalDunningAttempt.Body, account.AccountName),
                            true);
                


                //Email Platform Admins
                EmailManager.Send(
                            Settings.Endpoints.Emails.PlatformEmailAddresses,
                            Settings.Endpoints.Emails.FromPlatform,
                            "FINAL Failed Charge Warning",
                            "An account has been sent a final warning about failed charge",
                            "AccountName: <b>" + account.AccountName + "</b><br/><br/>AccountID: <b>" + account.AccountID + "</b><br/><br/>Failure Message: <b>" + failureMessage + "</b><br/>",
                            true);
            }


            //Store failure (increase failure count by 1)
            PlatformBillingManager.StoreAutomaticDunningAttempt(account.AccountID.ToString(), account.StoragePartition, stripeChargeId, amount, account.StripeSubscriptionID, stripeEventId, failureMessage);
     

            response.isSuccess = true;

            return response;
        }

        public static DataAccessResponseType ProcessFailedStripeChargeEvent(string stripeCustomerID, string stripeChargeId, string amount, string failureMessage, string stripeEventId)
        {
            /* FROM STRIPE DOCS (why we only focus on the charge failure since we ensure a card is always on file:
             * 
             * This type of charge is a one off and does not require any dunning, just a simple email to all account owners regarding the charge issue
             * 
             * */

            var response = new DataAccessResponseType();

            // 1. Get the account
            var account = AccountManager.GetAccount(stripeCustomerID);

            // 2. Get all owners for the account:
            var accountOwnerEmails = AccountManager.GetAccountOwnerEmails(stripeCustomerID);


                    //email all account owners a copy of the associated invoice
                    EmailManager.Send(
                            accountOwnerEmails,
                            Settings.Endpoints.Emails.FromBilling,
                            Settings.Copy.EmailMessages.ChargeFailed.FromName,
                            String.Format(Settings.Copy.EmailMessages.ChargeFailed.Subject),
                            String.Format(Settings.Copy.EmailMessages.ChargeFailed.Body, account.AccountName, account.AccountNameKey, failureMessage),
                            true);
                

            response.isSuccess = true;

            return response;
        }

        public static DataAccessResponseType ProcessStripeCustomerDelinquencyChangedEvent(string accountId, string storagePartition, bool newDelinquencyStatus)
        {
            var response = new DataAccessResponseType();

            if(newDelinquencyStatus == false)
            {
                //Account is no longer delinquent according to stripe.

                //Turn off Delinquent state
                AccountManager.UpdateAccountDelinquentStatus(accountId, false);

                //Clear DunningAttempts Table:
                PlatformBillingManager.ClearAutomaticDunningAttempt(accountId, storagePartition);
            }
            else if (newDelinquencyStatus == true)
            {
                //Account is delinquent according to stripe.

                //Turn on Delinquent state:
                AccountManager.UpdateAccountDelinquentStatus(accountId, true);
            }

            response.isSuccess = true;

            return response;
        }

        public static DataAccessResponseType ProcessStripeSubscriptionStatusChangedEvent(Account account, string newSubscriptionStatus, string previousSubscriptionStatus)
        {
            var response = new DataAccessResponseType();

            
            if (newSubscriptionStatus == "active" && previousSubscriptionStatus == "unpaid" || 
                newSubscriptionStatus == "active" && previousSubscriptionStatus == "past_due")
            {
                // Turn off delinquncy, clear dunning & reactive the account
                AccountManager.UpdateAccountActiveState(account.AccountID.ToString(), true);
                AccountManager.UpdateAccountDelinquentStatus(account.AccountID.ToString(), false);
                PlatformBillingManager.ClearAutomaticDunningAttempt(account.AccountID.ToString(), account.StoragePartition);

            }
            else if (newSubscriptionStatus == "past_due" && previousSubscriptionStatus == "active")
            {
                // Turn on delinquncy: The 'ProcessFailedStripeInvoicedChargeEvent' will handle the dunning emails
                AccountManager.UpdateAccountDelinquentStatus(account.AccountID.ToString(), true);

            }
            else if (newSubscriptionStatus == "unpaid" && previousSubscriptionStatus == "past_due")
            {

                // 1. Deactivate the account, assure that delinquent is still true send final email
                AccountManager.UpdateAccountActiveState(account.AccountID.ToString(), false);
                AccountManager.UpdateAccountDelinquentStatus(account.AccountID.ToString(), true);

                // 2. get the account
                //var account = AccountManager.GetAccount(accountId);

                // 3. Get all owners for the account:
                var accountOwnerEmails = AccountManager.GetAccountOwnerEmails(account.AccountID.ToString());

                //Stripe has marked the Subscritption as 'unpaid'. Messaing is an alert about account closure
                // 4. Email all account owners

                    //email all account owners a copy of account closure email
                    EmailManager.Send(
                            accountOwnerEmails,
                            Settings.Endpoints.Emails.FromAlerts,
                            Settings.Copy.EmailMessages.InvoicedChargeFailed_AccountDeactivated.FromName,
                            String.Format(Settings.Copy.EmailMessages.InvoicedChargeFailed_AccountDeactivated.Subject),
                            String.Format(Settings.Copy.EmailMessages.InvoicedChargeFailed_AccountDeactivated.Body, account.AccountName),
                            true);
                

                // 5. Email Platform Admins
                EmailManager.Send(
                            Settings.Endpoints.Emails.PlatformEmailAddresses,
                            Settings.Endpoints.Emails.FromPlatform,
                            "ACCOUNT DEACTIVATED",
                            "An account has been deactivated due to an unpaid subscription",
                            "AccountName: <b>" + account.AccountName + "</b><br/><br/>AccountID: <b>" + account.AccountID + "</b><br/>",
                            true);
            }

            response.isSuccess = true;

            return response;
        }

        #region (OPTIONAL) Processing of manual Invoice or Charge Failures

        /*

        public static DataAccessResponseType ProcessFailedStripeInvoicePaymentEvent(string stripeCustomerID, string amount, string failureMessage, string stripeInvoiceId, string stripeEventId)
        {
            var response = new DataAccessResponseType();

            //Get all owners for the account:
            //var accountOwnerEmails = AccountManager.GetAccountOwnerEmails(stripeCustomerID);
            //Get the account
            //var account = AccountManager.GetAccountByStripeCustomerID(stripeCustomerID, false);
            //var account = AccountManager.GetAccount(stripeCustomerID);


            //Log the issue in into the Dunning table for the account and mark the account with billing issues + count up attempts


            response.isSuccess = true;

            return response;
        }

        public static DataAccessResponseType ProcessFailedStripeChargeEvent(string stripeCustomerID, string amount, string failureMessage, string StripeEventId)
        {
            var response = new DataAccessResponseType();

            //Get all owners for the account:
            var accountOwnerEmails = AccountManager.GetAccountOwnerEmails(stripeCustomerID);
            //Get the account
            //var account = AccountManager.GetAccountByStripeCustomerID(stripeCustomerID, false);
            var account = AccountManager.GetAccount(stripeCustomerID);

            //email all account owners
            foreach (string email in accountOwnerEmails)
            {
                //email all account owners a copy of the associated invoice
                EmailManager.Send(
                        accountOwnerEmails,
                        Settings.Endpoints.Emails.FromPayments,
                        Settings.Copy.EmailMessages.ChargeFailed.FromName,
                        String.Format(Settings.Copy.EmailMessages.ChargeFailed.Subject),
                        String.Format(Settings.Copy.EmailMessages.ChargeFailed.Body, account.AccountName, account.AccountNameKey),
                        true);
            }

            //Log the issue in into the Dunning table for the account and mark the account with billing issues + count up attempts




            response.isSuccess = true;

            return response;
        }
        
        */

        #endregion

        #endregion

        #region STRIPE related Operations

        public static DataAccessResponseType ProcessRetryUnpaidInvoicesForStripeCustomer(string customerId)
        {
            var account = AccountManager.GetAccount(customerId, true, AccountManager.AccountIdentificationType.StripeCustomerID);

            return new StripeManager().RetryUnpaidInvoices(customerId, account.AccountName, account.AccountID.ToString());
        }

        #endregion

        #region Bulk Account Operations

        /* NOTIFICATIONS OFF
        public static DataAccessResponseType ProcessSendNotificationToBulkAccounts(string message, string messageType, double expirationDays, bool accountOwnersOnly, string columnName, string columnValue)
        {
            var response = new DataAccessResponseType();
            var userIds = new List<string>();

            try
            {
                if (String.IsNullOrEmpty(columnName) && String.IsNullOrEmpty(columnName))
                {
                    //Get list of all userIds from provisioned accounts
                    userIds = AccountManager.GetUserIDsFromAllProvisionedAccounts(accountOwnersOnly);
                }
                else
                {
                    //Get a subset of userIds from provisioned accounts based on columnName/Value:
                    userIds = AccountManager.GetUserIDsFromProvisionedAccountsByFilter(accountOwnersOnly, columnName, columnValue);
                }


                //Convert messageType to correct Enum:

                // string to enum
                NotificationType _convertedMesageType = (NotificationType)Enum.Parse(typeof(NotificationType), messageType);

                //Convert URL encoded characters back
                message = message.Replace("%2C", ",");

                foreach (string userId in userIds)
                {
                    try
                    {
                        NotificationsManager.SendNotificationToUser(_convertedMesageType, userId, message, expirationDays);
                    }
                    catch(Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to process and send notification to bulk accounts: '" + message + "'",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );
                    }
                }

                response.isSuccess = true;
                return response;
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to process and send notification to bulk accounts: '" + message + "'",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }



        }
        */


        public static DataAccessResponseType ProcessSendEmailToBulkAccounts(string fromEmail, string fromName, string emailSubject, string emailMessage, bool accountOwnersOnly, bool isImportant, string columnName, string columnValue)
        {
            var response = new DataAccessResponseType();
            var emails = new List<string>();


            //Get all email addresses for users in provisioned accounts
            try
            {
                if (String.IsNullOrEmpty(columnName) && String.IsNullOrEmpty(columnName))
                {
                    //Get list of all provisioned account ids
                    emails = AccountManager.GetUserEmailsFromAllProvisionedAccounts(accountOwnersOnly);
                }
                else
                {
                    //Get a subset of user emails based on the filterString:
                    emails = AccountManager.GetUserEmailsFromProvisionedAccountsByFilter(accountOwnersOnly, columnName, columnValue); 
                }

                EmailManager.Send(emails, fromEmail, fromName, emailSubject, emailMessage, true, isImportant);
                
                response.isSuccess = true;
                return response;
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to process and send emails to bulk accounts",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

        }


        #endregion

        #region APPLICATION: DATA INJECTION operations

        public static DataAccessResponseType ProcessSendApplicationDataInjectionImageDocuments(string accountId, int documentInjectionCount)
        {
            var result = new DataAccessResponseType { isSuccess = false };

            //Get the account
            var account = AccountManager.GetAccount(accountId);

            //Create base document
            var imageDocument = new ApplicationImageDocumentModel
            {
                AccountID = accountId,
                DocumentType = "ApplicationImage",
                Title = "Generic Application Image Document",
                Description = "Generic description for batch injected document. Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
            };

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);

            string triggerId = "IncrementApplicationImageCount";
            var requestOptions = new RequestOptions { PostTriggerInclude = new List<string> { triggerId } };

            int documentsInjected = 0;
            //int retryAttempts = 0;
            //int maxAttemptsPerRetry = 0;  //<-- Used to track max retries per attempt

            // Create new stopwatch
            Stopwatch stopwatch = new Stopwatch();

            // Begin timing tasks
            stopwatch.Start();

            //var retryStrategy = new DocumentDbRetryStrategy{ FastFirstRetry = true };

            //DocumentDbRetryStrategy.F

            do
            {
                imageDocument.Id = Guid.NewGuid().ToString();

                var createDocumentResponse = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentAsync(collectionUri.ToString(), imageDocument, requestOptions).Result;
                documentsInjected++; //<--Increment amount of documents that have been injected

                try
                {
                    AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId); //<--Invalidate Account Capacities Cache
                }
                catch { }
            }
            while (documentsInjected != documentInjectionCount);


            if (documentsInjected == documentInjectionCount)
            {
                result.isSuccess = true;
            }

            stopwatch.Stop();
            //Output timing into data injection log
            PlatformLogManager.LogActivity(
                    CategoryType.DataInjection,
                    ActivityType.DataInjection_ImageDocuments,
                    "Batch Injection complete! " + documentsInjected + " of " + documentInjectionCount + " documents injected",
                    "Time: " + stopwatch.ElapsedMilliseconds + " Milliseconds( " + String.Format("{0:0,0.00}", TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalMinutes) + " Minutes)", // | Retries: " + retryAttempts + " | Max Attempts Per Retry: " + maxAttemptsPerRetry,
                    accountId
                );

            return result;
        }

        #endregion

        #region (LEGACY) APPLICATION: DATA INJECTION operations (using manual retry policy)

        /*

        public static DataAccessResponseType ProcessSendApplicationDataInjectionImageDocuments_ManualRetryPolicy(string accountId, int documentInjectionCount)
        {
            var result = new DataAccessResponseType { isSuccess = false };

            //Get the account
            var account = AccountManager.GetAccount(accountId);

            //Create base document
            var imageDocument = new ApplicationImageDocumentModel {
                AccountID = accountId,
                DocumentType = "ApplicationImage",
                Title = "Generic Application Image Document",
                Description = "Generic description for batch injected document. Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
            };

            //Get the DocumentDB Client
            var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            client.OpenAsync();

            //Connect to the collection for the account
            //TODO: Switch to using ID directly when available
            var collection = client.CreateDocumentCollectionQuery(dbSelfLink).Where(c => c.Id == account.DocumentPartition).ToArray().FirstOrDefault();

            string triggerId = "IncrementApplicationImageCount";
            var requestOptions = new RequestOptions { PostTriggerInclude = new List<string> { triggerId } };

            int documentsInjected = 0;
            int retryAttempts = 0;
            int maxAttemptsPerRetry = 0;  //<-- Used to track max retries per attempt

            // Create new stopwatch
            Stopwatch stopwatch = new Stopwatch();

            // Begin timing tasks
            stopwatch.Start();

            //var retryStrategy = new DocumentDbRetryStrategy{ FastFirstRetry = true };
            
            //DocumentDbRetryStrategy.F

            do
            {
                int retryAttemptsThisInjection = 0; //<-- Used to track max retries per attempt

                bool injectionComplete = false;

                imageDocument.Id = Guid.NewGuid().ToString();

                #region DocumentDB Call With Retry Logic

                int maxRetryAttempts = 10;

                while (!injectionComplete && maxRetryAttempts > 0)
                {
                    try
                    {

                        // -----------------------------------------------------------------
                        // ATTEMPT to call DocumentDB Method
                        // -----------------------------------------------------------------

                        var createDocumentResponse = client.CreateDocumentAsync(collection.SelfLink, imageDocument, requestOptions).Result;
                        injectionComplete = true; //<--Mark this injection as complete in order to move on to the next
                        documentsInjected++; //<--Increment amount of documents that have been injected
                        try
                        {
                            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId); //<--Invalidate Account Capacities Cache
                        }catch{ }
                        
                        // -----------------------------------------------------------------
                        // END attempt to call DocumentDB Method
                        // -----------------------------------------------------------------

                        if(maxAttemptsPerRetry < retryAttemptsThisInjection)
                        {
                            maxAttemptsPerRetry = retryAttemptsThisInjection; //<-- Used to track max retries per attempt
                        }

                    }
                    catch (DocumentClientException de)
                    {
                        #region Catch DocumentClientException

                        if (de.StatusCode.HasValue)
                        {
                            var statusCode = (int)de.StatusCode;

                            if (statusCode == 429 || statusCode == 503)
                            {
                                //Sleep for retry amount
                                Thread.Sleep(de.RetryAfter);

                                //Decrement max retry attempts 
                                maxRetryAttempts--;
                                retryAttempts++;
                                retryAttemptsThisInjection++;
                            }
                            else
                            {
                                stopwatch.Stop();

                                //Log exception
                                PlatformLogManager.LogActivity(
                                    CategoryType.DataInjection,
                                    ActivityType.DataInjection_Exception,
                                    "Batch Injection of image documents caused a DocumentClientException after reaching maximum retry attempts of " + maxRetryAttempts + " (" + documentsInjected + "/" + documentInjectionCount + ")",
                                    "Time: " + stopwatch.ElapsedMilliseconds + " Milliseconds( " + TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalMinutes + " Minutes) | Retries: " + retryAttempts + " | ",
                                    accountId,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                    JsonConvert.SerializeObject(de, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                                );
                            }

                        }

                        #endregion
                    }
                    catch (AggregateException ae)
                    {
                        #region Catch AggregateException

                        foreach (Exception ex in ae.InnerExceptions)
                        {
                            if (ex is DocumentClientException)
                            {
                                var documentClientException = ex as DocumentClientException;
                                var statusCode = (int)documentClientException.StatusCode;
                                if (statusCode == 429 || statusCode == 503)
                                {
                                    //Sleep for retry amount
                                    Thread.Sleep(documentClientException.RetryAfter);

                                    //Decrement max retry attempts 
                                    maxRetryAttempts--;
                                    retryAttempts++;
                                    retryAttemptsThisInjection++;
                                }
                                else
                                {
                                    stopwatch.Stop();

                                    //Log exception
                                    PlatformLogManager.LogActivity(
                                        CategoryType.DataInjection,
                                        ActivityType.DataInjection_Exception,
                                        "Batch Injection of image documents  caused an AggregateException after reaching maximum retry attempts of " + maxRetryAttempts + " (" + documentsInjected + "/" + documentInjectionCount + ")",
                                        "Time: " + stopwatch.ElapsedMilliseconds + " Milliseconds( " + TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalMinutes + " Minutes) | Retries: " + retryAttempts + " | ",
                                        accountId,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                        JsonConvert.SerializeObject(ae, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                                    );


                                    throw;
                                }
                            }
                            else
                            {
                                stopwatch.Stop();

                                //Log exception
                                PlatformLogManager.LogActivity(
                                    CategoryType.DataInjection,
                                    ActivityType.DataInjection_Exception,
                                    "Batch Injection of image documents caused an AggregateException after reaching maximum retry attempts of " + maxRetryAttempts + " (" + documentsInjected + "/" + documentInjectionCount + ")",
                                    "Time: " + stopwatch.ElapsedMilliseconds + " Milliseconds( " + TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalMinutes + " Minutes) | Retries: " + retryAttempts + " | ",
                                    accountId,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                                    JsonConvert.SerializeObject(ae, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                                );
                            }
                        }

                        #endregion
                    }
                }
                if (maxRetryAttempts < 0)
                {
                    stopwatch.Stop();

                    PlatformLogManager.LogActivity(
                        CategoryType.DataInjection,
                        ActivityType.DataInjection_Exception,
                        "Batch Injection of image documents failed after reaching maximum retry attempts of " + maxRetryAttempts + " (" + documentsInjected + "/" + documentInjectionCount + ")",
                        "Time: " + stopwatch.ElapsedMilliseconds + " Milliseconds( " + TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalMinutes + " Minutes) | Retries: " + retryAttempts + " | ",
                        accountId
                    );
                }

                #endregion
            }
            while (documentsInjected != documentInjectionCount);


            if (documentsInjected == documentInjectionCount)
            {
                result.isSuccess = true;
            }

            stopwatch.Stop();
            //Output timing into data injection log
            PlatformLogManager.LogActivity(
                    CategoryType.DataInjection,
                    ActivityType.DataInjection_ImageDocuments,
                    "Batch Injection complete! " + documentsInjected + " of " + documentInjectionCount + " documents injected",
                    "Time: " + stopwatch.ElapsedMilliseconds + " Milliseconds( " + String.Format("{0:0,0.00}", TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalMinutes) + " Minutes) | Retries: " + retryAttempts + " | Max Attempts Per Retry: " + maxAttemptsPerRetry,
                    accountId
                );

            return result;
        }

         */

        #endregion

    }
}
