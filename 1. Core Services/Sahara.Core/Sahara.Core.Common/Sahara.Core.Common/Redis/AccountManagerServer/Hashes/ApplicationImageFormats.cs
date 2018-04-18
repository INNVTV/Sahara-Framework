using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class ApplicationImageFormatsHash
    {
        internal static TimeSpan DefaultCacheLength = TimeSpan.FromDays(30);

        public static string Key(string accountNameKey)
        {
            return accountNameKey + ":imageformats";
        }

        public static class Fields
        {
            public static string AllFormatsByGroupType(string imageGroupTypeNameKey)
            {
                return "grouptype:" + imageGroupTypeNameKey + ":all";
            }
        }
    }
}
