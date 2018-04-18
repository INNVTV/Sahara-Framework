using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AccountAdminSite.AccountManagementService;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.ServiceModel;
using System.Web.Caching;
using AccountAdminSite.ApplicationImageRecordsService;

namespace AccountAdminSite.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        #region View Controllers

        // GET: /Settings/
        public ActionResult Index()
        {
            return View();
        }


        // Used for Detail variation
        // GET: /Settings/{id}
        [Route("Account/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }

        #endregion

        #region JSON Services

        #region Account

        [Route("Account/Json/GetAccount")]
        public JsonNetResult GetAccount()
        {
            Account account = null;

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get the account from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "accountbyname:" + accountNameKey;
                string hashField = "model";

                try
                {
                    var redisValue = cache.HashGet(hashKey, hashField);

                    if (redisValue.HasValue)
                    {
                        account = JsonConvert.DeserializeObject<Account>(redisValue);
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

            if (account == null)
            {
                #region (Plan B) Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    account = accountManagementServiceClient.GetAccount(accountNameKey, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = account;

            return jsonNetResult;
        }


        /// <summary>
        /// ONLY USED FOR SUBSCRIBING ACCOUNTS
        /// </summary>
        /// <returns></returns>
        [OverrideAuthorization] //<--Allow unauthorized access
        [Route("Account/Json/GetAccountByIdForSub")]
        public JsonNetResult GetAccountByIdForSub(string accountId)
        {
            Account account = null;



            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                account = accountManagementServiceClient.GetAccount(accountId, Common.SharedClientKey);

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

                return null;
            }

            try
            {
                if (account.Active == true || account.Provisioned == true)
                {
                    //For security we ONLY return accounts that CAN BE subscribed to a plan and nothing active
                    return null;
                }
            }
            catch
            {
                return null;
            }


            //Strip account of important info
            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = account;

            return jsonNetResult;
        }

        [Route("Account/Json/GetAccountImages")]
        public JsonNetResult GetAccountImages()
        {
            Account account = null;

            List<ImageRecordGroupModel> accountImages = null;

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            #region (Plan A) Get AccountImages From Redis Cache

            IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();
            string redisKey = RedisCacheKeys.AccountImages.Key(accountNameKey);

            try
            {
                //First we attempt to get the account from the Redis Cache
                var accountImagesRedisValue = cache.StringGet(redisKey);
                if (accountImagesRedisValue.HasValue)
                {
                    accountImages = JsonConvert.DeserializeObject<List<ImageRecordGroupModel>>(accountImagesRedisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion


            #region Plan (B) Get directly from table storage and set the cache

            if (accountImages == null)
            {
                #region (STEP 1) Get Account Object (for id)

                #region (Plan A) Get Account

                try
                {
                    //First we attempt to get the account from the Redis Cache

                    //IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                    string hashKey = "accountbyname:" + accountNameKey;
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
                        account = accountManagementServiceClient.GetAccount(accountNameKey, Common.SharedClientKey);

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


                #endregion

                #region (STEP 2) Get Images Directly and Re-Cache

                accountImages = ImageRecordsCommon.GetImageRecordsForObject(accountNameKey, account.StoragePartition, "account", account.AccountID.ToString());

                try
                {
                    cache.StringSet(
                        redisKey,
                        JsonConvert.SerializeObject(accountImages),
                        RedisCacheKeys.AccountImages.Expiration,
                        When.Always,
                        CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }

            

            #endregion





            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = accountImages;

            return jsonNetResult;
        }


        #endregion

        #region Users

        [Route("Account/Json/Users/GetUsers")]
        [HttpGet]
        public JsonNetResult GetUsers()
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            List<AccountUser> users = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get the users from the Redis Cache first:

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "accountbyid:" + accountId.Replace("-", "");
                string hashField = "users";

                try
                {
                    var redisValue = cache.HashGet(hashKey, hashField);

                    if (redisValue.HasValue)
                    {
                        users = JsonConvert.DeserializeObject<List<AccountUser>>(redisValue);
                    }
                }
                catch
                {

                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (users == null)
            {
                #region (Plan B) Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();

                    users = accountManagementServiceClient.GetAccountUsers(accountId, Common.SharedClientKey).ToList();

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


            //Hide platform user account from the account holders:
            var indexOfPlatformAdmin = 0;
            bool platformAdminExists = false;
            for (var i = 0; i < users.Count; i++)
            {
                if(users[i].Email == "[Config_PlatformEmail]")
                {
                    platformAdminExists = true;
                    indexOfPlatformAdmin = i;
                }
            }

            if(platformAdminExists)
            {
                users.RemoveAt(indexOfPlatformAdmin);
            }
            

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = users;

            return jsonNetResult;
        }

        [Route("Account/Json/Users/DeleteUser")]
        [HttpPost]
        public JsonNetResult DeleteUser(string userId)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
                var requesterId = authCookie.Id;
                var accountId = authCookie.AccountID.ToString();

                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.DeleteAccountUser(accountId, userId,
                    requesterId,
                    AccountManagementService.RequesterType.AccountUser, Common.SharedClientKey);

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

        [Route("Account/Json/Users/UpdateName")]
        [HttpPost]
        public JsonNetResult UpdateName(string userId, string firstName, string lastName)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
                var requesterId = authCookie.Id;
                var accountId = authCookie.AccountID.ToString();

                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountUserFullName(accountId, userId, firstName, lastName,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.AccountUser, Common.SharedClientKey);

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

        [Route("Account/Json/Users/UpdateEmail")]
        [HttpPost]
        public JsonNetResult UpdateEmail(string userId, string email)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
  
            try
            {
                var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
                var requesterId = authCookie.Id;
                var accountId = authCookie.AccountID.ToString();

                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountUserEmail(accountId, userId, email,
                    requesterId,
                    AccountManagementService.RequesterType.AccountUser, Common.SharedClientKey);

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

        [Route("Account/Json/Users/UpdateRole")]
        [HttpPost]
        public JsonNetResult UpdateRole(string userId, string role)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();

                response = accountManagementServiceClient.UpdateAccountUserRole(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                    userId,
                    role,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.AccountUser, Common.SharedClientKey);

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


        [Route("Account/Json/Users/ChangeOwnerStatus")]
        [HttpPost]
        public JsonNetResult ChangeOwnerStatus(string userId, bool isOwner)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
                var requesterId = authCookie.Id;
                var accountId = authCookie.AccountID.ToString();

                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountOwnershipStatus(accountId, userId, isOwner,
                    requesterId,
                    AccountManagementService.RequesterType.AccountUser, Common.SharedClientKey);

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

        [Route("Account/Json/Users/ChangeActiveState")]
        [HttpPost]
        public JsonNetResult ChangeActiveState(string userId, bool isActive)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountUserActiveState(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), userId, isActive,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.AccountUser, Common.SharedClientKey);

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


        [Route("Account/Json/Users/SendPasswordLink")]
        [HttpPost]
        public JsonNetResult SendPasswordLink(string email)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.ClaimLostPassword(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), email, Common.SharedClientKey);

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

        [Route("Account/Json/Users/GetAllAccountsForEmail")]
        [HttpPost]
        public JsonNetResult GetAllAccountsForEmail(string email)
        {
            var userAccounts = new List<UserAccount>();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                userAccounts = accountManagementServiceClient.GetAllAccountsForEmail(email, Common.SharedClientKey).ToList();

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
            jsonNetResult.Data = userAccounts;

            return jsonNetResult;
        }

        #endregion

        #region Account Capacity

        [Route("Account/Json/GetAccountCapacity")]
        [HttpGet]
        public JsonNetResult GetAccountCapacity()
        {

            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            AccountCapacity accountCapacity = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "capacities";
                string hashField = "account:" + accountId;

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    accountCapacity = JsonConvert.DeserializeObject<AccountCapacity>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (accountCapacity == null)
            {
                #region (Plan B) Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    accountCapacity = accountManagementServiceClient.GetAccountCapacity(accountId, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = accountCapacity;

            return jsonNetResult;

        }

        #endregion

        #region Invitations

        [Route("Account/Json/Users/InviteUser")]
        [HttpPost]
        public JsonNetResult InviteUser(string email, string firstName, string lastName, string role, bool isOwner)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
 
            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.InviteAccountUser(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), email, firstName, lastName, role, isOwner,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.AccountUser, Common.SharedClientKey);

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

        [Route("Account/Json/Users/DeleteInvitation")]
        [HttpPost]
        public JsonNetResult DeleteInvitation(string invitationKey)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.DeleteAccountUserInvitation(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), invitationKey,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.AccountUser, Common.SharedClientKey);

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

        [Route("Account/Json/Users/ResendInvitation")]
        [HttpPost]
        public JsonNetResult ResendInvitation(string invitationKey)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.ResendAccountUserInvitation(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), invitationKey, Common.SharedClientKey);

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

        [Route("Account/Json/Users/GetInvitations")]
        [HttpGet]
        public JsonNetResult GetInvitations()
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            List<UserInvitation> invitations = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get the users from the Redis Cache first:

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "accountbyid:" + accountId.Replace("-", "");
                string hashField = "invitations";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    invitations = JsonConvert.DeserializeObject<List<UserInvitation>>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (invitations == null)
            {
                #region (Plan B) Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    invitations = accountManagementServiceClient.GetAccountUserInvitations(accountId, Common.SharedClientKey).ToList();

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = invitations;

            return jsonNetResult;
        }


        #endregion

        #region Credit Cards

        [Route("Account/Json/GetCardInfo")]
        [HttpGet]
        public JsonNetResult GetCardInfo()
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            AccountCreditCardInfo accountCreditCardInfo = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "accountbyid:" + user.AccountID.ToString().Replace("-", "");
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
            }

            #endregion

            if (accountCreditCardInfo == null)
            {
                #region (Plan B) Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    accountCreditCardInfo = accountManagementServiceClient.GetCreditCardInfo(
                        user.AccountID.ToString(),
                        user.Id,
                        AccountAdminSite.AccountManagementService.RequesterType.AccountUser, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = accountCreditCardInfo;

            return jsonNetResult;
        }


        [Route("Account/Json/AddUpdateCard")]
        [HttpPost]
        public JsonNetResult AddUpdateCard(string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                //Get the ip address and call origin
                var ipAddress = Request.UserHostAddress;
                var origin = "Web";
                    

                accountManagementServiceClient.Open();

                response = accountManagementServiceClient.AddUpdateCreditCard(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
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


        #endregion

        #region AccountLogs

        // Logs -----

        [Route("Account/Json/GetAccountLog")]
        [HttpGet]
        public JsonNetResult GetAccountLog(int maxRecords)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            #region Get data from WCF

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<AccountAdminSite.AccountManagementService.AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLog(accountId, maxRecords, Common.SharedClientKey).ToList();

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
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Account/Json/GetAccountLogByCategory")]
        [HttpGet]
        public JsonNetResult GetAccountLogByCategory(string categoryType, int maxRecords)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            #region Get data from WCF

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<AccountAdminSite.AccountManagementService.AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLogByCategory(accountId, categoryType, maxRecords, Common.SharedClientKey).ToList();

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
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Account/Json/GetAccountLogByActivity")]
        [HttpGet]
        public JsonNetResult GetAccountLogByActivity(string activityType, int maxRecords)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            #region Get data from WCF

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<AccountAdminSite.AccountManagementService.AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLogByActivity(accountId, activityType, maxRecords, Common.SharedClientKey).ToList();

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
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Account/Json/GetAccountLogByUser")]
        [HttpGet]
        public JsonNetResult GetAccountLogByUser(string userId, int maxRecords)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            #region Get data from WCF

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<AccountAdminSite.AccountManagementService.AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLogByUser(accountId, userId, maxRecords, Common.SharedClientKey).ToList();

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
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Account/Json/GetAccountLogByObject")]
        [HttpGet]
        public JsonNetResult GetAccountLogByObject(string objectId, int maxRecords)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            #region Get data from WCF

            /*
                 
                 Update Web.Config to below if you get this error:
                 
                 The maximum message size quota for incoming messages (65536) has been exceeded. To increase the quota, use the MaxReceivedMessageSize property on the appropriate binding element
                 
                 <binding name="NetTcpBinding_IAccountManagementService" maxBufferSize="262114" maxReceivedMessageSize="262114"> <!-- FOR LARGE LOG DATA-->
                    <security mode="None" />
                </binding>
                <binding name="NetTcpBinding_IPlatformManagementService" maxBufferSize="262114" maxReceivedMessageSize="262114"> <!-- FOR LARGE LOG DATA -->
                    <security mode="None" />
                </binding>
                  
                 
                 */

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<AccountAdminSite.AccountManagementService.AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLogByObject(accountId, objectId, maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                /*
                 
                 Update Web.Config to below if you get this error:
                 
                 The maximum message size quota for incoming messages (65536) has been exceeded. To increase the quota, use the MaxReceivedMessageSize property on the appropriate binding element
                 
                 <binding name="NetTcpBinding_IAccountManagementService" maxBufferSize="262114" maxReceivedMessageSize="262114"> <!-- FOR LARGE LOG DATA-->
                    <security mode="None" />
                </binding>
                <binding name="NetTcpBinding_IPlatformManagementService" maxBufferSize="262114" maxReceivedMessageSize="262114"> <!-- FOR LARGE LOG DATA -->
                    <security mode="None" />
                </binding>
                  
                 
                 */

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
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        // Log Types -----

        [Route("Account/Json/GetAccountLogCategories")]
        [HttpGet]
        public JsonNetResult GetAccountLogCategories()
        {
            var categories = new List<string>();

            var categoryCacheKey = "accountLogCategories";
            var cacheHours = 8;

            if (System.Web.HttpRuntime.Cache.Get(categoryCacheKey) == null)
            {
                #region Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    categories = accountManagementServiceClient.GetAccountLogCategories(Common.SharedClientKey).ToList();

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

                //Store into local cache for 8 hours
                System.Web.HttpRuntime.Cache.Insert(categoryCacheKey, categories, null, DateTime.Now.AddHours(cacheHours), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);


                #endregion
            }
            else
            {
                //Get data from cache
                categories = (List<string>)System.Web.HttpRuntime.Cache.Get(categoryCacheKey);
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = categories;

            return jsonNetResult;

        }

        [Route("Account/Json/GetAccountLogActivities")]
        [HttpGet]
        public JsonNetResult GetAccountLogActivities()
        {
            var activities = new List<string>();

            var activityCacheKey = "accountLogActivities";
            var cacheHours = 8;

            if (System.Web.HttpRuntime.Cache.Get(activityCacheKey) == null)
            {
                #region Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    activities = accountManagementServiceClient.GetAccountLogActivities(Common.SharedClientKey).ToList();

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

                #region Add Breaks for Dropdown Sections

                //Add breaks for sections in dropdowns
                var currentSectionName = String.Empty;

                for (int i = 0; i < activities.Count; i++)
                {
                    var thisSection = activities[i].Substring(0, activities[i].IndexOf("_"));

                    if (currentSectionName != thisSection && i != 0)
                    {
                        //Add a break
                        activities.Insert(i, "break");
                    }

                    currentSectionName = thisSection;

                }

                #endregion

                //Store into local cache for 8 hours
                System.Web.HttpRuntime.Cache.Insert(activityCacheKey, activities, null, DateTime.Now.AddHours(cacheHours), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);

            }
            else
            {
                //Get data from cache
                activities = (List<string>)System.Web.HttpRuntime.Cache.Get(activityCacheKey);
            }



            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = activities;

            return jsonNetResult;

        }

        #endregion

        #region Accounts List (To Trade or Communicate with)

        [Route("Account/Json/GetAccountsList")]
        [HttpGet]
        public JsonNetResult GetAccountsList()
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            #region Get data from WCF

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var accountsList = new List<Account>();

            try
            {
                accountManagementServiceClient.Open();
                accountsList = accountManagementServiceClient.GetAccounts(0, 0, "AccountName", Common.SharedClientKey).ToList();

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
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = accountsList;

            return jsonNetResult;

        }

        #endregion

        #region Close Account

        [Route("Account/Json/CloseAccount")]
        [HttpGet]
        public JsonNetResult CloseAccount()
        {

            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            DataAccessResponseType response = new DataAccessResponseType();

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.CloseAccount(user.AccountID.ToString(), user.Id, RequesterType.AccountUser, Common.SharedClientKey);

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

        #endregion

        #endregion


    }
}