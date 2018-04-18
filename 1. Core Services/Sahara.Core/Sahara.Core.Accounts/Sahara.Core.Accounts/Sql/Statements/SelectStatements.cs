using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Accounts.Sql.Statements
{
    internal static class SelectStatements
    {
        #region Accounts

        #region Single Account

        internal static Account SelectAccountByNameKey(string accountNameKey, bool includeUsers)
        {
            //Convert any incoming string into a short account name
            accountNameKey = Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(accountNameKey);

            var account = new Account();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select * From Accounts ");

            SqlStatement.Append("JOIN PaymentPlans ");
            SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

            SqlStatement.Append("JOIN PaymentFrequencies ");
            SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

            SqlStatement.Append("Where AccountNameKey=@AccountNameKey");


           

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();


            sqlCommand.CommandText = SqlStatement.ToString();
            sqlCommand.Connection.OpenWithRetry();

            sqlCommand.Parameters.Add("@AccountNameKey", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountNameKey"].Value = accountNameKey;


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            if (!reader.HasRows)
            {
                account = null;
                return account;
            }

            while (reader.Read())
            {
                account = Transforms.Transforms.DataReader_to_Account(reader);
            }

            sqlCommand.Connection.Close();

            if (includeUsers)
            {
                //account.Users = SelectAllAccountUsers(account.AccountID.ToString());
                account.Users = AccountUserManager.GetUsers(account.AccountID.ToString());
            }

            return account;
        }

        internal static Account SelectAccountByID(string accountID, bool includeUsers = true)
        {
            var account = new Account();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select * From Accounts ");

            SqlStatement.Append("JOIN PaymentPlans ");
            SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

            SqlStatement.Append("JOIN PaymentFrequencies ");
            SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

            SqlStatement.Append("Where AccountID= @AccountID");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();

            sqlCommand.Parameters.Add("@AccountID", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountID"].Value = accountID;


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            if(!reader.HasRows)
            {
                account = null;
                return account;
            }

            while (reader.Read())
            {
                account = Transforms.Transforms.DataReader_to_Account(reader);
            }

            sqlCommand.Connection.Close();

            if (includeUsers)
            {
                //account.Users = SelectAllAccountUsers(account.AccountID.ToString());
                account.Users = AccountUserManager.GetUsers(account.AccountID.ToString());
            }

            return account;
        }

        internal static Account SelectAccountByStripeCustomerID(string StripeCustomerID, bool includeUsers)
        {
            var account = new Account();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select * From Accounts ");

            SqlStatement.Append("JOIN PaymentPlans ");
            SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

            SqlStatement.Append("JOIN PaymentFrequencies ");
            SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

            SqlStatement.Append("Where StripeCustomerID= @StripeCustomerID");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();

            sqlCommand.Parameters.Add("@StripeCustomerID", SqlDbType.NVarChar);

            sqlCommand.Parameters["@StripeCustomerID"].Value = StripeCustomerID;


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            if (!reader.HasRows)
            {
                account = null;
                return account;
            }

            while (reader.Read())
            {
                account = Transforms.Transforms.DataReader_to_Account(reader);
            }

            sqlCommand.Connection.Close();

            if (includeUsers)
            {
                //account.Users = SelectAllAccountUsers(account.AccountID.ToString());
                account.Users = AccountUserManager.GetUsers(account.AccountID.ToString());
            }

            return account;
        }

        #endregion

        #region Account Lists

        internal static List<Account> SelectAllAccounts(int pageNumber, int pageSize, string orderBy)
        {
            List<Account> accounts = new List<Account>();

            StringBuilder SqlStatement = new StringBuilder();

                //SQL Statement =============================================================
                SqlStatement.Append("Select * From Accounts ");

                SqlStatement.Append("JOIN PaymentPlans ");
                SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

                SqlStatement.Append("JOIN PaymentFrequencies ");
                SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

                SqlStatement.Append("Order By ");
                SqlStatement.Append(orderBy);

                if(pageNumber > 0)
                {
                    SqlStatement.Append(" Offset ");
                    SqlStatement.Append(((pageNumber - 1) * pageSize));
                    SqlStatement.Append(" Rows FETCH NEXT ");
                    SqlStatement.Append(pageSize);
                    SqlStatement.Append(" Rows ONLY");
                }


            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                var account = Transforms.Transforms.DataReader_to_Account(reader);

                account.Update();

                accounts.Add(account);
            }

            sqlCommand.Connection.Close();

            return accounts;
        }

        internal static List<Account> SelectAllAccounts(int pageNumber, int pageSize, string columnName, string value, string orderBy)
        {

            if(columnName.ToLower() == "PaymentPlanName")
            {
                columnName = "Accounts.PaymentPlanName"; //<--- Since this column is joined we need to call a specific version
            }
            else if (columnName.ToLower() == "PaymentFrequencyMonths")
            {
                columnName = "Accounts.PaymentFrequencyMonths"; //<--- Since this column is joined we need to call a specific version
            }

            List<Account> accounts = new List<Account>();

            StringBuilder SqlStatement = new StringBuilder();

                //SQL Statement =============================================================
                SqlStatement.Append("Select * From Accounts ");
            
                SqlStatement.Append("JOIN PaymentPlans ");
                SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

                SqlStatement.Append("JOIN PaymentFrequencies ");
                SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

                SqlStatement.Append("Where ");
                SqlStatement.Append(columnName);
                SqlStatement.Append(" = '");
                SqlStatement.Append(value);
                SqlStatement.Append("' ");

                SqlStatement.Append("Order By ");
                SqlStatement.Append(orderBy);

                if(pageNumber > 0)
                {
                    SqlStatement.Append(" Offset ");
                    SqlStatement.Append(((pageNumber - 1) * pageSize));
                    SqlStatement.Append(" Rows FETCH NEXT ");
                    SqlStatement.Append(pageSize);
                    SqlStatement.Append(" Rows ONLY");
                }


            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                var account = Transforms.Transforms.DataReader_to_Account(reader);

                account.Update();

                accounts.Add(account);
            }

            sqlCommand.Connection.Close();

            return accounts;
        }

        internal static List<Account> SearchAccounts(string query, string orderBy, int maxResults)
        {
            List<Account> accounts = new List<Account>();

            StringBuilder SqlStatement = new StringBuilder();

            query = query.Replace("'", "''");

            //SQL Statement =============================================================
            SqlStatement.Append("Select Top ");
            SqlStatement.Append(maxResults);
            SqlStatement.Append(" * From Accounts ");

            SqlStatement.Append("JOIN PaymentPlans ");
            SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

            SqlStatement.Append("JOIN PaymentFrequencies ");
            SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

            SqlStatement.Append("Where AccountNameKey Like '%");
            SqlStatement.Append(Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(query));
            SqlStatement.Append("%'");

            SqlStatement.Append(" OR AccountID Like '%");
            SqlStatement.Append(query);
            SqlStatement.Append("%'");

            SqlStatement.Append(" OR StripeCustomerID Like '%");
            SqlStatement.Append(query);
            SqlStatement.Append("%'");

            SqlStatement.Append(" Order By ");
            SqlStatement.Append(orderBy);


            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();

            //Cannot use Sql Parameters to safegaurd against SQL Injection Attacks on LIKE statements (Also unnecessary):
            //sqlCommand.Parameters.Add("@QueryStringShort", SqlDbType.NVarChar);
            //sqlCommand.Parameters["@QueryStringShort"].Value = Sahara.Core.Common.AccountNames.ConvertToAccountNameKey(query);

            //sqlCommand.Parameters.Add("@QueryString", SqlDbType.NVarChar);
           //sqlCommand.Parameters["@QueryString"].Value = query;


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                var account = Transforms.Transforms.DataReader_to_Account(reader);

                account.Update();

                accounts.Add(account);
            }

            sqlCommand.Connection.Close();

            return accounts;
        }


        internal static List<UserAccount> SelectAllAccountsForEmail(string email)
        {
            List<UserAccount> userAccounts = new List<UserAccount>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Accounts.AccountID, Accounts.AccountName, Accounts.AccountNameKey, AccountUsers.AccountOwner, AccountUsers.Email From Accounts, AccountUsers ");
            SqlStatement.Append("Where Accounts.AccountID = AccountUsers.AccountID ");

            SqlStatement.Append("AND AccountUsers.Email = '");
            SqlStatement.Append(email);
            SqlStatement.Append("' ");

            SqlStatement.Append("ORDER BY Accounts.AccountName Asc");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                var userAccount = Transforms.Transforms.DataReader_to_UserAccount(reader);

                userAccounts.Add(userAccount);
            }

            sqlCommand.Connection.Close();

            return userAccounts;
        }


        internal static List<AccountUser> SelectAllAccountUsers(string accountID)
        {
            List<AccountUser> accountUsers = new List<AccountUser>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select * From AccountUsers ");
            SqlStatement.Append("Where AccountId = '");
            SqlStatement.Append(accountID);
            SqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
		    sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                accountUsers.Add(Transforms.Transforms.DataReader_to_BasicAccountUser(reader));
            }

            sqlCommand.Connection.Close();

            return accountUsers;
        }


        internal static List<Account> SelectLockedAccounts()
        {
            var Accounts = new List<Account>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select * From Accounts ");

            SqlStatement.Append("JOIN PaymentPlans ");
            SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

            SqlStatement.Append("JOIN PaymentFrequencies ");
            SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

            SqlStatement.Append("WHERE LockedDate Is Not Null"); //<--Locked Accounts have a LockedDate set

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                Accounts.Add(Transforms.Transforms.DataReader_to_Account(reader));
            }

            sqlCommand.Connection.Close();

            return Accounts;
        }


        internal static List<Account> SelectAccountsPendingClosure()
        {
            var accounts = new List<Account>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select * From Accounts ");

            SqlStatement.Append("JOIN PaymentPlans ");
            SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

            SqlStatement.Append("JOIN PaymentFrequencies ");
            SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

            SqlStatement.Append("WHERE AccountEndDate IS NOT NULL ORDER BY AccountEndDate ASC"); 

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();


            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                accounts.Add(Transforms.Transforms.DataReader_to_Account(reader));
            }

            sqlCommand.Connection.Close();

            return accounts;
        }


        #endregion

        #region AccountOwners

        internal static List<string> SelectAllAccountOwnerEmailsByAccountID(string accountID)
        {
            try
            {
                var ownerEmails = new List<string>();

                StringBuilder SqlStatement = new StringBuilder();

                //SQL Statement =============================================================
                SqlStatement.Append("Select Email ");
                SqlStatement.Append("From AccountUsers ");
                SqlStatement.Append("Where AccountID = '");
                SqlStatement.Append(accountID);
                SqlStatement.Append("' ");

                SqlStatement.Append("AND AccountOwner = '1'");

                //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
                SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			    sqlCommand.CommandText = SqlStatement.ToString();


                sqlCommand.Connection.OpenWithRetry();
                SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

                while (reader.Read())
                {
                    ownerEmails.Add((String)reader["Email"]);
                }

                sqlCommand.Connection.Close();

                return ownerEmails;
            }
            catch(Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to get all account owners of accountId '" + accountID + "'",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }

        }

        public static int SelectAccountOwnersCount(string accountID)
        {
            
            int response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT COUNT(*) AS Count FROM AccountUsers Where AccountID = '" + accountID + "' AND AccountOwner = '1'";

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

        public static int SelectActiveAccountOwnersCount(string accountID)
        {

            int response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT COUNT(*) AS Count FROM AccountUsers Where AccountID = '" + accountID + "' AND AccountOwner = '1'  AND Active = '1'";

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

        internal static List<string> SelectAllAccountOwnerEmailsByStripeCustomerID(string stripeCustomerID)
        {
            try
            {
                var ownerEmails = new List<string>();

                StringBuilder SqlStatement = new StringBuilder();

                //SQL Statement =============================================================
                SqlStatement.Append("Select Email ");
                SqlStatement.Append("From AccountUsers ");
                SqlStatement.Append("Join Accounts ");
                SqlStatement.Append("On Accounts.AccountID = AccountUsers.AccountID ");

                SqlStatement.Append("Where StripeCustomerID = '");
                SqlStatement.Append(stripeCustomerID);
                SqlStatement.Append("' ");

                SqlStatement.Append("AND AccountOwner = '1'");

                //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
                SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			    sqlCommand.CommandText = SqlStatement.ToString();


                sqlCommand.Connection.OpenWithRetry();
                SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

                while (reader.Read())
                {
                    ownerEmails.Add((String)reader["Email"]);
                }

                sqlCommand.Connection.Close();

                return ownerEmails;
            }
            catch(Exception e)
            {

                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to get all account owners of stripeCustomerId '" + stripeCustomerID + "'",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }

        #endregion


        #region AccountIDs, AccountUserIDs & Counts
        /*
        internal static List<String> SelectAllProvisionedAccountIDs()
        {
            var accountIds = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select AccountID ");
            SqlStatement.Append("From Accounts Where Provisioned='1'");

            SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            sqlCommand.Connection.Open();
            SqlDataReader reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
                accountIds.Add((String)reader["AccountID"].ToString());
            }

            sqlCommand.Connection.Close();

            return accountIds;
        }

        
        internal static List<String> SelectProvisionedAccountIDsByFilter(string columnName, string value)
        {
            var accountIds = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select AccountID ");
            SqlStatement.Append("From Accounts ");

            SqlStatement.Append("JOIN PaymentPlans ");
            SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

            SqlStatement.Append("JOIN PaymentFrequencies ");
            SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

            SqlStatement.Append("Where Provisioned='1' ");
            SqlStatement.Append("And ");
            SqlStatement.Append(columnName);
            SqlStatement.Append(" = '");
            SqlStatement.Append(value);
            SqlStatement.Append("' ");

            SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            sqlCommand.Connection.Open();
            SqlDataReader reader = sqlCommand.ExecuteReader();

            while (reader.Read())
            {
                accountIds.Add((String)reader["AccountID"].ToString());
            }

            sqlCommand.Connection.Close();

            return accountIds;
        }*/


        public static int SelectAccountUsersCount(string accountID)
        {

            int response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT COUNT(*) AS Count FROM AccountUsers Where AccountID = '" + accountID + "'";

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

        public static int SelectGlobalUsersCount()
        {

            int response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT COUNT(*) AS Count FROM AccountUsers";

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

        internal static List<string> SelectAllAccountOwnerIDsByAccountID(string accountID)
        {
            var ownerIds = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Id ");
            SqlStatement.Append("From AccountUsers ");
            SqlStatement.Append("Where AccountID = '");
            SqlStatement.Append(accountID);
            SqlStatement.Append("' ");

            SqlStatement.Append("AND AccountOwner = '1'");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                ownerIds.Add((String)reader["Id"]);
            }

            sqlCommand.Connection.Close();

            return ownerIds;
        }

        internal static List<string> SelectAllAccountOwnerIDsByStripeCustomerID(string stripeCustomerID)
        {
            var ownerIds = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Id ");
            SqlStatement.Append("From AccountUsers ");
            SqlStatement.Append("Join Accounts ");
            SqlStatement.Append("On Accounts.AccountID = AccountUsers.AccountID ");

            SqlStatement.Append("Where StripeCustomerID = '");
            SqlStatement.Append(stripeCustomerID);
            SqlStatement.Append("' ");

            SqlStatement.Append("AND AccountOwner = '1'");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                ownerIds.Add((String)reader["Id"]);
            }

            sqlCommand.Connection.Close();

            return ownerIds;
        }


        internal static List<string> SelectAllAccountUserIDsByAccountID(string accountID)
        {
            var userIds = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Id ");
            SqlStatement.Append("From AccountUsers ");
            SqlStatement.Append("Where AccountID = '");
            SqlStatement.Append(accountID);
            SqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                userIds.Add((String)reader["Id"]);
            }

            sqlCommand.Connection.Close();

            return userIds;
        }

        internal static List<string> SelectAllAccountUserIDsByStripeCustomerID(string stripeCustomerID)
        {
            var userIds = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Id ");
            SqlStatement.Append("From AccountUsers ");
            SqlStatement.Append("Join Accounts ");
            SqlStatement.Append("On Accounts.AccountID = AccountUsers.AccountID ");

            SqlStatement.Append("Where StripeCustomerID = '");
            SqlStatement.Append(stripeCustomerID);
            SqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                userIds.Add((String)reader["Id"]);
            }

            sqlCommand.Connection.Close();

            return userIds;
        }

        #endregion

        #region Email Address Lists

        internal static List<String> SelectUserEmailsFromAccount(string accountId, bool accountOwnersOnly)
        {
            var emails = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Email ");
            SqlStatement.Append("From AccountUsers ");

            SqlStatement.Append("WHERE AccountId='");
            SqlStatement.Append(accountId);
            SqlStatement.Append("'");
            if (accountOwnersOnly)
            {
                SqlStatement.Append(" AND AccountOwner='1'");
            }


            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                emails.Add((String)reader["Email"].ToString());
            }

            sqlCommand.Connection.Close();

            return emails;
        }


        internal static List<String> SelectUserEmailsFromAllProvisionedAccounts(bool accountOwnersOnly)
        {
            var emails = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Email ");
            SqlStatement.Append("From AccountUsers ");

            SqlStatement.Append("JOIN Accounts ");
            SqlStatement.Append("ON Accounts.AccountID = AccountUsers.AccountID ");
            SqlStatement.Append("WHERE Accounts.Provisioned='1'");
            if (accountOwnersOnly)
            {
                SqlStatement.Append(" AND AccountUsers.AccountOwner='1'");
            }


            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                emails.Add((String)reader["Email"].ToString());
            }

            sqlCommand.Connection.Close();

            return emails;
        }

        internal static List<String> SelectUserEmailsFromAllProvisionedAccountsByFilter(string columnName, string value, bool accountOwnersOnly)
        {
            var emails = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Email ");
            SqlStatement.Append("From AccountUsers ");

            SqlStatement.Append("JOIN Accounts ");
            SqlStatement.Append("ON Accounts.AccountID = AccountUsers.AccountID ");

            SqlStatement.Append("JOIN PaymentPlans ");
            SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

            SqlStatement.Append("JOIN PaymentFrequencies ");
            SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

            SqlStatement.Append("WHERE Accounts.Provisioned='1'");

            SqlStatement.Append("And ");
            SqlStatement.Append(columnName);
            SqlStatement.Append(" = '");
            SqlStatement.Append(value);
            SqlStatement.Append("'");

            if (accountOwnersOnly)
            {
                SqlStatement.Append(" AND AccountUsers.AccountOwner='1'");
            }

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                emails.Add((String)reader["Email"].ToString());
            }

            sqlCommand.Connection.Close();

            return emails;
        }

        #endregion

        #region UserID Lists

        internal static List<String> SelectUserIDsFromAllProvisionedAccounts(bool accountOwnersOnly)
        {
            var userIds = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Id ");
            SqlStatement.Append("From AccountUsers ");

            SqlStatement.Append("JOIN Accounts ");
            SqlStatement.Append("ON Accounts.AccountID = AccountUsers.AccountID ");
            SqlStatement.Append("WHERE Accounts.Provisioned='1'");
            if (accountOwnersOnly)
            {
                SqlStatement.Append(" AND AccountUsers.AccountOwner='1'");
            }


            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                userIds.Add((String)reader["Id"].ToString());
            }

            sqlCommand.Connection.Close();

            return userIds;
        }

        internal static List<String> SelectUserIDsFromAllProvisionedAccountsByFilter(string columnName, string columnValue, bool accountOwnersOnly)
        {
            var userIds = new List<string>();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            SqlStatement.Append("Select Id ");
            SqlStatement.Append("From AccountUsers ");

            SqlStatement.Append("JOIN Accounts ");
            SqlStatement.Append("ON Accounts.AccountID = AccountUsers.AccountID ");

            SqlStatement.Append("JOIN PaymentPlans ");
            SqlStatement.Append("ON Accounts.PaymentPlanName = PaymentPlans.PaymentPlanName ");

            SqlStatement.Append("JOIN PaymentFrequencies ");
            SqlStatement.Append("ON Accounts.PaymentFrequencyMonths = PaymentFrequencies.PaymentFrequencyMonths ");

            SqlStatement.Append("WHERE Accounts.Provisioned='1'");

            SqlStatement.Append("And ");
            SqlStatement.Append(columnName);
            SqlStatement.Append(" = '");
            SqlStatement.Append(columnValue);
            SqlStatement.Append("'");

            if (accountOwnersOnly)
            {
                SqlStatement.Append(" AND AccountUsers.AccountOwner='1'");
            }


            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                userIds.Add((String)reader["Id"].ToString());
            }

            sqlCommand.Connection.Close();

            return userIds;
        }

        #endregion

        #region Single Property

        internal static string SelectAccountIDByAcountNameKey(string accountNameKey)
        {
            string response = string.Empty;

            //SQL Statement =============================================================
            string SqlStatement =
                "Select AccountID From Accounts Where AccountNameKey = @AccountNameKey";

           // SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Parameters.Add("@AccountNameKey", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountNameKey"].Value = accountNameKey;

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();


            while (reader.Read())
            {
                response = reader["AccountID"].ToString();
            }

            sqlCommand.Connection.Close();

            return response;
        }

        internal static string SelectAccountNameByID(string accountID)
        {
            string response = string.Empty;

            //SQL Statement =============================================================
            string SqlStatement =
                "Select AccountName From Accounts Where AccountID = @AccountID";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Parameters.Add("@AccountID", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountID"].Value = accountID;

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response = reader["AccountName"].ToString();
            }

            sqlCommand.Connection.Close();

            return response;
        }

        internal static string SelectAccountNameKeyByID(string accountID)
        {
            string response = string.Empty;

            //SQL Statement =============================================================
            string SqlStatement =
                "Select AccountNameKey From Accounts Where AccountID = @AccountID";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Parameters.Add("@AccountID", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountID"].Value = accountID;

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response = reader["AccountNameKey"].ToString();
            }

            sqlCommand.Connection.Close();

            return response;
        }

        internal static string SelectStripeCustomerIDByAccountID(string accountID)
        {
            string response = string.Empty;

            //SQL Statement =============================================================
            string SqlStatement =
                "Select StripeCustomerID From Accounts Where AccountID = @AccountID";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Parameters.Add("@AccountID", SqlDbType.NVarChar);

            sqlCommand.Parameters["@AccountID"].Value = accountID;

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response = reader["StripeCustomerID"].ToString();
            }

            sqlCommand.Connection.Close();

            return response;
        }

        internal static int SelectAccountCount(string columnName = null, string value = null)
        {
            int response = 0;

            StringBuilder SqlStatement = new StringBuilder();

            if (columnName == null && value == null)
            {
                SqlStatement.Append("SELECT COUNT(*) FROM Accounts");
            }
            else
            {
                SqlStatement.Append("SELECT COUNT(*) FROM Accounts ");
                SqlStatement.Append("Where ");
                SqlStatement.Append(columnName);
                SqlStatement.Append(" = '");
                SqlStatement.Append(value);
                SqlStatement.Append("'");
            }

            //SQL Statement =============================================================
            //string SqlStatement =
             //   "Select StripeCustomerID From Accounts Where AccountID = @AccountID";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            response = (int)sqlCommand.ExecuteScalarWithRetry();

            sqlCommand.Connection.Close();

            return response;
        }

        internal static int SelectAccountCountCreatedSinceDateTime(DateTime sinceDateTime)
        {
            int response = 0;

            StringBuilder SqlStatement = new StringBuilder();

            
            SqlStatement.Append("SELECT COUNT(*) FROM Accounts ");
            SqlStatement.Append("Where ");
            SqlStatement.Append("CreatedDate");
            SqlStatement.Append(" >= '");
            SqlStatement.Append(sinceDateTime);
            SqlStatement.Append("'");
            
            //SQL Statement =============================================================
            //string SqlStatement =
            //   "Select StripeCustomerID From Accounts Where AccountID = @AccountID";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            response = (int)sqlCommand.ExecuteScalarWithRetry();

            sqlCommand.Connection.Close();

            return response;
        }


        #endregion

        #endregion

    }
}