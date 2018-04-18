using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {

        #region View Controllers

        // GET: /Sales/
        public ActionResult Index()
        {
            return View();
        }


        // Used for Detail variation
        // GET: /Sales/{id}
        [Route("Sales/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }

        #endregion


        #region JSON Services

        #region Initializaton

        [Route("Sales/Json/Get")]
        [HttpGet]
        public JsonNetResult GetSales()
        {
            /*

            Account account = null;
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get the account from the Redis Cache

                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "accountbyid:" + accountId.Replace("-", "");
                string hashField = "model";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    account = JsonConvert.DeserializeObject<Account>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (account == null)
            {
                #region (Plan B) Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    account = accountManagementServiceClient.GetAccount(accountId);

                    //Close the connection
                    WCFManager.CloseConnection(accountManagementServiceClient);

                }
                catch(Exception e)
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
            }
             
            */

            var results = new string[] {"one", "two", "three", "four"};

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = results;

            return jsonNetResult;

        }

        #endregion


        #region Details


        [Route("Sales/Json/Details/{id}")]
        public JsonNetResult Detail(string id)
        {
            /*
             *
            Account account = null;
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get the account from the Redis Cache

                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "accountbyid:" + accountId.Replace("-", "");
                string hashField = "model";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    account = JsonConvert.DeserializeObject<Account>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (account == null)
            {
                #region (Plan B) Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    account = accountManagementServiceClient.GetAccount(accountId);

                    //Close the connection
                    WCFManager.CloseConnection(accountManagementServiceClient);

                }
                catch(Exception e)
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
            }
             
            */

            var results = new string[] {"details 1", "details 2"};

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = results;

            return jsonNetResult;
        }

        #endregion


        #region Updates

        [Route("Accounts/Json/UpdateSomething")]
        [HttpPost]
        public JsonNetResult UpdateSomething(string id, string name)
        {
            /*

            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.DeleteAccountUser(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), userId,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.AccountUser);

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
            
             */

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            //jsonNetResult.Data = "DataAccessResponseType";

            return jsonNetResult;
        }

        #endregion


        #endregion



    }
}