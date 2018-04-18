using Sahara.Core.Application.Categorization.Models;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Categorization.Internal
{
    internal static class Caching
    {
        internal static void InvalidateCategorizationCaches(string accountNameKey)
        {
            //Switch to Application specific redis server?
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.KeyDelete(ApplicationCategorizationHash.Key(accountNameKey));
            }
            catch
            {

            }
            
        }
    }
}
