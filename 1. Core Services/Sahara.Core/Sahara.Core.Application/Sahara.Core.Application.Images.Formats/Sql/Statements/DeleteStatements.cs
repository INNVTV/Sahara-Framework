using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Formats.Sql.Statements
{
    internal static class DeleteStatements
    {
        internal static bool DeleteImageGroup(string sqlPartition, string schemaId, string imageGroupId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement (Delete The Category) =============================================================
            sqlStatement.Append("DELETE FROM ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".ImageGroup ");

            sqlStatement.Append("WHERE ImageGroupID = '");
            sqlStatement.Append(imageGroupId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();

            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static bool DeleteImageFormat(string sqlPartition, string schemaId, string imageFormatId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement (Delete The Category) =============================================================
            sqlStatement.Append("DELETE FROM ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".ImageFormat ");

            sqlStatement.Append("WHERE ImageFormatID = '");
            sqlStatement.Append(imageFormatId);
            sqlStatement.Append("' AND AllowDeletion = '1'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();

            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }
    }
}
