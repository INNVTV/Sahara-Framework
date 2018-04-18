using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.PlatformManagerServer.Strings
{
    public static class GlobalUserCount
    {
        public static TimeSpan Expiration = TimeSpan.FromMinutes(15);

        public static string Key()
        {
            return "globalusercount";
        }
        
    }
}
