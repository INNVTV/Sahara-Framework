using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Sahara.Core.Accounts.Registration.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Logging.PlatformLogs.Types;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Accounts.Registration.Sql.Statements
{
    class InsertStatements
    {
        public DataAccessResponseType InsertNewAccount(RegisterNewAccountModel newAccountModel, Guid newAccountID)
        {
            var response = new DataAccessResponseType {isSuccess = false};

            StringBuilder SqlStatement = new StringBuilder();

            //TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

            //newAccountModel.Provisioned = false;

            //SQL Statement =============================================================
            SqlStatement.Append("INSERT INTO Accounts (");

            SqlStatement.Append("AccountID,");
            SqlStatement.Append("AccountName,");
            SqlStatement.Append("AccountNameKey,");
            SqlStatement.Append("PhoneNumber,");
            SqlStatement.Append("CreatedDate");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@AccountID, ");
            SqlStatement.Append("@AccountName, ");
            SqlStatement.Append("@AccountNameKey, ");
            SqlStatement.Append("@PhoneNumber, ");
            SqlStatement.Append("@CreatedDate");

            SqlStatement.Append(")");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();
			


            
            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@AccountID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@AccountName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@AccountNameKey", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);




            sqlCommand.Parameters["@AccountID"].Value = newAccountID;
            sqlCommand.Parameters["@AccountName"].Value =  newAccountModel.AccountName;
            sqlCommand.Parameters["@AccountNameKey"].Value =  Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(newAccountModel.AccountName);
            sqlCommand.Parameters["@PhoneNumber"].Value = newAccountModel.PhoneNumber;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow; // TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo); //DateTime.Now;

            int insertAccountResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
            }
            catch(Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "inserting a new account into SQL",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        newAccountID.ToString(),
                        newAccountModel.AccountName);



                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();


            if (insertAccountResult == 1)
            {
                response.isSuccess = true;
            }
            else
            {
                response.ErrorMessage = "SQL result was malformed, check data integrity";
            }

            return response;
        }

    }


}
