using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Images.Formats;
using Sahara.Core.Application.Images.Formats.Models;
using Sahara.Core.Application.Images.Records;
using Sahara.Core.Application.Images.Records.Models;
using Sahara.Core.Application.Images.Storage.Public;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Imaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Processing.Public
{
    public static class ApplicationImageProcessingManager
    {
        //Creates initial record (as well as replacements for existing records and additions for gallery images) for ALL types (listings, galleries, etc...)
        public static DataAccessResponseType ProcessAndRecordApplicationImage(Account account, ImageProcessingManifestModel imageManifest, ImageCropCoordinates imageCropCoordinates, ImageEnhancementInstructions imageEnhancementInstructions = null)
        {

            var tableStorageResult = new DataAccessResponseType();

            #region Get image format & group for this new record

            ImageFormatGroupModel imageGroup;
            var imageFormat = ImageFormatsManager.GetImageFormat(account.AccountNameKey, imageManifest.GroupTypeNameKey, imageManifest.GroupNameKey, imageManifest.FormatNameKey, out imageGroup);

            if(imageFormat == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Not a valid image format" };
            }

            #endregion

            #region Get Image Record for this image slot on this object (If one exists)
          
            ImageRecordModel existingImageRecord = null;

            var imageRecords = ImageRecordsManager.GetImageRecordsForObject(account.AccountID.ToString(), account.StoragePartition, account.AccountNameKey, imageManifest.GroupTypeNameKey, imageManifest.ObjectId);

            foreach(ImageRecordGroupModel imageRecordGroupModel in imageRecords)
            {
                if(imageRecordGroupModel.GroupNameKey == imageManifest.GroupNameKey)
                {
                    foreach(ImageRecordModel record in imageRecordGroupModel.ImageRecords)
                    {
                        if(record.FormatNameKey == imageManifest.FormatNameKey)
                        {
                            if(record.BlobPath != null || record.GalleryImages.Count > 0)
                            {
                                existingImageRecord = record; //<-- We only assign it if images exist (single or gallery)
                            }
                            
                        }
                    }
                }
            }
            
            #endregion

            #region  Process image according to ImageFormat specs, save to blob storage for the AccountID

            //var folderName = account.AccountID.ToString() + "/" + imageManifest.GroupTypeNameKey + "images";
            var folderName = imageManifest.GroupTypeNameKey + "Images";

            //Override of "Product" to "Item" -----------------
            if(folderName == "productImages")
            {
                folderName = "itemImages";
            }
            //------------------------------------------------

            var applicationImageProcessor = new Imaging.ApplicationImageProcessor();

            if (imageManifest.Title.Length > Settings.Objects.Limitations.ImageFormatTitleMaxLength)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image titles cannot be longer than " + Settings.Objects.Limitations.ImageFormatTitleMaxLength + " characters" };
            }
            if (imageManifest.Description.Length > Settings.Objects.Limitations.ImageFormatDescriptionMaxLength)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Image descriptions cannot be longer than " + Settings.Objects.Limitations.ImageFormatDescriptionMaxLength + " characters" };
            }

            //If this is an addition to an image gallery. Make sure we are within safe limits before creating the image
            if (existingImageRecord != null && imageFormat.Gallery && existingImageRecord.GalleryImages.Count >= account.PaymentPlan.MaxImagesPerGallery)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your plan does not allow for more than " + account.PaymentPlan.MaxImagesPerGallery + " images per gallery" };
            }

            //If this is a gallery type we cannot use pipes "|" in titles or descriptions
            if (imageFormat.Gallery && imageManifest.Title.Contains("|"))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot use the '|' character in gallery titles." };
            }
            if (imageFormat.Gallery && imageManifest.Description.Contains("|"))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot use the '|' character in gallery descriptions." };
            }

            var processingResult = applicationImageProcessor.ProcessApplicationImage(account.AccountID.ToString(), account.StoragePartition, imageManifest.SourceContainerName, imageManifest.FileName, imageFormat.Width, imageFormat.Height, folderName, imageManifest.Type, imageManifest.Quality, imageCropCoordinates, imageEnhancementInstructions);
           
            #endregion

            if (processingResult.isSuccess)
            {
                //var cdnUrl = Settings.Azure.Storage.GetStoragePartition(account.StoragePartition).CDN;

                var newContainerName = account.AccountID.ToString();
                var newImageFileName = processingResult.SuccessMessage + "." + imageManifest.Type;
                //var newUrl = "http://" + cdnUrl + "/" + newContainerName + "/" + folderName + "/" + newImageFileName; //<-- CDN now assigned by consumers in case of future migration
                var newUrl = newContainerName + "/" + folderName + "/" + newImageFileName;
                var newBlobPath = folderName + "/" + newImageFileName;
                var newFilePath = newContainerName + "/" + newBlobPath;

                #region Record data to table storage

                if (existingImageRecord == null || !imageFormat.Gallery)
                {
                    // This is the first image added to this record (gallery or single)
                    #region Store or Replace Image Record for this ObjectType/ObjectID

                    tableStorageResult = ImageRecordsManager.CreateImageRecordForObject(
                            account.AccountID.ToString(),
                            account.StoragePartition,
                            imageManifest.GroupTypeNameKey,
                            imageManifest.ObjectId,
                            imageGroup.ImageFormatGroupName,
                            imageGroup.ImageFormatGroupNameKey,
                            imageFormat.ImageFormatName,
                            imageFormat.ImageFormatNameKey,
                            imageManifest.Title,
                            imageManifest.Description,
                            newUrl,
                            newImageFileName,
                            newFilePath,
                            newContainerName,
                            newBlobPath,
                            imageFormat.Height,
                            imageFormat.Width,
                            imageFormat.Listing
                        );

                    #endregion
                }
                else
                {
                    //This is an addition to a Galley, Append to the array of gallery image (And do not delete any previous blobs)
                    #region Append to gallery Image Record for this ObjectType/ObjectID

                    tableStorageResult = ImageRecordsManager.AppendToImageGalleryRecordForObject(
                            account,
                            imageManifest.GroupTypeNameKey,
                            imageManifest.ObjectId,
                            imageGroup.ImageFormatGroupName,
                            imageGroup.ImageFormatGroupNameKey,
                            imageFormat.ImageFormatName,
                            imageFormat.ImageFormatNameKey,
                            imageManifest.Title,
                            imageManifest.Description,
                            newUrl,
                            newImageFileName,
                            newFilePath,
                            newBlobPath
                        );

                    #endregion

                }
                #endregion

                // Add URL for UI to preload:
                tableStorageResult.SuccessMessage = newUrl;

                #region Clean up previous blob data

                if (existingImageRecord == null || imageFormat.Gallery)
                {
                    //Do nothing, this was the first image created for this slot, or was an image appended to a gallery
                }
                else
                {
                    //This is a replacement on a single image. Delete the previous image from blob storage
                    #region Delete previous blob image

                    ImageStorageManager.DeleteImageBlobs(account.StoragePartition, existingImageRecord.ContainerName, existingImageRecord.BlobPath);

                    #endregion
                }

                #endregion

                if (!tableStorageResult.isSuccess)
                {
                    #region There was an issue recording data to table storage. Delete the blobs we just created

                    try
                    {
                        ImageStorageManager.DeleteImageBlobs(account.StoragePartition, newContainerName, newBlobPath);
                    }
                    catch
                    {

                    }

                    #endregion
                }
            }

            //Return results
            return tableStorageResult;
        }

        //Will also clear ALL gallery contents, creating no records for the associated object
        public static DataAccessResponseType PurgeApplicationImageBlobsAndRecordsForObject()
        {
            return null;
        }

        #region Supplemental (Merged wih ProcessAndRecordApplicationImage above)

        //Cannot be used with gallery methods (use add or delete below)
        //public static DataAccessResponseType ReplaceApplicationImageBlobAndRecord()
        //{
        //return null;
        //}

        #endregion

        #region Gallery Specific

        /// <summary> (Merged with ProcessAndRecordApplicationImage above)
        /// Used to add additional images to gallery record types (initial image MUST exist first using ProcessAndRecordApplicationImage above
        /// </summary>
        /// <returns></returns>
        //public static DataAccessResponseType ProcessAndRecordImageGalleryAndRecord()
        //{
        //Check that a record exists and that format is of gallery type.

        //Check capacity against limits.

        //return null;
        //}

        /// <summary>
        /// Deletes a SINGLE image from an array of gallery images.
        /// If there is only ONE record then the ENTIRE record is cleared and processing is sent to "PurgeApplicationImageBlobsAndRecordsForObject"
        /// </summary>
        /// <returns></returns>
        public static DataAccessResponseType DeleteImageGalleryBlobAndRecord()
        {
            return null;
        }

        public static DataAccessResponseType ReorderGalleryImages()
        {
            return null;
        }

        #endregion


    }
}
