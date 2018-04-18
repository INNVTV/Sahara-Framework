using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.PlatformManagerServer.Hashes
{
    public static class CreditsHash
    {
        public static TimeSpan Expiration = TimeSpan.FromMinutes(8);

        public static string Key = "credits";

        public static class Fields
        {
            public static string TotalInCirculation = "circulation";
        }
    }
}
