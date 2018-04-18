using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Accounts.Commerce.Sql.Statements
{
    public static class UpdateStatements
    {

        #region Credits

        internal static bool UpdateAccountCredits(string accountID, int creditsToAddOrSubtract)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET ");
            sqlStatement.Append("Credits = Credits + ");
            sqlStatement.Append(creditsToAddOrSubtract);

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

        #endregion

    }
}