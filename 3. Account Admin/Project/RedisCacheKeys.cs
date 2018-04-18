using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountAdminSite.RedisCacheKeys
{
    /// <summary>
    /// Used as a SECONDARY layer of cache within the management portal in addition to the caching management found within CoreServices.
    /// the THIRD and final layer rests with the API projects. The THRD layer never handles ANY expirations. The seconds layer is responsible for expiring updated keys it manages or uses. 
    /// All keys are set to expire (nothing infinite).
    /// All keys are used by Management Portal or APIs and might be shared between those projects
    /// In all cases we use simple string (No Hashes)
    /// </summary>
    public static class AccountImages
    {
        public static TimeSpan Expiration = TimeSpan.FromDays(14);

        public static string Key(string accountNameKey)
        {
            return accountNameKey + ":accountImages";
        }

        //Expiration:
        public static void Expire(string accountNameKey)
        {
            IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();
            string redisKey = Key(accountNameKey);
            try
            {
                cache.KeyExpire(redisKey, TimeSpan.FromMilliseconds(1));
            }
            catch
            {

            }
        }
    }
}