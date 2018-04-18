using AccountAdminSite.ApplicationImageFormatsService;
using AccountAdminSite.ApplicationImageRecordsService;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    #region Shared Public Classes

    public static class ImageRecordsCommon
    {
        public static List<ImageRecordGroupModel> GetImageRecordsForObject(string accountNameKey, string storagePartitionName, string imageFormatGroupTypeNameKey, string objectId, bool listingsOnly = false)
        {
            var imageRecords = new List<ImageRecordGroupModel>();

            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            #region Get Image Formats/FormatGroups for this ImageFormatGroupTypeNameKey (Local cache first ---> Then Redis --> Then WCF/SQL)

            List<ImageFormatGroupModel> imageFormats = null;

            #region (Plan A) Get ImageFormats for this ImageFormatGroupType from Local Cache

            bool localCacheEmpty = false;
            string localCacheKey = accountNameKey + ":imageFormats:" + imageFormatGroupTypeNameKey;

            try
            {
                imageFormats = (List<ImageFormatGroupModel>)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (imageFormats == null)
            {
                localCacheEmpty = true;

                #region (Plan B) Get ImageFormats from ImageFormatsManager Shared Method (Redis or WCF call)

                imageFormats = SettingsCommon.GetImageFormatsHelper(accountNameKey, imageFormatGroupTypeNameKey);

                #endregion
            }


            if (localCacheEmpty)
            {
                #region store Account into local cache

                //store formats in local cache for 60 seconds:
                HttpRuntime.Cache.Insert(localCacheKey, imageFormats, null, DateTime.Now.AddSeconds(20), TimeSpan.Zero);

                #endregion
            }

            #endregion

            #region Get all image records for this object

            #region Connect to Table Storage & Set Retry Policy (Legacy)
            /*
            CloudStorageAccount storageAccount;

            // Credentials are from centralized CoreServiceSettings
            StorageCredentials storageCredentials = new StorageCredentials(CoreServices.PlatformSettings.Storage.AccountName, CoreServices.PlatformSettings.Storage.AccountKey);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;
            */
            #endregion

            #region Connect to table storage partition & Set Retry Policy 

            #region Get Storage Partition

            if (CoreServices.PlatformSettings.StorageParitions == null || CoreServices.PlatformSettings.StorageParitions.ToList().Count == 0)
            {
                //No Storage Partitions Available in Static List, refresh list from Core Services
                Common.RefreshPlatformSettings();
            }

            //Get the storage partition for this account
            var storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == storagePartitionName);

            if (storagePartition == null)
            {
                //May be a new partition, refresh platform setting and try again
                Common.RefreshPlatformSettings();
                storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == storagePartitionName);
            }

            #endregion

            var cdnEndpoint = "https://" + storagePartition.CDN + "/";

            CloudStorageAccount storageAccount;
            StorageCredentials storageCredentials = new StorageCredentials(storagePartition.Name, storagePartition.Key);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


            #endregion

            CloudTable cloudTable = null;

            if (!listingsOnly)
            {
                cloudTable = cloudTableClient.GetTableReference("acc" + accountId.Replace("-", "").ToLower() + imageFormatGroupTypeNameKey + "imgs");
            }
            else
            {
                cloudTable = cloudTableClient.GetTableReference("acc" + accountId.Replace("-", "").ToLower() + imageFormatGroupTypeNameKey + "lstimgs");
            }

            List<ImageRecordTableEntity> imageRecordEntities;

            //var imageRecordEntities = Internal.ImageRecordTableStorage.RetrieveImageRecords(accountId, imageFormatGroupTypeNameKey, objectId, listingsOnly);
            try
            {
                imageRecordEntities = (from record in cloudTable.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId) select record).ToList(); //new TableQuery<ImageRecordTableEntity>().AsQueryable().Where(p => p.PartitionKey == objectId).ToList();
            }
            catch
            {
                //Table may not exist yet, return empty record set (We do not use CreateIfNotExists on TableClient in case imageFormatGroupTypeNameKey is not valid!!!!)
                imageRecordEntities = new List<ImageRecordTableEntity>();
            }

            #endregion

            #region  Convert FormatGroups/Formats to RecordGroups/Records And merge them with Records (Where records exist)

            foreach (ImageFormatGroupModel imageFormatGroup in imageFormats)
            {
                var imageRecordGroupModel = new ImageRecordGroupModel
                {
                    GroupName = imageFormatGroup.ImageFormatGroupName,
                    GroupNameKey = imageFormatGroup.ImageFormatGroupNameKey
                };

                imageRecordGroupModel.ImageRecords = new List<ImageRecordModel>();

                foreach (ImageFormatModel imageFormat in imageFormatGroup.ImageFormats)
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

                                imageRecordGroupModel.ImageRecords.Add(Transforms.TableEntity_To_ImageRecord(cdnEndpoint, tableEntity, imageFormat, imageFormat.Gallery));

                                #endregion
                            }
                        }

                        if (!recordExists)
                        {
                            #region A record does not exist, use a null version of the format slug instead

                            imageRecordGroupModel.ImageRecords.Add(Transforms.ImageFormat_To_ImageRecord(imageFormat));

                            #endregion
                        }

                        #endregion
                    }

                }

                if (imageRecordGroupModel.ImageRecords.Count > 0)
                {
                    imageRecords.Add(imageRecordGroupModel);  //<-- Only append if group has records or format slugs
                }


            }

            #endregion

            return imageRecords;
        }
    }

    #endregion

    public class ImageRecordsController : Controller
    {
        [Route("ImageRecords/Json/GetImageRecordsForObject")]
        [HttpGet]
        public JsonNetResult GetImageRecordsForObject(string imageFormatGroupTypeNameKey, string objectId, bool listingsOnly = false)
        {            
            var accountNameKey = Common.GetSubDomain(Request.Url);
            var account = Common.GetAccountObject(accountNameKey);

            var imageRecords = ImageRecordsCommon.GetImageRecordsForObject(accountNameKey, account.StoragePartition, imageFormatGroupTypeNameKey, objectId, listingsOnly);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = imageRecords;

            return jsonNetResult;

        }
    }


    internal static class Transforms
    {
        public static ImageRecordModel TableEntity_To_ImageRecord(string cdnEndpoint, ImageRecordTableEntity imageRecordTableEntity, ImageFormatModel imageFormat, bool isGallery = false)
        {
            var imageRecord = new ImageRecordModel();

            
            imageRecord.Height = imageRecordTableEntity.Height;
            imageRecord.Width = imageRecordTableEntity.Width;

            if (!isGallery)
            {
                imageRecord.Type = "single";

                imageRecord.Title = imageRecordTableEntity.Title;
                imageRecord.Description = imageRecordTableEntity.Description;

                imageRecord.BlobPath = imageRecordTableEntity.BlobPath;
                imageRecord.Url = cdnEndpoint + imageRecordTableEntity.Url;
                imageRecord.FileName = imageRecordTableEntity.FileName;
                imageRecord.FilePath = imageRecordTableEntity.FilePath;

                imageRecord.BlobPath_sm = imageRecordTableEntity.BlobPath.Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
                imageRecord.Url_sm = cdnEndpoint + imageRecordTableEntity.Url.Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
                imageRecord.FileName_sm = imageRecordTableEntity.FileName.Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
                imageRecord.FilePath_sm = imageRecordTableEntity.FilePath.Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");

                imageRecord.BlobPath_xs = imageRecordTableEntity.BlobPath.Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                imageRecord.Url_xs = cdnEndpoint + imageRecordTableEntity.Url.Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                imageRecord.FileName_xs = imageRecordTableEntity.FileName.Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                imageRecord.FilePath_xs = imageRecordTableEntity.FilePath.Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
            }
            else
            {
                imageRecord.Type = "gallery";

                var blobPaths = imageRecordTableEntity.BlobPath.Split('|').ToList();

                var titles = imageRecordTableEntity.Title.Split('|').ToList();
                var descriptions = imageRecordTableEntity.Description.Split('|').ToList();
                var urls = imageRecordTableEntity.Url.Split('|').ToList();
                var fileNames = imageRecordTableEntity.FileName.Split('|').ToList();
                var filePaths = imageRecordTableEntity.FilePath.Split('|').ToList();

                if (urls.Count > 0)
                {
                    imageRecord.GalleryImages = new List<ImageRecordGalleryModel>();

                    for (int i = 0; i < urls.Count; i++)
                    {
                        var imageGalleryRecord = new ImageRecordGalleryModel
                        {
                            Url = cdnEndpoint + urls[i],
                            Url_sm = cdnEndpoint + urls[i].Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png"),
                            Url_xs = cdnEndpoint + urls[i].Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png")
                    };

                        try
                        {
                            imageGalleryRecord.Title = titles[i];
                            imageGalleryRecord.Description = descriptions[i];

                            imageGalleryRecord.BlobPath = blobPaths[i];
                            imageGalleryRecord.FileName = fileNames[i];
                            imageGalleryRecord.FilePath = filePaths[i];

                            imageGalleryRecord.BlobPath_sm = blobPaths[i].Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
                            imageGalleryRecord.FileName_sm = fileNames[i].Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
                            imageGalleryRecord.FilePath_sm = filePaths[i].Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");

                            imageGalleryRecord.BlobPath_xs = blobPaths[i].Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                            imageGalleryRecord.FileName_xs = fileNames[i].Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                            imageGalleryRecord.FilePath_xs = filePaths[i].Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");
                        }
                        catch
                        {

                        }

                        imageRecord.GalleryImages.Add(imageGalleryRecord);
                    }
                }
            }

            imageRecord.FormatName = imageFormat.ImageFormatName;
            imageRecord.FormatNameKey = imageFormat.ImageFormatNameKey;

            return imageRecord;
        }

        public static ImageRecordModel ImageFormat_To_ImageRecord(ImageFormatModel imageFormat)
        {
            var imageRecord = new ImageRecordModel();

            if (!imageFormat.Gallery)
            {
                imageRecord.Type = "single";
            }
            else
            {
                imageRecord.Type = "gallery";
            }

            imageRecord.Height = imageFormat.Height;
            imageRecord.Width = imageFormat.Width;

            imageRecord.FormatName = imageFormat.ImageFormatName;
            imageRecord.FormatNameKey = imageFormat.ImageFormatNameKey;

            return imageRecord;
        }
    }
}