using AccountAdminSite.AccountBillingService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    public class BillingController : Controller
    {

        #region JSON Services


        #region Invoices

        [Route("Billing/Json/GetNextInvoice")]
        [HttpGet]
        public JsonNetResult GetNextInvoice()
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            //var user = AuthenticationCookieManager.GetAuthenticationCookie();
            Invoice invoice = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

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

            if (invoice == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    invoice = accountBillingServiceClient.GetUpcomingInvoice(
                        accountId, Common.SharedClientKey);//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser);

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
            jsonNetResult.Data = invoice;

            return jsonNetResult;
        }
        #endregion


        #region Get Payments/Charges

        [Route("Billing/Json/GetPayment")]
        [HttpGet]
        public JsonNetResult GetPayment(string chargeId)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            Charge charge = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

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

        [Route("Billing/Json/GetPaymentHistory")]
        [HttpGet]
        public JsonNetResult GetPaymentHistory(int itemLimit)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            List<Charge> Payments = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

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

        [Route("Billing/Json/GetPaymentHistory_Next")]
        [HttpGet]
        public JsonNetResult GetPaymentHistory_Next(int itemLimit, string startingAfter)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            List<Charge> Payments = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

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

        [Route("Billing/Json/GetPaymentHistory_Last")]
        [HttpGet]
        public JsonNetResult GetPaymentHistory_Last(int itemLimit, string endingBefore)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            List<Charge> Payments = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

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

        #region Dunning

        [Route("Billing/Json/GetDunningAttempts")]
        [HttpGet]
        public JsonNetResult GetDunningAttempts()
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            List<DunningAttempt> DunningAttempts = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

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