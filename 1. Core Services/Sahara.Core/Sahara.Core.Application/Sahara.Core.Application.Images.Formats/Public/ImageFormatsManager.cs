using Newtonsoft.Json;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Images.Formats.Internal;
using Sahara.Core.Application.Images.Formats.Models;
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

namespace Sahara.Core.Application.Images.Formats
{
    public static class ImageFormatsManager
    {
        #region Image Group Types (Global)

        public static List<ImageFormatGroupTypeModel> GetImageFormatGroupTypes()
        {
            
            List<ImageFormatGroupTypeModel> imageGroupTypes = null;

            string cacheKey = GlobalHash.Key;
            string cacheField = GlobalHash.Fields.ImageGroupTypes;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            #region Get from cache

            try
            {
                var redisValue = cache.HashGet(cacheKey, cacheField);
                if (redisValue.HasValue)
                {
                    imageGroupTypes = JsonConvert.DeserializeObject<List<ImageFormatGroupTypeModel>>(redisValue);
                }
            }
            catch
            {

            }


            if (imageGroupTypes == null)
            {
                #region Get categories from SQL

                imageGroupTypes = Sql.Statements.SelectStatements.SelectImageGroupTypeList();

                #endregion

                #region Store count into cache

                try
                {
                    cache.HashSet(cacheKey, cacheField, JsonConvert.SerializeObject(imageGroupTypes), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }

            return imageGroupTypes;

            #endregion

        }

        #endregion

        #region Image Groups & Formats (Account Specific)

        #region Counts

        
        public static int GetCustomImageGroupCount(string accountNameKey)
        {

            var count = 0;

            var imageFormatGroupTypes = GetImageFormatGroupTypes();

            foreach(var imageFormatGroupType in imageFormatGroupTypes)
            {
                var imageGroups = GetImageFormats(accountNameKey, imageFormatGroupType.ImageFormatGroupTypeNameKey);

                foreach(var imageGroup in imageGroups)
                {
                    if(imageGroup.AllowDeletion)
                    {
                        count++;
                    }
                }

                //count = imageGroups.Count - 7; //<--7 is the default provisioned amount, must be updated manually
            }

            return count;
        }

        public static int GetCustomImageFormatCount(string accountNameKey)
        {
            var count = 0;

            var imageFormatGroupTypes = GetImageFormatGroupTypes();

            foreach (var imageFormatGroupType in imageFormatGroupTypes)
            {
                var imageGroups = GetImageFormats(accountNameKey, imageFormatGroupType.ImageFormatGroupTypeNameKey);

                foreach(var imageGroup in imageGroups)
                {
                    foreach (var imageFormat in imageGroup.ImageFormats)
                    {
                        if (imageFormat.AllowDeletion)
                        {
                            count++;
                        }
                    }
                }

            }

            return count;
        }

        public static int GetCustomImageGalleryCount(string accountNameKey)
        {
            var count = 0;

            var imageFormatGroupTypes = GetImageFormatGroupTypes();

            foreach (var imageFormatGroupType in imageFormatGroupTypes)
            {
                var imageGroups = GetImageFormats(accountNameKey, imageFormatGroupType.ImageFormatGroupTypeNameKey);

                foreach (var imageGroup in imageGroups)
                {
                    foreach (var imageFormat in imageGroup.ImageFormats)
                    {
                        if (imageFormat.AllowDeletion && imageFormat.Gallery)
                        {
                            count++;
                        }
                    }
                }

            }

            return count;
        }

        public static int GetCustomImageFormatsAsListingCount(string accountNameKey)
        {
            var count = 0;

            var imageFormatGroupTypes = GetImageFormatGroupTypes();

            foreach (var imageFormatGroupType in imageFormatGroupTypes)
            {
                var imageGroups = GetImageFormats(accountNameKey, imageFormatGroupType.ImageFormatGroupTypeNameKey);

                foreach (var imageGroup in imageGroups)
                {
                    foreach (var imageFormat in imageGroup.ImageFormats)
                    {
                        if (imageFormat.AllowDeletion && imageFormat.Listing)
                        {
                            count++;
                        }
                    }
                }

            }

            return count;
        }

        #endregion

        #region Get

        public static List<ImageFormatGroupModel> GetImageFormats(string accountNameKey, string imageFormatGroupTypeNameKey)
        {
            List<ImageFormatGroupModel> imageGroups = null;

            string cacheKey = ApplicationImageFormatsHash.Key(accountNameKey);
            string cacheField = ApplicationImageFormatsHash.Fields.AllFormatsByGroupType(imageFormatGroupTypeNameKey);

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            #region Get propertyTypes from cache

            try
            {
                var redisValue = cache.HashGet(cacheKey, cacheField);
                if (redisValue.HasValue)
                {
                    imageGroups = JsonConvert.DeserializeObject<List<ImageFormatGroupModel>>(redisValue);
                }
            }
            catch
            {

            }

            if (imageGroups == null)
            {
                #region Get categories from SQL

                var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

                imageGroups = Sql.Statements.SelectStatements.SelectImageFormatsByGroupType(account.SqlPartition, account.SchemaName, imageFormatGroupTypeNameKey);

                #endregion

                #region Store count into cache

                try
                {
                    cache.HashSet(cacheKey, cacheField, JsonConvert.SerializeObject(imageGroups), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                #endregion
            }

            return imageGroups;

            #endregion
        }

        
        public static ImageFormatModel GetImageFormat(string accountNameKey, string groupType, string groupName, string formatName, out ImageFormatGroupModel imageGroup)
        {
            imageGroup = null;
            ImageFormatModel imageFormat = null;

            //Get all formats for this group type
            var imageFormats = GetImageFormats(accountNameKey, groupType);

            //Extract the correct image format for this processing submission
            foreach (ImageFormatGroupModel group in imageFormats)
            {
                if (group.ImageFormatGroupNameKey == groupName)
                {
                    imageGroup = group;

                    foreach (ImageFormatModel format in group.ImageFormats)
                    {
                        if (format.ImageFormatNameKey == formatName)
                        {
                            imageFormat = format;
                        }
                    }
                }
            }



            return imageFormat;
        }

        #endregion

        #region Manage

        #region Create

        public static DataAccessResponseType CreateImageGroup(Account account, string imageGroupTypeNameKey, string imageGroupName)
        {
            var response = new DataAccessResponseType();

            #region Validate Input

            #region Validate Image Group Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidImageGroupName(imageGroupName);
            if (!ojectNameValidationResponse.isValid)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = ojectNameValidationResponse.validationMessage };
            }

            #endregion


            if (String.IsNullOrEmpty(imageGroupTypeNameKey))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "imageGroupTypeNameKey cannot be empty" };
            }

            if (String.IsNullOrEmpty(imageGroupName))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "imageGroupName cannot be empty" };
            }

            #endregion

            #region Validate Plan Restrictions

            if(GetCustomImageGroupCount(account.AccountNameKey) >= account.PaymentPlan.MaxImageGroups)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You have reached your account restriction of " + account.PaymentPlan.MaxImageGroups  + " custom image groups" };
            }

            #endregion

            //CREATE NAME KEY
            var imageGroupNameKey = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(imageGroupName);

            #region Validate GroupType Exists

            var imageGroupTypes = GetImageFormatGroupTypes();

            bool typeExists = false;

            foreach (ImageFormatGroupTypeModel type in imageGroupTypes)
            {
                if (type.ImageFormatGroupTypeNameKey == imageGroupTypeNameKey)
                {
                    typeExists = true;
                }
            }

            if (!typeExists)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Format group type '" + imageGroupTypeNameKey + "' does not exists!" };
            }

            #endregion

            #region Validate GroupName is unique to this type

            var imageFormats = GetImageFormats(account.AccountNameKey, imageGroupTypeNameKey);

            bool nameUniqueInType = true;

            foreach (ImageFormatGroupModel group in imageFormats)
            {
                if (group.ImageFormatGroupTypeNameKey == imageGroupTypeNameKey && group.ImageFormatGroupNameKey == imageGroupNameKey)
                {
                    nameUniqueInType = false;   
                }
            }

            if (!nameUniqueInType)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Group name '" + imageGroupNameKey + "' is not unique to the '" + imageGroupTypeNameKey + "' type!" };
            }

            #endregion

            #region Create Model & ID

            var imageGroup = new ImageFormatGroupModel
            {
                ImageFormatGroupTypeNameKey = imageGroupTypeNameKey,

                ImageFormatGroupID = Guid.NewGuid(),
                ImageFormatGroupName = imageGroupName,
                ImageFormatGroupNameKey = imageGroupNameKey
            };

            imageGroup.ImageFormatGroupID = Guid.NewGuid();

            #endregion

            //INSERT
            response = Sql.Statements.InsertStatements.InsertImageGroup(account.SqlPartition, account.SchemaName, imageGroup); //, account.PaymentPlan.MaxImageGroups);

            //CLear Cache
            if(response.isSuccess)
            {
                Caching.InvalidateImageFormatCaches(account.AccountNameKey);
            }
            
            return response;
        }

        public static DataAccessResponseType  CreateImageFormat(Account account, string imageGroupTypeNameKey, string imageGroupNameKey, string imageFormatName, int width, int height, bool listing, bool gallery)
        {
            var response = new DataAccessResponseType();

            #region Validate Plan Restrictions

            if (GetCustomImageFormatCount(account.AccountNameKey) >= account.PaymentPlan.MaxImageFormats)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You have reached your account restriction of " + account.PaymentPlan.MaxImageFormats + " custom image formats" };
            }

            if (gallery && GetCustomImageGalleryCount(account.AccountNameKey) >= account.PaymentPlan.MaxImageGalleries)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You have reached your account restriction of " + account.PaymentPlan.MaxImageGalleries + " custom image galleries" };
            }

            #endregion

            #region Validate Platform Restrictions

            if(listing)
            {
                if(GetCustomImageFormatsAsListingCount(account.AccountNameKey) >= Settings.Objects.Limitations.MaximumImageFormatsAsListings)
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You have reached the platform restriction of " + Settings.Objects.Limitations.MaximumImageFormatsAsListings + " listing images per inventory" };
                }

            }

            #endregion

            #region Validate Input

            if (String.IsNullOrEmpty(imageGroupTypeNameKey))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "imageGroupTypeNameKey cannot be empty" };
            }

            if (String.IsNullOrEmpty(imageGroupNameKey))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "imageGroupNameKey cannot be empty" };
            }

            if (String.IsNullOrEmpty(imageFormatName))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "imageFormatName cannot be empty" };
            }

            if (listing && gallery)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image format cannot be both a Listing and a Gallery" };
            }

            if (height < 1)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image height must be taller than 1px" };
            }
            if (width < 0)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image width must be a positive number" };
            }
            //if (width < 1) //<-- 0 now means that this is a VARIABLE WIDTH image
            //{
            //return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image width must be wider than 1px" };
            //}

            if (height > 4000)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image height cannot be taller than 4,000px" };
            }
            if (width > 4000)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image width cannot be wider than 4,000px" };
            }

            #endregion

            #region Validate Image Format Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidImageFormatName(imageFormatName);
            if (!ojectNameValidationResponse.isValid)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = ojectNameValidationResponse.validationMessage };
            }

            #endregion

            //CREATE NAME KEY
            var imageFormatNameKey = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(imageFormatName);

            #region Validate GroupType Exists

            var imageGroupTypes = GetImageFormatGroupTypes();

            bool typeExists = false;

            foreach(ImageFormatGroupTypeModel type in imageGroupTypes)
            {
                if(type.ImageFormatGroupTypeNameKey == imageGroupTypeNameKey)
                {
                    typeExists = true;
                }
            }

            if (!typeExists)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Format group type '" + imageGroupTypeNameKey + "' does not exists!" };
            }

            #endregion

            var imageFormats = GetImageFormats(account.AccountNameKey, imageGroupTypeNameKey);

            #region Validate Group Exists in this Type

            bool groupExistsInType = false;

            foreach (ImageFormatGroupModel group in imageFormats)
            {
                if (group.ImageFormatGroupTypeNameKey == imageGroupTypeNameKey && group.ImageFormatGroupNameKey == imageGroupNameKey)
                {
                    groupExistsInType = true;
                }
            }

            if (!groupExistsInType)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Format group '" + imageGroupNameKey + "' does not exists!" };
            }

            #endregion

            #region Validate Format name is unique to this group/type combo

            bool nameUniqueInGroup = true;

            foreach (ImageFormatGroupModel group in imageFormats)
            {
                if (group.ImageFormatGroupTypeNameKey == imageGroupTypeNameKey && group.ImageFormatGroupNameKey == imageGroupNameKey)
                {
                    foreach(ImageFormatModel format in group.ImageFormats)
                    {
                        if(format.ImageFormatName == imageFormatName || format.ImageFormatNameKey == imageFormatNameKey)
                        {
                            nameUniqueInGroup = false;
                        }
                    }
                }
            }

            if (!nameUniqueInGroup)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Format name '" + imageFormatNameKey + "' is not unique to the '" + imageGroupNameKey + "' group!" };
            }

            #endregion

            #region Create Model & ID

            var imageFormat = new ImageFormatModel
            {
                ImageFormatGroupTypeNameKey = imageGroupTypeNameKey,
                ImageFormatGroupNameKey = imageGroupNameKey,

                ImageFormatID = Guid.NewGuid(),
                ImageFormatName = imageFormatName,
                ImageFormatNameKey = imageFormatNameKey,

                Height = height,
                Width = width,

                Listing = listing,
                Gallery = gallery
            };

            #endregion

            //INSERT
            response = Sql.Statements.InsertStatements.InsertImageFormat(account.SqlPartition, account.SchemaName, imageFormat); //, account.PaymentPlan.MaxImageFormats);

            //CLear Cache
            if (response.isSuccess)
            {
                Caching.InvalidateImageFormatCaches(account.AccountNameKey);
            }

            return response;
        }

        #endregion

        #region Edit/Update (Removed - just delete and recreate)
        /*
        public static DataAccessResponseType UpdateImageFormatVisibleState(Account account, string imageFormatId, bool isVisible)
        {
            var response = new DataAccessResponseType();

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateImageFormatVisibleState(account.SqlPartition, account.SchemaName, imageFormatId, isVisible);

            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateImageFormatCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdateImageFormatGalleryState(Account account, string imageFormatId, bool isGallery)
        {
            var response = new DataAccessResponseType();

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateImageFormatGalleryState(account.SqlPartition, account.SchemaName, imageFormatId, isGallery);

            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateImageFormatCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdateImageFormatListingState(Account account, string imageFormatId, bool isListing)
        {
            var response = new DataAccessResponseType();

            //Make the update
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateImageFormatListingState(account.SqlPartition, account.SchemaName, imageFormatId, isListing);

            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateImageFormatCaches(account.AccountNameKey);
            }

            return response;
        }

        */

        #endregion

        #region Delete

        public static DataAccessResponseType DeleteImageGroup(Account account, string imageGroupTypeNameKey, string imageGroupNameKey)
        {
            var response = new DataAccessResponseType();

            //Get image formats for this grouptype
            var imageFormats = GetImageFormats(account.AccountNameKey, imageGroupTypeNameKey);
            ImageFormatGroupModel imageGroupToDelete = null;

            #region Extract Group from GroupType
            
            foreach(ImageFormatGroupModel group in imageFormats)
            {
                if (group.ImageFormatGroupNameKey == imageGroupNameKey)
                {
                    imageGroupToDelete = group;
                }
            }

            #endregion

            #region Validate Deletion Is Allowed

            if (imageGroupToDelete == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Image group does not exist!"
                };
            }

            if(imageGroupToDelete.ImageFormats.Count > 0)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Cannot delete an image group that has formats attached!"
                };
            }

            //Make sure deletion is allowed
            if (!imageGroupToDelete.AllowDeletion)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Deletion is not allowed on this image group."
                };
            }

            #endregion

            response.isSuccess = Sql.Statements.DeleteStatements.DeleteImageGroup(account.SqlPartition, account.SchemaName, imageGroupToDelete.ImageFormatGroupID.ToString());

            //Clear Cache
            if (response.isSuccess)
            {
                Caching.InvalidateImageFormatCaches(account.AccountNameKey);
            }

            return response;
        }


        public static DataAccessResponseType DeleteImageFormat(Account account, string imageGroupTypeNameKey, string imageGroupNameKey, string imageFormatNameKey)
        {
            var response = new DataAccessResponseType();

            //Get image formats for this grouptype
            var imageFormats = GetImageFormats(account.AccountNameKey, imageGroupTypeNameKey);
            ImageFormatModel imageFormatToDelete = null;

            #region Extract Format from GroupType/Group

            foreach (ImageFormatGroupModel group in imageFormats)
            {
                if (group.ImageFormatGroupNameKey == imageGroupNameKey)
                {
                    foreach(ImageFormatModel format in group.ImageFormats)
                    {
                        if(format.ImageFormatNameKey == imageFormatNameKey)
                        {
                            imageFormatToDelete = format;
                        }
                    }
                }
            }

            #endregion

            #region Validate Process Allowed

            if (imageFormatToDelete == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Image format does not exist!"
                };
            }

            //Make sure deletion is allowed
            if (!imageFormatToDelete.AllowDeletion)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Deletion is not allowed on this image format."
                };
            }

            #endregion

            response.isSuccess = Sql.Statements.DeleteStatements.DeleteImageFormat(account.SqlPartition, account.SchemaName, imageFormatToDelete.ImageFormatID.ToString());

            //Clear Cache
            if (response.isSuccess)
            {
                Caching.InvalidateImageFormatCaches(account.AccountNameKey);
            }

            return response;
        }

        #endregion 

        #endregion

        #endregion

    }
}
