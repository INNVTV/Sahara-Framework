using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts.Notifications.Models;
using Sahara.Core.Accounts.Notifications.TableEntities;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Accounts.Notifications.Internal
{
    internal static class NotificationsTableManager
    {
        //NOTIFICATIONS OFF BY DEFAULT. UNCOMMENT TO TURN ON (Requires USERS table storage account)

        /*

        #region Create

        internal static bool StoreNotification(NotificationType notificationType, string userId, string notificationMessage, double expirationMinutes)
        {
            try
            {
                var notificationEntity = new NotificationTableEntity(userId, notificationType.ToString());
                notificationEntity.Message = notificationMessage;
                notificationEntity.ExpirationMinutes = expirationMinutes;

                if (expirationMinutes > 0)
                {
                    notificationEntity.ExpirationDateTime = DateTime.UtcNow.AddMinutes(expirationMinutes);
                }
                else
                {
                    notificationEntity.ExpirationDateTime = DateTime.MaxValue;
                }

                TableOperation operation = TableOperation.Insert((notificationEntity as TableEntity));
                notificationEntity.cloudTable.Execute(operation);

                return true;
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to store notifications into table storage for: " + userId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return false;
            }
            
        }

        #endregion

        #region Get

        internal static IEnumerable<NotificationTableEntity> GetNotifications(NotificationType notificationType, NotificationStatus notificationStatus, string userId)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.UsersStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.UserIdToTableStorageName(userId) + "Notifications" + notificationType.ToString());

            cloudTable.CreateIfNotExists();

            TableQuery<NotificationTableEntity> query = new TableQuery<NotificationTableEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, notificationStatus.ToString()));

            var notificationEntities = cloudTable.ExecuteQuery(query);

            return notificationEntities;

        }


        internal static NotificationTableEntity GetNotification(NotificationType notificationType, string userId, string partitionKey, string rowKey)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.UsersStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.UserIdToTableStorageName(userId) + "Notifications" + notificationType.ToString());

            cloudTable.CreateIfNotExists();

            string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            string rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);

            TableQuery<NotificationTableEntity> query = new TableQuery<NotificationTableEntity>()
                .Where(TableQuery.CombineFilters(partitionFilter, TableOperators.And, rowKeyFilter));


            NotificationTableEntity notificationEntity = cloudTable.ExecuteQuery(query).FirstOrDefault();

            return notificationEntity;

        }


        #endregion

        #region Update

        internal static bool UpdateNotificationStatus(NotificationType notificationType, NotificationStatus newStatus, string userId, string partitionKey, string rowKey)
        {
            try
            {
                CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.UsersStorage.CreateCloudTableClient();
                //Create and set retry policy
                //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
                IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 6);
                cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


                CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.UserIdToTableStorageName(userId) +"Notifications" + notificationType.ToString());

                cloudTable.CreateIfNotExists();

                var notificationEntity = GetNotification(notificationType, userId, partitionKey, rowKey); //<-- Get
                var updatedEntity = notificationEntity;

                cloudTable.Execute(TableOperation.Delete(notificationEntity)); //<-- Delete origional row

                updatedEntity.Status = newStatus.ToString(); //<-- Update
                

                cloudTable.Execute(TableOperation.Insert(updatedEntity)); //<-- Insert

                return true;
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to update notification status for: " + userId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return false;
            }

        }


        #endregion

        #region Delete

        /// <summary>
        /// Used when deleting a user or account
        /// </summary>
        /// <param name="notificationType"></param>
        /// <param name="userId"></param>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        internal static bool ClearUserTable(NotificationType notificationType, string userId)
        {
            try
            {
                CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.UsersStorage.CreateCloudTableClient();

                //Create and set retry policy
                //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
                IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 6);
                cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

                CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.UserIdToTableStorageName(userId) + "Notifications" + notificationType.ToString());

                cloudTable.DeleteIfExists();

                return true;
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to clear all " + notificationType.ToString() + " notification table for: " + userId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return false;
            }

        }


        #endregion

*/
    }
}
