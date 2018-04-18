using Microsoft.WindowsAzure.Storage.Queue;

namespace Sahara.Core.Common.MessageQueues.PlatformPipeline.PublicTypes
{
    #region Constuctors/Types

    public static class PlatformMessageTypes
    {
        // Platform/Accounts -------------------

        public const string Invalid = "invalid"; //<-- indicates to worker that a message was dequeued, but could not be processed

        public const string ProvisionAccount = "provision-account";

        // Stripe/Billing -------------------

        public const string StripeChargeSucceeded = "stripe-charge-succeeded";

        public const string StripeRecurringChargeFailed = "stripe-recurring-charge-failed";

        public const string StripeChargeFailed = "stripe-charge-failed";

        public const string StripeCustomerDelinquencyChanged = "stripe-customer-delinquency-changed";

        public const string StripeSubscriptionStatusChanged = "stripe-subscription-status-changed";

        public const string RetryUnpaidInvoices = "retry-unpaid-invoices";

        //(OPTIONAL) Processing of 'manual' Invoice or Charge Failures
        //public const string StripeInvoicePaymentFailed = "stripe-invoice-payment-failed";
        //public const string StripeChargeFailed = "stripe-charge-failed";


        // Communications -------------------


        public const string SendNotificationToBulkAccounts = "send-notification-bulk-accounts";
        public const string SendEmailToBulkAccounts = "send-email-bulk-accounts";

        // Application -------------------

        public const string SendApplicationDataInjectionImageDocuments = "application-data-injection-image-documents";


        // Baseline -------------------

        public const string AnotherMessage = "another-message";

    }

    public abstract class PlatformQueueMessage
    {
        public string QueueMessageType;
        public CloudQueueMessage rawMessage;

    }

    #endregion

    public class Invalid_QueueMessage : PlatformQueueMessage
    {
    }


    public class ProvisionAccount_QueueMessage : PlatformQueueMessage
    {

        public string AccountID;
    }

    // ----- STRIPE ----------------------------------------------------------


    public class StripeChargeSucceeded_QueueMessage : PlatformQueueMessage
    {
        public string StripeCustomerId;
        public string StripeChargeId;
        public string StripeCardId;
        public string StripeInvoiceId;
        public string StripeEventId;
    }

    public class StripeInvoicePaymentFailed_QueueMessage : PlatformQueueMessage
    {
        public string StripeCustomerId;
        public string Amount;
        public string FailureMessage;
        public string StripeInvoiceId;
        public string StripeEventId;
    }

    public class StripeRecurringChargeFailed_QueueMessage : PlatformQueueMessage
    {
        public string StripeCustomerId;
        public string StripeChargeId;
        public string Amount;
        public string FailureMessage;
        public string StripeEventId;
    }

    public class StripeChargeFailed_QueueMessage : PlatformQueueMessage
    {
        public string StripeCustomerId;
        public string StripeChargeId;
        public string Amount;
        public string FailureMessage;
        public string StripeEventId;
    }

    public class StripeCustomerDelinquencyChanged_QueueMessage : PlatformQueueMessage
    {
        public string accountId;
        public bool newDelinquencyStatus;
    }

    public class StripeSubscriptionStatusChanged_QueueMessage : PlatformQueueMessage
    {
        public string customerId;
        public string newSubscriptionStatus;
        public string previousSubscriptionStatus;
    }

    // --- Stripe related

    public class RetryUnpaidInvoices_QueueMessage : PlatformQueueMessage
    {
        public string StripeCustomerId;
    }


    // ----- BULK COMMUNICATIONS ----------------------------------------------------------


    public class SendNotificationToBulkAccounts_QueueMessage : PlatformQueueMessage
    {

        public string NotificationMessage;
        public string NotificationType;
        public double ExpirationMinutes;
        public bool AccountOwnersOnly;
        public string ColumnName; //<--Used to get a subset of account records
        public string ColumnValue; //<--Used to get a subset of account records

    }


    public class SendEmailToBulkAccounts_QueueMessage : PlatformQueueMessage
    {
        public string FromName;
        public string FromEmail;
        public string EmailSubject;
        public string EmailMessage;
        public bool AccountOwnersOnly;
        public bool IsImportant;
        public string ColumnName; //<--Used to get a subset of account records
        public string ColumnValue; //<--Used to get a subset of account records

    }

    // ----- APPLICATION ----------------------------------------------------------


    public class SendApplicationDataInjectionImageDocuments_QueueMessage : PlatformQueueMessage
    {
        public string AccountID { get; set; }
        public int DocumentCount { get; set; }
    }

    // ----- SCAFFOLD ----------------------------------------------------------


    public class AnotherMessage_QueueMessage : PlatformQueueMessage
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }
        public string Property3 { get; set; }
    }


}
