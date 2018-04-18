using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sahara.Core.Accounts.Sql.Statements
{
    public static class UpdateStatements
    {

        #region Account


        internal static bool UpdateAccountEndDate(string accountID, DateTime accountEndDate, bool closureApproved)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET ");
            sqlStatement.Append("AccountEndDate = '");
            sqlStatement.Append(accountEndDate);
            sqlStatement.Append("', ");

            sqlStatement.Append("ClosureApproved = '");
            sqlStatement.Append(closureApproved);
            sqlStatement.Append("' ");

            sqlStatement.Append("WHERE AccountID = '");
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

        internal static bool UpdateAccountCreditCardExpirationDate(string accountID, string expirationMonth, string expirationYear)
        {
            bool isSuccess = false;

            //We set the day to the last day of the expiration month
            //By default we use the current time, as this is a time when the use is active, and allows us to shard the reminder emails throughout the day. You can also choose to set the time to be the same time for all users (for example all reminders are sent everyday at noon for all users)
            //DateTime lastDayOfExpirationMonth = new DateTime(Convert.ToInt32("20" + expirationYear), Convert.ToInt32(expirationMonth), DateTime.DaysInMonth(Convert.ToInt32("20" + expirationYear), Convert.ToInt32(expirationMonth)), 12, 0, 0); //<---- Noon, on the last day of the expiration month

            DateTime lastDayOfExpirationMonth = new DateTime(Convert.ToInt32("20" + expirationYear), Convert.ToInt32(expirationMonth), DateTime.DaysInMonth(Convert.ToInt32("20" + expirationYear), Convert.ToInt32(expirationMonth)), DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second); //<---- The current time, on the first day of the expiration month

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET ");
            sqlStatement.Append("CardExpiration = '");
            sqlStatement.Append(lastDayOfExpirationMonth);
            sqlStatement.Append("'");

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


        internal static bool CreateAccountStripeSubscription(string accountID, string stripeCustomerId, string stripeSubscriptionId, string stripeCardId, string newPaymentPlanName, string newPaymentFrequencyMonths)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET StripeCustomerID = '");
            sqlStatement.Append(stripeCustomerId);
            sqlStatement.Append("', ");

            sqlStatement.Append("StripeSubscriptionID = '");
            sqlStatement.Append(stripeSubscriptionId);
            sqlStatement.Append("', ");

            sqlStatement.Append("StripeCardID = '");
            sqlStatement.Append(stripeCardId);
            sqlStatement.Append("', ");


            sqlStatement.Append("PaymentPlanName = '");
            sqlStatement.Append(newPaymentPlanName);
            sqlStatement.Append("', ");

            sqlStatement.Append("PaymentFrequencyMonths = '");
            sqlStatement.Append(newPaymentFrequencyMonths);
            sqlStatement.Append("'");

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

        internal static bool CreateAccountStripeCustomer(string accountID, string stripeCustomerId, string stripeCardId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET StripeCustomerID = '");
            sqlStatement.Append(stripeCustomerId);
            sqlStatement.Append("', ");

            sqlStatement.Append("StripeCardID = '");
            sqlStatement.Append(stripeCardId);
            sqlStatement.Append("' ");

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


        internal static bool UpdateAccountPaymentPlan(string accountID, string newPaymentPlanName, string newPaymentFrequencyMonths)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET PaymentPlanName = '");
            sqlStatement.Append(newPaymentPlanName);
            sqlStatement.Append("', ");

            sqlStatement.Append("PaymentFrequencyMonths = '");
            sqlStatement.Append(newPaymentFrequencyMonths);
            sqlStatement.Append("' ");

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


        internal static bool UpdateAccountStripeCustomerID(string accountID, string stripeCustomerID)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET StripeCustomerID = '");
            sqlStatement.Append(stripeCustomerID);
            sqlStatement.Append("' ");

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

        internal static bool UpdateAccountStripeCardID(string accountID, string stripeCardID)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET StripeCardID = '");
            sqlStatement.Append(stripeCardID);
            sqlStatement.Append("' ");

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

        internal static bool UpdateAccountStripeSubscriptionID(string accountID, string StripeSubscriptionID)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET StripeSubscriptionID = '");
            sqlStatement.Append(StripeSubscriptionID);
            sqlStatement.Append("' ");

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

        /*
        internal static bool UpdateAccountStatus(string accountID, bool isLocked, bool isActive)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET Locked = '");
            sqlStatement.Append(isLocked);
            sqlStatement.Append("', ");

            sqlStatement.Append("Active = '");
            sqlStatement.Append(isActive);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE AccountID = '");
            sqlStatement.Append(accountID);
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
        }*/

        internal static bool UpdateAccountDelinquentState(string accountID, bool isDelinquent)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET Delinquent = '");
            sqlStatement.Append(isDelinquent);
            sqlStatement.Append("'");

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

        internal static bool UpdateAccountActiveStatus(string accountID, bool isActive)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET Active = '");
            sqlStatement.Append(isActive);
            sqlStatement.Append("'");

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


        internal static bool UpdateAccountName(string accountID, string newAccountName)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET AccountName = @AccountName");

            sqlStatement.Append(", AccountNameKey = @AccountNameKey");

            sqlStatement.Append(" WHERE AccountID = '");
            sqlStatement.Append(accountID);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();
			


            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@AccountName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@AccountNameKey", SqlDbType.Text);


            sqlCommand.Parameters["@AccountName"].Value = newAccountName;
            sqlCommand.Parameters["@AccountNameKey"].Value = Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(newAccountName);


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }


        internal static bool UpdateAccountClosureApproval(string accountID, bool isApproved)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET ClosureApproved = '");
            sqlStatement.Append(isApproved);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE AccountID = '");
            sqlStatement.Append(accountID);
            sqlStatement.Append("'");

            sqlStatement.Append(" AND AccountEndDate IS NOT NULL"); //<-- We should ensure that this account is indeed marked for closure

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


        internal static bool ReverseAccountClosure(string accountID)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET ClosureApproved = '0', ");
            sqlStatement.Append("AccountEndDate = NULL ");

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

        #region AccountUser

        internal static bool UpdateUserFullName(string userId, string firstName, string lastName)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE AccountUsers ");

            sqlStatement.Append("SET FirstName = '");
            sqlStatement.Append(firstName);
            sqlStatement.Append("'");

            sqlStatement.Append(", LastName = '");
            sqlStatement.Append(lastName);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE Id = '");
            sqlStatement.Append(userId);
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

        //Will update the users login email as well as their username which will disengage this email from other accounts associated with that email address
        internal static bool UpdateUserEmail(string accountID, string UserID, string email)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE AccountUsers ");

            sqlStatement.Append("SET Email = '");
            sqlStatement.Append(email);
            sqlStatement.Append("'");

            sqlStatement.Append(", UserName = '");
            sqlStatement.Append(Sahara.Core.Common.Methods.AccountUserNames.GenerateGlobalUniqueUserName(email, accountID));
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE Id = '");
            sqlStatement.Append(UserID);
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

        internal static bool UpdateUserOwnerStatus(string UserID, bool isOwner)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE AccountUsers ");

            sqlStatement.Append("SET AccountOwner = '");
            sqlStatement.Append(isOwner);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE Id = '");
            sqlStatement.Append(UserID);
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

        internal static bool UpdateUserActiveState(string UserID, bool isActive)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE AccountUsers ");

            sqlStatement.Append("SET Active = '");
            sqlStatement.Append(isActive);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE Id = '");
            sqlStatement.Append(UserID);
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

        internal static bool UpdateUserPhoto(string accountId, string userId, string imageUrl)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE AccountUsers ");

            /*
            sqlStatement.Append("SET Photo = 'https://");
            sqlStatement.Append(cdnEndpoint);
            sqlStatement.Append("/");
            sqlStatement.Append(accountId);
            sqlStatement.Append("/userphotos/");
            sqlStatement.Append(imageId);
            sqlStatement.Append("'");
            */

            sqlStatement.Append("SET Photo = '");
            sqlStatement.Append(imageUrl);
            sqlStatement.Append("'");


            sqlStatement.Append(" WHERE Id = '");
            sqlStatement.Append(userId);
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

        #region Migrate To Shared Partition
        /*
        internal static bool MigrateAccountDocumentPartitionToShared(string accountID, string newSharedDocumentPartitionId, string previousFreeDocumentPartitionId)
        {
            bool isSuccess = false;

            //TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statements =============================================================

            //Update Account =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET DocumentPartition = '");
            sqlStatement.Append(newSharedDocumentPartitionId);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE AccountID = '");
            sqlStatement.Append(accountID);
            sqlStatement.Append("'");

            //END STATEMENT =============================================================
            sqlStatement.Append("; ");

            //Increment Shared Partition =============================================================
            sqlStatement.Append("UPDATE DocumentPartitions ");

            sqlStatement.Append("SET TenantCount = ISNULL(TenantCount, 0) + 1");

            sqlStatement.Append(", LastUpdatedDate = '");
            sqlStatement.Append(DateTime.UtcNow);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE DocumentPartitionID = '");
            sqlStatement.Append(newSharedDocumentPartitionId);
            sqlStatement.Append("'");

            //END STATEMENT =============================================================
            sqlStatement.Append("; ");

            //Decrement Free Partition =============================================================
            sqlStatement.Append("UPDATE DocumentPartitions ");

            sqlStatement.Append("SET TenantCount = ISNULL(TenantCount, 0) - 1");

            sqlStatement.Append(", LastUpdatedDate = '");
            sqlStatement.Append(DateTime.UtcNow);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE DocumentPartitionID = '");
            sqlStatement.Append(previousFreeDocumentPartitionId);
            sqlStatement.Append("'");


            try
            {
                //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
                SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			    sqlCommand.CommandText = sqlStatement.ToString();


                sqlCommand.Connection.OpenWithRetry();
                int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

                sqlCommand.Connection.Close();

                if (result == 3)
                {
                    isSuccess = true;
                }

                #region Trace Results
                /*
                PlatformLogManager.LogActivity(
                        CategoryType.Trace,
                        ActivityType.Trace_Statement,
                        "SQL call completed. Result: '" + result + "' out of an expected '3'. isSuccess = '" + isSuccess + "'",
                        sqlStatement.ToString(),
                        accountID,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString()
                    );
                * /
                #endregion
            }
            catch(Exception e)
            {
                //Catch the exception

                PlatformLogManager.LogActivity(
                        CategoryType.Error,
                        ActivityType.Error_Exception,
                        "Attempt to update 3 SQL rows to migrate account from 'Free' to 'Shared' and increment & decrement TenantCounts on both 'DocumentPartition(s)' resulted in an exception",
                        e.Message,
                        accountID,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString()
                    );


                //Create Manual Task

                PlatformLogManager.LogActivity(
                        CategoryType.ManualTask,
                        ActivityType.ManualTask_SQL,
                        "SQL Call Failed",
                        sqlStatement.ToString(),
                        accountID,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString()
                    );
            }

            return isSuccess;
        }
    */
        #endregion
    }
}