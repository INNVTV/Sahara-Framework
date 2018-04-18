using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sahara.Core.Accounts.Sql.Statements
{
    public static class BoolStatements
    {
        public static bool SelectAccountActiveState(string accountId)
        {
            bool isActive = false;

            var SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Active From Accounts ");
            SqlStatement.Append("Where AccountID= @AccountID");

            //var sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();

            sqlCommand.Parameters.Add("@AccountID", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountID"].Value = accountId;


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                isActive = (Boolean)reader["Active"];
            }

            sqlCommand.Connection.Close();

            return isActive;
        }

        public static bool SelectAccountDelinquentState(string accountId)
        {
            bool isDelinquent = false;

            var SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Delinquent From Accounts ");
            SqlStatement.Append("Where AccountID= @AccountID");

            //var sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);

            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();



            sqlCommand.Connection.OpenWithRetry();

            sqlCommand.Parameters.Add("@AccountID", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountID"].Value = accountId;


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                isDelinquent = (Boolean)reader["Delinquent"];
            }

            sqlCommand.Connection.Close();

            return isDelinquent;
        }

        public static bool SelectAccountClosureApproval(string accountId)
        {
            bool closureApproved = false;

            var SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select ClosureApproved From Accounts ");
            SqlStatement.Append("Where AccountID= @AccountID");

            //var sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();



            sqlCommand.Connection.OpenWithRetry();

            sqlCommand.Parameters.Add("@AccountID", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountID"].Value = accountId;


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                closureApproved = (Boolean)reader["ClosureApproved"];
            }

            sqlCommand.Connection.Close();

            return closureApproved;
        }
        
    }
}
