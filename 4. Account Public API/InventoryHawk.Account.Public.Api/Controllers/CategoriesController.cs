using InventoryHawk.Account.Public.Api.ApplicationCategorizationService;
using InventoryHawk.Account.Public.Api.Models.Json.Categorization;
using InventoryHawk.Account.Public.Api.Models.Json.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    
    public class CategoriesController : Controller
    {
        [Route("categories/tree")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JsonNetResult CategoryTree(bool includeHidden = false, bool includeImages = false)
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

            List<CategoryTreeModel> categoryTree = null;
            CategoryTreeJson categoryTreeJson = null;

            string localCacheKey = accountNameKey + ":categorytree:" + includeHidden + ":" + includeImages;

            #region (Plan A) Get json from local cache

            try
            {
                categoryTreeJson = (CategoryTreeJson)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (categoryTreeJson == null)
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
                        categoryTreeJson = JsonConvert.DeserializeObject<CategoryTreeJson>(redisApiValue);
                        executionType = ExecutionType.redis_secondary;
                    }
                }
                catch
                {

                }

                #endregion

                if (categoryTreeJson == null)
                {
                    #region (Plan C) Get category data from Redis Cache and rebuild

                    try
                    {
                        //IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                        string hashMainKey = accountNameKey + ":categories";
                        string hashMainField = "tree:public";

                        if (includeHidden == true)
                        {
                            hashMainField = "tree:private";
                        }

                        try
                        {
                            var redisValue = cache.HashGet(hashMainKey, hashMainField);

                            if (redisValue.HasValue)
                            {
                                categoryTree = JsonConvert.DeserializeObject<List<ApplicationCategorizationService.CategoryTreeModel>>(redisValue);
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
                        //TODO: Log: error message for Redis call
                    }

                    #endregion

                    if (categoryTree == null)
                    {
                        #region (Plan D) Get data from WCF

                        var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();

                        try
                        {
                            applicationCategorizationServiceClient.Open();

                            categoryTree = applicationCategorizationServiceClient.GetCategoryTree(accountNameKey, includeHidden, Common.SharedClientKey).ToList();

                            executionType = ExecutionType.wcf;

                            WCFManager.CloseConnection(applicationCategorizationServiceClient);

                        }
                        catch (Exception e)
                        {
                            #region Manage Exception

                            string exceptionMessage = e.Message.ToString();

                            var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                            string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                            // Abort the connection & manage the exception
                            WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                            #endregion
                        }

                        #endregion
                    }
                }

                #region Transform into json object, add images & cache locally or locally and radisAPI layer

                if (categoryTreeJson != null)
                {
                    //Just cache locally (we got json from the api redis layer)
                    HttpRuntime.Cache.Insert(localCacheKey, categoryTreeJson, null, DateTime.Now.AddMinutes(Common.CategorizationCacheTimeInMinutes), TimeSpan.Zero);
                }
                else if (categoryTree != null)
                {
                    //Transform categories into JSON and cache BOTH locally AND into redis
                    categoryTreeJson = Transforms.Json.CategorizationTransforms.CategoryTree(accountNameKey, categoryTree, includeImages);
                    HttpRuntime.Cache.Insert(localCacheKey, categoryTreeJson, null, DateTime.Now.AddMinutes(Common.CategorizationCacheTimeInMinutes), TimeSpan.Zero);
                    try
                    {
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(categoryTreeJson), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }
                   
                }

                #endregion
            }

            if(categoryTreeJson != null)
            {
                //Add execution data
                stopWatch.Stop();
                categoryTreeJson.executionType = "test";
                categoryTreeJson.executionType = executionType.ToString();
                categoryTreeJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = categoryTreeJson;

            return jsonNetResult;
            
        }

        [Route("categories")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public JsonNetResult Categories(bool includeHidden = false)
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

            List<CategoryModel> categories = null;
            CategoriesJson categoriesObjectJson = null;

            string localCacheKey = accountNameKey + ":categories:list:public";

            if(includeHidden == true)
            {
                localCacheKey = accountNameKey + ":categories:list:private";
            }

            #region (Plan A) Get json from local cache

            try
            {
                categoriesObjectJson = (CategoriesJson)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if(categoriesObjectJson == null)
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
                        categoriesObjectJson = JsonConvert.DeserializeObject<CategoriesJson>(redisApiValue);
                        executionType = ExecutionType.redis_secondary;
                    }
                }
                catch
                {

                }

                #endregion

                if (categoriesObjectJson == null)
                {
                    #region (Plan C) Get category data from Redis Cache and rebuild

                    try
                    {
                        //IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                        string hashMainKey = accountNameKey + ":categories";
                        string hashMainField = "list:public";

                        if (includeHidden == true)
                        {
                            hashMainField = "list:private";
                        }

                        try
                        {
                            var redisValue = cache.HashGet(hashMainKey, hashMainField);

                            if (redisValue.HasValue)
                            {                            
                                categories = JsonConvert.DeserializeObject<List<ApplicationCategorizationService.CategoryModel>>(redisValue);
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
                        //TODO: Log: error message for Redis call
                    }

                    #endregion

                    if (categories == null)
                    {
                        #region (Plan D) Get data from WCF

                        var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();

                        try
                        {
                            applicationCategorizationServiceClient.Open();

                            categories = applicationCategorizationServiceClient.GetCategories(accountNameKey, includeHidden, Common.SharedClientKey).ToList();

                            executionType = ExecutionType.wcf;

                            WCFManager.CloseConnection(applicationCategorizationServiceClient);

                        }
                        catch (Exception e)
                        {
                            #region Manage Exception

                            string exceptionMessage = e.Message.ToString();

                            var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                            string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                            // Abort the connection & manage the exception
                            WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                            #endregion
                        }

                        #endregion
                    }
                }

                #region Transform into json object, add images & cache locally or locally and radisAPI layer

                if (categoriesObjectJson != null)
                {
                    //Just cache locally (we got json from the api redis layer)
                    HttpRuntime.Cache.Insert(localCacheKey, categoriesObjectJson, null, DateTime.Now.AddMinutes(Common.CategorizationCacheTimeInMinutes), TimeSpan.Zero);
                }
                else if (categories != null)
                {
                    //Transform categories into JSON and cache BOTH locally AND into redis
                    categoriesObjectJson = Transforms.Json.CategorizationTransforms.Categories(accountNameKey, categories);
                    HttpRuntime.Cache.Insert(localCacheKey, categoriesObjectJson, null, DateTime.Now.AddMinutes(Common.CategorizationCacheTimeInMinutes), TimeSpan.Zero);

                    try
                    {
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(categoriesObjectJson), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }
                    
                }

                #endregion
            }

            //Add execution data
            stopWatch.Stop();
            categoriesObjectJson.executionType = executionType.ToString();
            categoriesObjectJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = categoriesObjectJson;

            return jsonNetResult;
        }
    }   
}
