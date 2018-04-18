using InventoryHawk.Account.Public.Api.Models.Json.Categorization;
using InventoryHawk.Account.Public.Api.Models.Json.Common;
using InventoryHawk.Account.Public.Api.Models.Json.Products;
using InventoryHawk.Account.Public.Api.Models.Search;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace InventoryHawk.Account.Public.Api.Controllers
{

    public class ProductController : Controller
    {

        #region GET

        [Route("item/{id}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult GetProduct(string id)
        {
            return GetProductDetail("id", id);
        }

        [Route("item/{categoryNameKey}/{ProductNameKey}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult GetProduct(string categoryNameKey, string productNameKey)
        {
            return GetProductDetail("fullyQualifiedName", categoryNameKey + "/" + productNameKey);
        }

        [Route("item/{categoryNameKey}/{subcategoryNameKey}/{ProductNameKey}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult GetProduct(string categoryNameKey, string subcategoryNameKey, string productNameKey)
        {
            return GetProductDetail("fullyQualifiedName", categoryNameKey + "/" + subcategoryNameKey + "/" + productNameKey);
        }

        [Route("item/{categoryNameKey}/{subcategoryNameKey}/{subsubcategoryNameKey}/{ProductNameKey}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult GetProduct(string categoryNameKey, string subcategoryNameKey, string subsubcategoryNameKey, string productNameKey)
        {
            return GetProductDetail("fullyQualifiedName", categoryNameKey + "/" + subcategoryNameKey + "/" + subsubcategoryNameKey + "/" + productNameKey);
        }

        [Route("item/{categoryNameKey}/{subcategoryNameKey}/{subsubcategoryNameKey}/{subsubsubcategoryNameKey}/{ProductNameKey}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult GetProduct(string categoryNameKey, string subcategoryNameKey, string subsubcategoryNameKey, string subsubsubcategoryNameKey, string productNameKey)
        {
            return GetProductDetail("fullyQualifiedName", categoryNameKey + "/" + subcategoryNameKey + "/" + subsubcategoryNameKey + "/" + subsubsubcategoryNameKey + "/" + productNameKey);
        }

        #endregion


        #region POST/PUT/DELETE/MOVE (w/ APIKey)

        #region  Fiddler Settings

        /*
        [POST]

        https://[accountNameKey].[domain].com/item


        Content-Type: application/x-www-form-urlencoded

        RequestBody:
        apiKey=xxxxx&locatioPath=xxxx&productName=xxxx&isisible=true
        */

        #endregion

        [Route("item")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpPut]
        public HttpStatusCode CreateProduct(string apiKey, string locationPath, string itemName, bool isVisible)
        {
            //Get the subdomain (if exists) for the api call
            string accountNameKey = Common.GetSubDomain(Request.Url);
            if (String.IsNullOrEmpty(accountNameKey))
            {
                return HttpStatusCode.BadRequest;
            }

            #region Validate API Key

            var keyValid = Common.ValidateApiKey(accountNameKey, apiKey);

            if(!keyValid)
            {
                return HttpStatusCode.Forbidden;
            }
            

            #endregion

            return HttpStatusCode.OK;
        }

        [Route("item/property/update/")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpPost]
        public HttpStatusCode UpdateProductProperty(string apiKey, string productId, string propertyNameKey, string propertyValue)
        {
            return HttpStatusCode.OK;
        }

        [Route("item/tag/add")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpPost]
        public HttpStatusCode AddProductTag(string apiKey, string productId, string tagName)
        {
            return HttpStatusCode.OK;
        }

        [Route("item/tag/remove")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpPost]
        public HttpStatusCode RemoveProductTag(string apiKey, string productId, string tagName)
        {
            return HttpStatusCode.OK;
        }

        #endregion


        #region shared

        internal JsonNetResult GetProductDetail(string idProperty, string idValue)
        {
            ProductDetailJson productDetailsJson = null;

            ExecutionType executionType = ExecutionType.local;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Get the subdomain (if exists) for the api call
            string accountNameKey = Common.GetSubDomain(Request.Url);
            if (String.IsNullOrEmpty(accountNameKey))
            {
                return new JsonNetResult { Data = "Not found" }; //return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            string localCacheKey = accountNameKey + ":product:" + idProperty + ":" + idValue;


            #region (Plan A) Get json from local cache

            try
            {
                productDetailsJson = (ProductDetailJson)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if(productDetailsJson == null)
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
                        productDetailsJson = JsonConvert.DeserializeObject<ProductDetailJson>(redisApiValue);
                        executionType = ExecutionType.redis_secondary;
                    }
                }
                catch
                {

                }


                #endregion

                if (productDetailsJson == null)
                {
                    #region (Plan C) Get document from Search (No longer: DocumentDB)

                    productDetailsJson = new ProductDetailJson();

                    productDetailsJson.item = DataAccess.Product.GetProductDetail(accountNameKey, idProperty, idValue);

                    #region include parent objects

                    /*
                    object categoryObject = null;
                    object subcategoryObject = null;
                    object subsubcategoryObject = null;
                    object subsubsubcategoryObject = null;

                    productDetailsJson.product.TryGetValue("categoryNameKey", out categoryObject);
                    productDetailsJson.product.TryGetValue("subcategoryNameKey", out subcategoryObject);
                    productDetailsJson.product.TryGetValue("subsubcategoryNameKey", out subsubcategoryObject);
                    productDetailsJson.product.TryGetValue("subsubsubcategoryNameKey", out subsubsubcategoryObject);
                    */

                    if (productDetailsJson.item["categoryNameKey"] != null)
                    {
                        productDetailsJson.category = new CategorizationParentItemJson
                        {
                            name = productDetailsJson.item["categoryName"].ToString(),
                            nameKey = productDetailsJson.item["categoryNameKey"].ToString(),
                            fullyQualifiedName = productDetailsJson.item["categoryNameKey"].ToString()
                        };
                    }

                    if (productDetailsJson.item["subcategoryNameKey"] != null)
                    {
                        productDetailsJson.subcategory = new CategorizationParentItemJson
                        {
                            name = productDetailsJson.item["subcategoryName"].ToString(),
                            nameKey = productDetailsJson.item["subcategoryNameKey"].ToString(),
                            fullyQualifiedName = productDetailsJson.item["categoryNameKey"].ToString() + "/" + productDetailsJson.item["subcategoryNameKey"].ToString()
                        };
                    }


                    if (productDetailsJson.item["subsubcategoryNameKey"] != null)
                    {
                        productDetailsJson.subsubcategory = new CategorizationParentItemJson
                        {
                            name = productDetailsJson.item["subsubcategoryName"].ToString(),
                            nameKey = productDetailsJson.item["subsubcategoryNameKey"].ToString(),
                            fullyQualifiedName = productDetailsJson.item["categoryNameKey"].ToString() + "/" + productDetailsJson.item["subcategoryNameKey"].ToString() + "/" + productDetailsJson.item["subsubcategoryNameKey"].ToString()
                        };
                    }

                    if (productDetailsJson.item["subsubsubcategoryNameKey"] != null)
                    {
                        productDetailsJson.subsubsubcategory = new CategorizationParentItemJson
                        {
                            name = productDetailsJson.item["subsubsubcategoryName"].ToString(),
                            nameKey = productDetailsJson.item["subsubsubcategoryNameKey"].ToString(),
                            fullyQualifiedName = productDetailsJson.item["categoryNameKey"].ToString() + "/" + productDetailsJson.item["subcategoryNameKey"].ToString() + "/" + productDetailsJson.item["subsubcategoryNameKey"].ToString() + "/" + productDetailsJson.item["subsubsubcategoryNameKey"].ToString()
                        };
                    }

                    #endregion

                    executionType = ExecutionType.searchIndex;

                    #endregion

                    #region Cache into redisAPI layer

                    try
                    {
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(productDetailsJson), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }
                    
                    #endregion
                }

                #region Cache locally

                HttpRuntime.Cache.Insert(localCacheKey, productDetailsJson, null, DateTime.Now.AddMinutes(Common.ProductCacheTimeInMinutes), TimeSpan.Zero);

                #endregion

            }

            //Add execution data
            stopWatch.Stop();
            productDetailsJson.executionType = executionType.ToString();
            productDetailsJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = productDetailsJson;

            return jsonNetResult;

            ///Switch to using DocumentDB ----
            ///

            /*

            //CategoryModel category = null;
            SearchResults searchResultsObjectJson = null;

            string localCacheKey = accountNameKey + ":product:" + propertyValue;

            #region (Plan A) Get json from local cache

            try
            {
                searchResultsObjectJson = (SearchResults)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (searchResultsObjectJson == null)
            {
                #region (Plan B) Get Public json from search index

                var account = Common.GetAccountObject(accountNameKey);

                string filter = "(" + propertyName + " eq '" + propertyValue + "')";

                var searchResults = DataAccess.Products.Search(account, null, filter, "relevance", 0, 50, false, null, true);

                #endregion

                #region Transform into json object, add images & cache locally


                //Transform categories into JSON and cache locally
                searchResultsObjectJson = searchResults;
                //searchResultsObjectJson = Transforms.Json.CategorizationTransforms.Category(accountNameKey, category);
                HttpRuntime.Cache.Insert(localCacheKey, searchResultsObjectJson, null, DateTime.Now.AddMinutes(Common.SearchResultsCacheTimeInMinutes), TimeSpan.Zero);


                #endregion

                executionType = ExecutionType.searchIndex;
            }

            //Add execution data
            stopWatch.Stop();
            //searchResultsObjectJson.executionType = executionType.ToString();
            //searchResultsObjectJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = searchResultsObjectJson;

            return jsonNetResult;*/
        }

        #endregion
    }






}
