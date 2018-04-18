using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sahara.Core.Platform.Partitioning.Sql.Statements
{
    public static class UpdateStatements
    {  
        internal static bool UpdateSqlPartition(string accountID, string sqlPartitionName)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET SqlPartition = '");
            sqlStatement.Append(sqlPartitionName);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE AccountID = '");
            sqlStatement.Append(accountID);
            sqlStatement.Append("'");



            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result == 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }
    }
}
