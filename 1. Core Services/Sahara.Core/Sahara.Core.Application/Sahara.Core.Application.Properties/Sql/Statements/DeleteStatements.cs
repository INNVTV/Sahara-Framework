using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties.Sql.Statements
{
    public static class DeleteStatements
    {
        internal static bool DeleteProperty(string sqlPartition, string schemaId, string propertyId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement 1 (Delete All PropertyValues) =============================================================
            sqlStatement.Append("DELETE FROM ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".PropertyValue ");

            sqlStatement.Append(" WHERE PropertyID = '");
            sqlStatement.Append(propertyId);
            sqlStatement.Append("'; ");

            //SQL Statement 2 (Delete The Property) =============================================================
            sqlStatement.Append("DELETE FROM ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Property ");

            sqlStatement.Append(" WHERE PropertyID = '");
            sqlStatement.Append(propertyId);
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

        internal static bool DeletePropertyValue(string sqlPartition, string schemaId, string propertyValueId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement (Delete The Category) =============================================================
            sqlStatement.Append("DELETE FROM ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".PropertyValue ");

            sqlStatement.Append(" WHERE PropertyValueID = '");
            sqlStatement.Append(propertyValueId);
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

        internal static bool DeletePropertySwatch(string sqlPartition, string schemaId, string propertySwatchId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement (Delete The Category) =============================================================
            sqlStatement.Append("DELETE FROM ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".PropertySwatch ");

            sqlStatement.Append(" WHERE PropertySwatchID = '");
            sqlStatement.Append(propertySwatchId);
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
    }
}
