using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common
{
    public static class MemoryCacheManager
    {
        //Moved to Sahara.Core.Settings.Search to avoid circular reference issues:
        //public static ObjectCache SearchServiceCache = MemoryCache.Default;
        //public static int SearchServiceClientCacheTimeInMinutes = 60;

        public static ObjectCache SearchIndexCache = MemoryCache.Default;
        public static int SearchIndexClientCacheTimeInMinutes = 15;
    }
}
