using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Logging.PlatformLogs.Types
{
    [DataContract]
    public enum ActivityType
    {
        #region Error

        [EnumMember]
        Error_Exception,

        [EnumMember]
        Error_Other,

        [EnumMember]
        Error_StripeException,

        [EnumMember]
        Error_SendGrid,

        [EnumMember]
        Error_Worker,

        [EnumMember]
        Error_Custodian,

        [EnumMember]
        Error_WCFHost,

        [EnumMember]
        Error_QueuePipeline,

        [EnumMember]
        Error_QueuePipeline_UnreadableMessage,

        [EnumMember]
        Error_Imaging,

        [EnumMember]
        Error_PlanMigration,

        #endregion

        #region Platform

        [EnumMember]
        Platform_Initialized,

        #endregion

        #region Account

        [EnumMember]
        Account_Registered,

        [EnumMember]
        Account_Subscribed,

        [EnumMember]
        Account_Upgraded,

        [EnumMember]
        Account_Downgraded,

        [EnumMember]
        Account_LimitationReached,

        [EnumMember]
        Account_Provisioning_Requested,

        [EnumMember]
        Account_Provisioned,

        [EnumMember]
        Account_ClosureRequested,

        [EnumMember]
        Account_UnprovisionedClosure,

        [EnumMember]
        Account_Closed,

        [EnumMember]
        Account_Closure_Approved,

        [EnumMember]
        Account_Closure_Accelerated,

        [EnumMember]
        Account_Closure_Unapproved,

        [EnumMember]
        Account_Trial_Ended,

        [EnumMember]
        Account_Purge_Started,

        [EnumMember]
        Account_Purged,

        [EnumMember]
        Account_Deprovisioning_Started,

        [EnumMember]
        Account_Deprovisioning_BatchDocumentDeletion,

        [EnumMember]
        Account_Deprovisioned,

        [EnumMember]
        Account_Subscription_Reactivated,

        #endregion

        #region Billing

        [EnumMember]
        Billing_Issue,

        [EnumMember]
        Billing_OverdueInvoice_Paid,

        #endregion

        #region Imaging

        [EnumMember]
        Imaging_Error,

        #endregion

        #region StripeEvent

        //[EnumMember]
        //StripeEvent_IdempotentLogPurged, //<-- Idempotent Log Purged

        //------------------

        [EnumMember]
        StripeEvent_Other, //<-- Fallback for generic event types

        //-------------------
        /*
        [EnumMember]
        StripeEvent_Balance_Available,

        [EnumMember]
        StripeEvent_Transfer_Created,

        [EnumMember]
        StripeEvent_Transfer_Updated,

        [EnumMember]
        StripeEvent_Transfer_Paid,

        [EnumMember]
        StripeEvent_Transfer_Canceled,

        [EnumMember]
        StripeEvent_Transfer_Failed,
        */
        //-------------------

        [EnumMember]
        StripeEvent_InvoicePayment_Succeeded,

        [EnumMember]
        StripeEvent_InvoicePayment_Failed,

        //-------------------

        [EnumMember]
        StripeEvent_RecurringCharge_Succeeded,

        [EnumMember]
        StripeEvent_RecurringCharge_Failed,

        //-------------------

        [EnumMember]
        StripeEvent_Charge_Succeeded,

        [EnumMember]
        StripeEvent_Charge_Failed,

        //-------------------

        [EnumMember]
        StripeEvent_Charge_Refunded,

        //-------------------

        [EnumMember]
        StripeEvent_CreditCard_Updated,

        //-------------------

        [EnumMember]
        StripeEvent_Subscription_Created,

        [EnumMember]
        StripeEvent_Subscription_Updated,

        [EnumMember]
        StripeEvent_Subscription_StatusChanged,

        //-------------------

        [EnumMember]
        StripeEvent_Customer_Created,

        [EnumMember]
        StripeEvent_Customer_Updated,

        [EnumMember]
        StripeEvent_Customer_DelinquencyChanged,

        #endregion

        #region Authentication

        [EnumMember]
        Authentication_Passed,

        [EnumMember]
        Authentication_Failed,

        #endregion

        #region PlatformUser

        [EnumMember]
        PlatformUser_Invited,

        [EnumMember]
        PlatformUser_Created,

        [EnumMember]
        PlatformUser_Deleted,

        [EnumMember]
        PlatformUser_Edited,

        [EnumMember]
        PlatformUser_Role_Updated,

        #endregion

        #region Worker

        //[EnumMember]
        //Worker_Queue_Checking,

        [EnumMember]
        Worker_Queue_Empty,

        [EnumMember]
        Worker_Easeback_Initiated,

        [EnumMember]
        Worker_Status_Update,

        [EnumMember]
        Worker_Task_Processing,

        [EnumMember]
        Worker_Task_Completed,

        //[EnumMember]
        //Worker_Instance_Count,

        //[EnumMember]
        //Worker_Instance_Status, 

        [EnumMember]
        Worker_Sleeping,

        [EnumMember]
        Worker_Error,

        //[EnumMember]
        //Worker_NextRun,

        #endregion

        #region Custodian

        [EnumMember]
        Custodian_Status_Update,

        [EnumMember]
        Custodian_Scheduled_Task,

        [EnumMember]
        Custodian_KeepAlive_Status,


        [EnumMember]
        Custodian_RefreshCache_Status,

        [EnumMember]
        Custodian_RefreshAccountSite_Status,

        //[EnumMember]
        //Custodian_Scheduled_Tasks_Started,

        [EnumMember]
        Custodian_Scheduled_Tasks_Complete,

        [EnumMember]
        Custodian_TrialReminder_EmailSent,

        [EnumMember]
        Custodian_CardExpirationReminder_EmailSent,

        [EnumMember]
        Custodian_Sleeping,

        //[EnumMember]
        //Custodian_NextRun,

        #endregion

        #region Registration



        [EnumMember]
        Registration_Failed,

        [EnumMember]
        Registration_Error,




        #endregion

        #region WCFHost

        [EnumMember]
        WCFHost_Status,

        [EnumMember]
        WCFHost_Endpoints_Created,

        [EnumMember]
        WCFHost_Endpoint_Hosted,

        [EnumMember]
        WCFHost_Error,


        #endregion

        #region GarbageCollection

        [EnumMember]
        GarbageCollection_StripeEventLog,

        [EnumMember]
        GarbageCollection_CreditCardExpirationRemindersLog,

        [EnumMember]
        GarbageCollection_TrialExpirationRemindersLog,

        [EnumMember]
        GarbageCollection_IntermediaryStorage,

        [EnumMember]
        GarbageCollection_ClosedAccounts,

        [EnumMember]
        GarbageCollection_AccountActivitiesLog,

        [EnumMember]
        GarbageCollection_PlatformActivitiesLog,

        #endregion

        #region Manual

        //Informs admins that a manual task must be run due to platform errors

        [EnumMember]
        ManualTask_Stripe,

        [EnumMember]
        ManualTask_SQL,

        [EnumMember]
        ManualTask_DocumentDB,

        [EnumMember]
        ManualTask_Search,

        [EnumMember]
        ManualTask_TableStorage,

        [EnumMember]
        ManualTask_BlobStorage,

        [EnumMember]
        ManualTask_Other,

        [EnumMember]
        ManualTask_MessageQueue,

        #endregion

        #region DataInjection

        [EnumMember]
        DataInjection_ImageDocuments,

        [EnumMember]
        DataInjection_Exception,

        #endregion

        #region Trace

        [EnumMember]
        Trace_Statement,


        #endregion
    }

}
