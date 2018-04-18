using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.PlatformManagerServer.Hashes
{
    public static class ReportsHash
    {
        public static string Key = "reports";
        public static TimeSpan Expiration = TimeSpan.FromSeconds(90);

        public static class Fields
        {
            public static string Billing(int sinceHoursAgo)
            {
                return "billing:" + sinceHoursAgo;
            }
        }
    }
}
