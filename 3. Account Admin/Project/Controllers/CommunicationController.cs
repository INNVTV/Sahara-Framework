using AccountAdminSite.AccountCommunicationService;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
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

        #region Mark Notifications 'Read'

        [Route("Communication/Notifications/Json/MarkNotificationRead/")]
        [HttpPost]
        public JsonNetResult MarkNotificationRead(string notificationType, string notificationId)
        {
            var accountCommunicationServiceClient = new AccountCommunicationService.AccountCommunicationServiceClient();
            
            try
            {
                accountCommunicationServiceClient.Open();

                //Convert notification type to Enum
                NotificationType _convertedNotificationType = (NotificationType)Enum.Parse(typeof(NotificationType), notificationType);

                var response = accountCommunicationServiceClient.UpdateNotificationStatus(
                    _convertedNotificationType,
                    NotificationStatus.Read,
                    "Unread",
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    notificationId, Common.SharedClientKey
                    );

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

                JsonNetResult jsonNetErrorResult = new JsonNetResult();
                jsonNetErrorResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetErrorResult.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = WCFManager.UserFriendlyExceptionMessage };

                return jsonNetErrorResult;
            }

        }

        #endregion

        #region Get Notifications

        [Route("Communication/Notifications/Json/GetNotifications/{NotificationStatus}")]
        [HttpGet]
        public JsonNetResult GetNotifications(string notificationStatus)
        {
            var userId = AuthenticationCookieManager.GetAuthenticationCookie().Id;
            var accountCommunicationServiceClient = new AccountCommunicationService.AccountCommunicationServiceClient();

            try
            {                
                accountCommunicationServiceClient.Open();

                //Convert notification status to Enum
                NotificationStatus _convertedNotificationStatus = (NotificationStatus)Enum.Parse(typeof(NotificationStatus), notificationStatus);

                var userNotifications = accountCommunicationServiceClient.GetAccountUserNotifications(_convertedNotificationStatus, userId, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountCommunicationServiceClient);

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = userNotifications;

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

                JsonNetResult jsonNetErrorResult = new JsonNetResult();
                jsonNetErrorResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetErrorResult.Data = WCFManager.UserFriendlyExceptionMessage;

                return jsonNetErrorResult;
            }

        }

        [Route("Communication/Notifications/Json/GetNotificationsByType/{NotificationType}/{NotificationStatus}")]
        [HttpGet]
        public JsonNetResult GetNotificationsByType(string notificationType, string notificationStatus)
        {

            var userId = AuthenticationCookieManager.GetAuthenticationCookie().Id;
            var accountCommunicationServiceClient = new AccountCommunicationService.AccountCommunicationServiceClient();

            try
            {        
                accountCommunicationServiceClient.Open();

                //Convert notification type to Enum
                NotificationType _convertedNotificationType = (NotificationType)Enum.Parse(typeof(NotificationType), notificationType);

                //Convert notification status to Enum
                NotificationStatus _convertedNotificationStatus = (NotificationStatus)Enum.Parse(typeof(NotificationStatus), notificationStatus);

                var userNotifications = accountCommunicationServiceClient.GetAccountUserNotificationsByType(_convertedNotificationType, _convertedNotificationStatus, userId, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountCommunicationServiceClient);

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = userNotifications;

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

                JsonNetResult jsonNetErrorResult = new JsonNetResult();
                jsonNetErrorResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetErrorResult.Data = WCFManager.UserFriendlyExceptionMessage;

                return jsonNetErrorResult;
            }

        }


        #endregion

        #endregion

        #endregion
    }
}