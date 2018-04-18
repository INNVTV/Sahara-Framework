using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.ApiKeys.Sql.Statements
{
    public static class UpdateStatements
    {
        internal static bool UpdateApiKey(string sqlPartition, string schemaId, string oldKey, string newKey)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".ApiKeys ");

            sqlStatement.Append("SET ApiKey = '");
            sqlStatement.Append(newKey);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE ApiKey = '");
            sqlStatement.Append(oldKey);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result > 0)
            {
                isSuccess = true;
            }

            return isSuccess;
        }
    }
}
