using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Capacity.Internal
{
    internal static class AccountCapacityCaching
    {
        public static void InvalidateAccountCapacitiesCache(string accountId)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.HashDelete(
                        AccountCapacityHash.Key(),
                        AccountCapacityHash.Fields.AccountCapacity(accountId),
                        CommandFlags.FireAndForget
                    );
            }
            catch
            {

            }
        }

    }
}
