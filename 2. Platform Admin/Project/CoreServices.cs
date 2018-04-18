using PlatformAdminSite.PlatformSettingsService;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlatformAdminSite
{
    public static class CoreServices
    {

        /// <summary>
        /// Static wrapper for centralized CoreServices settings
        /// </summary>
        public static CorePlatformSettings PlatformSettings;


       // public static string ApplicationName;

        public static class Platform
        {
            public static bool Initialized;

            public static string[] UserRoles;
        }
        

        public static class Accounts
        {
            public static string[] UserRoles;
        }

        public static class RedisConnectionMultiplexers
        {
            //Because the  ConnectionMultiplexer  does a lot, it is designed to be shared and reused between callers.
            //You should not create a  ConnectionMultiplexer  per operation. It is fully thread-safe and ready for this usage.
            //In all the subsequent examples it will be assumed that you have a  ConnectionMultiplexer  instance stored away for re-use.

            public static ConnectionMultiplexer RedisMultiplexer;
            //public static ConnectionMultiplexer PlatformManager_Multiplexer;
            //public static ConnectionMultiplexer AccountManager_Multiplexer;
        }
    }
}