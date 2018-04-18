using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Capacity.Public;
using Sahara.Core.Application.Categorization.Models;
using Sahara.Core.Application.Categorization.Public;
using Sahara.Core.Application.Products.Public;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.AccountLogs;
using Sahara.Core.Logging.AccountLogs.Types;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Platform.Requests.Models;
using WCF.WcfEndpoints.Contracts.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Service.Application
{
    public class ApplicationCategorizationService : IApplicationCategorizationService
    {

        #region full tree view

        public List<CategoryTreeModel> GetCategoryTree(string accountNameKey, bool includeHidden, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountNameKey);
            return CategorizationManager.GetCategoryTree(account, includeHidden);
        }


        #endregion


        #region Categories

        #region Create

        public DataAccessResponseType CreateCategory(string accountId, string categoryName, bool isVisible, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Validate Plan Capabilities (Moved to SQL INSERT)

            /*
            //Verify that current category count is below maximum allowed by this plan
            if (CategorizationManager.GetCategoryCount(account) >= account.PaymentPlan.MaxCategories)
            {
                //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
                Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("categories", account.AccountID.ToString(), account.AccountName);

                
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for more than " + account.PaymentPlan.MaxCategories + " categories, please update your plan to add more." };

            }*/
            
            #endregion

            var result = CategorizationManager.CreateCategory(account, categoryName, isVisible);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_CategoryCreated,
                        "Category '" + categoryName + "' created",
                        requesterName + " created '" + categoryName + "' category",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessages[0]);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;

        }

        #endregion


        #region Update

        public DataAccessResponseType RenameCategory(string accountId, string categoryId, string newCategoryName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetCategoryByID(account, categoryId).CategoryName;


            var result = CategorizationManager.RenameCategory(account, categoryId, newCategoryName);
            
            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_CategoryRenamed,
                        "Category '" + objectName + "' renamed to '" + newCategoryName + "'",
                        requesterName + " renamed category '" + objectName + "' to '" + newCategoryName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        categoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateCategoryVisibleState(string accountId, string categoryId, bool visibleState, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetCategoryByID(account, categoryId).CategoryName;

            var result = CategorizationManager.UpdateCategoryVisibleState(account, categoryId, visibleState);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    var description = "Category '" + objectName + "' made visible";
                    var details = " made category '" + objectName + "' visible";

                    if (!visibleState)
                    {
                        description = "Category '" + objectName + "' hidden";
                        details = " made category '" + objectName + "' hidden";
                    }


                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_CategoryVisibilityChanged,
                        description,
                        requesterName + details,
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        categoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType ReorderCategories(string accountId, Dictionary<string, int> categoryOrderingDictionary, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Ensure ordering is allowed by account plan

            if(!account.PaymentPlan.AllowCustomOrdering)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for custom ordering of lists." };
            }

            #endregion

            var result = CategorizationManager.ReorderCategories(account, categoryOrderingDictionary);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_CategoriesReordered,
                        "Categories reordered",
                        requesterName + " reordered categories",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        null);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType ResetCategoryOrdering(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Ensure ordering is allowed by account plan

            if (!account.PaymentPlan.AllowCustomOrdering)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for custom ordering of lists." };
            }

            #endregion

            var result = CategorizationManager.ResetCategoryOrdering(account);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_CategoryOrderingReset,
                        "Category ordering reset",
                        requesterName + " reset category ordering",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        null);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateCategoryDescription(string accountId, string categoryId, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetCategoryByID(account, categoryId).CategoryName;

            var result = CategorizationManager.UpdateCategoryDescription(account, categoryId, newDescription);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    var description = "Category '" + objectName + "' description updated";
                    var details = " updated description on category '" + objectName + "'";

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_CategoryUpdated,
                        description,
                        requesterName + details,
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        categoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        #endregion

        #region Delete

        public DataAccessResponseType DeleteCategory(string accountId, string categoryId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get account
            var account  = AccountManager.GetAccount(accountId);

            //Get full category
            var category = CategorizationManager.GetCategoryByID(account, categoryId);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Validate Association Rules

            //check for any subcategories
            if(category.Subcategories.Count > 0)
            {
                return new DataAccessResponseType {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a category that has subcategories. Please delete or move all associated subcategories first."
                };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            //var objectName = CategorizationManager.GetCategory(account, categoryId).CategoryName;

            var result = CategorizationManager.DeleteCategory(account, categoryId);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_CategoryDeleted,
                        "Category '" + category.CategoryName + "' deleted",
                        requesterName + " deleted the '" + category.CategoryName + "' category",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        categoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;

        }

        #endregion

        #region Get

        //public int GetCategoryCount(string accountId)
        //{
            //var account = AccountManager.GetAccount(accountId);
            //return CategorizationManager.GetCategoryCount(account);
        //}

        public List<CategoryModel> GetCategories(string accountNameKey, bool includeHidden, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountNameKey);
            return CategorizationManager.GetCategories(account, includeHidden);
        }

        public CategoryModel GetCategoryByName(string accountNameKey, string categoryName, bool includeHiddenSubcategories, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountNameKey);
            return CategorizationManager.GetCategoryByName(account, categoryName, includeHiddenSubcategories);
        }

        //public CategoryModel GetCategory(string accountId, string categoryAttribute, bool includeHiddenSubcategories)
        //{
           // var account = AccountManager.GetAccount(accountId);
            //return CategorizationManager.GetCategory(account, categoryAttribute, includeHiddenSubcategories);
        //}

        #endregion


        #endregion

        #region Subcategories

        #region Create

        public DataAccessResponseType CreateSubcategory(string accountId, string categoryId, string subcategoryName, bool isVisible, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results with error message:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Validate Plan Capabilities (Moved to SQL INSERT)
            /*
            //Verify that current category count is below maximum allowed by this plan
            if (CategorizationManager.GetSubcategoryCount(account) >= account.PaymentPlan.MaxSubcategories)
            {
                //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
                Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("subcategories", account.AccountID.ToString(), account.AccountName);

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for more than " + account.PaymentPlan.MaxSubcategories + " subcategories, please update your plan to add more." };
            }
            */
            #endregion

            var result = CategorizationManager.CreateSubcategory(account, categoryId, subcategoryName, isVisible);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubcategoryCreated,
                        "Subcategory '" + subcategoryName + "' created",
                        requesterName + " created the '" + subcategoryName + "' subcategory",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessages[0]);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;

        }
        
        #endregion

        #region Update

        public DataAccessResponseType RenameSubcategory(string accountId, string subcategoryId, string newSubcategoryName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetSubcategoryByID(account, subcategoryId, false).SubcategoryName;


            var result = CategorizationManager.RenameSubcategory(account, subcategoryId, newSubcategoryName);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubcategoryRenamed,
                        "Subcategory '" + objectName + "' renamed to '" + newSubcategoryName + "'",
                        requesterName + " renamed subcategory '" + objectName + "' to '" + newSubcategoryName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateSubcategoryVisibleState(string accountId, string subcategoryId, bool visibleState, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion


            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetSubcategoryByID(account, subcategoryId, false).SubcategoryName;

            var result = CategorizationManager.UpdateSubcategoryVisibleState(account, subcategoryId, visibleState);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {
                    var description = "Subcategory '" + objectName + "' made visible";
                    var details = " made subcategory '" + objectName + "' visible";

                    if (!visibleState)
                    {
                        description = "Subcategory '" + objectName + "' hidden";
                        details = " made subcategory '" + objectName + "' hidden";
                    }


                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubcategoryVisibilityChanged,
                        description,
                        requesterName + details,
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType ReorderSubcategories(string accountId, Dictionary<string, int> subcategoryOrderingDictionary, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Ensure ordering is allowed by account plan

            if (!account.PaymentPlan.AllowCustomOrdering)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for custom ordering of lists." };
            }

            #endregion

            var result = CategorizationManager.ReorderSubcategories(account, subcategoryOrderingDictionary);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubcategoriesReordered,
                        "Subcategories reordered",
                        requesterName + " reordered subcategories",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        null);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType ResetSubcategoryOrdering(string accountId, string categoryId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Ensure ordering is allowed by account plan

            if (!account.PaymentPlan.AllowCustomOrdering)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for custom ordering of lists." };
            }

            #endregion

            var result = CategorizationManager.ResetSubcategoryOrdering(account, categoryId);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubcategoryOrderingReset,
                        "Subcategory ordering reset",
                        requesterName + " reset subcategory ordering",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        null);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateSubcategoryDescription(string accountId, string subcategoryId, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetSubcategoryByID(account, subcategoryId, false).SubcategoryName;

            var result = CategorizationManager.UpdateSubcategoryDescription(account, subcategoryId, newDescription);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    var description = "Subcategory '" + objectName + "' description updated";
                    var details = " updated description on subcategory '" + objectName + "'";

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubcategoryUpdated,
                        description,
                        requesterName + details,
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }


        #endregion

        #region Delete

        public DataAccessResponseType DeleteSubcategory(string accountId, string subcategoryId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get account
            var account = AccountManager.GetAccount(accountId);

            //Get full subcategory
            var subcategory = CategorizationManager.GetSubcategoryByID(account, subcategoryId, true);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Check for existance of child items before deletion

            bool productsExist = ProductManager.LocationPathContainsProducts(account, subcategory.FullyQualifiedName);


            //check for any subcategories
            if (productsExist && subcategory.Subsubcategories.Count > 0)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a subcategory that has subsubcategories and products. Please delete all subsubcategories and move products before deleting."
                };
            }
            else if(subcategory.Subsubcategories.Count > 0)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a subcategory that has subsubcategories. Please delete all subsubcategories before deleting."
                };
            }
            else if (productsExist)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a subcategory that has products associated. Please delete or move all products first."
                };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            //var objectName = CategorizationManager.GetSubcategory(account, categoryId, subcategoryId, false).SubcategoryName;


            var result = CategorizationManager.DeleteSubcategory(account, subcategoryId);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubcategoryDeleted,
                        "Subcategory '" + subcategory.SubcategoryName + "' deleted",
                        requesterName + " deleted the '" + subcategory.SubcategoryName + "' subcategory",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;

        }

        #endregion

        #region Get

        //public int GetSubcategoryCount(string accountId)
        //{
            //var account = AccountManager.GetAccount(accountId);
            //return CategorizationManager.GetSubcategoryCount(account);
        //}

        /* Removed - One should use: GetCategory
        public List<SubcategoryModel> GetSubcategories(string accountId, string categoryAttribute, bool includeHidden)
        {
            var account = AccountManager.GetAccount(accountId);
            return CategorizationManager.GetSubcategories(account.SqlPartition, account.SchemaName, categoryAttribute, includeHidden);
        }*/

        public SubcategoryModel GetSubcategoryByNames(string accountNameKey, string categoryName, string subcategoryName, bool includeHidden, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountNameKey);
            return CategorizationManager.GetSubcategoryByFullyQualifiedName(account, categoryName, subcategoryName, includeHidden);
        }

        public SubcategoryModel GetSubcategoryByFullyQualifiedName(string accountNameKey, string fullyQualifiedName, bool includeHidden, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            string[] keys = fullyQualifiedName.Split('/');

            string categoryNameKey;
            string subcategoryNameKey;

            try
            {
                categoryNameKey = keys[0];
                subcategoryNameKey = keys[1];
            }
            catch
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountNameKey);
            return CategorizationManager.GetSubcategoryByFullyQualifiedName(account, categoryNameKey, subcategoryNameKey, includeHidden);
        }

        //public SubcategoryModel GetSubcategory(string accountId, string categoryAttribute, string subcategoryAttribute, bool includeHidden)
        //{
        //var account = AccountManager.GetAccount(accountId);
        // return CategorizationManager.GetSubcategory(account, categoryAttribute, subcategoryAttribute, includeHidden);
        //}

        #endregion

        #endregion

        #region Subsubcategories

        #region Create

        public DataAccessResponseType CreateSubsubcategory(string accountId, string subcategoryId, string subsubcategoryName, bool isVisible, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results with error message:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Validate Plan Capabilities (Moved to SQL INSERT)
            /*
            //Verify that current category count is below maximum allowed by this plan
            if (CategorizationManager.GetSubsubcategoryCount(account) >= account.PaymentPlan.MaxSubsubcategories)
            {
                //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
                Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("subsubcategories", account.AccountID.ToString(), account.AccountName);

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for more than " + account.PaymentPlan.MaxSubsubcategories + " subsubcategories, please update your plan to add more." };
            }
            */
            #endregion

            var result = CategorizationManager.CreateSubsubcategory(account, subcategoryId, subsubcategoryName, isVisible);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubcategoryCreated,
                        "Subsubcategory '" + subsubcategoryName + "' created",
                        requesterName + " created the '" + subsubcategoryName + "' subsubcategory",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessages[0]);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;

        }

        #endregion

        #region Update

        public DataAccessResponseType RenameSubsubcategory(string accountId, string subsubcategoryId, string newSubsubcategoryName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetSubsubcategoryByID(account, subsubcategoryId, false).SubsubcategoryName;

            var result = CategorizationManager.RenameSubsubcategory(account, subsubcategoryId, newSubsubcategoryName);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubcategoryRenamed,
                        "Subsubcategory '" + objectName + "' renamed to '" + newSubsubcategoryName + "'",
                        requesterName + " renamed subsubcategory '" + objectName + "' to '" + newSubsubcategoryName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subsubcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateSubsubcategoryVisibleState(string accountId, string subsubcategoryId, bool visibleState, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion


            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetSubsubcategoryByID(account, subsubcategoryId, false).SubsubcategoryName;

            var result = CategorizationManager.UpdateSubsubcategoryVisibleState(account, subsubcategoryId, visibleState);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {
                    var description = "Subsubategory '" + objectName + "' made visible";
                    var details = " made subsubcategory '" + objectName + "' visible";

                    if (!visibleState)
                    {
                        description = "Subsubcategory '" + objectName + "' hidden";
                        details = " made subcsubategory '" + objectName + "' hidden";
                    }


                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubcategoryVisibilityChanged,
                        description,
                        requesterName + details,
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subsubcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType ReorderSubsubcategories(string accountId, Dictionary<string, int> subsubcategoryOrderingDictionary, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Ensure ordering is allowed by account plan

            if (!account.PaymentPlan.AllowCustomOrdering)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for custom ordering of lists." };
            }

            #endregion

            var result = CategorizationManager.ReorderSubsubcategories(account, subsubcategoryOrderingDictionary);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubcategoriesReordered,
                        "Subsubcategories reordered",
                        requesterName + " reordered subsubcategories",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        null);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType ResetSubsubcategoryOrdering(string accountId, string subcategoryId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Ensure ordering is allowed by account plan

            if (!account.PaymentPlan.AllowCustomOrdering)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for custom ordering of lists." };
            }

            #endregion

            var result = CategorizationManager.ResetSubsubcategoryOrdering(account, subcategoryId);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubcategoryOrderingReset,
                        "Subsubcategory ordering reset",
                        requesterName + " reset subsubcategory ordering",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        null);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateSubsubcategoryDescription(string accountId, string subsubcategoryId, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetSubsubcategoryByID(account, subsubcategoryId, false).SubsubcategoryName;

            var result = CategorizationManager.UpdateSubsubcategoryDescription(account, subsubcategoryId, newDescription);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    var description = "Subsubcategory '" + objectName + "' description updated";
                    var details = " updated description on subsubcategory '" + objectName + "'";

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubcategoryUpdated,
                        description,
                        requesterName + details,
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subsubcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }


        #endregion

        #region Delete

        public DataAccessResponseType DeleteSubsubcategory(string accountId, string subsubcategoryId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get account
            var account = AccountManager.GetAccount(accountId);

            //Get full subcategory
            var subsubcategory = CategorizationManager.GetSubsubcategoryByID(account, subsubcategoryId, true);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Check for existance of child items before deletion

            bool productsExist = ProductManager.LocationPathContainsProducts(account, subsubcategory.FullyQualifiedName);

            //check for any subcategories
            if (productsExist && subsubcategory.Subsubsubcategories.Count > 0)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a subsubcategory that has subsubsubcategories and products. Please delete all subsubsubcategories and move products before deleting."
                };
            }
            else if (subsubcategory.Subsubsubcategories.Count > 0)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a subsubcategory that has subsubsubcategories. Please delete all subsubsubcategories before deleting."
                };
            }
            else if (productsExist)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a subsubcategory that has products associated. Please delete or move all products first."
                };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            //var objectName = CategorizationManager.GetSubcategory(account, categoryId, subcategoryId, false).SubcategoryName;


            var result = CategorizationManager.DeleteSubsubcategory(account, subsubcategoryId);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubcategoryDeleted,
                        "Subsubcategory '" + subsubcategory.SubsubcategoryName + "' deleted",
                        requesterName + " deleted the '" + subsubcategory.SubsubcategoryName + "' subsubcategory",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subsubcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;

        }

        #endregion

        #region Get

        //public int GetSubsubcategoryCount(string accountId)
        //{
            //var account = AccountManager.GetAccount(accountId);
            //return CategorizationManager.GetSubsubcategoryCount(account);
        //}

        /* Removed - One should use: GetCategory
        public List<SubcategoryModel> GetSubcategories(string accountId, string categoryAttribute, bool includeHidden)
        {
            var account = AccountManager.GetAccount(accountId);
            return CategorizationManager.GetSubcategories(account.SqlPartition, account.SchemaName, categoryAttribute, includeHidden);
        }*/

        public SubsubcategoryModel GetSubsubcategoryByNames(string accountNameKey, string categoryName, string subcategoryName, string subsubcategoryName, bool includeHidden, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountNameKey);
            return CategorizationManager.GetSubsubcategoryByFullyQualifiedName(account, categoryName, subcategoryName, subsubcategoryName, includeHidden);
        }

        public SubsubcategoryModel GetSubsubcategoryByFullyQualifiedName(string accountNameKey, string fullyQualifiedName, bool includeHidden, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            string[] keys = fullyQualifiedName.Split('/');

            string categoryNameKey;
            string subcategoryNameKey;
            string subsubcategoryNameKey;

            try
            {
                categoryNameKey = keys[0];
                subcategoryNameKey = keys[1];
                subsubcategoryNameKey = keys[2];
            }
            catch
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountNameKey);
            return CategorizationManager.GetSubsubcategoryByFullyQualifiedName(account, categoryNameKey, subcategoryNameKey, subsubcategoryNameKey, includeHidden);
        }

        //public SubsubcategoryModel GetSubsubcategory(string accountId, string categoryAttribute, string subcategoryAttribute, string subsubcategoryAttribute, bool includeHidden)
        //{
        //var account = AccountManager.GetAccount(accountId);
        //return CategorizationManager.GetSubsubcategory(account, categoryAttribute, subcategoryAttribute, subsubcategoryAttribute, includeHidden);
        //}

        #endregion

        #endregion

        #region Subsubsubcategories

        #region Create

        public DataAccessResponseType CreateSubsubsubcategory(string accountId, string subsubcategoryId, string subsubsubcategoryName, bool isVisible, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results with error message:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Validate Plan Capabilities (Moved to SQL INSERT)
            /*
            //Verify that current category count is below maximum allowed by this plan
            if (CategorizationManager.GetSubsubsubcategoryCount(account) >= account.PaymentPlan.MaxSubsubsubcategories)
            {
                //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
                Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("subsubsubcategories", account.AccountID.ToString(), account.AccountName);

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for more than " + account.PaymentPlan.MaxSubsubsubcategories + " subsubsubcategories, please update your plan to add more." };
            }
            */
            #endregion

            var result = CategorizationManager.CreateSubsubsubcategory(account, subsubcategoryId, subsubsubcategoryName, isVisible);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubsubcategoryCreated,
                        "Subsubsubcategory '" + subsubsubcategoryName + "' created",
                        requesterName + " created the '" + subsubsubcategoryName + "' subsubsubcategory",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessages[0]);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;

        }

        #endregion

        #region Update

        public DataAccessResponseType RenameSubsubsubcategory(string accountId, string subsubsubcategoryId, string newSubsubsubcategoryName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetSubsubsubcategoryByID(account, subsubsubcategoryId).SubsubsubcategoryName;


            var result = CategorizationManager.RenameSubsubsubcategory(account, subsubsubcategoryId, newSubsubsubcategoryName);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubsubcategoryRenamed,
                        "Subsubsubcategory '" + objectName + "' renamed to '" + newSubsubsubcategoryName + "'",
                        requesterName + " renamed subsubsubcategory '" + objectName + "' to '" + newSubsubsubcategoryName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subsubsubcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateSubsubsubcategoryVisibleState(string accountId, string subsubsubcategoryId, bool visibleState, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion


            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetSubsubsubcategoryByID(account, subsubsubcategoryId).SubsubsubcategoryName;

            var result = CategorizationManager.UpdateSubsubsubcategoryVisibleState(account, subsubsubcategoryId, visibleState);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {
                    var description = "Subsubsubategory '" + objectName + "' made visible";
                    var details = " made Subsubsubategory '" + objectName + "' visible";

                    if (!visibleState)
                    {
                        description = "Subsubsubategory '" + objectName + "' hidden";
                        details = " made Subsubsubategory '" + objectName + "' hidden";
                    }


                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubsubcategoryVisibilityChanged,
                        description,
                        requesterName + details,
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subsubsubcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType ReorderSubsubsubcategories(string accountId, Dictionary<string, int> subsubsubcategoryOrderingDictionary, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Ensure ordering is allowed by account plan

            if (!account.PaymentPlan.AllowCustomOrdering)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for custom ordering of lists." };
            }

            #endregion

            var result = CategorizationManager.ReorderSubsubsubcategories(account, subsubsubcategoryOrderingDictionary);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubsubcategoriesReordered,
                        "Subsubsubcategories reordered",
                        requesterName + " reordered subsubsubcategories",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        null);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType ResetSubsubsubcategoryOrdering(string accountId, string subsubcategoryId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Ensure ordering is allowed by account plan

            if (!account.PaymentPlan.AllowCustomOrdering)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for custom ordering of lists." };
            }

            #endregion

            var result = CategorizationManager.ResetSubsubsubcategoryOrdering(account, subsubcategoryId);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubsubcategoryOrderingReset,
                        "Subsubsubcategory ordering reset",
                        requesterName + " reset subsubsubcategory ordering",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        null);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateSubsubsubcategoryDescription(string accountId, string subsubsubcategoryId, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            var objectName = CategorizationManager.GetSubsubsubcategoryByID(account, subsubsubcategoryId).SubsubsubcategoryName;

            var result = CategorizationManager.UpdateSubsubsubcategoryDescription(account, subsubsubcategoryId, newDescription);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    var description = "Subsubsubcategory '" + objectName + "' description updated";
                    var details = " updated description on subsubsubcategory '" + objectName + "'";

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubcategoryUpdated,
                        description,
                        requesterName + details,
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subsubsubcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }


        #endregion

        #region Delete

        public DataAccessResponseType DeleteSubsubsubcategory(string accountId, string subsubsubcategoryId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get account
            var account = AccountManager.GetAccount(accountId);

            //Get full subsubsubcategory
            var subsubsubcategory = CategorizationManager.GetSubsubsubcategoryByID(account, subsubsubcategoryId);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion


            #region Check for existance of child items before deletion

            bool productsExist = ProductManager.LocationPathContainsProducts(account, subsubsubcategory.FullyQualifiedName);

            if (productsExist)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete a subsubsubcategory that has products associated. Please delete or move all products first."
                };
            }

            #endregion

            //Get Name for logging (We do this first so that we can take advantage of Redis, and get state prior to changes.
            //var objectName = CategorizationManager.GetSubcategory(account, categoryId, subcategoryId, false).SubcategoryName;


            var result = CategorizationManager.DeleteSubsubsubcategory(account, subsubsubcategoryId);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_SubsubsubcategoryDeleted,
                        "Subsubsubcategory '" + subsubsubcategory.SubsubsubcategoryName + "' deleted",
                        requesterName + " deleted the '" + subsubsubcategory.SubsubsubcategoryName + "' subsubsubcategory",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        subsubsubcategoryId);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;

        }

        #endregion

        #region Get

        //public int GetSubsubsubcategoryCount(string accountId)
        //{
            //var account = AccountManager.GetAccount(accountId);
            //return CategorizationManager.GetSubsubsubcategoryCount(account);
        //}

        public SubsubsubcategoryModel GetSubsubsubcategoryByNames(string accountNameKey, string categoryName, string subcategoryName, string subsubcategoryName, string subsubsubcategoryName, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountNameKey);
            return CategorizationManager.GetSubsubsubcategoryByFullyQualifiedName(account, categoryName, subcategoryName, subsubcategoryName, subsubsubcategoryName);
        }

        public SubsubsubcategoryModel GetSubsubsubcategoryByFullyQualifiedName(string accountNameKey, string fullyQualifiedName, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            string[] keys = fullyQualifiedName.Split('/');

            string categoryNameKey;
            string subcategoryNameKey;
            string subsubcategoryNameKey;
            string subsubsubcategoryNameKey;

            try
            {
                categoryNameKey = keys[0];
                subcategoryNameKey = keys[1];
                subsubcategoryNameKey = keys[2];
                subsubsubcategoryNameKey = keys[3];
            }
            catch
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountNameKey);
            return CategorizationManager.GetSubsubsubcategoryByFullyQualifiedName(account, categoryNameKey, subcategoryNameKey, subsubcategoryNameKey, subsubsubcategoryNameKey);
        }

        //public SubsubsubcategoryModel GetSubsubsubcategoryByID(string accountId, string subsubsubcategoryId)
        //{
        //var account = AccountManager.GetAccount(accountId);
        //return CategorizationManager.GetSubsubsubcategoryByID(account, subsubsubcategoryId);
        //}

        /*
        public SubsubsubcategoryModel GetSubsubsubcategory(string accountId, string categoryAttribute, string subcategoryAttribute, string subsubcategoryAttribute, string subsubsubcategoryAttribute, bool includeHidden)
        {
            var account = AccountManager.GetAccount(accountId);
            return CategorizationManager.GetSubsubsubcategory(account, categoryAttribute, subcategoryAttribute, subsubcategoryAttribute, subsubsubcategoryAttribute, includeHidden);
        }*/

        #endregion

        #endregion

        #region Shared

        public int GetCategorizationCount(string accountId)
        {
            var account = AccountManager.GetAccount(accountId);
            return CategorizationManager.GetCategorizationCount(account);
        }

        #endregion
    }
}
