using Sahara.Core.Accounts.Models;
using Sahara.Core.Common.Methods;
using Sahara.Core.Common.Redis;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Common.Redis.AccountManagerServer.Strings;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Internal
{
    internal static class AccountUserCaching
    {
 
        internal static void ClearAssociatedAccountAndUserListCaches(string accountId)
        {
            //Update user list cache for this account for next call (we do this first so it is available to Account:
            AccountUserManager.GetUsers(accountId, false);

            //We refresh this last so the above user list is already cached.
            Internal.AccountCaching.UpdateAccountDetailCache(accountId);

        }

        internal static void DeleteUserHashReference(string userId)
        {

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                //Delete the entire Hash for this user
                cache.KeyDelete(UserHash.Key(userId), CommandFlags.FireAndForget);
            }
            catch
            {

            }

        }

        internal static void DeleteAllUserCacheReferences(AccountUser accountUser)
        {

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                //Delete the entire Hash for this user
                cache.KeyDelete(UserHash.Key(accountUser.Id.ToString()), CommandFlags.FireAndForget);

                //Delete the id lookup table
                cache.HashDelete(UserIdByUserNameHash.Key(accountUser.UserName), UserIdByUserNameHash.Fields.UserId(accountUser.UserName), CommandFlags.FireAndForget);
            }
            catch
            {

            }
        }



        /// <summary>
        /// Used by AccountManager to clear all user caches on an account after an Account update
        /// </summary>
        /// <param name="accountId"></param>
        internal static void ClearAllUserCaches(string accountId)
        {
            var users = AccountUserManager.GetUsers(accountId);
            foreach (var user in users)
            {
                DeleteAllUserCacheReferences(AccountUserManager.GetUser(user.Id));
            }
        }

        /// <summary>
        /// Clears ALL user caches across ALL accounts platform wide (unused?) Just use above method in a FOR loop
        /// </summary>
        ///
        /*
        internal static void ClearAllUserCaches()
        {
            var accounts = AccountManager.
            var users = GetUsers(accountId);
            foreach (var user in users)
            {
                DeleteAccountUsersIdentityReferencesInCache(GetUserIdentity(user.Id, false));
            }
        }*/
    }
}
