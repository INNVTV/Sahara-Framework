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
using Sahara.Core.Application.Images.Processing.Public;
using Sahara.Core.Application.Images.Records;
using Sahara.Core.Application.Images.Formats;
using Sahara.Core.Application.Images.Formats.Models;

namespace WCF.WcfEndpoints.Service.Application
{
    
    public class ApplicationImagesService : IApplicationImagesService
    {

        /// <summary>
        /// Before "adding" an image to an object the client must upload a source file to a dated directory within intermediary storage.
        /// Pass in the source file info along with any cropping or enhancement instructions and you will get back the final image id after processing is complete/
        /// Intermediary directory MUST named by todays date: "DD-MM-YYYY", this directory will be garbage collected by the Custodian at a set interval
        /// </summary>
        public DataAccessResponseType ProcessImage(string accountId, ImageProcessingManifestModel imageManifest, ImageCropCoordinates imageCropCoordinates, string requesterId, RequesterType requesterType, ImageEnhancementInstructions imageEnhancementInstructions, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);

            #region Adjust negative crop coordinates for top/left pixel

            //if any top/left values fall below 0 we adjust to 0
            if(imageCropCoordinates.Top < 0)
            {
                imageCropCoordinates.Top = 0;
            }
            if (imageCropCoordinates.Left < 0)
            {
                imageCropCoordinates.Left = 0;
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

            #region Validate Plan Capabilities

            // If enhancement instructions are sent, verify that current plan allows for it
            if (imageEnhancementInstructions != null && account.PaymentPlan.AllowImageEnhancements == false)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for image enhancements, please submit your job without enhancement instructions." };
            }

            //Verify that current image count is below maximum allowed by this plan
            //if (ApplicationImagesManager.GetApplicationImageCount(account) >= account.PaymentPlan.MaxProducts)
            //{
            //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
            //Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("images", account.AccountID.ToString(), account.AccountName);

            //return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for more than " + account.PaymentPlan.MaxProducts + " images, please update your plan." };
            //}

            #endregion

            var result = ApplicationImageProcessingManager.ProcessAndRecordApplicationImage(account, imageManifest, imageCropCoordinates, imageEnhancementInstructions);

            #region Log Account Activity


            if (result.isSuccess)
            {
                /*try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId,
                        CategoryType.ApplicationImage,
                        ActivityType.ApplicationImage_Created,
                        "Application image created",
                        requesterName + " created an application image",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessage);
                }
                catch { }*/
            }

            #endregion

            #region Invalidate Account Capacity Cache

            //AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;

        }


        #region Image Management

        #region Update 

        public DataAccessResponseType UpdateImageRecordTitle(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, string newTitle, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);

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

            #region Get image format to determine if this is a listing

            ImageFormatGroupModel imageGroup;
            var imageFormat = ImageFormatsManager.GetImageFormat(account.AccountNameKey, objectType, groupNameKey, formatNameKey, out imageGroup);

            if (imageFormat == null)
            {
                return null;
            }

            #endregion



            var result = ImageRecordsManager.UpdateImageRecordTitleForObject(accountId, account.StoragePartition, objectType, objectId, groupNameKey, formatNameKey, newTitle, imageFormat.Listing);

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateImageRecordDescription(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);

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

            #region Get image format to determine if this is a listing

            ImageFormatGroupModel imageGroup;
            var imageFormat = ImageFormatsManager.GetImageFormat(account.AccountNameKey, objectType, groupNameKey, formatNameKey, out imageGroup);

            if (imageFormat == null)
            {
                return null;
            }

            #endregion

            var result = ImageRecordsManager.UpdateImageRecordDescriptionForObject(accountId, account.StoragePartition, objectType, objectId, groupNameKey, formatNameKey, newDescription, imageFormat.Listing);

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateImageRecordGalleryTitle(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex, string newTitle, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);

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

            var result = ImageRecordsManager.UpdateImageGalleryRecordTitleForObject(accountId, account.StoragePartition, objectType, objectId, groupNameKey, formatNameKey, imageIndex, newTitle);

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateImageRecordGalleryDescription(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);

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

            var result = ImageRecordsManager.UpdateImageGalleryRecordDescriptionForObject(accountId, account.StoragePartition, objectType, objectId, groupNameKey, formatNameKey, imageIndex, newDescription);

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public DataAccessResponseType ReorderImageRecordGallery(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, List<int> imageIndexOrder, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);

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

            var result = ImageRecordsManager.ReorderGalleryImages(accountId, account.StoragePartition, objectType, objectId, groupNameKey, formatNameKey, imageIndexOrder);

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }


        #endregion

        #region Delete

        public DataAccessResponseType DeleteImageRecord(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);

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

            var result = ImageRecordsManager.DeleteImageRecordForObject(account, objectType, objectId, groupNameKey, formatNameKey);

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;

        }


        public DataAccessResponseType DeleteGalleryImage(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);

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

            var result = ImageRecordsManager.DeleteGalleryImage(account, objectType, objectId, groupNameKey, formatNameKey, imageIndex);

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        #endregion

        #endregion
    }
}
