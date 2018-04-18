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
    public class SubsubcategoryController : Controller
    {
        [Route("Subsubcategory/{categoryNameKey}/{subcategoryNameKey}/{subsubcategoryNameKey}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult Subsubcategory(string categoryNameKey, string subcategoryNameKey, string subsubcategoryNameKey, bool includeHidden = false, bool includeItems = false)
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

            SubsubcategoryModel subsubcategory = null;
            SubsubcategoryDetailsJson subsubcategoryDetailsObjectJson = null;

            string localCacheKey = accountNameKey + ":subsubcategory:" + categoryNameKey + ":" + subcategoryNameKey + ":" + subsubcategoryNameKey + ":includeHidden:" + includeHidden + "includeProducts:" + includeItems;


            #region (Plan A) Get json from local cache

            try
            {
                subsubcategoryDetailsObjectJson = (SubsubcategoryDetailsJson)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (subsubcategoryDetailsObjectJson == null)
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
                        subsubcategoryDetailsObjectJson = JsonConvert.DeserializeObject<SubsubcategoryDetailsJson>(redisApiValue);
                        executionType = ExecutionType.redis_secondary;
                    }
                }
                catch
                {

                }


                #endregion

                if (subsubcategoryDetailsObjectJson == null)
                {
                    #region (Plan C) Get category data from Redis Cache and rebuild

                    try
                    {
                        //IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                        string hashMainKey = accountNameKey + ":categories";
                        string hashMainField = categoryNameKey + "/" + subcategoryNameKey + "/" + subsubcategoryNameKey + ":public";

                        if (includeHidden == true)
                        {
                            hashMainField = categoryNameKey + "/" + subcategoryNameKey + "/" + subsubcategoryNameKey + ":private";
                        }

                        try
                        {
                            var redisValue = cache.HashGet(hashMainKey, hashMainField);

                            if (redisValue.HasValue)
                            {
                                subsubcategory = JsonConvert.DeserializeObject<ApplicationCategorizationService.SubsubcategoryModel>(redisValue);
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

                    if (subsubcategory == null)
                    {
                        #region (Plan D) Get data from WCF

                        var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();

                        try
                        {
                            applicationCategorizationServiceClient.Open();

                            subsubcategory = applicationCategorizationServiceClient.GetSubsubcategoryByNames(accountNameKey, categoryNameKey, subcategoryNameKey, subsubcategoryNameKey, includeHidden, Common.SharedClientKey);

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

                if (subsubcategoryDetailsObjectJson != null)
                {
                    //Just cache locally (we got json from the api redis layer)
                    HttpRuntime.Cache.Insert(localCacheKey, subsubcategoryDetailsObjectJson, null, DateTime.Now.AddMinutes(Common.CategorizationCacheTimeInMinutes), TimeSpan.Zero);
                }
                else if (subsubcategory != null)
                {
                    //Transform categories into JSON and cache BOTH locally AND into redis
                    subsubcategoryDetailsObjectJson = Transforms.Json.CategorizationTransforms.Subsubcategory(accountNameKey, subsubcategory, includeItems, includeHidden);
                    HttpRuntime.Cache.Insert(localCacheKey, subsubcategoryDetailsObjectJson, null, DateTime.Now.AddMinutes(Common.CategorizationCacheTimeInMinutes), TimeSpan.Zero);
                    try
                    {
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(subsubcategoryDetailsObjectJson), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }
                    
                }

                #endregion
            }

            if (subsubcategoryDetailsObjectJson == null)
            {
                //return empty
                return new JsonNetResult();
            }

            //Add execution data
            stopWatch.Stop();
            subsubcategoryDetailsObjectJson.executionType = executionType.ToString();
            subsubcategoryDetailsObjectJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = subsubcategoryDetailsObjectJson;

            return jsonNetResult;
        }
    }
}
