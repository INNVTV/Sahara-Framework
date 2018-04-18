using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts.Notifications.Internal;
using Sahara.Core.Accounts.Notifications.TableEntities;
using System.Runtime.Serialization;
using Sahara.Core.Accounts.Notifications.Models;
using Sahara.Core.Common.Methods;
using Sahara.Core.Common.ResponseTypes;
using StackExchange.Redis;
using Sahara.Core.Common.Redis;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Newtonsoft.Json;
using Sahara.Core.Logging.PlatformLogs.Helpers;

namespace Sahara.Core.Accounts.Notifications
{
    /// <summary>
    /// Notifications are sorted by dateTimeTicks+GUID in the RowKey per Partiton Key.
    /// Meaning each partition of "read", "unread", "expired-unread", etc... is sorted spereratly
    /// If you pull more than one partition, ordering will be based on partitions FIRST!
    /// Always best to focus on ONE type at a time.
    /// </summary>
    public static class NotificationsManager
    {

        /* Notifications removed. 

        #region Notifications

        #region Send

        public static DataAccessResponseType SendNotificationToBulkAccounts(NotificationType notificationType, string notificationMessage, double expirationMinutes, bool accountOwnersOnly, string columnName, string columnValue)
        {

            #region Validate Parameters

            if(String.IsNullOrEmpty(notificationMessage))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include a notification message." };
            }

            #endregion  

            try
            {
                //strip out commas in messages, replace with URL encoded value. This is to allow for the worker to properly use the comma delimited message parameters on the other side
                notificationMessage = notificationMessage.Replace(",", "%2C");

                //We offload this task for the Worker to process
                Sahara.Core.Common.MessageQueues.PlatformPipeline.PlatformQueuePipeline.SendMessage.SendNotificationToBulkAccounts(notificationMessage, notificationType.ToString(), expirationMinutes, accountOwnersOnly, columnName, columnValue);

                return new DataAccessResponseType { isSuccess = true };

            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to send notifications to bulk accounts: " + notificationMessage,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }

        }

        public static DataAccessResponseType SendNotificationToAccount(NotificationType notificationType, string accountId, string notificationMessage, double expirationMinutes = 0, bool accountOwnersOnly = true)
        {

            #region Validate Parameters

            if (String.IsNullOrEmpty(notificationMessage))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include a notification message." };
            }
            if (String.IsNullOrEmpty(accountId))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include an accountId." };
            }

            #endregion  


            var userIds = new List<string>();
            bool reponse = false;

            try
            {

                if (accountOwnersOnly)
                {
                    userIds = AccountManager.GetAccountOwnerIds(accountId);
                }
                else
                {
                    userIds = AccountManager.GetAccountUserIds(accountId);
                }

                foreach (string userId in userIds)
                {
                    reponse = NotificationsTableManager.StoreNotification(notificationType, userId, notificationMessage, expirationMinutes);
                    Caching.ClearAllAssocitedCaches(userId, notificationType.ToString());
                }

                return new DataAccessResponseType { isSuccess = reponse };
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to send notification to account: " + accountId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }

        }

        public static DataAccessResponseType SendNotificationToUser(NotificationType notificationType, string userId, string notificationMessage, double expirationMinutes)
        {

            #region Validate Parameters

            if (String.IsNullOrEmpty(notificationMessage))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include a notification message." };
            }
            if (String.IsNullOrEmpty(userId))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include a UserId." };
            }

            #endregion  


            try
            {
                NotificationsTableManager.StoreNotification(notificationType, userId, notificationMessage, expirationMinutes);
                Caching.ClearAllAssocitedCaches(userId, notificationType.ToString());

                return new DataAccessResponseType { isSuccess = true };
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to send notification to user: " + userId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }

        }

        #endregion

        #region Get

        /// <summary>
        /// Merges all notification types in order of priority (Dunning, Alert, Warning, Default)
        /// Mostly used as a single method to get all unread notifications with alerts at the top pf the list
        /// Leverages the caching scenarios of the underlying methods
        /// </summary>
        /// <param name="notificationStatus"></param>
        /// <param name="userId"></param>
        /// <param name="useCachedVersion"></param>
        /// <returns></returns>
        public static List<UserNotification> GetNotifications(NotificationStatus notificationStatus, string userId, bool useCachedVersion = true)
        {
            var notifications = new List<UserNotification>();

            //Get notifications from Cache, in order of priority:

            notifications.AddRange(GetNotificationsByType(NotificationType.Alert, notificationStatus, userId, useCachedVersion));
            notifications.AddRange(GetNotificationsByType(NotificationType.Warning, notificationStatus, userId, useCachedVersion));
            notifications.AddRange(GetNotificationsByType(NotificationType.Success, notificationStatus, userId, useCachedVersion));
            notifications.AddRange(GetNotificationsByType(NotificationType.Information, notificationStatus, userId, useCachedVersion));


            return notifications;

        }

        public static List<UserNotification> GetNotificationsByType(NotificationType notificationType, NotificationStatus notificationStatus, string userId, bool useCachedVersion = true)
        {
            var notifications = new List<UserNotification>();

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            List<UserNotification> cachedNotifications = null;

            if (useCachedVersion)
            {
                //Retreive Messages from Cache:

                try
                {
                    var redisValue = cache.HashGet(
                            UserHash.Key(userId),
                            UserHash.Fields.Notifications(notificationType.ToString(), notificationStatus.ToString())
                        );
 
                    if (redisValue.HasValue)
                    {
                        cachedNotifications = JsonConvert.DeserializeObject<List<UserNotification>>(redisValue);
                    }
                }
                catch
                {

                }

            }

            if (cachedNotifications == null)
            {
                notifications = ScrubExpiredAndTransformNotifications(NotificationsTableManager.GetNotifications(notificationType, notificationStatus, userId), notificationType, userId);

                try
                {
                    //Store Messages in Redis UserHash
                    cache.HashSet(
                            UserHash.Key(userId),
                            UserHash.Fields.Notifications(notificationType.ToString(), notificationStatus.ToString()),
                            JsonConvert.SerializeObject(notifications),
                            When.Always,
                            CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }
            }
            else
            {
                bool dataChanged = false;
                //Messages are in cache, cast returned object as Account, update settings and return:
                notifications = ScrubExpiredNotifications((List<UserNotification>)cachedNotifications, notificationType, userId, out dataChanged); //< -- Scrub for Expired notifications

                if (dataChanged)
                {
                    try
                    {
                        //If data was changed, clear caches and store scrubbed data into the cache
                        Caching.ClearAllAssocitedCaches(userId, notificationType.ToString());
                    
                        // If data has been scrubbed, store scrubbed version back into cache
                        cache.HashSet(
                            UserHash.Key(userId),
                            UserHash.Fields.Notifications(notificationType.ToString(), notificationStatus.ToString()),
                            JsonConvert.SerializeObject(notifications),
                            When.Always,
                            CommandFlags.FireAndForget
                        );
                    }
                    catch
                    {

                    }
                }
            }

            return notifications;
        }

        #endregion


        /*
        #region Clear

        /// <summary>
        /// Used for clearing all unread notifications of a particular type for all users on an account.
        /// Handy for recalling alerts and other timely issues once those issues have been resolved.
        /// Mostly for clearing Dunnings to all users once a lapsed subscription is back on track)
        /// </summary>
        /// <param name="notificationType"></param>
        /// <param name="accountId"></param>
        /// <param name="notificationMessage"></param>
        /// <param name="expirationMinutes"></param>
        /// <param name="accountOwnersOnly"></param>
        /// <returns></returns>
        public static DataAccessResponseType ClearAllUnreadNotificationsForAccount(NotificationType notificationType, string accountId)
        {
            var userIds = new List<string>();
            bool reponse = false;

            try
            {
                userIds = AccountManager.GetAccountUserIds(accountId);
                
                foreach (string userId in userIds)
                {
                    //Get all UNREAD BillingAlerts for each user in this account
                    var userNotifications = GetNotificationsByType(notificationType, NotificationStatus.Unread, userId, false);

                    //Update to ClearedUnread
                    foreach (UserNotification notification in userNotifications)
                    {
                        UpdateNotificationStatus(notificationType, NotificationStatus.ClearedUnread, NotificationStatus.Unread.ToString(), userId, notification.NotificationId);
                    }

                    Caching.ClearAllAssocitedCaches(userId, notificationType.ToString());
                }

                return new DataAccessResponseType { isSuccess = reponse };
            }
            catch (Exception e)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }

        }

        #endregion * /

        #region Update Status

        public static DataAccessResponseType UpdateNotificationStatus(NotificationType notificationType, NotificationStatus notificationStatus, string currentStatus, string userId, string rowKey)
        {

            try
            {
                var result = Internal.NotificationsTableManager.UpdateNotificationStatus(notificationType, notificationStatus, userId, currentStatus, rowKey);

                //Clear all associated caches:
                if (result)
                {
                    result = Caching.ClearAllAssocitedCaches(userId, notificationType.ToString());
                }

                return new DataAccessResponseType { isSuccess = result };
            }
            catch (Exception e)
            {
                //Since a user may mark a notification as "read" twice - we silently fail on this and do not log the exception
                //Log exception and email platform admins
                /*
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to update notification status for: " + userId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );
                * /
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }
            
        }


        #endregion

        #region Delete

        /// <summary>
        /// Used when deleting User/Account
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static DataAccessResponseType DeleteAllNotificationsForUser(NotificationType notificationType, string userId)
        {
            try
            {
                Caching.ClearAllAssocitedCaches(userId, notificationType.ToString());
                if(Internal.NotificationsTableManager.ClearUserTable(notificationType, userId))
                {
                    return new DataAccessResponseType { isSuccess = true };
                }
                else
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "An unknown error occured while trying to clear the users notification table." };
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to delete all " + notificationType.ToString() + " notifications for: " + userId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }
            
        }

        #endregion

        #endregion

        #region Helper Methods

        internal static List<UserNotification> ScrubExpiredNotifications(List<UserNotification> notifications, NotificationType notificationType, string userId, out bool dataChanged)
        {
            dataChanged = false;

            var userNotificationsToRemove = new List<UserNotification>(); //<-- YOu cannot remove from a list being used in the foreach proess so we add to this and process after

            //Check if any cached notifications are now expired: <-- This should only run if an item becomes expired during the caching period, otherwise it is caught on the first non-cached request
            foreach (UserNotification userNotification in notifications)
            {
                if (userNotification.ExpirationMinutes > 0
                    && userNotification.Status != NotificationStatus.ExpiredRead.ToString()
                    && userNotification.Status != NotificationStatus.ExpiredUnread.ToString())
                {
                    if (userNotification.ExpirationDateTime < DateTime.UtcNow)
                    {
                        //Cached notification is expired, remove from list and mark as such
                        //notifications.Remove(userNotification);

                        //Cached notification is expired, add to the removal list 
                        userNotificationsToRemove.Add(userNotification);

                        //Determine which expiration type to use, and mark it as such (skip if none)
                        if (userNotification.Status == NotificationStatus.Read.ToString())
                        {
                            NotificationsTableManager.UpdateNotificationStatus(notificationType, NotificationStatus.ExpiredRead, userId, userNotification.Status, userNotification.NotificationId);
                        }
                        else if (userNotification.Status == NotificationStatus.Unread.ToString())
                        {
                            NotificationsTableManager.UpdateNotificationStatus(notificationType, NotificationStatus.ExpiredUnread, userId, userNotification.Status, userNotification.NotificationId);
                        }

                        //update output to reflect that data has been changed:
                        dataChanged = true;
                    }
                }
            }

            foreach (UserNotification userNotificationToRemove in userNotificationsToRemove)
            {
                notifications.Remove(userNotificationToRemove);
            }

            return notifications;
        }

        internal static List<UserNotification> ScrubExpiredAndTransformNotifications(IEnumerable<NotificationTableEntity> notificationTableEntities, NotificationType notificationType, string userId)
        {
            var notifications = new List<UserNotification>();

            foreach (var notificationEntity in notificationTableEntities)
            {

                if (notificationEntity.ExpirationMinutes > 0
                    && notificationEntity.Status != NotificationStatus.ExpiredRead.ToString()
                    && notificationEntity.Status != NotificationStatus.ExpiredUnread.ToString())
                {
                    //Notification has expiration settings, determine if expired and mark appropriatly:
                    if (notificationEntity.ExpirationDateTime < DateTime.UtcNow)
                    {

                        //Determine which expiration type to use, and mark it as such (skip if none)
                        if (notificationEntity.Status == NotificationStatus.Read.ToString())
                        {
                            NotificationsTableManager.UpdateNotificationStatus(notificationType, NotificationStatus.ExpiredRead, userId, notificationEntity.PartitionKey, notificationEntity.RowKey);
                        }
                        else if (notificationEntity.Status == NotificationStatus.Unread.ToString())
                        {
                            NotificationsTableManager.UpdateNotificationStatus(notificationType, NotificationStatus.ExpiredUnread, userId, notificationEntity.PartitionKey, notificationEntity.RowKey);
                        }
                    }
                    else
                    {
                        //Notification is not yet expired, add to results list:
                        notifications.Add(Transformations.TransformToUserNotification(notificationEntity, notificationType));
                    }
                }
                else
                {
                    //Notification has no expiration, add to results list:
                    notifications.Add(Transformations.TransformToUserNotification(notificationEntity, notificationType));
                }

            }

            return notifications;
        }

        internal static List<UserNotification> TransformNotifications(IEnumerable<NotificationTableEntity> notificationTableEntities, NotificationType notificationType, string userId)
        {
            

            var notifications = new List<UserNotification>();

            foreach (var notificationEntity in notificationTableEntities)
            {
                notifications.Add(Transformations.TransformToUserNotification(notificationEntity, notificationType));
            }

            return notifications;
        }

        #endregion
*/

    }

}

