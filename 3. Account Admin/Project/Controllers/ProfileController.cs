using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AccountAdminSite.AccountManagementService;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.ServiceModel;

namespace AccountAdminSite.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult Index()
        {
            return View();
        }

        #region JSON Feeds

        #region Get (Shared globally)

        /// <summary>
        /// Gets the profile of the currently logged in user. Used by Angular Views to display role based proeprties/abilities and up to date user properties. Should sync to WCF Services everytime a new angular view is rendered, or once every 1-2 minutes.
        /// </summary>
        /// <returns></returns>
        [Route("Profile/Json/GetCurrentUser")]
        [HttpGet]
        public JsonNetResult GetCurrentUser()
        {
            //This is used often by the UI to refresh the user, wrap in a Try/Catch in case of intermitent failures:

            try
            {
                var userId = AuthenticationCookieManager.GetAuthenticationCookie().Id;

                if(userId == null)
                {
                    Response.Redirect("/Login");
                }

                AccountUser user = null;

                #region (Plan A) Get data from Redis Cache

                try
                {
                    //First we attempt to get the user from the Redis Cache

                    IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                    string hashKey = "user:" + userId.Replace("-", "");
                    string hashField = "model";

                    var redisValue = cache.HashGet(hashKey, hashField);

                    //con.Close();

                    if (redisValue.HasValue)
                    {
                        user = JsonConvert.DeserializeObject<AccountUser>(redisValue);
                    }

                }
                catch (Exception e)
                {
                    var error = e.Message;
                }

                #endregion

                if (user == null)
                {
                    #region (Plan B) Get data from WCF

                    //If a failure occurs, or the redis cache is empty we get the user from the WCF service
                    var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                    try
                    {
                        accountManagementServiceClient.Open();
                        user = accountManagementServiceClient.GetAccountUser(userId, Common.SharedClientKey);

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
                }

                //Update the local cookies:
                AuthenticationCookieManager.SaveAuthenticationCookie(user);

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = user;

                return jsonNetResult;
            }
            catch
            {
                // Use cookies instead
                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                var accountUser = AuthenticationCookieManager.GetAuthenticationCookie();
                jsonNetResult.Data = accountUser;

                return jsonNetResult;
            }
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets the profile of the currently logged in user. Used by Angular Views to display role based proeprties/abilities and up to date user properties. Should sync to WCF Services everytime a new angular view is rendered, or once every 1-2 minutes.
        /// </summary>
        /// <returns></returns>
        [Route("Profile/Json/GetOtherAccounts")]
        [HttpGet]
        public JsonNetResult GetOtherAccounts()
        {
            List<UserAccount> response = null;

            try
            {
                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    response = accountManagementServiceClient.GetAllAccountsForEmail(AuthenticationCookieManager.GetAuthenticationCookie().Email, Common.SharedClientKey).ToList();

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

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = response;

                return jsonNetResult;
            }
            catch
            {
                // Use cookies instead
                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = AuthenticationCookieManager.GetAuthenticationCookie();

                return jsonNetResult;
            }
        }

        #endregion

        #region Updates

        [Route("Profile/Json/UpdateName")]
        [HttpPost]
        public JsonNetResult UpdateName(string firstName, string lastName)
        {
            var response = new DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();

                var user = AuthenticationCookieManager.GetAuthenticationCookie();

                response = accountManagementServiceClient.UpdateAccountUserFullName(
                    user.AccountID.ToString(),
                    user.Id,
                    firstName,
                    lastName,
                    user.Id,
                    RequesterType.Exempt, Common.SharedClientKey);

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


        [Route("Profile/Json/UpdateEmail")]
        [HttpPost]
        public JsonNetResult UpdateEmail(string email)
        {
            var response = new DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();

                var user = AuthenticationCookieManager.GetAuthenticationCookie();

                response = accountManagementServiceClient.UpdateAccountUserEmail(
                    user.AccountID.ToString(),
                    user.Id,
                    email,
                    user.Id,
                    RequesterType.Exempt, Common.SharedClientKey);

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


        [Route("Profile/Json/UpdatePassword")]
        [HttpPost]
        public JsonNetResult UpdatePassword(string currentPassword, string newPassword)
        {
            var response = new DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                var user = AuthenticationCookieManager.GetAuthenticationCookie();

                response = accountManagementServiceClient.UpdateAccountUserPassword(
                    user.AccountID.ToString(),
                    user.Email,
                    currentPassword,
                    newPassword,
                    user.Id,
                    RequesterType.Exempt, Common.SharedClientKey);

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

        #region Imaging


        [Route("Profile/Json/UploadPhoto")]
        [HttpPost]
        public JsonNetResult UploadPhoto() //  HttpPostedFileBase file)     //  Object file)
        {
            var length = Request.ContentLength;
            var bytes = new byte[length];

            Request.InputStream.Read(bytes, 0, length);

            var response = new DataAccessResponseType();
            var platformManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            JsonNetResult jsonNetResult = new JsonNetResult();

            try
            {
                platformManagementServiceClient.Open();

                var user = AuthenticationCookieManager.GetAuthenticationCookie();

                response = platformManagementServiceClient.UpdateAccountUserProfilePhoto(
                    user.AccountID.ToString(),
                    user.Id,
                    bytes,
                    user.Id,
                    AccountAdminSite.AccountManagementService.RequesterType.Exempt, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(platformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformManagementServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #endregion
    }
}