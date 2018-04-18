using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Sahara.Core.Settings.Models.DataConnections
{
    public class StorageConnections
    {
        private string _platformStorageName { get; set; }
        private string _platformStorageKey { get; set; }

        //private string _accountsStorageName { get; set; }
        //private string _accountsStorageKey { get; set; }

        //NOT USED (Turn back on for NOTIFICATIONS)
        //private string _usersStorageName { get; set; }
        //private string _usersStorageKey { get; set; }

        private string _intermediateStorageName { get; set; }
        private string _intermediateStorageKey { get; set; }

        /*
        public StorageConnections(string platformStorageName, string platformStorageKey)
        {
            _platformStorageName = platformStorageName; 
            _platformStorageKey = platformStorageKey;
        }
         */

        public StorageConnections(string platformStorageName, string platformStorageKey, string intermediateStorageName, string intermediateStorageKey)
        {
            _platformStorageName = platformStorageName;
            _platformStorageKey = platformStorageKey;

            //_accountsStorageName = accountsStorageName;
            //_accountsStorageKey = accountsStorageKey;

            //_usersStorageName = usersStorageName;
            //_usersStorageKey = usersStorageKey;

            _intermediateStorageName = intermediateStorageName;
            _intermediateStorageKey = intermediateStorageKey;

            // ---- For Clents ----

            IntermediaryStorageName = _intermediateStorageName;
            IntermediaryStorageKey = intermediateStorageKey;

            //AccountStorageName = _accountsStorageName;
            //AccountStorageKey = _accountsStorageKey;
        }

        // Used to pass access to clients that store files prior to calling WCF for processing
        public string IntermediaryStorageName;
        public string IntermediaryStorageKey;
        //public string AccountStorageName;
        //public string AccountStorageKey;

        /// <summary>
        /// Used to store Platform specific data
        /// </summary>
        public CloudStorageAccount PlatformStorage
        {
            get
            {
                CloudStorageAccount _storageAccount;

                StorageCredentials _storageCredentials = new StorageCredentials(_platformStorageName, _platformStorageKey);

                //string key = Convert.ToBase64String(storageAccount.Credentials.ExportKey());

                _storageAccount = new CloudStorageAccount(_storageCredentials, false);
                
                return _storageAccount;
            }
        }

        /// <summary>
        /// Used to store Account specific data. Each account has a container named after their ID
        /// </summary>
        ///
        /* MOVED TO PARTITIONED
        public CloudStorageAccount AccountsStorage
        {
            get
            {
                CloudStorageAccount _storageAccount;

                StorageCredentials _storageCredentials = new StorageCredentials(_accountsStorageName, _accountsStorageKey);

                //string key = Convert.ToBase64String(storageAccount.Credentials.ExportKey());

                _storageAccount = new CloudStorageAccount(_storageCredentials, false);

                return _storageAccount;
            }
        }
        */

        /// <summary>
        /// Used to store User specific data, such as Notifications.
        /// </summary>
        /// OFF FOR DEFAULT SINCE NOTIFICATIONS ARE NOT USED UNCOMMENT TO TURN ON (Requires Users Storage Account)
        ///
        /*
        public CloudStorageAccount UsersStorage
        {
            get
            {
                CloudStorageAccount _storageAccount;

                StorageCredentials _storageCredentials = new StorageCredentials(_usersStorageName, _usersStorageKey);

                //string key = Convert.ToBase64String(storageAccount.Credentials.ExportKey());

                _storageAccount = new CloudStorageAccount(_storageCredentials, false);

                return _storageAccount;
            }
        }*/

        /// <summary>
        /// Used for short term storage of assets (such as source images) prior to future processing.
        /// Assets are stored in dated containers which are cleaned up by the Custodian at a set interval.
        /// </summary>
        public CloudStorageAccount IntermediateStorage
        {
            get
            {
                CloudStorageAccount _storageAccount;

                StorageCredentials _storageCredentials = new StorageCredentials(_intermediateStorageName, _intermediateStorageKey);

                //string key = Convert.ToBase64String(storageAccount.Credentials.ExportKey());

                _storageAccount = new CloudStorageAccount(_storageCredentials, false);

                return _storageAccount;
            }
        }

         
    }
}
