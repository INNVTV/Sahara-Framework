using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Newtonsoft.Json;
using PlatformAdminSite.PlatformManagementService;
using System.ServiceModel;
using System.Web.Caching;
using StackExchange.Redis;



namespace PlatformAdminSite.Controllers
{
    [Authorize]
    public class PlatformController : Controller
    {

        #region View Controllers

        //
        // GET: /Platform/
        public ActionResult Index()
        {
            return View();
        }

        // Used for Detail variation
        // GET: /Scaffold/{id}
        /*
        [Route("Platform/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }*/



        #endregion

        #region JSON Services

        #region Platform Snapshot (dashboard)
        /*
        [Route("Platform/Json/GetSnapshot")]
        [HttpGet]
        public JsonNetResult GetSnapshot()
        {
            var platformSnapshot = new PlatformSnapshot();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                platformSnapshot = platformManagementServiceClient.GetPlatformShapshot();

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

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
         * jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = platformSnapshot;

            return jsonNetResult;
        }
        */
        #endregion

        #region Platform User Management

        #region Users

        [Route("Platform/Json/GetUsers")]
        [HttpGet]
        public JsonNetResult GetUsers()
        {
            var users = new List<PlatformUser>();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                users = platformManagementServiceClient.GetPlatformUsers(Common.SharedClientKey).ToList();

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

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = users;

            return jsonNetResult;
        }


        [Route("Platform/Json/CreateUser")]
        [HttpPost]
        public JsonNetResult CreateUser(string email, string firstName, string lastName, string role, string password, string confirmPassword)
        {
            //Confirm passwords before sending to core services
            #region Confirm
            if (password != confirmPassword)
            {
                var error = new PlatformManagementService.DataAccessResponseType();
                error.isSuccess = false;
                error.ErrorMessage = "Password and confirmation password do not match.";

                JsonNetResult jsonNetErrorResult = new JsonNetResult();
                jsonNetErrorResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetErrorResult.Data = error;

                return jsonNetErrorResult;

            }
            #endregion

            var response = new PlatformManagementService.DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                response = platformManagementServiceClient.CreatePlatformUser(email, firstName, lastName, password, role,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Platform/Json/DeleteUser")]
        [HttpPost]
        public JsonNetResult DeleteUser(string userId)
        {
            var response = new PlatformManagementService.DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                response = platformManagementServiceClient.DeletePlatformUser(userId,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Platform/Json/UpdateName")]
        [HttpPost]
        public JsonNetResult UpdateName(string userId, string firstName, string lastName)
        {
            var response = new PlatformManagementService.DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                response = platformManagementServiceClient.UpdatePlatformUserFullName(userId, firstName, lastName,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Platform/Json/UpdateEmail")]
        [HttpPost]
        public JsonNetResult UpdateEmail(string userId, string email)
        {
            var response = new PlatformManagementService.DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            
            try
            {
                platformManagementServiceClient.Open();

                response = platformManagementServiceClient.UpdatePlatformUserEmail(userId, email,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Platform/Json/UpdateRole")]
        [HttpPost]
        public JsonNetResult UpdateRole(string userId, string role)
        {
            var response = new PlatformManagementService.DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                response = platformManagementServiceClient.UpdatePlatformUserRole(userId,
                    role,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Platform/Json/ChangeActiveState")]
        [HttpPost]
        public JsonNetResult ChangeActiveState(string userId, bool isActive)
        {
            var response = new PlatformManagementService.DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                response = platformManagementServiceClient.UpdatePlatformUserActiveState(userId, isActive,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Platform/Json/SendPasswordLink")]
        [HttpPost]
        public JsonNetResult SendPasswordLink(string accountId, string email)
        {
            var response = new PlatformManagementService.DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();
                response = platformManagementServiceClient.ClaimLostPassword(email, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #region Invitations

        [Route("Platform/Json/InviteUser")]
        [HttpPost]
        public JsonNetResult InviteUser(string email, string firstName, string lastName, string role)
        {
            var response = new PlatformManagementService.DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                response = platformManagementServiceClient.InvitePlatformUser(email, firstName, lastName, role,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Platform/Json/DeleteInvitation")]
        [HttpPost]
        public JsonNetResult DeleteInvitation(string invitationKey)
        {
            var response = new PlatformManagementService.DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            try
            {
                platformManagementServiceClient.Open();
                response = platformManagementServiceClient.DeletePlatformUserInvitation(invitationKey,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Platform/Json/ResendInvitation")]
        [HttpPost]
        public JsonNetResult ResendInvitation(string invitationKey)
        {
            var response = new PlatformManagementService.DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                response = platformManagementServiceClient.ResendPlatformUserInvitation(invitationKey, Common.SharedClientKey);

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

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Platform/Json/GetInvitations")]
        [HttpGet]
        public JsonNetResult GetInvitations()
        {
            var invitations = new List<PlatformInvitation>();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();
                invitations = platformManagementServiceClient.GetPlatformUserInvitations(Common.SharedClientKey).ToList();

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

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = invitations;

            return jsonNetResult;
        }


        #endregion


        #region Password Claims

        [Route("Platform/Json/GetPasswordClaims")]
        [HttpGet]
        public JsonNetResult GetPasswordClaims()
        {
            var claims = new List<PlatformPasswordResetClaim>();
            var platformManagementServiceClient = new PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                claims = platformManagementServiceClient.GetLostPasswordClaims(AuthenticationCookieManager.GetAuthenticationCookie().Id, RequesterType.PlatformUser, Common.SharedClientKey).ToList();

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

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = claims;

            return jsonNetResult;
        }

        #endregion

        #endregion

        #region Document Partitions


        /*
        [Route("Platform/Json/GetDocumentPartitionTier")]
        [HttpGet]
        public JsonNetResult GetDocumentPartitionTier(string documentPartitionTierId)
        {
            DocumentPartitionTier documentPartitionTier = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer.GetDatabase();

                string hashKey = "partitiontiers";
                string hashField = documentPartitionTierId;

                var redisValue = cache.HashGet(hashKey, hashField);

                //con.Close();

                if (redisValue.HasValue)
                {
                    documentPartitionTier = JsonConvert.DeserializeObject<DocumentPartitionTier>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (documentPartitionTier == null)
            {
                #region (Plan B) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    documentPartitionTier = PlatformManagementServiceClient.GetDocumentPartitionTier(documentPartitionTierId);

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = documentPartitionTier;

            return jsonNetResult;
        }

        [Route("Platform/Json/GetDocumentPartitions")]
        [HttpGet]
        public JsonNetResult GetDocumentPartitions(string documentPartitionTierId)
        {
            List<DocumentPartition> documentPartitions = null;

            #region (Plan A) Get data from Redis Cache

            /*
            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer.GetDatabase();

                string hashKey = "partitiontiers";
                string hashField = documentPartitionTierId;

                var redisValue = cache.HashGet(hashKey, hashField);

                //con.Close();

                if (redisValue.HasValue)
                {
                    documentPartitionTier = JsonConvert.DeserializeObject<DocumentPartitionTier>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }
            * /
            #endregion

            if (documentPartitions == null)
            {
                #region (Plan B) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    documentPartitions = PlatformManagementServiceClient.GetDocumentPartitions(documentPartitionTierId).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = documentPartitions;

            return jsonNetResult;
        }

        [Route("Platform/Json/GetDocumentPartition")]
        [HttpGet]
        public JsonNetResult GetDocumentPartition(string documentPartitionId)
        {
            DocumentPartition documentPartition = null;

            #region (Plan A) Get data from Redis Cache

            /*
            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer.GetDatabase();

                string hashKey = "partitiontiers";
                string hashField = documentPartitionTierId;

                var redisValue = cache.HashGet(hashKey, hashField);

                //con.Close();

                if (redisValue.HasValue)
                {
                    documentPartitionTier = JsonConvert.DeserializeObject<DocumentPartitionTier>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }
            * /
            #endregion

            if (documentPartition == null)
            {
                #region (Plan B) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    documentPartition = PlatformManagementServiceClient.GetDocumentPartition(documentPartitionId);

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = documentPartition;

            return jsonNetResult;
        }
    */


        [Route("Platform/Json/GetDocumentPartitionCollectionProperties")]
        [HttpGet]
        public JsonNetResult GetDocumentPartitionCollectionProperties(string documentPartitionId)
        {
            DocumentPartitionCollectionProperties documentPartitionCollectionProperties = null;

            #region (Plan A) Get data from Redis Cache

            /*
            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer.GetDatabase();

                string hashKey = "partitiontiers";
                string hashField = documentPartitionTierId;

                var redisValue = cache.HashGet(hashKey, hashField);

                //con.Close();

                if (redisValue.HasValue)
                {
                    documentPartitionTier = JsonConvert.DeserializeObject<DocumentPartitionTier>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }
            */
            #endregion

            if (documentPartitionCollectionProperties == null)
            {
                #region (Plan B) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    documentPartitionCollectionProperties = PlatformManagementServiceClient.GetDocumentPartitionProperties(documentPartitionId, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = documentPartitionCollectionProperties;

            return jsonNetResult;
        }

        [Route("Platform/Json/GetDocumentPartitionTenantCollectionProperties")]
        [HttpGet]
        public JsonNetResult GetDocumentPartitionTenantCollectionProperties(string accountId)
        {
            DocumentPartitionTenantCollectionProperties documentPartitionTenantCollectionProperties = null;

            #region (Plan A) Get data from Redis Cache

            /*
            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer.GetDatabase();

                string hashKey = "partitiontiers";
                string hashField = documentPartitionTierId;

                var redisValue = cache.HashGet(hashKey, hashField);

                //con.Close();

                if (redisValue.HasValue)
                {
                    documentPartitionTier = JsonConvert.DeserializeObject<DocumentPartitionTier>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }
            */
            #endregion

            if (documentPartitionTenantCollectionProperties == null)
            {
                #region (Plan B) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    documentPartitionTenantCollectionProperties = PlatformManagementServiceClient.GetDocumentPartitionTenantProperties(accountId, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = documentPartitionTenantCollectionProperties;

            return jsonNetResult;
        }


        #endregion

        #region SQL Partitions

        [Route("Platform/Json/GetSqlPartitions")]
        [HttpGet]
        public JsonNetResult GetSqlPartitions()
        {
            List<SqlPartition> sqlPartitions = null;

            if (sqlPartitions == null)
            {
                #region (Plan A) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    sqlPartitions = PlatformManagementServiceClient.GetSqlPartitions(Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = sqlPartitions;

            return jsonNetResult;
        }


        [Route("Platform/Json/GetSqlPartition")]
        [HttpGet]
        public JsonNetResult GetSqlPartition(string sqlPartitionName)
        {
            SqlPartition sqlPartition = null;

            if (sqlPartition == null)
            {
                #region (Plan A) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    sqlPartition = PlatformManagementServiceClient.GetSqlPartition(sqlPartitionName, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = sqlPartition;

            return jsonNetResult;
        }

        [Route("Platform/Json/GetSqlPartitionSchemas")]
        [HttpGet]
        public JsonNetResult GetSqlPartitionSchemas(string sqlPartitionName)
        {
            List<string> sqlPartitionSchemas = null;

            if (sqlPartitionSchemas == null)
            {
                #region (Plan A) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    sqlPartitionSchemas = PlatformManagementServiceClient.GetSqlPartitionSchemas(sqlPartitionName, 50, Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = sqlPartitionSchemas;

            return jsonNetResult;
        }


        [Route("Platform/Json/GetSqlPartitionTenantSchemaLog")]
        [HttpGet]
        public JsonNetResult GetSqlPartitionTenantSchemaLog(string accountId)
        {
            List<SqlSchemaLog> sqlSchemaLogs = null;

            if (sqlSchemaLogs == null)
            {
                #region (Plan A) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    sqlSchemaLogs = PlatformManagementServiceClient.GetSqlSchemaLog(accountId, 50, Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = sqlSchemaLogs;

            return jsonNetResult;
        }

        #endregion

        #region Search Partitions

        [Route("Platform/Json/GetSearchPartitions")]
        [HttpGet]
        public JsonNetResult GetSearchPartitions()
        {
            List<SearchPartition> searchPartitions = null;

            if (searchPartitions == null)
            {
                #region (Plan A) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    searchPartitions = PlatformManagementServiceClient.GetSearchPartitions(Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = searchPartitions;

            return jsonNetResult;
        }


        #endregion

        #region Storage Partitions

        [Route("Platform/Json/GetStoragePartitions")]
        [HttpGet]
        public JsonNetResult GetStoragePartitions()
        {
            List<StoragePartition> storagePartitions = null;

            if (storagePartitions == null)
            {
                #region (Plan A) Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();

                    storagePartitions = PlatformManagementServiceClient.GetStoragePartitions(Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = storagePartitions;

            return jsonNetResult;
        }


        #endregion


        #region PlatformLogs

        // Logs -----

        [Route("Platform/Json/GetPlatformLog")]
        [HttpGet]
        public JsonNetResult GetPlatformLog(int maxRecords)
        {

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

            var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            var creditsTransactonLog = new List<PlatformAdminSite.PlatformManagementService.PlatformActivityLog>();

            try
            {
                PlatformManagementServiceClient.Open();
                creditsTransactonLog = PlatformManagementServiceClient.GetPlatformLog(maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(PlatformManagementServiceClient);
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
                WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Platform/Json/GetPlatformLogByCategory")]
        [HttpGet]
        public JsonNetResult GetPlatformLogByCategory(string categoryType, int maxRecords)
        {

            #region Get data from WCF

            var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            var creditsTransactonLog = new List<PlatformAdminSite.PlatformManagementService.PlatformActivityLog>();

            try
            {
                PlatformManagementServiceClient.Open();
                creditsTransactonLog = PlatformManagementServiceClient.GetPlatformLogByCategory(categoryType, maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(PlatformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Platform/Json/GetPlatformLogByActivity")]
        [HttpGet]
        public JsonNetResult GetPlatformLogByActivity(string activityType, int maxRecords)
        {

            #region Get data from WCF

            var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            var creditsTransactonLog = new List<PlatformAdminSite.PlatformManagementService.PlatformActivityLog>();

            try
            {
                PlatformManagementServiceClient.Open();
                creditsTransactonLog = PlatformManagementServiceClient.GetPlatformLogByActivity(activityType, maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(PlatformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }


        [Route("Platform/Json/GetPlatformLogByAccount")]
        [HttpGet]
        public JsonNetResult GetPlatformLogByAccount(string accountId, int maxRecords)
        {

            #region Get data from WCF

            var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            var creditsTransactonLog = new List<PlatformAdminSite.PlatformManagementService.PlatformActivityLog>();

            try
            {
                PlatformManagementServiceClient.Open();
                creditsTransactonLog = PlatformManagementServiceClient.GetPlatformLogByAccount(accountId, maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(PlatformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Platform/Json/GetPlatformLogByUser")]
        [HttpGet]
        public JsonNetResult GetPlatformLogByUser(string userId, int maxRecords)
        {

            #region Get data from WCF

            var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            var creditsTransactonLog = new List<PlatformAdminSite.PlatformManagementService.PlatformActivityLog>();

            try
            {
                PlatformManagementServiceClient.Open();
                creditsTransactonLog = PlatformManagementServiceClient.GetPlatformLogByUser(userId, maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(PlatformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Platform/Json/GetPlatformLogCategories")]
        [HttpGet]
        public JsonNetResult GetPlatformLogCategories()
        {
            var categories = new List<string>();

            var categoryCacheKey = "platformLogCategories";
            var cacheHours = 8;

            if (System.Web.HttpRuntime.Cache.Get(categoryCacheKey) == null)
            {
                #region Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();
                    categories = PlatformManagementServiceClient.GetPlatformLogCategories(Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Platform/Json/GetPlatformLogActivities")]
        [HttpGet]
        public JsonNetResult GetPlatformLogActivities()
        {
            var activities = new List<string>();

            var activityCacheKey = "platformLogActivities";
            var cacheHours = 8;

            if (System.Web.HttpRuntime.Cache.Get(activityCacheKey) == null)
            {
                #region Get data from WCF

                var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    PlatformManagementServiceClient.Open();
                    activities = PlatformManagementServiceClient.GetPlatformLogActivities(Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(PlatformManagementServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

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


        #region Snapshot

        [Route("Platform/Json/GetSnapshot")]
        [HttpGet]
        public JsonNetResult GetSnapshot()
        {

            #region Get data from WCF


            var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            var infrastructureSnapshot = new InfrastructureSnapshot();

            try
            {
                PlatformManagementServiceClient.Open();
                infrastructureSnapshot = PlatformManagementServiceClient.GetInfrastructureShapshot(Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(PlatformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = infrastructureSnapshot;

            return jsonNetResult;

        }

        #endregion

        #endregion


    }
}