using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Platform.Deprovisioning.Sql.Statements
{
    internal static class UpdateStatements
    {
        /// <summary>
        /// Decriment storage & search partition tenant count
        /// </summary>
        /// <param name="storagePartition"></param>
        /// <param name="searchPartition"></param>
        /// <returns></returns>
        internal static bool UpdatePartitionsTenantCounts(string storagePartition, string searchPartition)
        {
            bool isSuccess = false;

            //TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

            StringBuilder sqlStatement = new StringBuilder();

            //Updte Storage Partition =============================================================
            sqlStatement.Append("UPDATE StoragePartitions ");

            sqlStatement.Append("SET TenantCount = (TenantCount - 1) Where Name = '");
            sqlStatement.Append(storagePartition);
            sqlStatement.Append("'; ");

            //Updte Search Partition =============================================================
            sqlStatement.Append("UPDATE SearchPartitions ");

            sqlStatement.Append("SET TenantCount = (TenantCount - 1) Where Name = '");
            sqlStatement.Append(searchPartition);
            sqlStatement.Append("'; ");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result > 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }
    }
}
