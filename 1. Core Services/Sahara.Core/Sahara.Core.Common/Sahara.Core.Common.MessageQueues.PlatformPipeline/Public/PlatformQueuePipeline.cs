using System;
using Microsoft.WindowsAzure.Storage.Queue;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Common.MessageQueues.PlatformPipeline.PublicTypes;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Common.MessageQueues.PlatformPipeline
{
    public static class PlatformQueuePipeline
    {
        #region Constructor

        private static CloudQueue _queue = Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudQueueClient().GetQueueReference(PipelineSettings.QueueReferenceName);
        
        static PlatformQueuePipeline()
        {

            //Create and set retry policy
            IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 8);
            _queue.ServiceClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

            _queue.CreateIfNotExists();
        }

        #endregion

        #region Send

        /// <summary>
        /// Usage:
        /// PlatformQueuePipeline.SendMessage.MethodName("param1", "param2");
        /// </summary>
        public static class  SendMessage
        {

            #region Provisioning related Messages

            public static void ProvisionAccount(string accountId)
            {
                string message = string.Format("{0},{1}", PlatformMessageTypes.ProvisionAccount, accountId);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);

            }

            #endregion

            #region Communication Related Messages

            public static void SendNotificationToBulkAccounts(string notificationCopy, string notificationType, double expirationDays, bool accountOwnersOnly, string columnName, string columnValue)
            {
                string message = string.Format("{0},{1},{2},{3},{4},{5},{6}", PlatformMessageTypes.SendNotificationToBulkAccounts, notificationCopy, notificationType, expirationDays, accountOwnersOnly.ToString(), columnName, columnValue);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);

            }

            public static void SendEmailToBulkAccounts(string fromEmail, string fromName, string emailSubject, string emailMessage, bool accountOwnersOnly, bool isImportant, string columnName, string columnValue)
            {
                string message = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", PlatformMessageTypes.SendEmailToBulkAccounts, fromEmail, fromName, emailSubject, emailMessage, accountOwnersOnly.ToString(), isImportant.ToString(), columnName, columnValue);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);

            }

            #endregion

            #region STRIPE Related Messages

            /* NOTES:
             * We pass along the eventId so that the worker can retreive and log the full event at the time of processing
             * We cannot pass the entire event object as it is a JSON object with commas and cannot be pasrsed into a comma deliminated message queu!
             */

            public static void StripeChargeSucceeded(string stripeCustomerId, string stripeChargeId, string stripeCardId, string stripeInvoiceId, string stripeEventId)
            {
                string message = string.Format("{0},{1},{2},{3},{4},{5}", PlatformMessageTypes.StripeChargeSucceeded, stripeCustomerId, stripeChargeId, stripeCardId, stripeInvoiceId, stripeEventId);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);

            }

            public static void StripeRecurringChargeFailed(string stripeCustomerID, string stripeChargeId, string amount, string failureMessage, string eventId)
            {
                string message = string.Format("{0},{1},{2},{3},{4},{5}", PlatformMessageTypes.StripeRecurringChargeFailed, stripeCustomerID, stripeChargeId, amount, failureMessage, eventId);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);

            }

            public static void RetryUnpaidInvoices(string stripeCustomerID)
            {
                if(!String.IsNullOrEmpty(stripeCustomerID))
                {
                    string message = string.Format("{0},{1}", PlatformMessageTypes.RetryUnpaidInvoices, stripeCustomerID);

                    CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                    _queue.AddMessage(cloudQueueMessage);
                }
            }

            public static void StripeChargeFailed(string stripeCustomerID, string stripeChargeId, string amount, string failureMessage, string eventId)
            {
                string message = string.Format("{0},{1},{2},{3},{4},{5}", PlatformMessageTypes.StripeChargeFailed, stripeCustomerID, stripeChargeId, amount, failureMessage, eventId);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);

            }

            public static void StripeCustomerDelinquencyChanged(string accountId, bool newDelinquentStatus)
            {
                string message = string.Format("{0},{1},{2}", PlatformMessageTypes.StripeCustomerDelinquencyChanged, accountId, newDelinquentStatus);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);
            }

            public static void StripeSubscriptionStatusChanged(string customerId, string newSubscriptionStatus, string previousSubscriptionStatus)
            {
                string message = string.Format("{0},{1},{2},{3}", PlatformMessageTypes.StripeSubscriptionStatusChanged, customerId, newSubscriptionStatus, previousSubscriptionStatus);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);
            }

            #region (OPTIONAL) Processing of 'manual' Invoice or Charge Failures

            #endregion

            #region Application related messages

            #region Testing

            public static bool SendApplicationDataInjectionImageDocuments(string accountId, int documentCount)
            {
                try
                {
                    string message = string.Format("{0},{1},{2}", PlatformMessageTypes.SendApplicationDataInjectionImageDocuments, accountId, documentCount);

                    CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                    _queue.AddMessage(cloudQueueMessage);

                    return true;
                }
                catch
                {
                    return false;
                }

            }

            #endregion

            #endregion



            #region Scaffold for new message types

            public static void AnotherMethod(string accountId, string anotherProp)
            {
                string message = string.Format("{0},{1},{2}", PlatformMessageTypes.AnotherMessage, accountId, anotherProp);

                CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(message);
                _queue.AddMessage(cloudQueueMessage);
            }

            #endregion
        }

        #endregion


        #endregion

        #region Get

        public static PlatformQueueMessage GetNextQueueMessage(TimeSpan timeSpan)
        {
            var message = _queue.GetMessage(timeSpan);

            if (message != null)
            {
                if (message.DequeueCount > 3)
                {
                    //If this message has had 4 or more failed attempts (DequeueCount) at processing, log the raw message, email system admins, delete from queue & return null


                    //Log Issues For Administrator Analysis:
                    PlatformLogManager.LogActivity(
                            CategoryType.Error,
                            ActivityType.Error_QueuePipeline,
                            "Could not process message after " + message.DequeueCount + " attempts",
                            "Raw Message: {" + message.AsString + "}, " + "Dequeue Count: {" + message.DequeueCount + "}"
                        );


                    //Delete Message From Queue
                    _queue.DeleteMessage(message);

                    //Email administrators about the issue:
                    EmailManager.Send(
                        Settings.Endpoints.Emails.PlatformEmailAddresses,
                        Settings.Endpoints.Emails.FromAlerts,
                        "platform Queue",
                        "Platform Queue Alert",
                        "A platform queue message has been dequeued past the minimum allowed and has been archived.</br></br>Message Contents:</br></br>" + message.AsString,
                        true
                        );

                    //Create a ManualTask for this message
                    PlatformLogManager.LogActivity(
                        CategoryType.ManualTask,
                        ActivityType.ManualTask_MessageQueue,
                        "Message Processing Failed",
                        "Raw Message: {" + message.AsString + "}",
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString()
                    );

                    return null;
                }
            }

            try
            {
                string messageType;
                string[] messageParts;

                if (message != null)
                {
                    messageParts = message.AsString.Split(new char[] { ',' });
                    messageType = messageParts[0];
                }
                else
                {
                    return null;
                }

                switch (messageType)
                {
                    case PlatformMessageTypes.ProvisionAccount:
                        ProvisionAccount_QueueMessage provisionAccount = new ProvisionAccount_QueueMessage();
                        provisionAccount.rawMessage = message;
                        provisionAccount.QueueMessageType = messageType;
                        provisionAccount.AccountID = messageParts[1];
                        return provisionAccount;

                    case PlatformMessageTypes.StripeChargeSucceeded:
                        StripeChargeSucceeded_QueueMessage stripeChargeSucceeded = new StripeChargeSucceeded_QueueMessage();
                        stripeChargeSucceeded.rawMessage = message;
                        stripeChargeSucceeded.QueueMessageType = messageType;
                        stripeChargeSucceeded.StripeCustomerId = messageParts[1];
                        stripeChargeSucceeded.StripeChargeId = messageParts[2];
                        stripeChargeSucceeded.StripeCardId = messageParts[3];
                        stripeChargeSucceeded.StripeInvoiceId = messageParts[4];
                        stripeChargeSucceeded.StripeEventId = messageParts[5];
                        return stripeChargeSucceeded;

                    case PlatformMessageTypes.StripeRecurringChargeFailed:
                        StripeRecurringChargeFailed_QueueMessage stripeRecurringChargeFailed = new StripeRecurringChargeFailed_QueueMessage();
                        stripeRecurringChargeFailed.rawMessage = message;
                        stripeRecurringChargeFailed.QueueMessageType = messageType;
                        stripeRecurringChargeFailed.StripeCustomerId = messageParts[1];
                        stripeRecurringChargeFailed.StripeChargeId = messageParts[2];
                        stripeRecurringChargeFailed.Amount = messageParts[3];
                        stripeRecurringChargeFailed.FailureMessage = messageParts[4];
                        stripeRecurringChargeFailed.StripeEventId = messageParts[5];
                        return stripeRecurringChargeFailed;

                    case PlatformMessageTypes.RetryUnpaidInvoices:
                        RetryUnpaidInvoices_QueueMessage retryUnpaidInvoices = new RetryUnpaidInvoices_QueueMessage();
                        retryUnpaidInvoices.rawMessage = message;
                        retryUnpaidInvoices.QueueMessageType = messageType;
                        retryUnpaidInvoices.StripeCustomerId = messageParts[1];
                        return retryUnpaidInvoices;

                    case PlatformMessageTypes.StripeChargeFailed:
                        StripeChargeFailed_QueueMessage stripeChargeFailed = new StripeChargeFailed_QueueMessage();
                        stripeChargeFailed.rawMessage = message;
                        stripeChargeFailed.QueueMessageType = messageType;
                        stripeChargeFailed.StripeCustomerId = messageParts[1];
                        stripeChargeFailed.StripeChargeId = messageParts[2];
                        stripeChargeFailed.Amount = messageParts[3];
                        stripeChargeFailed.FailureMessage = messageParts[4];
                        stripeChargeFailed.StripeEventId = messageParts[5];
                        return stripeChargeFailed;

                    case PlatformMessageTypes.StripeCustomerDelinquencyChanged:
                        StripeCustomerDelinquencyChanged_QueueMessage stripeCustomerDelinquencyChanged = new StripeCustomerDelinquencyChanged_QueueMessage();
                        stripeCustomerDelinquencyChanged.rawMessage = message;
                        stripeCustomerDelinquencyChanged.QueueMessageType = messageType;
                        stripeCustomerDelinquencyChanged.accountId = messageParts[1];
                        stripeCustomerDelinquencyChanged.newDelinquencyStatus = Convert.ToBoolean(messageParts[2]);
                        return stripeCustomerDelinquencyChanged;

                    case PlatformMessageTypes.StripeSubscriptionStatusChanged:
                        StripeSubscriptionStatusChanged_QueueMessage stripeSubscriptionStatusChanged = new StripeSubscriptionStatusChanged_QueueMessage();
                        stripeSubscriptionStatusChanged.rawMessage = message;
                        stripeSubscriptionStatusChanged.QueueMessageType = messageType;
                        stripeSubscriptionStatusChanged.customerId = messageParts[1];
                        stripeSubscriptionStatusChanged.newSubscriptionStatus = messageParts[2];
                        stripeSubscriptionStatusChanged.previousSubscriptionStatus = messageParts[3];
                        return stripeSubscriptionStatusChanged;

                    #region (OPTIONAL) Processing of manual Invoice or Charge Failures

                    #endregion

                    case PlatformMessageTypes.SendNotificationToBulkAccounts:
                        SendNotificationToBulkAccounts_QueueMessage sendNotificationToBulkAccounts = new SendNotificationToBulkAccounts_QueueMessage();
                        sendNotificationToBulkAccounts.rawMessage = message;
                        sendNotificationToBulkAccounts.QueueMessageType = messageType;
                        sendNotificationToBulkAccounts.NotificationMessage = messageParts[1];
                        sendNotificationToBulkAccounts.NotificationType = messageParts[2];
                        sendNotificationToBulkAccounts.ExpirationMinutes = Convert.ToDouble(messageParts[3]);
                        sendNotificationToBulkAccounts.AccountOwnersOnly = Convert.ToBoolean(messageParts[4]);
                        sendNotificationToBulkAccounts.ColumnName = messageParts[5];
                        sendNotificationToBulkAccounts.ColumnValue = messageParts[6];
                        return sendNotificationToBulkAccounts;


                    case PlatformMessageTypes.SendEmailToBulkAccounts:
                        SendEmailToBulkAccounts_QueueMessage sendEmailToBulkAccounts = new SendEmailToBulkAccounts_QueueMessage();
                        sendEmailToBulkAccounts.rawMessage = message;
                        sendEmailToBulkAccounts.QueueMessageType = messageType;
                        sendEmailToBulkAccounts.FromEmail = messageParts[1];
                        sendEmailToBulkAccounts.FromName = messageParts[2];
                        sendEmailToBulkAccounts.EmailSubject = messageParts[3];
                        sendEmailToBulkAccounts.EmailMessage = messageParts[4];
                        sendEmailToBulkAccounts.AccountOwnersOnly = Convert.ToBoolean(messageParts[5]);
                        sendEmailToBulkAccounts.IsImportant = Convert.ToBoolean(messageParts[6]);
                        sendEmailToBulkAccounts.ColumnName = messageParts[7];
                        sendEmailToBulkAccounts.ColumnValue = messageParts[8];
                        return sendEmailToBulkAccounts;


                    case PlatformMessageTypes.SendApplicationDataInjectionImageDocuments:
                        SendApplicationDataInjectionImageDocuments_QueueMessage sendApplicationDataInjectionImageDocuments = new SendApplicationDataInjectionImageDocuments_QueueMessage();
                        sendApplicationDataInjectionImageDocuments.rawMessage = message;
                        sendApplicationDataInjectionImageDocuments.QueueMessageType = messageType;
                        sendApplicationDataInjectionImageDocuments.AccountID = messageParts[1];
                        sendApplicationDataInjectionImageDocuments.DocumentCount = Convert.ToInt32(messageParts[2]);
                        return sendApplicationDataInjectionImageDocuments;

                    case PlatformMessageTypes.AnotherMessage:
                        return null;

                    default:
                        Invalid_QueueMessage invalid = new Invalid_QueueMessage();
                        return invalid;

                }
            }
            catch(Exception e)
            {

                //Log Issues For Administrator Analysis:
                PlatformLogManager.LogActivity(
                        CategoryType.Error,
                        ActivityType.Error_QueuePipeline_UnreadableMessage,
                        "Could Not Read Message.",
                        "Raw Message: {" + message.AsString + "}, " + "Dequeue Count: {" + message.DequeueCount + "}" + "(Error: " + e.Message + ")"
                    );

                //Delete Message From Queue
                _queue.DeleteMessage(message);


                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to get a platform queue message. Message has been archived. Message Contents: " + message.AsString,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }

        }

        #endregion

        #region Delete

        public static bool DeleteQueueMessage(PlatformQueueMessage queueMessage)
        {
            _queue.DeleteMessage(queueMessage.rawMessage);
            return true;
        }

        #endregion


    }
}
