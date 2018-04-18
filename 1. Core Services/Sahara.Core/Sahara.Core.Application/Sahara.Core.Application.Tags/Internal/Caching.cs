using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Tags
{
    internal static class Caching
    {
        internal static void InvalidateTagsCache(string accountNameKey)
        {
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.KeyDelete(
                        ApplicationTagsHash.Key(accountNameKey),
                        CommandFlags.FireAndForget
                    );
            }
            catch
            {

            }

        }
    }
}
