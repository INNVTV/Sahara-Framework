using Sahara.Core.Accounts.Commerce.Public;
using Sahara.Core.Logging.AccountLogs;
using Sahara.Core.Logging.AccountLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Platform.Requests.Models;
using WCF.WcfEndpoints.Contracts.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts;
using WCF.WcfEndpoints.Contracts.Application;
using Sahara.Core.Imaging.Models;
using System.ServiceModel;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Drawing;
//using Sahara.Core.Application.ApplicationImages;
//using Sahara.Core.Application.ApplicationImages.Models;
using Sahara.Core.Application.Categorization.Public;
using Sahara.Core.Accounts.Capacity.Public;
using Sahara.Core.Application.Images.Formats.Models;
using Sahara.Core.Application.Images.Formats;
using Sahara.Core.Application.Images.Records;

namespace WCF.WcfEndpoints.Service.Application
{
    
    public class ApplicationImageFormatsService : IApplicationImageFormatsService
    {
        #region Image Group Types  (Global)

        public List<ImageFormatGroupTypeModel> GetImageFormatGroupTypes()
        {
            return ImageFormatsManager.GetImageFormatGroupTypes();
        }

        #endregion

        #region Image Groups & Formats  (Account Specific)

        #region GET

        public List<ImageFormatGroupModel> GetImageFormats(string accountNameKey, string imageFormatGroupTypeNameKey)
        {
            return ImageFormatsManager.GetImageFormats(accountNameKey, imageFormatGroupTypeNameKey);
        }

        #endregion

        #region Manage

        #region Create

        public DataAccessResponseType CreateImageGroup(string accountNameKey, string imageGroupTypeNameKey, string imageGroupName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            

            var result = ImageFormatsManager.CreateImageGroup(account, imageGroupTypeNameKey, imageGroupName);

            if(result.isSuccess)
            {
                #region Invalidate Account Capacity Cache

                AccountCapacityManager.InvalidateAccountCapacitiesCache(account.AccountID.ToString());

                #endregion
            }

            return result;
        }


        public DataAccessResponseType CreateImageFormat(string accountNameKey, string imageGroupTypeNameKey, string imageGroupNameKey, string imageFormatName, int width, int height, bool listing, bool gallery, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            if(imageGroupNameKey == "default")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot append new formats to the default group" };
            }
            if (imageGroupNameKey == "main")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You cannot append new formats to the main group" };
            }

           
            var result = ImageFormatsManager.CreateImageFormat(account, imageGroupTypeNameKey, imageGroupNameKey, imageFormatName, width, height, listing, gallery);

            if(result.isSuccess)
            {
                #region Invalidate Account Capacity Cache

                AccountCapacityManager.InvalidateAccountCapacitiesCache(account.AccountID.ToString());

                #endregion
            }

            return result;
        }

        #endregion

        #region Edit/Update (Removed - Just delete and recreate, Too many important rules in place to allow edits.)

        //Too many important rules in place to allow edits.
        //Since you can only edit on formats that have NO image records attached (due to listing duplicats and search results, etc...)
        //Easier to just DELETE & RECREATE the format.

        /*

        public DataAccessResponseType UpdateImageFormatVisibleState(string accountNameKey, string imageFormatId, bool visibleState, string requesterId, RequesterType requesterType)
        {
            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountNameKey);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var result = ImageFormatsManager.UpdateImageFormatVisibleState(account, imageFormatId, visibleState);

            #region Log Account Activity
            /*
            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId,
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
            }* /

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateImageFormatGalleryState(string accountNameKey, string imageFormatId, bool isGallery, string requesterId, RequesterType requesterType)
        {
            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountNameKey);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var result = ImageFormatsManager.UpdateImageFormatGalleryState(account, imageFormatId, isGallery);

            #region Log Account Activity
            /*
            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId,
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
            }* /

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateImageFormatListingState(string accountNameKey, string imageFormatId, bool isListing, string requesterId, RequesterType requesterType)
        {
            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountNameKey);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var result = ImageFormatsManager.UpdateImageFormatListingState(account, imageFormatId, isListing);

            #region Log Account Activity
            /*
            if (result.isSuccess)
            {
                try
                {
                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId,
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
            }* /

            #endregion

            return result;
        }

        */

        #endregion

        #region Delete

        public DataAccessResponseType DeleteImageGroup(string accountNameKey, string imageGroupTypeNameKey, string imageGroupNameKey, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var result = ImageFormatsManager.DeleteImageGroup(account, imageGroupTypeNameKey, imageGroupNameKey);

            if (result.isSuccess)
            {
                #region Invalidate Account Capacity Cache

                AccountCapacityManager.InvalidateAccountCapacitiesCache(account.AccountID.ToString());

                #endregion
            }

            return result;
        }

        public DataAccessResponseType DeleteImageFormat(string accountNameKey, string imageGroupTypeNameKey, string imageGroupNameKey, string imageFormatNameKey, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

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

            #region Validate that no image records exist exist that use this format

            if(ImageRecordsManager.ImageRecordExistsForImageKey(account.AccountID.ToString(), account.StoragePartition, account.AccountNameKey, imageGroupTypeNameKey, imageGroupNameKey + "-" + imageFormatNameKey))
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot delete an image format that has any image records associated with it." };
            }

            #endregion

            var result = ImageFormatsManager.DeleteImageFormat(account, imageGroupTypeNameKey, imageGroupNameKey, imageFormatNameKey);

            if (result.isSuccess)
            {
                #region Invalidate Account Capacity Cache

                AccountCapacityManager.InvalidateAccountCapacitiesCache(account.AccountID.ToString());

                #endregion
            }

            return result;
        }

        #endregion

        #endregion

        #endregion
    }
}
