using InventoryHawk.Account.Public.Api.ApplicationPropertiesService;
using InventoryHawk.Account.Public.Api.Models.Json.Common;
using InventoryHawk.Account.Public.Api.Models.Json.Properties;
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
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    public class PropertiesController : Controller
    {
        [Route("properties/item")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult GetProductProperties(string type, string filter = null)
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

            List<PropertyModel> properties = null;
            PropertiesResultJson propertiesJson = null;

            string localCacheKey = accountNameKey + ":properties:all";
            PropertyListType propertyListType = PropertyListType.All;

            if (!String.IsNullOrEmpty(type))
            {
                if (type == "listings" || type == "listing" || type == "list")
                {
                    localCacheKey = accountNameKey + ":properties:listings";
                    propertyListType = PropertyListType.Listings;
                }
                else if (type == "details" || type == "detail")
                {
                    localCacheKey = accountNameKey + ":properties:details";
                    propertyListType = PropertyListType.Details;
                }
                else if (type == "featured" || type == "feature")
                {
                    if(filter == "listingsOnly")
                    {
                        localCacheKey = accountNameKey + ":properties:featured:listingsOnly";
                        propertyListType = PropertyListType.Featured;
                    }
                    else
                    {
                        localCacheKey = accountNameKey + ":properties:featured";
                        propertyListType = PropertyListType.Featured;
                    }
                }
            }

            #region (Plan A) Get json from local cache

            try
            {
                propertiesJson = (PropertiesResultJson)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (propertiesJson == null)
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
                        propertiesJson = JsonConvert.DeserializeObject<PropertiesResultJson>(redisApiValue);
                        executionType = ExecutionType.redis_secondary;
                    }
                }
                catch
                {

                }
                

                #endregion

                if (propertiesJson == null)
                {
                    #region (Plan C) Get property data from Redis Cache or WCF and Rebuild

                    properties = DataAccess.Properties.GetProperties(accountNameKey, propertyListType, out executionType);

                    #endregion
                }

                #region Transform into json object, add images & cache locally or locally and radisAPI layer

                if (propertiesJson != null)
                {
                    //Just cache locally (we got json from the api redis layer)
                    HttpRuntime.Cache.Insert(localCacheKey, propertiesJson, null, DateTime.Now.AddMinutes(Common.PropertiesCacheTimeInMinutes), TimeSpan.Zero);
                }
                else if (properties != null)
                {
                    //Transform properties into JSON and cache BOTH locally AND into redis
                    propertiesJson = Transforms.Json.PropertyTransforms.Properties(accountNameKey, properties, filter);
                    HttpRuntime.Cache.Insert(localCacheKey, propertiesJson, null, DateTime.Now.AddMinutes(Common.PropertiesCacheTimeInMinutes), TimeSpan.Zero);
                    try
                    {
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(propertiesJson), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }
                    
                }

                #endregion
            }

            //Add execution data
            stopWatch.Stop();
            propertiesJson.executionType = executionType.ToString();
            propertiesJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = propertiesJson;

            return jsonNetResult;

        }

        [Route("properties/item/{propertyNameKey}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult GetProductProperty(string propertyNameKey)
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

            PropertyModel property = null;
            PropertyResultJson propertyJson = null;

            string localCacheKey = accountNameKey + ":property:" + propertyNameKey;


            #region (Plan A) Get json from local cache

            try
            {
                propertyJson = (PropertyResultJson)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (propertyJson == null)
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
                        propertyJson = JsonConvert.DeserializeObject<PropertyResultJson>(redisApiValue);
                        executionType = ExecutionType.redis_secondary;
                    }
                }
                catch
                {

                }


                #endregion

                if (propertyJson == null)
                {
                    #region (Plan C) Get property data from Redis Cache or WCF and EXTRACT

                    var properties = DataAccess.Properties.GetProperties(accountNameKey, PropertyListType.All, out executionType);

                    property = properties.Find(x => x.PropertyNameKey.ToLower().Replace(" ", "") == propertyNameKey.ToLower().Replace(" ", ""));

                    if(property == null)
                    {
                        JsonNetResult jsonNetResultEmpty = new JsonNetResult();
                        jsonNetResultEmpty.Formatting = Newtonsoft.Json.Formatting.Indented;
                        jsonNetResultEmpty.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                        jsonNetResultEmpty.Data = null;
                        return jsonNetResultEmpty;
                    }

                    #endregion
                }

                #region Transform into json object, add images & cache locally or locally and radisAPI layer

                if (propertyJson != null)
                {
                    //Just cache locally (we got json from the api redis layer)
                    HttpRuntime.Cache.Insert(localCacheKey, propertyJson, null, DateTime.Now.AddMinutes(Common.PropertiesCacheTimeInMinutes), TimeSpan.Zero);
                }
                else if (property != null)
                {
                    //Transform properties into JSON and cache BOTH locally AND into redis
                    propertyJson = Transforms.Json.PropertyTransforms.Property(accountNameKey, property);
                    HttpRuntime.Cache.Insert(localCacheKey, propertyJson, null, DateTime.Now.AddMinutes(Common.PropertiesCacheTimeInMinutes), TimeSpan.Zero);
                    try
                    {
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(propertyJson), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }

                }

                #endregion
            }

            //Add execution data
            stopWatch.Stop();
            propertyJson.executionType = executionType.ToString();
            propertyJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = propertyJson;

            return jsonNetResult;

        }
    }
}