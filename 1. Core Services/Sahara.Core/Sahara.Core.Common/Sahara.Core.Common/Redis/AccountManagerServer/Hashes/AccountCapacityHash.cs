using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class AccountCapacityHash
    {

        public static string Key()
        {
            return "capacities";
        }

        public static class Fields
        {
            public static string AccountCapacity(string accountId)
            {
                return "account:" + accountId;
            }
        }
    }
}
