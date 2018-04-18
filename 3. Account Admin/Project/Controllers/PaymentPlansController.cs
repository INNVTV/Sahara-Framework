using AccountAdminSite.AccountManagementService;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    public class PaymentPlansController : Controller
    {
        // GET: PaymentPlan
        public ActionResult Index()
        {
            return View();
        }


        #region JSON Feeds

        #region Get

        [Route("PaymentPlans/Json/GetPlans")]
        [HttpGet]
        public JsonNetResult GetPlans()
        {

            List<AccountPaymentPlanService.PaymentPlan> paymentPlans = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "paymentplans";
                string hashField = "list:False:True";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    paymentPlans = JsonConvert.DeserializeObject<List<AccountPaymentPlanService.PaymentPlan>>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (paymentPlans == null)
            {
                #region (Plan B) Get data from WCF

                var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

                try
                {
                    accountPaymentPlanServiceClient.Open();
                    paymentPlans = accountPaymentPlanServiceClient.GetPaymentPlans(false, true).ToList();

                    WCFManager.CloseConnection(accountPaymentPlanServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountPaymentPlanServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = paymentPlans;

            

            return jsonNetResult;
        }

        [Route("PaymentPlans/Json/GetPlan/")]
        [HttpGet]
        public JsonNetResult GetPlan(string planName)
        {
            AccountPaymentPlanService.PaymentPlan paymentPlan = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "paymentplans";
                string hashField = "plan:" + planName;

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    paymentPlan = JsonConvert.DeserializeObject<AccountPaymentPlanService.PaymentPlan>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (paymentPlan == null)
            {
                #region (Plan B) Get data from WCF

                var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

                try
                {
                    accountPaymentPlanServiceClient.Open();
                    paymentPlan = accountPaymentPlanServiceClient.GetPaymentPlan(planName);

                    //Close the connection
                    WCFManager.CloseConnection(accountPaymentPlanServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountPaymentPlanServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = paymentPlan;

            return jsonNetResult;
        }

        [Route("PaymentPlans/Json/GetFrequencies")]
        [HttpGet]
        public JsonNetResult GetFrequencies()
        {

            List<AccountPaymentPlanService.PaymentFrequency> paymentFrequencies = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "paymentplans";
                string hashField = "freq:list";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    paymentFrequencies = JsonConvert.DeserializeObject<List<AccountPaymentPlanService.PaymentFrequency>>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion


            if (paymentFrequencies == null)
            {
                #region (Plan B) Get data from WCF

                var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

                try
                {
                    accountPaymentPlanServiceClient.Open();

                    paymentFrequencies = accountPaymentPlanServiceClient.GetPaymentFrequencies().ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountPaymentPlanServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountPaymentPlanServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion

            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = paymentFrequencies;

            return jsonNetResult;

        }


        #endregion

        #region Subscribe

        [Route("PaymentPlans/Json/Subscribe")]
        [HttpPost]
        public JsonNetResult Subscribe(string planName, string frequencyMonths, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                //Get the ip address and call origin
                var ipAddress = Request.UserHostAddress;
                var origin = "Web";

                accountManagementServiceClient.Open();

                response = accountManagementServiceClient.CreateSubscripton(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                    planName,
                    frequencyMonths,
                    cardName,
                    cardNumber,
                    cvc,
                    expirationMonth,
                    expirationYear,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.AccountUser,
                    ipAddress,
                    origin, Common.SharedClientKey);

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

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        /// <summary>
        /// ONLY USED FOR SUBSCRIBING ACCOUNTS
        /// </summary>
        /// <returns></returns>
        [OverrideAuthorization] //<--Allow unauthorized access
        [Route("PaymentPlans/Json/SubscribeFromSub")]
        [HttpPost]
        public JsonNetResult SubscribeFromSub(string accountId, string planName, string frequencyMonths, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                //Get the ip address and call origin
                var ipAddress = Request.UserHostAddress;
                var origin = "Web";

                accountManagementServiceClient.Open();

                response = accountManagementServiceClient.CreateSubscripton(
                    accountId,
                    planName,
                    frequencyMonths,
                    cardName,
                    cardNumber,
                    cvc,
                    expirationMonth,
                    expirationYear,
                    null, //<-- Exempt
                    AccountManagementService.RequesterType.Exempt, //<-- Exempt
                    ipAddress,
                    origin, Common.SharedClientKey);

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

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        #endregion

        #region Update

        [Route("PaymentPlans/Json/UpdatePlan")]
        [HttpPost]
        public JsonNetResult UpdatePlan(string planName, string frequencyMonths)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                //Get the ip address and call origin
                var ipAddress = Request.UserHostAddress;
                var origin = "Web";

                accountManagementServiceClient.Open();

                response = accountManagementServiceClient.UpdateAccountPlan(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                    planName,
                    frequencyMonths,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.AccountUser,
                    ipAddress,
                    origin, Common.SharedClientKey);

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

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

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
    }
}