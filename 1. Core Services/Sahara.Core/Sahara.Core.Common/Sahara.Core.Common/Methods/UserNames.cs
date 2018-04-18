using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Methods
{
    public static class AccountUserNames
    {
        public static string GenerateGlobalUniqueUserName(string email, string accountId)
        {
            return email + "_" + accountId;
        }
    }
}
