using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;

namespace Sahara.Core.Settings.Azure
{
    public static class DocumentDbClients
    {
        //Accounts Database ----------------
        private static string _accountUrl { get; set; }
        private static string _accountKey { get; set; }
        public static IReliableReadWriteDocumentClient AccountDocumentClient;

        #region Consistency Levels

        #region Details

        /*
         * 
         * You can configure a default consistency level on your database account that applies to all the collections (across all of the databases) under your database account.
         * By default, all reads and queries issued against the user defined resources will use the default consistency level specified on the database account.
         * However, you can lower the consistency level of a specific read/query request by specifying [x-ms-consistency-level] request header.
         * There are four types of consistency levels supported by the DocumentDB replication protocol - these are briefly described below.
         
         * Strong: Strong consistency guarantees that a write is only visible after it is committed durably by the majority quorum of replicas.
         * A write is either synchronously committed durably by both the primary and the quorum of secondaries or it is aborted.
         * A read is always acknowledged by the majority read quorum - a client can never see an uncommitted or partial write and is always guaranteed to read the latest acknowledged write.
         * Strong consistency provides absolute guarantees on data consistency, but offers the lowest level of read and write performance.

         * Bounded staleness: Bounded staleness consistency guarantees the total order of propagation of writes with the possibility that reads lag behind writes by at most K prefixes.
         * The read is always acknowledged by a majority quorum of replicas. The response of a read request specifies its relative freshness (in terms of K).
         * Bounded staleness provides more predictable behavior for read consistency while offering the lowest latency writes.
         * As reads are acknowledged by a majority quorum, read latency is not the lowest offered by the system.

         * Session: Unlike the global consistency models offered by strong and bounded staleness consistency levels, “session” consistency is tailored for a specific client session.
         * Session consistency is usually sufficient since it provides guaranteed monotonic reads, and writes and ability to read your own writes.
         * A read request for session consistency is issued against a replica that can serve the client requested version (part of the session cookie).
         * Session consistency provides predictable read data consistency for a session while offering the lowest latency writes. Reads are also low latency as except in the rare cases, the read will be served by a single replica.

         * Eventual: Eventual consistency is the weakest form of consistency wherein a client may get the values which are older than the ones it had seen before, over time.
         * In the absence of any further writes, the replicas within the group will eventually converge. The read request is served by any secondary index.

            
         * Eventual consistency provides the weakest read consistency but offers the lowest latency for both reads and writes.
         
         */

        #endregion

        //private Microsoft.Azure.Documents.ConsistencyLevel _account_DefaultConsistencyLevel = Microsoft.Azure.Documents.ConsistencyLevel.Eventual;

        #endregion

        private static ConnectionPolicy _connectionPolicy = new ConnectionPolicy { 
            //Since we are running within Azure we use Direct/TCP connections for performance.
            //Web clients can alo use this.
            //External clients like mobile phones that have ReadOnly Keys should use Gateway/Https
            ConnectionMode = ConnectionMode.Direct, 
            ConnectionProtocol = Protocol.Tcp
        };

        public static void InitializeDocumentClients(string accountUrl, string accountKey)
        {
            _accountUrl = accountUrl;
            _accountKey = accountKey;

            //Static DocumentCLient availabel to ENTIRE application
            AccountDocumentClient = new DocumentClient(new Uri(
                    _accountUrl),
                    _accountKey,
                    _connectionPolicy //<--Since we are running within Azure we use Direct/TCP connections for performance. Web clients can alo use this. External clients like mobile phones that have ReadOnly Keys should use Gateway/Https
                    ).AsReliable(new ExponentialBackoff("DefaultRetryPolicy", 15, TimeSpan.FromMilliseconds(5), TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(100), true));

            AccountDocumentClient.OpenAsync(); //<-- We now only call OpenAsync once, upon first creation of the static connection.

            //_applicationItemUrl = applicationItemUrl;
            //_applicationItemKey = applicationItemKey;


            //PERFORMANCE NOTES:

            /*
             * 
             * Use a singleton DocumentDB client for the lifetime of your application 
             * -------------------------------------------------------------
             * Note that each DocumentClient instance is thread-safe and performs efficient connection management and address caching when operating in Direct Mode.
             * To allow efficient connection management and better performance by DocumentClient, it is recommended to use a single instance of DocumentClient per AppDomain for the lifetime of the application.
             * 
             * 
             * Call OpenAsync to avoid startup latency on first request
             * ----------------------------------------------------------------
             * By default, the first request will have a higher latency because it has to fetch the address routing table.
             * To avoid this startup latency on the first request, you should call OpenAsync() once during initialization as follows.
             * 
             */
        }


        /*
        public IReliableReadWriteDocumentClient AccountDocumentClient
        {
            get
            {
                var client = new DocumentClient(new Uri(
                    _accountUrl),
                    _accountKey,
                    _connectionPolicy //<--Since we are running within Azure we use Direct/TCP connections for performance. Web clients can alo use this. External clients like mobile phones that have ReadOnly Keys should use Gateway/Https
                    ).AsReliable(new ExponentialBackoff("DefaultRetryPolicy", 15, TimeSpan.FromMilliseconds(5), TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(100), true));

                return client;
            }
        }*/


    }
    }
