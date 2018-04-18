using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis
{
    public static class ApiRedisLayer
    {
        /// <summary>
        /// Clear ALL api caches for the account.
        /// Used on ANY data updates for the account
        /// </summary>
        /// <param name="accountNameKey"></param>
        public static void InvalidateAccountApiCacheLayer(string accountNameKey)
        {
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            try
            {
                cache.KeyDelete(accountNameKey + ":apicache");
            }
            catch
            {

            }
            
        }
    }
}
