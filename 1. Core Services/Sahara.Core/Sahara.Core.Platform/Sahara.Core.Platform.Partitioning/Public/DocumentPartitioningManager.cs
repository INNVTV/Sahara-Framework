using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Client.TransientFaultHandling;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Sahara.Core.Common.DocumentModels;
using Sahara.Core.Common.Redis.PlatformManagerServer.Hashes;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Platform.Partitioning.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Application.Search;

namespace Sahara.Core.Platform.Partitioning.Public
{
    public static class DocumentPartitioningManager
    {
        #region Get

        /// <summary>
        /// Gets the next available partition for a particular tier to assign a new or upgraded account to, return partition id in SuccessMessage.
        /// </summary>
        /// <returns></returns>
        public static DataAccessResponseType CreateDocumentCollectionAccountPartition(string accountNameKey, IReliableReadWriteDocumentClient client, string databaseId)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            Uri databaseUri = UriFactory.CreateDatabaseUri(databaseId);

            #region Create DocumentCollection object

            DocumentCollection newCollection = null;

            try
            {
                newCollection = client.CreateDocumentCollectionQuery(databaseUri.ToString()).Where(c => c.Id == accountNameKey).ToArray().FirstOrDefault();
            }
            catch(DocumentClientException de)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            de.GetBaseException(),
                            "attempting to create new Collection on DocumentDB for '" + accountNameKey + "'",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );
            }
                        
            if (newCollection == null)
            {

                //Collection is null so create it:
                var requestOptions = new RequestOptions {
                    //OfferType = Sahara.Core.Settings.Azure.DocumentDB.DefaultCollectionOfferType_AccountPartitions, //<-- Depricated (Standard is the default)
                    OfferThroughput = Sahara.Core.Settings.Azure.DocumentDB.DefaultCollectionOfferThroughput_AccountPartitions
                };
                newCollection = client.CreateDocumentCollectionAsync(databaseUri.ToString(), new DocumentCollection { Id = accountNameKey }, requestOptions).Result;

                if (newCollection != null)
                {

                    //Used to access embedded resources
                    var assembly = Assembly.GetExecutingAssembly();


                    #region Create Triggers on the new Document Collection (REMOVED - NO LONGER REQUIRED TO MAINTAIN COUNTS)

                    /*

                    #region Increment/Decrement Count Triggers

                    //Allows us to track document counts for each Accont as well as the entire collection whenever a document type such as "Product" is added or removed from the collection

                    #region Create Increment Trigger

                    //Delete if exists
                    #region Delete
                    Microsoft.Azure.Documents.Trigger incrementTriggerExists = client.CreateTriggerQuery(newCollection.SelfLink).Where(t => t.Id == "IncrementProductCount").AsEnumerable().FirstOrDefault();
                    if (incrementTriggerExists != null)
                    {
                        client.DeleteTriggerAsync(incrementTriggerExists.SelfLink);
                    }
                    #endregion

                    string incrementProductCount_FileContents = string.Empty;

                    using (Stream stream = assembly.GetManifestResourceStream("Sahara.Core.Platform.Partitioning.DocumentScripts.Triggers.IncrementProductCount.js"))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        incrementProductCount_FileContents = reader.ReadToEnd();
                    }

                    //Create new increment trigger
                    //string incrementTriggerPath = @"DocumentScripts\Triggers\IncrementProductCount.js";
                    //string incrementTriggerId = System.IO.Path.GetFileNameWithoutExtension(incrementTriggerPath);
                    //string incrementTriggerBody = File.ReadAllText(incrementTriggerPath);
                    Microsoft.Azure.Documents.Trigger incrementTrigger = new Microsoft.Azure.Documents.Trigger
                    {
                        Id = "IncrementProductCount",
                        Body = incrementProductCount_FileContents,
                        TriggerOperation = TriggerOperation.Create, //<-- Can only be used as part of a Create call
                        TriggerType = TriggerType.Post //<-- Runs after the Create is complete
                    };

                    client.CreateTriggerAsync(newCollection.SelfLink, incrementTrigger);

                    #endregion

                    #region Create Decrement Trigger

                    //Delete if exists
                    #region Delete
                    Microsoft.Azure.Documents.Trigger decrementTriggerExists = client.CreateTriggerQuery(newCollection.SelfLink).Where(t => t.Id == "DecrementProductCount").AsEnumerable().FirstOrDefault();
                    if (decrementTriggerExists != null)
                    {
                        client.DeleteTriggerAsync(decrementTriggerExists.SelfLink);
                    }
                    #endregion

                    string decrementProductCount_FileContents = string.Empty;

                    using (Stream stream = assembly.GetManifestResourceStream("Sahara.Core.Platform.Partitioning.DocumentScripts.Triggers.DecrementProductCount.js"))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        decrementProductCount_FileContents = reader.ReadToEnd();
                    }

                    //Create new decrement trigger
                    //string decrementTriggerPath = @"DocumentScripts\Triggers\DecrementProductCount.js";
                    //string decrementTriggerId = System.IO.Path.GetFileNameWithoutExtension(decrementTriggerPath);
                    //string decrementTriggerBody = File.ReadAllText(decrementTriggerPath);
                    Microsoft.Azure.Documents.Trigger decrementTrigger = new Microsoft.Azure.Documents.Trigger
                    {
                        Id = "DecrementProductCount",
                        Body = decrementProductCount_FileContents,
                        TriggerOperation = TriggerOperation.Delete, //<-- Can only be used as part of a Delete call
                        TriggerType = TriggerType.Post //<-- Runs after the Delete is complete
                    };

                    client.CreateTriggerAsync(newCollection.SelfLink, decrementTrigger);

                    #endregion

                    #endregion
*/
                    #endregion

                    #region Create the 'CollectionProperties' Document on the new Document Collection (REMOVED - NO LONGER REQUIRED)

                    /*

                    //This document is used to track various properties on the collection such as overall document counts. It is updated by the increment/decrmemnt count triggers when a document is added or removed for an account, etc...

                    var _docId = "CollectionProperties";

                    #region TryDelete (If Exists)

                    var existingCollectionPropertiesDoc = client.CreateDocumentQuery(newCollection.SelfLink).Where(s => s.Id == _docId).AsEnumerable().FirstOrDefault();
                    if (existingCollectionPropertiesDoc != null)
                    {
                        client.DeleteDocumentAsync(existingCollectionPropertiesDoc.SelfLink).ConfigureAwait(false);
                    }

                    #endregion

                    var collectionPropertiesDocument = new CollectionPropertiesDocumentModel { Id = _docId, DocumentCount = 0, ProductCount = 0 };
                    var result = client.CreateDocumentAsync(newCollection.SelfLink, collectionPropertiesDocument);
                    */

                    #endregion



                }
                else
                {
                    PlatformExceptionsHelper.LogErrorAndAlertAdmins(
                            "Could not create new collection on DocumentDB account",
                            "attempting to create new collection on DocumentDB account for '" + accountNameKey + "'",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );
                }

            }

            #endregion

            response.isSuccess = true;
            response.ResponseObject = newCollection;

            return response;

        }

        #endregion

        #region Get Properties

        public static DocumentPartitionCollectionProperties GetDocumentPartitionProperties(string documentPartitionId)
        {
            var response = new DocumentPartitionCollectionProperties();

            #region Removed as we are no longer using triggers to track docdb updates

            /*
            try
            {
                #region Removed since DocDB Client 1.4 allowed for ID based routing

                /*
                var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
                var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
                client.OpenAsync();

                //Connect to the collection for the request
                //TODO: Switch to using ID directly when available
                var collection = client.CreateDocumentCollectionQuery(dbSelfLink).Where(c => c.Id == documentPartitionId).ToArray().FirstOrDefault();

                //Get the CollectionPropertis document from the collection
                var query = client.CreateDocumentQuery<CollectionPropertiesDocumentModel>(collection.SelfLink, new SqlQuerySpec { QueryText = "SELECT * FROM CollectionProperties p WHERE p.id = 'CollectionProperties'" });
                var collectionProperties = query.AsEnumerable().FirstOrDefault();


                //Assign properties
                var documentCount = Convert.ToInt32(collectionProperties.DocumentCount);
                var ProductCount = Convert.ToInt32(collectionProperties.ProductCount);
                var tenantCount = GetDocumentPartition(documentPartitionId).TenantCount; //Each tenant documents

                //response.DocumentCount = Convert.ToInt32(collectionProperties.DocumentCount);
                response.ProductCount = ProductCount;

                //Manipulate DocumentCount property to accurately reflect itself (+1) as well as every AccountProperties document (+ (tenantcount * 1))
                response.DocumentCount = (documentCount + tenantCount) + 1;
                
                //response.DocumentCount = response.DocumentCount + GetDocumentPartition(documentPartitionId).TenantCount; //Each tenant documents
                //response.DocumentCount = response.DocumentCount + 1; // + itself
                * / 

                #endregion

                var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
                client.OpenAsync();

                // Build a collection Uri out of the known IDs using URIFactory helper
                // Helper allow you to properly generate the following URI format for Document DB:
                // "dbs/{xxx}/colls/{xxx}/docs/{xxx}"
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, documentPartitionId);

                //Get the CollectionPropertis document from the collection
                var query = client.CreateDocumentQuery<CollectionPropertiesDocumentModel>(collectionUri.ToString(), "SELECT * FROM CollectionProperties p WHERE p.id = 'CollectionProperties'");
                var collectionProperties = query.AsEnumerable().FirstOrDefault();

                //Assign properties
                var documentCount = Convert.ToInt32(collectionProperties.DocumentCount);
                var ProductCount = Convert.ToInt32(collectionProperties.ProductCount);
                //var tenantCount = GetDocumentPartition(documentPartitionId).TenantCount; //Each tenant documents

                //response.DocumentCount = Convert.ToInt32(collectionProperties.DocumentCount);
                response.ProductCount = ProductCount;

                //Manipulate DocumentCount property to accurately reflect itself (+1) as well as every AccountProperties document (+ (tenantcount * 1))
                response.DocumentCount = (documentCount) + 1;

            }
            catch(Exception e)
            {

            }*/

            #endregion

            return response;
        }

        public static DocumentPartitionTenantCollectionProperties GetDocumentPartitionTenantProperties(string accountId, string documentPartitionId)
        {
            var response = new DocumentPartitionTenantCollectionProperties();

            #region Removed as we are no longer using triggers to track docdb updates

            /*
                try
                {
                    #region Removed since DocDB Client 1.4 allowed for ID based routing
                    /*
                        var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
                        var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
                        client.OpenAsync();

                        //Connect to the collection for the request
                        //TODO: Switch to using ID directly when available
                        var collection = client.CreateDocumentCollectionQuery(dbSelfLink).Where(c => c.Id == documentPartitionId).ToArray().FirstOrDefault();

                        //Get the CollectionPropertis document from the collection
                        var query = client.CreateDocumentQuery<CollectionPropertiesDocumentModel>(collection.SelfLink, new SqlQuerySpec { QueryText = "SELECT * FROM AccountProperties p WHERE p.id = 'AccountProperties" + accountId + "'" });
                        var collectionProperties = query.AsEnumerable().FirstOrDefault();


                        //Assign properties
                        response.ProductCount = Convert.ToInt32(collectionProperties.ProductCount);

                        //Total Document Count = ProductCount + AccountProperties Document:
                        response.DocumentCount = response.ProductCount + 1;
                 
                     * /
                    #endregion

                    var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
                    //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
                    client.OpenAsync();

                    // Build a collection Uri out of the known IDs using URIFactory helper
                    // Helper allow you to properly generate the following URI format for Document DB:
                    // "dbs/{xxx}/colls/{xxx}/docs/{xxx}"
                    Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, documentPartitionId);

                    //Get the CollectionPropertis document from the collection
                    var query = client.CreateDocumentQuery<CollectionPropertiesDocumentModel>(collectionUri.ToString(), "SELECT * FROM AccountProperties p WHERE p.id = 'AccountProperties" + accountId + "'");
                    var collectionProperties = query.AsEnumerable().FirstOrDefault();


                    //Assign properties
                    response.ProductCount = Convert.ToInt32(collectionProperties.ProductCount);

                    //Total Document Count = ProductCount + AccountProperties Document:
                    response.DocumentCount = response.ProductCount + 1;
                }
                catch (Exception e)
                {

                }
*/
            #endregion

            return response;
        }

        #endregion
    }


}