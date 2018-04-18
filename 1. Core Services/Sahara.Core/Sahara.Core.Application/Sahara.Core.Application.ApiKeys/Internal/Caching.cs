using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;

namespace Sahara.Core.Application.ApiKeys.Internal
{
    internal static class Caching
    {
        internal static void InvalidateApiKeysCache(string accountNameKey)
        {
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.KeyDelete(AccountApiKeysHash.Key(accountNameKey));
            }
            catch
            {

            }

        }
    }
}
