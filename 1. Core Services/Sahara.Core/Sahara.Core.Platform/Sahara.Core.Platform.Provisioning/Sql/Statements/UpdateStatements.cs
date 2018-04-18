using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using System;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Platform.Provisioning.Sql.Statements
{
    internal static class UpdateStatements
    {       /* 
        internal static bool SetAccountDocumentPartition(string accountID, string documentPartitionId)
        {
            bool isSuccess = false;

            //TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statements =============================================================

            //Update Account =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET DocumentPartition = '");
            sqlStatement.Append(documentPartitionId);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE AccountID = '");
            sqlStatement.Append(accountID);
            sqlStatement.Append("'");

            //END STATEMENT =============================================================
            sqlStatement.Append("; ");

            //Increment Partition =============================================================
            sqlStatement.Append("UPDATE DocumentPartitions ");

            sqlStatement.Append("SET TenantCount = ISNULL(TenantCount, 0) + 1");

            sqlStatement.Append(", LastUpdatedDate = '");
            sqlStatement.Append(DateTime.UtcNow);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE DocumentPartitionID = '");
            sqlStatement.Append(documentPartitionId);
            sqlStatement.Append("'");


            try
            {
                //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
                SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
                sqlCommand.CommandText = sqlStatement.ToString();


                sqlCommand.Connection.OpenWithRetry();
                int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

                sqlCommand.Connection.Close();

                if (result == 2)
                {
                    isSuccess = true;
                }

                #region Trace Results
                /*
                PlatformLogManager.LogActivity(
                        CategoryType.Trace,
                        ActivityType.Trace_Statement,
                        "SQL call completed. Result: '" + result + "' out of an expected '2'. isSuccess = '" + isSuccess + "'",
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
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(
                        CategoryType.Error,
                        ActivityType.Error_Exception,
                        "Attempt to update 2 SQL rows to update 'DocumentPartition' on account and increment TenantCount on 'DocumentPartition' resulted in exception",
                        e.Message,
                        accountID,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString()
                    );
                
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
        /*
        
        internal static bool UpdateDocumentPartition(string accountID, string documrntPartitionId)
        {
            bool isSuccess = false;

            //TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET DocumrntPartitionID = '");
            sqlStatement.Append(documrntPartitionId);
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
        }

        
        internal static bool UpdateDocumentDatabaseLinks(string accountID, string documentDatabaseLink, string propertiesCollectionLink, string selfLinkReferencesDocumentLink)
        {
            bool isSuccess = false;

            //TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET DocumentDatabaseLink = '");
            sqlStatement.Append(documentDatabaseLink);
            sqlStatement.Append("', ");

            sqlStatement.Append("PropertiesCollectionLink = '");
            sqlStatement.Append(propertiesCollectionLink);
            sqlStatement.Append("', ");

            sqlStatement.Append(" SelfLinkReferencesDocumentLink = '");
            sqlStatement.Append(selfLinkReferencesDocumentLink);
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
        }
        */
        internal static bool UpdateProvisiongStatus(string accountID, bool isProvisioned, bool isActive)
        {
            bool isSuccess = false;

            //TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET Provisioned = '");
            sqlStatement.Append(isProvisioned);
            sqlStatement.Append("', ");

            sqlStatement.Append(" Active = '");
            sqlStatement.Append(isActive);
            sqlStatement.Append("', ");

            sqlStatement.Append("ProvisionedDate = '");
            sqlStatement.Append(DateTime.UtcNow); //TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo)); //DateTime.Now);
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

        internal static bool UpdateProvisiongStatus(string accountID, bool isProvisioned, bool isActive, string storagePartition, string searchPartition)
        {
            bool isSuccess = false;

            //TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE Accounts ");

            sqlStatement.Append("SET StoragePartition = '");
            sqlStatement.Append(storagePartition);
            sqlStatement.Append("', ");

            sqlStatement.Append("SearchPartition = '");
            sqlStatement.Append(searchPartition);
            sqlStatement.Append("', ");

            sqlStatement.Append("Provisioned = '");
            sqlStatement.Append(isProvisioned);
            sqlStatement.Append("', ");

            sqlStatement.Append("Active = '");
            sqlStatement.Append(isActive);
            sqlStatement.Append("', ");

            sqlStatement.Append("ProvisionedDate = '");
            sqlStatement.Append(DateTime.UtcNow); //TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo)); //DateTime.Now);
            sqlStatement.Append("' ");

            sqlStatement.Append("WHERE AccountID = '");
            sqlStatement.Append(accountID);
            sqlStatement.Append("'; ");

            //Updte Storage Partition =============================================================
            sqlStatement.Append("UPDATE StoragePartitions ");

            sqlStatement.Append("SET TenantCount = (TenantCount + 1) Where Name = '");
            sqlStatement.Append(storagePartition);
            sqlStatement.Append("'; ");

            //Updte Search Partition =============================================================
            sqlStatement.Append("UPDATE SearchPartitions ");

            sqlStatement.Append("SET TenantCount = (TenantCount + 1) Where Name = '");
            sqlStatement.Append(searchPartition);
            sqlStatement.Append("'; ");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result > 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }


    }
}
