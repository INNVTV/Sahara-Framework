using API.WebhooksApi.Internal;
using Sahara.Core.Accounts;
using Sahara.Core.Common.MessageQueues.PlatformPipeline;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Logging.PlatformLogs.Types;
using Stripe;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API.WebhooksApi.Controllers
{
    [ServiceRequestActionFilter]
    public class StripeController : ApiController
    {
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("stripe")]
        [HttpPost]
        public HttpResponseMessage Post(StripeEvent stripeEvent)
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            string stripeEvent_ID = stripeEvent.Id;

            if (stripeEvent_ID == "evt_00000000000000")
            {
                //Event is a test - return 200
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            #region Ensure idempotency
           
            bool isLogged = StripeWebhookEventsLogManager.HasEventBeenLogged(stripeEvent_ID);

            if (isLogged)
            {
                //Event is logged -  return 200
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            #endregion

            string StripeEvent_Type = stripeEvent.Type;

            switch (StripeEvent_Type)
            {
                case "invoice.payment_succeeded":

                    #region Log Invoice Payment Event

                    try
                    {
                        //Unpack stripe event object
                        string stripeCustomerID = stripeEvent.Data.Object.customer;
                        string StripeEvent_Id = stripeEvent.Id;

                        //Get the Event requestId, or RequestType for the event.
                        string request = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            request = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }

                        //Get the account
                        var account = AccountManager.GetAccount(stripeCustomerID, true, AccountManager.AccountIdentificationType.StripeCustomerID);


                        var stripePlanId = "Customer is not subscribed to a plan";
                        if (account.StripePlanID != null)
                        {
                            stripePlanId = account.StripePlanID;
                        }

                        //Log Activity:
                        PlatformLogManager.LogActivity(
                            CategoryType.StripeEvent,
                            ActivityType.StripeEvent_InvoicePayment_Succeeded,
                            stripePlanId,
                            request + " | " + stripeEvent.Id,
                            account.AccountID.ToString(),
                            account.AccountName,
                            null,
                            null,
                            null,
                            null,
                            "webhook"
                        );

                        //Return 200 response
                        httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later        
                    }
                    catch (Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to capture the 'invoice.payment_succeeded' stripe webhook event",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );

                        //Return 200 response (Since the activity has been logged as well as the exception we return a 200 response so that Stripe does not attempt to call the webhook again )
                        httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later        

                        //httpResponse = Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); //<-- Stripe will attempt to call the webhook later
                    }

                    #endregion

                    break;

                case "invoice.payment_failed":

                    #region Log Payment Failed Event

                    try
                    {
                        //Unpack stripe event object
                        string customerID = stripeEvent.Data.Object.customer;
                        string amount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars((string)stripeEvent.Data.Object.amount_due);
                        string failureMessage = stripeEvent.Data.Object.failure_message;
                        //string eventObject = stripeEvent.Data.Object.ToString();
                        string StripeEvent_Id = stripeEvent.Id;
                        string invoiceId = stripeEvent.Data.Object.Id;

                        //Get the Event requestId, or RequestType for the event.
                        string request = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            request = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }

                        //(OPTIONAL)Send message queue for PlatformWorker to process
                        //PlatformQueuePipeline.SendMessage.StripeInvoicePaymentFailed(customerID, amount, failureMessage, invoiceId, StripeEvent_Id);

                        //Get the account
                        var account = AccountManager.GetAccount(customerID, true, AccountManager.AccountIdentificationType.StripeCustomerID);


                        var stripePlanId = "Customer is not subscribed to a plan";
                        if (account.StripePlanID != null)
                        {
                            stripePlanId = account.StripePlanID;
                        }

                        //Log Activity:
                        PlatformLogManager.LogActivity(
                            CategoryType.StripeEvent,
                            ActivityType.StripeEvent_InvoicePayment_Failed,
                            stripePlanId,
                            request + " | " + stripeEvent.Id,
                                account.AccountID.ToString(),
                                account.AccountName,
                                null,
                                null,
                                null,
                                null,
                                "webhook"
                        );

                        PlatformLogManager.LogActivity(
                                CategoryType.Billing,
                                ActivityType.Billing_Issue,
                                "Invoice payment failed for " + account.AccountName,
                                request + " | " + stripeEvent.Id,
                                account.AccountID.ToString(),
                                account.AccountName,
                                null,
                                null,
                                null,
                                null,
                                "webhook"
                            );

                        //Return 200 response
                        httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later        
                    }
                    catch (Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to capture  the 'invoice.payment_failed' stripe webhook event",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );



                        httpResponse = Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); //<-- Stripe will attempt to call the webhook later
                    }

                    #endregion

                    break;

                case "charge.succeeded":

                    #region Send Charge Succeeded Message Queue


                    try
                    {
                        //Unpack stripe event object
                        string stripeCustomerID = stripeEvent.Data.Object.customer;
                        string stripeChargeId = stripeEvent.Data.Object.id;
                        string stripeCardId = stripeEvent.Data.Object.source.id; //<-- check if source is a card when we upgrade to using Bitcoins
                        string amount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars((string)stripeEvent.Data.Object.amount);
                        string stripeInvoiceId = stripeEvent.Data.Object.invoice;
                        //string eventObject = stripeEvent.Data.Object.ToString();
                        string StripeEvent_Id = stripeEvent.Id;

                        //Get the Event requestId, or RequestType for the event.
                        string request = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            request = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }


                        //Send message queue for PlatformWorker to process
                        PlatformQueuePipeline.SendMessage.StripeChargeSucceeded(stripeCustomerID, stripeChargeId, stripeCardId, stripeInvoiceId, StripeEvent_Id);


                        try
                        {
                            #region Attempt to log the stripe event and return a HttpStatusCode.OK/200 code to Stripe

                            //Get the account
                            var account = AccountManager.GetAccount(stripeCustomerID);

                            //Log the webhook activity:
                            if (!String.IsNullOrEmpty(stripeInvoiceId))
                            {
                                var stripePlanId = "Customer is not subscribed to a plan";
                                if (account.StripePlanID != null)
                                {
                                    stripePlanId = account.StripePlanID;
                                }

                                //Log Activity:
                                PlatformLogManager.LogActivity(
                                    CategoryType.StripeEvent,
                                    ActivityType.StripeEvent_RecurringCharge_Succeeded,
                                    "Recurring charge for " + amount + " (" + stripePlanId + ")",
                                    request + " | " + stripeEvent.Id,
                                    account.AccountID.ToString(),
                                    account.AccountName,
                                    null,
                                    null,
                                    null,
                                    null,
                                    "webhook"
                                );

                            }
                            else
                            {

                                //If this is a one-off charge, we include the charge details

                                //Log Activity:
                                PlatformLogManager.LogActivity(
                                    CategoryType.StripeEvent,
                                    ActivityType.StripeEvent_Charge_Succeeded,
                                    "Charge for " + amount,
                                    request + " | " + stripeEvent.Id + " | " + stripeChargeId,
                                    account.AccountID.ToString(),
                                    account.AccountName
                                );

                            }

                            //Return 200 response
                            httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later        

                            #endregion
                        }
                        catch (Exception e)
                        {
                            #region Return a HttpStatusCode.OK/200 code to Stripe

                            //If this fails we send Stripe a 200 response since the worke role has a work order in the queue. This avoide Stripe retrying the call and adding duplicate work orders to the queue

                            //Log exception and email platform admins
                            PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                                e,
                                "attempting to log additional  data for the  the 'charge.succeeded' stripe webhook event",
                                System.Reflection.MethodBase.GetCurrentMethod()
                            );


                            //Return 200 response (Since the message queue has been sent to the worker as well as the exception logged we return a 200 response so that Stripe does not attempt to call the webhook again and the customer does not get multiple invoices)
                            httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later  

                            #endregion
                        }

                    }
                    catch (Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to capture  the 'charge.succeeded' stripe webhook event",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );

                        // Sending message to Queue failed, return a bad request response so that stripe can attempt to call the webhook later
                        httpResponse = Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); //<-- Stripe will attempt to call the webhook later
                    }

                    #endregion

                    break;

                case "charge.failed":

                    #region Send Charge Failed Message Queue

                    try
                    {
                        //Unpack stripe event object

                        string customerId = stripeEvent.Data.Object.customer;
                        string invoiceId = stripeEvent.Data.Object.invoice;
                        string amount = stripeEvent.Data.Object.amount;
                        string failureMessage = stripeEvent.Data.Object.failure_message;
                        //string eventObject = stripeEvent.Data.Object.ToString();
                        string StripeEvent_Id = stripeEvent.Id;
                        string stripeChargeId = stripeEvent.Data.Object.id;

                        //Get the Event requestId, or RequestType for the event.
                        string request = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            request = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }

                        //StripeWebhookLog logType = null;

                        if (!String.IsNullOrEmpty(invoiceId))
                        {
                            //logType = StripeWebhookLogActivity.ChargeFailed_Automatic;

                            //Send message queue for PlatformWorker to process.
                            //Since the charge has an associated invoice, we will issue a command to update account Delinquent state and send the appropriate emails
                            PlatformQueuePipeline.SendMessage.StripeRecurringChargeFailed(customerId, stripeChargeId, amount, failureMessage, StripeEvent_Id);
                        }
                        else
                        {
                            // ** This charge is a one off, does not have an associated invoice and will not cause the account to become delinquent or execute the dunning process.

                            if (!String.IsNullOrEmpty(customerId)) //<--Make sure this is valid stripe customer, and not a first time signup with credit card issues
                            {
                                PlatformQueuePipeline.SendMessage.StripeChargeFailed(customerId, stripeChargeId, amount, failureMessage, StripeEvent_Id);
                            }

                        }


                        try
                        {
                            if (!String.IsNullOrEmpty(customerId))
                            {
                                #region Attempt to log the stripe event and return a HttpStatusCode.OK/200 code to Stripe (only if this is an actual stripe customer)

                                //Get the account
                                var account = AccountManager.GetAccount(customerId, true, AccountManager.AccountIdentificationType.StripeCustomerID);

                                var stripePlanId = "Customer is not subscribed to a plan";
                                if (account.StripePlanID != null)
                                {
                                    stripePlanId = account.StripePlanID;
                                }


                                //Log the webhook activity:
                                if (!String.IsNullOrEmpty(invoiceId))
                                {
                                    //If an invoice is involved in the charge we include the subscription details and invoiceId

                                    //Log Activity:
                                    PlatformLogManager.LogActivity(
                                        CategoryType.StripeEvent,
                                        ActivityType.StripeEvent_RecurringCharge_Failed,
                                        "Recurring charge: " + stripePlanId + " | " + Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(amount),
                                        request + " | " + stripeEvent.Id + " | " + invoiceId,
                                        account.AccountID.ToString(),
                                        account.AccountName,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "webhook"

                                    );


                                }
                                else
                                {

                                    //If this is a one-off charge, we include the charge details
                                    PlatformLogManager.LogActivity(
                                        CategoryType.StripeEvent,
                                        ActivityType.StripeEvent_Charge_Failed,
                                        "Single Charge (No Plan or Invoice) | " + Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(amount),
                                        request + " | " + stripeEvent.Id + " | " + stripeChargeId,
                                        account.AccountID.ToString(),
                                        account.AccountName,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "webhook"
                                    );
                                }

                                // Log Billing Issue
                                PlatformLogManager.LogActivity(
                                        CategoryType.Billing,
                                        ActivityType.Billing_Issue,
                                        "Charge failed for " + account.AccountName,
                                        stripeEvent.Id,
                                        account.AccountID.ToString(),
                                        account.AccountName,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "webhook"
                                    );


                                //Return 200 response
                                httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later 

                                #endregion
                            }

                        }
                        catch (Exception e)
                        {
                            #region Return a HttpStatusCode.OK/200 code to Stripe

                            //If this fails we send Stripe a 200 response since the worke role has a work order in the queue. This avoide Stripe retrying the call and adding duplicate work orders to the queue

                            //Log exception and email platform admins
                            PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                                e,
                                "attempting to log additional  data for the  the 'charge.failed' stripe webhook event",
                                System.Reflection.MethodBase.GetCurrentMethod()
                            );

                            //Return 200 response (Since the message queue has been sent to the worker as well as the exception logged we return a 200 response so that Stripe does not attempt to call the webhook again and the customer does not get multiple failed cahrge notices)
                            httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later  

                            #endregion
                        }

                    }
                    catch (Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to capture  the 'charge.failed' stripe webhook event",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );


                        // Sending message to Queue failed, return a bad request response so that stripe can attempt to call the webhook later
                        httpResponse = Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); //<-- Stripe will attempt to call the webhook later

                    }

                    #endregion

                    break;

                case "charge.refunded":

                    #region Log Charge Refunded Event

                    try
                    {
                        //Unpack stripe event object
                        string stripeCustomerID = stripeEvent.Data.Object.customer;
                        string chargeId = stripeEvent.Id;

                        //Get the Event requestId, or RequestType for the event.
                        string request = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            request = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }

                        //Get the account
                        var account = AccountManager.GetAccount(stripeCustomerID, true, AccountManager.AccountIdentificationType.StripeCustomerID);

                        //Log the Activity
                        PlatformLogManager.LogActivity(
                                CategoryType.StripeEvent,
                                ActivityType.StripeEvent_Charge_Refunded,
                                chargeId,
                                request,
                                account.AccountID.ToString(),
                                account.AccountName,
                                null,
                                null,
                                null,
                                null,
                                "webhook"
                            );

                        //Return 200 response
                        httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later        
                    }
                    catch (Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to capture  the 'charge.refunded' stripe webhook event",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );

                        httpResponse = Request.CreateResponse(HttpStatusCode.BadRequest, e.Message); //<-- Stripe will attempt to call the webhook later
                    }

                    #endregion

                    break;

                case "customer.source.updated":

                    #region Log Customer Card Updated Event

                    try
                    {
                        //Get customer id from the stripe event object
                        string customerID = stripeEvent.Data.Object.id;

                        //Get the Event requestId, or RequestType for the event.
                        string request = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            request = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }

                        //Messages to account owners has occuered on succesful response.

                        //Get the account
                        var account = AccountManager.GetAccount(customerID, true, AccountManager.AccountIdentificationType.StripeCustomerID);

                        //Log the Activity
                        PlatformLogManager.LogActivity(
                                CategoryType.StripeEvent,
                                ActivityType.StripeEvent_CreditCard_Updated,
                                customerID,
                                request + " | " + stripeEvent.Id,
                                account.AccountID.ToString(),
                                account.AccountName,
                                null,
                                null,
                                null,
                                null,
                                "webhook"
                            );


                        //Return the response
                        httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later          
                    }
                    catch (Exception e)
                    {

                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to capture  the 'customer.source.updated' stripe webhook event",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );

                        httpResponse = Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
                    }

                    #endregion

                    break;


                case "customer.created":

                    #region Log Customer Created Event

                    try
                    {
                        //Get customer id from the stripe event object
                        string customerID = stripeEvent.Data.Object.id;

                        //Get the Event requestId, or RequestType for the event.
                        string request = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            request = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }

                        //Get the account
                        //var account = AccountManager.GetAccountByStripeCustomerID(customerID, false);
                        var account = AccountManager.GetAccount(customerID);

                        var stripePlanId = "Customer created without subscription";
                        if (account.StripePlanID != null)
                        {
                            stripePlanId = account.StripePlanID;
                        }

                        //Log the Activity
                        PlatformLogManager.LogActivity(
                                CategoryType.StripeEvent,
                                ActivityType.StripeEvent_Customer_Created,
                                stripePlanId,
                                request + " | " + stripeEvent.Id,
                                account.AccountID.ToString(),
                                account.AccountName,
                                null,
                                null,
                                null,
                                null,
                                "webhook"
                            );

                        //Return the response
                        httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later          
                    }
                    catch (Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to capture  the 'customer.created' stripe webhook event",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );


                        httpResponse = Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
                    }

                    #endregion

                    break;

                case "customer.subscription.created":

                    #region Log Subscription Created Event

                    try
                    {
                        //Get customer id from the stripe event object
                        string customerID = stripeEvent.Data.Object.customer;

                        //Get the Event requestId, or RequestType for the event.
                        string request = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            request = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }

                        //Get the account
                        //var account = AccountManager.GetAccountByStripeCustomerID(customerID, false);
                        var account = AccountManager.GetAccount(customerID, true, AccountManager.AccountIdentificationType.StripeCustomerID);


                        //Log the Activity
                        PlatformLogManager.LogActivity(
                                CategoryType.StripeEvent,
                                ActivityType.StripeEvent_Subscription_Created,
                                account.StripePlanID + " | " + Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars((string)stripeEvent.Data.Object.plan.amount),
                                request + " | " + stripeEvent.Id,
                                account.AccountID.ToString(),
                                account.AccountName,
                                null,
                                null,
                                null,
                                null,
                                "webhook"
                            );

                        //Return the response
                        httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later          
                    }
                    catch (Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to capture  the 'customer.subscription.created' stripe webhook event",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );

                        httpResponse = Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
                    }

                    #endregion

                    break;

                case "customer.subscription.updated":

                    #region Log Subscription Updated Event & Send Message Que on Status Update

                    try
                    {
                        //Get customer id from the stripe event object
                        string customerID = stripeEvent.Data.Object.customer;

                        //Get the Event requestId, or RequestType for the event.
                        string request = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            request = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }

                        #region Manage Subscription Status Changes

                        try
                        {
                            //Get status updates and execute thusly

                            string newSubscriptionStatus = stripeEvent.Data.Object.status;
                            string previousSubscriptionStatus = stripeEvent.Data.PreviousAttributes.status;

                            if (!String.IsNullOrEmpty(newSubscriptionStatus) && !String.IsNullOrEmpty(previousSubscriptionStatus))
                            {
                                //The subscriptio status has changed:

                                string loggingDescription = "Status changed from '" + previousSubscriptionStatus + "' to '" + newSubscriptionStatus + "'";

                                //Send subscription update to worker for processing..
                                PlatformQueuePipeline.SendMessage.StripeSubscriptionStatusChanged(customerID, newSubscriptionStatus, previousSubscriptionStatus);

                                try
                                {
                                    #region Attempt to log additional data about the stripe event and return a HttpStatusCode.OK/200 code to Stripe

                                    //Get the account
                                    //var account = AccountManager.GetAccountByStripeCustomerID(customerID, false);
                                    var account = AccountManager.GetAccount(customerID);

                                    //Log the Activity
                                    PlatformLogManager.LogActivity(
                                            CategoryType.StripeEvent,
                                            ActivityType.StripeEvent_Subscription_StatusChanged,
                                            loggingDescription + " on subscription: " + account.StripePlanID,
                                            request + " | " + stripeEvent.Id,
                                            account.AccountID.ToString(),
                                            account.AccountName,
                                            null,
                                            null,
                                            null,
                                            null,
                                            "webhook"
                                        );

                                    //Return the response
                                    httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later          

                                    #endregion
                                }
                                catch (Exception e)
                                {
                                    #region Return a HttpStatusCode.OK/200 code to Stripe

                                    //If this fails we send Stripe a 200 response since the worke role has a work order in the queue. This avoide Stripe retrying the call and adding duplicate work orders to the queue

                                    //Log exception and email platform admins
                                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                                        e,
                                        "attempting to log status update for the  the 'customer.subscription.updated' stripe webhook event",
                                        System.Reflection.MethodBase.GetCurrentMethod()
                                    );


                                    httpResponse = Request.CreateResponse(HttpStatusCode.OK, e.Message); //<-- Message Queue Sent, tell Stripe NOT to send message again


                                    #endregion
                                }

                            }
                            else
                            {
                                //Get the account
                                var account = AccountManager.GetAccount(customerID);

                                //Log the Activity
                                PlatformLogManager.LogActivity(
                                        CategoryType.StripeEvent,
                                        ActivityType.StripeEvent_Subscription_Updated,
                                        account.StripePlanID + " | " + Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars((string)stripeEvent.Data.Object.plan.amount),
                                        request + " | " + stripeEvent.Id,
                                        account.AccountID.ToString(),
                                        account.AccountName,
                                        null,
                                        null,
                                        null,
                                        null,
                                        "webhook"
                                    );

                                //Return the response
                                httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later  
                            }

                        }
                        catch
                        {

                        }

                        #endregion

                    }
                    catch (Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to capture  the 'customer.subscription.updated' stripe webhook event",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );

                        httpResponse = Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
                    }

                    #endregion

                    break;

                case "customer.updated":

                    #region Log Customer Updated Event & Send Updated Delinquent Status To Platform Worker if changed from 'true' to 'false'

                    try
                    {
                        //Get customer id from the stripe event object
                        string customerID = stripeEvent.Data.Object.id;

                        //Get the Event requestId, or RequestType for the event.
                        string request = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            request = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }

                        //Get the account
                        //var account = AccountManager.GetAccountByStripeCustomerID(customerID, false);
                        var account = AccountManager.GetAccount(customerID, true, AccountManager.AccountIdentificationType.StripeCustomerID);


                        var stripePlanId = "Customer is not subscribed to a plan";
                        if (account.StripePlanID != null)
                        {
                            stripePlanId = account.StripePlanID;
                        }

                        //Log the Activity
                        PlatformLogManager.LogActivity(
                                CategoryType.StripeEvent,
                                ActivityType.StripeEvent_Customer_Updated,
                                stripePlanId,
                                request + " | " + stripeEvent.Id,
                                account.AccountID.ToString(),
                                account.AccountName,
                                null,
                                null,
                                null,
                                null,
                                "webhook"
                            );

                        #region Manage Customer Delinquney State Changes

                        try
                        {
                            //Check if  previous attribute includes a delinquency change
                            bool newDelinquentStatus = stripeEvent.Data.Object.delinquent;
                            bool previousDelinquentStatus = stripeEvent.Data.PreviousAttributes.delinquent;

                            try
                            {
                                //Send Updated Delinquent Status To Platform Worker if changed

                                //Send delinquency update to worker for processing..
                                PlatformQueuePipeline.SendMessage.StripeCustomerDelinquencyChanged(account.AccountID.ToString(), newDelinquentStatus);

                                try
                                {
                                    #region Attempt to log additional data about the stripe event and return a HttpStatusCode.OK/200 code to Stripe

                                    string deliquencyDescription = string.Empty;

                                    if (newDelinquentStatus == false && previousDelinquentStatus == true)
                                    {
                                        //Customer is no loner delinquent.
                                        deliquencyDescription = "Delinquency Cleared";
                                    }

                                    if (newDelinquentStatus == true && previousDelinquentStatus == false)
                                    {
                                        //Customer is now delinquent.
                                        deliquencyDescription = " Account Delinquent";

                                    }

                                    //Log the Activity
                                    PlatformLogManager.LogActivity(
                                            CategoryType.StripeEvent,
                                            ActivityType.StripeEvent_Customer_DelinquencyChanged,
                                            deliquencyDescription + " | " + stripePlanId,
                                            request + " | " + stripeEvent.Id,
                                            account.AccountID.ToString(),
                                            account.AccountName
                                        );


                                    //Return the response
                                    httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later          


                                    #endregion
                                }
                                catch (Exception e)
                                {
                                    #region Return a HttpStatusCode.OK/200 code to Stripe

                                    //If this fails we send Stripe a 200 response since the worke role has a work order in the queue. This avoide Stripe retrying the call and adding duplicate work orders to the queue

                                    //Log exception and email platform admins
                                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                                        e,
                                        "attempting to log additional data for a delinquncy change in the 'customer.updated' stripe webhook event",
                                        System.Reflection.MethodBase.GetCurrentMethod()
                                    );


                                    httpResponse = Request.CreateResponse(HttpStatusCode.OK, e.Message); //<-- Alert stripe that Message Queue sent, Exception logged so do not call WebHook again


                                    #endregion
                                }
                            }
                            catch (Exception e)
                            {
                                #region Mange Exception

                                //Log exception and email platform admins
                                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                                    e,
                                    "attempting to send message queue related a delinquency change in the 'customer.updated' stripe webhook event",
                                    System.Reflection.MethodBase.GetCurrentMethod()
                                );

                                return Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);//<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later          

                                #endregion
                            }
                        }
                        catch
                        {
                            //Fail silently, delinquency is unchanged
                            httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later
                        }

                        #endregion

                    }
                    catch (Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to capture the 'customer.updated' stripe webhook event",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );

                        httpResponse = Request.CreateResponse(HttpStatusCode.BadRequest, e.Message);
                    }

                    #endregion

                    break;

                default:
                    {
                        #region Catch all to log all other event types

                        string details = string.Empty;

                        //Get the Event requestId, or RequestType for the event.
                        string requestIdOrType = "automatic"; //<-- Default until proven otherwise
                        //If the Request string is null (has no ID for the API request) then the request was automatic (e.g. Stripe's automatic subscription handling)
                        if (!string.IsNullOrEmpty(stripeEvent.Request))
                        {
                            requestIdOrType = stripeEvent.Request; //<--- Not an automated call, was manually issued by a user request, or the Stripe dashboard
                        }

                        //Log the Activity
                        PlatformLogManager.LogActivity(
                                CategoryType.StripeEvent,
                                ActivityType.StripeEvent_Other,
                                StripeEvent_Type,
                                requestIdOrType + " | " + stripeEvent.Id,
                                null,
                                null,
                                null,
                                null,
                                null,
                                null,
                                "webhook"
                            );


                        httpResponse = Request.CreateResponse(HttpStatusCode.OK); //<--Stripe expects a 200 response for successful transactions, otherwise it will call the service again later

                        #endregion

                        break;
                    }


            }

            //Log the event:
            StripeWebhookEventsLogManager.LogWebhookEvent(stripeEvent_ID);

            return httpResponse;

        }

    }
}
