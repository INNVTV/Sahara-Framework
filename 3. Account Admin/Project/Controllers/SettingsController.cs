using AccountAdminSite.ApplicationImageFormatsService;
using AccountAdminSite.ApplicationPropertiesService;
using AccountAdminSite.Models.ImageFormat;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{

    #region Shared Public Classes

    /// <summary>
    /// Shared by Products to merge Properties with ProductProperties
    /// </summary>
    [Authorize]
    public static class SettingsCommon
    {
        #region Get Properties Shared Helper

        public static List<PropertyModel> GetProperties(string accountNameKey, string type = "all")
        {
            List<PropertyModel> properties = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":properties";
                string hashField = type;

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    properties = JsonConvert.DeserializeObject<List<PropertyModel>>(redisValue);
                }


            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (properties == null)
            {
                #region (Plan B) Get data from WCF

                var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

                try
                {
                    applicationPropertiesServiceClient.Open();
                    properties = applicationPropertiesServiceClient.GetProperties(accountNameKey, PropertyListType.All, Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(applicationPropertiesServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            return properties;
        }

        #endregion

        #region Get Image Formats Shared Helper
        /// <summary>
        /// Shared helper method
        /// </summary>
        /// <returns></returns>
        public static List<ImageFormatGroupModel> GetImageFormatsHelper(string accountNameKey, string imageFormatGroupTypeNameKey)
        {
            List<ImageFormatGroupModel> imageGroups = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":imageformats";
                string hashField = "grouptype:" + imageFormatGroupTypeNameKey + ":all";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    imageGroups = JsonConvert.DeserializeObject<List<ImageFormatGroupModel>>(redisValue);
                }


            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (imageGroups == null)
            {
                #region (Plan B) Get data from WCF

                var applicationImageFormatsServiceClient = new ApplicationImageFormatsService.ApplicationImageFormatsServiceClient();

                try
                {
                    applicationImageFormatsServiceClient.Open();
                    imageGroups = applicationImageFormatsServiceClient.GetImageFormats(accountNameKey, imageFormatGroupTypeNameKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(applicationImageFormatsServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationImageFormatsServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            return imageGroups;
        }

        #endregion
    }

    #endregion

    [Authorize]
    public class SettingsController : Controller
    {

        #region View Controllers

        // GET: /Settings/
        public ActionResult Index()
        {
            return View();
        }


        // Used for Detail variation
        // GET: /Settings/{id}
        /*
        [Route("Settings/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }*/

        #endregion


        #region JSON Services


        #region JSON Services for Properties

        #region Get

        [Route("Properties/Json/GetProperties")]
        [HttpGet]
        public JsonNetResult GetProperties()
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            List<PropertyModel> properties = SettingsCommon.GetProperties(accountNameKey);

            /*
            List<PropertyModel> properties = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":properties";
                string hashField = "list";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    properties = JsonConvert.DeserializeObject<List<PropertyModel>>(redisValue);
                }


            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (properties == null)
            {
                #region (Plan B) Get data from WCF

                var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

                try
                {
                    applicationPropertiesServiceClient.Open();
                    properties = applicationPropertiesServiceClient.GetProperties(accountNameKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(applicationPropertiesServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

           */
            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = properties;

            return jsonNetResult;

        }

        [Route("Properties/Json/GetPropertyTypes")]
        [HttpGet]
        public JsonNetResult GetPropertyTypes()
        {

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            List<PropertyTypeModel> propertyTypes = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "global";
                string hashField = "propertytypes";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    propertyTypes = JsonConvert.DeserializeObject<List<PropertyTypeModel>>(redisValue);
                }


            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (propertyTypes == null)
            {
                #region (Plan B) Get data from WCF

                var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

                try
                {
                    applicationPropertiesServiceClient.Open();
                    propertyTypes = applicationPropertiesServiceClient.GetPropertyTypes(Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(applicationPropertiesServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = propertyTypes;

            return jsonNetResult;

        }

        #endregion

        #region Create

        [Route("Properties/Json/CreateProperty")]
        [HttpPost]
        public JsonNetResult CreateProperty(string propertyTypeNameKey, string propertyName)
        {

            var response = new ApplicationPropertiesService.DataAccessResponseType();
            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.CreateProperty(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), propertyTypeNameKey, propertyName,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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


        [Route("Properties/Json/CreatePropertyValue")]
        [HttpPost]
        public JsonNetResult CreatePropertyValue(string propertyName, string propertyValueName)
        {
            var response = new ApplicationPropertiesService.DataAccessResponseType();
            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.CreatePropertyValue(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), propertyName, propertyValueName,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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

        #region Swatch Management

        //Step 1:
        [Route("Properties/Json/UploadSwatchImage")]
        [HttpPost]
        public JsonNetResult UploadSwatchImage()
        {
            var length = Request.ContentLength;
            var imageBytes = new byte[length];

            Request.InputStream.Read(imageBytes, 0, length);

            var url = "";
            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                url = applicationPropertiesServiceClient.UploadPropertySwatchImage(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), imageBytes,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);


                // Upate the response object
                //response.isSuccess = false;
                //response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = url;

            return jsonNetResult;

        }

        //Step 2:
        [Route("Properties/Json/CreateSwatchValue")]
        [HttpPost]
        public JsonNetResult CreateSwatchValue(string propertyName, string swatchImage, string swatchLabel)
        {
            var response = new ApplicationPropertiesService.DataAccessResponseType();
            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.CreateSwatchValue(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), propertyName, swatchImage, swatchLabel,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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

        #region Update
        [Route("Properties/Json/UpdatePropertyListingState")]
        [HttpPost]
        public JsonNetResult UpdatePropertyListingState(string propertyNameKey, bool isListing)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationPropertiesService.DataAccessResponseType();

            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.UpdatePropertyListingState(accountId, propertyNameKey, isListing,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Properties/Json/UpdatePropertyDetailsState")]
        [HttpPost]
        public JsonNetResult UpdatePropertyDetailsState(string propertyNameKey, bool isDetails)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationPropertiesService.DataAccessResponseType();

            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.UpdatePropertyDetailsState(accountId, propertyNameKey, isDetails,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Properties/Json/UpdateSortableState")]
        [HttpPost]
        public JsonNetResult UpdateSortableState(string propertyNameKey, bool isSortable)
        {

            var response = new ApplicationPropertiesService.DataAccessResponseType();

            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.UpdatePropertySortableState(
                    AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                    propertyNameKey,
                    isSortable,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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


        [Route("Properties/Json/UpdateAppendableState")]
        [HttpPost]
        public JsonNetResult UpdateAppendableState(string propertyNameKey, bool isAppendable)
        {

            var response = new ApplicationPropertiesService.DataAccessResponseType();

            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.UpdatePropertyAppendableState(
                    AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                    propertyNameKey,
                    isAppendable,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Properties/Json/UpdateFacetInterval")]
        [HttpPost]
        public JsonNetResult UpdateFacetInterval(string propertyNameKey, string facetInterval)
        {

            var response = new ApplicationPropertiesService.DataAccessResponseType();
            int newFacetInterval = 0;
                
            try
            {
                newFacetInterval = Int32.Parse(facetInterval);

                var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

                try
                {
                    applicationPropertiesServiceClient.Open();
                    response = applicationPropertiesServiceClient.UpdatePropertyFacetInterval(
                        AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                        propertyNameKey,
                        newFacetInterval,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(applicationPropertiesServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

                    // Upate the response object
                    response.isSuccess = false;
                    response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                    //response.ErrorMessages[0] = exceptionMessage;

                    #endregion
                }

            }
            catch
            {
                response.isSuccess = false;
                response.ErrorMessage = "Interval must be a valid whole number value";
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        [Route("Properties/Json/UpdateFacetableState")]
        [HttpPost]
        public JsonNetResult UpdateFacetableState(string propertyNameKey, bool isFacetable)
        {

            var response = new ApplicationPropertiesService.DataAccessResponseType();

            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.UpdatePropertyFacetableState(
                    AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                    propertyNameKey,
                    isFacetable,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Properties/Json/UpdateSymbol")]
        [HttpPost]
        public JsonNetResult UpdateSymbol(string propertyNameKey, string symbol)
        {
            if(symbol == null)
            {
                symbol = "";
            }

            var response = new ApplicationPropertiesService.DataAccessResponseType();

            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.UpdatePropertySymbol(
                    AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                    propertyNameKey,
                    symbol,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Properties/Json/UpdateSymbolPlacement")]
        [HttpPost]
        public JsonNetResult UpdateSymbolPlacement(string propertyNameKey, string symbolPlacement)
        {

            var response = new ApplicationPropertiesService.DataAccessResponseType();

            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.UpdatePropertySymbolPlacement(
                    AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                    propertyNameKey,
                    symbolPlacement,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Properties/Json/UpdateFeaturedProperties")]
        [HttpPost]
        public JsonNetResult UpdateFeaturedProperties(List<String> featuredPropertyOrder)
        {

            var response = new ApplicationPropertiesService.DataAccessResponseType();

            var featuredPropertyOrderDictionary = new Dictionary<string, int>();

            foreach (String id in featuredPropertyOrder)
            {
                featuredPropertyOrderDictionary.Add(id, (featuredPropertyOrderDictionary.Count + 1));
            }

            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.UpdateFeaturedProperties(
                    AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                    featuredPropertyOrderDictionary,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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


        [Route("Properties/Json/ResetFeaturedProperties")]
        [HttpPost]
        public JsonNetResult ResetFeaturedProperties()
        {

            var response = new ApplicationPropertiesService.DataAccessResponseType();

            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.ResetFeaturedProperties(
                    AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(),
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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

        #region Delete

        [Route("Properties/Json/DeleteProperty")]
        [HttpGet]
        public JsonNetResult DeleteProperty(string propertyId)
        {

            var response = new ApplicationPropertiesService.DataAccessResponseType();
            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.DeleteProperty(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), propertyId,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Properties/Json/DeletePropertyValue")]
        [HttpPost]
        public JsonNetResult DeletePropertyValue(string propertyNameKey, string propertyValueNameKey)
        {

            var response = new ApplicationPropertiesService.DataAccessResponseType();
            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.DeletePropertyValue(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), propertyNameKey, propertyValueNameKey,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Properties/Json/DeleteSwatchValue")]
        [HttpPost]
        public JsonNetResult DeleteSwatchValue(string propertyNameKey, string swatchNameKey)
        {

           var response = new ApplicationPropertiesService.DataAccessResponseType();
            var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

            try
            {
                applicationPropertiesServiceClient.Open();
                response = applicationPropertiesServiceClient.DeletePropertySwatch(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), propertyNameKey, swatchNameKey,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationPropertiesService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationPropertiesServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

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

        #region JSON Services for Tags

        #region Get

        [Route("Tags/Json/GetTags")]
        [HttpGet]
        public JsonNetResult GetTags()
        {

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            List<string> tags = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":tags";
                string hashField = "list";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    tags = JsonConvert.DeserializeObject<List<string>>(redisValue);
                }


            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (tags == null)
            {
                #region (Plan B) Get data from WCF

                var applicationTagsServiceClient = new ApplicationTagsService.ApplicationTagsServiceClient();

                try
                {
                    applicationTagsServiceClient.Open();
                    tags = applicationTagsServiceClient.GetTags(accountNameKey, Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(applicationTagsServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationTagsServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = tags;

            return jsonNetResult;

        }

        #endregion

        #region Updates

        [Route("Tags/Json/CreateTag")]
        [HttpPost]
        public JsonNetResult CreateTag(string tagName)
        {

            var response = new ApplicationTagsService.DataAccessResponseType();
            var applicationTagsServiceClient = new ApplicationTagsService.ApplicationTagsServiceClient();

            try
            {
                applicationTagsServiceClient.Open();
                response = applicationTagsServiceClient.CreateTag(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), tagName,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationTagsService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationTagsServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationTagsServiceClient, exceptionMessage, currentMethodString);

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

        #region Delete

        [Route("Tags/Json/DeleteTag")]
        [HttpPost]
        public JsonNetResult DeleteTag(string tagName)
        {
            var response = new ApplicationTagsService.DataAccessResponseType();
            var applicationTagsServiceClient = new ApplicationTagsService.ApplicationTagsServiceClient();

            try
            {
                applicationTagsServiceClient.Open();
                response = applicationTagsServiceClient.DeleteTag(AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString(), tagName,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    ApplicationTagsService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationTagsServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationTagsServiceClient, exceptionMessage, currentMethodString);

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


        #region JSON Services for ImageFormats

        #region Get

        [Route("ImageFormat/Json/GetImageGroupTypes")]
        [HttpGet]
        public JsonNetResult GetImageGroupTypes()
        {

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            List<ImageFormatGroupTypeModel> imageFormatGroupTypes = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "global";
                string hashField = "imagegrouptypes";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    imageFormatGroupTypes = JsonConvert.DeserializeObject<List<ImageFormatGroupTypeModel>>(redisValue);
                }


            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (imageFormatGroupTypes == null)
            {
                #region (Plan B) Get data from WCF

                var applicationImageFormatsServiceClient = new ApplicationImageFormatsService.ApplicationImageFormatsServiceClient();

                try
                {
                    applicationImageFormatsServiceClient.Open();
                    imageFormatGroupTypes = applicationImageFormatsServiceClient.GetImageFormatGroupTypes().ToList();

                    //Close the connection
                    WCFManager.CloseConnection(applicationImageFormatsServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationImageFormatsServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }


            #region Build out local version for settings page (aggregating all groupypes with associated group/models)

            var imageGroupTypeSettingsModels = new List<ImageGroupTypeSettingsModel>();

            foreach (ImageFormatGroupTypeModel imageFormatGroupType in imageFormatGroupTypes)
            {
                var imageGroupTypeSettingsModel = new ImageGroupTypeSettingsModel
                {
                    ImageGroupTypeID = imageFormatGroupType.ImageFormatGroupTypeID.ToString(),
                    ImageGroupTypeName = imageFormatGroupType.ImageFormatGroupTypeName,
                    ImageGroupTypeNameKey = imageFormatGroupType.ImageFormatGroupTypeNameKey
                };

                imageGroupTypeSettingsModel.ImageGroups = SettingsCommon.GetImageFormatsHelper(accountNameKey, imageFormatGroupType.ImageFormatGroupTypeNameKey);

                imageGroupTypeSettingsModels.Add(imageGroupTypeSettingsModel);

            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = imageGroupTypeSettingsModels;

            return jsonNetResult;

        }

        [Route("ImageFormat/Json/GetImageFormats")]
        [HttpGet]
        public JsonNetResult GetImageFormats(string imageGroupTypeNameKey)
        {

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            List<ImageFormatGroupModel> imageGroups = SettingsCommon.GetImageFormatsHelper(accountNameKey, imageGroupTypeNameKey);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = imageGroups;

            return jsonNetResult;

        }


        #endregion

        #region Create

        [Route("ImageFormat/Json/CreateImageGroup")]
        [HttpPost]
        public JsonNetResult CreateImageGroup(string imageGroupTypeNameKey, string imageGroupName)
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            var response = new ApplicationImageFormatsService.DataAccessResponseType();

            #region WCF Call

            var applicationImageFormatsServiceClient = new ApplicationImageFormatsService.ApplicationImageFormatsServiceClient();

            try
            {
                applicationImageFormatsServiceClient.Open();
                response = applicationImageFormatsServiceClient.CreateImageGroup(
                    accountNameKey,
                    imageGroupTypeNameKey,
                    imageGroupName,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountAdminSite.ApplicationImageFormatsService.RequesterType.AccountUser, Common.SharedClientKey
               );

                //Close the connection
                WCFManager.CloseConnection(applicationImageFormatsServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationImageFormatsServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("ImageFormat/Json/CreateImageFormat")]
        [HttpPost]
        public JsonNetResult CreateImageFormat(string imageGroupTypeNameKey, string imageGroupNameKey, string imageFormatName, int width, int height, bool listing, bool gallery)
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            var response = new ApplicationImageFormatsService.DataAccessResponseType();

            #region WCF Call

            var applicationImageFormatsServiceClient = new ApplicationImageFormatsService.ApplicationImageFormatsServiceClient();

            try
            {
                applicationImageFormatsServiceClient.Open();
                response = applicationImageFormatsServiceClient.CreateImageFormat(
                    accountNameKey,
                    imageGroupTypeNameKey,
                    imageGroupNameKey,
                    imageFormatName,
                    width,
                    height,
                    listing,
                    gallery,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountAdminSite.ApplicationImageFormatsService.RequesterType.AccountUser, Common.SharedClientKey
               );

                //Close the connection
                WCFManager.CloseConnection(applicationImageFormatsServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationImageFormatsServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #region Delete

        [Route("ImageFormat/Json/DeleteImageGroup")]
        [HttpPost]
        public JsonNetResult DeleteImageGroup(string imageGroupTypeNameKey, string imageGroupNameKey)
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            var response = new ApplicationImageFormatsService.DataAccessResponseType();

            #region WCF Call

            var applicationImageFormatsServiceClient = new ApplicationImageFormatsService.ApplicationImageFormatsServiceClient();

            try
            {
                applicationImageFormatsServiceClient.Open();
                response = applicationImageFormatsServiceClient.DeleteImageGroup(
                    accountNameKey,
                    imageGroupTypeNameKey,
                    imageGroupNameKey,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountAdminSite.ApplicationImageFormatsService.RequesterType.AccountUser, Common.SharedClientKey
               );

                //Close the connection
                WCFManager.CloseConnection(applicationImageFormatsServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationImageFormatsServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("ImageFormat/Json/DeleteImageFormat")]
        [HttpPost]
        public JsonNetResult DeleteImageFormat(string imageGroupTypeNameKey, string imageGroupNameKey, string imageFormatNameKey)
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            var response = new ApplicationImageFormatsService.DataAccessResponseType();

            #region WCF Call

            var applicationImageFormatsServiceClient = new ApplicationImageFormatsService.ApplicationImageFormatsServiceClient();

            try
            {
                applicationImageFormatsServiceClient.Open();
                response = applicationImageFormatsServiceClient.DeleteImageFormat(
                    accountNameKey,
                    imageGroupTypeNameKey,
                    imageGroupNameKey,
                    imageFormatNameKey,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountAdminSite.ApplicationImageFormatsService.RequesterType.AccountUser, Common.SharedClientKey
               );

                //Close the connection
                WCFManager.CloseConnection(applicationImageFormatsServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationImageFormatsServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

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