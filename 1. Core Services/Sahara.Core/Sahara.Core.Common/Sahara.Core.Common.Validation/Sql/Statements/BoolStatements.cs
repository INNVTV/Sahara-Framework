using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Sahara.Core.Common.Validation.Sql.Statements
{
    class SqlBoolStatements
    {
        public bool AccountNameExists(string AccountName)
        {

            bool result = false;

            // Bool Statement =============================================================

            StringBuilder SqlStatement = new StringBuilder();


            SqlStatement.Append("SELECT CASE WHEN EXISTS ");
            SqlStatement.Append("( ");
            SqlStatement.Append("SELECT * FROM ACCOUNTS ");


            SqlStatement.Append("WHERE (AccountName = @AccountName) ");

            SqlStatement.Append("OR (AccountNameKey = @AccountName) ");

            SqlStatement.Append("OR (AccountNameKey = @AccountNameKey) ");

            SqlStatement.Append(") ");
            SqlStatement.Append("THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END");


            // Issue Command ================================================================
            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);

            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Parameters.Add("@AccountName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@AccountNameKey", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountName"].Value = AccountName;
            sqlCommand.Parameters["@AccountNameKey"].Value = Methods.AccountNames.ConvertToAccountNameKey(AccountName);

            sqlCommand.Connection.OpenWithRetry();

            result = Convert.ToBoolean(sqlCommand.ExecuteScalarWithRetry());

            // Close Connection ================================================================

            sqlCommand.Connection.Close();

            // Return Results ================================================================
            return result;
        }

        public bool PlanNameExists(string PlanName)
        {

            bool result = false;

            // Bool Statement =============================================================

            StringBuilder SqlStatement = new StringBuilder();


            SqlStatement.Append("SELECT CASE WHEN EXISTS ");
            SqlStatement.Append("( ");
            SqlStatement.Append("SELECT * FROM PaymentPlans ");


            SqlStatement.Append("WHERE (PaymentPlanName = @PlanName) ");


            SqlStatement.Append(") ");
            SqlStatement.Append("THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END");


            // Issue Command ================================================================
            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Parameters.Add("@PlanName", SqlDbType.NVarChar);

            sqlCommand.Parameters["@PlanName"].Value = PlanName;


            sqlCommand.Connection.OpenWithRetry();

            result = Convert.ToBoolean(sqlCommand.ExecuteScalarWithRetry());

            // Close Connection ================================================================

            sqlCommand.Connection.Close();

            // Return Results ================================================================
            return result;
        }
    }
}
