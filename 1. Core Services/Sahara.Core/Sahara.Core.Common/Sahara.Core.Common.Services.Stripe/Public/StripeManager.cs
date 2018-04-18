using System;
using System.Collections.Generic;
using Sahara.Core.Common.ResponseTypes;
using Stripe;
using Sahara.Core.Common.Services.Stripe.Models;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Newtonsoft.Json;

namespace Sahara.Core.Common.Services.Stripe
{
    public class StripeManager
    {

        // The default Sahara scenario requires that a paid plan is in place before allowing for other transactions (or microtransactions) to occur
        // It is just as easy to remove all plans and only allow for micro transactions, or to have both
        //Sahara also only maintains ONE credit card at a time by default for simplicity 

        public StripeManager()
        {
            global::Stripe.StripeConfiguration.SetApiKey(Settings.Services.Stripe.Account.ApiKey);
        }

        #region Customers

        /* Not Used - We create the Customer with the Card & Subscription at once
        public DataAccessResponseType CreateCustomer(string accountID, string accountName, string creatorEmail, string planID)
        {
            var response = new DataAccessResponseType();

            var customerService = new StripeCustomerService();

            var customer = new StripeCustomerCreateOptions();

            var metaData = new Dictionary<string, string>();

            metaData.Add("Account ID", accountID);
            metaData.Add("Org Name", accountName);
            metaData.Add("Org Name Short", Sahara.Core.Common.AccountNames.ConvertToAccountNameKey(accountName));
            metaData.Add("Org Email", creatorEmail);

            customer.Metadata = metaData;
            customer.Description = accountName;

            //customer.PlanId = "Micro-Monthly";


            try
            {
                StripeCustomer result = customerService.Create(customer);
                response.isSuccess = true;
                response.SuccessCode = result.Id; //<-- return the StripeID for the Account
                return response;
            }
            catch(StripeException s)
            {
                return TransformException(s);
            }

        }*/

        public DataAccessResponseType CreateCustomerAndCard(string accountId, string accountName, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, out string stripeCustomerId, out string stripeCardId)
        {
            var response = new DataAccessResponseType();
            var customerService = new StripeCustomerService();

            var customer = new StripeCustomerCreateOptions();

            var customerMetaData = new Dictionary<string, string>();

            customerMetaData.Add("AccountID", accountId);
            customerMetaData.Add("OrgAccountName", accountName);
            customerMetaData.Add("OrgAccountNameKey", Methods.AccountNames.ConvertToAccountNameKey(accountName));

            customer.Metadata = customerMetaData;
            customer.Description = accountName;

            customer.SourceCard = new SourceCard();

            customer.SourceCard.Name = cardName;
            customer.SourceCard.Number = cardNumber;
            customer.SourceCard.Cvc = cvc;
            customer.SourceCard.ExpirationMonth = expirationMonth;
            customer.SourceCard.ExpirationYear = expirationYear;

            try
            {
                StripeCustomer result = customerService.Create(customer);
                response.isSuccess = true;
                response.SuccessMessage = "Customer & Card have been created!";
                response.SuccessCode = result.Id; //<-- return the StripeID for the Account
                stripeCustomerId = result.Id;
                //stripeCardId = result.StripeDefaultCardId;
                stripeCardId = result.Sources.Data[0].Card.Id;
                return response;
            }
            catch (StripeException s)
            {
                //We don't log or send alerts about this exception. It fails by design and returns a null customer.

                //Log exception and email platform admins
                //PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    //s,
                    //"attempting to create customer and card for " + accountName + "/" + accountId + " via Stripe API",
                    //System.Reflection.MethodBase.GetCurrentMethod()
                //);

                stripeCustomerId = null;
                stripeCardId = null;
                return TransformException(s);
            }


        }

        public DataAccessResponseType Rollback_CreateCustomerAndCard(string customerId)
        {

            var customerService = new StripeCustomerService();
            //var cardService = new StripeCardService();

            try
            {
                //cardService.Delete(stripeCustomerId, stripeCardId);
                customerService.Delete(customerId);
                return new DataAccessResponseType { isSuccess = true };
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to rollback customer and card for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return TransformException(s);
            }

        }



        public StripeCustomer GetCustomer(string customerId)
        {

            var customerService = new StripeCustomerService();
            var customer = new StripeCustomer();

            try
            {
                customer = customerService.Get(customerId);
            }
            catch(StripeException s)
            {
                var errorMessage = s.Message;
                //We don't log or send alerts about this exception. It fails by design and returns a null customer.
                customer = null;
            }
            
            return customer;

        }


        public DataAccessResponseType UpdateCustomerName(string customerId, string newAccountName)
        {
            var response = new DataAccessResponseType();

            var customerService = new StripeCustomerService();

            var update = new StripeCustomerUpdateOptions();

            update.Description = newAccountName;

            try
            {
                StripeCustomer result = customerService.Update(customerId, update);
                response.isSuccess = true;
                response.SuccessCode = result.Id; //<-- return the StripeID for the Account
                return response;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to update the customer name to " + newAccountName + "  for " + customerId + "  via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                return TransformException(s);
            }

        }

        public DataAccessResponseType DeleteCustomer(string customerId)
        {
            var response = new DataAccessResponseType();

            var customerService = new StripeCustomerService();
            
            try
            {
                customerService.Delete(customerId);
                response.isSuccess = true;
                return response;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to delete customer: " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                return TransformException(s);
            }

        }

        #endregion

        #region Subscriptions

        public StripeSubscription GetSubscription(string customerId, string subscriptionId)
        {
            var subscriptionService = new StripeSubscriptionService();

            return subscriptionService.Get(customerId, subscriptionId);
        }

        /// <summary>
        /// Use if Account does not yet have a CustomerId and associated CreditCard on Stripe
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="stripePaymentPlanID"></param>
        /// <param name="cardName"></param>
        /// <param name="cardNumber"></param>
        /// <param name="cvc"></param>
        /// <param name="expirationMonth"></param>
        /// <param name="expirationYear"></param>
        /// <param name="stripeCustomerId"></param>
        /// <param name="stripeSubscriptionId"></param>
        /// <param name="stripeCardId"></param>
        /// <returns></returns>
        public DataAccessResponseType CreateCustomerSubscriptionAndCard(string accountId, string accountName, string stripePaymentPlanID, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, out string stripeCustomerId, out string stripeSubscriptionId, out string stripeCardId)
        {
            var response = new DataAccessResponseType();
            var customerService = new StripeCustomerService();

            var customer = new StripeCustomerCreateOptions();

            var customerMetaData = new Dictionary<string, string>();

            //Does account already have a customerId
            //var stripeMetaData = new Dictionary<string, string>();
            
            //stripeMetaData.Add("TransactionDescription", "Started subscription to " + paymentPlan.PaymentPlanName + " (" + paymentFrequency.PaymentFrequencyName + ")");



            customerMetaData.Add("AccountID", accountId);
            customerMetaData.Add("OrgAccountName", accountName);
            customerMetaData.Add("OrgAccountNameKey", Methods.AccountNames.ConvertToAccountNameKey(accountName));
            //metaData.Add("OrgEmail", creatorEmail);

            customer.Metadata = customerMetaData;
            customer.Description = accountName;

            customer.PlanId = stripePaymentPlanID;     

            customer.SourceCard = new SourceCard();

            customer.SourceCard.Name = cardName;
            customer.SourceCard.Number = cardNumber;
            customer.SourceCard.Cvc = cvc;
            customer.SourceCard.ExpirationMonth = expirationMonth;
            customer.SourceCard.ExpirationYear = expirationYear;


            try
            {
                StripeCustomer result = customerService.Create(customer);
                response.isSuccess = true;
                response.SuccessMessage = "Customer, Card & Subscription have been created!";
                response.SuccessCode = result.Id; //<-- return the StripeID for the Account
                stripeCustomerId = result.Id;
                //stripeSubscriptionId = result.StripeSubscriptionList.StripeSubscriptions[0].Id;
                //stripeCardId = result.StripeDefaultCardId;

                stripeSubscriptionId = result.Subscriptions.Data[0].Id;
                stripeCardId = result.Sources.Data[0].Card.Id;

                return response;
            }
            catch (StripeException s)
            {

                //Log exception and email platform admins (We avoid logging this as it is usually a declined card or incorrect card number)
                /*
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to create a customer, subscription and card for " + accountName + "/" + accountId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );*/
                

                stripeCustomerId = null;
                stripeSubscriptionId = null;
                stripeCardId = null;
                return TransformException(s);
            }

            
        }

        /// <summary>
        /// Use if Account already has a CustomerId and an associated CreditCard on Stripe from previous micro transactions
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="accountName"></param>
        /// <param name="stripePaymentPlanID"></param>
        /// <param name="cardName"></param>
        /// <param name="cardNumber"></param>
        /// <param name="cvc"></param>
        /// <param name="expirationMonth"></param>
        /// <param name="expirationYear"></param>
        /// <param name="stripeCustomerId"></param>
        /// <param name="stripeSubscriptionId"></param>
        /// <param name="stripeCardId"></param>
        /// <returns></returns>
        public DataAccessResponseType CreateSubscriptionAndCard(string customerId, string accountName, string stripePaymentPlanID, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, out string stripeCustomerId, out string stripeSubscriptionId, out string stripeCardId)
        {
            var response = new DataAccessResponseType();

            //var customerService = new StripeCustomerService();
            //var customerUpdateOptions = new StripeCustomerUpdateOptions();
            
            var subscriptionService = new StripeSubscriptionService();
            var stripeSubscriptionCreateOptions = new StripeSubscriptionCreateOptions();

            stripeSubscriptionCreateOptions.PlanId = stripePaymentPlanID;

            stripeSubscriptionCreateOptions.Card = new StripeCreditCardOptions();

            //Will replace and remove the current default card (if one exists)
            stripeSubscriptionCreateOptions.Card.Name = cardName;
            stripeSubscriptionCreateOptions.Card.Number = cardNumber;
            stripeSubscriptionCreateOptions.Card.Cvc = cvc;
            stripeSubscriptionCreateOptions.Card.ExpirationMonth = expirationMonth;
            stripeSubscriptionCreateOptions.Card.ExpirationYear = expirationYear;
 
            try
            {

                StripeSubscription result = subscriptionService.Create(customerId, stripePaymentPlanID, stripeSubscriptionCreateOptions);

                response.isSuccess = true;
                response.SuccessMessage = "Card & Subscription have been created!";
                response.SuccessCode = result.Id; //<-- return the StripeID for the Account
                stripeCustomerId = customerId;
                stripeSubscriptionId = result.Id;
                stripeCardId = GetCustomerDefaultCard(customerId).StripeCardId;
                return response;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins (We avoid logging this as it is usually a declined card or incorrect card number)
                /*
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to create a subscription and card for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );
                */

                stripeCustomerId = null;
                stripeSubscriptionId = null;
                stripeCardId = null;
                return TransformException(s);
            }


        }


        public DataAccessResponseType ReactivateExistingSubscription(string customerId, string subscriptionId, string stripePaymentPlanID, Dictionary<string, string> metaData)
        {
            // From the Stripe Docs: https://support.stripe.com/questions/how-can-i-resume-a-subscription-after-it-has-been-canceled
            /*
             * If your customer canceled with at_period_end=true, and the subscription hasn't been fully canceled yet, then you can resume the subscription with the update subscription API call.
             * You'll want to set plan to the same ID as the current plan. This will update the subscription to set cancel_at_period_end=false, ensuring that the subscription won't be canceled at the end of the billing period.
             * You can also use the “Reactivate Subscription” option from your dashboard.
             * 
             * If the cancelation already went through (that is, your customer no longer has an active subscription), you'll want to create a new subscription for that customer.
             * Keep in mind that we'll immediately start your customer on the plan again since we're starting a new billing cycle, so this could result in a new charge.
             * You can override this behavior to control when your customer is charged (e.g., to maintain the old billing cycle).
             * 
             */

            var response = new DataAccessResponseType();

            try
            {
                var subscriptionService = new StripeSubscriptionService();

                try
                {
                    StripeSubscription subscription = subscriptionService.Update(customerId, subscriptionId, new StripeSubscriptionUpdateOptions { PlanId = stripePaymentPlanID, Metadata = metaData });
                    response.isSuccess = true;
                    return response;
                }
                catch (StripeException s)
                {
                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        s,
                        "attempting to reactivate the user subscription to " + stripePaymentPlanID + " for " + customerId + " via Stripe API",
                        System.Reflection.MethodBase.GetCurrentMethod()
                    );


                    return TransformException(s);
                }

            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to reactivate the user subscription to " + stripePaymentPlanID + " for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                return TransformException(s);
            }




        }

        public DataAccessResponseType ReactivateClosedSubscription(string customerId, string stripePaymentPlanID, Dictionary<string, string> metaData, out string stripeSubscriptionId)
        {

            // From the Stripe Docs: https://support.stripe.com/questions/how-can-i-resume-a-subscription-after-it-has-been-canceled
            /*
             * If your customer canceled with at_period_end=true, and the subscription hasn't been fully canceled yet, then you can resume the subscription with the update subscription API call.
             * You'll want to set plan to the same ID as the current plan. This will update the subscription to set cancel_at_period_end=false, ensuring that the subscription won't be canceled at the end of the billing period.
             * You can also use the “Reactivate Subscription” option from your dashboard.
             * 
             * If the cancelation already went through (that is, your customer no longer has an active subscription), you'll want to create a new subscription for that customer.
             * Keep in mind that we'll immediately start your customer on the plan again since we're starting a new billing cycle, so this could result in a new charge.
             * You can override this behavior to control when your customer is charged (e.g., to maintain the old billing cycle).
             * 
             */

            var response = new DataAccessResponseType();

            //var customerService = new StripeCustomerService();
            //var customerUpdateOptions = new StripeCustomerUpdateOptions();

            var subscriptionService = new StripeSubscriptionService();
            var stripeSubscriptionCreateOptions = new StripeSubscriptionCreateOptions();
            stripeSubscriptionCreateOptions.Metadata = metaData;

            stripeSubscriptionCreateOptions.PlanId = stripePaymentPlanID;

            try
            {

                StripeSubscription result = subscriptionService.Create(customerId, stripePaymentPlanID, stripeSubscriptionCreateOptions);

                response.isSuccess = true;
                response.SuccessMessage = "Subscription has been reactivated!";
                response.SuccessCode = result.Id; //<-- return the StripeID for the Account
                //stripeCustomerId = customerId;
                stripeSubscriptionId = result.Id;

                return response;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to reactivate a subscription for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                //stripeCustomerId = null;
                stripeSubscriptionId = null;

                return TransformException(s);
            }

        }


        public DataAccessResponseType Rollback_CreateCustomerSubscriptionAndCard(string customerId) //, string stripeCardId)
        {

            var customerService = new StripeCustomerService();
            //var cardService = new StripeCardService();

            try
            {
                //cardService.Delete(stripeCustomerId, stripeCardId);
                customerService.Delete(customerId);
                return new DataAccessResponseType { isSuccess = true };
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to rollback the customer subscription for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return TransformException(s);
            }

        }


        #region Other Scenarios

        // In some scenarios you may wish to allow for micro transactions to occur BEFORE a subscription is created. If so you would already have a CustomerId & possbily a CardId on file and only need to add the subscription info
        //The default Sahara scenario requires that a paid plan is in place before allowing for microtransactions to occur, it is just as easy to remove all plans and only allow for micro transactions, or to have both

        //Customer exists, but subscription & card does not:
        //public DataAccessResponseType CreateSubscriptionAndCard(string stripeCustomerId, string stripePaymentPlanID, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, out string stripeSubscriptionID, out string stripeCardId)

        //Customer & Card exist, but subscription does not:
        //public DataAccessResponseType CreateSubscription(string stripeCustomerId, string stripeCardId, string stripePaymentPlanID, out string stripeSubscriptionID)

        #endregion


        public DataAccessResponseType UpdateCustomerSubscription(string customerId, string subscriptionId, string newStripePaymentPlanID, Dictionary<string, string> metaData)
        {
            var response = new DataAccessResponseType();
            
            try
            {

                //var customerService = new StripeCustomerService();
                var subscriptionService = new StripeSubscriptionService();

                //var updateCustomerSubscription = new StripeSubscriptionUpdateOptions();// StripeCustomerUpdateSubscriptionOptions();

                //updateCustomerSubscription.PlanId = stripePaymentPlanID;

                //var metaData = new Dictionary<string, string>();
                //metaData.Add("TransactionDescription", "Updated plan to " + newStripePaymentPlanID);

                try
                {
                    StripeSubscription subscription = subscriptionService.Update(customerId, subscriptionId, new StripeSubscriptionUpdateOptions { PlanId = newStripePaymentPlanID, Metadata = metaData });
                    response.isSuccess = true;
                    return response;
                }
                catch (StripeException s)
                {
                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        s,
                        "attempting to update the user subscription to " + newStripePaymentPlanID + " for " + customerId + " via Stripe API",
                        System.Reflection.MethodBase.GetCurrentMethod()
                    );


                    return TransformException(s);
                }

            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to update the user subscription to " + newStripePaymentPlanID + " for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                return TransformException(s);
            }
        }


        public DataAccessResponseType CancelSubscription(string customerId, string subscriptionId)
        {
            var response = new DataAccessResponseType();

            //var customerService = new StripeCustomerService();
            var subscriptionService = new StripeSubscriptionService();

            try
            {
                subscriptionService.Cancel(customerId, subscriptionId, true);
                response.isSuccess = true;
                return response;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to cancel subscription: " + subscriptionId + "  for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return TransformException(s);
            }

        }




        #endregion

        #region Cards

        public CreditCardInfo GetCustomerDefaultCard(string customerId)
        {
            //var cards = new List<string>();

            try
            {
                var stripeCustomer = GetCustomer(customerId);

                if (stripeCustomer.Sources.Data == null)
                {
                    //return "No card on file";
                    return null;
                }
                else
                {
                    var cardService = new StripeCardService();
                    //IEnumerable<StripeCard> stripeCards = cardService.List(customerID);
                    var stripeCard = cardService.Get(customerId, stripeCustomer.Sources.Data[0].Card.Id);

                    //return stripeCard.Brand + " - ****" + stripeCard.Last4;
                    return new CreditCardInfo { CardName = stripeCard.Name, CardBrand = stripeCard.Brand, Last4 = stripeCard.Last4, ExpirationMonth = stripeCard.ExpirationMonth, ExpirationYear = stripeCard.ExpirationYear, StripeCardId = stripeCard.Id };
                }
            }
            catch(StripeException s)
            {
                var errorMessage = s.Message;

                //We don't log or send alerts about this exception. It fails by design and returns a null card.
                return null;
            }
            catch(Exception e)
            {
                var errorMessage = e.Message;
                //We don't log or send alerts about this exception. It fails by design and returns a null card.
                return null;
            }

        }

        public CreditCardInfo GetCard(string customerId, string cardId)
        {
            try
            {
                var cardService = new StripeCardService();
                //IEnumerable<StripeCard> stripeCards = cardService.List(customerID);
                var stripeCard = cardService.Get(customerId, cardId);

                //return stripeCard.Brand + " - ****" + stripeCard.Last4;
                return new CreditCardInfo { CardName = stripeCard.Name, CardBrand = stripeCard.Brand, Last4 = stripeCard.Last4, ExpirationMonth = stripeCard.ExpirationMonth, ExpirationYear = stripeCard.ExpirationYear, StripeCardId = stripeCard.Id };

            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get retrieve card " + cardId + " for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                return null;
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to get retrieve card " + cardId + " for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                return null;
            }

        }

        public DataAccessResponseType UpdateCustomerDefaultCreditCard(string customerId, string newCardName, string newCardNumber, string newCvc, string newExpirationMonth, string newExpirationYear, out string newStripeCardId)
        {

            var response = new DataAccessResponseType();

            var cardService = new StripeCardService();
            var cardCreationOptions = new StripeCardCreateOptions();

            cardCreationOptions.SourceCard = new SourceCard();

            cardCreationOptions.SourceCard.Name = newCardName;
            cardCreationOptions.SourceCard.Number = newCardNumber;
            cardCreationOptions.SourceCard.Cvc = newCvc;
            cardCreationOptions.SourceCard.ExpirationMonth = newExpirationMonth;
            cardCreationOptions.SourceCard.ExpirationYear = newExpirationYear;

            StripeCard newCard;

            try
            {

                //Add new card
                newCard = cardService.Create(customerId, cardCreationOptions);

                if (newCard != null)
                {

                    //Delete all other cards (new card will automatically become the default)
                    IEnumerable<StripeCard> cards = cardService.List(customerId);
                    foreach (StripeCard card in cards)
                    {
                        if (card.Id != newCard.Id)
                        {
                            cardService.Delete(customerId, card.Id);
                        }
                    }

                    //Return the new card ID:
                    newStripeCardId = newCard.Id;

                    response.isSuccess = true;
                    response.SuccessMessage = "Card has been updated!";
                    return response;
                }
                else
                {
                    response.isSuccess = false;
                    newStripeCardId = String.Empty;
                    return response;
                }


            }
            catch (StripeException s)
            {
                //Log exception and email platform admins (Since users may use incorrect data we do not alert or log this very common and simple error)
                /*
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to update the default customer card for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );*/
                newStripeCardId = String.Empty;
                return TransformException(s);
            }


        }


        /* Not Used: Only one card per Customer
        /// <summary>
        /// Returns a list of last four numbers and expirations
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        public List<string> GetCustomerCards(string customerID)
        {
            var cards = new List<string>();

            var cardService = new StripeCardService();
            var stripeCards = cardService.List(customerID);

            foreach(StripeCard card in stripeCards)
            {
                var cardType = card.Type;

                /* Uncomment to get short card type names
                if(cardType == "American Express")
                {
                    cardType = "Amex";
                }
                else if(cardType == "Mastercard")
                {
                    cardType = "Master";
                }
                else if(cardType == "Discover")
                {
                    cardType = "Disc";
                }
                else if(cardType == "Diners Club")
                {
                    cardType = "Diners";
                }* /

                cards.Add(cardType.ToUpper() + " - ****" + card.Last4);
            }

            return cards;

        }*/

        /* Not Used - We create the Card with the Customer & Subscription at once
        public DataAccessResponseType AddCustomerCard(string stripeCustomerID, string cardNumber, string cvc, string expirationMonth, string expirationYear, string cardName)
        {
            var response = new DataAccessResponseType();

            var cardService = new StripeCardService();
            var cardCreationOptions = new StripeCardCreateOptions(); 

            cardCreationOptions.CardNumber = cardNumber;
            cardCreationOptions.CardCvc = cvc;
            cardCreationOptions.CardName = cardName;
            cardCreationOptions.CardExpirationMonth = expirationMonth;
            cardCreationOptions.CardExpirationYear = expirationYear;

            StripeCard stripeCard;

            try
            {
                stripeCard = cardService.Create(stripeCustomerID, cardCreationOptions);
            }
            catch(StripeException e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            if(stripeCard != null)
            {
                response.isSuccess = true;
            }

            return response;

        }*/

        #endregion

        #region Plans

        public DataAccessResponseType CreatePlan(string id, string name, string amountInCents, string interval, int intervalCount)
        {
            var response = new DataAccessResponseType();
            var planService = new StripePlanService();

            var plan = new StripePlanCreateOptions();

            plan.Id = id;
            plan.Name = name;
            plan.Amount = Convert.ToInt32(amountInCents.Replace(".",""));
            plan.Currency = "USD";

            plan.Interval = interval.ToLower();
            plan.IntervalCount = intervalCount;

            StripePlan createdPlan;

            try
            {
                //Note: Yearly plans are the maximum available on Stripe. If weekly plans are required, some refactoring of business logic will be needed

                createdPlan = planService.Create(plan);
                if(createdPlan != null)
                {
                    response.isSuccess = true;
                    
                }
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to create plan '" + name + "' with id '" + id + "' and amount '" + amountInCents + "' via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                return TransformException(s);
            }
            return response;
        }

        public IEnumerable<StripePlan> GetPlans()
        {
            var planService = new StripePlanService();

            var listOptions = new StripeListOptions();
            listOptions.Limit = 100;

            return planService.List(listOptions);
        }

        public StripePlan GetPlan(string planID)
        {
            var planService = new StripePlanService();

            return planService.Get(planID);
        }

        public DataAccessResponseType PlanExists(string planName)
        {
            var response = new DataAccessResponseType();
            var planService = new StripePlanService();

            var planId = planName.ToLower(); // GetStripePlanId(planName);

            try
            {
                var plan = planService.Get(planId);

                if (plan != null)
                {
                    response.isSuccess = true;
                    return response;
                }
                else
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "No such plan exists in Stripe.";
                    return response;
                }
            }
            catch (StripeException s)
            {
                if(s.Message.ToLower().Contains("no such plan"))
                {
                    //Ignore this
                    response.isSuccess = false;
                    response.ErrorMessage = "No such plan exists in Stripe.";
                    return response;
                }
                else
                {
                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        s,
                        "attempting to check if plan '" + planName + "' exists via Stripe API",
                        System.Reflection.MethodBase.GetCurrentMethod()
                    );

                    return TransformException(s);
                }

            }
            catch(Exception e)
            {
                if (e.Message.ToLower().Contains("no such plan"))
                {
                    //Ignore this
                    response.isSuccess = false;
                    response.ErrorMessage = "No such plan exists in Stripe.";
                    return response;
                }
                else
                {
                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to check if plan '" + planName + "' exists via Stripe API",
                        System.Reflection.MethodBase.GetCurrentMethod()
                    );


                    response.isSuccess = false;
                    response.ErrorMessage = e.Message;
                    return response;
                }

            }
        }

        public List<string> GetPlanIDs()
        {
            var planIDs = new List<string>();

            var plans = GetPlans();

            foreach(StripePlan plan in plans)
            {
                planIDs.Add(plan.Id);
            }

            return planIDs;
        }

        public DataAccessResponseType DeletePlan(string planName)
        {
            var response = new DataAccessResponseType();
            var planService = new StripePlanService();

            //var planId = GetStripePlanId(planName);

            planService.Delete(planName.ToLower());

            response.isSuccess = true;
            return response;
        }

        /*
        public string GetStripePlanId(string stripePlanName)
        {
            var planService = new StripePlanService();
            var id = string.Empty;

            var plans = planService.List();
            foreach(var plan in plans)
            {
                if(plan.Name.ToLower() == stripePlanName.ToLower())
                {
                    id = plan.Id.ToString();
                }
            }

            return id;
        }

        public DataAccessResponseType UpdatePlanName(string oldPlanName, string newPlanName)
        {
            var response = new DataAccessResponseType();
            var planService = new StripePlanService();

            var palnId = oldPlanName.ToLower(); // GetStripePlanId(oldPlanName);

            planService.Update(palnId, new StripePlanUpdateOptions { Name = newPlanName });

            response.isSuccess = true;
            return response;
        }*/

        #endregion

        #region Charges

        public StripeCharge GetCharge(string chargeId, bool includeBalanceTransactions = false)
        {
            var chargeService = new StripeChargeService();

            chargeService.ExpandBalanceTransaction = includeBalanceTransactions;
            chargeService.ExpandInvoice = true;

            try
            {
                var charge = chargeService.Get(chargeId);
                return charge;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to retrieve charge " + chargeId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }


        public DataAccessResponseType ChargeAccount(string customerId, string description, int amount)
        {
            /*
             * We charge the account right away and do not create an invoice
             * Invoices will kick off the dunning/multiple attempts process which is not needed for micro-transactions
             * If the charge fails there will be no future attempts as is with recurring subscription (hence no invoice)
             * The charge will show up with no invoice on the user side, and if it fails it will be hidden as no future attempts will occur.
             * The user will have to manually attempt the charge again when the credit card or other issue have been resolved/updated
             */
            var chargeService = new StripeChargeService();

            try
            {
                var stripeChargeCreateOptions = new StripeChargeCreateOptions();

                stripeChargeCreateOptions.Amount = amount;
                stripeChargeCreateOptions.Capture = true;
                stripeChargeCreateOptions.Description = description;
                stripeChargeCreateOptions.CustomerId = customerId;
                stripeChargeCreateOptions.Currency = "USD";

                var stripeCharge = chargeService.Create(stripeChargeCreateOptions);

                if (stripeCharge.Paid)
                {
                    return new DataAccessResponseType{ isSuccess = true };
                }
                else{
                    return new DataAccessResponseType{ isSuccess = false, ErrorMessage = "Charge failed to be created." };
                }
            }
            catch (StripeException s)
            {
                //Stripe throws an exception if the charge is declined, s we do not track this as a true exception

                return TransformException(s);
            }

        }
 

        #region Get Initial Charge List(s)

        public IEnumerable<StripeCharge> GetCharges(int limit, string customerId = null, bool includeBalanceTransactions = false)
        {
            var chargeService = new StripeChargeService();
            chargeService.ExpandBalanceTransaction = includeBalanceTransactions;
            chargeService.ExpandInvoice = true;

            try
            {
                var listOptons = new StripeChargeListOptions();

                listOptons.CustomerId = customerId;

                listOptons.Limit = limit;

                var charges = chargeService.List(listOptons);
                return charges;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get charges for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }


        #endregion

        #region Get Paginated Charge List(s)

        public IEnumerable<StripeCharge> GetCharges_Next(int limit, string startingAfterChargeId, string customerId = null, bool includeBalanceTransactions = false)
        {
            var chargeService = new StripeChargeService();
            chargeService.ExpandBalanceTransaction = includeBalanceTransactions;
            chargeService.ExpandInvoice = true;
            try
            {
                var listOptons = new StripeChargeListOptions();
                listOptons.CustomerId = customerId;

                listOptons.Limit = limit;

                listOptons.StartingAfter = startingAfterChargeId;

                var charges = chargeService.List(listOptons);
                return charges;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get paginated_next charges for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        public IEnumerable<StripeCharge> GetCharges_Last(int limit, string endingBeforeChargeId, string customerId = null, bool includeBalanceTransactions = false)
        {
            var chargeService = new StripeChargeService();
            chargeService.ExpandBalanceTransaction = includeBalanceTransactions;
            chargeService.ExpandInvoice = true;

            try
            {
                var listOptons = new StripeChargeListOptions();
                listOptons.CustomerId = customerId;


                listOptons.Limit = limit;

                listOptons.EndingBefore = endingBeforeChargeId;

                var charges = chargeService.List(listOptons);
                return charges;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get painated_last charges for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        public IEnumerable<StripeCharge> GetCharges_SinceHoursAgo(int sinceHoursAgo, string startingAfterChargeId = null, string customerId = null, bool includeBalanceTransactions = false)
        {
            var chargeService = new StripeChargeService();
            chargeService.ExpandBalanceTransaction = includeBalanceTransactions;
            chargeService.ExpandInvoice = true;
            try
            {
                var listOptons = new StripeChargeListOptions();
                listOptons.CustomerId = customerId;
                listOptons.Limit = 100;

                if (startingAfterChargeId != null)
                {
                    listOptons.StartingAfter = startingAfterChargeId;
                }

                var stripeDateFilter = new StripeDateFilter();
                stripeDateFilter.GreaterThan = DateTime.UtcNow.AddHours(sinceHoursAgo * -1);

                listOptons.Created = stripeDateFilter;

                var charges = chargeService.List(listOptons);
                return charges;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get paginated_next charges for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        #endregion

        #endregion

        #region Refunds

        public DataAccessResponseType RefundPayment(string chargeId, int refundAmount, out StripeRefund refundedCharge)
        {
            var refundService = new StripeRefundService();
            var chargeService = new StripeChargeService();
            //var refundedCharge = new StripeCharge();

            var charge = chargeService.Get(chargeId);

            if ((charge.Amount - charge.AmountRefunded) <= 0)
            {
                refundedCharge = null;
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot refund a charge with no balance!" };
            }

            if (refundAmount > (charge.Amount - charge.AmountRefunded))
            {
                refundedCharge = null;
                return new DataAccessResponseType { isSuccess = false, ErrorMessage= "Cannot refund an amount greater than the available balance!" };
            }

            try
            {
                
                if (refundAmount == charge.Amount)
                {
                    refundedCharge = refundService.Create(chargeId, new StripeRefundCreateOptions { Amount = charge.Amount });
                }
                else
                {
                    refundedCharge = refundService.Create(chargeId, new StripeRefundCreateOptions { Amount = refundAmount });
                }

                if (refundedCharge != null)
                {
                    return new DataAccessResponseType { isSuccess = true };
                }
                else
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "An unknown error occuered during refund processing. Please use the Stripe Dashboard." };
                }
                
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to refund " + refundAmount + " for " + chargeId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                refundedCharge = null;
                return TransformException(s);
            }
        }

        /*
        public IEnumerable<StripeRefund> GetRefundsForCharge(string chargeId)
        {
            var chargeService = new StripeCustomerService

            chargeService.

            try
            {
                var ref = balanceService.List(listOptons);
                return balances;
            }
            catch (StripeException s)
            {

                TransformException(s);
                return null;
            }
        }*/

        #endregion

        #region Invoices

        public StripeInvoice GetInvoice(string invoiceId)
        {
            var invoiceService = new StripeInvoiceService();

            try
            {
                var invoice = invoiceService.Get(invoiceId);
                return invoice;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get invoice " + invoiceId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        public StripeInvoice GetUpcomingInvoice(string customerID)
        {
            var invoiceService = new StripeInvoiceService();

            try
            {
                var invoice = invoiceService.Upcoming(customerID);
                return invoice;
            }
            catch (StripeException s)
            {
                var errorMessage = s.Message;

                //We let this silently fail

                //Log exception and email platform admins
                /*
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get the next invoice for " + customerID + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );*/

                return null;
            }
        }

        #region Inoice and charge accounts
        /*
        public DataAccessResponseType InvoiceAndChargeAccount(string customerId, string description, int amount)
        {

            var invoiceLineItemsService = new StripeInvoiceItemService();
            var invoiceService = new StripeInvoiceService();

            try
            {
                //Create invoice line item(s)
                var stripeInvoiceItemCreateOptions = new StripeInvoiceItemCreateOptions();

                stripeInvoiceItemCreateOptions.CustomerId = customerId;
                stripeInvoiceItemCreateOptions.Description = description;
                stripeInvoiceItemCreateOptions.Amount = amount;
                stripeInvoiceItemCreateOptions.Currency = "USD";

                //Add items to service/customer
                invoiceLineItemsService.Create(stripeInvoiceItemCreateOptions);


                //Pull those line items in and generate invoice
                var stripeInvoice = invoiceService.Create(customerId); //<-- Pulls in all pending invoice line items into a final invoice (Great way to add items throughout the day and do all billing at EOD)

                //Pay the invoice immediatly
                stripeInvoice = invoiceService.Pay(stripeInvoice.Id);

                if (stripeInvoice.Paid.HasValue)
                {
                    if (stripeInvoice.Paid.Value)
                    {
                        return new DataAccessResponseType { isSuccess = true };
                    }
                    else
                    {
                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Payment failed." };
                    }
                }
                else
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Payment failed." };
                }

            }
            catch (StripeException s)
            {
                return TransformException(s);
            }

        }
        */

        #endregion

        /*
        public IEnumerable<StripeInvoice> GetUpcomingInvoices(int itemLimit)
        {
            var invoiceService = new StripeInvoiceService();

            var listOptons = new StripeInvoiceListOptions();
            listOptons.Limit = itemLimit;

            listOptons.Date = new StripeDateFilter
            {
                GreaterThan = DateTime.UtcNow
            };


            try
            {
                var invoices = invoiceService.List(listOptons);
                return invoices;
            }
            catch (StripeException s)
            {
                return null;
            }
        }
        */

        #region Get Initial Invoice List(s)

        public IEnumerable<StripeInvoice> GetInvoices(int limit, string customerId = null)
        {
            var invoiceService = new StripeInvoiceService();

            try
            {
                var listOptons = new StripeInvoiceListOptions();

                listOptons.CustomerId = customerId;
                
                listOptons.Limit = limit;

                var invoices = invoiceService.List(listOptons);
                return  invoices;
            }
            catch(StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get invoices for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        public IEnumerable<StripeInvoice> GetInvoices(int limit, DateTime startDate, DateTime endDate, string customerId = null)
        {
            var invoiceService = new StripeInvoiceService();

            try
            {
                var listOptons = new StripeInvoiceListOptions();
                listOptons.CustomerId = customerId;
                

                listOptons.Limit = limit;

                listOptons.Date = new StripeDateFilter
                {
                    GreaterThanOrEqual = startDate,
                    LessThanOrEqual = endDate
                };

                var invoices = invoiceService.List(listOptons);
                return invoices;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get invoices for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        #endregion

        #region Get Paginated Invoice List(s)

        public IEnumerable<StripeInvoice> GetInvoices_Next(int limit, string startingAfterInvoiceId, string customerId = null)
        {
            var invoiceService = new StripeInvoiceService();

            try
            {
                var listOptons = new StripeInvoiceListOptions();
                listOptons.CustomerId = customerId;
                

                listOptons.Limit = limit;

                listOptons.StartingAfter = startingAfterInvoiceId;

                var invoices = invoiceService.List(listOptons);
                return invoices;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get paginated invoices_next for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        public IEnumerable<StripeInvoice> GetInvoices_Last(int limit, string endingBeforeInvoiceId, string customerId = null)
        {
            var invoiceService = new StripeInvoiceService();

            try
            {
                var listOptons = new StripeInvoiceListOptions();
                listOptons.CustomerId = customerId;
                

                listOptons.Limit = limit;

                listOptons.EndingBefore = endingBeforeInvoiceId;

                var invoices = invoiceService.List(listOptons);
                return invoices;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get paginated invoices_last for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                return null;
            }
        }


        public IEnumerable<StripeInvoice> GetInvoices_ByDateRange_Next(int limit, string startingAfterInvoiceId, DateTime startDate, DateTime endDate, string customerId = null)
        {
            var invoiceService = new StripeInvoiceService();

            try
            {
                var listOptons = new StripeInvoiceListOptions();
                listOptons.CustomerId = customerId;

                listOptons.Limit = limit;

                listOptons.Date = new StripeDateFilter
                {
                    GreaterThanOrEqual = startDate,
                    LessThanOrEqual = endDate
                };

                listOptons.StartingAfter = startingAfterInvoiceId;

                var invoices = invoiceService.List(listOptons);
                return invoices;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get invoices by date range for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        public IEnumerable<StripeInvoice> GetInvoices_ByDateRange_Last(int limit, string endingBeforeInvoiceId, DateTime startDate, DateTime endDate, string customerId = null)
        {
            var invoiceService = new StripeInvoiceService();

            try
            {
                var listOptons = new StripeInvoiceListOptions();
                listOptons.CustomerId = customerId;

                listOptons.Limit = limit;
                

                listOptons.Date = new StripeDateFilter
                {
                    GreaterThanOrEqual = startDate,
                    LessThanOrEqual = endDate
                };

                listOptons.EndingBefore = endingBeforeInvoiceId;

                var invoices = invoiceService.List(listOptons);
                return invoices;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get paginated invoices_next by date range for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        #endregion


        #region Retry Unpaid Invoices

        public DataAccessResponseType RetryUnpaidInvoices(string customerId, string accountName, string accountId)
        {
            try
            {
                var invoiceService = new StripeInvoiceService();
                var invoices = GetInvoices(15, customerId);

                //loop through latest invoices and determine if they are unpaid, past_due or pending
                foreach(var invoice in invoices)
                {
                        if (!invoice.Paid && invoice.Closed.HasValue)
                        {
                            if (invoice.Closed.Value == false)
                            {
                                //Pay the invoice
                                invoiceService.Pay(invoice.Id);

                                PlatformLogManager.LogActivity
                                (
                                    CategoryType.Billing,
                                    ActivityType.Billing_OverdueInvoice_Paid,
                                    "Paid overdue invoice for " + accountName,
                                    "Overdue invoice: " + invoice.Id + " for customer: " + customerId + " has been paid",
                                    accountId,
                                    accountName
                                );
                            }
                        }
                        else if (!invoice.Paid && invoice.Closed.HasValue)
                        {
                            if(invoice.Closed.Value == true)
                            {
                                // Log as a manual task (must be handled by Stripe.
                                // In cases where there are multiple 
                                PlatformLogManager.LogActivity
                                (
                                    CategoryType.ManualTask,
                                    ActivityType.ManualTask_Stripe,
                                    "Reopen or forgive closed invoice for " + accountName,
                                    "Log into Stripe to Close or Forgive invoice: " + invoice.Id + " for customer: " + customerId,
                                    accountId,
                                    accountName
                                );

                                /*
                                //Invoice is unpaid and closed, reopen the invoice before attempting payment
                                var invoiceUpdateOptions = new StripeInvoiceUpdateOptions();
                                invoiceUpdateOptions.Closed = false;
                                invoiceUpdateOptions.Forgiven = null;
                                invoiceService.Update(invoice.Id, invoiceUpdateOptions);

                                //Pay the invoice
                                invoiceService.Pay(invoice.Id);
                                 **/
                            }
                    }

                }
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to retry unpaid invoices for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return TransformException(s);
            }

            return new DataAccessResponseType { isSuccess = true };
        }

        #endregion

        /// <summary>
        /// Used when upgrading a subscription so that the pro-rated amount is billed immediatly outside of normal subscription schedules
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        public DataAccessResponseType PayUpcomingInvoice(string customerId)
        {
            var invoiceService = new StripeInvoiceService();

            try
            {
                var upcomingInvoice = invoiceService.Create(customerId);
                //var upcomingInvoice = invoiceService.Upcoming(customerID);
                var chargedIvnoice = invoiceService.Pay(upcomingInvoice.Id);
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to pay upcoming invoices for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                return TransformException(s);
            }

            return new DataAccessResponseType { isSuccess = true };
        }


        public DateTime GetNextBillingDate(string customerId)
        {
            try
            {
                return GetUpcomingInvoice(customerId).PeriodEnd; //.PeriodStart.Value;
                //return GetCustomer(customerID).StripeSubscription.PeriodEnd.Value;
            }
            catch(StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get the next billing date for " + customerId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return DateTime.UtcNow;
            }
        }


        #endregion

        #region Balances

        public StripeBalance GetBalance()
        {
            var balanceService = new StripeBalanceService();

            try
            {
                var balance = balanceService.Get();
                return balance;
            }
            catch (StripeException s)
            {
                TransformException(s);
                return null;
            }
        }

        #endregion

        #region Balance Transactions

        public StripeBalanceTransaction GetBalanceTransaction(string balanceTransactionId)
        {
            var balanceService = new StripeBalanceService();

            try
            {
                var balance = balanceService.Get(balanceTransactionId);
                return balance;
            }
            catch (StripeException s)
            {
                TransformException(s);
                return null;
            }
        }

        public IEnumerable<StripeBalanceTransaction> GetBalanceTransactionsForSource(string sourceId, string startingAfterBalanceTransactionId = null)
        {
            var balanceService = new StripeBalanceService();

            var listOptons = new StripeBalanceTransactionListOptions();
            listOptons.Limit = 100;
            listOptons.SourceId = sourceId;

            if (startingAfterBalanceTransactionId != null)
            {
                listOptons.StartingAfter = startingAfterBalanceTransactionId;
            }

            try
            {
                var balances = balanceService.List(listOptons);
                return balances;
            }
            catch (StripeException s)
            {

                TransformException(s);
                return null;
            }
        }

        public IEnumerable<StripeBalanceTransaction> GetBalanceTransactions(int limit)
        {
            var balanceService = new StripeBalanceService();

            var listOptons = new StripeBalanceTransactionListOptions();
            listOptons.Limit = limit;

            try
            {
                var balances = balanceService.List(listOptons);
                return balances;
            }
            catch (StripeException s)
            {

                TransformException(s);
                return null;
            }
        }

        public IEnumerable<StripeBalanceTransaction> GetBalanceTransactions_CreatedSinceHoursAgo(int sinceHoursAgo, string startingAfterBalanceTransactionId = null)
        {
            var balanceService = new StripeBalanceService();

            var listOptons = new StripeBalanceTransactionListOptions();
            listOptons.Limit = 100;

            if (startingAfterBalanceTransactionId != null)
            {
                listOptons.StartingAfter = startingAfterBalanceTransactionId;
            }

            var stripeDateFilter = new StripeDateFilter();
            stripeDateFilter.GreaterThan = DateTime.UtcNow.AddHours(sinceHoursAgo * -1);

            listOptons.Created = stripeDateFilter;

            try
            {
                var balances = balanceService.List(listOptons);

                return balances;
            }
            catch (StripeException s)
            {

                TransformException(s);
                return null;
            }
        }

        public IEnumerable<StripeBalanceTransaction> GetBalanceTransactions_AvailableSinceHoursAgo(int sinceHoursAgo, string startingAfterBalanceTransactionId = null)
        {
            var balanceService = new StripeBalanceService();

            var listOptons = new StripeBalanceTransactionListOptions();
            listOptons.Limit = 100;

            if (startingAfterBalanceTransactionId != null)
            {
                listOptons.StartingAfter = startingAfterBalanceTransactionId;
            }

            var stripeDateFilter = new StripeDateFilter();
            stripeDateFilter.GreaterThan = DateTime.UtcNow.AddHours(sinceHoursAgo * -1);

            listOptons.AvailableOn = stripeDateFilter;

            try
            {
                var balances = balanceService.List(listOptons);

                return balances;
            }
            catch (StripeException s)
            {

                TransformException(s);
                return null;
            }
        }

        #endregion

        #region Transfers

        public StripeTransfer GetTransfer(string transferId)
        {
            var transferService = new StripeTransferService();

            try
            {
                var transfer = transferService.Get(transferId);
                return transfer;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to retrieve transfer " + transferId + " via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        #region Get Initial Transfers List(s)

        public IEnumerable<StripeTransfer> GetTransfers(int limit)
        {
            var transferService = new StripeTransferService();

            var listOptons = new StripeTransferListOptions();
            listOptons.Limit = limit;

            try
            {
                var transfers = transferService.List(listOptons); ;
                return transfers;
            }
            catch (StripeException s)
            {
                TransformException(s);
                return null;
            }
        }

        public IEnumerable<StripeTransfer> GetTransfers(int limit, DateTime startDate, DateTime endDate)
        {
            var transferService = new StripeTransferService();

            try
            {
                var listOptons = new StripeTransferListOptions();

                listOptons.Limit = limit;

                listOptons.Date = new StripeDateFilter
                {
                    GreaterThanOrEqual = startDate,
                    LessThanOrEqual = endDate
                };

                var transfers = transferService.List(listOptons);
                return transfers;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get initial transfers by date via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        #endregion

        #region Get Paginated Transfers List(s)

        public IEnumerable<StripeTransfer> GetTransfers_Next(int limit, string startingAfterTransferId)
        {
            var transferService = new StripeTransferService();

            try
            {
                var listOptons = new StripeTransferListOptions();

                listOptons.Limit = limit;

                listOptons.StartingAfter = startingAfterTransferId;

                var transfers = transferService.List(listOptons);
                return transfers;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get paginated_next transfers via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        public IEnumerable<StripeTransfer> GetTransfers_Last(int limit, string endingBeforeTransferId)
        {
            var transferService = new StripeTransferService();

            try
            {
                var listOptons = new StripeTransferListOptions();

                listOptons.Limit = limit;

                listOptons.EndingBefore = endingBeforeTransferId;

                var transfers = transferService.List(listOptons);
                return transfers;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get painated_last transfers for via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        public IEnumerable<StripeTransfer> GetTransfers_SinceHours(int sinceHours, string startingAfterTransferId = null)
        {
            var transferService = new StripeTransferService();

            try
            {
                var listOptons = new StripeTransferListOptions();
                listOptons.Limit = 100;

                if (startingAfterTransferId != null)
                {
                    listOptons.StartingAfter = startingAfterTransferId;
                }

                var stripeDateFilter = new StripeDateFilter();
                stripeDateFilter.GreaterThan = DateTime.UtcNow.AddHours(sinceHours * -1);

                listOptons.Created = stripeDateFilter;

                var transfers = transferService.List(listOptons);
                return transfers;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get transfers since a start date for via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        public IEnumerable<StripeTransfer> GetTransfers_ByDateRange_Next(int limit, string startingAfterTransferId, DateTime startDate, DateTime endDate)
        {
            var transferService = new StripeTransferService();

            try
            {
                var listOptons = new StripeTransferListOptions();

                listOptons.Limit = limit;

                listOptons.Date = new StripeDateFilter
                {
                    GreaterThanOrEqual = startDate,
                    LessThanOrEqual = endDate
                };

                listOptons.StartingAfter = startingAfterTransferId;

                var transfers = transferService.List(listOptons);
                return transfers;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get transfers by date range via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        public IEnumerable<StripeTransfer> GetTransfers_ByDateRange_Last(int limit, string endingBeforeTransferId, DateTime startDate, DateTime endDate)
        {
            var transferService = new StripeTransferService();

            try
            {
                var listOptons = new StripeTransferListOptions();
 
                listOptons.Limit = limit;


                listOptons.Date = new StripeDateFilter
                {
                    GreaterThanOrEqual = startDate,
                    LessThanOrEqual = endDate
                };

                listOptons.EndingBefore = endingBeforeTransferId;

                var transfers = transferService.List(listOptons);
                return transfers;
            }
            catch (StripeException s)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    s,
                    "attempting to get paginated transfers_next by date range via Stripe API",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        #endregion

        #endregion

        #region Application Fees

        /*
        public IEnumerable<StripeApplicationFee> GetApplicationFeeHistory()
        {
            var applicationFeeService = new StripeApplicationFeeService();

            var listOptons = new StripeApplicationFeeListOptions();
            //listOptons.Limit = 20;
            //listOptons.ChargeId = "ch_15Vuhd2So09Rq3umbm6wla9R";

            applicationFeeService.ExpandCharge = true;
            applicationFeeService.ExpandBalanceTransaction = true;
            applicationFeeService.ExpandAccount = true;

            try
            {
                var applicationFees = applicationFeeService.List(listOptons);

                var last24Hours = GetCharges_SinceHours(62, null, true);
                var balanceTransactions = GetBalanceTransactions_SinceDaysAgo(3);

                return applicationFees;
            }
            catch (StripeException s)
            {

                TransformException(s);
                return null;
            }
        }
        */
        #endregion

        #region Account

        /* NOT USED
        public StripeAccount GetStripeAccount()
        {
            var stripeAccountService = new StripeAccountService();

            try
            {
                var stripeAccount = stripeAccountService.Get();

                return stripeAccount;
            }
            catch (StripeException s)
            {

                TransformException(s);
                return null;
            }
        }
        */
        #endregion

        #region Helpers

        #region Exception Handling Helpers

        private DataAccessResponseType TransformException(StripeException s)
        {
            //We ignore logging certain exceptions 
            if (   s.StripeError.ErrorType != "card_error" && s.StripeError.Code != "missing"
                || s.StripeError.ErrorType != "card_error" && s.StripeError.Code != "rate_limit"
                || s.StripeError.ErrorType != "card_error" && s.StripeError.Code != "processing_error")
            {
                //Log stripe exception
                PlatformLogManager.LogActivity(
                    CategoryType.Error,
                    ActivityType.Error_StripeException,
                    s.Message,
                    "ErrorType: '" + s.StripeError.ErrorType + "' ErrorCode: '" + s.StripeError.Code + "' ErrorMessage'" + s.StripeError.Message + "'",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    JsonConvert.SerializeObject(s)
                    );
            }


            var response = new DataAccessResponseType();

            response.isSuccess = false;
            response.ErrorMessage = s.Message;
            response.ErrorMessages.Add(s.Message);
            response.ErrorMessages.Add(s.StripeError.Code);
            response.ErrorMessages.Add(s.StripeError.Message);

            return response;

        }

        #endregion

        #endregion
    }
}
