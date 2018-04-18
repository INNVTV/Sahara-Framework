using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class ApplicationPropertiesHash
    {
        public static TimeSpan DefaultCacheLength = TimeSpan.FromDays(7);

        public static string Key(string accountNameKey)
        {
            return accountNameKey + ":properties";
        }

        public static class Fields
        {
            // List of Properties

            public static string All()
            {
                return "all";             
            }

            public static string Listings()
            {
                return "listings";
            }

            public static string Details()
            {
                return "details";
            }

            public static string Featured()
            {
                return "featured";
            }

            // Properties Count
            public static string Count()
            {
                return "count";
            }

            // Property Detail

            public static string Details(string propertyNameKey)
            {
                return "details:" + propertyNameKey;
            }

            // List of Values (see details above)

            //public static string PropertyValues(string propertyNameKey)
            //{
                //return "values:" + propertyNameKey;
            //}

        }
    }
}
