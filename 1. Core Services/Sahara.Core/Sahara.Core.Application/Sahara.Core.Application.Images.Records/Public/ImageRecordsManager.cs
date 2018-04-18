using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Images.Formats;
using Sahara.Core.Application.Images.Formats.Models;
using Sahara.Core.Application.Images.Records.Internal;
using Sahara.Core.Application.Images.Records.Models;
using Sahara.Core.Application.Images.Records.TableEntities;
using Sahara.Core.Application.Images.Storage.Public;
using Sahara.Core.Common.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Records
{
    public class ImageRecordsManager
    {

        #region Create or Append Image Record AppendToImageGalleryRecordForObject

        public static DataAccessResponseType CreateImageRecordForObject(string accountId, string storagePartition, string imageFormatGroupTypeNameKey, string objectId, string imageGroupName, string imageGroupNameKey, string imageFormatName, string imageFormatNameKey, string title, string description, string url, string filename, string filepath, string containerName, string blobPath, int height, int width, bool isListing = false)
        {
            try
            {
                return Internal.ImageRecordTableStorage.StoreImageRecord(accountId, storagePartition, imageFormatGroupTypeNameKey, objectId, imageGroupName, imageGroupNameKey, imageFormatName, imageFormatNameKey, title, description, url, filename, filepath, containerName, blobPath, height, width, isListing);                                
            }
            catch(Exception e)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }          
        }

        public static DataAccessResponseType AppendToImageGalleryRecordForObject(Account account, string imageFormatGroupTypeNameKey, string objectId, string imageGroupName, string imageGroupNameKey, string imageFormatName, string imageFormatNameKey, string title, string description, string url, string filename, string filepath, string blobPath)
        {
            try
            {
                return Internal.ImageRecordTableStorage.AppendImageGalleryRecord(account, imageFormatGroupTypeNameKey, objectId, imageGroupName, imageGroupNameKey, imageFormatName, imageFormatNameKey, title, description, url, filename, filepath, blobPath);
            }
            catch (Exception e)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }
        }


        #endregion

        /// <summary>
        /// Possibly never used by clients (With the exception of Account Admin to manage images for an objet so the associated missing formats can be seen
        /// In most cases clients should connect to Table storage on their own to perform this merge to avoid a WCF call
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountNameKey"></param>
        /// <param name="imageFormatGroupTypeNameKey"></param>
        /// <param name="objectId"></param>
        /// <param name="listingsOnly"></param>
        /// <returns></returns>
        public static List<ImageRecordGroupModel> GetImageRecordsForObject(string accountId, string storagePartition, string accountNameKey, string imageFormatGroupTypeNameKey, string objectId, bool listingsOnly = false)
        {
            var imageRecords = new List<ImageRecordGroupModel>();

            #region Get Image Formats/FormatGroups for this ImageFormatGroupTypeNameKey

            var imageFormats = ImageFormatsManager.GetImageFormats(accountNameKey, imageFormatGroupTypeNameKey);

            //If there are no formats ignore are return null to the user
            if(imageFormats.Count == 0)
            {
                return null;
            }

            #endregion

            #region Get all image records for this object

            var imageRecordEntities = Internal.ImageRecordTableStorage.RetrieveImageRecords(accountId, storagePartition, imageFormatGroupTypeNameKey, objectId, listingsOnly);

            #endregion

            #region  Convert FormatGroups/Formats to RecordGroups/Records And merge them with Records (Where records exist)

            foreach (ImageFormatGroupModel imageFormatGroup in imageFormats)
            {
                var imageRecordGroupModel = new ImageRecordGroupModel {
                    GroupName = imageFormatGroup.ImageFormatGroupName,
                    GroupNameKey = imageFormatGroup.ImageFormatGroupNameKey
                };

                imageRecordGroupModel.ImageRecords = new List<ImageRecordModel>();

                foreach(ImageFormatModel imageFormat in imageFormatGroup.ImageFormats)
                {
                    if (listingsOnly && !imageFormat.Listing)
                    {
                        //ignore
                    }
                    else
                    {
                        #region Get the associated record for each format (if one exists, or create a null object for it if a record does not

                        var recordExists = false;

                        foreach (ImageRecordTableEntity tableEntity in imageRecordEntities)
                        {
                            if (tableEntity.ImageKey == imageFormat.ImageFormatGroupNameKey + "-" + imageFormat.ImageFormatNameKey)
                            {
                                recordExists = true;

                                #region A record exists for this format, convert and remove from list

                                imageRecordGroupModel.ImageRecords.Add(Internal.Transforms.TableEntity_To_ImageRecord(tableEntity, imageFormat, imageFormat.Gallery));

                                #endregion
                            }
                        }

                        if (!recordExists)
                        {
                            #region A record does not exist, use a null version of the format slug instead

                            imageRecordGroupModel.ImageRecords.Add(Internal.Transforms.ImageFormat_To_ImageRecord(imageFormat));

                            #endregion
                        }

                        #endregion
                    }

                }

                if(imageRecordGroupModel.ImageRecords.Count > 0)
                {
                    imageRecords.Add(imageRecordGroupModel); //<-- Only append if group has records
                }

            }

            #endregion

            // Return Image Records for this object
            return imageRecords;
        }

        /// <summary>
        /// Used to check if an image format has ANY records BEFORE deletion is allowd by Formts Services
        /// </summary>
        public static bool ImageRecordExistsForImageKey(string accountId, string storagePartition, string accountNameKey, string imageFormatGroupTypeNameKey, string imageRecordKey)
        {

            #region Get all image records for this object

            var imageRecordEntity = Internal.ImageRecordTableStorage.RetrieveAnyImageRecordForFormatKey(accountId, storagePartition, imageFormatGroupTypeNameKey, imageRecordKey);

            #endregion

            if(imageRecordEntity == null)
            {
                return false;
            }
            else
            {
                return true;
            }


        }


        #region Updates

        public static DataAccessResponseType UpdateImageRecordTitleForObject(string accountId, string storagePartition, string objectType, string objectId, string groupNameKey, string formatNameKey, string newTitle, bool isListing)
        {
            if (newTitle.Length > Settings.Objects.Limitations.ImageFormatTitleMaxLength)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image titles cannot be longer than " + Settings.Objects.Limitations.ImageFormatTitleMaxLength + " characters" };
            }

            return Internal.ImageRecordTableStorage.UpdateImageRecordTitle(accountId, storagePartition, objectType, objectId, groupNameKey, formatNameKey, newTitle, isListing);
        }

        public static DataAccessResponseType UpdateImageRecordDescriptionForObject(string accountId, string storagePartition, string objectType, string objectId, string groupNameKey, string formatNameKey, string newDescription, bool isListing)
        {
            if (newDescription.Length > Settings.Objects.Limitations.ImageFormatDescriptionMaxLength)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image descriptions cannot be longer than " + Settings.Objects.Limitations.ImageFormatDescriptionMaxLength + " characters" };
            }

            return Internal.ImageRecordTableStorage.UpdateImageRecordDescription(accountId, storagePartition, objectType, objectId, groupNameKey, formatNameKey, newDescription, isListing);
        }

        public static DataAccessResponseType UpdateImageGalleryRecordTitleForObject(string accountId, string storagePartition, string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex, string newTitle)
        {
            if (newTitle.Length > Settings.Objects.Limitations.ImageFormatTitleMaxLength)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image titles cannot be longer than " + Settings.Objects.Limitations.ImageFormatTitleMaxLength + " characters" };
            }
            if (newTitle.Contains("|"))
            {
                newTitle = newTitle.Replace("|", " ");
            }

            return Internal.ImageRecordTableStorage.UpdateImageRecordGalleryTitle(accountId, storagePartition, objectType, objectId, groupNameKey, formatNameKey, imageIndex, newTitle);
        }

        public static DataAccessResponseType UpdateImageGalleryRecordDescriptionForObject(string accountId, string storagePartition, string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex, string newDescription)
        {
            if (newDescription.Length > Settings.Objects.Limitations.ImageFormatDescriptionMaxLength)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image descriptions cannot be longer than " + Settings.Objects.Limitations.ImageFormatDescriptionMaxLength + " characters" };
            }
            if (newDescription.Contains("|"))
            {
                newDescription = newDescription.Replace("|", " ");
            }

            return Internal.ImageRecordTableStorage.UpdateImageRecordGalleryDescription(accountId, storagePartition, objectType, objectId, groupNameKey, formatNameKey, imageIndex, newDescription);
        }


        #endregion

        #region Deletions

        /// <summary>
        /// Delete ALL records/blobs for an objects images (ONLY Used when deleting a product)
        /// </summary>
        public static DataAccessResponseType DeleteAllImageRecordsForObject(Account account, string imageFormatGroupTypeNameKey, string objectId)
        {
            //Get the record (so we can loop through blobs for deletion)
            var imageRecords = ImageRecordsManager.GetImageRecordsForObject(account.AccountID.ToString(), account.StoragePartition, account.AccountNameKey, imageFormatGroupTypeNameKey, objectId);

            //Delete all Blobs associated with ALL image records (loop if gallery)
            foreach (ImageRecordGroupModel group in imageRecords)
            {
                foreach (ImageRecordModel record in group.ImageRecords)
                {
                    if(record.Type == "single")
                    {
                        if(record.BlobPath != null)
                        {
                            ImageStorageManager.DeleteImageBlobs(account.StoragePartition, record.ContainerName, record.BlobPath);
                        }                        
                    }
                    else
                    {
                        foreach(ImageRecordGalleryModel galleryRecord in record.GalleryImages)
                        {
                            if(galleryRecord.BlobPath != null)
                            {
                                ImageStorageManager.DeleteImageBlobs(account.StoragePartition, record.ContainerName, galleryRecord.BlobPath);
                            }                           
                        }
                    }
                    
                }
            }
            
            return Internal.ImageRecordTableStorage.DeleteAllImageRecordsForObject(account.AccountID.ToString(), account.StoragePartition, imageFormatGroupTypeNameKey, objectId);
        }

        /// <summary>
        /// Delete ALL  records/blobs for an object in one particular format
        /// </summary>
        public static DataAccessResponseType DeleteImageRecordForObject(Account account, string imageFormatGroupTypeNameKey, string objectId, string imageGroupNameKey, string imageFormatNameKey)
        {
            //Get the record (so we can loop through blobs for deletion)
            var imageRecords = ImageRecordsManager.GetImageRecordsForObject(account.AccountID.ToString(), account.StoragePartition, account.AccountNameKey, imageFormatGroupTypeNameKey, objectId);

            //Delete all Blobs associated with ALL image records (loop if gallery)
            foreach (ImageRecordGroupModel group in imageRecords)
            {
                if(group.GroupNameKey == imageGroupNameKey)
                {
                    foreach (ImageRecordModel record in group.ImageRecords)
                    {
                        if(record.FormatNameKey == imageFormatNameKey)
                        {
                            if (record.Type == "single")
                            {
                                ImageStorageManager.DeleteImageBlobs(account.StoragePartition, record.ContainerName, record.BlobPath);
                            }
                            else
                            {
                                foreach (ImageRecordGalleryModel galleryRecord in record.GalleryImages)
                                {
                                    ImageStorageManager.DeleteImageBlobs(account.StoragePartition, record.ContainerName, galleryRecord.BlobPath);
                                }
                            }
                        }

                    }
                }

            }

            //Can also be used to clear ALL images in a gallery record
            return Internal.ImageRecordTableStorage.DeleteImageRecord(account.AccountID.ToString(), account.StoragePartition, imageFormatGroupTypeNameKey, objectId, imageGroupNameKey, imageFormatNameKey);
        }

        #endregion

        #region Gallery Management


        public static DataAccessResponseType ReorderGalleryImages(string accountId, string storagePartition, string objectType, string objectId, string groupNameKey, string formatNameKey, List<int> imageIndexOrder)
        {
            return Internal.ImageRecordTableStorage.ReorderGalleryImages(accountId, storagePartition, objectType, objectId, groupNameKey, formatNameKey, imageIndexOrder);
        }

        public static DataAccessResponseType DeleteGalleryImage(Account account, string imageFormatGroupTypeNameKey, string objectId, string imageGroupNameKey, string imageFormatNameKey, int imageIndex)
        {
            //Get the records (so we can loop through blobs for deletion)
            var imageRecords = ImageRecordsManager.GetImageRecordsForObject(account.AccountID.ToString(), account.StoragePartition, account.AccountNameKey, imageFormatGroupTypeNameKey, objectId);

            ImageRecordModel imageRecord = null;

            #region  Get the record associated with the gallery deletion request
            //Delete all Blobs associated with ALL image records (loop if gallery)
            foreach (ImageRecordGroupModel group in imageRecords)
            {
                if (group.GroupNameKey == imageGroupNameKey)
                {
                    foreach (ImageRecordModel record in group.ImageRecords)
                    {
                        if (record.FormatNameKey == imageFormatNameKey)
                        {
                            if (record.Type != "gallery")
                            {
                                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This image record is not a gallery" };
                            }
                            else
                            {
                                //Assign to the record object
                                imageRecord = record;

                                ImageStorageManager.DeleteImageBlobs(account.StoragePartition, record.ContainerName, record.GalleryImages[imageIndex].BlobPath);
                                
                            }
                        }

                    }
                }

            }

            #endregion

            //if this is the only image in the gallery we delete the entire record
            if (imageRecord.GalleryImages.Count <= 1)
            {
                //This is the last image so we delete the ENTIRE record
                return Internal.ImageRecordTableStorage.DeleteImageRecord(account.AccountID.ToString(), account.StoragePartition, imageFormatGroupTypeNameKey, objectId, imageGroupNameKey, imageFormatNameKey);
            }
            else
            {
                //Update record to remove the array index on ALL comma deliminated records
                return Internal.ImageRecordTableStorage.DeleteImageGalleryRecordItem(account.AccountID.ToString(), account.StoragePartition, imageFormatGroupTypeNameKey, objectId, imageGroupNameKey, imageFormatNameKey, imageIndex);
            }

        }
       

        #endregion
    }
}
