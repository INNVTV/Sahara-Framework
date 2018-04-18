using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PlatformAdminSite.AccountCommunicationService;
using System.ServiceModel;
using Newtonsoft.Json;

namespace PlatformAdminSite.Controllers
{
    public class CommunicationController : Controller
    {
        // GET: Communication
        public ActionResult Index()
        {
            return View();
        }


        #region Notifications

        #region JSON Methods

        #region Send Notifications

        [Route("Communication/Notifications/Json/SendNotificationToUser")]
        [HttpPost]
        public JsonNetResult SendNotificationToUser(string notificationType, string userId, string notificationMessage, string expirationMinutes)
        {
            double minutesConverted = 0;

            if (Double.TryParse(expirationMinutes, out minutesConverted))
            {
            }
            else
            {

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = "Expiration minutes must be a valid double." };
            }

            var accountCommunicationServiceClient = new AccountCommunicationService.AccountCommunicationServiceClient();
            
            try
            {
                accountCommunicationServiceClient.Open();

                //Convert notification type to Enum
                NotificationType _convertedMesageType = (NotificationType)Enum.Parse(typeof(NotificationType), notificationType);

                var response = accountCommunicationServiceClient.SendNotificationToUser(_convertedMesageType, userId, notificationMessage, minutesConverted,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.AccountCommunicationService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountCommunicationServiceClient);

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.Data = response;

                return jsonNetResult;
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountCommunicationServiceClient, exceptionMessage, currentMethodString);

                #endregion

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = WCFManager.UserFriendlyExceptionMessage };

                return jsonNetResult;
            }

        }

        [Route("Communication/Notifications/Json/SendNotificationToAccount")]
        [HttpPost]
        public JsonNetResult SendNotificationToAccount(string notificationType, string accountId, string notificationMessage, string expirationMinutes, bool accountOwnersOnly)
        {
            double minutesConverted = 0;

            if (Double.TryParse(expirationMinutes, out minutesConverted))
            {
            }
            else
            {

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = "Expiration minutes must be a valid double." };
            }

            var accountCommunicationServiceClient = new AccountCommunicationService.AccountCommunicationServiceClient();
            
            try
            {
                accountCommunicationServiceClient.Open();

                //Convert notification type to Enum
                NotificationType _convertedMesageType = (NotificationType)Enum.Parse(typeof(NotificationType), notificationType);

                var response = accountCommunicationServiceClient.SendNotificationToAccount(_convertedMesageType, accountId, notificationMessage, minutesConverted, accountOwnersOnly,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.AccountCommunicationService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountCommunicationServiceClient);

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = response;

                return jsonNetResult;
            }
            catch (Exception e)
            {

                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountCommunicationServiceClient, exceptionMessage, currentMethodString);

                #endregion


                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = WCFManager.UserFriendlyExceptionMessage };

                return jsonNetResult;
            }
        }

        [Route("Communication/Notifications/Json/SendNotificationToBulkAccounts")]
        [HttpPost]
        public JsonNetResult SendNotificationToBulkAccounts(string notificationType, string notificationMessage, string expirationMinutes, bool accountOwnersOnly, string columnName, string columnValue)
        {
            double minutesConverted = 0;

            if (Double.TryParse(expirationMinutes, out minutesConverted))
            {
            }
            else{

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = "Expiration minutes must be a valid double." };

                return jsonNetResult;
            }

            var accountCommunicationServiceClient = new AccountCommunicationService.AccountCommunicationServiceClient();
            
            try
            {
                accountCommunicationServiceClient.Open(); 

                //Convert notification type to Enum
                NotificationType _convertedMesageType = (NotificationType)Enum.Parse(typeof(NotificationType), notificationType);

                var response = accountCommunicationServiceClient.SendNotificationToBulkAccounts(_convertedMesageType, notificationMessage, minutesConverted, accountOwnersOnly, columnName, columnValue,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.AccountCommunicationService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountCommunicationServiceClient);

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = response;

                return jsonNetResult;
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountCommunicationServiceClient, exceptionMessage, currentMethodString);

                #endregion

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = WCFManager.UserFriendlyExceptionMessage};

                return jsonNetResult;
            }
        }

        #endregion

        #region Get Notifications

        [Route("Communication/Notifications/Json/GetUserNotifications/{NotificationStatus}/{UserId}")]
        [HttpGet]
        public JsonNetResult GetUserNotifications(string notificationStatus, string userId)
        {
            var notifications = new List<UserNotification>();
            var accountCommunicationServiceClient = new AccountCommunicationService.AccountCommunicationServiceClient();

            try
            {
                accountCommunicationServiceClient.Open();

                //Convert notification status to Enum
                NotificationStatus _convertedNotificationStatus = (NotificationStatus)Enum.Parse(typeof(NotificationStatus), notificationStatus);

                notifications = accountCommunicationServiceClient.GetAccountUserNotifications(_convertedNotificationStatus, userId, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountCommunicationServiceClient);

            }
            catch (Exception e)
            {

                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountCommunicationServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = notifications;

            return jsonNetResult;

        }

        [Route("Communication/Notifications/Json/GetUserNotificationsByType/{NotificationType}/{NotificationStatus}/{UserId}")]
        [HttpGet]
        public JsonNetResult GetUserNotificationsByType(string notificationType, string notificationStatus, string userId)
        {
            var notifications = new List<UserNotification>();
            var accountCommunicationServiceClient = new AccountCommunicationService.AccountCommunicationServiceClient();

            try
            {
                accountCommunicationServiceClient.Open();

                //Convert notification type to Enum
                NotificationType _convertedNotificationType = (NotificationType)Enum.Parse(typeof(NotificationType), notificationType);

                //Convert notification status to Enum
                NotificationStatus _convertedNotificationStatus = (NotificationStatus)Enum.Parse(typeof(NotificationStatus), notificationStatus);

                notifications = accountCommunicationServiceClient.GetAccountUserNotificationsByType(_convertedNotificationType, _convertedNotificationStatus, userId, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountCommunicationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountCommunicationServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = notifications;

            return jsonNetResult;
        }


        #endregion

        #endregion

        #endregion
    }
}