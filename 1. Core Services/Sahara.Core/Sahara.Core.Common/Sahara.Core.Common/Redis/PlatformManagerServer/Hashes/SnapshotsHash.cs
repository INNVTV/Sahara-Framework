using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.PlatformManagerServer.Hashes
{
    public static class SnapshotsHash
    {
        public static string Key = "snapshots";
        public static TimeSpan Expiration = TimeSpan.FromSeconds(120);

        public static class Fields
        {
            public static string Accounts = "accounts";
            public static string Infrastructure = "infrastructure";
            public static string Billing = "billing";
        }
    }
}
