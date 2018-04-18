using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class ThemesHash
    {

        public static string Key = "themes";

        public static class Fields
        {
            public static string ThemesList()
            {
                return "list";
            }
        }
    }
}
