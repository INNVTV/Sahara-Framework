using Newtonsoft.Json;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.ApiKeys.Models;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Common.ResponseTypes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.ApiKeys
{
    public static class ApiKeysManager
    {
        public static List<ApiKeyModel> GetApiKeys(Account account, bool useCachedVersion = true)
        {
            List<ApiKeyModel> keys = null;

            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = string.Empty;

            redisHashField = AccountApiKeysHash.Fields.List();

            if (useCachedVersion)
            {
                #region Get keys from cache

                try
                {
                    var redisValue = cache.HashGet(AccountApiKeysHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        keys = JsonConvert.DeserializeObject<List<ApiKeyModel>>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (keys == null)
            {
                #region Get keys from SQL

                keys = Sql.Statements.SelectStatements.SelectApiKeysList(account.SqlPartition, account.SchemaName);

                #endregion

                #region Store keys into cache

                try
                {
                    cache.HashSet(AccountApiKeysHash.Key(account.AccountNameKey), redisHashField, JsonConvert.SerializeObject(keys), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }

            return keys;
        }

        public static string GenerateApiKey(Account account, string name, string description)
        {


            var key = new ApiKeyModel { Name = name, Description = description };
            key.ApiKey = Guid.NewGuid();

            #region validate character lengths

            if (key.Name.Length > 80)
            {
                return null;
            }
            if (key.Description.Length > 360)
            {
                return null;
            }

            #endregion

            #region Validate unique name/label & key count

            var keys = GetApiKeys(account);

            if(keys.Count >= 25) //<-- Limit of 25 API Keys
            {
                return null;
            }

            foreach(var existingKey in keys)
            {
                if(existingKey.Name == name)
                {
                    return null;
                }
            }



            #endregion


            var response = Sql.Statements.InsertStatements.InsertApiKey(account.SqlPartition, account.SchemaName, key);

            if(response.isSuccess)
            {

                #region Clear Redis Cache

                Internal.Caching.InvalidateApiKeysCache(account.AccountNameKey);

                #endregion

                #region Invalidate Account API Caching Layer

                Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

                #endregion

                return key.ApiKey.ToString();
            }
            else
            {
                return null;
            }
            
        }

        public static string RegenerateApiKey(Account account, string apiKey)
        {
            var newKey = Guid.NewGuid();

            var isSuccess = Sql.Statements.UpdateStatements.UpdateApiKey(account.SqlPartition, account.SchemaName, apiKey, newKey.ToString());

            if(isSuccess)
            {
                #region Clear Redis Cache

                Internal.Caching.InvalidateApiKeysCache(account.AccountNameKey);

                #endregion

                #region Invalidate Account API Caching Layer

                Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

                #endregion

                return newKey.ToString();
            }
            else
            {
                return null;
            }
            
        }

        public static DataAccessResponseType DeleteApiKey(Account account, string apiKey)
        {
            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.DeleteStatements.DeleteApiKey(account.SqlPartition, account.SchemaName, apiKey);

            if(response.isSuccess)
            {
                #region Clear Redis Cache

                Internal.Caching.InvalidateApiKeysCache(account.AccountNameKey);

                #endregion

                #region Invalidate Account API Caching Layer

                Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

                #endregion
            }

            return response;
        }
    }
}
