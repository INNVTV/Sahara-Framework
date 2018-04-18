using Newtonsoft.Json;
using PlatformAdminSite.AccountBillingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    public class PaymentsController : Controller
    {
        #region JSON Services


        #region Get Payments/Charges

        [Route("Payments/Json/GetPayment")]
        [HttpGet]
        public JsonNetResult GetPayment(string chargeId)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            Charge charge = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

                string hashKey = "accountbyid:" + accountId.ToString().Replace("-", "");
                string hashField = "creditcard";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    accountCreditCardInfo = JsonConvert.DeserializeObject<AccountCreditCardInfo>(redisValue);
                }
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }*/

            #endregion

            if (charge == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    charge = accountBillingServiceClient.GetPayment(
                            chargeId, Common.SharedClientKey
                        );//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }




                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = charge;

            return jsonNetResult;
        }

        [Route("Payments/Json/GetPaymentHistory")]
        [HttpGet]
        public JsonNetResult GetPaymentHistory(int itemLimit, string accountId)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Charge> Payments = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

                string hashKey = "accountbyid:" + accountId.ToString().Replace("-", "");
                string hashField = "creditcard";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    accountCreditCardInfo = JsonConvert.DeserializeObject<AccountCreditCardInfo>(redisValue);
                }
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }*/

            #endregion

            if (Payments == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    Payments = accountBillingServiceClient.GetPaymentHistory(
                            itemLimit,
                            accountId, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }




                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = Payments;

            return jsonNetResult;
        }

        [Route("Payments/Json/GetPaymentHistory_Next")]
        [HttpGet]
        public JsonNetResult GetPaymentHistory_Next(int itemLimit, string accountId, string startingAfter)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Charge> Payments = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

                string hashKey = "accountbyid:" + accountId.ToString().Replace("-", "");
                string hashField = "creditcard";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    accountCreditCardInfo = JsonConvert.DeserializeObject<AccountCreditCardInfo>(redisValue);
                }
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }*/

            #endregion

            if (Payments == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    Payments = accountBillingServiceClient.GetPaymentHistory_Next(
                            itemLimit,
                            startingAfter,
                            accountId, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = Payments;

            return jsonNetResult;
        }

        [Route("Payments/Json/GetPaymentHistory_Last")]
        [HttpGet]
        public JsonNetResult GetPaymentHistory_Last(int itemLimit, string accountId, string endingBefore)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Charge> Payments = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

                string hashKey = "accountbyid:" + accountId.ToString().Replace("-", "");
                string hashField = "creditcard";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    accountCreditCardInfo = JsonConvert.DeserializeObject<AccountCreditCardInfo>(redisValue);
                }
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }*/

            #endregion

            if (Payments == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    Payments = accountBillingServiceClient.GetPaymentHistory_Last(
                            itemLimit,
                            endingBefore,
                            accountId, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = Payments;

            return jsonNetResult;
        }

        #endregion

        #region refund payments/charges

        [Route("Payments/Json/RefundPayment")]
        [HttpGet]
        public JsonNetResult RefundPayment(string accountId, string chargeId, decimal refundAmount)
        {
            DataAccessResponseType response = null;

            if (string.IsNullOrEmpty(accountId))
            {
                response = new DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot apply refunds on closed/null accounts." };
            }
            else
            {
                var user = AuthenticationCookieManager.GetAuthenticationCookie();


                if (response == null)
                {

                    var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                    try
                    {
                        accountBillingServiceClient.Open();
                        response = accountBillingServiceClient.RefundPayment(
                            accountId,
                            chargeId,
                            refundAmount,
                            AuthenticationCookieManager.GetAuthenticationCookie().Id,
                            PlatformAdminSite.AccountBillingService.RequesterType.PlatformUser, Common.SharedClientKey
                        );//,
                          //user.Id,
                          //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                        //Close the connection
                        WCFManager.CloseConnection(accountBillingServiceClient);
                    }
                    catch (Exception e)
                    {
                        #region Manage Exception

                        string exceptionMessage = e.Message.ToString();

                        var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                        string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                        // Abort the connection & manage the exception
                        WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                        #endregion
                    }

                }
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #region Dunning

        [Route("Payments/Json/GetDunningAttempts")]
        [HttpGet]
        public JsonNetResult GetDunningAttempts(string accountId)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<DunningAttempt> DunningAttempts = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

                string hashKey = "accountbyid:" + accountId.ToString().Replace("-", "");
                string hashField = "creditcard";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    accountCreditCardInfo = JsonConvert.DeserializeObject<AccountCreditCardInfo>(redisValue);
                }
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }*/

            #endregion

            if (DunningAttempts == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    DunningAttempts = accountBillingServiceClient.GetDunningAttempts(
                            accountId, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }




                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = DunningAttempts;

            return jsonNetResult;
        }

        #endregion

        #endregion
    }
}