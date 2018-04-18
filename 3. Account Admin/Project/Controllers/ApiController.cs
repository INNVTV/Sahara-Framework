using AccountAdminSite.ApplicationApiKeysService;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    public class ApiController : Controller
    {

        #region View Controllers

        // GET: Api
        public ActionResult Index()
        {
            return View();
        }

        #endregion




        #region JSON Services

        [Route("Api/Json/GetApiKeys")]
        public JsonNetResult GetAccount()
        {
            //Account account = null;
            List<ApiKeyModel> keys = null;

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get the apikeys from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":apikeys";
                string hashField = "list";

                try
                {
                    var redisValue = cache.HashGet(hashKey, hashField);

                    if (redisValue.HasValue)
                    {
                        keys = JsonConvert.DeserializeObject<List<ApiKeyModel>>(redisValue);
                    }
                }
                catch
                {

                }


            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (keys == null)
            {
                #region (Plan B) Get data from WCF

                var applicationApiKeysServiceClient = new ApplicationApiKeysService.ApplicationApiKeysServiceClient();

                try
                {
                    applicationApiKeysServiceClient.Open();
                    keys = applicationApiKeysServiceClient.GetApiKeys(accountNameKey, Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(applicationApiKeysServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationApiKeysServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = keys;

            return jsonNetResult;
        }

        [Route("Api/Json/GenerateApiKey")]
        [HttpPost]
        public JsonNetResult GenerateApiKey(string name, string description)
        {
            string response = null;
            var applicationApiKeysServiceClient = new ApplicationApiKeysService.ApplicationApiKeysServiceClient();

            try
            {
                var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
                var requesterId = authCookie.Id;
                var accountNameKey = authCookie.AccountNameKey;

                applicationApiKeysServiceClient.Open();
                response = applicationApiKeysServiceClient.GenerateApiKey(accountNameKey, name, description,
                    requesterId, ApplicationApiKeysService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationApiKeysServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationApiKeysServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                //response.isSuccess = false;
                //response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Api/Json/RegenerateApiKey")]
        [HttpPost]
        public JsonNetResult RegenerateApiKey(string apiKey)
        {
            string response = null;
            var applicationApiKeysServiceClient = new ApplicationApiKeysService.ApplicationApiKeysServiceClient();

            try
            {
                var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
                var requesterId = authCookie.Id;
                var accountNameKey = authCookie.AccountNameKey;

                applicationApiKeysServiceClient.Open();
                response = applicationApiKeysServiceClient.RegenenerateApiKey(accountNameKey, apiKey,
                    requesterId, ApplicationApiKeysService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationApiKeysServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationApiKeysServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                //response.isSuccess = false;
                //response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Api/Json/DeleteApiKey")]
        [HttpPost]
        public JsonNetResult DeleteApiKey(string apiKey)
        {
            var response = new DataAccessResponseType();
            var applicationApiKeysServiceClient = new ApplicationApiKeysService.ApplicationApiKeysServiceClient();

            try
            {
                var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
                var requesterId = authCookie.Id;
                var accountNameKey = authCookie.AccountNameKey;

                applicationApiKeysServiceClient.Open();
                response = applicationApiKeysServiceClient.DeleteApiKey(accountNameKey, apiKey,
                    requesterId, ApplicationApiKeysService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationApiKeysServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationApiKeysServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                //response.isSuccess = false;
                //response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
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
    }
}