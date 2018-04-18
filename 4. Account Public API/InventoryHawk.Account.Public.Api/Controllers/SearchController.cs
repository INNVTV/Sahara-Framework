using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http.Cors;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.Search;
using System.Web.Mvc;
using InventoryHawk.Account.Public.Api.ApplicationSearchService;
using StackExchange.Redis;
using InventoryHawk.Account.Public.Api.Models.Json.Search;
using InventoryHawk.Account.Public.Api.Models.Json.Common;
using System.Diagnostics;
using InventoryHawk.Account.Public.Api.Models.Search;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    public class SearchController : Controller
    {
        [Route("search/facets")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JsonNetResult ProductFacets(bool includeHidden = false)
        {
            ExecutionType executionType = ExecutionType.local;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Get the subdomain (if exists) for the api call
            string accountNameKey = Common.GetSubDomain(Request.Url);
            if (String.IsNullOrEmpty(accountNameKey))
            {
                return new JsonNetResult { Data = "Not found" }; //return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            AccountManagementService.Account account = Common.GetAccountObject(accountNameKey);

            string localCacheKey = accountNameKey + ":product:search:facets:includeHidden:" + includeHidden;


            List<ProductSearchFacet> facets = null;
            List<ProductSearchFacetJson> facetsJson = null;

            if (account == null)
            {
                return new JsonNetResult { Data = "Not found" }; //return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else
            {
                #region (Plan A) Get data from local cache

                try
                {
                    facetsJson = (List<ProductSearchFacetJson>)HttpRuntime.Cache[localCacheKey];
                }
                catch (Exception e)
                {
                    var error = e.Message;
                    //TODO: Log: error message for local cache call
                }

                #endregion

                if (facetsJson == null)
                {

                    #region (Plan B) Get Public json from second layer of Redis Cache

                    IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                    string pathAndQuery = Common.GetApiPathAndQuery(Request.Url);

                    string hashApiKey = accountNameKey + ":apicache";
                    string hashApiField = pathAndQuery;

                    try
                    {
                        var redisApiValue = cache.HashGet(hashApiKey, hashApiField);

                        if (redisApiValue.HasValue)
                        {
                            facetsJson = JsonConvert.DeserializeObject<List<ProductSearchFacetJson>>(redisApiValue);
                            executionType = ExecutionType.redis_secondary;
                        }
                    }
                    catch
                    {

                    }

                    #endregion

                    if(facetsJson == null)
                    {
                        #region (Plan C) Get data from Redis Cache

                        try
                        {
                            //Attempt to get facets from the Redis Cache

                            string hashKey = accountNameKey + ":search";
                            string hashField = "facets:products:visible";
                            
                            if(includeHidden)
                            {
                                hashField = "facets:products";
                            }

                            try
                            {
                                var redisValue = cache.HashGet(hashKey, hashField);

                                if (redisValue.HasValue)
                                {
                                    facets = JsonConvert.DeserializeObject<List<ProductSearchFacet>>(redisValue);
                                    executionType = ExecutionType.redis_main;
                                }
                            }
                            catch
                            {

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
                            #region (Plan D) Get data from WCF

                            var applicationSearchServiceClient = new ApplicationSearchService.ApplicationSearchServiceClient();

                            try
                            {
                                applicationSearchServiceClient.Open();
                                facets = applicationSearchServiceClient.GetProductFacets(accountNameKey, includeHidden, Common.SharedClientKey).ToList();

                                //Close the connection
                                WCFManager.CloseConnection(applicationSearchServiceClient);

                                executionType = ExecutionType.wcf;

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
                    }

                    #region  Transform into json object, & cache locally or locally and redisAPI layer

                    if (facetsJson != null)
                    {
                        //Just cache locally (we got json from the api redis layer)
                        HttpRuntime.Cache.Insert(localCacheKey, facetsJson, null, DateTime.Now.AddMinutes(Common.SearchFacetsCacheTimeInMinutes), TimeSpan.Zero);
                    }
                    else if (facets != null)
                    {
                        //Transform categories into JSON and cache BOTH locally AND into redis
                        facetsJson = Transforms.Json.SearchTransforms.ProductFacets(facets);
                        HttpRuntime.Cache.Insert(localCacheKey, facetsJson, null, DateTime.Now.AddMinutes(Common.SearchFacetsCacheTimeInMinutes), TimeSpan.Zero);

                        try
                        {
                            cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(facetsJson), When.Always, CommandFlags.FireAndForget);
                        }
                        catch
                        {

                        }
                        
                    }

                    #endregion
                }
            }



            //Create results object
            ProductSearchFacetsJson facetsJsonResult = new ProductSearchFacetsJson();
            facetsJsonResult.facets = facetsJson;

            //Add execution data
            stopWatch.Stop();
            facetsJsonResult.executionType = executionType.ToString();
            facetsJsonResult.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = facetsJsonResult;

            return jsonNetResult;

        }

        [Route("search/sortables")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JsonNetResult ProductSortables(bool includeHidden = false)
        {
            ExecutionType executionType = ExecutionType.local;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Get the subdomain (if exists) for the api call
            string accountNameKey = Common.GetSubDomain(Request.Url);
            if (String.IsNullOrEmpty(accountNameKey))
            {
                return new JsonNetResult { Data = "Not found" }; //return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            AccountManagementService.Account account = Common.GetAccountObject(accountNameKey);

            string localCacheKey = accountNameKey + ":product:search:sortables";
            if (includeHidden)
            {
                //localCacheKey = accountNameKey + ":product:search:sortables:private"; //<-- Not used, in parity with what the admin have (you can cherry pick with a custom site)
            }

            List<ProductSearchSortable> sortables = null;
            List<ProductSearchSortableJson> sortablesJson = null;

            if (account == null)
            {
                return new JsonNetResult { Data = "Not found" }; //return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else
            {
                #region (Plan A) Get data from local cache

                try
                {
                    sortablesJson = (List<ProductSearchSortableJson>)HttpRuntime.Cache[localCacheKey];
                }
                catch (Exception e)
                {
                    var error = e.Message;
                    //TODO: Log: error message for local cache call
                }

                #endregion

                if (sortablesJson == null)
                {

                    #region (Plan B) Get Public json from second layer of Redis Cache

                    IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                    string pathAndQuery = Common.GetApiPathAndQuery(Request.Url);

                    string hashApiKey = accountNameKey + ":apicache";
                    string hashApiField = pathAndQuery;

                    try
                    {
                        var redisApiValue = cache.HashGet(hashApiKey, hashApiField);

                        if (redisApiValue.HasValue)
                        {
                            sortablesJson = JsonConvert.DeserializeObject<List<ProductSearchSortableJson>>(redisApiValue);
                            executionType = ExecutionType.redis_secondary;
                        }
                    }
                    catch
                    {

                    }


                    #endregion

                    if (sortablesJson == null)
                    {
                        #region (Plan C) Get data from Redis Cache

                        try
                        {
                            //Attempt to get facets from the Redis Cache

                            string hashKey = accountNameKey + ":search";
                            string hashField = "sortables:products";

                            try
                            {
                                var redisValue = cache.HashGet(hashKey, hashField);

                                if (redisValue.HasValue)
                                {
                                    sortables = JsonConvert.DeserializeObject<List<ProductSearchSortable>>(redisValue);
                                    executionType = ExecutionType.redis_main;
                                }
                            }
                            catch
                            {

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
                            #region (Plan D) Get data from WCF

                            var applicationSearchServiceClient = new ApplicationSearchService.ApplicationSearchServiceClient();

                            try
                            {
                                applicationSearchServiceClient.Open();
                                sortables = applicationSearchServiceClient.GetProductSortables(accountNameKey, Common.SharedClientKey).ToList();

                                //Close the connection
                                WCFManager.CloseConnection(applicationSearchServiceClient);

                                executionType = ExecutionType.wcf;

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
                    }

                    #region  Transform into json object, & cache locally or locally and redisAPI layer

                    if (sortablesJson != null)
                    {
                        //Just cache locally (we got json from the api redis layer)
                        HttpRuntime.Cache.Insert(localCacheKey, sortablesJson, null, DateTime.Now.AddMinutes(Common.SearchFacetsCacheTimeInMinutes), TimeSpan.Zero);
                    }
                    else if (sortables != null)
                    {
                        //Transform categories into JSON and cache BOTH locally AND into redis
                        sortablesJson = Transforms.Json.SearchTransforms.ProductSortables(sortables);
                        HttpRuntime.Cache.Insert(localCacheKey, sortablesJson, null, DateTime.Now.AddMinutes(Common.SearchFacetsCacheTimeInMinutes), TimeSpan.Zero);
                        try
                        {
                            cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(sortablesJson), When.Always, CommandFlags.FireAndForget);
                        }
                        catch
                        {

                        }
                        
                    }

                    #endregion
                }
            }



            //Create results object
            ProductSearchSortablesJsonResult sortablesJsonResult = new ProductSearchSortablesJsonResult();
            sortablesJsonResult.sortables = sortablesJson;

            //Add execution data
            stopWatch.Stop();
            sortablesJsonResult.executionType = executionType.ToString();
            sortablesJsonResult.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = sortablesJsonResult;

            return jsonNetResult;

        }

        [Route("search/items")]
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JsonNetResult Products(string query = "", string filter = null, string orderBy = "relevance", int skip = 0, int take = 50, int top = 50, string locationSort = null, bool includeHidden = false)
        {
            //Alows you to use take and top interchangelbly
            if (top != take && top != 50)
            {
                take = top;
            }

            //We limit TAKEs on the api to 200 for performance purposes
            if (take > 120)
            {
                take = 120;
            }

            ExecutionType executionType = ExecutionType.local;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Get the subdomain (if exists) for the api call
            string accountNameKey = Common.GetSubDomain(Request.Url);
            if (String.IsNullOrEmpty(accountNameKey))
            {
                return new JsonNetResult { Data = "Not found" }; //return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            //CategoryModel category = null;
            SearchResultsJson searchResultsJson = null;
            SearchResults searchResults = null;

            string localCacheKey = accountNameKey + ":search:query:" + query + ":filter:" + filter + ":orderby:" + orderBy + ":skip:" + skip + ":take:" + take + ":locationSort:" + locationSort + ":hidden:" + includeHidden;


            #region (Plan A) Get json from local cache

            try
            {
                searchResultsJson = (SearchResultsJson)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (searchResultsJson == null)
            {
                #region (Plan B) Get Public json from second layer of Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string pathAndQuery = Common.GetApiPathAndQuery(Request.Url);

                string hashApiKey = accountNameKey + ":apicache";
                string hashApiField = pathAndQuery;

                try
                {
                    var redisApiValue = cache.HashGet(hashApiKey, hashApiField);

                    if (redisApiValue.HasValue)
                    {
                        searchResultsJson = JsonConvert.DeserializeObject<SearchResultsJson>(redisApiValue);
                        executionType = ExecutionType.redis_secondary;
                    }
                }
                catch
                {

                }

                #endregion

                if (searchResultsJson == null)
                {
                    #region (Plan C) Get Public json from search index

                    var account = Common.GetAccountObject(accountNameKey);

                    bool locationSortBool = false;
                    if(locationSort != null)
                    {
                        locationSortBool = true; //geo.distance(foundation, geography'POINT(-118.77975709999998 34.0259216)')
                    }

                    searchResults = DataAccess.Search.SearchProducts(account, query, filter, orderBy, skip, take, locationSortBool, locationSort, includeHidden);

                    #endregion

                    #region Transform into json object, add images

                    //Transform into JSON and cache onto Secondary Cache Layer:
                    searchResultsJson = Transforms.Json.SearchTransforms.ProductResults(searchResults);

                    #endregion

                    try
                    {
                        //Cache on Secondary Cache:
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(searchResultsJson), When.Always, CommandFlags.FireAndForget);                                   
                    }
                    catch
                    {

                    }

                    executionType = ExecutionType.searchIndex;
                }

                //Cache Locally
                HttpRuntime.Cache.Insert(localCacheKey, searchResultsJson, null, DateTime.Now.AddMinutes(Common.SearchResultsCacheTimeInMinutes), TimeSpan.Zero);

            }

            //Add execution data
            stopWatch.Stop();
            searchResultsJson.executionType = executionType.ToString();
            searchResultsJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = searchResultsJson;

            return jsonNetResult;

        }


    }
}