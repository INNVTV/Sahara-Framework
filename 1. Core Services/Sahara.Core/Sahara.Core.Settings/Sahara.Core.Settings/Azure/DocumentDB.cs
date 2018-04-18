using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Settings.Models.DataConnections;
using StackExchange.Redis;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents;

namespace Sahara.Core.Settings.Azure
{
    public static class DocumentDB
    {

        #region Private Properties

        //private static DocumentClients _documentClients;

        #endregion

        #region INITIALIZATION

        internal static void Initialize()
        {
            #region Initialize DocumentDB Connections

            switch (Environment.Current.ToLower())
            {


                #region Production

                case "production":

                    //DefaultCollectionOfferType_AccountPartitions = "Standard";
                    DefaultCollectionOfferThroughput_AccountPartitions = 400;

                    ReadOnlyAccountName = "https://[Config_AccountName].documents.azure.com:443/";
                    ReadOnlyAccountKey = "[Config_AccountKey]";

                    DocumentDbClients.InitializeDocumentClients(

                        //AccountDB (Local)
                        "https://[Config_AccountName].documents.azure.com:443/",
                        "[Config_AccountKey]";
                        );




                    break;

                #endregion


                #region Stage

                case "stage":

                    //DefaultCollectionOfferType_AccountPartitions = "Standard";
                    DefaultCollectionOfferThroughput_AccountPartitions = 400;

                    ReadOnlyAccountName = "https://[Config_AccountName].documents.azure.com:443/";
                    ReadOnlyAccountKey = "[Config_AccountKey]";

                    DocumentDbClients.InitializeDocumentClients(

                        //AccountDB (Local)
                        "https://[Config_AccountName].documents.azure.com:443/",
                        "[Config_AccountKey]"
                        );
                    break;


                #endregion


                #region Local/Debug

                case "debug":

                    //DefaultCollectionOfferType_AccountPartitions = "Standard";
                    ReadOnlyAccountName = "https://[Config_AccountName].documents.azure.com:443/";
                    ReadOnlyAccountKey = "[Config_AccountKey]";

                    DocumentDbClients.InitializeDocumentClients(

                        //AccountDB (Debug)
                        "https://[Config_AccountName].documents.azure.com:443/",
                        "[Config_AccountKey]"
                        );
                    break;


                case "local":

                    //DefaultCollectionOfferType_AccountPartitions = "Standard";
                    DefaultCollectionOfferThroughput_AccountPartitions = 400;

                    ReadOnlyAccountName = "https://[Config_AccountName].documents.azure.com:443/";
                    ReadOnlyAccountKey = "[Config_AccountKey]";

                    DocumentDbClients.InitializeDocumentClients(

                        //AccountDB (Local)
                        "https://[Config_AccountName].documents.azure.com:443/",
                        "[Config_AccountKey]"
                        );

                    break;

                #endregion

                default:

                    break;


            }

            #endregion

            #region Create or Get Databases & Self Links

            #region Create Account Partitions Database

            
            AccountPartitionDatabaseId = "AccountPartitions";

            #endregion

            #endregion

        }

        #endregion

        public static int DefaultCollectionOfferThroughput_AccountPartitions; //<-- 400 - 10000 (Only used with standard - S1-3 DEPRICATED

        public static string ReadOnlyAccountName;
        public static string ReadOnlyAccountKey;

        public static string AccountPartitionDatabaseId;

    }
}
