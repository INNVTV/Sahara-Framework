using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class ApplicationTagsHash
    {
        public static TimeSpan DefaultCacheLength = TimeSpan.FromDays(7);

        public static string Key(string accountNameKey)
        {
            return accountNameKey + ":tags";
        }

        public static class Fields
        {
            public static string TagList()
            {
                return "list";
            }
        }
    }

}
