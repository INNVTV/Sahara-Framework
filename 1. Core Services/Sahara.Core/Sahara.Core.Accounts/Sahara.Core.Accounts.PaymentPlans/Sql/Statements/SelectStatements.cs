using Sahara.Core.Accounts.PaymentPlans.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sahara.Core.Accounts.PaymentPlans.Sql.Statements
{
    internal static class SelectStatements
    {

        #region Lists

        internal static List<PaymentPlan> SelectPaymentPlans(bool includeHiddenPlans, bool orderByRateAsc)
        {

            List<PaymentPlan> paymentPlans = new List<PaymentPlan>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select * From PaymentPlans ");

            if (!includeHiddenPlans)
            {
                SqlStatement.Append("Where Visible = '1' ");
            }

            if (orderByRateAsc)
            {
                SqlStatement.Append("Order By MonthlyRate Asc");
            }
            else
            {
                SqlStatement.Append("Order By MonthlyRate Desc");
            }

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                paymentPlans.Add(Transforms.Transforms.DataReader_to_PaymentPlan(reader));
            }

            sqlCommand.Connection.Close();

            return paymentPlans;
        }

        internal static List<PaymentFrequency> SelectPaymentFrequencies()
        {

            List<PaymentFrequency> paymentPaymentFrequencies = new List<PaymentFrequency>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select * From PaymentFrequencies Order By PaymentFrequencyMonths Asc");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                paymentPaymentFrequencies.Add(Transforms.Transforms.DataReader_to_PaymentFrequency(reader));
            }

            sqlCommand.Connection.Close();

            return paymentPaymentFrequencies;
        }

        #endregion

        #region Single Object

        internal static PaymentPlan SelectPaymentPlan(string planName)
        {
            PaymentPlan paymentPlan = null;

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select * From PaymentPlans ");
            SqlStatement.Append("Where PaymentPlanName='");
            SqlStatement.Append(planName);
            SqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                paymentPlan = Transforms.Transforms.DataReader_to_PaymentPlan(reader);
            }

            sqlCommand.Connection.Close();

            return paymentPlan;
        }

        internal static PaymentFrequency SelectPaymentFrequency(string frequencyMonths)
        {

            PaymentFrequency paymentFrequency = null;

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select * From PaymentFrequencies ");
            SqlStatement.Append("Where PaymentFrequencyMonths='");
            SqlStatement.Append(frequencyMonths);
            SqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                paymentFrequency = Transforms.Transforms.DataReader_to_PaymentFrequency(reader);
            }

            sqlCommand.Connection.Close();

            return paymentFrequency;
        }

        #endregion


        #region Bools

        /*
        internal static bool PlanNameExists(string planName)
        {

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            //SqlStatement.Append("Select Count(*) From PaymentPlans Where PaymentPlanName='" + planName + "'");
            SqlStatement.Append("IF EXISTS (SELECT TOP 1 * FROM PaymentPlans WHERE PaymentPlanName='" + planName + "') select 1 else select 0");

            SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            sqlCommand.Connection.Open();

            int result = (int)sqlCommand.ExecuteScalar();
            sqlCommand.Connection.Close();

            if (result <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }*/

        internal static bool AccountsWithPlanExists(string planName)
        {
            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            //SqlStatement.Append("Select Count(*) From PaymentPlans Where PaymentPlanName='" + planName + "'");
            SqlStatement.Append("IF EXISTS (SELECT TOP 1 * FROM Accounts WHERE PaymentPlanName='" + planName + "') select 1 else select 0");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();

            int result = (int)sqlCommand.ExecuteScalarWithRetry();
            sqlCommand.Connection.Close();

            if (result <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        #endregion

        #region Counts

        public static int SelectAccountsOnPlanCount(string paymentPlanName)
        {

            int response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT COUNT(*) AS Count FROM Accounts Where PaymentPlanName = '" + paymentPlanName + "'";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();



            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response = Convert.ToInt32(reader["Count"]);
            }

            sqlCommand.Connection.Close();

            return response;
        }

        #endregion


    }
}
