using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Settings.Internal
{
    internal static class Caching
    {
        internal static void InvalidateSettingsCache(string accountNameKey)
        {
            try
            {
                //Switch to Application specific redis server?
                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
                cache.HashDelete(AccountSettingsHash.Key(), AccountSettingsHash.Fields.Document(accountNameKey));
            }
            catch
            {

            }

        }
    }
}
