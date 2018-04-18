using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Models.DataConnections
{
    public class DefaultRetryStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception ex)
        {
            if (ex != null && ex is SqlException)
            {
                foreach (SqlError error in (ex as SqlException).Errors)
                {
                    switch (error.Number)
                    {
                        case 1205:
                            //System.Diagnostics.Debug.WriteLine("SQL Error: Deadlock condition. Retrying...");
                            return true;

                        case -2:
                            //System.Diagnostics.Debug.WriteLine("SQL Error: Timeout expired. Retrying...");
                            return true;
                    }
                }
            }

            // For all others, do not retry.
            return false;
        }
    }

    public class DatabaseConnections
    {
        public DatabaseConnections()
        {
            _masterDatabaseName = "master";
            _platformDatabaseName = "Platform";
            _accountsDatabaseName = "Accounts";
            //_membershipTableName = ""; // <-- Set based on Account Partition
            //_accountDatabasePartitionName = ""; //< -- Set based on user
            //_inventoryUserName = ""; //< -- Set based on user

            //Retry Policies=================================
            // Get an instance of the RetryManager class.
            //var retryManager = EnterpriseLibraryContainer.Current.GetInstance<RetryManager>();

            // Create a retry policy that uses a default retry strategy from the 
            // configuration.
            retryPolicy = new RetryPolicy<DefaultRetryStrategy>(5, new TimeSpan(0, 0, 3));

            var retryInterval = TimeSpan.FromSeconds(3);
            var strategy = new FixedInterval("fixed", 4, retryInterval);
            var strategies = new List<RetryStrategy> { strategy };
            var manager = new RetryManager(strategies, "fixed");
            RetryManager.SetDefault(manager);

        }

        RetryPolicy retryPolicy;

        public string SqlServerName { get; set; }
        public string SqlUserName { get; set; }
        public string SqlPassword { get; set; }
        //============================================
        private string _platformDatabaseName { get; set; }
        private string _accountsDatabaseName { get; set; }
        //private string _membershipTableName { get; set; } // Remove as it is now sharded
        private string _masterDatabaseName { get; set; }


        //private string _accountDatabasePartitionName;// { get; set; }
        //private string _inventoryUserName;// { get; set; }

        //public string SchemaUserName { get; set; }
        //public string SchemaAccountName { get; set; }
        //==============================================

        /*
        public void SetUserAccountDatabase(string databaseName, Guid AccountID, string AccountNameKey)
        {
            
            _inventoryDatabaseName = databaseName;
            _inventoryUserName = "User_" + AccountID.ToString().Replace("-", "") + "_" + AccountNameKey;

            SchemaUserName = "User_" + AccountID.ToString().Replace("-", "") + "_" + AccountNameKey;
            SchemaAccountName = "Account_" + AccountID.ToString().Replace("-", "") + "_" + AccountNameKey;
             

        }
        public void SetPartitionDatabase(string databaseName)
        {
            _partitionDatabaseName = databaseName;
        }*/


        //public void SetMembershipDatabase(string databaseName)
        //{
        //    _membershipTableName = databaseName;
        //}


        public ReliableSqlConnection MasterSqlConnection
        {
            get
            {
                ReliableSqlConnection sqlConnection = new ReliableSqlConnection(_generateConnectionString(_masterDatabaseName), retryPolicy);
                return sqlConnection;
            }
        }
        public ReliableSqlConnection PlatformSqlConnection
        {
            get
            {
                ReliableSqlConnection sqlConnection = new ReliableSqlConnection(_generateConnectionString(_platformDatabaseName), retryPolicy);
                return sqlConnection;
            }
        }
        public ReliableSqlConnection AccountsSqlConnection
        {
            get
            {
                ReliableSqlConnection sqlConnection = new ReliableSqlConnection(_generateConnectionString(_accountsDatabaseName), retryPolicy);
                return sqlConnection;
            }
        }

        /// <summary>
        /// Returns the databse connectionstring of the databasename passed in:
        /// </summary>
        /// <param name="AccountDatabasePartitionName"></param>
        /// <returns></returns>
        public ReliableSqlConnection DatabasePartitionSqlConnection(string databasePartitionName)
        {
            //_accountDatabasePartitionName = AccountDatabasePartitionName;
            ReliableSqlConnection sqlConnection = new ReliableSqlConnection(_generateConnectionString(databasePartitionName), retryPolicy);
            return sqlConnection;
        }

        /*
        public SqlConnection DatabasePartitionSqlConnection
        {
            get
            {
                SqlConnection sqlConnection = new SqlConnection(_generateConnectionString(_partitionDatabaseName));
                return sqlConnection;
            }
        }*/

        //public SqlConnection MembershipSqlConnection
        //{
        //    get
         //   {
          //      SqlConnection sqlConnection = new SqlConnection(_generateConnectionString(_membershipTableName));
         //       return sqlConnection;
          //  }
        //}

        private string _generateConnectionString(string databaseName)
        {
            StringBuilder connectionString = new StringBuilder();

            connectionString.Append("Server=");
            connectionString.Append(SqlServerName);
            connectionString.Append(".database.windows.net;");

            connectionString.Append("Database=");
            connectionString.Append(databaseName);

            connectionString.Append(";MultipleActiveResultSets=true;");

            connectionString.Append("User ID=");
            connectionString.Append(SqlUserName);

            connectionString.Append(";Password=");
            connectionString.Append(SqlPassword);

            connectionString.Append(";Trusted_Connection=False;Encrypt=True;Persist Security Info=True;"); //<-- Adding Persist Security Info=True; resolved SQL connectivity errors when making multiple calls

            connectionString.Append("Connection Timeout=45;"); //<-- Tripled the default timeout

            return connectionString.ToString();
        
        }
    }
}
