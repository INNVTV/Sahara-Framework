using Sahara.Core.Platform.Partitioning.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Sahara.Core.Settings.Models.Partitions;

namespace Sahara.Core.Platform.Partitioning.Sql.Statements
{
    internal static class SelectStatements
    {
        #region SQL Partitions

        /// <summary>
        /// Returns a list of all account data database partition names
        /// </summary>
        /// <returns></returns>
        public static List<string> SelectSharedDatabasePartitionNames()
        {
            List<string> response = new List<string>();

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT name from sys.databases where name like '%" + Sahara.Core.Settings.Platform.Partitioning.AccountDatabasebaseSharedPartitionName + "%'";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response.Add(reader["name"].ToString());
            }

            sqlCommand.Connection.Close();

            return response;
        }

        /// <summary>
        /// Returns a list of all account data database partitions
        /// </summary>
        /// <returns></returns>
        public static List<SqlPartition> SelectSharedDatabasePartitions()
        {
            List<SqlPartition> response = new List<SqlPartition>();

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT name, create_date from sys.databases where name like '%" + Sahara.Core.Settings.Platform.Partitioning.AccountDatabasebaseSharedPartitionName + "%'";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response.Add(new SqlPartition { Name = reader["name"].ToString(), CreatedDate = (DateTime)reader["create_date"] });
            }

            sqlCommand.Connection.Close();

            return response;
        }

        /// <summary>
        /// Returns an account data database partition
        /// </summary>
        /// <returns></returns>
        public static SqlPartition SelectSharedDatabasePartition(string partitionName)
        {
            SqlPartition response = new SqlPartition();

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT TOP 1 name, create_date from sys.databases where name = '" + partitionName + "'";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response = new SqlPartition { Name = reader["name"].ToString(), CreatedDate = (DateTime)reader["create_date"] };
            }

            sqlCommand.Connection.Close();

            return response;
        }

        /// <summary>
        /// Returns a count of the number of unique User schemas in the database
        /// </summary>
        /// <returns></returns>
        public static int SelectSharedDatabaseUniqueUserSchemaCount(string accountDatabasePartitionName)
        {
            int response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT COUNT(DISTINCT SCHEMA_NAME(schema_id)) AS Count FROM sys.objects WHERE type = 'U'";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(accountDatabasePartitionName));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(accountDatabasePartitionName).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response = Convert.ToInt32(reader["Count"]);
            }

            sqlCommand.Connection.Close();

            return response;
        }

        /// <summary>
        /// Returns a list of all the account id's of unique User schemas in the database partition
        /// </summary>
        /// <returns></returns>
        public static List<String> SelectSharedDatabaseUniqueSchemas(string accountDatabasePartitionName, int maxResults)
        {
            var response  = new List<String>();

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT TOP " + maxResults + " name FROM sys.schemas Where principal_id = 1 and name like '%" + Sahara.Core.Settings.Platform.Partitioning.AccountPartitionSchemaDesignation + "%'";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(accountDatabasePartitionName));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(accountDatabasePartitionName).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response.Add(reader["name"].ToString());
            }

            sqlCommand.Connection.Close();

            return response;
        }


        /// <summary>
        /// Gets the name of the newest account data partition in the system (based on name)
        /// </summary>
        /// <returns></returns>
        public static string SelectLatestSharedDatabasePartitionName()
        {
            string response = String.Empty;

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            // Get the newest database partition name by selecting all databases with the base database parition name order by descesning, which will bring the latest one to the top:
            SqlStatement.Append("SELECT top 1 name from sys.databases where name like '%");
            SqlStatement.Append(Sahara.Core.Settings.Platform.Partitioning.AccountDatabasebaseSharedPartitionName);
            SqlStatement.Append("%' order by name desc");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response = reader["name"].ToString();
            }

            sqlCommand.Connection.Close();

            return response;
        }

        public static decimal SelectTenantSchemaVersion(string sqlPartition, string schemaId)
        {
            decimal response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT TOP 1 Version from " + schemaId + ".SchemaLog ORDER BY InstallDate Desc";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response = (decimal)reader["Version"];
            }

            sqlCommand.Connection.Close();

            return response;
        }

        public static List<SqlSchemaLog> SelectTenantSchemaLog(string sqlPartition, string schemaId, int maxResults)
        {
            var logs = new List<SqlSchemaLog>();

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT TOP " + maxResults + " * from " + schemaId + ".SchemaLog ORDER BY InstallDate Desc";

           // SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                logs.Add(Transforms.DataReader_to_SqlSchemaLog(reader));
            }

            sqlCommand.Connection.Close();

            return logs;
        }

        #endregion

        #region Storage Partitions

        public static List<StoragePartition> SelectStoragePartitions()
        {
            List<StoragePartition> partitions = new List<StoragePartition>();

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT * From StoragePartitions";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                partitions.Add(new StoragePartition { Name = reader["Name"].ToString(), Key = reader["Key"].ToString(), TenantCount = (int)reader["TenantCount"], MaxTenants = Settings.Platform.Partitioning.MaximumTenantsPerStorageAccount, CDN = reader["CDN"].ToString(), URL = reader["URL"].ToString() });
            }

            sqlCommand.Connection.Close();

            return partitions;
        }

        #endregion

        #region Search Partitions

        public static List<SearchPartition> SelectSearchPartitions(string plan = null)
        {
            List<SearchPartition> partitions = new List<SearchPartition>();

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT * From SearchPartitions";

            if(plan != null)
            {
                SqlStatement += " Where [Plan] = '" + plan + "'";
            }

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                string searchPlan = reader["Plan"].ToString();
                

                int maxTenants = Int32.Parse((searchPlan.Substring(searchPlan.LastIndexOf("-") + 1)));

                /* MAx Tenatnts are now pulled from the SarchPlan name
                 * int maxTenants = 0;
                 * 
                switch (searchPlan.ToLower())
                {
                    case "free":
                        maxTenants = Settings.Platform.Partitioning.MaximumTenantsPerFreeSearchService;
                        break;
                    case "basic":
                        maxTenants = Settings.Platform.Partitioning.MaximumTenantsPerBasicSearchServiceShared;
                        break;
                    case "basic-dedicated":
                        maxTenants = Settings.Platform.Partitioning.MaximumTenantsPerBasicSearchServiceDedicated;
                        break;
                    case "s1":
                        maxTenants = Settings.Platform.Partitioning.MaximumTenantsPerS1SearchServiceShared;
                        break;
                    case "s1-dedicated":
                        maxTenants = Settings.Platform.Partitioning.MaximumTenantsPerS1SearchServiceDedicated;
                        break;
                    case "s2":
                        maxTenants = Settings.Platform.Partitioning.MaximumTenantsPerS2SearchServiceShared;
                        break;
                    case "s2-dedicated":
                        maxTenants = Settings.Platform.Partitioning.MaximumTenantsPerS2SearchServiceDedicated;
                        break;
                }*/

                partitions.Add(new SearchPartition { Name = reader["Name"].ToString(), Key = reader["Key"].ToString(), TenantCount = (int)reader["TenantCount"], MaxTenants = maxTenants, Plan = reader["Plan"].ToString() });
            }

            sqlCommand.Connection.Close();

            return partitions;
        }

        #endregion
    }
}
