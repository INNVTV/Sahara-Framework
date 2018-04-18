using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Strings
{
    public static class AccountsForEmailString
    {
        public static TimeSpan Expiration = TimeSpan.FromMinutes(15);

        public static string Key(string email)
        {
            return "accountsfor:" + email;
        }
        
    }
}
