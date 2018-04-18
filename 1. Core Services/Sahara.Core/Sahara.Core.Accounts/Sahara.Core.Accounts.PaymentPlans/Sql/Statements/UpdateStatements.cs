using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Accounts.PaymentPlans.Sql.Statements
{
    internal static class UpdateStatements
    {
        /*
        internal static bool UpdatePlanName(string paymentPlanName, string newName)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PaymentPlans ");

            sqlStatement.Append("SET PaymentPlanName = '");
            sqlStatement.Append(newName);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PaymentPlanName = '");
            sqlStatement.Append(paymentPlanName);
            sqlStatement.Append("'");

            SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            sqlCommand.Connection.Open();
            int result = sqlCommand.ExecuteNonQuery(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result == 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }
        */

        internal static bool UpdatePlanVisibility(string paymentPlanName, bool isVisible)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PaymentPlans ");

            sqlStatement.Append("SET Visible = '");
            sqlStatement.Append(isVisible);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PaymentPlanName = '");
            sqlStatement.Append(paymentPlanName);
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

        #region Update Plan Limitations

        internal static bool UpdateMaxUsers(string paymentPlanName, int newLimit)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PaymentPlans ");

            sqlStatement.Append("SET MaxUsers = '");
            sqlStatement.Append(newLimit);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PaymentPlanName = '");
            sqlStatement.Append(paymentPlanName);
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

        internal static bool UpdateMaxCategories(string paymentPlanName, int newLimit)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PaymentPlans ");

            sqlStatement.Append("SET MaxCategories = '");
            sqlStatement.Append(newLimit);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PaymentPlanName = '");
            sqlStatement.Append(paymentPlanName);
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

        internal static bool UpdateMaxSubcategories(string paymentPlanName, int newLimit)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PaymentPlans ");

            sqlStatement.Append("SET MaxSubcategories = '");
            sqlStatement.Append(newLimit);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PaymentPlanName = '");
            sqlStatement.Append(paymentPlanName);
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

        internal static bool UpdateMaxTags(string paymentPlanName, int newLimit)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PaymentPlans ");

            sqlStatement.Append("SET MaxTags = '");
            sqlStatement.Append(newLimit);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PaymentPlanName = '");
            sqlStatement.Append(paymentPlanName);
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

        internal static bool UpdateMaxImages(string paymentPlanName, int newLimit)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PaymentPlans ");

            sqlStatement.Append("SET MaxImages = '");
            sqlStatement.Append(newLimit);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PaymentPlanName = '");
            sqlStatement.Append(paymentPlanName);
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

        internal static bool UpdatePlanAllowImageEnhancements(string paymentPlanName, bool allowImageEnhancements)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PaymentPlans ");

            sqlStatement.Append("SET AllowImageEnhancements = '");
            sqlStatement.Append(allowImageEnhancements);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PaymentPlanName = '");
            sqlStatement.Append(paymentPlanName);
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
