using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Platform.Initialization.Sql.Statements
{
    internal static class DeleteStatements
    {
        public static bool DeleteDatabase(string databaseName)
        {
            bool isSuccess = false;

            var sqlStatement = new StringBuilder();

            sqlStatement.Append("Drop Database ");
            sqlStatement.Append(databaseName);

            //execute the query
            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection.CreateCommand();
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
