using System;
using System.Collections.Generic;
using System.Linq;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Infrastructure.Azure.Models.Database;
using Sahara.Core.Infrastructure.Azure.Types.Database;
using Sahara.Core.Platform.Partitioning.Sql.Scripts.Create;
using Sahara.Core.Platform.Partitioning.Sql.Scripts.Initialization;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Platform.Partitioning.Models;

namespace Sahara.Core.Platform.Partitioning
{
    internal static class SqlPartitioning
    {

        /// <summary>
        /// Gets the next avaialbe account database name with the most open tenant slots and below the maximum allowance of tenants. 
        /// Returns String.Empty if there are no partitions in the system, or if all the partitions are at capacity
        /// </summary>
        /// <returns></returns>
        internal static string GetNextAvailablePartition()
        {
            string partitionName = String.Empty;

            List<OpenPartition> openPartitions = new List<OpenPartition>();

            // 1. Get list of all database partitions
            var databasePartitionNames = Sql.Statements.SelectStatements.SelectSharedDatabasePartitionNames();
            foreach (string databasePartitionName in databasePartitionNames)
            {
                // 2. Get count of unique schemas of type 'U' | This represents how many tenants are currenty sharing the partition:
                int schemaCount = Sql.Statements.SelectStatements.SelectSharedDatabaseUniqueUserSchemaCount(databasePartitionName);

                // 3. If count is below the maximum tenant allowance: Add it to the list of open partitions
                if(schemaCount < Sahara.Core.Settings.Platform.Partitioning.MaximumTenantsPerAccountDatabasePartition)
                {
                    openPartitions.Add(
                        new OpenPartition
                            {
                                PartitionName = databasePartitionName,
                                TenantCount = schemaCount
                            }
                        );
                }
            }

            // 4. Choose a partiton from the list with the most avaialbe open slots:
            if(openPartitions.Count > 1)
            {
                // There is more than one option, we want the partition with the most open slots so we sort by TenantCount ascending.
                openPartitions = openPartitions.OrderBy(t => t.TenantCount).ToList();
                //Partitions with most open slots are now at the top

                // Our coice is the first item, the one with the most availabe open tenant slots
                partitionName = openPartitions[0].PartitionName;
            }
            else if (openPartitions.Count == 1)
            {
                // Our coice is the first item, there is only one availalbe partition, no need to sort
                partitionName = openPartitions[0].PartitionName;
            }

            return partitionName; //<-- If returns String.Empty then either no partitions exist, or all partitions are full.
        }

        internal static DataAccessResponseType AssignPartition(string accountID, string sqlPartitionName)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            try
            {
                if(Sql.Statements.UpdateStatements.UpdateSqlPartition(accountID, sqlPartitionName))
                {
                    response.isSuccess = true;

                    return response;
                }
                else
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "An error occurred while updating the SqlPartition name, or the AccountID does not exist.";

                    return response;
                }
            }
            catch(Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to assign partition: " + sqlPartitionName + " to account: " + accountID,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;

                return response;
            }

        }

        internal static string InitializeNewDatabasePartition()
        {
            string partitionName = String.Empty;

            // 1. Start with base name: 
            string basePartitionName = Sahara.Core.Settings.Platform.Partitioning.AccountDatabasebaseSharedPartitionName;

            // 2. Get next partiton ID and append to base partition name
            int nextPartitionIdentity = GetNextParitionID();

            partitionName = basePartitionName + nextPartitionIdentity.ToString();

            // 3. Create DataBase:
            Create.CreateDatabase(
                new DatabaseModel
                {
                    DatabaseTier = DatabaseTier.Basic,
                    DatabaseSize = MaxSize_Basic._2GB,
                    DatabaseName = partitionName
                });

            // 4. Create Stored Procedure to DestroySchema (Used for deprovisioning accounts on this partition)
            var SqlInitialization = new Initialization();
            SqlInitialization.InitializePartition(partitionName);

            // 5. return the name of the partition created
            return partitionName;
        }

        internal static List<SqlPartition> GetPartitionList()
        {
            return Sql.Statements.SelectStatements.SelectSharedDatabasePartitions();
        }

        internal static SqlPartition GetPartition(string partitionName)
        {
            return Sql.Statements.SelectStatements.SelectSharedDatabasePartition(partitionName);
        }

        internal static int GetPartitionTenantCount(string partitionName)
        {
            return Sql.Statements.SelectStatements.SelectSharedDatabaseUniqueUserSchemaCount(partitionName);
        }

        #region Private Methods

        /// <summary>
        /// Get next partition ID (Used when spinning up a new partition
        /// </summary>
        /// <returns></returns>
        private static int GetNextParitionID()
        {

            //Get latest database parition name:
            string name = Sql.Statements.SelectStatements.SelectLatestSharedDatabasePartitionName();
            if (name != String.Empty)
            {
                //Isolate just the ID portion of the latest partition name
                string strID = name.Replace(Sahara.Core.Settings.Platform.Partitioning.AccountDatabasebaseSharedPartitionName, "");


                //convert to an Int and add 1
                return Convert.ToInt32(strID) + 1;

                //Add 1 to the identity and return:
            }
            else
            {
                //If no partition exists, this is the first partition, so add 1 to the IdentitySeed:
                return Sahara.Core.Settings.Platform.Partitioning.AccountPartitonIdentitySeed + 1;
            }

        }

        #endregion

        #region Private Models

        private class OpenPartition
        {
            public string PartitionName { get; set; }
            public int TenantCount { get; set; }
        }

        #endregion


    }
}
