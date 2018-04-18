using System;
using System.Collections.Generic;
using Sahara.Core.Common.Methods;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Partitioning.Models;
using Sahara.Core.Logging.PlatformLogs.Helpers;

namespace Sahara.Core.Platform.Partitioning
{
    public static class SqlPartitioningManager
    {
        
        /// <summary>
        /// Gets the next available partition to assign an account to, return partition name in SuccessMessage
        /// </summary>
        /// <returns></returns>
        public static DataAccessResponseType GetAndAssignNextAvailableAccountSqlPartition(string accountID)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            // 1. Get the next partition name:
            string nextPartitionName = SqlPartitioning.GetNextAvailablePartition();

            if (nextPartitionName == String.Empty)
            {
                // 1a. All partitions are full, or none exist. Create a new or initial partition:

                try
                {
                    nextPartitionName = SqlPartitioning.InitializeNewDatabasePartition();
                }
                catch(Exception e)
                {
                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to get and assign next available partition for: " + accountID,
                        System.Reflection.MethodBase.GetCurrentMethod()
                    );

                    response.isSuccess = false;
                    response.ErrorMessage = e.Message;

                    return response;
                }

                if (nextPartitionName == String.Empty)
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Unable to create a new database partition.";

                    return response;
                }

            }

            //2. Assign the partition to the account:
            var assignPartitionRespone = SqlPartitioning.AssignPartition(accountID, nextPartitionName);

            assignPartitionRespone.SuccessMessage = nextPartitionName; //<--Return name of partition in SuccessMessage

            return assignPartitionRespone;

        }

        public static List<SqlPartition> GetSqlPartitions(bool includeTenantCount, bool useCachedVersion = true)
        {
            var partitions = new List<SqlPartition>();

            //DataCache dataCache = new DataCache(NamedCache.Short);
            //string cacheId = AccountPartitionsCacheID.All(includeTenantCount);
            object cachedAccountPartitions = null;

            if (useCachedVersion)
            {
                //Check the cache first
                //cachedAccountPartitions = dataCache.Get(cacheId);
            }

            if (cachedAccountPartitions == null)
            {

                partitions = SqlPartitioning.GetPartitionList();

                if (includeTenantCount)
                {
                    foreach (SqlPartition partition in partitions)
                    {
                        partition.TenantCount = SqlPartitioning.GetPartitionTenantCount(partition.Name);
                    }
                }

                //store data into cache
                //dataCache.Put(cacheId, partitions);
            }
            else
            {
                //use cached version
                partitions = (List<SqlPartition>)cachedAccountPartitions;
            }

            return partitions;
        }

        public static SqlPartition GetSqlPartition(string partitionName, bool useCachedVersion = true) //bool includeSchemas, bool useCachedVersion = true) //bool includeTenants, bool useCachedVersion = true)
        {
            var partition = new SqlPartition();

            //DataCache dataCache = new DataCache(NamedCache.Short);
            //string cacheId = AccountPartitionsCacheID.Detail(partitionName, includeTenants);
            object cachedAccountPartition = null;

            if (useCachedVersion)
            {
                //Check the cache first
                //cachedAccountPartition = dataCache.Get(cacheId);
            }

            if (cachedAccountPartition == null)
            {
                partition = SqlPartitioning.GetPartition(partitionName);
                partition.TenantCount = SqlPartitioning.GetPartitionTenantCount(partitionName);

                /*
                if (includeSchemas)
                {
                    //Get list of all schemas 
                    var schemas = Sql.Statements.SelectStatements.SelectSharedDatabaseUniqueSchemas(partitionName);
                    partition.Schemas = schemas;
                }
                */

                /*
                if (includeTenants)
                {
                    foreach (string schema in schemas)
                    {
                        //convert each schema back to an AccountID and get each account
                        string accountID = Sahara.Core.Common.Methods.SchemaNames.FromAccountSchemaName(schema).ToString();
                        //partition.Tenants.Add(Sahara.Accounts.Core.Management.AccountManager.GetAccountByID(accountID, false));
                        //partition.Tenants.Add(AccountManager.GetAccount(accountID));
                    }
                }*/

                //store data into cache
                //dataCache.Put(cacheId, partition);
            }
            else
            {
                //use cached version
                partition = (SqlPartition)cachedAccountPartition;
            }

            return partition;
        }

        public static List<string> GetSqlPartitionSchemas(string databaseName, int maxResults)
        {
            return Sql.Statements.SelectStatements.SelectSharedDatabaseUniqueSchemas(databaseName, maxResults);
        }

        public static decimal GetSqlPartitionTenantSchemaVersion(string sqlPartition, string schemaId)
        {
            return Sql.Statements.SelectStatements.SelectTenantSchemaVersion(sqlPartition, schemaId);
        }

        public static List<SqlSchemaLog> GetSqlPartitionTenantSchemaLog(string sqlPartition, string schemaId, int maxResults)
        {
            return Sql.Statements.SelectStatements.SelectTenantSchemaLog(sqlPartition, schemaId, maxResults);
        }
        
    }
}
