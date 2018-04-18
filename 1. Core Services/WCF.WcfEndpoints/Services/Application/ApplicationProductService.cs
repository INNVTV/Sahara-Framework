using WCF.WcfEndpoints.Contracts.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Application.DocumentModels.Product;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests.Models;
using Sahara.Core.Application.Products.Models;
using Sahara.Core.Application.Products.Public;
using Sahara.Core.Accounts;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Logging.AccountLogs;
using Sahara.Core.Logging.AccountLogs.Types;
using Sahara.Core.Accounts.Capacity.Public;
using Sahara.Core.Application.Properties;
using Sahara.Core.Application.Tags.Public;
using Sahara.Core.Application.Categorization.Public;
using Sahara.Core.Imaging.Models;
using Sahara.Core.Common.Types;

namespace WCF.WcfEndpoints.Service.Application
{
    public class ApplicationProductService : IApplicationProductService
    {

        #region Create

        public DataAccessResponseType CreateProduct(string accountId, string locationPath, string productName, bool isVisible, string requesterId, RequesterType requesterType, string sharedClientKey)
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

            #region Determine location categorization type (subcat, subsubcat or subsubsubcat) and make sure it is a valid location

            string[] locationPathKeys = locationPath.Split('/');

            switch (locationPathKeys.Count())
            {
                case 1:
                    {

                        var category = CategorizationManager.GetCategoryByName(account, locationPathKeys[0], true, false);
                        if (category == null)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This is not a valid category." };
                        }
                        if(category.Subcategories.Count > 0)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot add products to a category that has subcategories." };
                        }

                        break;
                    }
                case 2:
                    {

                        var subcategory = CategorizationManager.GetSubcategoryByFullyQualifiedName(account, locationPathKeys[0], locationPathKeys[1], true, false);
                        if (subcategory == null)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This is not a valid subcategory." };
                        }
                        if (subcategory.Subsubcategories.Count > 0)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot add products to a subcategory that has subsubcategories." };
                        }

                        break;
                    }
                case 3:
                    {
                        var subsubcategory = CategorizationManager.GetSubsubcategoryByFullyQualifiedName(account, locationPathKeys[0], locationPathKeys[1], locationPathKeys[2], true, false);
                        if (subsubcategory == null)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This is not a valid subsubcategory." };
                        }
                        if (subsubcategory.Subsubsubcategories.Count > 0)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot add products to a subsubcategory that has subsubsubcategories." };
                        }

                        break;
                    }
                case 4:
                    {
                        var subsubsubcategory = CategorizationManager.GetSubsubsubcategoryByFullyQualifiedName(account, locationPathKeys[0], locationPathKeys[1], locationPathKeys[2], locationPathKeys[3], false);
                        if (subsubsubcategory == null)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This is not a valid subsubsubcategory." };
                        }

                        break;
                    }

                default:
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Could not parse location path." };
            }

            #endregion

            #region Validate Plan Capabilites for entire account

            var totalProductsInAccount = ProductManager.GetProductCount(account);

            if(totalProductsInAccount >= account.PaymentPlan.MaxProducts)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of " + account.PaymentPlan.MaxProducts.ToString("#,##0") + " products allowed for your account." };    
            }

            #endregion

            #region Validate Plan Capabilities based on max products per categorization

            //Get current product count in the LocationPath product will be moved/added to:
            var productCount = ProductManager.GetProductCount(account, locationPath);

            switch (locationPathKeys.Count())
            {
                case 1:
                    {

                        //Verify that current product count is below maximum allowed per set on this categorization type
                        if (productCount >= account.PaymentPlan.MaxProductsPerSet)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of " + account.PaymentPlan.MaxProductsPerSet + " products allowed for this category." };
                        }

                        break;
                    }
                case 2:
                    {

                        //Verify that current product count is below maximum allowed per set on this categorization type
                        if (productCount >= account.PaymentPlan.MaxProductsPerSet)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of " + account.PaymentPlan.MaxProductsPerSet + " products allowed for this subcategory." };
                        }

                        break;
                    }
                case 3:
                    {
                        //Verify that current product count is below maximum allowed per set on this categorization type
                        if (productCount >= account.PaymentPlan.MaxProductsPerSet)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of " + account.PaymentPlan.MaxProductsPerSet + " products allowed for this subsubcategory." };
                        }

                        break;
                    }
                case 4:
                    {
                        //Verify that current product count is below maximum allowed per set on this categorization type
                        if (productCount >= account.PaymentPlan.MaxProductsPerSet)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of " + account.PaymentPlan.MaxProductsPerSet + " products allowed for this subsubsubcategory." };
                        }

                        break;
                    }

                default:
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Could not determine plan allownaces for products. Please try again." };
            }

            //Get current product count in the LocationPath product will be moved/added to:
            //var productCount = ProductManager.GetProductCount(account, locationPath);

            //Verify that current product count is below maximum allowed per set on this categorization type
            //if (productCount >= account.PaymentPlan.MaxProductsPerSet)
            //{
                //return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of products allowed (" + account.PaymentPlan.MaxProductsPerSet + ") for this categorization." };
            //}
            
            #endregion

            //Approved to creat product!
            var result = ProductManager.CreateProduct(account, locationPath, productName, isVisible);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_ProductCreated,
                        "Product '" + productName + "' created",
                        requesterName + " created '" + productName + "' category",
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

            return result;
        }

        #endregion

        #region Not used, but needed to expose 'ProductDocumentModel' type to clients


        public ProductDocumentModel GetProduct(string productId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            throw new NotImplementedException();
        }

        #endregion

        #region Get (DEPRICATED)


        /*

        public int GetProductCount(string accountId)
        {
            var account = AccountManager.GetAccount(accountId);

            return ProductManager.GetProductCount(account);
        }

        --- Use direct DocumentDB calls for this---

        public ProductResults GetProducts(string accountNameKey)
        {
            throw new NotImplementedException();
        }

        public ProductDocumentModel GetProduct(string productId)
        {
            throw new NotImplementedException();
        }
        */
        #endregion

        #region UPDATES

        public DataAccessResponseType UpdateProductVisibleState(string accountId, string fullyQualifiedName, string productName, bool visibleState, string requesterId, RequesterType requesterType, string sharedClientKey)
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


            var result = ProductManager.UpdateProductVisibleState(account, fullyQualifiedName, visibleState);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_ProductVisibilityChanged,
                        "Visibility changed for product '" + productName + "'",
                        requesterName + "Changed visibility for product '" + productName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        "");
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion
            return result;
        }

        public DataAccessResponseType RenameProduct(string accountId, string fullyQualifiedName, string oldName, string newName, string requesterId, RequesterType requesterType, string sharedClientKey)
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


            var result = ProductManager.RenameProduct(account, fullyQualifiedName, newName);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_ProductRenamed,
                        "Product '" + oldName + "' has been renamed to '" + newName + "'",
                        requesterName + "Changed name of product '" + oldName + "' to '" + newName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        "");
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType ReorderProducts(string accountId, Dictionary<string, int> productOrderingDictionary, string locationPath, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //===========================================================
            //locationPath is used to query DocumentDB
            //Dictionary is used to loop through and update prducts, as we save the documents back
            //Can we use a trigger or batch update?????

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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for custom ordering of products." };
            }

            #endregion

            var result = ProductManager.ReorderProducts(account, productOrderingDictionary, locationPath);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_ProductsReordered,
                        "Products reordered",
                        requesterName + " reordered products",
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

        public DataAccessResponseType ResetProductOrdering(string accountId, string locationPath, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //==========================================================
            //locationPath is used to query DocumentDB
            //We to loop through and update prducts to have OrderID=0, as we save the documents back
            //Can we use a trigger or batch update?????

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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for custom ordering of products." };
            }

            #endregion

            var result = ProductManager.ResetProductOrdering(account, locationPath);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_ProductOrderingReset,
                        "Product ordering reset",
                        requesterName + " reset product ordering",
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

        #endregion

        #region UPDATE PROPERTIES

        public DataAccessResponseType UpdateProductProperty(string accountId, string fullyQualifiedName, string productName, string propertyNameKey, string propertyValue, ProductPropertyUpdateType updateType, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get Property
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

            #region Assert property exists

            if(property == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not a valid property." };
            }

            #endregion

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

            #region Validate UpdateType is Valid

            if(updateType == ProductPropertyUpdateType.Append && property.Appendable != true)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This property cannot be appended to." };
            }

            #endregion

            var result = ProductManager.UpdateProductProperty(account, fullyQualifiedName, property, propertyValue, updateType);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyUpdatedOnProduct,
                        "Property '" + property.PropertyName + "' updated to '" + propertyValue + "' for product '" + productName + "'",
                        requesterName + "Updated property '" + property.PropertyName + "' to '" + propertyValue + "' for product '" + productName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        "");
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateProductLocationProperty(string accountId, string fullyQualifiedName, string productName, string propertyNameKey, PropertyLocationValue propertyLocationValue, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get Property
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

            #region Assert property exists

            if (property == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not a valid property." };
            }

            #endregion

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

            #region Validate UpdateType is Valid

            if (property.PropertyTypeNameKey != "location")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This property is not a valid location type." };
            }

            #endregion

            var result = ProductManager.UpdateProductLocationProperty(account, fullyQualifiedName, property, propertyLocationValue);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyUpdatedOnProduct,
                        "Location value '" + property.PropertyName + "' updated for product '" + productName + "'",
                        requesterName + "Updated location value of '" + property.PropertyName + "' for product '" + productName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        "");
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }


        public DataAccessResponseType RemoveProductPropertyCollectionItem(string accountId, string fullyQualifiedName, string productName, string propertyNameKey, int collectionItemIndex, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get Property
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

            #region Assert property exists

            if (property == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not a valid property." };
            }

            #endregion

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


            var result = ProductManager.RemoveProductPropertyCollectionItem(account, fullyQualifiedName, property, collectionItemIndex);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyUpdatedOnProduct,
                        "Property '" + property.PropertyName + "' index '" + collectionItemIndex + "' removed for product '" + productName + "'",
                        requesterName + " removed index '" + collectionItemIndex + "' from '" + property.PropertyName + "' for product '" + productName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        "");
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }


        public DataAccessResponseType ClearProductProperty(string accountId, string fullyQualifiedName, string productName, string propertyNameKey, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get Property
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

            #region Assert property exists

            if (property == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not a valid property." };
            }

            #endregion

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

            var result = ProductManager.ClearProductProperty(account, fullyQualifiedName, property);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyRemovedOnProduct,
                        "Property '" + property.PropertyName + "' removed on product '" + productName + "'",
                        requesterName + "Removed property '" + property.PropertyName + "' on product '" + productName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        "");
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

        #region UPDATE TAGS

        public DataAccessResponseType AddProductTag(string accountId, string fullyQualifiedName, string productName, string tagName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Assert tag exists

            var tags = TagManager.GetTags(account.AccountNameKey);

            if(tags != null)
            {
                if (!tags.Contains(tagName))
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not a valid tag." };
                }
            }
            else
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not a valid tag." };
            }


            #endregion

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

            var result = ProductManager.AddProductTag(account, fullyQualifiedName, tagName);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_TagAddedToProduct,
                        "Tag '" + tagName + "' added to product '" + productName + "'",
                        requesterName + "Added tag '" + tagName + "' to product '" + productName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        "");
                }
                catch { }
            }

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType RemoveProductTag(string accountId, string fullyQualifiedName, string productName, string tagName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Assert tag exists

            var tags = TagManager.GetTags(account.AccountNameKey);

            if (tags != null)
            {
                if (!tags.Contains(tagName))
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not a valid tag." };
                }
            }
            else
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not a valid tag." };
            }


            #endregion

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

            var result = ProductManager.RemoveProductTag(account, fullyQualifiedName, tagName);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_TagRemovedFromProduct,
                        "Tag '" + tagName + "' removed on product '" + productName + "'",
                        requesterName + "Removed tag '" + tagName + "' on product '" + productName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        "");
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

        #region UPDATE IMAGES

        /* MOVED TO COMMON IMAGING METHOD
        public DataAccessResponseType AddProductImage(string accountId, string productId, ImageFormatInstructions imageFormatInstructions, ImageSourceFile imageSourceFile, ImageCropCoordinates imageCropCoordinates, ImageEnhancementInstructions imageEnhancementInstructions, string requesterId, RequesterType requesterType)
        {
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

            var result = ProductManager.AddProductImage(account, productId, imageFormatInstructions, imageSourceFile, imageCropCoordinates, imageEnhancementInstructions);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId,
                        CategoryType.Inventory,
                        ActivityType.Inventory_ProductImageAdded,
                        "Image '" + imageSourceFile.ImageID + "' added to product '" + productId + "'",
                        requesterName + "Added image '" + productId + "' to product '" + productId + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        "");
                }
                catch { }
            }

            #endregion

            return result;
        }
        */

        //Remove product Image

        #endregion

        #region MOVE

        public DataAccessResponseType MoveProduct(string accountId, string productId, string newLocationPath, string requesterId, RequesterType requesterType, string sharedClientKey)
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

            #region Determine NEW location categorization type (subcat, subsubcat or subsubsubcat) and make sure it is a valid location

            string[] newLocationPathKeys = newLocationPath.Split('/');

            switch(newLocationPathKeys.Count())
            {
                case 1:
                    {

                        var category = CategorizationManager.GetCategoryByName(account, newLocationPathKeys[0], true, false);
                        if (category == null)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This is not a valid category." };
                        }
                        if (category.Subcategories.Count > 0)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot move products to a category that has subcategories." };
                        }
                        break;
                    }
                case 2:
                    {

                        var subcategory = CategorizationManager.GetSubcategoryByFullyQualifiedName(account, newLocationPathKeys[0], newLocationPathKeys[1], true, false);
                        if(subcategory == null)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This is not a valid subcategory." };
                        }
                        if (subcategory.Subsubcategories.Count > 0)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot move products to a subcategory that has subsubcategories." };
                        }
                        break;
                    }
                case 3:
                    {
                        var subsubcategory = CategorizationManager.GetSubsubcategoryByFullyQualifiedName(account, newLocationPathKeys[0], newLocationPathKeys[1], newLocationPathKeys[2], true, false);
                        if (subsubcategory == null)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This is not a valid subsubcategory." };
                        }
                        if (subsubcategory.Subsubsubcategories.Count > 0)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot move products to a subsubcategory that has subsubsubcategories." };
                        }

                        break;
                    }
                case 4:
                    {
                        var subsubsubcategory = CategorizationManager.GetSubsubsubcategoryByFullyQualifiedName(account, newLocationPathKeys[0], newLocationPathKeys[1], newLocationPathKeys[2], newLocationPathKeys[3], false);
                        if (subsubsubcategory == null)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This is not a valid subsubsubcategory." };
                        }

                        break;
                    }

                default:
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Could not parse new locatio path." };
            }

            #endregion

            #region Validate Plan Capabilities based on categorization type

            //Get current product count in the LocationPath product will be moved/added to:
            var productCount = ProductManager.GetProductCount(account, newLocationPath);

            switch (newLocationPathKeys.Count())
            {
                case 1:
                    {

                        //Verify that current product count is below maximum allowed per set on this categorization type
                        if (productCount >= account.PaymentPlan.MaxProductsPerSet)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of " + account.PaymentPlan.MaxProductsPerSet + " products allowed for the category you are moving this product to." };
                        }

                        break;
                    }
                case 2:
                    {

                        //Verify that current product count is below maximum allowed per set on this categorization type
                        if (productCount >= account.PaymentPlan.MaxProductsPerSet)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of " + account.PaymentPlan.MaxProductsPerSet + " products allowed for the subcategory you are moving this product to." };
                        }

                        break;
                    }
                case 3:
                    {
                        //Verify that current product count is below maximum allowed per set on this categorization type
                        if (productCount >= account.PaymentPlan.MaxProductsPerSet)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of " + account.PaymentPlan.MaxProductsPerSet + " products allowed for the subsubcategory you are moving this product to." };
                        }

                        break;
                    }
                case 4:
                    {
                        //Verify that current product count is below maximum allowed per set on this categorization type
                        if (productCount >= account.PaymentPlan.MaxProductsPerSet)
                        {
                            return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of " + account.PaymentPlan.MaxProductsPerSet + " products allowed for the subsubsubcategory you are moving this product to." };
                        }

                        break;
                    }

                default:
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Could not determine plan allownaces for products. Please try again." };
            }

            //Get current product count in the LocationPath product will be moved/added to:
            //var productCount = ProductManager.GetProductCount(account, locationPath);

            //Verify that current product count is below maximum allowed per set on this categorization type
            //if (productCount >= account.PaymentPlan.MaxProductsPerSet)
            //{
            //return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of products allowed (" + account.PaymentPlan.MaxProductsPerSet + ") for this categorization." };
            //}

            #endregion

            #region Validate Plan Capabilities (Depricated)

            //Get current product count in the LocationPath product will be moved/added to:
            //var productCount = ProductManager.GetProductCount(account, newLocationPath);


            //Verify that current product count is below maximum allowed per set
            //if (productCount >= account.PaymentPlan.MaxProductsPerSet)
            //{
            //return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the maximum amount of products allowed (" + account.PaymentPlan.MaxProductsPerSet + ") for the categorization you are attemtping to move to." };
            // }

            #endregion

            //Move is approved!
            var result = ProductManager.MoveProduct(account, productId, newLocationPath);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_ProductCreated,
                        "Product '" + result.SuccessMessage + "' moved to '" + newLocationPath + "'",
                        requesterName + " moved product '" + result.SuccessMessage + "' to '" + newLocationPath + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessage);
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

        #region DELETE


        public DataAccessResponseType DeleteProduct(string accountId, string productId, string requesterId, RequesterType requesterType, string sharedClientKey)
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


            //Move is approved!
            var result = ProductManager.DeleteProduct(account, productId);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_ProductCreated,
                        "Product '" + result.SuccessMessage + "' deleted.",
                        requesterName + " deleted the product '" + result.SuccessMessage + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessage);
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
    }
}
