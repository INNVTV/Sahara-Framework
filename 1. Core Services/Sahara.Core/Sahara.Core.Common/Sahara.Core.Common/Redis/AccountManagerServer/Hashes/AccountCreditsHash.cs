using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class AccountCreditsHash
    {

        public static string Key()
        {
            return "credits";
        }

        public static class Fields
        {
            public static string CreditsAvailable(string accountId)
            {
                return "credits:available:" + accountId;
            }
        }
    }
}
