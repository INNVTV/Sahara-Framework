using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Platform.Deprovisioning.Sql.Statements
{
    internal class DeleteStatements
    {
        private static string _schemaID;
        private static string _database;

        internal DeleteStatements(string accountID, string database)
        {
            _schemaID = accountID;
            _database = database;
        }

        internal bool DeleteAccount()
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("DELETE FROM Accounts ");
            sqlStatement.Append("WHERE AccountID=@AccountID");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();
			

            sqlCommand.Parameters.Add("@AccountID", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountID"].Value = _schemaID;

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
