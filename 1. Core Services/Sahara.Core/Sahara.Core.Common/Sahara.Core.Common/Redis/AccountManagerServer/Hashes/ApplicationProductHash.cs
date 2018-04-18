using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class ApplicationProductHash
    {
        public static TimeSpan DefaultCacheLength = TimeSpan.FromDays(7);

        public static string Key(string accountNameKey)
        {
            return accountNameKey + ":products";
        }

        public static class Fields
        {

        }

    }
}
