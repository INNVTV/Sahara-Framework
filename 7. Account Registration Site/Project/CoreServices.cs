using AccountRegistration.PlatformSettingsService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AccountRegistration
{
    public static class CoreServices
    {

        /// <summary>
        /// Static wrapper for centralized CoreServices settings
        /// </summary>
        public static CorePlatformSettings PlatformSettings;


        /*
        public static class RedisConnectionMultiplexers
        {
            //Because the  ConnectionMultiplexer  does a lot, it is designed to be shared and reused between callers.
            //You should not create a  ConnectionMultiplexer  per operation. It is fully thread-safe and ready for this usage.
            //In all the subsequent examples it will be assumed that you have a  ConnectionMultiplexer  instance stored away for re-use.

            public static ConnectionMultiplexer PlatformManager_Multiplexer;
            public static ConnectionMultiplexer AccountManager_Multiplexer;
        }*/
    }
}