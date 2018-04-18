using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class AccountSettingsHash
    {
        //internal static TimeSpan DefaultCacheLength = TimeSpan.FromDays(14);

        public static string Key()
        {
            return "account:settings";
        }

        public static class Fields
        {

            public static string Document(string accountNameKey)
            {
                return accountNameKey;
            }

        }
    }
}
