using Sahara.Core.Accounts.Notifications.Models;
using Sahara.Core.Common.Methods;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Notifications.Internal
{
    internal static class Caching
    {
        public static bool ClearAllAssocitedCaches(string userId, string noificationType)
        {
            try
            {
                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();

                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                string userKey = UserHash.Key(userId);

                /*
                string allNotificationsUnread = UserHash.Fields.Notifications(NotificationStatus.Unread.ToString());
                string allNotificationsRead = UserHash.Fields.Notifications(NotificationStatus.Read.ToString());
                string allNotificationsExpiredUnread = UserHash.Fields.Notifications(NotificationStatus.ExpiredUnread.ToString());
                string allNotificationsExpiredRead = UserHash.Fields.Notifications(NotificationStatus.ExpiredRead.ToString());
                 * */

                string unreadNotificationsField = UserHash.Fields.Notifications(noificationType, NotificationStatus.Unread.ToString());
                string readNotificationsField = UserHash.Fields.Notifications(noificationType, NotificationStatus.Read.ToString());
                string expiredUnreadNotificationsField = UserHash.Fields.Notifications(noificationType, NotificationStatus.ExpiredUnread.ToString());
                string expiredReadNotificationsField = UserHash.Fields.Notifications(noificationType, NotificationStatus.ExpiredRead.ToString());

                try
                {
                    cache.HashDelete(userKey, unreadNotificationsField, CommandFlags.FireAndForget);
                    cache.HashDelete(userKey, readNotificationsField, CommandFlags.FireAndForget);
                    cache.HashDelete(userKey, expiredUnreadNotificationsField, CommandFlags.FireAndForget);
                    cache.HashDelete(userKey, expiredReadNotificationsField, CommandFlags.FireAndForget);
                }
                catch
                {

                }


                //con.Close();

                return true;
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to clear all notification caches for: " + userId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return false;
            }
        }
    }
}
