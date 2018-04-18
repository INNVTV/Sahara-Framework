using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Capacity.Public;
using Sahara.Core.Application.Properties;
using Sahara.Core.Application.Properties.Models;
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
    public class ApplicationPropertiesService : IApplicationPropertiesService
    {
      
        #region Create

        public DataAccessResponseType CreateProperty(string accountId, string propertyTypeNameKey, string propertyName, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Validate Plan Capabilities

            if(!account.PaymentPlan.AllowLocationData && propertyTypeNameKey == "location")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for location data." };
            }

            //Verify that current category count is below maximum allowed by this plan
            if (PropertiesManager.GetPropertyCount(account) >= account.PaymentPlan.MaxProperties)
            {
                //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
                Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("properties", account.AccountID.ToString(), account.AccountName);

                string propertyDescription = "properties";

                if(account.PaymentPlan.MaxProperties == 1)
                {
                    propertyDescription = "property";
                }

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for more than " + account.PaymentPlan.MaxProperties + " " + propertyDescription + ". Please update your plan to add more." };
            }

            #endregion

            var result = PropertiesManager.CreateProperty(account, propertyTypeNameKey, propertyName);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyCreated,
                        "Property '" + propertyName + "' created",
                        requesterName + " created '" + propertyName + "' property",
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

        public DataAccessResponseType CreatePropertyValue(string accountId, string propertyNameKey, string propertyValueName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            #region Validate Plan Capabilities

            //Verify that current category count is below maximum allowed by this plan
            if (property.Values.Count >= account.PaymentPlan.MaxValuesPerProperty)
            {
                //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
                Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("propertyValues", account.AccountID.ToString(), account.AccountName);

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for more than " + account.PaymentPlan.MaxValuesPerProperty + " values per property, please update your plan to add more." };
            }

            #endregion

            var result = PropertiesManager.CreatePropertyValue(account, property, propertyValueName);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyValueCreated,
                        "Property value '" + propertyValueName + "' created on property '" + property.PropertyName + "'",
                        requesterName + " created '" + propertyValueName + "' property value on property '" + property.PropertyName + "'",
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

        #region Swatch Property Type

        //Step 1:
        // Future performance update: have client upload image to intermediary storage, submit location with imag eid for WCF processing (similar to other imageing solutions)
        //Used to upload swatch image and display it prior to creating a label and submitting
        public string UploadPropertySwatchImage(string accountId, byte[] imageByteArray, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                //return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
                return null;
            }

            #endregion

            string imageUrl = "";

            var result = PropertiesManager.UploadSwatchImage(accountId, account.StoragePartition, imageByteArray, out imageUrl);

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            var cdnEndpoint = Sahara.Core.Settings.Azure.Storage.GetStoragePartition(account.StoragePartition).CDN;

            //Returned WITH CDN url so it can be viewiwed:
            return "https://" + cdnEndpoint + "/" + imageUrl;
        }

        //Step 2:
        public DataAccessResponseType CreateSwatchValue(string accountId, string propertyNameKey, string swatchImage, string swatchLabel, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

            //STRIP OUT CDN DETAILS FOR GREATURE FUTURE FLEXIBILITY:
            var cdnEndpoint = Sahara.Core.Settings.Azure.Storage.GetStoragePartition(account.StoragePartition).CDN;
            var stripOut = "https://" + cdnEndpoint + "/";
            swatchImage = swatchImage.Replace(stripOut, "");

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

            #region Validate Plan Capabilities

            //Verify that current category count is below maximum allowed by this plan
            if (property.Swatches.Count >= account.PaymentPlan.MaxValuesPerProperty)
            {
                //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
                Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("propertyValues", account.AccountID.ToString(), account.AccountName);

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for more than " + account.PaymentPlan.MaxValuesPerProperty + " values per property, please update your plan to add more." };
            }

            #endregion

            var result = PropertiesManager.CreatePropertySwatch(account, property, swatchImage, swatchLabel);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyValueCreated,
                        "Property swatch '" + swatchLabel + "' created on property '" + property.PropertyName + "'",
                        requesterName + " created '" + swatchLabel + "' swatch on property '" + property.PropertyName + "'",
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

        #endregion

        #region Get

        public List<PropertyTypeModel> GetPropertyTypes(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Global
            return PropertiesManager.GetPropertyTypes();
        }

        public PropertyModel GetProperty(string accountNameKey, string propertyNameKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Account Specific
            var account = AccountManager.GetAccount(accountNameKey);
            return PropertiesManager.GetProperty(account, propertyNameKey);
        }

        public List<PropertyModel> GetProperties(string accountNameKey, PropertyListType listType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Account Specific
            var account = AccountManager.GetAccount(accountNameKey);
            return PropertiesManager.GetProperties(account, listType);
        }

        public int GetPropertyCount(string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return 0;
            }

            //Account Specific
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);
            return PropertiesManager.GetPropertyCount(account);
        }

        #endregion

        #region Update

        public DataAccessResponseType UpdatePropertyListingState(string accountId, string propertyNameKey, bool listingState, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            var result = PropertiesManager.UpdatePropertyListingState(account, property, listingState);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyVisibilityChanged,
                        "Property '" + property.PropertyName + "' listing state  updates.",
                        requesterName + " updated listing state for '" + property.PropertyName + "' property",
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

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdatePropertyDetailsState(string accountId, string propertyNameKey, bool detailsState, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            var result = PropertiesManager.UpdatePropertyDetailsState(account, property, detailsState);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyVisibilityChanged,
                        "Property '" + property.PropertyName + "' details state updates.",
                        requesterName + " updated details state for '" + property.PropertyName + "' property",
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

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }


        public DataAccessResponseType UpdatePropertyFacetInterval(string accountId, string propertyNameKey, int newFacetInterval, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            var result = PropertiesManager.UpdatePropertyFacetInterval(account, property, newFacetInterval);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyDeleted,
                        "Property '" + property.PropertyName + "' facet interval updates.",
                        requesterName + " updated facet interval for '" + property.PropertyName + "' property",
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

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdatePropertyFacetableState(string accountId, string propertyNameKey, bool isFacetable, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            var result = PropertiesManager.UpdatePropertyFacetableState(account, property, isFacetable);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyDeleted,
                        "Property '" + property.PropertyName + "' facetable state updated.",
                        requesterName + " updated facetable state on '" + property.PropertyName + "' property",
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

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdatePropertySortableState(string accountId, string propertyNameKey, bool isSortable, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            var result = PropertiesManager.UpdatePropertySortableState(account, property, isSortable);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyDeleted,
                        "Property '" + property.PropertyName + "' sortable state updated.",
                        requesterName + " updated sortable state on '" + property.PropertyName + "' property",
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

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdatePropertyAppendableState(string accountId, string propertyNameKey, bool isAppendable, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            var result = PropertiesManager.UpdatePropertyAppendableState(account, property, isAppendable);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyDeleted,
                        "Property '" + property.PropertyName + "' appendable state updated.",
                        requesterName + " updated appendable state on '" + property.PropertyName + "' property",
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

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        #region Removed

        /*
        public DataAccessResponseType UpdatePropertyHighlightedState(string accountId, string propertyNameKey, bool isHighlighted, string requesterId, RequesterType requesterType)
        {
            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            var result = PropertiesManager.UpdatePropertyHighlightedState(account, property, isHighlighted);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyDeleted,
                        "Property '" + property.PropertyName + "' highlight state updated.",
                        requesterName + " updated highlight state on '" + property.PropertyName + "' property",
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

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }
        */
        #endregion


        public DataAccessResponseType UpdatePropertySymbol(string accountId, string propertyNameKey, string symbol, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            var result = PropertiesManager.UpdatePropertySymbol(account, property, symbol);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyDeleted,
                        "Property '" + property.PropertyName + "' symbol updated.",
                        requesterName + " updated symbol on '" + property.PropertyName + "' property",
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

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdatePropertySymbolPlacement(string accountId, string propertyNameKey, string symbolPlacement, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            var result = PropertiesManager.UpdatePropertySymbolPlacement(account, property, symbolPlacement);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyDeleted,
                        "Property '" + property.PropertyName + "' symbol placement updated.",
                        requesterName + " updated symbol placement on '" + property.PropertyName + "' property",
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

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }


        #endregion

        #region Featured

        public DataAccessResponseType UpdateFeaturedProperties(string accountId, Dictionary<string, int> featuredOrderingDictionary, string requesterId, RequesterType requesterType, string sharedClientKey)
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

            var result = PropertiesManager.UpdateFeaturedProperties(account, featuredOrderingDictionary);

            #region Log Account Activity
            /*
            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId,account.StoragePartition,
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
            }*/

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;    
        }


        public DataAccessResponseType ResetFeaturedProperties(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey)
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


            var result = PropertiesManager.ResetFeaturedProperties(account);

            #region Log Account Activity

            /*
            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId,account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyCreated,
                        "Featured properties reset",
                        requesterName + " reset featured properties",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        null);
                }
                catch { }
            }*/

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }



        #endregion

        #region Delete

        public DataAccessResponseType DeleteProperty(string accountId, string propertyNameKey, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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


            // REVISIT ONCE SEARCH RE-INDEXING/REBUILDING CAN OCCUR
            var result = new DataAccessResponseType(); // PropertiesManager.DeleteProperty(account, property);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyDeleted,
                        "Property '" + property.PropertyName + "' deleted.",
                        requesterName + " deleted '" + property.PropertyName + "' property",
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

        public DataAccessResponseType DeletePropertyValue(string accountId, string propertyNameKey, string propertyValueNameKey, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var result = new DataAccessResponseType();

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            PropertyValueModel propertyValue = null;

            //Extract propertyValue
            foreach (PropertyValueModel value in property.Values)
            {
                if (value.PropertyValueNameKey == propertyValueNameKey)
                {
                    propertyValue = value;
                }
            }

            if(propertyValue != null)
            {
                result = PropertiesManager.DeletePropertyValue(account, property, propertyValue);
            }
            else
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Property value does not exist" };
            }

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyValueDeleted,
                        "Property value ''" + propertyValue.PropertyValueName + "' on property " + property.PropertyName + "' deleted.",
                        requesterName + " deleted property value ''" + propertyValue.PropertyValueName + "' on property " + property.PropertyName + "'.",
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

        public DataAccessResponseType DeletePropertySwatch(string accountId, string propertyNameKey, string propertySwatchNameKey, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var result = new DataAccessResponseType();

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            //Get PROPERTY
            var property = PropertiesManager.GetProperty(account, propertyNameKey);

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

            PropertySwatchModel propertySwatch = null;

            //Extract propertyValue
            foreach (PropertySwatchModel swatch in property.Swatches)
            {
                if (swatch.PropertySwatchNameKey == propertySwatchNameKey)
                {
                    propertySwatch = swatch;
                }
            }

            if (propertySwatch != null)
            {
                result = PropertiesManager.DeletePropertySwatch(account, property, propertySwatch);
            }
            else
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Swatch does not exist" };
            }

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyValueDeleted,
                        "Swatch ''" + propertySwatch.PropertySwatchLabel + "' on property " + property.PropertyName + "' deleted.",
                        requesterName + " deleted swatch ''" + propertySwatch.PropertySwatchLabel + "' on property " + property.PropertyName + "'.",
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
