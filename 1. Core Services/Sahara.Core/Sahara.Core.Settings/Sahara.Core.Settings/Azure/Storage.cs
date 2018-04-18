using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Settings.Models.DataConnections;
using Sahara.Core.Settings.Models.Partitions;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Sahara.Core.Settings.Azure
{
    public static class Storage
    {
        #region PLATFORM STORAGE

        #region Private Properties

        private static StorageConnections _storageConnections;

        #endregion

        #region INITIALIZATION

        internal static void Initialize()
        {
            #region Initialize Storage Connections

            switch (Environment.Current.ToLower())
            {


                #region Production

                case "production":
                    _storageConnections = new StorageConnections(

                        //Platform
                        "[Config_Name]",
                        "[Config_Key]",

                        //Intermediate
                        "[Config_Name]",
                        "[Config_Key]"

                        );
                    break;

                #endregion


                #region Stage

                case "stage":
                    _storageConnections = new StorageConnections(

                        //Platform
                        "[Config_Name]",
                        "[Config_Key]",

                        //Intermediate
                        "[Config_Name]",
                        "[Config_Key]"

                        );
                    break;


                #endregion


                #region Local/Debug

                case "debug":
                    _storageConnections = new StorageConnections(

                        //Platform
                        "[Config_Name]",
                        "[Config_Key]",

                        //Intermediate
                        "[Config_Name]",
                        "[Config_Key]"

                        );
                    break;


                case "local":
                    _storageConnections = new StorageConnections(

                        //Platform
                        "[Config_Name]",
                        "[Config_Key]",

                        //Intermediate
                        "[Config_Name]",
                        "[Config_Key]"
                        
                        );
                    break;

                #endregion

                default:
                    _storageConnections = null;
                    break;


            }

            #endregion

            #region Retreive all storage connecttions from SQL

            //AccountStoragePartitions = RefreshStoragePartitions(); //<-- To avoid issues on startup we wait until first partition request to load static list

            #endregion

        }

        #endregion


        public static StorageConnections StorageConnections
        {
            get
            {
                return _storageConnections;
            }
        }


        #endregion

        #region PARTITIONED ACCOUNT STORAGE

        /* ==============================================
        // PARTITIONED ACCOUNT STORAGE  
        =============================================*/

        //Static list of Storage Partitions for Accounts (created from SQL when CoreServices starts, reset if partition is searched for does not exist)
        internal static List<StoragePartition> AccountStoragePartitions = new List<StoragePartition>();

        //Get & Create CloudStorageAccount from static list (refreshed if needed) for an account storage partition
        public static CloudStorageAccount GetStoragePartitionAccount(string name)
        {
            var partition = GetStoragePartition(name);

            if(partition != null)
            {
                CloudStorageAccount _storageAccount;

                StorageCredentials _storageCredentials = new StorageCredentials(partition.Name, partition.Key);

                //string key = Convert.ToBase64String(storageAccount.Credentials.ExportKey());

                _storageAccount = new CloudStorageAccount(_storageCredentials, false);

                return _storageAccount;
            }
            else
            {
                return null;
            }

            
        }

        //Public method to get a particular storage partiton from the static list available.
        //If cannot be foound we attempt a refresh (in case this is a new partition)
        public static StoragePartition GetStoragePartition(string name)
        {
            StoragePartition storagePartition = null;

            if (AccountStoragePartitions.Count > 0)
            {
                storagePartition = AccountStoragePartitions.FirstOrDefault(partition => partition.Name == name);
            }

            

            if(storagePartition == null)
            {
                //Did not exist, attempt to refresh the static list and try again
                AccountStoragePartitions = RefreshStoragePartitions();

                if (AccountStoragePartitions.Count > 0)
                {
                    storagePartition = AccountStoragePartitions.FirstOrDefault(partition => partition.Name == name);
                }
            }

            return storagePartition;
        }

        //Duplicate SQL code from PartitionManager ONLY for get operations within Settings
        internal static List<StoragePartition> RefreshStoragePartitions()
        {

            List<StoragePartition> partitions = new List<StoragePartition>();

            try
            {
                //Will fail if platform is not initialized, so wraped in Try/Catch
                //Once initialized settings must run initialization startup again

                //SQL Statement =============================================================
                string SqlStatement =
                    "SELECT * From StoragePartitions";

                //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);
                SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
                sqlCommand.CommandText = SqlStatement.ToString();


                sqlCommand.Connection.OpenWithRetry();
                SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

                while (reader.Read())
                {
                    partitions.Add(new StoragePartition { Name = reader["Name"].ToString(), Key = reader["Key"].ToString(), TenantCount = (int)reader["TenantCount"], CDN = reader["CDN"].ToString(), URL = reader["URL"].ToString() });
                }

                sqlCommand.Connection.Close();

            }
            catch
            {

            }

            return partitions;
        }


        #endregion

    }
}
