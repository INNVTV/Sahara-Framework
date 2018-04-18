using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.PlatformManagerServer.Hashes
{
    /// <summary>
    /// Used for Search/Browse results for Platform Admins
    /// </summary>
    public static class AccountListsHash
    {
        public static TimeSpan Expiration = TimeSpan.FromSeconds(60);

        public static string Key = "accountlists";

        public static class Fields
        {
            public static string All(int pageNumber, int pageSize, string orderBy)
            {
                return "all:" + pageNumber + ":" + pageSize + ":" + orderBy;
            }

            public static string Filter(string filter, string value, int pageNumber, int pageSize, string orderBy)
            {
                return "filter:" + filter + ":" + value + ":" + pageNumber + ":" + pageSize + ":" + orderBy;
            }
            public static string Search(string query, string orderBy, int maxResults)
            {
                return "search" + query + ":" + orderBy + ":" + maxResults.ToString();
            }

        }

    }
}
