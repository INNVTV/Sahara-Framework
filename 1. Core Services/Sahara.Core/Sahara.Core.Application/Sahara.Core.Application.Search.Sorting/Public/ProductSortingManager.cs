using Newtonsoft.Json;
using Sahara.Core.Application.Search.Models.Product;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Search.Sorting.Public
{
    public static class ProductSortingManager
    {
        public static List<ProductSearchSortable> GetProductSortables(string accountNameKey, bool useCachedVersion = true)
        {
            List<ProductSearchSortable> productSortables = null;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = string.Empty;

            redisHashField = ApplicationSearchHash.Fields.ProductSortables();


            if (useCachedVersion)
            {
                #region Get sortables from cache

                try
                {
                    var redisValue = cache.HashGet(ApplicationSearchHash.Key(accountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        productSortables = JsonConvert.DeserializeObject<List<ProductSearchSortable>>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (productSortables == null)
            {
                #region Build Sortables

                productSortables = Internal.ProductSortingTasks.BuildProductSortables(accountNameKey);

                #endregion

                #region Store sortables into cache

                try
                {
                    cache.HashSet(ApplicationSearchHash.Key(accountNameKey), redisHashField, JsonConvert.SerializeObject(productSortables), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }
                
                #endregion
            }

            return productSortables;
        }
    }
}
