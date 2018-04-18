using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class GlobalHash
    {
        public static string Key = "global";

        public static class Fields
        {
            public static string PropertyTypes = "propertytypes";
            public static string ImageGroupTypes = "imagegrouptypes";
        }
    }
}
