using InventoryHawk.Account.Public.Api.Models.Json.Common;
using InventoryHawk.Account.Public.Api.Models.Json.Tags;
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
    public class TagsController : Controller
    {
        [Route("tags")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult Tags()
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

            List<string> tags = null;
            TagsListJson tagsListObjectJson = null;

            string localCacheKey = accountNameKey + ":tags";


            #region (Plan A) Get json from local cache

            try
            {
                tagsListObjectJson = (TagsListJson)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (tagsListObjectJson == null)
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
                        tagsListObjectJson = JsonConvert.DeserializeObject<TagsListJson>(redisApiValue);
                        executionType = ExecutionType.redis_secondary;
                    }
                }
                catch
                {

                }


                #endregion

                if (tagsListObjectJson == null)
                {
                    #region (Plan C) Get data from Redis Cache and rebuild

                    try
                    {
                        //IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                        string hashMainKey = accountNameKey + ":tags";
                        string hashMainField = "list";

                        try
                        {
                            var redisValue = cache.HashGet(hashMainKey, hashMainField);

                            if (redisValue.HasValue)
                            {
                                tags = JsonConvert.DeserializeObject<List<String>>(redisValue);
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

                    if (tags == null)
                    {
                        #region (Plan D) Get data from WCF

                        var applicationTagsServiceClient = new ApplicationTagsService.ApplicationTagsServiceClient();

                        try
                        {
                            applicationTagsServiceClient.Open();

                            tags = applicationTagsServiceClient.GetTags(accountNameKey, Common.SharedClientKey).ToList();

                            executionType = ExecutionType.wcf;

                            WCFManager.CloseConnection(applicationTagsServiceClient);

                        }
                        catch (Exception e)
                        {
                            #region Manage Exception

                            string exceptionMessage = e.Message.ToString();

                            var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                            string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                            // Abort the connection & manage the exception
                            WCFManager.CloseConnection(applicationTagsServiceClient, exceptionMessage, currentMethodString);

                            #endregion
                        }

                        #endregion
                    }
                }

                #region Transform into json object, add images & cache locally or locally and radisAPI layer

                if (tagsListObjectJson != null)
                {
                    //Just cache locally (we got json from the api redis layer)
                    HttpRuntime.Cache.Insert(localCacheKey, tagsListObjectJson, null, DateTime.Now.AddMinutes(Common.CategorizationCacheTimeInMinutes), TimeSpan.Zero);
                }
                else if (tags != null)
                {
                    //Transform into JSON and cache BOTH locally AND into redis
                    tagsListObjectJson = new TagsListJson();
                    tagsListObjectJson.tags = tags;
                    tagsListObjectJson.count = tags.Count;

                    HttpRuntime.Cache.Insert(localCacheKey, tagsListObjectJson, null, DateTime.Now.AddMinutes(Common.TagsListCacheTimeInMinutes), TimeSpan.Zero);

                    try
                    {
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(tagsListObjectJson), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }
                    
                }

                #endregion
            }

            //Add execution data
            stopWatch.Stop();
            tagsListObjectJson.executionType = executionType.ToString();
            tagsListObjectJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = tagsListObjectJson;

            return jsonNetResult;
        }
    }
}
