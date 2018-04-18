using Microsoft.Azure.Search;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Sahara.Core.Settings.Models.Partitions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Azure
{
    public static class Search
    {
        public static ObjectCache SearchServiceCache = MemoryCache.Default;
        public static int SearchServiceClientCacheTimeInMinutes = 60;

        #region LEGACY (MOVED TO PARTITIONED STRATEGY)

        #endregion


        internal static void Initialize()
        {
            //Not needed
        }



        //Static list of Search Partitions for Accounts (created from SQL when CoreServices starts, reset if partition is searched for does not exist)
        public static List<SearchPartition> AccountSearchPartitions = new List<SearchPartition>();



        public static SearchServiceClient GetSearchPartitionClient(string name)
        {
            //Get from cache first
            var accountSearchService = SearchServiceCache[name] as SearchServiceClient;

            if(accountSearchService == null)
            {
                //Not in cache, pull from partitions list and create: ----------------------------

                var partition = GetSearchPartition(name);

                if (partition != null)
                {
                    accountSearchService = new SearchServiceClient(partition.Name, new SearchCredentials(partition.Key));

                    //Store in cache: ---------------------
                    SearchServiceCache.Set(name, accountSearchService, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(SearchServiceClientCacheTimeInMinutes) });
                }
                else
                {
                    accountSearchService = null;
                }
            }

            return accountSearchService;

        }
            

        //Public method to get a particular search partiton from the static list available.
        //If cannot be foound we attempt a refresh (in case this is a new partition)
        public static SearchPartition GetSearchPartition(string name)
        {
            SearchPartition searchPartition = null;


            if(AccountSearchPartitions.Count > 0)
            {
                searchPartition = AccountSearchPartitions.FirstOrDefault(partition => partition.Name == name);
            }
            

            if (searchPartition == null)
            {
                //Did not exist, attempt to refresh the static list and try again
                AccountSearchPartitions = RefreshSearchPartitions();

                if (AccountSearchPartitions.Count > 0)
                {
                    searchPartition = AccountSearchPartitions.FirstOrDefault(partition => partition.Name == name);
                }
            }

            return searchPartition;
        }

        //Duplicate SQL code from PartitionManager ONLY for get operations within Settings
        public static List<SearchPartition> RefreshSearchPartitions()
        {
            List<SearchPartition> partitions = new List<SearchPartition>();


            try
            {
                //Will fail if platform is not initialized, so wraped in Try/Catch
                //Once initialized settings must run initialization startup again

                //SQL Statement =============================================================
                string SqlStatement =
                    "SELECT * From SearchPartitions";

                //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);
                SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
                sqlCommand.CommandText = SqlStatement.ToString();


                sqlCommand.Connection.OpenWithRetry();
                SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

                while (reader.Read())
                {
                    partitions.Add(new SearchPartition { Name = reader["Name"].ToString(), Key = reader["Key"].ToString(), TenantCount = (int)reader["TenantCount"], Plan = reader["Plan"].ToString() });
                }

                sqlCommand.Connection.Close();

            }
            catch
            {

            }



            return partitions;
        }



    }
}
