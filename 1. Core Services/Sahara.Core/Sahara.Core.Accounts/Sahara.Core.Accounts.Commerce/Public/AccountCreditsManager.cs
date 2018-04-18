using Newtonsoft.Json;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Common.Redis.PlatformManagerServer.Hashes;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.Stripe;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Commerce.Public
{
    public static class AccountCreditsManager
    {

        #region Get Credits

        public static int GetCredits(string accountId, bool useCachedVersion = true)
        {

            string cachedCreditsAvailable = String.Empty;

            if(useCachedVersion)
            {
                try
                {
                    //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                    IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
                    Object redisValue = null;

                    redisValue = cache.HashGet(
                                        AccountCreditsHash.Key(),
                                        AccountCreditsHash.Fields.CreditsAvailable(accountId)
                                    );

                    if (((RedisValue)redisValue).HasValue)
                    {
                        cachedCreditsAvailable = JsonConvert.DeserializeObject<string>((RedisValue)redisValue);

                        
                    }
                }
                catch
                {
                    cachedCreditsAvailable = String.Empty;
                }

            }

            if(String.IsNullOrEmpty(cachedCreditsAvailable))
            {
                //Nothing in cache, pull from DB:
                int creditsAvailable = Sql.Statements.SelectStatements.SelectCredits(accountId);

                //Store results into cache
                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    cache.HashSet(
                        AccountCreditsHash.Key(),
                        AccountCreditsHash.Fields.CreditsAvailable(accountId),
                        JsonConvert.SerializeObject(creditsAvailable),
                        When.Always,
                        CommandFlags.FireAndForget);
                }
                catch
                {

                }

                //Return Results
                return creditsAvailable;
            }
            else
            {
                //Convert & return cached results
                return Convert.ToInt32(cachedCreditsAvailable);
            }

        }

        public static int GetCreditsInCirculation(bool useCachedVersion = true)
        {

            string cachedCreditsAvailable = String.Empty;

            if (useCachedVersion)
            {
                try
                {
                    //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
                    IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
                    Object redisValue = null;

                    redisValue = cache.HashGet(
                                        CreditsHash.Key,
                                        CreditsHash.Fields.TotalInCirculation
                                    );

                    if (((RedisValue)redisValue).HasValue)
                    {
                        cachedCreditsAvailable = JsonConvert.DeserializeObject<string>((RedisValue)redisValue);


                    }
                }
                catch
                {
                    cachedCreditsAvailable = String.Empty;
                }

            }

            if (String.IsNullOrEmpty(cachedCreditsAvailable))
            {
                //Nothing in cache, pull from DB:
                int creditsAvailable = Sql.Statements.SelectStatements.SelectCreditsInCirculation();

                //Store results into cache and expire
                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    cache.HashSet(
                        CreditsHash.Key,
                        CreditsHash.Fields.TotalInCirculation,
                        JsonConvert.SerializeObject(creditsAvailable),
                        When.Always,
                        CommandFlags.FireAndForget);

                    cache.KeyExpire(
                        CreditsHash.Key,
                        CreditsHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                //Return Results
                return creditsAvailable;
            }
            else
            {
                //Convert & return cached results
                return Convert.ToInt32(cachedCreditsAvailable);
            }

        }

        #endregion

        #region Buy Credits

        /// <summary>
        /// Purchase credits with existing account card
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="dollarAmount"></param>
        /// <returns></returns>
        public static DataAccessResponseType BuyCredits(string accountId, decimal dollarAmount)
        {
            #region Validate Input

            if(!Common.Methods.Billing.IsDecimalInTwoPlaces(dollarAmount))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Amount must be a monetary value with 2 decimal places. Examples: '2.25' or '.50'" };
            }

            if (dollarAmount <= 0)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Amount must be greater than 0." };
            }

            #endregion

            //Does the account have a credit card on file?
            if(AccountManager.GetAccountCreditCardInfo(accountId) == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "No credit card on file" };
            }

            return ExecuteCreditsTransaction(accountId, dollarAmount);

        }

        /// <summary>
        /// Purchase credits while adding a new card to the account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="dollarAmount"></param>
        /// <param name="creditCard"></param>
        /// <returns></returns>
        /*
        public static DataAccessResponseType BuyCredits(string accountId, int dollarAmount, NewCreditCard creditCard)
        {
            //Add/Update Card to the account, get back StripeCustomerID
            var addUpdateNewCreditCardResponse = AccountManager.AddUpdateCreditCard();

            if(addUpdateNewCreditCardResponse.isSuccess)
            {
                //if successful, make the transaction
                return ExecuteCreditsTransaction(accountId, dollarAmount);
            }
            else
            {
                return addUpdateNewCreditCardResponse;
            }

        }*/

        #region Shared Private Methods

        private static DataAccessResponseType ExecuteCreditsTransaction(string accountId, decimal dollarAmount)
        {
            //Convert dollarAmount to credits
            var creditsToAdd = Common.Methods.Commerce.ConvertDollarAmountToCredits(dollarAmount);
            var dollarAmountStripeInteger = Sahara.Core.Common.Methods.Billing.ConvertDecimalToStripeAmount(dollarAmount);

            var account = AccountManager.GetAccount(accountId);

            //Make sure account has a StripeCustomerId
            if(account.StripeCustomerID == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Accont does not have a customerId for Stripe." };
            }

            //Create invoice and charge the account;
            var stripeManager = new StripeManager();

            var stripeCustomerId = account.StripeCustomerID;
            var chargeDescription = "Purchase of " + creditsToAdd + " credits";

            var chargeAccountResponse = stripeManager.ChargeAccount(account.StripeCustomerID, chargeDescription, dollarAmountStripeInteger);

            if (!chargeAccountResponse.isSuccess)
            {
                //If unsuccesful, return the result:
                return chargeAccountResponse;
            }

            //If successful store into SQL
            bool sqlResult = Sql.Statements.UpdateStatements.UpdateAccountCredits(accountId, creditsToAdd);

            if(!sqlResult)
            {
                //Log issue so that credits can be added manually
            }

            //Log transaction into Azure Tables for the account
            //Partitions: PurchasedCredits, SpentCredits
            //TODO

            //Refresh cache:
            RefreshCreditsCache(accountId);

            return new DataAccessResponseType { isSuccess = true };
        }

        #endregion

        #endregion

        #region Spend Credits

        public static DataAccessResponseType SpendCredits(string accountId, int creditsToSpend, string description)
        {
            #region Validate

            //is amount negative?
            if (creditsToSpend < 0)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot spend less than 0 credits" };
            }

            //is amount 0?
            if (creditsToSpend == 0)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot spend 0 credits" };
            }

            //Do enough credits exist?
            int existingCredits = Sql.Statements.SelectStatements.SelectCredits(accountId);
            if(existingCredits < creditsToSpend)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not enough credits available." };
            }


            #endregion

            //convert to a negative number
            var creditsToSpendNegative = creditsToSpend * -1;

            //Update SQL
            Sql.Statements.UpdateStatements.UpdateAccountCredits(accountId, creditsToSpendNegative);

            //Refresh cache:
            RefreshCreditsCache(accountId);

            return new DataAccessResponseType { isSuccess = true };
        }

        #endregion

        #region Trade Credits

        public static DataAccessResponseType TradeCredits(string fromAccountId, string toAccountId, int creditsToExchange, string description, out Account receiverAccount)
        {
            #region Validate

            //is amount negative?
            if (creditsToExchange < 0)
            {
                receiverAccount = null;
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot exchange less than 0 credits" };
            }

            //is amount 0?
            if (creditsToExchange == 0)
            {
                receiverAccount = null;
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot exchange 0 credits" };
            }

            //Do enough credits exist?
            int existingCredits = Sql.Statements.SelectStatements.SelectCredits(fromAccountId);
            if (existingCredits < creditsToExchange)
            {
                receiverAccount = null;
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not enough credits available." };
            }

            //Is the account you are trading with valid?
            receiverAccount = AccountManager.GetAccount(toAccountId);
            if (receiverAccount == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "The account you are trading with does not exist." };
            }

            #endregion

            //convert to a negative number
            var creditsToExchangeNegative = creditsToExchange * -1;

            try
            {
                //Update SQL w/ negative value on giver (Remove credits from giver)
                Sql.Statements.UpdateStatements.UpdateAccountCredits(fromAccountId, creditsToExchangeNegative);

                try
                {
                    //Update SQL w/ postive value on reciever (Give credits to receiver)
                    Sql.Statements.UpdateStatements.UpdateAccountCredits(toAccountId, creditsToExchange);
                }
                catch(Exception e)
                {
                    try
                    {
                        //Rollback givers credits w/ positive value if exchange fails at this point
                        Sql.Statements.UpdateStatements.UpdateAccountCredits(fromAccountId, creditsToExchange);
                    }
                    catch(Exception ee)
                    {
                        //If rollback fails, we record the exception and alert admins so it can be manually adjusted
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                                ee,
                                "attempting to rollback '" + creditsToExchange + "' credits to giver during exchange after error updating value on the receivers account: " + toAccountId,
                                System.Reflection.MethodBase.GetCurrentMethod(),
                                fromAccountId,
                                null       
                            );

                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = ee.Message };
                    }

                    //If exchage fails and a rollback occurred, we record the exception and alert admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to complete credit exchange by updating value '" + creditsToExchange + "' on the receivers account: " + toAccountId + ". Credit rollback to giver succeeded.",
                            System.Reflection.MethodBase.GetCurrentMethod(),
                            fromAccountId,
                            null
                        );

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
                }
            }
            catch(Exception e)
            {
                //If exchage fails we record the exception and alert admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to start credit exchange between accounts " + fromAccountId + " and " + toAccountId + " for " + creditsToExchange + " credits",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        fromAccountId,
                        null
                    );

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }


            //Refresh caches for BOTH accounts:
            RefreshCreditsCache(fromAccountId);
            RefreshCreditsCache(toAccountId);

            return new DataAccessResponseType { isSuccess = true };
        }

        #endregion



        private static void RefreshCreditsCache(string accountId)
        {
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            //Clear:
            //cache.HashDelete(
            //AccountCreditsHash.Key(),
            //AccountCreditsHash.Fields.CreditsAvailable(accountId),
            //CommandFlags.FireAndForget
            //);

            //Update with non cached version:
            GetCredits(accountId, false);
        }
    }
}
