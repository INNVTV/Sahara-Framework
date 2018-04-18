using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Search.Internal
{
    public static class SearchCaching
    {

        internal static void InvalidateProductSearchFacetsCache(string accountNameKey)
        {
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.KeyDelete(
                    Sahara.Core.Common.Redis.AccountManagerServer.Hashes.ApplicationSearchHash.Key(accountNameKey),
                    CommandFlags.FireAndForget
                    );
            }
            catch
            {

            }

        }

    }
}
