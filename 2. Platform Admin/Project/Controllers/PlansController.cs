using Newtonsoft.Json;
using PlatformAdminSite.AccountManagementService;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    public class PlansController : Controller
    {
        // GET: PaymentPlan
        public ActionResult Index()
        {
            return View();
        }

        #region JSON Feeds


        #region Get

        [Route("Plans/Json/GetPlans")]
        [HttpGet]
        public JsonNetResult GetPlans()
        {
            List<AccountPaymentPlanService.PaymentPlan> paymentPlans = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "paymentplans";
                string hashField = "list:True:True";

                var redisValue = cache.HashGet(hashKey, hashField);

                //con.Close();

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

                    paymentPlans = accountPaymentPlanServiceClient.GetPaymentPlans(true, true).ToList();

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
            jsonNetResult.Data = paymentPlans;

            return jsonNetResult;

        }

        [Route("Plans/Json/GetFrequencies")]
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

        #region Create

        [Route("Plans/Json/CreatePlan")]
        [HttpPost]
        public JsonNetResult CreatePlan(string paymentPlanName, string monthlyRate, string maxUsers,
            string maxCategorizationsPerSet, string maxProductsPerSet, string maxProperties, string maxValuesPerProperty, string maxTags,
            bool allowSalesLeads, bool allowLocationData, bool allowCustomOrdering, bool allowThemes, bool allowImageEnhancements, bool basicSupport,
            bool enhancedSupport, string maxImageGroups, string maxImageFormats, string maxImageGalleries, string maxImagesPerGallery, bool visible)
        {
            var response = new AccountPaymentPlanService.DataAccessResponseType();
            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

            #region Validate Input

            if (String.IsNullOrEmpty(paymentPlanName) || String.IsNullOrEmpty(monthlyRate) || String.IsNullOrEmpty(maxUsers) || String.IsNullOrEmpty(maxCategorizationsPerSet) || String.IsNullOrEmpty(maxProductsPerSet))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Not all form fields have a value." };

                return jsonNetResultError;
            }


            decimal monthlyRateDec;

            int maxCategorizationsPerSetInt;
            int maxProductsPerSetInt;

            int maxUsersInt;
            int maxPropertiesInt;
            int maxValuesPerPropertyInt;
            int maxTagsInt;


            int maxImageGroupsInt;
            int maxImageFormatsInt;
            int maxImageGalleriesInt;
            int maxImagesPerGalleryInt;

            if (!decimal.TryParse(monthlyRate, out monthlyRateDec))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Monthly Rate be a valid decimal." };

                return jsonNetResultError;
            }


            if (!int.TryParse(maxProductsPerSet, out maxProductsPerSetInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Max Products Per Set must be a valid integer." };

                return jsonNetResultError;
            }
            if (!int.TryParse(maxCategorizationsPerSet, out maxCategorizationsPerSetInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Max CategorizationsP Per Set must be a valid integer." };

                return jsonNetResultError;
            }


            if (!int.TryParse(maxUsers, out maxUsersInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Max Users must be a valid integer." };

                return jsonNetResultError;
            }

            

            if (!int.TryParse(maxProperties, out maxPropertiesInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Max Properties must be a valid integer." };

                return jsonNetResultError;
            }

            if (!int.TryParse(maxValuesPerProperty, out maxValuesPerPropertyInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Max Values Per Property must be a valid integer." };

                return jsonNetResultError;
            }

            if (!int.TryParse(maxTags, out maxTagsInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Max Tags must be a valid integer." };

                return jsonNetResultError;
            }



            if (!int.TryParse(maxImageGroups, out maxImageGroupsInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Max Image Groups must be a valid integer." };

                return jsonNetResultError;
            }

            if (!int.TryParse(maxImageFormats, out maxImageFormatsInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Max Image Formats must be a valid integer." };

                return jsonNetResultError;
            }

            if (!int.TryParse(maxImageGalleries, out maxImageGalleriesInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Max Image Galleries must be a valid integer." };

                return jsonNetResultError;
            }

            if (!int.TryParse(maxImagesPerGallery, out maxImagesPerGalleryInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Max Images Per Gallery must be a valid integer." };

                return jsonNetResultError;
            }

            #endregion

            try
            {
                /* REMOVED - DONE IN SQL NOW - 
                accountPaymentPlanServiceClient.Open();
                response = accountPaymentPlanServiceClient.CreatePaymentPlan(
                        paymentPlanName,
                        monthlyRateDec,
                        maxUsersInt,
                        maxCategorizationsPerSetInt,
                        maxProductsPerSetInt,
                        maxPropertiesInt,
                        maxValuesPerPropertyInt,
                        maxTagsInt,
                        allowSalesLeads,
                        allowImageEnhancements,
                        allowLocationData,
                        allowCustomOrdering,
                        allowThemes,
                        basicSupport,
                        enhancedSupport,
                        maxImageGroupsInt,
                        maxImageFormatsInt,
                        maxImageGalleriesInt,
                        maxImagesPerGalleryInt,
                        visible,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        PlatformAdminSite.AccountPaymentPlanService.RequesterType.PlatformUser
                    );*/

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

        [Route("Plans/Json/DeletePlan")]
        [HttpPost]
        public JsonNetResult DeletePlan(string paymentPlanName)
        {
            var response = new AccountPaymentPlanService.DataAccessResponseType();
            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

            try
            {
                accountPaymentPlanServiceClient.Open();
                response = accountPaymentPlanServiceClient.DeletePaymentPlan(
                        paymentPlanName,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        PlatformAdminSite.AccountPaymentPlanService.RequesterType.PlatformUser, Common.SharedClientKey
                    );

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

        #region Updates
        /*
        [Route("Plans/Json/UpdatePlanName")]
        [HttpPost]
        public JsonNetResult UpdatePlanName(string paymentPlanName, string newName)
        {
            var response = new AccountPaymentPlanService.DataAccessResponseType();
            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

            try
            {
                accountPaymentPlanServiceClient.Open();
                response = accountPaymentPlanServiceClient.UpdatePlanName(
                        paymentPlanName,
                        newName,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        PlatformAdminSite.AccountPaymentPlanService.RequesterType.PlatformUser
                    );

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

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }


            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
         * jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }
        */

        [Route("Plans/Json/UpdatePlanVisibility")]
        [HttpPost]
        public JsonNetResult UpdatePlanVisibility(string paymentPlanName, bool isVisible)
        {
            var response = new AccountPaymentPlanService.DataAccessResponseType();
            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

            try
            {
                accountPaymentPlanServiceClient.Open();
                response = accountPaymentPlanServiceClient.UpdatePlanVisibility(
                        paymentPlanName,
                        isVisible,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        PlatformAdminSite.AccountPaymentPlanService.RequesterType.PlatformUser, Common.SharedClientKey
                    );

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

        #region Limitatons

        /*
        [Route("Plans/Json/UpdatePlanMaxUsers")]
        [HttpPost]
        public JsonNetResult UpdatePlanMaxUsers(string paymentPlanName, string newLimit)
        {
            int newLimitInt;

            #region Validate limit

            if (!int.TryParse(newLimit, out newLimitInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Must be a valid integer." };

                return jsonNetResultError;
            }
            
            #endregion

            var response = new AccountPaymentPlanService.DataAccessResponseType();
            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

            try
            {
                accountPaymentPlanServiceClient.Open();
                response = accountPaymentPlanServiceClient.UpdatePlanMaxUsers(
                        paymentPlanName,
                        newLimitInt,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        PlatformAdminSite.AccountPaymentPlanService.RequesterType.PlatformUser
                    );

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

        [Route("Plans/Json/UpdatePlanMaxCategories")]
        [HttpPost]
        public JsonNetResult UpdatePlanMaxCategories(string paymentPlanName, string newLimit)
        {
            int newLimitInt;

            #region Validate limit

            if (!int.TryParse(newLimit, out newLimitInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Must be a valid integer." };

                return jsonNetResultError;
            }

            #endregion

            var response = new AccountPaymentPlanService.DataAccessResponseType();
            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

            try
            {
                accountPaymentPlanServiceClient.Open();
                response = accountPaymentPlanServiceClient.UpdatePlanMaxCategories(
                        paymentPlanName,
                        newLimitInt,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        PlatformAdminSite.AccountPaymentPlanService.RequesterType.PlatformUser
                    );

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


        [Route("Plans/Json/UpdatePlanMaxSubcategories")]
        [HttpPost]
        public JsonNetResult UpdatePlanMaxSubcategories(string paymentPlanName, string newLimit)
        {
            int newLimitInt;

            #region Validate limit

            if (!int.TryParse(newLimit, out newLimitInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Must be a valid integer." };

                return jsonNetResultError;
            }

            #endregion

            var response = new AccountPaymentPlanService.DataAccessResponseType();
            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

            try
            {
                accountPaymentPlanServiceClient.Open();
                response = accountPaymentPlanServiceClient.UpdatePlanMaxSubcategories(
                        paymentPlanName,
                        newLimitInt,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        PlatformAdminSite.AccountPaymentPlanService.RequesterType.PlatformUser
                    );

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

        [Route("Plans/Json/UpdatePlanMaxTags")]
        [HttpPost]
        public JsonNetResult UpdatePlanMaxTags(string paymentPlanName, string newLimit)
        {
            int newLimitInt;

            #region Validate limit

            if (!int.TryParse(newLimit, out newLimitInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Must be a valid integer." };

                return jsonNetResultError;
            }

            #endregion

            var response = new AccountPaymentPlanService.DataAccessResponseType();
            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

            try
            {
                accountPaymentPlanServiceClient.Open();
                response = accountPaymentPlanServiceClient.UpdatePlanMaxTags(
                        paymentPlanName,
                        newLimitInt,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        PlatformAdminSite.AccountPaymentPlanService.RequesterType.PlatformUser
                    );

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

        
        [Route("Plans/Json/UpdatePlanMaxImages")]
        [HttpPost]
        public JsonNetResult UpdatePlanMaxApplicationImages(string paymentPlanName, string newLimit)
        {
            int newLimitInt;

            #region Validate limit

            if (!int.TryParse(newLimit, out newLimitInt))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.Data = new AccountPaymentPlanService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Must be a valid integer." };

                return jsonNetResultError;
            }

            #endregion

            var response = new AccountPaymentPlanService.DataAccessResponseType();
            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

            try
            {
                accountPaymentPlanServiceClient.Open();
                response = accountPaymentPlanServiceClient.UpdatePlanMaxImages(
                        paymentPlanName,
                        newLimitInt,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        PlatformAdminSite.AccountPaymentPlanService.RequesterType.PlatformUser
                    );

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
        

        [Route("Plans/Json/UpdatePlanAllowImageEnhancements")]
        [HttpPost]
        public JsonNetResult UpdatePlanAllowImageEnhancements(string paymentPlanName, bool allowImageEnhancements)
        {
            var response = new AccountPaymentPlanService.DataAccessResponseType();
            var accountPaymentPlanServiceClient = new AccountPaymentPlanService.AccountPaymentPlanServiceClient();

            try
            {
                accountPaymentPlanServiceClient.Open();
                response = accountPaymentPlanServiceClient.UpdatePlanAllowImageEnhancements(
                        paymentPlanName,
                        allowImageEnhancements,
                        AuthenticationCookieManager.GetAuthenticationCookie().Id,
                        PlatformAdminSite.AccountPaymentPlanService.RequesterType.PlatformUser
                    );

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
*/

        #endregion

        #region Accout Plan/Subscription Management

        [Route("Plans/Json/SubscribeAccount")]
        [HttpPost]
        public JsonNetResult Subscribe(string accountId, string planName, string frequencyMonths, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear)
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
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.PlatformUser,
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

        /// <summary>
        /// Checks to see if there is an upgrade path available for an account plan
        /// </summary>
        /// <param name="planName"></param>
        /// <returns></returns>
        [Route("Plans/Json/UpgradePathAvailable")]
        [HttpPost]
        public JsonNetResult UpgradePathAvailable(string planName)
        {
            var response = new AccountPaymentPlanService.DataAccessResponseType();

            List<AccountPaymentPlanService.PaymentPlan> paymentPlans = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "paymentplans";
                string hashField = "list:True:True";

                var redisValue = cache.HashGet(hashKey, hashField);

                //con.Close();

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

                    paymentPlans = accountPaymentPlanServiceClient.GetPaymentPlans(true, true).ToList();

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

            response.isSuccess = false;
            response.ErrorMessage = "No upgrade paths available for this plan.";

            AccountPaymentPlanService.PaymentPlan planBeingChecked = null;

            try
            {
                planBeingChecked = paymentPlans.First(p => p.PaymentPlanName.ToLower() == planName.ToLower());
            }
            catch
            {
            }
            

            if(planBeingChecked != null)
            {
                foreach (var plan in paymentPlans)
                {
                    if (plan.MonthlyRate > planBeingChecked.MonthlyRate && plan.SearchPlan == planBeingChecked.SearchPlan)
                    {
                        response.isSuccess = true;
                        response.ErrorMessage = null;
                        response.SuccessMessage += plan.PaymentPlanName + "|";
                    }
                }

                if (response.isSuccess)
                {
                    response.SuccessMessage = "Can upgrade to: " + response.SuccessMessage;
                    var lastIndex = response.SuccessMessage.LastIndexOf("|");
                    response.SuccessMessage = response.SuccessMessage.Remove(lastIndex, 1);
                    
                }
            }



            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Plans/Json/UpdateAccountPlan")]
        [HttpPost]
        public JsonNetResult UpdatePlan(string accountId, string planName, string frequencyMonths)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                //Get the ip address and call origin
                var ipAddress = Request.UserHostAddress;
                var origin = "Web";

                accountManagementServiceClient.Open();

                response = accountManagementServiceClient.UpdateAccountPlan(
                    accountId,
                    planName,
                    frequencyMonths,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.PlatformUser,
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