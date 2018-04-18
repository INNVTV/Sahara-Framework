using InventoryHawk.Account.Public.Api.PlatformSettingsService;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Search;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api
{
    /// <summary>
    /// Static wrapper to hold settings from CoreServices
    /// </summary>
    public class CoreServices
    {
        // Class from CoreServices-----------------------------------
        public static CorePlatformSettings PlatformSettings;

        /// <summary>
        /// Static connector to the Redis Caches
        /// </summary>
        public static class RedisConnectionMultiplexers
        {
            //Because the  ConnectionMultiplexer  does a lot, it is designed to be shared and reused between callers.
            //You should not create a  ConnectionMultiplexer  per operation. It is fully thread-safe and ready for this usage.
            //In all the subsequent examples it will be assumed that you have a  ConnectionMultiplexer  instance stored away for re-use.

            public static ConnectionMultiplexer RedisMultiplexer;
            //public static ConnectionMultiplexer PlatformManager_Multiplexer;
            //public static ConnectionMultiplexer AccountManager_Multiplexer;
        }

        /// <summary>
        /// Static SearchServiceClient for Client Queries
        /// </summary>
        //public static SearchServiceClient SearchServiceQueryClient;

        /// <summary>
        /// Static client for the DocumentDatabases
        /// </summary>
        public static class DocumentDatabases
        {
            public static DocumentClient Accounts_DocumentClient;
        }
    }
}