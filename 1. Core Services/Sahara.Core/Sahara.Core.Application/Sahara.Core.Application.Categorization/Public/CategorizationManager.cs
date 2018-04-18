using Newtonsoft.Json;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Categorization.Internal;
using Sahara.Core.Application.Categorization.Models;
using Sahara.Core.Application.Images.Records;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Validation;
using Sahara.Core.Common.Validation.ResponseTypes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Categorization.Public
{
    public static class CategorizationManager
    {

        public enum IdentificationType
        {
            Unknown,
            ID,
            Name,
        }


        #region TreeView

        public static List<CategoryTreeModel> GetCategoryTree(Account account, bool includeHidden = true, bool useCachedVersion = true)
        {
            List<CategoryTreeModel> categoryTree = null;

            #region Create Cache & HashField

            string redisHashField = string.Empty;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            redisHashField = ApplicationCategorizationHash.Fields.Tree(includeHidden);


            #endregion


            if (useCachedVersion)
            {
                #region Get category from cache (if user requests by name)

                try
                {
                    var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        categoryTree = JsonConvert.DeserializeObject<List<CategoryTreeModel>>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (categoryTree == null)
            {
                #region Build Category Tree

                categoryTree = new List<CategoryTreeModel>();

                // Level 0 -------------------

                var categories = GetCategories(account, includeHidden);

                foreach(var categoryListItem in categories)
                {
                    //Level 1 -------------
                    var categoryLevel = new CategoryTreeModel
                    {
                        ID = categoryListItem.CategoryID,
                        Name = categoryListItem.CategoryName,
                        NameKey = categoryListItem.CategoryNameKey,
                        FullyQualifiedName = categoryListItem.FullyQualifiedName,
                        LocationPath = categoryListItem.LocationPath,
                        Subcategories = new List<SubcategoryTreeModel>()
                    };                   
                    categoryLevel.Subcategories = new List<SubcategoryTreeModel>();

                    var category = GetCategoryByName(account, categoryListItem.CategoryNameKey, includeHidden);

                    foreach (SubcategoryListModel subcategory in category.Subcategories)
                    {
                        //Level 2 -------------
                        var subcategoryLevel = new SubcategoryTreeModel
                        {
                            ID = subcategory.SubcategoryID,
                            Name = subcategory.SubcategoryName,
                            NameKey = subcategory.SubcategoryNameKey,
                            FullyQualifiedName = subcategory.FullyQualifiedName,
                            LocationPath = subcategory.LocationPath
                        };
                        subcategoryLevel.Subsubcategories = new List<SubsubcategoryTreeModel>();

                        var subcategories = GetSubcategoryByFullyQualifiedName(account, category.CategoryNameKey, subcategory.SubcategoryNameKey, includeHidden);

                        foreach (SubsubcategoryListModel subsubcategory in subcategories.Subsubcategories)
                        {
                            //Level 3 -------------
                            var subsubcategoryLevel = new SubsubcategoryTreeModel
                            {
                                ID = subsubcategory.SubsubcategoryID,
                                Name = subsubcategory.SubsubcategoryName,
                                NameKey = subsubcategory.SubsubcategoryNameKey,
                                FullyQualifiedName = subsubcategory.FullyQualifiedName,
                                LocationPath = subsubcategory.LocationPath
                            };
                            subsubcategoryLevel.Subsubsubcategories = new List<SubsubsubcategoryTreeModel>();

                            var subsubcategories = GetSubsubcategoryByFullyQualifiedName(account, category.CategoryNameKey, subcategory.SubcategoryNameKey, subsubcategory.SubsubcategoryNameKey, includeHidden);

                            foreach (SubsubsubcategoryListModel subsubsubcategory in subsubcategories.Subsubsubcategories)
                            {
                                //Level 4 -------------
                                var subsubsubcategoryLevel = new SubsubsubcategoryTreeModel
                                {
                                    ID = subsubsubcategory.SubsubsubcategoryID,
                                    Name = subsubsubcategory.SubsubsubcategoryName,
                                    NameKey = subsubsubcategory.SubsubsubcategoryNameKey,
                                    FullyQualifiedName = subsubsubcategory.FullyQualifiedName,
                                    LocationPath = subsubsubcategory.LocationPath
                                };
                                
                                subsubcategoryLevel.Subsubsubcategories.Add(subsubsubcategoryLevel);
                            }

                            subcategoryLevel.Subsubcategories.Add(subsubcategoryLevel);
                        }

                        categoryLevel.Subcategories.Add(subcategoryLevel);

                    }

                    categoryTree.Add(categoryLevel);
                }

                #endregion

                if (categoryTree != null)
                {

                    #region Store category into cache (using short names ONLY)

                    try
                    {
                        cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                            ApplicationCategorizationHash.Fields.Tree(includeHidden),
                            JsonConvert.SerializeObject(categoryTree), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }

                    #endregion
                }

            }

            return categoryTree;
        }


        #endregion

        #region Categories

        #region Create

        public static DataAccessResponseType CreateCategory(Account account, string categoryName, bool isVisible = true)
        {
            //TO DO: Always clear/update caches AND update counts!

            var response = new DataAccessResponseType();

            #region Validate Category Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(categoryName);
            if (!ojectNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = ojectNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(ojectNameValidationResponse.validationMessage);

                return response;
            }

            #endregion


            var category = new CategoryModel {
                CategoryID = Guid.NewGuid(),
                CategoryName = categoryName,
                CategoryNameKey = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName),
                Visible = isVisible
            };


            #region Check if this name already exists

            var categoryExists = GetCategoryByName(account, category.CategoryNameKey);

            if (categoryExists != null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A category with that name already exists.";

                return response;
            }

            #endregion


            response = Sql.Statements.InsertStatements.InsertCategory(account.SqlPartition, account.SchemaName, category, account.PaymentPlan.MaxCategorizationsPerSet);

            if(response.isSuccess)
            {
                //Clear Category Caches:
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            //response.SuccessMessage = category.CategoryID.ToString(); //<--Returned for logging purposes
            response.SuccessMessages = new List<string>();
            response.SuccessMessages.Add(category.CategoryID.ToString()); //<--Returned for logging purposes
            response.SuccessMessage = category.CategoryNameKey; //<--Returned for edit new category on admin site

            return response;
        
        }

        #endregion

        #region Get
        /*
        public static int GetCategoryCount(Account account, bool useCachedVersion = true)
        {
            int count = 0;
            string cachedCount = null;
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            string redisHashField = ApplicationCategorizationHash.Fields.Count();  

            if (useCachedVersion)
            {
                #region Get count from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    cachedCount = JsonConvert.DeserializeObject<string>(redisValue);
                }

                #endregion
            }
            if (cachedCount == null)
            {
                #region Get count from SQL

                count = Sql.Statements.SelectStatements.SelectCategoryCount(account.SqlPartition, account.SchemaName);

                #endregion

                #region Store count into cache

                cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField, JsonConvert.SerializeObject(count), When.Always, CommandFlags.FireAndForget);

                #endregion
            }
            else
            {
                count = Convert.ToInt32(cachedCount);
            }
            

            return count;
        }
        */
        public static string GetCategoryIdFromName(Account account, string categoryName, bool useCachedVersion = true)
        {
            string id = null;
            string cachedId = null;
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = ApplicationCategorizationHash.Fields.Category(Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName), false);

            if (useCachedVersion)
            {
                #region Get id from cache

                try
                {
                    var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        CategoryModel category = JsonConvert.DeserializeObject<CategoryModel>(redisValue);
                        if(category != null)
                        {
                            cachedId = category.CategoryID.ToString();
                        }
                    
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (cachedId == null)
            {
                #region Get count from SQL

                id = Sql.Statements.SelectStatements.SelectCategoryIdByName(account.SqlPartition, account.SchemaName, categoryName);

                #endregion

            }
            else
            {
                id = cachedId;
            }

            return id;
        }

        public static CategoryModel GetCategoryByName(Account account, string categoryName, bool includeHiddenSubcategories = true, bool useCachedVersion = true)
        {
            CategoryModel category = null;

            #region Create Cache & HashField

            string redisHashField = string.Empty;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            redisHashField = ApplicationCategorizationHash.Fields.Category(Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName), includeHiddenSubcategories);
            

            #endregion


            if (useCachedVersion)
            {
                #region Get category from cache (if user requests by name)

                try
                {
                    var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        category = JsonConvert.DeserializeObject<CategoryModel>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (category == null)
            {
                #region Get category from SQL

                category = Sql.Statements.SelectStatements.SelectCategoryByName(account.SqlPartition, account.SchemaName, categoryName, includeHiddenSubcategories);

                #endregion

                if (category != null)
                {

                    #region Store category into cache (using short names ONLY)

                    try
                    {
                        cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                            ApplicationCategorizationHash.Fields.Category(category.CategoryNameKey, includeHiddenSubcategories),
                            JsonConvert.SerializeObject(category), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }

                    #endregion
                }

            }

            return category;
        }

        public static CategoryModel GetCategoryByID(Account account, string categoryId, bool includeHiddenSubcategories = true)
        {
            //We don't pull from cache here, but we will re-cache by names (below)

            CategoryModel category = null;

            #region Create Cache & HashField

            string redisHashField = string.Empty;

            #endregion

            #region Get category from SQL

            category = Sql.Statements.SelectStatements.SelectCategoryById(account.SqlPartition, account.SchemaName, categoryId, includeHiddenSubcategories);

            #endregion

            if (category != null)
            {

                #region Store category into cache (using short names ONLY)


                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                        ApplicationCategorizationHash.Fields.Category(category.CategoryNameKey, includeHiddenSubcategories),
                        JsonConvert.SerializeObject(category), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }

            

            return category;
        }


        /* Remove after testing
        public static CategoryModel GetCategory(Account account, string categoryAttribute, bool includeHiddenSubcategories = true, bool useCachedVersion = true, IdentificationType identificationType = IdentificationType.Unknown)
        {
            CategoryModel category = null;


            #region If Unknown: Detrmine if Attribute is a ID or a Name

            if (identificationType == IdentificationType.Unknown)
            {
                Guid categoryId;

                if (Guid.TryParse(categoryAttribute, out categoryId))
                {
                    //attribute is a CategoryID:
                    identificationType = IdentificationType.ID;
                }
                else
                {
                    //attribute is a CategoryName:
                    identificationType = IdentificationType.Name;
                }
            }

            #endregion

            #region Create Cache & HashField

            string redisHashField = string.Empty;

            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            
            if(identificationType == IdentificationType.Name)
            {
                redisHashField = ApplicationCategorizationHash.Fields.Category(Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryAttribute), includeHiddenSubcategories);
            }

            #endregion


            if (useCachedVersion && identificationType == IdentificationType.Name)
            {
                #region Get category from cache (if user requests by name)

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    category = JsonConvert.DeserializeObject<CategoryModel>(redisValue);
                }

                #endregion
            }
            if (category == null)
            {
                #region Get category from SQL

                if(identificationType == IdentificationType.ID)
                {
                    category = Sql.Statements.SelectStatements.SelectCategoryById(account.SqlPartition, account.SchemaName, categoryAttribute, includeHiddenSubcategories);
                }
                else
                {
                    category = Sql.Statements.SelectStatements.SelectCategoryByName(account.SqlPartition, account.SchemaName, categoryAttribute, includeHiddenSubcategories);
                }
                
                #endregion

                if (category != null)
                {
                    #region Get Subcategories

                    //category.Subcategories = GetSubcategories(account, category.CategoryID.ToString(), includeHiddenSubcategories);

                    #endregion

                    #region Store category into cache (using short names ONLY)

                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                        ApplicationCategorizationHash.Fields.Category(category.CategoryNameKey, includeHiddenSubcategories),
                        JsonConvert.SerializeObject(category), When.Always, CommandFlags.FireAndForget);
                    
                    #endregion
                }

            }

            return category;
        }
        */
        public static List<CategoryModel> GetCategories(Account account, bool includeHidden, bool useCachedVersion = true)
        {
            List<CategoryModel> categories = null;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = ApplicationCategorizationHash.Fields.List(includeHidden);

            if (useCachedVersion)
            {
                #region Get categories from cache

                try
                {
                    var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        categories = JsonConvert.DeserializeObject<List<CategoryModel>>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (categories == null)
            {
                #region Get categories from SQL

                categories = Sql.Statements.SelectStatements.SelectCategoryList(account.SqlPartition, account.SchemaName, includeHidden);

                #endregion

                #region Store count into cache

                try
                {
                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField, JsonConvert.SerializeObject(categories), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }

            return categories;
        }

        #endregion

        #region Update

        public static DataAccessResponseType RenameCategory(Account account, string categoryId, string newCategoryName)
        {
            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not implemented until renameing is implemented into worker role background task across ALL references within DocumentStorage and SerachIndex." };

            /*

            var response = new DataAccessResponseType();

            #region Validate Category Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(newCategoryName);
            if (!ojectNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = ojectNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(ojectNameValidationResponse.validationMessage);

                return response;
            }

            #endregion

            #region Check if this name already exists & is not of the same categoryId (Depricatied)
            /*
            //var categoryExists = GetCategory(account, newCategoryName, true, true, IdentificationType.Name);
            bool nameExists = Sql.Statements.SelectStatements.CategoryNameExistsElsewhere(account.SqlPartition, account.SchemaName, categoryId, newCategoryName);

            if (nameExists)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A category with that name already exists.";

                return response;
            }
            * /
            #endregion

            #region Make sure the new name still converts to the same short name

            var existingCategory = GetCategoryByID(account, categoryId);

            if(Common.Methods.ObjectNames.ConvertToObjectNameKey(newCategoryName) != existingCategory.CategoryNameKey)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A category name can only be edited to adjust casing and spacing.";

                return response;
            }

            #endregion

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateCategoryName(account.SqlPartition, account.SchemaName, categoryId, newCategoryName);

            if(response.isSuccess)
            {
                //Add the new CategoryNameKey to the results object
                response.Results = new List<string>();
                response.Results.Add(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(newCategoryName));

                //Clear all associated caches
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }


            return response;*/

        }

        public static DataAccessResponseType UpdateCategoryVisibleState(Account account, string categoryId, bool isVisible)
        {
            var response = new DataAccessResponseType();

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateCategoryVisibleState(account.SqlPartition, account.SchemaName, categoryId, isVisible);

            //Clear all associated caches
            if(response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;

        }

        public static DataAccessResponseType ReorderCategories(Account account, Dictionary<string, int> categoryOrderingDictionary)
        {
            var response = new DataAccessResponseType();

            response = Sql.Statements.UpdateStatements.ReorderCategories(account.SqlPartition, account.SchemaName, categoryOrderingDictionary);

            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType ResetCategoryOrdering(Account account)
        {
            var response = new DataAccessResponseType();

            response = Sql.Statements.UpdateStatements.ResetCategoryOrdering(account.SqlPartition, account.SchemaName);

            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdateCategoryDescription(Account account, string categoryId, string newDescription)
        {
            var response = new DataAccessResponseType();

            if(newDescription.Length > 1200)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Description cannot exceed 1200 characters" };
            }

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateCategoryDescription(account.SqlPartition, account.SchemaName, categoryId, newDescription);

            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;

        }

        #endregion

        #region Delete

        public static DataAccessResponseType DeleteCategory(Account account, string categoryId)
        {
            var response = new DataAccessResponseType();
         
            response.isSuccess = Sql.Statements.DeleteStatements.DeleteCategory(account.SqlPartition, account.SchemaName, categoryId);

            if(response.isSuccess)
            {
                ImageRecordsManager.DeleteAllImageRecordsForObject(account, "category", categoryId);

                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        #endregion

        #endregion

        #region Subcategories

        #region Create

        public static DataAccessResponseType CreateSubcategory(Account account, string categoryId, string subcategoryName, bool isVisible = true)
        {
            //TO DO: Always clear/update caches AND update counts!

            var response = new DataAccessResponseType();

            var category = GetCategoryByID(account, categoryId);

            if(category == null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "The category you are adding a subcategory to does not exist.";
                return response;
            }

            #region Validate Subcategory Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(subcategoryName);
            if (!ojectNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = ojectNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(ojectNameValidationResponse.validationMessage);

                return response;
            }

            #endregion

            var subcategory = new SubcategoryModel
            {
                CategoryID = category.CategoryID,
                SubcategoryID = Guid.NewGuid(),
                SubcategoryName = subcategoryName,
                SubcategoryNameKey = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryName),
                Visible = isVisible
            };

            #region Check if this name already exists

            var subcategoryExists = GetSubcategoryByFullyQualifiedName(account, category.CategoryNameKey, subcategory.SubcategoryNameKey, false, true);

            if (subcategoryExists != null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A subcategory with that name already exists on this category.";

                return response;
            }

            #endregion

            response = Sql.Statements.InsertStatements.InsertSubcategory(account.SqlPartition, account.SchemaName, subcategory, account.PaymentPlan.MaxCategorizationsPerSet);

            if (response.isSuccess)
            {
                //Clear Subcategory Caches for this category:
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            //response.SuccessMessage = subcategory.SubcategoryID.ToString();
            response.SuccessMessages = new List<string>();
            response.SuccessMessages.Add(subcategory.SubcategoryID.ToString()); //<--Returned for logging purposes
            response.SuccessMessage =
                category.CategoryNameKey + 
                "/" + subcategory.SubcategoryNameKey;
                //<--Returned for edit new category on admin site

            return response;

        }

        #endregion

        #region Get
        /*
        public static int GetSubcategoryCount(Account account, bool useCachedVersion = true)
        {
            int count = 0;
            string cachedCount = null;
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            string redisHashField = ApplicationCategorizationHash.Fields.SubcategoryCount();

            if (useCachedVersion)
            {
                #region Get count from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    cachedCount = JsonConvert.DeserializeObject<string>(redisValue);
                }

                #endregion
            }
            if (cachedCount == null)
            {
                #region Get count from SQL

                count = Sql.Statements.SelectStatements.SelectSubcategoryCount(account.SqlPartition, account.SchemaName);

                #endregion

                #region Store count into cache

                cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField, JsonConvert.SerializeObject(count), When.Always, CommandFlags.FireAndForget);

                #endregion
            }
            else
            {
                count = Convert.ToInt32(cachedCount);
            }


            return count;
        }
        */
        public static SubcategoryModel GetSubcategoryByFullyQualifiedName(Account account, string categoryName, string subcategoryName, bool includeHiddenObjects, bool useCachedVersion = true)
        {
            SubcategoryModel subcategory = null;


            #region Create Cache & HashField (If attributes are BOTH names)

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            string redisHashField = string.Empty;

            redisHashField = ApplicationCategorizationHash.Fields.Subcategory(
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryName),
                    includeHiddenObjects);
            

            #endregion

            if (useCachedVersion)
            {
                #region Get subcategory from cache

                try
                {
                    var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        subcategory = JsonConvert.DeserializeObject<SubcategoryModel>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (subcategory == null)
            {

                #region Get subcategory from SQL

                subcategory = Sql.Statements.SelectStatements.SelectSubcategoryByNames(account.SqlPartition, account.SchemaName, categoryName, subcategoryName, includeHiddenObjects);

                #endregion

                if (subcategory != null)
                {
                    #region Store subcategory into cache (By NAMES reference ONLY)

                    try
                    {
                        cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                            ApplicationCategorizationHash.Fields.Subcategory(
                                subcategory.Category.CategoryNameKey,
                                subcategory.SubcategoryNameKey,
                                includeHiddenObjects),
                            JsonConvert.SerializeObject(subcategory), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }

                    #endregion
                }
            }

            return subcategory;
        }

        public static SubcategoryModel GetSubcategoryByID(Account account, string subcategoryId, bool includeHiddenObjects)
        {
            SubcategoryModel subcategory = null;
            
            #region Get subcategory from SQL

                subcategory = Sql.Statements.SelectStatements.SelectSubcategoryById(account.SqlPartition, account.SchemaName, subcategoryId, includeHiddenObjects);

                #endregion

            if (subcategory != null)
            {
                #region Store subcategory into cache (By NAMES reference ONLY)

                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                            ApplicationCategorizationHash.Fields.Subcategory(
                                subcategory.Category.CategoryNameKey,
                                subcategory.SubcategoryNameKey,
                                includeHiddenObjects),
                            JsonConvert.SerializeObject(subcategory), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                    #endregion
            }
            

            return subcategory;
        }


        /* Remove after testing
        public static SubcategoryModel GetSubcategory(Account account, string categoryAttribute, string subcategoryAttribute, bool includeHiddenObjects, bool useCachedVersion = true, IdentificationType identificationTypes = IdentificationType.Unknown)
        {
            SubcategoryModel subcategory = null;

            var categoryIdentificationType = IdentificationType.Unknown;
            var subcategoryIdentificationType = IdentificationType.Unknown;

            Guid subcategoryId;
            Guid categoryId;

            #region If Unknown or Mixed: Detrmine if Attributes are an ID's or Name's
                
            //-- Category Attribute

            if (Guid.TryParse(categoryAttribute, out categoryId))
            {
                //attribute is a CategoryID:
                categoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a CategoryName:
                categoryIdentificationType = IdentificationType.Name;
            }

            //-- Subcategory Attribute

            if (Guid.TryParse(subcategoryAttribute, out subcategoryId))
            {
                //attribute is a SubcategoryID:
                subcategoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a SubcategoryName:
                subcategoryIdentificationType = IdentificationType.Name;
            }

            #endregion

            //var category = GetCategory(account, categoryAttribute);
            //var categoryId = category.CategoryID.ToString();

            #region Create Cache & HashField (If attributes are BOTH names)

            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();

            string redisHashField = string.Empty;

            if (categoryIdentificationType == IdentificationType.Name && subcategoryIdentificationType == IdentificationType.Name)
            {
                redisHashField = ApplicationCategorizationHash.Fields.Subcategory(
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryAttribute),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryAttribute),
                    includeHiddenObjects);
            }

            #endregion

            if (useCachedVersion && categoryIdentificationType == IdentificationType.Name && subcategoryIdentificationType == IdentificationType.Name)
            {
                #region Get subcategory from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    subcategory = JsonConvert.DeserializeObject<SubcategoryModel>(redisValue);
                }

                #endregion
            }
            if (subcategory == null)
            {
                
                #region Get subcategory from SQL

                if (subcategoryIdentificationType == IdentificationType.ID)
                {
                    subcategory = Sql.Statements.SelectStatements.SelectSubcategoryById(account.SqlPartition, account.SchemaName, subcategoryAttribute, includeHiddenObjects);
                }
                else if (subcategoryIdentificationType == IdentificationType.Name && categoryIdentificationType == IdentificationType.Name)
                {
                    subcategory = Sql.Statements.SelectStatements.SelectSubcategoryByNames(account.SqlPartition, account.SchemaName, categoryAttribute, subcategoryAttribute, includeHiddenObjects);
                }
                else if (subcategoryIdentificationType == IdentificationType.Name && categoryIdentificationType == IdentificationType.ID)
                {
                    subcategory = Sql.Statements.SelectStatements.SelectSubcategoryByNameAndCategoryId(account.SqlPartition, account.SchemaName, categoryAttribute, subcategoryAttribute, includeHiddenObjects);
                }

                #endregion

                if (subcategory != null)
                {
                    #region Store subcategory into cache (By NAMES reference ONLY)

                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                        ApplicationCategorizationHash.Fields.Subcategory(
                            subcategory.Category.CategoryNameKey,
                            subcategory.SubcategoryNameKey,
                            includeHiddenObjects),
                        JsonConvert.SerializeObject(subcategory), When.Always, CommandFlags.FireAndForget);

                    #endregion
                }
            }

            return subcategory;
        }
        */
        /* Removed: One should use GetCategory to get the Subcategory list
        public static List<SubcategoryModel> GetSubcategories(Account account, string categoryAttribute, bool includeHidden, bool useCachedVersion = true)
        {
            List<SubcategoryModel> subcategories = null;
            Guid categoryId;
 
            //if (!Guid.TryParse(categoryAttribute, out categoryId))
            //{
                //categoryId = new Guid(GetCategoryIdFromName(account, categoryAttribute));
            //}

            IdentificationType identificationType = IdentificationType.Unknown;

            #region Determine if Attribute is a ID or a Name

            if (Guid.TryParse(categoryAttribute, out categoryId))
            {
                //attribute is a CategoryID:
                identificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a CategoryName:
                identificationType = IdentificationType.Name;
            }
            
            #endregion

            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            string redisHashField = ApplicationCategorizationHash.Fields.Category(categoryAttribute, includeHidden);

            if (useCachedVersion && identificationType == IdentificationType.Name)
            {
                #region Get subcategories from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    var category = JsonConvert.DeserializeObject<CategoryModel>(redisValue);
                    subcategories = category.Subcategories;
                }

                #endregion
            }
            if (subcategories == null)
            {
                if(identificationType != IdentificationType.ID)
                {
                    //Get the categoryID
                    categoryId = new Guid(GetCategoryIdFromName(account, categoryAttribute));
                }

                #region Get subcategories from SQL

                subcategories = Sql.Statements.SelectStatements.SelectSubcategoryList(account.SqlPartition, account.SchemaName, categoryId.ToString(), includeHidden);

                #endregion

                #region Store into cache

                cache.HashSet(ApplicationSubcategoryHash.Key(schemaId, categoryAttribute), redisHashField, JsonConvert.SerializeObject(subcategories), When.Always, CommandFlags.FireAndForget);

                #endregion
            }

            return subcategories;
        }
        */

        #endregion

        #region Update

        public static DataAccessResponseType RenameSubcategory(Account account, string subcategoryId, string newSubcategoryName)
        {
            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not implemented until renameing is implemented into worker role background task across ALL references within DocumentStorage and SerachIndex." };

            /*
            var response = new DataAccessResponseType();

            #region Validate Category Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(newSubcategoryName);
            if (!ojectNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = ojectNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(ojectNameValidationResponse.validationMessage);

                return response;
            }

            #endregion

            #region Check if this name already exists AND is not the same categoryId (Depricated)
            /*
            //var subcategoryExists = GetSubcategory(account, categoryId, newSubcategoryName, true, true);
            var nameExists = Sql.Statements.SelectStatements.SubcategoryNameExistsElsewhere(account.SqlPartition, account.SchemaName, categoryId, subcategoryId, newSubcategoryName);

            if (nameExists)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A subcategory with that name already exists in this category.";

                return response;
            }
            * /
            #endregion

            #region Make sure the new name still converts to the same short name

            var existingSubcategory = GetSubcategoryByID(account, subcategoryId, false);

            if (Common.Methods.ObjectNames.ConvertToObjectNameKey(newSubcategoryName) != existingSubcategory.SubcategoryNameKey)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A subcategory name can only be edited to adjust casing and spacing.";

                return response;
            }

            #endregion

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateSubcategoryName(account.SqlPartition, account.SchemaName, subcategoryId, newSubcategoryName);

            if (response.isSuccess)
            {
                //Add the new CategoryNameKey to the results object
                response.Results = new List<string>();
                response.Results.Add(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(newSubcategoryName));

                //Clear all associated caches
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            

            return response;

            */

        }

        public static DataAccessResponseType UpdateSubcategoryVisibleState(Account account, string subcategoryId, bool isVisible)
        {
            var response = new DataAccessResponseType();

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateSubcategoryVisibleState(account.SqlPartition, account.SchemaName, subcategoryId, isVisible);

            //Clear all associated caches
            Caching.InvalidateCategorizationCaches(account.AccountNameKey);

            return response;

        }

        public static DataAccessResponseType ReorderSubcategories(Account account, Dictionary<string, int> subcategoryOrderingDictionary)
        {
            var response = new DataAccessResponseType();

            response = Sql.Statements.UpdateStatements.ReorderSubcategories(account.SqlPartition, account.SchemaName, subcategoryOrderingDictionary);

            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType ResetSubcategoryOrdering(Account account, string categoryId)
        {
            var response = new DataAccessResponseType();

            response = Sql.Statements.UpdateStatements.ResetSubcategoryOrdering(account.SqlPartition, account.SchemaName, categoryId);

            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdateSubcategoryDescription(Account account, string subcategoryId, string newDescription)
        {
            var response = new DataAccessResponseType();

            if (newDescription.Length > 1200)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Description cannot exceed 1200 characters" };
            }

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateSubcategoryDescription(account.SqlPartition, account.SchemaName, subcategoryId, newDescription);

            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;

        }


        #endregion

        #region Delete

        public static DataAccessResponseType DeleteSubcategory(Account account, string subcategoryId)
        {
            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.DeleteStatements.DeleteSubcategory(account.SqlPartition, account.SchemaName, subcategoryId);

            if(response.isSuccess)
            {
                ImageRecordsManager.DeleteAllImageRecordsForObject(account, "subcategory", subcategoryId);

                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        #endregion

        #endregion

        #region Subsubcategories

        #region Create

        public static DataAccessResponseType CreateSubsubcategory(Account account, string subcategoryId, string subsubcategoryName, bool isVisible = true)
        {
            //TO DO: Always clear/update caches AND update counts!

            var response = new DataAccessResponseType();

            //Get Subcategory
            var subcategory = GetSubcategoryByID(account, subcategoryId, false);

            if (subcategory == null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "The subcategory you are adding a subsubcategory to does not exist.";
                return response;
            }

            #region Validate Subsubcategory Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(subsubcategoryName);
            if (!ojectNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = ojectNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(ojectNameValidationResponse.validationMessage);

                return response;
            }

            #endregion

            var subsubcategory = new SubsubcategoryModel
            {
                CategoryID = subcategory.Category.CategoryID,
                SubcategoryID = subcategory.SubcategoryID,
                SubsubcategoryID = Guid.NewGuid(),
                SubsubcategoryName = subsubcategoryName,
                SubsubcategoryNameKey = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubcategoryName),
                Visible = isVisible
            };

            #region Check if this name already exists

            var subsubcategoryExists = GetSubsubcategoryByFullyQualifiedName(account, subcategory.Category.CategoryNameKey, subcategory.SubcategoryNameKey, subsubcategory.SubsubcategoryNameKey, false, true);

            if (subsubcategoryExists != null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A subsubcategory with that name already exists on this subcategory.";

                return response;
            }

            #endregion

            response = Sql.Statements.InsertStatements.InsertSubsubcategory(account.SqlPartition, account.SchemaName, subsubcategory, account.PaymentPlan.MaxCategorizationsPerSet);

            if (response.isSuccess)
            {
                //Clear Subcategory Caches for this category:
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            //response.SuccessMessage = subsubcategory.SubsubcategoryID.ToString();
            response.SuccessMessages = new List<string>();
            response.SuccessMessages.Add(subsubcategory.SubsubcategoryID.ToString()); //<--Returned for logging purposes
            response.SuccessMessage =
                subcategory.Category.CategoryNameKey +
                "/" + subcategory.SubcategoryNameKey +
                "/" + subsubcategory.SubsubcategoryNameKey;
            //<--Returned for edit new category on admin site


            return response;

        }

        #endregion

        #region Get
        /*
        public static int GetSubsubcategoryCount(Account account, bool useCachedVersion = true)
        {
            int count = 0;
            string cachedCount = null;
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            string redisHashField = ApplicationCategorizationHash.Fields.SubsubcategoryCount();

            if (useCachedVersion)
            {
                #region Get count from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    cachedCount = JsonConvert.DeserializeObject<string>(redisValue);
                }

                #endregion
            }
            if (cachedCount == null)
            {
                #region Get count from SQL

                count = Sql.Statements.SelectStatements.SelectSubsubcategoryCount(account.SqlPartition, account.SchemaName);

                #endregion

                #region Store count into cache

                cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField, JsonConvert.SerializeObject(count), When.Always, CommandFlags.FireAndForget);

                #endregion
            }
            else
            {
                count = Convert.ToInt32(cachedCount);
            }


            return count;
        }
        */
        public static SubsubcategoryModel GetSubsubcategoryByFullyQualifiedName(Account account, string categoryNameKey, string subcategoryNameKey, string subsubcategoryNameKey, bool includeHiddenObjects, bool useCachedVersion = true)
        {
            SubsubcategoryModel subsubcategory = null;


            #region Create Cache & HashField

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            string redisHashField = string.Empty;

            redisHashField = ApplicationCategorizationHash.Fields.Subsubcategory(
                    categoryNameKey,
                    subcategoryNameKey,
                    subsubcategoryNameKey,
                    includeHiddenObjects);
            

            #endregion

            if (useCachedVersion)
            {
                #region Get subcategory from cache

                try
                {
                    var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        subsubcategory = JsonConvert.DeserializeObject<SubsubcategoryModel>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (subsubcategory == null)
            {

                #region Get subsubcategory from SQL

                subsubcategory = Sql.Statements.SelectStatements.SelectSubsubcategoryByNames(account.SqlPartition, account.SchemaName, categoryNameKey, subcategoryNameKey, subsubcategoryNameKey, includeHiddenObjects);

                #endregion

                if (subsubcategory != null)
                {
                    #region Store subcategory into cache (By NAMES reference ONLY)

                    try
                    {
                        cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                            ApplicationCategorizationHash.Fields.Subsubcategory(
                                subsubcategory.Category.CategoryNameKey,
                                subsubcategory.Subcategory.SubcategoryNameKey,
                                subsubcategory.SubsubcategoryNameKey,
                                includeHiddenObjects),
                            JsonConvert.SerializeObject(subsubcategory), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }

                    #endregion
                }
            }

            return subsubcategory;
        }

        public static SubsubcategoryModel GetSubsubcategoryByID(Account account, string subsubcategoryId, bool includeHiddenObjects)
        {
            //We don't pull from cache here, but we will re-cache by names (below)

            SubsubcategoryModel subsubcategory = null;

            #region Get subsubcategory from SQL

                subsubcategory = Sql.Statements.SelectStatements.SelectSubsubcategoryById(account.SqlPartition, account.SchemaName, subsubcategoryId, includeHiddenObjects);

                #endregion

            if (subsubcategory != null)
            {
                #region Store subcategory into cache (By NAMES reference ONLY)

                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                            ApplicationCategorizationHash.Fields.Subsubcategory(
                                subsubcategory.Category.CategoryNameKey,
                                subsubcategory.Subcategory.SubcategoryNameKey,
                                subsubcategory.SubsubcategoryNameKey,
                                includeHiddenObjects),
                            JsonConvert.SerializeObject(subsubcategory), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }
            

            return subsubcategory;
        }


        /* Remove after testing
        public static SubsubcategoryModel GetSubsubcategory(Account account, string categoryAttribute, string subcategoryAttribute, string subsubcategoryAttribute, bool includeHiddenObjects, bool useCachedVersion = true, IdentificationType identificationTypes = IdentificationType.Unknown)
        {
            SubsubcategoryModel subsubcategory = null;

            var categoryIdentificationType = IdentificationType.Unknown;
            var subcategoryIdentificationType = IdentificationType.Unknown;
            var subsubcategoryIdentificationType = IdentificationType.Unknown;

            Guid subsubcategoryId;
            Guid subcategoryId;
            Guid categoryId;

            #region If Unknown or Mixed: Detrmine if Attributes are an ID's or Name's

            //-- Category Attribute

            if (Guid.TryParse(categoryAttribute, out categoryId))
            {
                //attribute is a CategoryID:
                categoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a CategoryName:
                categoryIdentificationType = IdentificationType.Name;
            }

            //-- Subcategory Attribute

            if (Guid.TryParse(subcategoryAttribute, out subcategoryId))
            {
                //attribute is a SubcategoryID:
                subcategoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a SubcategoryName:
                subcategoryIdentificationType = IdentificationType.Name;
            }

            //-- Subsubcategory Attribute

            if (Guid.TryParse(subsubcategoryAttribute, out subsubcategoryId))
            {
                //attribute is a SubcategoryID:
                subsubcategoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a SubcategoryName:
                subsubcategoryIdentificationType = IdentificationType.Name;
            }

            #endregion

            //var category = GetCategory(account, categoryAttribute);
            //var categoryId = category.CategoryID.ToString();

            #region Create Cache & HashField (If attributes are BOTH names)

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            string redisHashField = string.Empty;

            if (categoryIdentificationType == IdentificationType.Name
                && subcategoryIdentificationType == IdentificationType.Name
                && subsubcategoryIdentificationType == IdentificationType.Name)
            {
                redisHashField = ApplicationCategorizationHash.Fields.Subsubcategory(
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryAttribute),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryAttribute),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubcategoryAttribute),
                    includeHiddenObjects);
            }

            #endregion

            if (useCachedVersion
                && categoryIdentificationType == IdentificationType.Name
                && subcategoryIdentificationType == IdentificationType.Name
                && subsubcategoryIdentificationType == IdentificationType.Name)
            {
                #region Get subcategory from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    subsubcategory = JsonConvert.DeserializeObject<SubsubcategoryModel>(redisValue);
                }

                #endregion
            }
            if (subsubcategory == null)
            {

                #region Get subsubcategory from SQL

                if (subsubcategoryIdentificationType == IdentificationType.ID)
                {
                    subsubcategory = Sql.Statements.SelectStatements.SelectSubsubcategoryById(account.SqlPartition, account.SchemaName, subsubcategoryAttribute, includeHiddenObjects);
                }
                else if (subsubcategoryIdentificationType == IdentificationType.Name
                    && subcategoryIdentificationType == IdentificationType.Name
                    && categoryIdentificationType == IdentificationType.Name)
                {
                    subsubcategory = Sql.Statements.SelectStatements.SelectSubsubcategoryByNames(account.SqlPartition, account.SchemaName, categoryAttribute, subcategoryAttribute, subsubcategoryAttribute, includeHiddenObjects);
                }
                else if (subcategoryIdentificationType == IdentificationType.Name && categoryIdentificationType == IdentificationType.ID)
                {
                    //Why? When is this used? Is there an alternative?
                    //subcategory = Sql.Statements.SelectStatements.SelectSubcategoryByNameAndCategoryId(account.SqlPartition, account.SchemaName, categoryAttribute, subcategoryAttribute, includeHiddenObjects);
                }

                #endregion

                if (subsubcategory != null)
                {
                    #region Store subcategory into cache (By NAMES reference ONLY)

                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                        ApplicationCategorizationHash.Fields.Subsubcategory(
                            subsubcategory.Category.CategoryNameKey,
                            subsubcategory.Subcategory.SubcategoryNameKey,
                            subsubcategory.SubsubcategoryNameKey,
                            includeHiddenObjects),
                        JsonConvert.SerializeObject(subsubcategory), When.Always, CommandFlags.FireAndForget);

                    #endregion
                }
            }

            return subsubcategory;
        }
        */
        /* Removed: One should use GetCategory to get the Subcategory list
        public static List<SubcategoryModel> GetSubcategories(Account account, string categoryAttribute, bool includeHidden, bool useCachedVersion = true)
        {
            List<SubcategoryModel> subcategories = null;
            Guid categoryId;
 
            //if (!Guid.TryParse(categoryAttribute, out categoryId))
            //{
                //categoryId = new Guid(GetCategoryIdFromName(account, categoryAttribute));
            //}

            IdentificationType identificationType = IdentificationType.Unknown;

            #region Determine if Attribute is a ID or a Name

            if (Guid.TryParse(categoryAttribute, out categoryId))
            {
                //attribute is a CategoryID:
                identificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a CategoryName:
                identificationType = IdentificationType.Name;
            }
            
            #endregion

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = ApplicationCategorizationHash.Fields.Category(categoryAttribute, includeHidden);

            if (useCachedVersion && identificationType == IdentificationType.Name)
            {
                #region Get subcategories from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    var category = JsonConvert.DeserializeObject<CategoryModel>(redisValue);
                    subcategories = category.Subcategories;
                }

                #endregion
            }
            if (subcategories == null)
            {
                if(identificationType != IdentificationType.ID)
                {
                    //Get the categoryID
                    categoryId = new Guid(GetCategoryIdFromName(account, categoryAttribute));
                }

                #region Get subcategories from SQL

                subcategories = Sql.Statements.SelectStatements.SelectSubcategoryList(account.SqlPartition, account.SchemaName, categoryId.ToString(), includeHidden);

                #endregion

                #region Store into cache

                cache.HashSet(ApplicationSubcategoryHash.Key(schemaId, categoryAttribute), redisHashField, JsonConvert.SerializeObject(subcategories), When.Always, CommandFlags.FireAndForget);

                #endregion
            }

            return subcategories;
        }
        */

        #endregion

        #region Update

        public static DataAccessResponseType RenameSubsubcategory(Account account, string subsubcategoryId, string newSubsubcategoryName)
        {
            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not implemented until renameing is implemented into worker role background task across ALL references within DocumentStorage and SerachIndex." };

            /*
            var response = new DataAccessResponseType();

            #region Validate Category Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(newSubsubcategoryName);
            if (!ojectNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = ojectNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(ojectNameValidationResponse.validationMessage);

                return response;
            }

            #endregion

            #region Check if this name already exists AND is not the same categoryId (Depricated)
            /*
            //var subcategoryExists = GetSubcategory(account, categoryId, newSubcategoryName, true, true);
            var nameExists = Sql.Statements.SelectStatements.SubcategoryNameExistsElsewhere(account.SqlPartition, account.SchemaName, categoryId, subcategoryId, newSubcategoryName);

            if (nameExists)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A subcategory with that name already exists in this category.";

                return response;
            }
            * /
            #endregion

            #region Make sure the new name still converts to the same short name

            var existingSubsubcategory = Sql.Statements.SelectStatements.SelectSubsubcategoryById(account.SqlPartition, account.SchemaName, subsubcategoryId, true); // GetSubsubcategory(account, categoryId, subcategoryAttribute, subsubcategoryId, false, true, IdentificationType.ID);

            if (Common.Methods.ObjectNames.ConvertToObjectNameKey(newSubsubcategoryName) != existingSubsubcategory.SubsubcategoryNameKey)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A subsubcategory name can only be edited to adjust casing and spacing.";

                return response;
            }

            #endregion

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateSubsubcategoryName(account.SqlPartition, account.SchemaName, subsubcategoryId, newSubsubcategoryName);

            if (response.isSuccess)
            {
                //Add the new CategoryNameKey to the results object
                response.Results = new List<string>();
                response.Results.Add(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(newSubsubcategoryName));

                //Clear all associated caches
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }



            return response;
            */
        }

        public static DataAccessResponseType UpdateSubsubcategoryVisibleState(Account account, string subsubcategoryId, bool isVisible)
        {
            var response = new DataAccessResponseType();

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateSubsubcategoryVisibleState(account.SqlPartition, account.SchemaName, subsubcategoryId, isVisible);

            //Clear all associated caches
            Caching.InvalidateCategorizationCaches(account.AccountNameKey);

            return response;

        }

        public static DataAccessResponseType ReorderSubsubcategories(Account account, Dictionary<string, int> subsubcategoryOrderingDictionary)
        {
            var response = new DataAccessResponseType();

            response = Sql.Statements.UpdateStatements.ReorderSubsubcategories(account.SqlPartition, account.SchemaName, subsubcategoryOrderingDictionary);

            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType ResetSubsubcategoryOrdering(Account account, string subcategoryId)
        {
            var response = new DataAccessResponseType();

            response = Sql.Statements.UpdateStatements.ResetSubsubcategoryOrdering(account.SqlPartition, account.SchemaName, subcategoryId);

            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdateSubsubcategoryDescription(Account account, string subsubcategoryId, string newDescription)
        {
            var response = new DataAccessResponseType();

            if (newDescription.Length > 1200)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Description cannot exceed 1200 characters" };
            }

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateSubsubcategoryDescription(account.SqlPartition, account.SchemaName, subsubcategoryId, newDescription);

            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;

        }


        #endregion

        #region Delete

        public static DataAccessResponseType DeleteSubsubcategory(Account account, string subsubcategoryId)
        {
            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.DeleteStatements.DeleteSubsubcategory(account.SqlPartition, account.SchemaName, subsubcategoryId);

            if (response.isSuccess)
            {
                ImageRecordsManager.DeleteAllImageRecordsForObject(account, "subsubcategory", subsubcategoryId);

                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        #endregion

        #endregion

        #region Subsubsubcategories

        #region Create

        public static DataAccessResponseType CreateSubsubsubcategory(Account account, string subsubcategoryId, string subsubsubcategoryName, bool isVisible = true)
        {
            //TO DO: Always clear/update caches AND update counts!

            var response = new DataAccessResponseType();

            #region Validate Subsubsubcategory Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(subsubsubcategoryName);
            if (!ojectNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = ojectNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(ojectNameValidationResponse.validationMessage);

                return response;
            }

            #endregion

            //Get Subsubcategory
            var subsubcategory = GetSubsubcategoryByID(account, subsubcategoryId, true);

            if (subsubcategory == null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "The subsubcategory you are adding a subsubsubcategory to does not exist.";
                return response;
            }

            var subsubsubcategory = new SubsubsubcategoryModel
            {
                CategoryID = subsubcategory.Category.CategoryID,
                SubcategoryID = subsubcategory.Subcategory.SubcategoryID,
                SubsubcategoryID = subsubcategory.SubsubcategoryID,
                SubsubsubcategoryID = Guid.NewGuid(),
                SubsubsubcategoryName = subsubsubcategoryName,
                SubsubsubcategoryNameKey = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubsubcategoryName),
                Visible = isVisible
            };

            #region Check if this name already exists

            var subsubsubcategoryExists = GetSubsubsubcategoryByFullyQualifiedName(account, subsubcategory.Category.CategoryNameKey, subsubcategory.Subcategory.SubcategoryNameKey, subsubcategory.SubsubcategoryNameKey, subsubsubcategory.SubsubsubcategoryNameKey, false);

            if (subsubsubcategoryExists != null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A subsubsubcategory with that name already exists on this subsubcategory.";

                return response;
            }

            #endregion

            response = Sql.Statements.InsertStatements.InsertSubsubsubcategory(account.SqlPartition, account.SchemaName, subsubsubcategory, account.PaymentPlan.MaxCategorizationsPerSet);

            if (response.isSuccess)
            {
                //Clear Subcategory Caches for this category:
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            //response.SuccessMessage = subsubsubcategory.SubsubsubcategoryID.ToString();
            response.SuccessMessages = new List<string>();
            response.SuccessMessages.Add(subsubsubcategory.SubsubsubcategoryID.ToString()); //<--Returned for logging purposes
            response.SuccessMessage =
                subsubcategory.Category.CategoryNameKey +
                "/" + subsubcategory.Subcategory.SubcategoryNameKey +
                "/" + subsubcategory.SubsubcategoryNameKey +
                "/" + subsubsubcategory.SubsubsubcategoryNameKey;
            //<--Returned for edit new category on admin site

            return response;

        }

        #endregion

        #region Get

        /*
        public static int GetSubsubsubcategoryCount(Account account, bool useCachedVersion = true)
        {
            int count = 0;
            string cachedCount = null;
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = ApplicationCategorizationHash.Fields.SubsubsubcategoryCount();

            if (useCachedVersion)
            {
                #region Get count from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    cachedCount = JsonConvert.DeserializeObject<string>(redisValue);
                }

                #endregion
            }
            if (cachedCount == null)
            {
                #region Get count from SQL

                count = Sql.Statements.SelectStatements.SelectSubsubsubcategoryCount(account.SqlPartition, account.SchemaName);

                #endregion

                #region Store count into cache

                cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField, JsonConvert.SerializeObject(count), When.Always, CommandFlags.FireAndForget);

                #endregion
            }
            else
            {
                count = Convert.ToInt32(cachedCount);
            }


            return count;
        }
        */
        public static SubsubsubcategoryModel GetSubsubsubcategoryByFullyQualifiedName(Account account, string categoryName, string subcategoryName, string subsubcategoryName, string subsubsubcategoryName, bool useCachedVersion = true)
        {
            SubsubsubcategoryModel subsubsubcategory = null;

            #region Create Cache & HashField

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            string redisHashField = string.Empty;

                redisHashField = ApplicationCategorizationHash.Fields.Subsubsubcategory(
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryName),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubcategoryName),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubsubcategoryName)
                    );
            

            #endregion

            if (useCachedVersion)
            {
                #region Get subsubcategory from cache

                try
                {
                    var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        subsubsubcategory = JsonConvert.DeserializeObject<SubsubsubcategoryModel>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (subsubsubcategory == null)
            {

                #region Get subsubsubcategory from SQL

                subsubsubcategory = Sql.Statements.SelectStatements.SelectSubsubsubcategoryByNames(account.SqlPartition, account.SchemaName, categoryName, subcategoryName, subsubcategoryName, subsubsubcategoryName);
                #endregion

                if (subsubsubcategory != null)
                {
                    #region Store subcategory into cache (By NAMES reference ONLY)

                    try
                    {
                        cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                            ApplicationCategorizationHash.Fields.Subsubsubcategory(
                                subsubsubcategory.Category.CategoryNameKey,
                                subsubsubcategory.Subcategory.SubcategoryNameKey,
                                subsubsubcategory.Subsubcategory.SubsubcategoryNameKey,
                                subsubsubcategory.SubsubsubcategoryNameKey),
                            JsonConvert.SerializeObject(subsubsubcategory), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }

                    #endregion
                }
            }

            return subsubsubcategory;
        }

        public static SubsubsubcategoryModel GetSubsubsubcategoryByID(Account account, string subsubsubcategoryId)
        {
            //We don't pull from cache here, but we will re-cache by names (below)

            SubsubsubcategoryModel subsubsubcategory = null;
            #region Get subsubsubcategory from SQL

            subsubsubcategory = Sql.Statements.SelectStatements.SelectSubsubsubcategoryById(account.SqlPartition, account.SchemaName, subsubsubcategoryId);
            #endregion

            if (subsubsubcategory != null)
            {
                #region Store subcategory into cache (By NAMES reference ONLY)

                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                        ApplicationCategorizationHash.Fields.Subsubsubcategory(
                            subsubsubcategory.Category.CategoryNameKey,
                            subsubsubcategory.Subcategory.SubcategoryNameKey,
                            subsubsubcategory.Subsubcategory.SubsubcategoryNameKey,
                            subsubsubcategory.SubsubsubcategoryNameKey),
                        JsonConvert.SerializeObject(subsubsubcategory), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }
            

            return subsubsubcategory;
        }


        //Needed?
        /*
        public static SubsubsubcategoryModel GetSubsubsubcategoryByID(Account account, string subsubsubcategoryId)
        {
            //We don't cache by Id, but will recache by names aftre aquiring

            SubsubsubcategoryModel subsubsubcategory = null;

            #region If Unknown or Mixed: Detrmine if Attributes are an ID's or Name's

            //-- Category Attribute

            if (Guid.TryParse(categoryAttribute, out categoryId))
            {
                //attribute is a CategoryID:
                categoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a CategoryName:
                categoryIdentificationType = IdentificationType.Name;
            }

            //-- Subcategory Attribute

            if (Guid.TryParse(subcategoryAttribute, out subcategoryId))
            {
                //attribute is a SubcategoryID:
                subcategoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a SubcategoryName:
                subcategoryIdentificationType = IdentificationType.Name;
            }

            //-- Subsubcategory Attribute

            if (Guid.TryParse(subsubcategoryAttribute, out subsubcategoryId))
            {
                //attribute is a SubcategoryID:
                subsubcategoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a SubcategoryName:
                subsubcategoryIdentificationType = IdentificationType.Name;
            }

            //-- Subsubsubcategory Attribute

            if (Guid.TryParse(subsubsubcategoryAttribute, out subsubsubcategoryId))
            {
                //attribute is a SubcategoryID:
                subsubsubcategoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a SubcategoryName:
                subsubsubcategoryIdentificationType = IdentificationType.Name;
            }

            #endregion

            //var category = GetCategory(account, categoryAttribute);
            //var categoryId = category.CategoryID.ToString();

            #region Create Cache & HashField (If attributes are BOTH names)

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            string redisHashField = string.Empty;

            if (categoryIdentificationType == IdentificationType.Name
                && subcategoryIdentificationType == IdentificationType.Name
                 && subsubcategoryIdentificationType == IdentificationType.Name
                  && subsubsubcategoryIdentificationType == IdentificationType.Name)
            {
                redisHashField = ApplicationCategorizationHash.Fields.Subsubsubcategory(
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryAttribute),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryAttribute),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubcategoryAttribute),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubsubcategoryAttribute),
                    includeHiddenObjects);
            }

            #endregion

            if (useCachedVersion
                && categoryIdentificationType == IdentificationType.Name
                && subcategoryIdentificationType == IdentificationType.Name
                && subsubcategoryIdentificationType == IdentificationType.Name
                && subsubsubcategoryIdentificationType == IdentificationType.Name)
            {
                #region Get subsubcategory from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    subsubsubcategory = JsonConvert.DeserializeObject<SubsubsubcategoryModel>(redisValue);
                }

                #endregion
            }
            if (subsubsubcategory == null)
            {

                #region Get subsubsubcategory from SQL

                if (subsubsubcategoryIdentificationType == IdentificationType.ID)
                {
                    subsubsubcategory = Sql.Statements.SelectStatements.SelectSubsubsubcategoryById(account.SqlPartition, account.SchemaName, subsubsubcategoryAttribute);
                }
                else if (
                    subsubsubcategoryIdentificationType == IdentificationType.Name
                    && subsubcategoryIdentificationType == IdentificationType.Name
                    && subcategoryIdentificationType == IdentificationType.Name
                    && categoryIdentificationType == IdentificationType.Name)
                {
                    subsubsubcategory = Sql.Statements.SelectStatements.SelectSubsubsubcategoryByNames(account.SqlPartition, account.SchemaName, categoryAttribute, subcategoryAttribute, subsubcategoryAttribute, subsubsubcategoryAttribute);
                }
                else if (subcategoryIdentificationType == IdentificationType.Name && categoryIdentificationType == IdentificationType.ID)
                {
                    //Why? When is this used? Is there an alternative?
                    //subcategory = Sql.Statements.SelectStatements.SelectSubcategoryByNameAndCategoryId(account.SqlPartition, account.SchemaName, categoryAttribute, subcategoryAttribute, includeHiddenObjects);
                }

                #endregion

                if (subsubsubcategory != null)
                {
                    #region Store subcategory into cache (By NAMES reference ONLY)

                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                        ApplicationCategorizationHash.Fields.Subsubsubcategory(
                            subsubsubcategory.Category.CategoryNameKey,
                            subsubsubcategory.Subcategory.SubcategoryNameKey,
                            subsubsubcategory.Subsubcategory.SubsubcategoryNameKey,
                            subsubsubcategory.SubsubsubcategoryNameKey,
                            includeHiddenObjects),
                        JsonConvert.SerializeObject(subsubsubcategory), When.Always, CommandFlags.FireAndForget);

                    #endregion
                }
            }

            return subsubsubcategory;
        }
        */
        /* REMOVE AFTER TESTING
        public static SubsubsubcategoryModel GetSubsubsubcategory(Account account, string categoryAttribute, string subcategoryAttribute, string subsubcategoryAttribute, string subsubsubcategoryAttribute, bool includeHiddenObjects, bool useCachedVersion = true, IdentificationType identificationTypes = IdentificationType.Unknown)
        {
            SubsubsubcategoryModel subsubsubcategory = null;

            var categoryIdentificationType = IdentificationType.Unknown;
            var subcategoryIdentificationType = IdentificationType.Unknown;
            var subsubcategoryIdentificationType = IdentificationType.Unknown;
            var subsubsubcategoryIdentificationType = IdentificationType.Unknown;

            Guid subsubsubcategoryId;
            Guid subsubcategoryId;
            Guid subcategoryId;
            Guid categoryId;

            #region If Unknown or Mixed: Detrmine if Attributes are an ID's or Name's

            //-- Category Attribute

            if (Guid.TryParse(categoryAttribute, out categoryId))
            {
                //attribute is a CategoryID:
                categoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a CategoryName:
                categoryIdentificationType = IdentificationType.Name;
            }

            //-- Subcategory Attribute

            if (Guid.TryParse(subcategoryAttribute, out subcategoryId))
            {
                //attribute is a SubcategoryID:
                subcategoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a SubcategoryName:
                subcategoryIdentificationType = IdentificationType.Name;
            }

            //-- Subsubcategory Attribute

            if (Guid.TryParse(subsubcategoryAttribute, out subsubcategoryId))
            {
                //attribute is a SubcategoryID:
                subsubcategoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a SubcategoryName:
                subsubcategoryIdentificationType = IdentificationType.Name;
            }

            //-- Subsubsubcategory Attribute

            if (Guid.TryParse(subsubsubcategoryAttribute, out subsubsubcategoryId))
            {
                //attribute is a SubcategoryID:
                subsubsubcategoryIdentificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a SubcategoryName:
                subsubsubcategoryIdentificationType = IdentificationType.Name;
            }

            #endregion

            //var category = GetCategory(account, categoryAttribute);
            //var categoryId = category.CategoryID.ToString();

            #region Create Cache & HashField (If attributes are BOTH names)

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            string redisHashField = string.Empty;

            if (categoryIdentificationType == IdentificationType.Name
                && subcategoryIdentificationType == IdentificationType.Name
                 && subsubcategoryIdentificationType == IdentificationType.Name
                  && subsubsubcategoryIdentificationType == IdentificationType.Name)
            {
                redisHashField = ApplicationCategorizationHash.Fields.Subsubsubcategory(
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryAttribute),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryAttribute),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubcategoryAttribute),
                    Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubsubcategoryAttribute),
                    includeHiddenObjects);
            }

            #endregion

            if (useCachedVersion
                && categoryIdentificationType == IdentificationType.Name
                && subcategoryIdentificationType == IdentificationType.Name
                && subsubcategoryIdentificationType == IdentificationType.Name
                && subsubsubcategoryIdentificationType == IdentificationType.Name)
            {
                #region Get subsubcategory from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    subsubsubcategory = JsonConvert.DeserializeObject<SubsubsubcategoryModel>(redisValue);
                }

                #endregion
            }
            if (subsubsubcategory == null)
            {

                #region Get subsubsubcategory from SQL

                if (subsubsubcategoryIdentificationType == IdentificationType.ID)
                {
                    subsubsubcategory = Sql.Statements.SelectStatements.SelectSubsubsubcategoryById(account.SqlPartition, account.SchemaName, subsubsubcategoryAttribute);
                }
                else if (
                    subsubsubcategoryIdentificationType == IdentificationType.Name
                    && subsubcategoryIdentificationType == IdentificationType.Name
                    && subcategoryIdentificationType == IdentificationType.Name
                    && categoryIdentificationType == IdentificationType.Name)
                {
                    subsubsubcategory = Sql.Statements.SelectStatements.SelectSubsubsubcategoryByNames(account.SqlPartition, account.SchemaName, categoryAttribute, subcategoryAttribute, subsubcategoryAttribute, subsubsubcategoryAttribute);
                }
                else if (subcategoryIdentificationType == IdentificationType.Name && categoryIdentificationType == IdentificationType.ID)
                {
                    //Why? When is this used? Is there an alternative?
                    //subcategory = Sql.Statements.SelectStatements.SelectSubcategoryByNameAndCategoryId(account.SqlPartition, account.SchemaName, categoryAttribute, subcategoryAttribute, includeHiddenObjects);
                }

                #endregion

                if (subsubsubcategory != null)
                {
                    #region Store subcategory into cache (By NAMES reference ONLY)

                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey),
                        ApplicationCategorizationHash.Fields.Subsubsubcategory(
                            subsubsubcategory.Category.CategoryNameKey,
                            subsubsubcategory.Subcategory.SubcategoryNameKey,
                            subsubsubcategory.Subsubcategory.SubsubcategoryNameKey,
                            subsubsubcategory.SubsubsubcategoryNameKey,
                            includeHiddenObjects),
                        JsonConvert.SerializeObject(subsubsubcategory), When.Always, CommandFlags.FireAndForget);

                    #endregion
                }
            }

            return subsubsubcategory;
        }
        */
        /* Removed: One should use GetCategory to get the Subcategory list
        public static List<SubcategoryModel> GetSubcategories(Account account, string categoryAttribute, bool includeHidden, bool useCachedVersion = true)
        {
            List<SubcategoryModel> subcategories = null;
            Guid categoryId;
 
            //if (!Guid.TryParse(categoryAttribute, out categoryId))
            //{
                //categoryId = new Guid(GetCategoryIdFromName(account, categoryAttribute));
            //}

            IdentificationType identificationType = IdentificationType.Unknown;

            #region Determine if Attribute is a ID or a Name

            if (Guid.TryParse(categoryAttribute, out categoryId))
            {
                //attribute is a CategoryID:
                identificationType = IdentificationType.ID;
            }
            else
            {
                //attribute is a CategoryName:
                identificationType = IdentificationType.Name;
            }
            
            #endregion

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = ApplicationCategorizationHash.Fields.Category(categoryAttribute, includeHidden);

            if (useCachedVersion && identificationType == IdentificationType.Name)
            {
                #region Get subcategories from cache

                var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                if (redisValue.HasValue)
                {
                    var category = JsonConvert.DeserializeObject<CategoryModel>(redisValue);
                    subcategories = category.Subcategories;
                }

                #endregion
            }
            if (subcategories == null)
            {
                if(identificationType != IdentificationType.ID)
                {
                    //Get the categoryID
                    categoryId = new Guid(GetCategoryIdFromName(account, categoryAttribute));
                }

                #region Get subcategories from SQL

                subcategories = Sql.Statements.SelectStatements.SelectSubcategoryList(account.SqlPartition, account.SchemaName, categoryId.ToString(), includeHidden);

                #endregion

                #region Store into cache

                cache.HashSet(ApplicationSubcategoryHash.Key(schemaId, categoryAttribute), redisHashField, JsonConvert.SerializeObject(subcategories), When.Always, CommandFlags.FireAndForget);

                #endregion
            }

            return subcategories;
        }
        */

        #endregion

        #region Update

        public static DataAccessResponseType RenameSubsubsubcategory(Account account, string subsubsubcategoryId, string newSubsubsubcategoryName)
        {
            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not implemented until renameing is implemented into worker role background task across ALL references within DocumentStorage and SerachIndex." };

            /*
            var response = new DataAccessResponseType();

            #region Validate Category Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(newSubsubsubcategoryName);
            if (!ojectNameValidationResponse.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = ojectNameValidationResponse.validationMessage;
                response.ErrorMessages.Add(ojectNameValidationResponse.validationMessage);

                return response;
            }

            #endregion

            #region Check if this name already exists AND is not the same categoryId (Depricated)
            /*
            //var subcategoryExists = GetSubcategory(account, categoryId, newSubcategoryName, true, true);
            var nameExists = Sql.Statements.SelectStatements.SubcategoryNameExistsElsewhere(account.SqlPartition, account.SchemaName, categoryId, subcategoryId, newSubcategoryName);

            if (nameExists)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A subcategory with that name already exists in this category.";

                return response;
            }
            * /
            #endregion

            #region Make sure the new name still converts to the same short name

            var existingSubsubsubcategory = Sql.Statements.SelectStatements.SelectSubsubsubcategoryById(account.SqlPartition, account.SchemaName, subsubsubcategoryId); // GetSubsubcategory(account, categoryId, subcategoryAttribute, subsubcategoryId, false, true, IdentificationType.ID);

            if (Common.Methods.ObjectNames.ConvertToObjectNameKey(newSubsubsubcategoryName) != existingSubsubsubcategory.SubsubsubcategoryNameKey)
            {
                response.isSuccess = false;
                response.ErrorMessage = "A subsubsubcategory name can only be edited to adjust casing and spacing.";

                return response;
            }

            #endregion

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateSubsubsubcategoryName(account.SqlPartition, account.SchemaName, subsubsubcategoryId, newSubsubsubcategoryName);

            if (response.isSuccess)
            {
                //Add the new CategoryNameKey to the results object
                response.Results = new List<string>();
                response.Results.Add(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(newSubsubsubcategoryName));

                //Clear all associated caches
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }



            return response;
            */
        }

        public static DataAccessResponseType UpdateSubsubsubcategoryVisibleState(Account account, string subsubsubcategoryId, bool isVisible)
        {
            var response = new DataAccessResponseType();

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateSubsubsubcategoryVisibleState(account.SqlPartition, account.SchemaName, subsubsubcategoryId, isVisible);

            //Clear all associated caches
            Caching.InvalidateCategorizationCaches(account.AccountNameKey);

            return response;

        }

        public static DataAccessResponseType ReorderSubsubsubcategories(Account account, Dictionary<string, int> subsubsubcategoryOrderingDictionary)
        {
            var response = new DataAccessResponseType();

            response = Sql.Statements.UpdateStatements.ReorderSubsubsubcategories(account.SqlPartition, account.SchemaName, subsubsubcategoryOrderingDictionary);

            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType ResetSubsubsubcategoryOrdering(Account account, string subsubcategoryId)
        {
            var response = new DataAccessResponseType();

            response = Sql.Statements.UpdateStatements.ResetSubsubsubcategoryOrdering(account.SqlPartition, account.SchemaName, subsubcategoryId);

            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdateSubsubsubcategoryDescription(Account account, string subsubsubcategoryId, string newDescription)
        {
            var response = new DataAccessResponseType();

            if (newDescription.Length > 1200)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Description cannot exceed 1200 characters" };
            }

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateSubsubsubcategoryDescription(account.SqlPartition, account.SchemaName, subsubsubcategoryId, newDescription);

            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;

        }


        #endregion

        #region Delete

        public static DataAccessResponseType DeleteSubsubsubcategory(Account account, string subsubsubcategoryId)
        {
            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.DeleteStatements.DeleteSubsubsubcategory(account.SqlPartition, account.SchemaName, subsubsubcategoryId);

            if (response.isSuccess)
            {
                ImageRecordsManager.DeleteAllImageRecordsForObject(account, "subsubsubcategory", subsubsubcategoryId);

                Caching.InvalidateCategorizationCaches(account.AccountNameKey);
            }

            return response;
        }

        #endregion

        #endregion

        #region Shared

        public static int GetCategorizationCount(Account account, bool useCachedVersion = true)
        {
            int count = 0;
            string cachedCount = null;
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string redisHashField = ApplicationCategorizationHash.Fields.Count();

            if (useCachedVersion)
            {
                #region Get count from cache

                try
                {
                    var redisValue = cache.HashGet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField);
                    if (redisValue.HasValue)
                    {
                        cachedCount = JsonConvert.DeserializeObject<string>(redisValue);
                    }
                }
                catch
                {

                }

                #endregion
            }
            if (cachedCount == null)
            {
                #region Get count from SQL

                count = Sql.Statements.SelectStatements.SelectCategorizationCount(account.SqlPartition, account.SchemaName);

                #endregion

                #region Store count into cache

                try
                {
                    cache.HashSet(ApplicationCategorizationHash.Key(account.AccountNameKey), redisHashField, JsonConvert.SerializeObject(count), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }
                
                #endregion
            }
            else
            {
                count = Convert.ToInt32(cachedCount);
            }


            return count;
        }

        #endregion
    }
}
