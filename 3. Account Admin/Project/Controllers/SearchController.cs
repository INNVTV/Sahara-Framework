using AccountAdminSite.AccountManagementService;
using AccountAdminSite.ApplicationImageRecordsService;
using AccountAdminSite.ApplicationSearchService;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        #region View Controllers

        // GET: /Search/
        public ActionResult Index()
        {
            return View();
        }

        // Used for searchMode variation
        // GET: /Search/{searchMode}
        [Route("Search/{searchMode}")]
        public ActionResult SearchMode(string searchMode)
        {
            if(searchMode == "list" || searchMode == "map")
            {
                return View("Index"); //<---Redirect to main index, angular will take ouver routing
            }
            else
            {
                return null;
            }
            
        }

        #endregion

        #region JSON Services

        #region Product

        [Route("Search/Json/Products")]
        [HttpGet]
        public JsonNetResult SearchProducts(string text, string filter = null, string orderBy = "relevance", int skip = 0, int top = 50, bool locationSort = false, string locationSortString = null)
        {

            //ProductResults productResults = null;
            //List<ProductDocumentModel> products = null;
            //dynamic products = null;
            DocumentSearchResult azuresearchresult = null;

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url).ToLower();
            Account account = Common.GetAccountObject(accountNameKey);

            if (account != null)
            {
                //Get client for this accounts search index 
                //var accountSearchIndex = CoreServices.SearchServiceQueryClient.Indexes.GetClient(account.ProductSearchIndex);

                //Get search partition for this account, create client and connect to index:

                //Get from cache first
                var accountSearchIndex = Common.SearchIndexCache[account.ProductSearchIndex] as ISearchIndexClient;

                if(accountSearchIndex == null)
                {
                    //Not in cache, create: ----------------------------
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

                //Assign filters
                searchParameters.Filter = filter;

                searchParameters.OrderBy = new List<string>();


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


                if (orderBy.ToLower() != "relevance")
                {
                    var orderByList = orderBy.Split(',');
                              
                    foreach(string orderByItem in orderByList)
                    {
                        searchParameters.OrderBy.Add(orderByItem);
                    }       
                    
                    //Orderby Format: 'categoryNameKey asc'   (relevance = 'search.Score asc')
                }






                searchParameters.Skip = skip;
                searchParameters.Top = top;
                searchParameters.IncludeTotalResultCount = true;


                //build strings to search against:
                //TO DO:

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

                //accountSearchIndex.b
                //azuresearchresult = Common.SearchIndexes.FirstOrDefault(i => i.IndexName == "").SearchIndexClient.Documents.Search(text + "*", searchParameters);
                //Common.SearchIndexes["IndexName"].Documents.Search(text + "*", searchParameters);
                
            }
            else
            {
                //Could not resolve Account
            }


            #region Loop through each search result, transform into SearchResult object and append associated 'Listing' type ImageRecords to each results

            var searchresults = new Models.Search.SearchResults();

            searchresults.Count = azuresearchresult.Count;
            searchresults.Returned = azuresearchresult.Results.Count();

            if((skip + top) >= azuresearchresult.Count)
            {
                searchresults.Remaining = 0;
            }
            else
            {
                searchresults.Remaining = Convert.ToInt32(azuresearchresult.Count) - (skip + top);
            }

            if(skip >= searchresults.Count)
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

            foreach(Microsoft.Azure.Search.Models.SearchResult azureresult in azuresearchresult.Results)
            {
                var result = new Models.Search.Result();

                result.Score = azureresult.Score;
                result.Document = azureresult.Document;
                //result.Images = new List<dynamic>();
                result.Images = new System.Dynamic.ExpandoObject();

                //Get all listing image records for this product id
                var imageRecordGroups = ImageRecordsCommon.GetImageRecordsForObject(accountNameKey, account.StoragePartition, "product", azureresult.Document["id"].ToString(), true); //<--Listing images ONLY!

                foreach (ImageRecordGroupModel imageRecordGroup in imageRecordGroups)
                {
                    IDictionary<string, object> dynamicImageGroup = new System.Dynamic.ExpandoObject();

                    //List<IDictionary<string, object>> dynamicImageRecords = new List<IDictionary<string, object>>();

                    foreach (ImageRecordModel imageRecord in imageRecordGroup.ImageRecords)
                    {

                        //IDictionary<string, object> dynamicImageRecord = new System.Dynamic.ExpandoObject(); 
                        IDictionary<string, object> dynamicImageRecordProperties = new System.Dynamic.ExpandoObject();

                        //Since this is ONLY for listings we can ignore creating arrays for gallery image results

                        #region Not needed since this is ALWAYS ONLY listings Look at API version to get LATEST code

                        /*
                        if (imageRecord.Type == "gallery" && imageRecord.GalleryImages != null)
                        {
                            //dynamicImageRecordProperties["urls"] = imageRecord.GalleryImages;

                            List<IDictionary<string, object>> galleryImages = new List<IDictionary<string, object>>();

                            foreach (ImageRecordGalleryModel galleryItem in imageRecord.GalleryImages)
                            {
                                IDictionary<string, object> galleryImage = new System.Dynamic.ExpandoObject();

                                galleryImage["url"] = galleryItem.Url;
                                galleryImage["title"] = galleryItem.Title;
                                galleryImage["description"] = galleryItem.Description;
                                galleryImage["filename"] = galleryItem.FileName;

                                galleryImages.Add(galleryImage);

                            }

                            dynamicImageRecordProperties["images"] = galleryImages;
                        }
                        else
                        {
                            dynamicImageRecordProperties["url"] = imageRecord.Url;
                            dynamicImageRecordProperties["title"] = imageRecord.Title;
                            dynamicImageRecordProperties["description"] = imageRecord.Description;
                            dynamicImageRecordProperties["filename"] = imageRecord.FileName;
                        }*/

                        #endregion

                        dynamicImageRecordProperties["url"] = imageRecord.Url;
                        dynamicImageRecordProperties["title"] = imageRecord.Title;
                        dynamicImageRecordProperties["description"] = imageRecord.Description;
                        dynamicImageRecordProperties["height"] = imageRecord.Height;
                        dynamicImageRecordProperties["width"] = imageRecord.Width;

                        dynamicImageRecordProperties["formatname"] = imageRecord.FormatName;
                        dynamicImageRecordProperties["formatkey"] = imageRecord.FormatNameKey;

                        dynamicImageRecordProperties["filepath"] = imageRecord.FilePath;
                        dynamicImageRecordProperties["filename"] = imageRecord.FileName;

                        //dynamicImageRecord[imageRecord.FormatNameKey] = dynamicImageRecordProperties;

                        if (!((IDictionary<String, object>)result.Images).ContainsKey(imageRecordGroup.GroupNameKey))
                        {
                            result.Images[imageRecordGroup.GroupNameKey] = new System.Dynamic.ExpandoObject();
                        }

                        ((IDictionary<String, Object>)(result.Images[imageRecordGroup.GroupNameKey]))[imageRecord.FormatNameKey] = dynamicImageRecordProperties;


                        //dynamicImageRecords.Add(dynamicImageRecord);

                    }

                    //result.Images[imageRecordGroup.GroupNameKey] = dynamicImageRecords;
                }

                searchresults.Results.Add(result);
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = searchresults;

            return jsonNetResult;

        }

        #endregion


        #region Facets

        [Route("Search/Json/Facets")]
        [HttpGet]
        public JsonNetResult Facets()
        {
            var accountNameKey = Common.GetSubDomain(Request.Url);

            List<ProductSearchFacet> facets = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":search";
                string hashField = "facets:products";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    facets = JsonConvert.DeserializeObject<List<ProductSearchFacet>>(redisValue);
                }


            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (facets == null)
            {
                #region (Plan B) Get data from WCF

                var applicationSearchServiceClient = new ApplicationSearchService.ApplicationSearchServiceClient();

                try
                {
                    applicationSearchServiceClient.Open();
                    facets = applicationSearchServiceClient.GetProductFacets(accountNameKey, true, Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(applicationSearchServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationSearchServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = facets;

            return jsonNetResult;

        }

        #endregion


        #region Sortables

        [Route("Search/Json/Sortables")]
        [HttpGet]
        public JsonNetResult Sortables()
        {
            var accountNameKey = Common.GetSubDomain(Request.Url);

            List<ProductSearchSortable> sortables = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":search";
                string hashField = "sortables:products";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    sortables = JsonConvert.DeserializeObject<List<ProductSearchSortable>>(redisValue);
                }


            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (sortables == null)
            {
                #region (Plan B) Get data from WCF

                var applicationSearchServiceClient = new ApplicationSearchService.ApplicationSearchServiceClient();

                try
                {
                    applicationSearchServiceClient.Open();
                    sortables = applicationSearchServiceClient.GetProductSortables(accountNameKey, Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(applicationSearchServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationSearchServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = sortables;

            return jsonNetResult;

        }

        #endregion


        #region Sortables

        [Route("Search/Json/Featured")]
        [HttpGet]
        public JsonNetResult Featured()
        {
            var accountNameKey = Common.GetSubDomain(Request.Url);

            var featuredProperties = SettingsCommon.GetProperties(accountNameKey, "featured");
            var featuredPropertiesListings = new List<ApplicationPropertiesService.PropertyModel>();

            foreach (var property in featuredProperties)
            {
                if(property.Listing && property.PropertyTypeNameKey != "location" && property.PropertyTypeNameKey != "paragraph")
                {
                    //We ONLY extract listing types and non- locations:
                    featuredPropertiesListings.Add(property);
                }
                
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = featuredPropertiesListings;

            return jsonNetResult;

        }

        #endregion

        #endregion
    }
}