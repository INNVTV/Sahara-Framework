using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class AccountApiKeysHash
    {
        //internal static TimeSpan DefaultCacheLength = TimeSpan.FromDays(15);

        public static string Key(string accountNameKey)
        {
            return accountNameKey + ":apikeys";
        }

        public static class Fields
        {

            public static string List()
            {
                return "list";
            }

        }
    }
}
