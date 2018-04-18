using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Sahara.Core.Platform.Initialization;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Platform.BackgroundTasks;
using Sahara.Core.Common.MessageQueues.PlatformPipeline.PublicTypes;
using Sahara.Core.Common.MessageQueues.PlatformPipeline;
using Sahara.Core.Accounts;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using System.Collections;

namespace Worker.PlatformWorker
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class PlatformWorker : StatelessService
    {
        internal static string workerName = "Platform Worker";
        //internal static int secondsToSleep = 45;


        public PlatformWorker(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            #region Working w/ Service Fabric Enviornment Variables

            //Pull in enviornment variables: -----------------------------------------------------------
            //var hostId = Environment.GetEnvironmentVariable("Fabric_ApplicationHostId");
            //var appHostType = Environment.GetEnvironmentVariable("Fabric_ApplicationHostType");
            //var tyoeEndpoint = Environment.GetEnvironmentVariable("Fabric_Endpoint_[YourServiceName]TypeEndpoint");
            //var nodeName = Environment.GetEnvironmentVariable("Fabric_NodeName");

            //Or print them all out (Fabric Only):
            /*
            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
            {
                if (de.Key.ToString().StartsWith("Fabric"))
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, " Environment variable {0} = {1}", de.Key, de.Value);
                }
            }
            */

            //EVERY SINGLE ONE:
            /*
            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, " Environment variable {0} = {1}", de.Key, de.Value);
                
            }*/

            #endregion


            //Check if platform is initialized
            bool platformInitialized = PlatformInitializationManager.isPlatformInitialized();

            bool queueEmpty = true;
            int emptyProcessCount = 0;

            var nodeName = Environment.GetEnvironmentVariable("Fabric_NodeName");
            ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("{0} waking up on {1}", workerName, nodeName));

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {

                    var platformQueueMessage = PlatformQueuePipeline.GetNextQueueMessage(TimeSpan.FromMinutes(1));

                    if (platformQueueMessage != null)
                    {
                        queueEmpty = false;
                        emptyProcessCount = 0; // <--- revert process count to 0 every time a work order comes in;


                        var processingResult = new DataAccessResponseType();

                        try
                        {
                            #if DEBUG

                            ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Checking platform queue..."));

                            #endif

                            //Process Message
                            switch (platformQueueMessage.QueueMessageType)
                            {

                                case PlatformMessageTypes.Invalid:
                                    {
                                        processingResult.ErrorMessage = "Invalid Message";
                                        // Do nothing, return worker to shorter intervals to check for next message(s), after X number of Dequeus message will be removed
                                        break;
                                    }

                                case PlatformMessageTypes.ProvisionAccount:
                                    {
                                        #region PROCESS ACCOUNT PROVISIONING

                                        //Get account id & name:
                                        string accountID = ((ProvisionAccount_QueueMessage)platformQueueMessage).AccountID;
                                        string accountName = AccountManager.GetAccountName(accountID);

                                        //Log Task:
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Processing,
                                            "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Provisioning of '" + accountName + "' has started",
                                            accountID,
                                            accountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                            "Node: " + nodeName

                                        );

                                        //Process Task =========================================
                                        // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                        processingResult = BackgroundTasksManager.ProcessAccountProvisioning(accountID);

                                        //Log Task Completion: ====================================================
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Completed,
                                            "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Provisioning of '" + accountName + "' completed",
                                            accountID,
                                            accountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                            "Node: " + nodeName
                                        );

                                        #endregion

                                        break;
                                    }

                                case PlatformMessageTypes.StripeChargeSucceeded:
                                    {

                                        #region PROCESS SUCCESSFUL STRIPE CHARGE EVENT

                                        //Get customer id & name:
                                        //string stripeCustomerID = ((StripeChargeSucceeded_QueueMessage)platformQueueMessage).StripeCustomerID;
                                        //string accountName = AccountManager.GetAccountName(accountID);

                                        var account = AccountManager.GetAccount(((StripeChargeSucceeded_QueueMessage)platformQueueMessage).StripeCustomerId);

                                        //Log Task:
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Processing,
                                            "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Preparing invoice for StripeCustomerID: '" + ((StripeChargeSucceeded_QueueMessage)platformQueueMessage).StripeCustomerId + "'",
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                            "Node: " + nodeName
                                        );


                                        //Process Task =========================================
                                        // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                        processingResult = BackgroundTasksManager.ProcessSuccessfulStripeChargeEvent(
                                            ((StripeChargeSucceeded_QueueMessage)platformQueueMessage).StripeCustomerId,
                                            ((StripeChargeSucceeded_QueueMessage)platformQueueMessage).StripeChargeId,
                                            ((StripeChargeSucceeded_QueueMessage)platformQueueMessage).StripeCardId,
                                            ((StripeChargeSucceeded_QueueMessage)platformQueueMessage).StripeInvoiceId,
                                            ((StripeChargeSucceeded_QueueMessage)platformQueueMessage).StripeEventId
                                        );

                                        //Log Task Completion: ======================================================
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Completed,
                                            "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Invoice sent for StripeCustomerID: '" + ((StripeChargeSucceeded_QueueMessage)platformQueueMessage).StripeCustomerId + "'",
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                            "Node: " + nodeName
                                        );


                                        #endregion

                                        break;
                                    }



                                case PlatformMessageTypes.StripeRecurringChargeFailed:
                                    {
                                        #region PROCESS FAILED RECURRING STRIPE CHARGE

                                        //AUtomatic charges are placed on subscriptions when the payment date reoccurs, if a charge fails AND has an invoice associated we send an email to all the account owners and mark the accounts 'Delinquent' state as 'TRUE'
                                        //Only charges WITH Inovoices are part of the Dunning process.

                                        var account = AccountManager.GetAccount(((StripeRecurringChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId);

                                        //Log Task:
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Processing,
                                            "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Preparing billing alerts and dunning management for StripeCustomerID: '" + ((StripeRecurringChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId + "'",
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                            "Node: " + nodeName
                                        );

                                        //Process Task =========================================
                                        // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                        processingResult = BackgroundTasksManager.ProcessFailedStripeRecurringChargeEvent(
                                                ((StripeRecurringChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId,
                                                ((StripeRecurringChargeFailed_QueueMessage)platformQueueMessage).StripeChargeId,
                                                ((StripeRecurringChargeFailed_QueueMessage)platformQueueMessage).Amount,
                                                ((StripeRecurringChargeFailed_QueueMessage)platformQueueMessage).FailureMessage,
                                                ((StripeRecurringChargeFailed_QueueMessage)platformQueueMessage).StripeEventId
                                            );

                                        //Log Task Completion: ======================================================
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Completed,
                                            "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Billing alerts and dunning sent to StripeCustomerID: '" + ((StripeRecurringChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId + "'",
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                            "Node: " + nodeName
                                        );

                                        #endregion

                                        break;
                                    }



                                case PlatformMessageTypes.StripeChargeFailed:
                                    {
                                        #region PROCESS FAILED STRIPE CHARGE

                                        //AUtomatic charges are placed on subscriptions when the payment date reoccurs, if a charge fails AND has an invoice associated we send an email to all the account owners and mark the accounts 'Delinquent' state as 'TRUE'
                                        //Only charges WITH Inovoices are part of the Dunning process.

                                        var account = AccountManager.GetAccount(((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId);

                                        //Log Task:
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Processing,
                                            "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Preparing billing alerts forStripeCustomerID: '" + ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId + "'",
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                            "Node: " + nodeName

                                        );


                                        //Process Task =========================================
                                        // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                        processingResult = BackgroundTasksManager.ProcessFailedStripeChargeEvent(
                                                ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId,
                                                ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeChargeId,
                                                ((StripeChargeFailed_QueueMessage)platformQueueMessage).Amount,
                                                ((StripeChargeFailed_QueueMessage)platformQueueMessage).FailureMessage,
                                                ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeEventId
                                            );

                                        //Log Task Completion: ======================================================
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Completed,
                                            "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Billing alerts sent to StripeCustomerID: '" + ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId + "'",
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                            "Node: " + nodeName
                                        );

                                        #endregion

                                        break;
                                    }



                                case PlatformMessageTypes.StripeCustomerDelinquencyChanged:
                                    {
                                        #region PROCESS CUSTOMER DELINQUENCY CHANGED STRIPE EVENT

                                        var account = AccountManager.GetAccount(((StripeCustomerDelinquencyChanged_QueueMessage)platformQueueMessage).accountId);

                                        //Update account 'Delinquent' state

                                        //Log Task:
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Processing,
                                            "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Delinquency status changing to '" + ((StripeCustomerDelinquencyChanged_QueueMessage)platformQueueMessage).newDelinquencyStatus + "'",
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                            "Node: " + nodeName
                                        );



                                        //Process Task =========================================
                                        // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                        processingResult = BackgroundTasksManager.ProcessStripeCustomerDelinquencyChangedEvent(
                                                ((StripeCustomerDelinquencyChanged_QueueMessage)platformQueueMessage).accountId,
                                                account.StoragePartition,
                                                ((StripeCustomerDelinquencyChanged_QueueMessage)platformQueueMessage).newDelinquencyStatus
                                            );

                                        //Log Task Completion: ======================================================
                                        PlatformLogManager.LogActivity(
                                           CategoryType.Worker,
                                           ActivityType.Worker_Task_Completed,
                                           "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                           "Delinquency status changed to '" + ((StripeCustomerDelinquencyChanged_QueueMessage)platformQueueMessage).newDelinquencyStatus + "'",
                                           account.AccountID.ToString(),
                                           account.AccountName,
                                           null,
                                           null,
                                           null,
                                           null,
                                           "Node: " + nodeName
                                       );

                                        #endregion

                                        break;
                                    }



                                case PlatformMessageTypes.StripeSubscriptionStatusChanged:
                                    {
                                        #region PROCESS SUBSCRIPTION STATUS CHANGED STRIPE EVENT

                                        //Update account 'Delinquent' state

                                        var account = AccountManager.GetAccount(((StripeSubscriptionStatusChanged_QueueMessage)platformQueueMessage).customerId);

                                        //Log Task:
                                        PlatformLogManager.LogActivity(
                                           CategoryType.Worker,
                                           ActivityType.Worker_Task_Processing,
                                           "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                           "Subscription status changing to " + ((StripeSubscriptionStatusChanged_QueueMessage)platformQueueMessage).newSubscriptionStatus,
                                           account.AccountID.ToString(),
                                           account.AccountName,
                                           null,
                                           null,
                                           null,
                                           null,
                                           "Node: " + nodeName
                                       );


                                        //Process Task =========================================
                                        // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                        processingResult = BackgroundTasksManager.ProcessStripeSubscriptionStatusChangedEvent(
                                                account,
                                                ((StripeSubscriptionStatusChanged_QueueMessage)platformQueueMessage).newSubscriptionStatus,
                                                ((StripeSubscriptionStatusChanged_QueueMessage)platformQueueMessage).previousSubscriptionStatus
                                            );

                                        //Log Task Completion: ======================================================
                                        PlatformLogManager.LogActivity(
                                           CategoryType.Worker,
                                           ActivityType.Worker_Task_Completed,
                                           "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Subscription status changed to " + ((StripeSubscriptionStatusChanged_QueueMessage)platformQueueMessage).newSubscriptionStatus,
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                           "Node: " + nodeName
                                       );

                                        #endregion

                                        break;
                                    }




                                case PlatformMessageTypes.RetryUnpaidInvoices:
                                    {
                                        #region PROCESS RETRY UNPAID STRIPE INVOICES FOR CUSTOMER (Used after a new card has been updated to a good card on a delinquwnt account)

                                        //Get account id & name:
                                        string customerId = ((RetryUnpaidInvoices_QueueMessage)platformQueueMessage).StripeCustomerId;
                                        var account = AccountManager.GetAccount(customerId);

                                        //Log Task:
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Processing,
                                            "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Retrying unpaid invoices after credit card update",
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                           "Node: " + nodeName
                                        );

                                        //Process Task =========================================
                                        // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                        processingResult = BackgroundTasksManager.ProcessRetryUnpaidInvoicesForStripeCustomer(customerId);

                                        //Log Task Completion: ====================================================
                                        PlatformLogManager.LogActivity(
                                            CategoryType.Worker,
                                            ActivityType.Worker_Task_Completed,
                                            "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                            "Unpaid invoices retried",
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                           "Node: " + nodeName
                                        );

                                        #endregion

                                        break;
                                    }



                                #region (OPTIONAL) Processing Failed InvoicePayments and Charges

                                /*
                            case PlatformMessageTypes.StripeInvoicePaymentFailed:

                                #region PROCESS FAILED STRIPE INVOICE PAYMENT EVENT

                                //Log Task:
                                PlatformLogManager.LogActivity(WorkerLogActivity.ProcessingTask,
                                    "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                    "StripeCustomerID: '" + ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId + "'");


                                //Process Task =========================================
                                // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                processingResult = BackgroundTasksManager.ProcessFailedStripeInvoicePaymentEvent(
                                        ((StripeInvoicePaymentFailed_QueueMessage)platformQueueMessage).StripeCustomerId,
                                        ((StripeInvoicePaymentFailed_QueueMessage)platformQueueMessage).Amount,
                                        ((StripeInvoicePaymentFailed_QueueMessage)platformQueueMessage).FailureMessage,
                                        ((StripeInvoicePaymentFailed_QueueMessage)platformQueueMessage).StripeInvoiceId,
                                        ((StripeInvoicePaymentFailed_QueueMessage)platformQueueMessage).StripeEventId
                                    );

                                //Log Task Completion: ======================================================
                                PlatformLogManager.LogActivity(WorkerLogActivity.TaskCompleted,
                                    "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                    "StripeCustomerID: '" + ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId + "'");

                                #endregion

                                break;

                            case PlatformMessageTypes.StripeChargeFailed:

                                #region PROCESS FAILED STRIPE CHARGE EVENT

                                //Log Task:
                                PlatformLogManager.LogActivity(WorkerLogActivity.ProcessingTask,
                                    "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                    "StripeCustomerID: '" + ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId + "'");


                                //Process Task =========================================
                                // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                processingResult = BackgroundTasksManager.ProcessFailedStripeChargeEvent(
                                        ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId,
                                        ((StripeChargeFailed_QueueMessage)platformQueueMessage).Amount,
                                        ((StripeChargeFailed_QueueMessage)platformQueueMessage).FailureMessage,
                                        ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeEventId
                                    );

                                //Log Task Completion: ======================================================
                                PlatformLogManager.LogActivity(WorkerLogActivity.TaskCompleted,
                                    "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                    "StripeCustomerID: '" + ((StripeChargeFailed_QueueMessage)platformQueueMessage).StripeCustomerId + "'");

                                #endregion

                                break;

                                 */

                                #endregion

                                case PlatformMessageTypes.SendNotificationToBulkAccounts:

                                    #region PROCESS SEND NOTIFICATION TO BULK ACCOUNTS (NOTIFICATIONS OFF)
                                    /*
                                    //Log Task:
                                    PlatformLogManager.LogActivity(
                                       CategoryType.Worker,
                                       ActivityType.Worker_Task_Processing,
                                       "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                        "Message: '[" +
                                        ((SendNotificationToBulkAccounts_QueueMessage)platformQueueMessage).NotificationType + "] " +
                                        ((SendNotificationToBulkAccounts_QueueMessage)platformQueueMessage).NotificationMessage + "'",
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "Node: " + nodeName
                                   );


                                    //Process Task =========================================
                                    // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                    processingResult = BackgroundTasksManager.ProcessSendNotificationToBulkAccounts(
                                            ((SendNotificationToBulkAccounts_QueueMessage)platformQueueMessage).NotificationMessage,
                                            ((SendNotificationToBulkAccounts_QueueMessage)platformQueueMessage).NotificationType,
                                            ((SendNotificationToBulkAccounts_QueueMessage)platformQueueMessage).ExpirationMinutes,
                                            ((SendNotificationToBulkAccounts_QueueMessage)platformQueueMessage).AccountOwnersOnly,
                                            ((SendNotificationToBulkAccounts_QueueMessage)platformQueueMessage).ColumnName,
                                            ((SendNotificationToBulkAccounts_QueueMessage)platformQueueMessage).ColumnValue
                                        );

                                    //Log Task Completion: ======================================================
                                    PlatformLogManager.LogActivity(
                                       CategoryType.Worker,
                                       ActivityType.Worker_Task_Completed,
                                       "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                        "Message: '[" +
                                        ((SendNotificationToBulkAccounts_QueueMessage)platformQueueMessage).NotificationType + "] " +
                                        ((SendNotificationToBulkAccounts_QueueMessage)platformQueueMessage).NotificationMessage + "'",
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "Node: " + nodeName
                                   );
                                    */
                                    #endregion

                                    break;



                                case PlatformMessageTypes.SendEmailToBulkAccounts:

                                    #region PROCESS SEND EMAIL TO BULK ACCOUNTS

                                    //Log Task:
                                    PlatformLogManager.LogActivity(
                                       CategoryType.Worker,
                                       ActivityType.Worker_Task_Processing,
                                       "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                        "Message: '[" +
                                        ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).EmailSubject + "] " +
                                        ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).EmailMessage + "'",
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "Node: " + nodeName
                                   );


                                    //Process Task =========================================
                                    // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                    processingResult = BackgroundTasksManager.ProcessSendEmailToBulkAccounts(
                                            ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).FromEmail,
                                            ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).FromName,
                                            ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).EmailSubject,
                                            ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).EmailMessage,
                                            ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).AccountOwnersOnly,
                                            ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).IsImportant,
                                            ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).ColumnName,
                                            ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).ColumnValue

                                        );

                                    //Log Task Completion: ======================================================
                                    PlatformLogManager.LogActivity(
                                       CategoryType.Worker,
                                       ActivityType.Worker_Task_Completed,
                                       "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                        "Message: '[" +
                                        ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).EmailSubject + "] " +
                                        ((SendEmailToBulkAccounts_QueueMessage)platformQueueMessage).EmailMessage + "'",
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "Node: " + nodeName
                                   );

                                    #endregion

                                    break;

                                case PlatformMessageTypes.SendApplicationDataInjectionImageDocuments:

                                    #region PROCESS SEND APPLICATION TESTING BULK DOCUMENT INJECTION 

                                    //Log Task:
                                    PlatformLogManager.LogActivity(
                                       CategoryType.Worker,
                                       ActivityType.Worker_Task_Processing,
                                       "Processing Task: '" + platformQueueMessage.QueueMessageType + "'",
                                        "Message: '" +
                                        ((SendApplicationDataInjectionImageDocuments_QueueMessage)platformQueueMessage).AccountID + ", " +
                                        ((SendApplicationDataInjectionImageDocuments_QueueMessage)platformQueueMessage).DocumentCount + "'",
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "Node: " + nodeName
                                   );


                                    //Process Task =========================================
                                    // !! REMEMBER TO: Transform message inside of" PlatformQueuePipeline.GetNextQueueMessage !!
                                    processingResult = BackgroundTasksManager.ProcessSendApplicationDataInjectionImageDocuments(
                                            ((SendApplicationDataInjectionImageDocuments_QueueMessage)platformQueueMessage).AccountID,
                                            ((SendApplicationDataInjectionImageDocuments_QueueMessage)platformQueueMessage).DocumentCount
                                        );

                                    //Log Task Completion: ======================================================
                                    PlatformLogManager.LogActivity(
                                       CategoryType.Worker,
                                       ActivityType.Worker_Task_Completed,
                                       "Completed Task: '" + platformQueueMessage.QueueMessageType + "'",
                                        "Message: '[" +
                                        ((SendApplicationDataInjectionImageDocuments_QueueMessage)platformQueueMessage).AccountID + "] " +
                                        ((SendApplicationDataInjectionImageDocuments_QueueMessage)platformQueueMessage).DocumentCount + "'",
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "Node: " + nodeName
                                   );

                                    #endregion

                                    break;

                                #region NOTES FOR ADDING NEW MESSAGE TYPES

                                // You must also transform messages inside the "PlatformQueuePipeline" Class Switch Statement

                                #endregion

                                default:

                                    break;
                            }

                            #if DEBUG

                            ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Message processing complete..."));

                            #endif

                            //Delete Message from Queue After Verifying Successful Processing -------------------
                            if (processingResult.isSuccess)
                            {
                                PlatformQueuePipeline.DeleteQueueMessage(platformQueueMessage);
                            }
                            else
                            {
                                //Log in Master Error log and in Worker Log:

                                PlatformLogManager.LogActivity(
                                    CategoryType.Worker,
                                    ActivityType.Worker_Error,
                                        "An error occurred while processing a task inside Platform Worker. Processing will be retried shortly. (node: " + nodeName + ")",
                                        processingResult.ErrorMessage,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "Node: " + nodeName
                                        );


                                PlatformLogManager.LogActivity(
                                    CategoryType.Error,
                                    ActivityType.Error_Worker,
                                        "An error occurred while processing a task inside Platform Worker. Processing will be retried shortly. (node: # " + nodeName + ")",
                                        processingResult.ErrorMessage,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "Node: " + nodeName
                                    );

                            }

                            #if DEBUG
                            ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Message deleted from queue..."));
                            #endif

                        }
                        catch (Exception e)
                        {
                            try
                            {
                                //Log in Master Error log and in Worker Log:
                                PlatformLogManager.LogActivity(
                                    CategoryType.Worker,
                                    ActivityType.Worker_Error,
                                    e.Message + " (node:" + nodeName + ")",
                                    "Message: " + platformQueueMessage.rawMessage.ToString(),
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    "Node: " + nodeName);


                                PlatformLogManager.LogActivity(
                                    CategoryType.Error,
                                    ActivityType.Error_Worker,
                                    e.Message + " (node:" + nodeName + ")",
                                    "Message: " + platformQueueMessage.rawMessage.ToString(),
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    null,
                                    "Node: " + nodeName);

                            }
                            catch
                            {
                                //Log in Master Error log and in Worker Log:
                                PlatformLogManager.LogActivity(
                                        CategoryType.Worker,
                                        ActivityType.Worker_Error,
                                        e.Message,
                                        "node:" + nodeName,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "Node: " + nodeName
                                    );


                                PlatformLogManager.LogActivity(
                                        CategoryType.Error,
                                        ActivityType.Error_Worker,
                                        e.Message,
                                        "node:" + nodeName,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "Node: " + nodeName
                                );

                            }
                        }

                    }
                    else
                    {
                        queueEmpty = true;

                        #if DEBUG

                        ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Platform queue empty"));
                        ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Ending Process"));

                        #endif

                    }
                }
                catch (Exception e)
                {
                    //Log in Master Error log and in Worker Log:
                    PlatformLogManager.LogActivity(
                        CategoryType.Worker,
                        ActivityType.Worker_Error,
                         e.Message,
                         "Error occurred while attempting to process next message in queue. (node:" + nodeName + ")",
                         null,
                         null,
                         null,
                         null,
                         null,
                         null,
                         "Node: " + nodeName
                    );

                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to process next message in queue. (node:" + nodeName + ")",
                        System.Reflection.MethodBase.GetCurrentMethod()
                    );


                }

                if (queueEmpty == true)
                {
                    #region Easeback processing while queue is empty

                    if (emptyProcessCount <= Sahara.Core.Settings.Platform.Worker.EasebackRound1.emptyCacheCountMax)
                    {

                        if (emptyProcessCount == 0)
                        {
                            PlatformLogManager.LogActivity(
                                CategoryType.Worker,
                                ActivityType.Worker_Queue_Empty,
                                "Queue empty (node:" + nodeName + ")",
                                "Queue empty",
                                null,
                                null,
                                null,
                                null,
                                null,
                                null,
                                "Node: " + nodeName
                            );

                            //Sleep for 4 seconds to put next_run log activity at the top:
                            Thread.Sleep(4000);

                            PlatformLogManager.LogActivity(
                                CategoryType.Worker,
                                ActivityType.Worker_Easeback_Initiated,
                                "Starting ease back sequence (node:" + nodeName + ")",
                                "Cycles of " + Sahara.Core.Settings.Platform.Worker.EasebackRound1.duration + " to " + Sahara.Core.Settings.Platform.Worker.EasebackRound2.duration,
                                null,
                                null,
                                null,
                                null,
                                null,
                                null,
                                "Node: " + nodeName
                            );

                        }

                        //tick emptyProcessCount up by 1. 
                        emptyProcessCount++;

                        #if DEBUG
                        ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Empty Process Count: {0}", emptyProcessCount));
                        ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Sleeping For: {0}", Sahara.Core.Settings.Platform.Worker.EasebackRound1.duration));
                        #endif

                        Thread.Sleep(Sahara.Core.Settings.Platform.Worker.EasebackRound1.milliseconds);

                    }
                    else if (emptyProcessCount <= Sahara.Core.Settings.Platform.Worker.EasebackRound2.emptyCacheCountMax)
                    {
                        //tick emptyProcessCount up by 1. 
                        emptyProcessCount++;

                        #if DEBUG
                        ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Empty Process Count: {0}", emptyProcessCount));
                        ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Sleeping For: {0}", Sahara.Core.Settings.Platform.Worker.EasebackRound2.duration));
                        #endif

                        Thread.Sleep(Sahara.Core.Settings.Platform.Worker.EasebackRound2.milliseconds);
                    }
                    else if (emptyProcessCount >= (Sahara.Core.Settings.Platform.Worker.EasebackRound2.emptyCacheCountMax + 1))
                    {
                        if (emptyProcessCount == Sahara.Core.Settings.Platform.Worker.EasebackRound2.emptyCacheCountMax + 1)
                        {
                            PlatformLogManager.LogActivity(
                                CategoryType.Worker,
                                ActivityType.Worker_Sleeping,
                                "Worker (node:" + nodeName + ") sleeping for cycles of " + Sahara.Core.Settings.Platform.Worker.EasebackRound3.duration + " until a message is picked up in the queue.",
                                "Worker sleeping",
                                null,
                                null,
                                null,
                                null,
                                null,
                                null,
                                "Node: " + nodeName
                            );


                        }
                        else
                        {
                        }

                        //tick emptyProcessCount up by 1. 
                        emptyProcessCount++;

                        #if DEBUG
                        ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Empty Process Count: {0}", emptyProcessCount));
                        ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Sleeping For: {0}", Sahara.Core.Settings.Platform.Worker.EasebackRound3.duration));
                        #endif

                        Thread.Sleep(Sahara.Core.Settings.Platform.Worker.EasebackRound3.milliseconds);
                    }

                    #endregion
                }
                else
                {
                    #if DEBUG
                    ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("Sleeping For: {0}", Sahara.Core.Settings.Platform.Worker.TaskBreak.duration));
                    #endif

                    // A message existed and was procssed, sleep for beat before cecking the queue again
                    Thread.Sleep(Sahara.Core.Settings.Platform.Worker.TaskBreak.milliseconds);
                }
            }
        }
    }
}
