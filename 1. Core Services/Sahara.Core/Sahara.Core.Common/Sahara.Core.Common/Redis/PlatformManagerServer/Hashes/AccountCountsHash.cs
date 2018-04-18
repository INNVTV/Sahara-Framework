using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.PlatformManagerServer.Hashes
{
    /// <summary>
    /// Used for Search/Browse results for Platform Admins
    /// </summary>
    public static class AccountCountsHash
    {
        public static TimeSpan Expiration = TimeSpan.FromSeconds(180);

        public static string Key = "account:counts";

        public static class Fields
        {
            public static string CountAll()
            {
                return "all";
            }
            public static string CountFilter(string filter, string value)
            {
                return "filter:" + filter + ":" + value;
            }
            public static string CountSinceDateTime(DateTime sinceDateTime)
            {
                return "since:datetime:" + sinceDateTime.ToShortDateString();
            }


        }

    }
}
