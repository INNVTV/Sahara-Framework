using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Platform.Custodian.Sql.Statements
{
    public static class SelectStatements
    {
        /// <summary>
        /// Returns a list of AccountID's for accounts that have been marked as "Closed" and are ready for Deprovisioning.
        /// </summary>
        /// <returns></returns>
        public static List<string> SelectClosedAccountsToDeprovision()
        {
            var AccountIDs = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select AccountID From Accounts ");
            SqlStatement.Append("Where AccountEndDate <= '");
            SqlStatement.Append(DateTime.UtcNow);
            SqlStatement.Append("'");
            //Optional
            SqlStatement.Append(" AND ClosureApproved='1'"); //<-- Closure MUST be approved by a Platform Admin first.

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                AccountIDs.Add(reader["AccountID"].ToString());
            }

            sqlCommand.Connection.Close();

            return AccountIDs;
        }

        /// <summary>
        /// Returns a list of AccountID's for accounts that are due for a reminder email {X} days prior to credit card expiration
        /// </summary>
        /// <returns></returns>
        public static List<string> SelectAccountIDsForCreditCardExpirationReminders(int daysTillExpiration)
        {
            var AccountIDs = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //Get the value in days of the next time this worker will process
            var timeLapseBetweenProcesses = TimeSpan.FromMilliseconds(Sahara.Core.Settings.Platform.Custodian.Frequency.Length).TotalDays;

            //We create a padding of about 30 minutes of time for before and after the custodian processes the next request (60 minutes total) to give us double coverage on the date ranges which also allows for downtime & other issues
            var paddingInDays = TimeSpan.FromMilliseconds(1800000).TotalDays; 

            //SQL Statement =============================================================
            SqlStatement.Append("Select AccountID From Accounts ");
            SqlStatement.Append("Where CardExpiration >= '");
            SqlStatement.Append(DateTime.UtcNow.AddDays(daysTillExpiration - paddingInDays));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND CardExpiration <= '");
            SqlStatement.Append(DateTime.UtcNow.AddDays(daysTillExpiration + timeLapseBetweenProcesses + paddingInDays));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND StripeSubscriptionID IS NOT NULL"); //<-- We only apply dunning reminders to recurring subscribers


            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                AccountIDs.Add(reader["AccountID"].ToString());
            }

            sqlCommand.Connection.Close();

            return AccountIDs;
        }

    }
}
