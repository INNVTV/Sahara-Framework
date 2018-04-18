using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Settings.Platform
{
    public static class Partitioning
    {
        #region BASE SETTINGS

        /*=======================================================================================
        
            Document Partition Settings are stored in SQL under Documentcs table
        
        =======================================================================================*/
        

        public static int MaximumTenantsPerAccountDatabasePartition = 4; // <-- (10 = 140,000 account limit) -- Amount of tenants/accounts that share each database partition. You can increase this at any time and all existing partitions will balance themselves out. You cannot decrease this number once you begin partitioning
        public static string AccountDatabasebaseSharedPartitionName = "Shared_"; //<-- Shared Account partition base database name(s)
        public static string AccountDatabasebaseDedicatedPartitionName = "Dedicated_"; //<-- Isolated Account partition base database name(s)
        public static int AccountPartitonIdentitySeed = 1000; //<-- First account partition will be named: AccountPartition1001
        public static string AccountPartitionSchemaDesignation = "acc"; //<-- Designamtion placed in front of AccountIDs for schema names in SQL data stores

        public static int MaximumTenantsPerStorageAccount = 4; //<-- 500TB Limit each account / @ 4 accounts = 125TB per account (or: 125,000 GB per account)
 

        #endregion

        #region INITIALIZE

        internal static void Initialize()
        {
            #region Environment Settings

            switch (Environment.Current.ToLower())
            {

                #region Production

                case "production":

                    break;

                #endregion


                #region Stage

                case "stage":

                    MaximumTenantsPerAccountDatabasePartition = 3; //<-- Kept low for partition testing

                    MaximumTenantsPerStorageAccount = 3; //<-- Kept low for partition testing


                    break;

                #endregion


                #region Local/Debug



                case "debug":

                    MaximumTenantsPerAccountDatabasePartition = 2;

                    MaximumTenantsPerStorageAccount = 2; //<-- Kept low for partition testing

                    break;

                case "local":

                    //MaximumTenantsPerAccountDatabasePartition = 10; // <-- 140,000 account limit

                    //MaximumTenantsPerStorageAccount = 5; //<-- 500TB Limit each account / @ 5 accounts = 100TB per account (or: 100,000 GB per account)


                    break;

                #endregion

            }

            #endregion

        }

        #endregion
    }
}
