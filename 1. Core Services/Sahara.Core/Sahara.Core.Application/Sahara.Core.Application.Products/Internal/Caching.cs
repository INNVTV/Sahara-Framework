using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Products.Internal
{
    internal static class Caching
    {
        internal static void InvalidateProductCaches(string accountNameKey)
        {
            //Used to handle public API calls for products only. Admins always use direct DOcumentDB Calls. Only clear this when add products or updating products.
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.KeyDelete(ApplicationProductHash.Key(accountNameKey));

                //We also clear the associated search hash so that facets tied to products are properly updated
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
