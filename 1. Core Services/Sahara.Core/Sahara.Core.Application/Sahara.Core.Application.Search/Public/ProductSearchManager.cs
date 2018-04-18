using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using Newtonsoft.Json;
using Sahara.Core.Application.DocumentModels.Product;
using Sahara.Core.Application.Search.Enums;
using Sahara.Core.Application.Search.Models.Product;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Types;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Logging.PlatformLogs.Types;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Search
{
    public static class ProductSearchManager
    {

        #region Search Index

        //Used internally by Core Services to search against a clients index
        public static DocumentSearchResult SearchProducts(string searchPartitionName, string searchIndexName, string text, string filter = null, string orderBy = "relevance", int skip = 0, int top = 50)
        {
            DocumentSearchResult azuresearchresult = null;

            //Get from cache first
            var accountSearchIndex = Common.MemoryCacheManager.SearchIndexCache[searchIndexName] as ISearchIndexClient;

            if (accountSearchIndex == null)
            {
                //Not in cache, create: ----------------------------

                //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
                SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);
                accountSearchIndex = searchServiceClient.Indexes.GetClient(searchIndexName);

                //Store in cache: ---------------------
                Common.MemoryCacheManager.SearchIndexCache.Set(searchIndexName, accountSearchIndex, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(Common.MemoryCacheManager.SearchIndexClientCacheTimeInMinutes) });
            }


            //Prepare the query
            var searchParameters = new SearchParameters();

            //Assign filters
            searchParameters.Filter = filter;

            if (orderBy.ToLower() != "relevance")
            {
                searchParameters.OrderBy = new List<string>();
                searchParameters.OrderBy.Add(orderBy);

                //Orderby Format: 'categoryNameKey asc'   (relevance = 'search.Score asc')
            }


            searchParameters.Skip = skip;
            searchParameters.Top = top;
            searchParameters.IncludeTotalResultCount = true;


            //Perform the search
            //NOTE: We add a wildcard at the end to get better results from text searches
            azuresearchresult = accountSearchIndex.Documents.Search(text + "*", searchParameters);

            return azuresearchresult;
        }

        #endregion

        #region Create Index & Manage Fields

        public static bool CreateProductSearchIndex(string accountNamekey, string searchPartitionName, string searchPartitionKey)
        {
            bool result = false;
            try
            {
                //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
                SearchServiceClient searchServiceClient = new SearchServiceClient(searchPartitionName, new SearchCredentials(searchPartitionKey));

                //Create the Data Source (Removed - now updated manually)  ----------------------------
                #region (Removed - now updated manually) 
                /*
                DataSource datasource = new DataSource
                {
                    Name = Common.Methods.Strings.ConvertDocumentCollectionNameToSearchDataSourceName(documentPartitionId),
                    Type = "documentdb",
                    Container = new DataContainer
                    {
                        Name = documentPartitionId,
                        Query = "SELECT p.id, p.AccountID, p.AccountNameKey, p.DocumentType, p.Name, p.NameKey, p.LocationPath, p.FullyQualifiedName, p.Visible, p.Tags, p.IndexedProperties, p._ts FROM Products p WHERE p._ts > @HighWaterMark AND p.DocumentType = 'Product'"
                    },
                    DataChangeDetectionPolicy = new HighWaterMarkChangeDetectionPolicy { HighWaterMarkColumnName = "_ts" },
                    DataDeletionDetectionPolicy = new SoftDeleteColumnDeletionDetectionPolicy { SoftDeleteColumnName = "isDeleted", SoftDeleteMarkerValue = "true" },
                    Credentials = new DataSourceCredentials
                    {
                        ConnectionString = "AccountEndpoint=" + Settings.Azure.DocumentDB.ReadOnlyAccountName + ";AccountKey=" + Settings.Azure.DocumentDB.ReadOnlyAccountKey + ";Database=" + Settings.Azure.DocumentDB.AccountPartitionDatabaseId
                    }

                };

                var datasourceCreated = searchServiceClient.DataSources.Create(datasource);
                */
                #endregion


                //Create Index -----------------------------------
                Microsoft.Azure.Search.Models.Index index = new Microsoft.Azure.Search.Models.Index
                {
                    Name = accountNamekey // + "-products"
                };

                index.Fields = new List<Field>();

                index.Fields.Add(new Field { Name = "id", Type = Microsoft.Azure.Search.Models.DataType.String, IsKey = true, IsFilterable = true });

                //index.Fields.Add(new Field { Name = "AccountID", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, });
                //index.Fields.Add(new Field { Name = "AccountNameKey", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true });

                //index.Fields.Add(new Field { Name = "DocumentType", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true });
                index.Fields.Add(new Field { Name = "name", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true, IsSearchable = true });
                index.Fields.Add(new Field { Name = "nameKey", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true });

                index.Fields.Add(new Field { Name = "locationPath", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true });

                index.Fields.Add(new Field { Name = "fullyQualifiedName", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true, IsSearchable = true });

                index.Fields.Add(new Field { Name = "categoryName", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true, IsSearchable = true });
                index.Fields.Add(new Field { Name = "categoryNameKey", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true, IsSearchable = true });

                index.Fields.Add(new Field { Name = "subcategoryName", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true, IsSearchable = true });
                index.Fields.Add(new Field { Name = "subcategoryNameKey", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true, IsSearchable = true });

                index.Fields.Add(new Field { Name = "subsubcategoryName", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true, IsSearchable = true });
                index.Fields.Add(new Field { Name = "subsubcategoryNameKey", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true, IsSearchable = true });

                index.Fields.Add(new Field { Name = "subsubsubcategoryName", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true, IsSearchable = true });
                index.Fields.Add(new Field { Name = "subsubsubcategoryNameKey", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = true, IsSortable = true, IsSearchable = true });

                index.Fields.Add(new Field { Name = "dateCreated", Type = Microsoft.Azure.Search.Models.DataType.DateTimeOffset, IsFilterable = true, IsSortable = true, IsFacetable = true, IsSearchable = false }); //<-- Dates cannot be searchable

                index.Fields.Add(new Field { Name = "visible", Type = Microsoft.Azure.Search.Models.DataType.Boolean, IsFilterable = true });

                index.Fields.Add(new Field { Name = "orderId", Type = Microsoft.Azure.Search.Models.DataType.Int32, IsSortable = true, IsFacetable = false, IsSearchable = false, IsFilterable = false });

                index.Fields.Add(new Field { Name = "tags", Type = Microsoft.Azure.Search.Models.DataType.Collection(Microsoft.Azure.Search.Models.DataType.String), IsFilterable = true, IsSearchable = true, IsFacetable =true });

                // -- Depricated in favor of new property creaions as needed
                //index.Fields.Add(new Field { Name = "Properties", Type = Microsoft.Azure.Search.Models.DataType.Collection(Microsoft.Azure.Search.Models.DataType.String), IsFilterable = true, IsSearchable = true }); //<-- PropertyName:PropertyValue

                // -- Depricated in favor of brnging in images after search results are created
                //index.Fields.Add(new Field { Name = "thumbnails", Type = Microsoft.Azure.Search.Models.DataType.Collection(Microsoft.Azure.Search.Models.DataType.String), IsFilterable = false, IsSearchable = false }); //<-- ThumbnailName:URL
                //index.Fields.Add(new Field { Name = "images", Type = Microsoft.Azure.Search.Models.DataType.Collection(Microsoft.Azure.Search.Models.DataType.String), IsFilterable = false, IsSearchable = false }); //<-- ThumbnailName:URL

                var indexResult = searchServiceClient.Indexes.Create(index);

                //Create Indexer (Removed - now updated manually) ---------------------------------
                #region (Removed - now updated manually)
                /*
                Indexer indexer = new Indexer
                {
                    Name = containerShortName + "-indexer",

                    Schedule = new IndexingSchedule
                    {
                        Interval = new TimeSpan(0, 5, 0),
                        StartTime = DateTime.UtcNow
                    },
                    DataSourceName = datasource.Name,
                    //Description = "",
                    TargetIndexName = index.Name
                    //Parameters = new IndexingParameters
                    //{

                    //},
                };

                searchServiceClient.Indexers.Create(indexer);
                */
                #endregion

                if(indexResult != null)
                {
                    result = true;
                }

            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to create product search index for '" + accountNamekey + "'",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );

                PlatformLogManager.LogActivity(
                            CategoryType.ManualTask,
                            ActivityType.ManualTask_Search,
                            "Product search index creation failed for '" + accountNamekey + "'",
                            "You may need to manually create product search index for '" + accountNamekey + "'",
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            System.Reflection.MethodBase.GetCurrentMethod().ToString()
                        );
            }

            return result;
        }

        public static DataAccessResponseType AddProductPropertyToSearchIndexFields(string searchPartition, string indexName, string fieldName, string propertyTypeNameKey) //, string searchPartitionName, string searchPartitionKey)
        {
            var searchUpdateResponse = new DataAccessResponseType();

            try
            {
                #region Determine Search Field DataType from PropertyType

                //Default is string: 
                Microsoft.Azure.Search.Models.DataType fieldDataType = Microsoft.Azure.Search.Models.DataType.String;

                switch (propertyTypeNameKey)
                {
                    case "number":
                        fieldDataType = Microsoft.Azure.Search.Models.DataType.Double;
                        break;

                    case "datetime":
                        fieldDataType = Microsoft.Azure.Search.Models.DataType.DateTimeOffset;
                        break;

                    case "location":
                        fieldDataType = Microsoft.Azure.Search.Models.DataType.GeographyPoint;
                        break;

                    case "predefined":
                        fieldDataType = Microsoft.Azure.Search.Models.DataType.Collection(Microsoft.Azure.Search.Models.DataType.String);
                        break;

                    case "swatch":
                        fieldDataType = Microsoft.Azure.Search.Models.DataType.Collection(Microsoft.Azure.Search.Models.DataType.String);
                        break;

                    default:
                        break;
                }

                #endregion

                //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
                //SearchServiceClient searchServiceClient = new SearchServiceClient(searchPartitionName, new SearchCredentials(searchPartitionKey));

                //Get search partition
                SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartition);

                //Get Index -----------------------------------
                Microsoft.Azure.Search.Models.Index index = searchServiceClient.Indexes.Get(indexName);

                //Manage field options based on DataType

                bool isSearchable = false;
                bool isSortable = true;
                bool isFacetable = true;
                bool isRetrievable = true;
                bool isFilterable = true;

                if (propertyTypeNameKey == "paragraph")
                {
                    isSearchable = true; //<-- Only Strings and Collections of Strngs can be searchable
                    isFacetable = false; //<-- Paragraphs SHOULD not be set as facetable in Azure search
                    isSortable = false; //<-- Paragraphs SHOULD not be set as sortable in Azure search
                    //isRetrievable = false;  //<-- Paragraphs SHOULD not be returned in results (Now we do return paragraphs since we use this for detail pages)
                    isFilterable = false;  //<-- Paragraphs SHOULD not be filterable
                }
                else if(propertyTypeNameKey == "string")
                {
                    isFacetable = false; //<-- Allows for string searches of ID's, etc. + Saves storage on search index on fre form strings
                    isSearchable = true;
                }              
                else if (propertyTypeNameKey == "predefined" || propertyTypeNameKey == "swatch")
                {
                    isSearchable = true; //<-- Collections CAN be searchable
                    isSortable = false; //<-- Collections CAN NOT be set as sortable in Azure search
                }
                else if (propertyTypeNameKey == "location")
                {
                    isFacetable = false; //<-- Geography points CAN NOT be set as facetable in Azure search
                }

                index.Fields.Add(new Field { Name = fieldName, Type = fieldDataType, IsFilterable = isFilterable, IsRetrievable = isRetrievable, IsSearchable = isSearchable, IsFacetable = isFacetable, IsSortable = isSortable });

                #region Add an additional string metadata field for geography data

                //Allows us to search for address copy in order to get back products with the associated geographic points
                //We append the term "LocationMetadata" to the field name and create a SEARCHABLE ONLY field 
                //We now added ability to retrieve so it can be unpacked by API calls and merged with location results

                if (propertyTypeNameKey == "location")
                {
                    index.Fields.Add(new Field { Name = fieldName + "LocationMetadata", Type = Microsoft.Azure.Search.Models.DataType.String, IsFilterable = false, IsRetrievable = true, IsSearchable = true, IsFacetable = false, IsSortable = false });
                }

                #endregion

                var indexResult = searchServiceClient.Indexes.CreateOrUpdate(index);

                if (indexResult != null)
                {
                    searchUpdateResponse.isSuccess = true;
                }

            }
            catch (Exception e)
            {

                searchUpdateResponse.isSuccess = false;
                searchUpdateResponse.ErrorMessage = e.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to create new field '" + fieldName + "' for index '" + indexName + "'",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );

                /*
                PlatformLogManager.LogActivity(
                            CategoryType.ManualTask,
                            ActivityType.ManualTask_Search,
                            "Search index field creation failed during setup of new field '" + fieldName + "' on index '" + indexName + "'",
                            "You may need to manually create index field '" + fieldName + "' on index '" + indexName + "'",
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            System.Reflection.MethodBase.GetCurrentMethod().ToString()
                        );*/
            }

            return searchUpdateResponse;
        }


        //Unused: Waiting for ability to remove fields in Azure Search
        public static bool RemoveProductSearchIndexField(string indexName, string fieldName, string searchPartitionName, string searchPartitionKey)
        {
            return false;
            /*
            bool result = false;
            try
            {
                SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;


                //Get Index -----------------------------------
                Microsoft.Azure.Search.Models.Index index = searchServiceClient.Indexes.Get(indexName);

                foreach(Field field in index.Fields)
                {
                    if(field.Name == fieldName)
                    {
                        index.Fields.Remove(field);
                    }
                }

                var indexResult = searchServiceClient.Indexes.CreateOrUpdate(index);

                if (indexResult != null)
                {
                    result = true;
                }

            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to create new field '" + fieldName + "' for index '" + indexName + "'",
                            System.Reflection.MethodBase.GetCurrentMethod()
                        );

                PlatformLogManager.LogActivity(
                            CategoryType.ManualTask,
                            ActivityType.ManualTask_Search,
                            "Search index field creation failed during setup of new field '" + fieldName + "' on index '" + indexName + "'",
                            "You may need to manually create index field '" + fieldName + "' on index '" + indexName + "'",
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            System.Reflection.MethodBase.GetCurrentMethod().ToString()
                        );
            }

            return result;*/
        }

        #endregion

        #region Create Search Document in Index

        public static void CreateProductDocumentInSearchIndex(string searchPartitionName, string searchIndexName, ProductDocumentModel productDocumentModel)
        {

            //Get from cache first
            var accountSearchIndex = Common.MemoryCacheManager.SearchIndexCache[searchIndexName] as ISearchIndexClient;

            if (accountSearchIndex == null)
            {
                //Not in cache, create: ----------------------------

                //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
                SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);
                accountSearchIndex = searchServiceClient.Indexes.GetClient(searchIndexName);

                //Store in cache: ---------------------
                Common.MemoryCacheManager.SearchIndexCache.Set(searchIndexName, accountSearchIndex, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(Common.MemoryCacheManager.SearchIndexClientCacheTimeInMinutes) });
            }

            //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
            //var searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);

            //var searchIndexClient = searchServiceClient.Indexes.GetClient(searchIndexName);

            //Microsoft.Azure.Search.Models.DocumentIndexResult documentIndexResult = null;


            #region Convert to list of search documents to add/update

            dynamic productSearchModel = new ProductSearchModel()
            {
                id = productDocumentModel.Id,
                //AccountID = document.AccountID,
                //AccountNameKey = document.AccountNameKey,
                //DocumentType = document.DocumentType,
                fullyQualifiedName = productDocumentModel.FullyQualifiedName,
                locationPath = productDocumentModel.LocationPath,

                categoryName = productDocumentModel.CategoryName,
                subcategoryName = productDocumentModel.SubcategoryName,
                subsubcategoryName = productDocumentModel.SubsubcategoryName,
                subsubsubcategoryName = productDocumentModel.SubsubsubcategoryName,

                categoryNameKey = productDocumentModel.CategoryNameKey,
                subcategoryNameKey = productDocumentModel.SubcategoryNameKey,
                subsubcategoryNameKey = productDocumentModel.SubsubcategoryNameKey,
                subsubsubcategoryNameKey = productDocumentModel.SubsubsubcategoryNameKey,

                name = productDocumentModel.Name,
                nameKey = productDocumentModel.NameKey,
                orderId = productDocumentModel.OrderID,
                //OrderID = document.OrderID,
                //title = productDocumentModel.Title,
                visible = productDocumentModel.Visible,

                dateCreated = productDocumentModel.DateCreated

            };
 

            #endregion

            var searchUpdateBatch = new List<dynamic>();
            searchUpdateBatch.Add(productSearchModel);
            var uploadOrMergeAction = IndexBatch.MergeOrUpload(searchUpdateBatch.ToArray());
            Microsoft.Azure.Search.Models.DocumentIndexResult documentIndexResult = null;
            documentIndexResult = accountSearchIndex.Documents.Index(uploadOrMergeAction);
        }

        #endregion

        #region Update Search Document in Index

        // REMOVE
        public static void UpdateProductDocumentsInSearchIndex(string accountNameKey, string searchPartitionName, string searchIndexName, List<ProductDocumentModel> documents, ProductSearchIndexAction action)
        {

            //Get from cache first
            var accountSearchIndex = Common.MemoryCacheManager.SearchIndexCache[searchIndexName] as ISearchIndexClient;

            if (accountSearchIndex == null)
            {
                //Not in cache, create: ----------------------------

                //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
                SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);
                accountSearchIndex = searchServiceClient.Indexes.GetClient(searchIndexName);

                //Store in cache: ---------------------
                Common.MemoryCacheManager.SearchIndexCache.Set(searchIndexName, accountSearchIndex, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(Common.MemoryCacheManager.SearchIndexClientCacheTimeInMinutes) });
            }


            //try (Removed try catch block in favor of rollbacks in parent/caller method
            //{
            //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
            ///SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);
                ///var searchIndexClient = searchServiceClient.Indexes.GetClient(searchIndexName);

                Microsoft.Azure.Search.Models.DocumentIndexResult documentIndexResult = null;

                switch (action)
                {
                    case ProductSearchIndexAction.Add:
                    case ProductSearchIndexAction.Update:

                        #region Add/Update to Search Index

                        var searchUpdateBatch = new List<dynamic>();

                        #region Convert to list of search documents to add/update

                        foreach (ProductDocumentModel document in documents)
                        {
                            dynamic productSearchModel = new ProductSearchModel()
                            {
                                id = document.Id,
                                //AccountID = document.AccountID,
                                //AccountNameKey = document.AccountNameKey,
                                //DocumentType = document.DocumentType,
                                fullyQualifiedName = document.FullyQualifiedName,
                                locationPath = document.LocationPath,


                                categoryName = document.CategoryName,
                                subcategoryName = document.SubcategoryName,
                                subsubcategoryName = document.SubsubcategoryName,
                                subsubsubcategoryName = document.SubsubsubcategoryName,

                                categoryNameKey = document.CategoryNameKey,
                                subcategoryNameKey = document.SubcategoryNameKey,
                                subsubcategoryNameKey = document.SubsubcategoryNameKey,
                                subsubsubcategoryNameKey = document.SubsubsubcategoryNameKey,

                                dateCreated = document.DateCreated,

                                orderId = document.OrderID,

                                name = document.Name,
                                nameKey = document.NameKey,
                                //OrderID = document.OrderID,
                                //title = document.Title,
                                visible = document.Visible,
                            };


                            if (document.Tags != null)
                            {
                                productSearchModel.tags = document.Tags.ToArray();
                            }

                            /* REMOVED, properties are handled individually
                             * 
                            if (document.Properties != null)
                            {
                                foreach (KeyValuePair<string, string> entry in document.Properties)
                                {                   
                                    //(productSearchModel as IDictionary<String, string>)[entry.Key] = entry.Value;

                                    //ExpandoObject
                                    //(object)productSearchModel.GetType().GetProperty(entry.Key) = entry.Value;
                                    //(dynamic)productSearchModel.GetType().GetProperty("PropertyName") ..[entry.Key] = entry.Value;// .Add(entry.Key + ":" + entry.Value);
                                }

                                //productSearchModel.properties = StringifyProductProperties(document.Properties).ToArray();
                            }
                            */

                            //if (document.xxx != null)
                            //{
                            //productSearchModel.Thumbnails = GenerateIndexedThumbnails(document.);
                            //}

                            searchUpdateBatch.Add(productSearchModel);
                        }

                    #endregion

                        var uploadOrMergeAction = IndexBatch.MergeOrUpload(searchUpdateBatch.ToArray());
                        documentIndexResult = accountSearchIndex.Documents.Index(uploadOrMergeAction);

                        #endregion

                        break;

                    case ProductSearchIndexAction.Delete:

                        #region Delete from Search Index

                        var keyValues = new List<string>();

                        #region Generate list of keys to delete

                        foreach (ProductDocumentModel document in documents)
                        {
                            keyValues.Add(document.Id);
                        }

                        #endregion

                        var deleteAction = Microsoft.Azure.Search.Models.IndexBatch.Delete("id", keyValues);
                        documentIndexResult = accountSearchIndex.Documents.Index(deleteAction);

                        #endregion

                        break;

                    default:
                        break;
                }
                if (documentIndexResult == null)
                {

                }

                //Invalidate Facets Cache
                Internal.SearchCaching.InvalidateProductSearchFacetsCache(accountNameKey);

            //}

            /*
            catch (IndexBatchException ibe)
            {
                #region Handle Exception

                string Ids = String.Empty;

                #region Generate list of document id(s) at issue

                foreach (ProductDocumentModel document in documents)
                {
                    Ids += " [" + document.Id + "]";
                }

                #endregion

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        ibe,
                        "attempting to " + action.ToString().ToLower() + " document(s) to/from search index '" + searchIndexName + "' for documents:" + Ids,
                        System.Reflection.MethodBase.GetCurrentMethod()
                    );

                PlatformLogManager.LogActivity(
                            CategoryType.ManualTask,
                            ActivityType.ManualTask_Search,
                            "Search index " + action.ToString().ToLower() + " action on search index '" + searchIndexName + "' for documents:" + Ids,
                            "You may need to manually " + action.ToString().ToLower() + " for document Id's :" + Ids + " for index '" + searchIndexName + "'",
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            System.Reflection.MethodBase.GetCurrentMethod().ToString()
                        );

                #endregion
            }
            catch (Exception e)
            {
                #region Handle Exception

                string Ids = String.Empty;

                #region Generate list of document id(s) at issue

                foreach (ProductDocumentModel document in documents)
                {
                    Ids += " [" + document.Id + "]";
                }

                #endregion

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to " + action.ToString().ToLower() + " document(s) to/from search index '" + searchIndexName + "' for documents:" + Ids,
                        System.Reflection.MethodBase.GetCurrentMethod()
                    );

                PlatformLogManager.LogActivity(
                            CategoryType.ManualTask,
                            ActivityType.ManualTask_Search,
                            "Search index " + action.ToString().ToLower() + " action on search index '" + searchIndexName + "' for documents:" + Ids,
                            "You may need to manually " + action.ToString().ToLower() + " for document Id's :" + Ids + " for index '" + searchIndexName + "'",
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            System.Reflection.MethodBase.GetCurrentMethod().ToString()
                        );

                #endregion
            }*/

        }

        // 'additionalMetadata' is used for fields such as 'location/geograpypoint' types that require an additional searchable string field to increase search vectors
        public static void UpdateProductPropertyInSearchIndex(string accountNamekey, string searchPartitionName, string searchIndexName, string documentKey, string searchFieldName, string propertyValue, ProductPropertyUpdateType updateType, ProductPropertySearchFieldType propertySearchFieldType, string additionalMetadata = null, GeographyPoint geographyPoint = null)
        {

            //Get from cache first
            var accountSearchIndex = Common.MemoryCacheManager.SearchIndexCache[searchIndexName] as ISearchIndexClient;

            if (accountSearchIndex == null)
            {
                //Not in cache, create: ----------------------------

                //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
                SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);
                accountSearchIndex = searchServiceClient.Indexes.GetClient(searchIndexName);

                //Store in cache: ---------------------
                Common.MemoryCacheManager.SearchIndexCache.Set(searchIndexName, accountSearchIndex, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(Common.MemoryCacheManager.SearchIndexClientCacheTimeInMinutes) });
            }

            // SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
            ///SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);
            ///var searchIndexClient = searchServiceClient.Indexes.GetClient(searchIndexName);

            //Microsoft.Azure.Search.Models.DocumentIndexResult documentIndexResult = null;

            var searchDocument = accountSearchIndex.Documents.Get(documentKey);

            if(updateType == ProductPropertyUpdateType.Replace)
            {
                switch(propertySearchFieldType)
                {
                    case ProductPropertySearchFieldType.String:
                        searchDocument[searchFieldName] = propertyValue;
                        break;
                    case ProductPropertySearchFieldType.Collection:
                        List<string> arrayList = new List<string>();
                        arrayList.Add(propertyValue);
                        searchDocument[searchFieldName] = arrayList.ToArray();
                        break;
                    case ProductPropertySearchFieldType.Location:
                        searchDocument[searchFieldName] = geographyPoint;
                        searchDocument[searchFieldName + "LocationMetadata"] = additionalMetadata;
                        break;
                }
                
            }
            else if(updateType == ProductPropertyUpdateType.Append)
            {
                string[] arrayIn = (string[])searchDocument[searchFieldName];

                var arrayOut = arrayIn.ToList();
                arrayOut.Add(propertyValue);

                searchDocument[searchFieldName] = arrayOut.ToArray();
            }
            //else if (updateType == ProductPropertyUpdateType.Clear)
            //{
                //searchDocument[propertyName] = null;
            //}

            var searchUpdateBatch = new List<dynamic>();
            searchUpdateBatch.Add(searchDocument);
            var uploadOrMergeAction = IndexBatch.MergeOrUpload(searchUpdateBatch.ToArray());
            Microsoft.Azure.Search.Models.DocumentIndexResult documentIndexResult = null;
            documentIndexResult = accountSearchIndex.Documents.Index(uploadOrMergeAction);

            //Invalidate Facets Cache
            Internal.SearchCaching.InvalidateProductSearchFacetsCache(accountNamekey);
        }

        public static void RemoveProductPropertyCollectionItemInSearchIndex(string accountNamekey, string searchPartitionName, string searchIndexName, string documentKey, string searchFieldName, int collectionItemIndex)
        {

            //Get from cache first
            var accountSearchIndex = Common.MemoryCacheManager.SearchIndexCache[searchIndexName] as ISearchIndexClient;

            if (accountSearchIndex == null)
            {
                //Not in cache, create: ----------------------------

                //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
                SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);
                accountSearchIndex = searchServiceClient.Indexes.GetClient(searchIndexName);

                //Store in cache: ---------------------
                Common.MemoryCacheManager.SearchIndexCache.Set(searchIndexName, accountSearchIndex, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(Common.MemoryCacheManager.SearchIndexClientCacheTimeInMinutes) });
            }


            //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
            ///SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);
            ///var searchIndexClient = searchServiceClient.Indexes.GetClient(searchIndexName);

            //Microsoft.Azure.Search.Models.DocumentIndexResult documentIndexResult = null;

            var searchDocument = accountSearchIndex.Documents.Get(documentKey);

            //Get array, make local version, remove item, re-assign to searchfield:
            string[] arrayIn = (string[])searchDocument[searchFieldName];
            var arrayOut = arrayIn.ToList();
            arrayOut.RemoveAt(collectionItemIndex);
            searchDocument[searchFieldName] = arrayOut.ToArray();

            //Update the document in search:
            var searchUpdateBatch = new List<dynamic>();
            searchUpdateBatch.Add(searchDocument);
            var uploadOrMergeAction = IndexBatch.MergeOrUpload(searchUpdateBatch.ToArray());
            Microsoft.Azure.Search.Models.DocumentIndexResult documentIndexResult = null;
            documentIndexResult = accountSearchIndex.Documents.Index(uploadOrMergeAction);

            //Invalidate Facets Cache
            Internal.SearchCaching.InvalidateProductSearchFacetsCache(accountNamekey);
        }

        public static void ClearProductPropertyInSearchIndex(string accountNamekey, string searchPartitionName, string searchIndexName, string documentKey, string searchFieldName, bool isCollection)
        {
            //Get from cache first
            var accountSearchIndex = Common.MemoryCacheManager.SearchIndexCache[searchIndexName] as ISearchIndexClient;

            if (accountSearchIndex == null)
            {
                //Not in cache, create: ----------------------------

                //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
                SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);
                accountSearchIndex = searchServiceClient.Indexes.GetClient(searchIndexName);

                //Store in cache: ---------------------
                Common.MemoryCacheManager.SearchIndexCache.Set(searchIndexName, accountSearchIndex, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(Common.MemoryCacheManager.SearchIndexClientCacheTimeInMinutes) });
            }

            //SearchServiceClient searchServiceClient = Settings.Azure.Search.AccountsSearchServiceClient;
            ///SearchServiceClient searchServiceClient = Settings.Azure.Search.GetSearchPartitionClient(searchPartitionName);
            ///var searchIndexClient = searchServiceClient.Indexes.GetClient(searchIndexName);

            //Microsoft.Azure.Search.Models.DocumentIndexResult documentIndexResult = null;

            var searchDocument = accountSearchIndex.Documents.Get(documentKey);

            if(!isCollection)
            {
                searchDocument[searchFieldName] = null;
            }
            else
            {
                searchDocument[searchFieldName] = new string[0];
            }
            
            var searchUpdateBatch = new List<dynamic>();
            searchUpdateBatch.Add(searchDocument);
            var uploadOrMergeAction = IndexBatch.MergeOrUpload(searchUpdateBatch.ToArray());
            Microsoft.Azure.Search.Models.DocumentIndexResult documentIndexResult = null;
            documentIndexResult = accountSearchIndex.Documents.Index(uploadOrMergeAction);

            //Invalidate Facets Cache
            Internal.SearchCaching.InvalidateProductSearchFacetsCache(accountNamekey);
        }


        #endregion

        #region Helpers

        /// <summary>
        /// Returns Property Dictionary type to a list of strings to be used for search indexing (until Azure search allows for Lists of dictionaey types)
        /// </summary>
        /// <returns></returns>
        /*
        private static List<string> StringifyProductProperties(Dictionary<string, string> properties)
        {
            var strings = new List<string>();

            foreach (KeyValuePair<string, string> entry in properties)
            {
                strings.Add(entry.Key + ":" + entry.Value);
            }

            return strings;
        }
        */

        #endregion

    }
}
