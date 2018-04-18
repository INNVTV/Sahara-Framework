using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class ApplicationSearchHash
    {
        public static TimeSpan DefaultCacheLength = TimeSpan.FromDays(10);

        public static string Key(string accountNameKey)
        {
            return accountNameKey + ":search";
        }

        public static class Fields
        {
            public static string ProductFacets()
            {
                return "facets:products";
            }
            public static string ProductFacetsVisible()
            {
                return "facets:products:visible";
            }

            public static string ProductSortables()
            {
                return "sortables:products";
            }

        }
    }
}
