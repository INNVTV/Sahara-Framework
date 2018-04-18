using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Accounts.Commerce.Sql.Statements
{
    internal static class SelectStatements
    {
        #region Credits

        internal static int SelectCredits(string accountId)
        {
            int response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "Select Credits From Accounts Where AccountID = '" + accountId + "'";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();


            while (reader.Read())
            {
                response = Convert.ToInt32(reader["Credits"].ToString());
            }

            sqlCommand.Connection.Close();

            return response;
        }

        internal static int SelectCreditsInCirculation()
        {
            int response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT SUM(Credits) AS TotalCredits FROM Accounts;";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();


            while (reader.Read())
            {
                response = Convert.ToInt32(reader["TotalCredits"].ToString());
            }

            sqlCommand.Connection.Close();

            return response;
        }

        #endregion

    }
}