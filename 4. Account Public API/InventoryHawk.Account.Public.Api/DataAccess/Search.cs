using InventoryHawk.Account.Public.Api.ApplicationImageRecordsService;
using InventoryHawk.Account.Public.Api.ApplicationProductService;
using InventoryHawk.Account.Public.Api.ApplicationPropertiesService;
using InventoryHawk.Account.Public.Api.Models.Json.Common;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace InventoryHawk.Account.Public.Api.DataAccess
{
    public static class Search
    {
        public static Models.Search.SearchResults SearchProducts(AccountManagementService.Account account, string text, string filter = null, string orderBy = "relevance", int skip = 0, int top = 120, bool locationSort = false, string locationSortString = null, bool includeHidden = false)
        {

            if(top > 120)
            {
                //We do not ever allow more than 120 results per page in search
                top = 120;
            }

            //ProductResults productResults = null;
            //List<ProductDocumentModel> products = null;
            //dynamic products = null;
            DocumentSearchResult azuresearchresult = null;


            //Get from cache first
            var accountSearchIndex = Common.SearchIndexCache[account.ProductSearchIndex] as ISearchIndexClient;

            if(accountSearchIndex == null)
            {
                //Not in cache, create: ----------------------------

                //Get client for this accounts search index (Moved to partitioning)
                //var accountSearchIndex = CoreServices.SearchServiceQueryClient.Indexes.GetClient(account.ProductSearchIndex);

                //Get search partition for this account, create client and connect to index: =======================================================

                if (CoreServices.PlatformSettings.SearchParitions == null || CoreServices.PlatformSettings.SearchParitions.ToList().Count == 0)
                {
                    //No Search Partitions Available in Static List, refresh list from Core Services
                    Common.RefreshPlatformSettings();
                }

                var searchPartition = CoreServices.PlatformSettings.SearchParitions.FirstOrDefault(partition => partition.Name == account.SearchPartition);

                if (searchPartition == null)
                {
                    //May be a new partition, refresh platform setting and try again
                    Common.RefreshPlatformSettings();
                    searchPartition = CoreServices.PlatformSettings.SearchParitions.FirstOrDefault(partition => partition.Name == account.SearchPartition);
                }
                var searchServiceClient = new SearchServiceClient(searchPartition.Name, new SearchCredentials(searchPartition.Key));
                accountSearchIndex = searchServiceClient.Indexes.GetClient(account.ProductSearchIndex);

                //Store in cache: ---------------------
                Common.SearchIndexCache.Set(account.ProductSearchIndex, accountSearchIndex, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(Common.SearchIndexClientCacheTimeInMinutes) });

            }




            //Prepare the query
            var searchParameters = new SearchParameters();

            if(includeHidden == false)
            {
                if(String.IsNullOrEmpty(filter))
                {
                    filter = "(visible eq true)";
                }
                else
                {
                    filter += " and (visible eq true)";
                }
                
            }
           
            //Assign filters
            searchParameters.Filter = filter;

           
            //Add location sorting first (always supercedes others) if applicable
            if (locationSort)
            {
                //include location sorting
                //if (locationSortType == "point")
                //{
                searchParameters.OrderBy.Add(locationSortString);
                //}
                //else if (locationSortType == "bounds")
                //{
           
                //}
           
            }


            
                 
           
            searchParameters.Skip = skip;
            searchParameters.Top = top;


            //We only add ordering and result count if we ask for MORE than 1 item back

            searchParameters.OrderBy = new List<string>();

            if (top > 1)
            {
                if (orderBy.ToLower() != "relevance")
                {
                    var orderByArray = orderBy.Split(',');

                    searchParameters.OrderBy.Add(orderBy);
                }

                searchParameters.IncludeTotalResultCount = true;
            }



            //build strings to search against:
            //TO DO:
            //

            bool listingImagesOnly = true;
            //bool includesLocationData = false; //<--If true we will need to merge location Metadata and remove Metadata field

            if (top == 1)
            {
                //This uses the Details Property Fields:

                //$select=categoryNameKey,categoryName,orderId,subcategoryName

                listingImagesOnly = false; //<--Get all images for details
                searchParameters.Select = GetSearchFields(account.AccountNameKey, PropertyListType.Details); //<-- InOrder???
            }
            else
            {
                //This uses the Listings Property Fields:

                //$select=categoryNameKey,categoryName,orderId,subcategoryName
                searchParameters.Select = GetSearchFields(account.AccountNameKey, PropertyListType.Listings); //<-- InOrder???
            }



            //If the query contains dashes and no spaces, wrap it in quotes in order to get GUIDS and SKUS (The indexer replaces "-" with " " when storing into search)
            try
            {
                if (text.Contains("-"))
                {
                    if (!text.Contains(" "))
                    {
                        text = "\"" + text + "\"";
                    }
                }
            }
            catch
            {

            }

            //Perform the search
            //NOTE: We add a wildcard at the end to get better results from text searches
            azuresearchresult = accountSearchIndex.Documents.Search(text + "*", searchParameters);
            
            


            #region Loop through each search result, transform into SearchResult object and append associated 'Listing' type ImageRecords to each results

            var searchresults = new Models.Search.SearchResults();

            searchresults.Count = azuresearchresult.Count;
            searchresults.Returned = azuresearchresult.Results.Count();

            if ((skip + top) >= azuresearchresult.Count)
            {
                searchresults.Remaining = 0;
            }
            else
            {
                searchresults.Remaining = Convert.ToInt32(azuresearchresult.Count) - (skip + top);
            }

            if (skip >= searchresults.Count)
            {
                searchresults.Range = null;
            }
            else if (searchresults.Returned == 0)
            {
                searchresults.Range = null;
            }
            else
            {
                searchresults.Range = (skip + 1) + "-" + (skip + searchresults.Returned);
            }

            searchresults.ContinuationToken = azuresearchresult.ContinuationToken;
            searchresults.Facets = azuresearchresult.Facets;
            searchresults.Coverage = azuresearchresult.Coverage;

            searchresults.Results = new List<Models.Search.Result>();

            foreach (Microsoft.Azure.Search.Models.SearchResult azureresult in azuresearchresult.Results)
            {
                var result = new Models.Search.Result();

                result.Score = azureresult.Score;
                result.Document = azureresult.Document;
                //result.Images = new List<dynamic>();
                //result.Images = new System.Dynamic.ExpandoObject();
                
                result.Images = Dynamics.Images.BuildDynamicImagesListForJson(account.AccountNameKey, "product", azureresult.Document["id"].ToString(), listingImagesOnly);

                searchresults.Results.Add(result);
            }

            #endregion

            return searchresults;
        }

        internal static List<string> GetSearchFields(string accountNameKey, PropertyListType propertyListType)
        {
            List<string> searchFields = null;

            string localCacheKey = accountNameKey + ":searchFields:" + propertyListType.ToString().ToLower();

            #region (Plan A) Get search fields list from local cache

            try
            {
                searchFields = (List<string>)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (searchFields == null)
            {
                #region (Plan B) Get from second layer of Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashApiKey = accountNameKey + ":apicache";
                string hashApiField = "searchFields:" + propertyListType.ToString().ToLower();

                try
                {
                    var redisApiValue = cache.HashGet(hashApiKey, hashApiField);

                    if (redisApiValue.HasValue)
                    {
                        searchFields = JsonConvert.DeserializeObject<List<string>>(redisApiValue);
                    }

                }
                catch
                {

                }

                #endregion

                if(searchFields == null)
                {
                    #region (Plan C) Get Fresh Set of Properties and Rebuild

                    #region Build fresh version of searchFieldList using properties list from Redis Cache or WCF

                    ExecutionType executionTypeResults;
                    var properties = DataAccess.Properties.GetProperties(accountNameKey, propertyListType, out executionTypeResults);

                    searchFields = new List<string>();

                    #region Build Global Fields

                    searchFields.Add("id");
                    searchFields.Add("name");
                    searchFields.Add("nameKey");

                    searchFields.Add("locationPath");
                    searchFields.Add("fullyQualifiedName");

                    searchFields.Add("categoryName");
                    searchFields.Add("categoryNameKey");

                    searchFields.Add("subcategoryName");
                    searchFields.Add("subcategoryNameKey");

                    searchFields.Add("subsubcategoryName");
                    searchFields.Add("subsubcategoryNameKey");

                    searchFields.Add("subsubsubcategoryName");
                    searchFields.Add("subsubsubcategoryNameKey");

                    searchFields.Add("orderId");

                    #endregion

                    #region Add Custom Fields

                    foreach (PropertyModel property in properties)
                    {
                        searchFields.Add(property.SearchFieldName);

                        if(property.PropertyTypeNameKey == "location")
                        {
                            searchFields.Add(property.SearchFieldName + "LocationMetadata"); //<-- Add LocationMetadata
                        }
                    }

                    #endregion

                    #endregion

                    #endregion

                    try
                    {
                        //Cache on Secondary/ APIRedis:
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(searchFields), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }
                    
                }

                //Cache Localy
                HttpRuntime.Cache.Insert(localCacheKey, searchFields, null, DateTime.Now.AddMinutes(Common.SearchFieldsCacheTimeInMinutes), TimeSpan.Zero);
            }


            return searchFields;

        }

        /*
        public static List<ProductDocumentModel> GetProductsInLocationPath(string documentPartition, string propertyName, string value)
        {
            List<ProductDocumentModel> products = null;

            //var client = CoreServices.DocumentDatabases.Accounts_DocumentClient;
            //CoreServices.DocumentDatabases.Accounts_DocumentClient.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(CoreServices.PlatformSettings.DocumentDB.AccountPartitionsDatabaseId, documentPartition);

            string sqlQuery = "SELECT * FROM Products p WHERE p.DocumentType ='Product' AND p." + propertyName + "='" + value + "' ORDER BY p.Name Asc";

            var productResults = CoreServices.DocumentDatabases.Accounts_DocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery);

            //var accountCollection = client.Crea

            //applicationImages = result.ToList();
            products = productResults.ToList();

            /*============================================================================================================================================================
              Since we can only order by 1 item we check and see if we also have to order by OrderID on the product list by seeing if any of the order ids are > 0
            ============================================================================================================================================================* /
            if (products.Any(p => p.OrderID > 0))
            {
                products = products.OrderBy(p => p.OrderID).ToList();
            }

            return products;
        }*/
    }
}