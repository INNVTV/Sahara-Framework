using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Methods
{
    public static class EndPoints
    {
        public static string AccountManagementURL(string accountNameKey)
        {
                return accountNameKey + "." + Settings.Endpoints.URLs.AccountManagementDomain;
        }
    }
}
