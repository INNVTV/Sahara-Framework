using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts.Notifications.Models;
using Sahara.Core.Accounts.Notifications.TableEntities;

namespace Sahara.Core.Accounts.Notifications.Internal
{
    internal static class Transformations
    {
        /* NOTIFICATIONS OFF
        public static UserNotification TransformToUserNotification(NotificationTableEntity notificationTableEntity, NotificationType notificatonType)
        {
            UserNotification notification = null;

            if (notificationTableEntity != null)
            {
                notification = new UserNotification
                {
                    NotificationType = notificatonType.ToString(),
                    NotificationId = notificationTableEntity.RowKey,
                    NotificationMessage = notificationTableEntity.Message,
                    Status = notificationTableEntity.Status,

                    ExpirationMinutes = notificationTableEntity.ExpirationMinutes,
                    ExpirationDateTime = notificationTableEntity.ExpirationDateTime,
                    LastUpdatedDateTime = notificationTableEntity.Timestamp.UtcDateTime,
                    CreatedDateTime = notificationTableEntity.CreatedDateTime
                    
                };
            }
            return notification;
        }
*/

    }
}
