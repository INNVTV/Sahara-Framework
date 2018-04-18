using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts.Notifications.Models;
using Sahara.Core.Platform.Requests.Models;
using Sahara.Core.Common.ResponseTypes;

namespace WCF.WcfEndpoints.Contracts.Account
{
    [ServiceContract]
    public interface IAccountCommunicationService
    {
       /*==================================================================================
       * NOTIFICATIONS (OFF)
       ==================================================================================*/

        //Get -----

        [OperationContract]
        List<UserNotification> GetAccountUserNotifications(NotificationStatus notificationStatus, string userId, string sharedClientKey);

        [OperationContract]
        List<UserNotification> GetAccountUserNotificationsByType(NotificationType notificationType, NotificationStatus notificationStatus, string userId, string sharedClientKey);


        //Update -----

        [OperationContract]
        DataAccessResponseType UpdateNotificationStatus(NotificationType notificationType, NotificationStatus notificationStatus, string currentStatus, string userId, string notificationMessageId, string sharedClientKey);



        //Send -----

        [OperationContract]
        DataAccessResponseType SendNotificationToUser(NotificationType notificationType, string userId, string notificationMessage, double expirationMinutes, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType SendNotificationToAccount(NotificationType notificationType, string accountId, string notificationMessage, double expirationMinutes, bool accountOwnersOnly, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType SendNotificationToBulkAccounts(NotificationType notificationType, string notificationMessage, double expirationMinutes, bool accountOwnersOnly, string columnName, string columnValue, string requesterId, RequesterType requesterType, string sharedClientKey);

          


        /*==================================================================================
         * EMAILS
         ==================================================================================*/

        //Send -----

        [OperationContract]
        DataAccessResponseType SendEmailToUser(string userId, string fromName, string fromEmail, string emailSubject, string emailMessage, bool isImportant, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType SendEmailToAccount(string accountId, string fromName, string fromEmail, string emailSubject, string emailMessage, bool isImportant, bool accountOwnersOnly, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType SendEmailToBulkAccounts(string fromName, string fromEmail, string emailSubject, string emailMessage, bool isImportant, bool accountOwnersOnly, string columnName, string columnValue, string requesterId, RequesterType requesterType, string sharedClientKey);

        /*==================================================================================
         * MESSAGES
         ==================================================================================*/

        //Placeholder for future account-to-account or user-to-user messaging

        /*==================================================================================
         * INSTANT MESSAGES
         ==================================================================================*/

        //Placeholder for future account-to-account or user-to-user instant messaging
    }



}
