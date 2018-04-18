using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Accounts.PaymentPlans.Sql.Statements
{
    public static class DeleteStatements
    {
        public static bool DeletePlan(string paymentPlanName)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("DELETE FROM PaymentPlans ");
            sqlStatement.Append("WHERE PaymentPlanName=@paymentPlanName");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();
			


            sqlCommand.Parameters.Add("@paymentPlanName", SqlDbType.NVarChar);

            sqlCommand.Parameters["@paymentPlanName"].Value = paymentPlanName;

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
