using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Sahara.Core.Accounts;
using Sahara.Core.Application.Properties;
using Sahara.Core.Application.Properties.Models;
using Sahara.Core.Application.Search.Models.Product;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Search.Facets.Public
{
    public class ProductFacetManager
    {
        #region Facets

        public static List<ProductSearchFacet> GetProductFacets(string accountNameKey, bool includeHidden = true, bool useCachedVersion = true)
        {
            List<ProductSearchFacet> productFacets = null;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = string.Empty;

            if(includeHidden)
            {
                redisHashField = ApplicationSearchHash.Fields.ProductFacets();
            }
            else
            {
                redisHashField = ApplicationSearchHash.Fields.ProductFacetsVisible();
            }

            if (useCachedVersion)
            {
                #region Get facets from cache

                try
                {
                    var redisValue = cache.HashGet(ApplicationSearchHash.Key(accountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        productFacets = JsonConvert.DeserializeObject<List<ProductSearchFacet>>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (productFacets == null)
            {
                #region Build Facets From Search Index 

                productFacets = Internal.ProductFacetTasks.BuildProductFacets(accountNameKey, includeHidden);

                #endregion

                #region Store count into cache

                try
                {
                    cache.HashSet(ApplicationSearchHash.Key(accountNameKey), redisHashField, JsonConvert.SerializeObject(productFacets), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }

            return productFacets;
        }

        #endregion

    }
}
