using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Search;
using Sahara.Core.Application.Tags.TableEntities;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Validation;
using Sahara.Core.Common.Validation.ResponseTypes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Tags.Public
{
    public static class TagManager
    {
        public static DataAccessResponseType CreateTag(Account account, string tagName)
        {
            #region Convert multiple spaces with single

            tagName = Regex.Replace(tagName, @"\s+", " ");

            #endregion

            #region Validate Tag Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidTagName(tagName);
            if (!ojectNameValidationResponse.isValid)
            {
                return new DataAccessResponseType {isSuccess = false, ErrorMessage = ojectNameValidationResponse.validationMessage};
            }

            #endregion

            #region validate tag does not exist

            var tags = GetTags(account.AccountNameKey);
            foreach(string tag in tags)
            {
                if(tag.ToLower() == tagName.ToLower())
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "A tag with that name already exists." };
                }
            }

            #endregion

            var tagEntity = new TagTableEntity(account.AccountID.ToString(), account.StoragePartition, tagName);

            var response = Internal.StoreTag(tagEntity, account.StoragePartition);

            if(response.isSuccess)
            {
                Caching.InvalidateTagsCache(account.AccountNameKey);
            }

            return response;
        }

        public static List<string> GetTags(string accountNameKey, bool useCachedVersion = true)
        {
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = ApplicationTagsHash.Fields.TagList();

            var tags = new List<string>();
            List<string> tagsCache = null;

            if (useCachedVersion)
            {
                try
                {
                    #region Attempt to get from Cache

                    var redisValue = cache.HashGet(ApplicationTagsHash.Key(accountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        tagsCache = JsonConvert.DeserializeObject<List<string>>(redisValue);
                    }

                    #endregion
                }
                catch
                {

                }
            }

            if (tagsCache == null)
            {
                #region Retrieve and store into cache

                var account = AccountManager.GetAccount(accountNameKey);
                var tagEntities = Internal.RetrieveTags(account.AccountID.ToString(), account.StoragePartition).ToList();

                foreach (TagTableEntity tagTableEntity in tagEntities)
                {
                    tags.Add(tagTableEntity.TagName);
                }

                tags.Sort();

                try
                {
                    //Store into "tags" Cache & set expiration
                    cache.HashSet(ApplicationTagsHash.Key(accountNameKey), redisHashField, JsonConvert.SerializeObject(tags), When.Always, CommandFlags.FireAndForget);
                    cache.KeyExpire(ApplicationTagsHash.Key(accountNameKey), ApplicationTagsHash.DefaultCacheLength, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }
            else
            {
                tags = tagsCache;
            }

            return tags;
        }

        public static int GetTagCount(string accountNameKey)
        {
            return GetTags(accountNameKey).Count();
        }

        #region Not Implemented

        public static int GetTagUsageCount(string accountId, string tagName)
        {
            //Get count of how many items are taged by this tagName. Used to add extra properties to tag view, showing how often it is used.

            return 0;
        }


        public static DataAccessResponseType DeleteTag(Account account, string tagName)
        {
            var response = new DataAccessResponseType();

            #region Check if tag is currently in use on ANY documents for this account

            #region Get any relevant documents (Legacy)
            /*
            //Get the DocumentDB Client
            var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            client.OpenAsync();

            string sqlQuery = "SELECT Top 1 * FROM Products p WHERE ARRAY_CONTAINS(p.Tags, '" + tagName + "')";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = client.CreateDocumentQuery<Document>(collectionUri.ToString(), sqlQuery).AsEnumerable().FirstOrDefault();
            */
            #endregion

            #region Get any relevant documents (from Search)

            //$filter=tags/any(t: t eq '345')
            string searchFilter = "tags/any(s: s eq '" + tagName + "')";

            var documentSearchResult = ProductSearchManager.SearchProducts(account.SearchPartition, account.ProductSearchIndex, "", searchFilter, "relevance", 0, 1);
            
            #endregion

            #endregion

            if (documentSearchResult.Count > 0)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a tag that is in use on a product. Please remove all tag associations before deleting."
                };
            }
            else
            {
                response = Internal.DeleteTag(account, tagName);
            }

            if (response.isSuccess)
            {
                Caching.InvalidateTagsCache(account.AccountNameKey);
            }

            return response;
        }

        #endregion
    }
}
