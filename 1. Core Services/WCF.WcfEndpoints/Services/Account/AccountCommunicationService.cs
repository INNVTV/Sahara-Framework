using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts.Notifications;
using Sahara.Core.Platform.Requests.Models;
using WCF.WcfEndpoints.Contracts.Account;
using Sahara.Core.Accounts.Notifications.TableEntities;
using Sahara.Core.Accounts.Notifications.Models;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Accounts;

namespace WCF.WcfEndpoints.Service.Account
{
    public class AccountCommunicationService : WCF.WcfEndpoints.Contracts.Account.IAccountCommunicationService
    {
        #region NOTIFICATIONS (OFF)

        

        #region Get

        public List<UserNotification> GetAccountUserNotifications(NotificationStatus notificationMessageStatus, string userId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //return NotificationsManager.GetNotifications(notificationMessageStatus, userId);
            return null;
        }

        public List<UserNotification> GetAccountUserNotificationsByType(NotificationType notificationMessageType, NotificationStatus notificationMessageStatus, string userId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //return NotificationsManager.GetNotificationsByType(notificationMessageType, notificationMessageStatus, userId);
            return null;
        }

        #endregion


        #region Update

        public DataAccessResponseType UpdateNotificationStatus(NotificationType notificationMessageType, NotificationStatus notificationMessageStatus, string currentStatus, string userId, string notificationMessageId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //return NotificationsManager.UpdateNotificationStatus(notificationMessageType, notificationMessageStatus, currentStatus, userId, notificationMessageId);
            return null;
        }

        #endregion


        #region Send

        public DataAccessResponseType SendNotificationToUser(NotificationType notificationMessageType, string userId, string notificationMessage, double expirationMinutes, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                null); //<-- Only PlatformUsers can send notifications

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }
            #endregion

            //return NotificationsManager.SendNotificationToUser(notificationMessageType, userId, notificationMessage, expirationMinutes);
            return null;
        }

        public DataAccessResponseType SendNotificationToAccount(NotificationType notificationMessageType, string accountId, string notificationMessage, double expirationMinutes, bool accountOwnersOnly, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                null); //<-- Only PlatformUsers can send notifications

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            //return NotificationsManager.SendNotificationToAccount(notificationMessageType, accountId, notificationMessage, expirationMinutes, accountOwnersOnly);
            return null;
        }

        public DataAccessResponseType SendNotificationToBulkAccounts(NotificationType notificationMessageType, string notificationMessage, double expirationMinutes, bool accountOwnersOnly, string columnName, string columnValue, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null); //<-- Only PlatformUsers can send notifications

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            //return NotificationsManager.SendNotificationToBulkAccounts(notificationMessageType, notificationMessage, expirationMinutes, accountOwnersOnly, columnName, columnValue);
            return null;
        }

        #endregion

        #endregion

        #region EMAILS

        #region Send

        public DataAccessResponseType SendEmailToUser(string userId, string fromName, string fromEmail, string emailSubject, string emailMessage, bool isImportant, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                null); //<-- Only PlatformUsers can send notifications

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            return AccountUserManager.SendEmailToUser(userId, fromName, fromEmail, emailSubject, emailMessage, isImportant);
        }

        public DataAccessResponseType SendEmailToAccount(string accountId, string fromName, string fromEmail, string emailSubject, string emailMessage, bool isImportant, bool accountOwnersOnly, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                null); //<-- Only PlatformUsers can send notifications

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            return AccountManager.SendEmailToAccount(accountId, fromEmail, fromName, emailSubject, emailMessage, accountOwnersOnly, isImportant);
        }

        public DataAccessResponseType SendEmailToBulkAccounts(string fromName, string fromEmail, string emailSubject, string emailMessage, bool isImportant, bool accountOwnersOnly, string columnName, string columnValue, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null); //<-- Only PlatformUsers can send notifications

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            return AccountManager.SendEmailToBulkAccounts(fromEmail, fromName, emailSubject, emailMessage, accountOwnersOnly, isImportant, columnName, columnValue);
        }

        #endregion
         
        #endregion

        #region MESSAGES

        //Placeholder for future account-to-account or user-to-user messaging

        #endregion

        #region INSTANT MESSAGES

        //Placeholder for future account-to-account or user-to-user instant messaging

        #endregion

    }
}
