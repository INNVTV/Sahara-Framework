using InventoryHawk.Account.Public.Api.ApplicationPropertiesService;
using InventoryHawk.Account.Public.Api.Models.Json.Common;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.DataAccess
{
    public static class Properties
    {

        public static List<PropertyModel> GetProperties(string accountNameKey, PropertyListType propertyListType, out ExecutionType executionTypeResult)
        {
            List<PropertyModel> properties = null;
            executionTypeResult = ExecutionType.redis_main;


            #region (Plan A) Get data from Redis Cache

            try
                {
                    IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                    string hashMainKey = accountNameKey + ":properties";
                    string hashMainField = propertyListType.ToString().ToLower();

                    try
                    {
                        var redisValue = cache.HashGet(hashMainKey, hashMainField);

                        if (redisValue.HasValue)
                        {
                            properties = JsonConvert.DeserializeObject<List<PropertyModel>>(redisValue);
                            executionTypeResult = ExecutionType.redis_main;
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

            if (properties == null) 
            {
                #region (Plan D) Get data from WCF

                    var applicationPropertiesServiceClient = new ApplicationPropertiesService.ApplicationPropertiesServiceClient();

                    try
                    {
                        applicationPropertiesServiceClient.Open();

                        properties = applicationPropertiesServiceClient.GetProperties(accountNameKey, propertyListType, Common.SharedClientKey).ToList();

                        executionTypeResult = ExecutionType.wcf;

                        WCFManager.CloseConnection(applicationPropertiesServiceClient);

                    }
                    catch (Exception e)
                    {
                        #region Manage Exception

                        string exceptionMessage = e.Message.ToString();

                        var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                        string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                        // Abort the connection & manage the exception
                        WCFManager.CloseConnection(applicationPropertiesServiceClient, exceptionMessage, currentMethodString);

                        #endregion
                    }

                    #endregion
            }

            return properties;
        }

        public static string GetPropertyType(string accountNameKey, string propertyName)
        {

            string propertyType = null;
            string localCacheKey = accountNameKey + ":propertyType:" + propertyName;

            #region (Plan A) Get local cache version

            try
            {
                propertyType = (string)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if(String.IsNullOrEmpty(propertyType))
            {
                #region (Plan B) Loop through properties objects from Redis or WCF

                ExecutionType executionType;
                var properties = GetProperties(accountNameKey, PropertyListType.All, out executionType);

                foreach(PropertyModel property in properties)
                {
                    if(property.PropertyName == propertyName || property.PropertyNameKey == propertyName)
                    {
                        propertyType = property.PropertyTypeNameKey;
                    }
                }

                #endregion

                
                if(!String.IsNullOrEmpty(propertyType))
                {
                    //Cache locally
                    HttpRuntime.Cache.Insert(localCacheKey, propertyType, null, DateTime.Now.AddMinutes(Common.PropertyTypeCacheTimeInMinutes), TimeSpan.Zero);
                }
                
            }

            return propertyType;
        }

    }
}