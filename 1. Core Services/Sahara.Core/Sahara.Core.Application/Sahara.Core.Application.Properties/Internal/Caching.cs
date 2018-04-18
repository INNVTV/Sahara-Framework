using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties.Internal
{
    internal static class Caching
    {
        internal static void InvalidateAllPropertyCachesForAccount(string accountNameKey)
        {
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.KeyDelete(
                        ApplicationPropertiesHash.Key(accountNameKey),
                        CommandFlags.FireAndForget
                    );

                //We also clear the associated search hash so that facets tied to properties are properly updated
                cache.KeyDelete(
                        ApplicationSearchHash.Key(accountNameKey),
                        CommandFlags.FireAndForget
                    );
            }
            catch
            {

            }
        }
    }
}
