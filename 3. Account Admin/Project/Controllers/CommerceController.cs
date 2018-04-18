using AccountAdminSite.AccountCommerceService;
using AccountAdminSite.AccountManagementService;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    public class CommerceController : Controller
    {
        // GET: Commerce
        //public ActionResult Index()
        //{
            //return View();
        //}




        #region Credits

        #region JSON Services

        #region GET

        [Route("Commerce/Json/GetCreditsToDollarExchangeValue")]
        [HttpGet]
        public JsonNetResult GetCreditsToDollarExchangeValue()
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID;
            string creditsToDollarExchangeValueString = String.Empty;

            #region (Plan A) Get data from Local Cache (We cache the exchange rate locally for 8 hours to decrease Core Service requests)

            var exchangeRateCacheKey = "CreditsToDollarExchangeValue";
            var cacheMinutes = 480;

            var localCacheValue = System.Web.HttpRuntime.Cache.Get(exchangeRateCacheKey);

            #endregion


            if (localCacheValue == null)
            {
                //local cache is empty, get rate from Core Services
                #region (Plan B) Get data from WCF

                //If a failure occurs, or the redis cache is empty we get the user from the WCF service
                var accountCommerceServiceClient = new AccountCommerceService.AccountCommerceServiceClient();

                try
                {
                    accountCommerceServiceClient.Open();
                    creditsToDollarExchangeValueString = accountCommerceServiceClient.GetDollarsToCreditsExchangeRate(1, Common.SharedClientKey).ToString();

                    //Close the connection
                    WCFManager.CloseConnection(accountCommerceServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountCommerceServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                //Store data into local cache for 8 hours:
                System.Web.HttpRuntime.Cache.Insert(exchangeRateCacheKey, creditsToDollarExchangeValueString, null, DateTime.Now.AddMinutes(cacheMinutes), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);

                #endregion
            }
            else
            {
                //convert local cache object to return value
                creditsToDollarExchangeValueString = localCacheValue.ToString();
            }


            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsToDollarExchangeValueString;

            return jsonNetResult;

        }

        [Route("Commerce/Json/GetAvailableCredits")]
        [HttpGet]
        public JsonNetResult GetAvailableCredits()
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID;
            string creditsAvailableString = String.Empty;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get the user from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "credits";
                string hashField = "credits:available:" + accountId;

                var redisValue = cache.HashGet(hashKey, hashField);

                //con.Close();

                if (redisValue.HasValue)
                {
                    creditsAvailableString = JsonConvert.DeserializeObject<String>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
            }

            #endregion

            if (String.IsNullOrEmpty(creditsAvailableString))
            {
                #region (Plan B) Get data from WCF

                //If a failure occurs, or the redis cache is empty we get the user from the WCF service
                var accountCommerceServiceClient = new AccountCommerceService.AccountCommerceServiceClient();

                try
                {
                    accountCommerceServiceClient.Open();
                    creditsAvailableString = accountCommerceServiceClient.GetCredits(accountId.ToString(), Common.SharedClientKey).ToString();

                    //Close the connection
                    WCFManager.CloseConnection(accountCommerceServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountCommerceServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsAvailableString;

            return jsonNetResult;

        }

        /*
        [Route("Commerce/Json/GetCreditsTransactionLog")]
        [HttpGet]
        public JsonNetResult GetCreditsTransactionLog()
        {           
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID;

            #region Get data from WCF

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLogByCategory(accountId.ToString(), AccountManagementService.CategoryType.Credits, 120).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion
            
            
            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
         * jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }
         */

        #endregion

        #region POST

        [Route("Commerce/Json/BuyCredits")]
        [HttpPost]
        public JsonNetResult BuyCredits(decimal dollarAmount)
        {
            var accountUser = AuthenticationCookieManager.GetAuthenticationCookie();
            var response = new AccountAdminSite.AccountCommerceService.DataAccessResponseType();

            var accountCommerceServiceClient = new AccountCommerceService.AccountCommerceServiceClient();

            try
            {
                //Get the ip address and call origin
                var ipAddress = Request.UserHostAddress;
                var origin = "Web";

                accountCommerceServiceClient.Open();
                response = accountCommerceServiceClient.BuyCredits(
                    accountUser.AccountID.ToString(),
                    dollarAmount,
                    accountUser.Id,
                    AccountAdminSite.AccountCommerceService.RequesterType.AccountUser,
                    ipAddress,
                    origin, Common.SharedClientKey
                    );

                //Close the connection
                WCFManager.CloseConnection(accountCommerceServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountCommerceServiceClient, exceptionMessage, currentMethodString);

                    #endregion
            }



            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        [Route("Commerce/Json/SpendCredits")]
        [HttpPost]
        public JsonNetResult SpendCredits(int creditsAmount, string description)
        {
            var accountUser = AuthenticationCookieManager.GetAuthenticationCookie();
            var response = new AccountAdminSite.AccountCommerceService.DataAccessResponseType();

            var accountCommerceServiceClient = new AccountCommerceService.AccountCommerceServiceClient();

            try
            {
                //Get the ip address and call origin
                var ipAddress = Request.UserHostAddress;
                var origin = "Web";

                accountCommerceServiceClient.Open();
                response = accountCommerceServiceClient.SpendCredits(
                    accountUser.AccountID.ToString(),
                    creditsAmount,
                    description,
                    accountUser.Id,
                    AccountAdminSite.AccountCommerceService.RequesterType.AccountUser,
                    ipAddress,
                    origin, Common.SharedClientKey
                    );

                //Close the connection
                WCFManager.CloseConnection(accountCommerceServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountCommerceServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }



            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        [Route("Commerce/Json/TradeCredits")]
        [HttpPost]
        public JsonNetResult TradeCredits(string toAccountId, int creditsAmount, string description)
        {
            var accountUser = AuthenticationCookieManager.GetAuthenticationCookie();
            var response = new AccountAdminSite.AccountCommerceService.DataAccessResponseType();

            var accountCommerceServiceClient = new AccountCommerceService.AccountCommerceServiceClient();

            try
            {
                //Get the ip address and call origin
                var ipAddress = Request.UserHostAddress;
                var origin = "Web";

                accountCommerceServiceClient.Open();
                response = accountCommerceServiceClient.TradeCredits(
                    accountUser.AccountID.ToString(),
                    toAccountId,
                    creditsAmount,
                    description,
                    accountUser.Id,
                    AccountAdminSite.AccountCommerceService.RequesterType.AccountUser,
                    ipAddress,
                    origin, Common.SharedClientKey
                    );

                //Close the connection
                WCFManager.CloseConnection(accountCommerceServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountCommerceServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }



            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        #endregion

        #endregion

        #endregion
    }        
        
}