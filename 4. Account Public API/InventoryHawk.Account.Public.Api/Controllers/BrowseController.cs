using InventoryHawk.Account.Public.Api.Models.Json.Common;
using InventoryHawk.Account.Public.Api.Models.Json.Search;
using InventoryHawk.Account.Public.Api.Models.Search;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    public class BrowseController : Controller
    {
        // GET: Browse
        [Route("browse/{searchFieldName}/{searchFieldValue}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult BrowseByPropertyNameValue(string searchFieldName, string searchFieldValue, string orderBy = "dateCreated desc", int skip=0, int take=50, int top = 50, bool includeHidden = false)
        {
            //Alows you to use take and top interchangelbly
            if(top != take && top != 50)
            {
                take = top;                
            }


            //We limit TAKEs on the api to 200 for performance purposes
            if(take > 120)
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

            string localCacheKey = accountNameKey + ":browse:propertyname:" + searchFieldName + ":propertyvalue:" + searchFieldValue + ":skip:" + skip + ":take:" + take + ":public";

            if (includeHidden == true)
            {
                localCacheKey = accountNameKey + ":browse:propertyname:" + searchFieldName + ":propertyvalue:" + searchFieldValue + ":skip:" + skip + ":take:" + take + ":private";
            }

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

                    //ToDo:
                    //Determine propertyType and build filter accordingly. Reject Paragraphs, Etc...

                    string filter = String.Empty;

                    if (searchFieldName == "categoryName" || searchFieldName == "categoryNameKey"
                        || searchFieldName == "subcategoryName" || searchFieldName == "subcategoryNameKey"
                        || searchFieldName == "subsubcategoryName" || searchFieldName == "subsubcategoryNameKey"
                        || searchFieldName == "subsubsubcategoryName" || searchFieldName == "subsubsubcategoryNameKey"
                        || searchFieldName == "locationPath"
                        )
                    {
                        //If this is from one of our standard properties we use a simple EQ filter:
                        filter = "(" + searchFieldName + " eq '" + searchFieldValue + "')";
                    }
                    else if (searchFieldName == "tags" || searchFieldName == "tag")
                    {
                        //If this is from tags we use a collections EQ filter:
                        filter = "(tags/any(s: s eq '" + searchFieldValue + "'))";
                    }
                    else
                    {
                        #region Check if this is a CUSTOM type: and build filter accordingly. Reject Paragraphs, Etc..

                        var propertyType = DataAccess.Properties.GetPropertyType(accountNameKey, searchFieldName);

                        if (!String.IsNullOrEmpty(propertyType))
                        {
                            if (propertyType == "string" || propertyType == "number")
                            {
                                filter = "(" + searchFieldName + " eq '" + searchFieldValue + "')";
                            }
                            else if (propertyType == "predefined" || propertyType == "swatch")
                            {
                                filter = "(" + searchFieldName + "/any(s: s eq '" + searchFieldValue + "'))";
                            }

                            //(fabric any(s: s eq Red Wool ))
                        }
                        else
                        {
                            return null;
                        }

                        #endregion
                    }

                    searchResults = DataAccess.Search.SearchProducts(account, null, filter, orderBy, skip, take, false, null, includeHidden);

                    #endregion

                    #region Transform into json object, add images

                    //Transform into JSON and cache onto Secondary Cache Layer:
                    searchResultsJson = Transforms.Json.SearchTransforms.ProductResults(searchResults);

                    #endregion

                    try
                    {
                        //Cache on Secondary Cache:
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(searchResultsJson), When.Always, CommandFlags.FireAndForget);
                        executionType = ExecutionType.searchIndex;
                    }
                    catch
                    {

                    }

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