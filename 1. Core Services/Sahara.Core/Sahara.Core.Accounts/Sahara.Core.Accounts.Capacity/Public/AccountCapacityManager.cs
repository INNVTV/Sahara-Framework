using Newtonsoft.Json;
using Sahara.Core.Accounts.Capacity.Internal;
using Sahara.Core.Accounts.Capacity.Models;
using Sahara.Core.Application.Categorization.Public;
using Sahara.Core.Application.Images.Formats;
using Sahara.Core.Application.Products.Public;
using Sahara.Core.Application.Properties;
using Sahara.Core.Application.Tags.Public;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Capacity.Public
{
    public static class AccountCapacityManager
    {
        public static AccountCapacity GetAccountCapacity(string accountId, bool useCachedVersion = true)
        {
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = AccountCapacityHash.Fields.AccountCapacity(accountId);
            
            AccountCapacity accountCapacityCache = null;
            AccountCapacity accountCapacity = null;

            try
            {
                #region Attempt to get from Cache

                if (useCachedVersion)
                {
                    try
                    {
                        var redisValue = cache.HashGet(AccountCapacityHash.Key(), redisHashField);
                        if (redisValue.HasValue)
                        {
                            accountCapacityCache = JsonConvert.DeserializeObject<AccountCapacity>(redisValue);
                        }
                    }
                    catch
                    {

                    }

                }

                #endregion
            }
            catch
            {

            }
    
            if (accountCapacityCache == null)
            {
                #region Collect Data and Store into Cache

                var account = AccountManager.GetAccount(accountId, true, Sahara.Core.Accounts.AccountManager.AccountIdentificationType.AccountID);

                accountCapacity = new AccountCapacity
                {
                    //Fill in Payment Plan Limits
                    UsersMax = account.PaymentPlan.MaxUsers,
                    ProductsMax = account.PaymentPlan.MaxProducts,
                    CategorizationsMax = account.PaymentPlan.MaxCategorizations,

                    //CategoriesMax = account.PaymentPlan.MaxCategories,
                    //SubcategoriesMax = account.PaymentPlan.MaxSubcategories,
                    //SubsubcategoriesMax = account.PaymentPlan.MaxSubsubcategories,
                    //SubsubsubcategoriesMax = account.PaymentPlan.MaxSubsubsubcategories,
                    PropertiesMax = account.PaymentPlan.MaxProperties,
                    TagsMax = account.PaymentPlan.MaxTags,

                    CustomImageGroupsMax = account.PaymentPlan.MaxImageGroups,
                    CustomImageFormatsMax = account.PaymentPlan.MaxImageFormats,
                    CustomImageGalleriesMax = account.PaymentPlan.MaxImageGalleries,

                    ImagesPerGalleryMax = account.PaymentPlan.MaxImagesPerGallery,

                    //ProductsPerSetMax = account.PaymentPlan.MaxProductsPerSet,
                    //SubcategoriesPerSetMax = account.PaymentPlan.MaxSubcategoriesPerSet
                };

                //Get Counts
                accountCapacity.UsersCount = account.Users.Count -1; //<--Offset by 1 to hide: platformadmin@[Config_PlatformEmail].com

                //TODO: Switch to Product / change name to "ProductManager"
                accountCapacity.ProductsCount = ProductManager.GetProductCount(account);
                accountCapacity.CategorizationsCount = CategorizationManager.GetCategorizationCount(account);
                accountCapacity.PropertiesCount = PropertiesManager.GetPropertyCount(account);
                accountCapacity.TagsCount = TagManager.GetTagCount(account.AccountNameKey);
                accountCapacity.CustomImageGroupsCount = ImageFormatsManager.GetCustomImageGroupCount(account.AccountNameKey);
                accountCapacity.CustomImageFormatsCount = ImageFormatsManager.GetCustomImageFormatCount(account.AccountNameKey);
                accountCapacity.CustomImageGalleriesCount = ImageFormatsManager.GetCustomImageGalleryCount(account.AccountNameKey);

                //Calculate Remaining Amounts Available
                accountCapacity.UsersRemaining = accountCapacity.UsersMax - accountCapacity.UsersCount;
                accountCapacity.ProductsRemaining = accountCapacity.ProductsMax - accountCapacity.ProductsCount;
                accountCapacity.CategorizationsRemaining = accountCapacity.CategorizationsMax - accountCapacity.CategorizationsCount;
                //accountCapacity.SubcategoriesRemaining = accountCapacity.SubcategoriesMax - accountCapacity.SubcategoriesCount;
                accountCapacity.PropertiesRemaining = accountCapacity.PropertiesMax - accountCapacity.PropertiesCount;
                accountCapacity.TagsRemaining = accountCapacity.TagsMax - accountCapacity.TagsCount;
                accountCapacity.CustomImageGroupsRemaining = accountCapacity.CustomImageGroupsMax - accountCapacity.CustomImageGroupsCount;
                accountCapacity.CustomImageFormatsRemaining = accountCapacity.CustomImageFormatsMax - accountCapacity.CustomImageFormatsCount;
                accountCapacity.CustomImageGalleriesRemaining = accountCapacity.CustomImageGalleriesMax - accountCapacity.CustomImageGalleriesCount;


                //Calculate Percentages Used ---------------------------------------------------------


                if(accountCapacity.UsersCount != 0 && accountCapacity.UsersMax != 0)
                {
                    accountCapacity.UsersPercentageUsed = (int)(((decimal)accountCapacity.UsersCount / (decimal)accountCapacity.UsersMax) * 100);
                }
                else
                {
                    accountCapacity.UsersPercentageUsed = 0;
                }


                if (accountCapacity.ProductsCount != 0 && accountCapacity.ProductsMax != 0)
                {
                    accountCapacity.ProductsPercentageUsed = (int)(((decimal)accountCapacity.ProductsCount / (decimal)accountCapacity.ProductsMax) * 100);
                }
                else
                {
                    accountCapacity.ProductsPercentageUsed = 0;
                }


                if (accountCapacity.CategorizationsCount != 0 && accountCapacity.CategorizationsMax != 0)
                {
                    accountCapacity.CategorizationsPercentageUsed = (int)(((decimal)accountCapacity.CategorizationsCount / (decimal)accountCapacity.CategorizationsMax) * 100);
                }
                else
                {
                    accountCapacity.CategorizationsPercentageUsed = 0;
                }


                if (accountCapacity.PropertiesCount != 0 && accountCapacity.PropertiesMax != 0)
                {
                    accountCapacity.PropertiesPercentageUsed = (int)(((decimal)accountCapacity.PropertiesCount / (decimal)accountCapacity.PropertiesMax) * 100);
                }
                else
                {
                    accountCapacity.PropertiesPercentageUsed = 0;
                }

                if (accountCapacity.TagsCount != 0 && accountCapacity.TagsMax != 0)
                {
                    accountCapacity.TagsPercentageUsed = (int)(((decimal)accountCapacity.TagsCount / (decimal)accountCapacity.TagsMax) * 100);
                }
                else
                {
                    accountCapacity.TagsPercentageUsed = 0;
                }



                if (accountCapacity.CustomImageGroupsCount != 0 && accountCapacity.CustomImageGroupsMax != 0)
                {
                    accountCapacity.CustomImageGroupsPercentageUsed = (int)(((decimal)accountCapacity.CustomImageGroupsCount / (decimal)accountCapacity.CustomImageGroupsMax) * 100);
                }
                else
                {
                    accountCapacity.CustomImageGroupsPercentageUsed = 0;
                }



                if (accountCapacity.CustomImageFormatsCount != 0 && accountCapacity.CustomImageFormatsMax != 0)
                {
                    accountCapacity.CustomImageFormatsPercentageUsed = (int)(((decimal)accountCapacity.CustomImageFormatsCount / (decimal)accountCapacity.CustomImageFormatsMax) * 100);
                }
                else
                {
                    accountCapacity.CustomImageFormatsPercentageUsed = 0;
                }

                if (accountCapacity.CustomImageGalleriesCount != 0 && accountCapacity.CustomImageGalleriesMax != 0)
                {
                    accountCapacity.CustomImageGalleriesPercentageUsed = (int)(((decimal)accountCapacity.CustomImageGalleriesCount / (decimal)accountCapacity.CustomImageGalleriesMax) * 100);
                }
                else
                {
                    accountCapacity.CustomImageGalleriesPercentageUsed = 0;
                }



                try
                {
                    //Store into "capacties" Cache
                    cache.HashSet(AccountCapacityHash.Key(), redisHashField, JsonConvert.SerializeObject(accountCapacity), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }
            else
            {
                accountCapacity = accountCapacityCache;
            }


            return accountCapacity;

        }

        public static void InvalidateAccountCapacitiesCache(string accountId)
        {
            AccountCapacityCaching.InvalidateAccountCapacitiesCache(accountId);
        }
    }
}
