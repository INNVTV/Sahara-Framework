using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Images.Records.TableEntities;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Records.Internal
{
    internal static class ImageRecordTableStorage
    {
        #region Keys

        internal static string ImageRecordTableName(string imageGroupTypeNameKey)
        {
            return imageGroupTypeNameKey + "imgs";
        }

        internal static string ImageRecordListingTableName(string imageGroupTypeNameKey)
        {
            return imageGroupTypeNameKey + "lstimgs";
        }

        #endregion

        #region Store/Append

        internal static DataAccessResponseType StoreImageRecord(string accountId, string storagePartition, string imageGroupTypeNameKey, string objectId, string imageGroupName, string imageGroupNameKey, string imageFormatName, string imageFormatNameKey, string title, string description, string url, string filename, string filepath, string containerName, string blobPath, int height, int width, bool isListing = false)
        {
            var response = new DataAccessResponseType();

            var imageRecordEntity = new ImageRecordTableEntity
            {

                //PartitionKey = objectId,
                //RowKey = imageGroupNameKey + "-" + imageFormatNameKey,

                ObjectId = objectId, //<-- Partitionkey
                ImageKey = imageGroupNameKey + "-" + imageFormatNameKey, //<-- RowKey

                ImageGroup = imageGroupName,
                ImageGroupKey = imageGroupNameKey,
                ImageFormat = imageFormatName,
                ImageFormatKey = imageFormatNameKey,

                Title = title,
                Description = description,

                Url = url,
                FileName = filename,
                FilePath = filepath,

                BlobPath = blobPath,
                ContainerName = containerName,

                Height = height,
                Width =width               
            };


            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy--------
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            
            TableOperation operation = TableOperation.InsertOrReplace((imageRecordEntity as TableEntity));

            string tableName = Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ImageRecordTableStorage.ImageRecordTableName(imageGroupTypeNameKey);  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages
            string listingTablename = Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ImageRecordTableStorage.ImageRecordListingTableName(imageGroupTypeNameKey);  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages

            if(tableName.Length > 63 || listingTablename.Length > 63)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Storage table names cannot be longer than 63 characters!" };
            }


            try
            {
                CloudTable cloudTable = cloudTableClient.GetTableReference(tableName);  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages
                cloudTable.CreateIfNotExists();
                var tableResult = cloudTable.Execute(operation);


                if(isListing)
                {
                    //If this is a listing, we also add a copy to the listing variation of the table
                    CloudTable cloudTable2 = cloudTableClient.GetTableReference(listingTablename);  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages
                    cloudTable2.CreateIfNotExists();
                    var tableResult2 = cloudTable2.Execute(operation);

                    response.isSuccess = true; //tableResult.;
                }
                else
                {
                    response.isSuccess = true;
                }

            }
            catch
            {
                response.isSuccess = false; //tableResult.;
                //response.ErrorMessage = "image exists";
            }


            return response;
        }
        
        internal static DataAccessResponseType AppendImageGalleryRecord(Account account, string imageGroupTypeNameKey, string objectId, string imageGroupName, string imageGroupNameKey, string imageFormatName, string imageFormatNameKey, string title, string description, string url, string filename, string filepath, string blobPath)
        {
            var response = new DataAccessResponseType();

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(account.StoragePartition).CreateCloudTableClient();

            //Create and set retry policy--------
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(account.AccountID.ToString()) + Internal.ImageRecordTableStorage.ImageRecordTableName(imageGroupTypeNameKey));  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages

            //Get the entity to update
            var imageRecordEntity = (from record in cloudTable.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId && p.ImageKey == imageGroupNameKey + "-" + imageFormatNameKey) select record).FirstOrDefault();


            //Validate maximum gallery images allowed:
            int currentCount = imageRecordEntity.Url.Split('|').Count();
            if(currentCount >= account.PaymentPlan.MaxImagesPerGallery)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached your account limit of " + account.PaymentPlan.MaxImagesPerGallery + " images per gallery." };
            }


            //Append to image data
            imageRecordEntity.BlobPath = imageRecordEntity.BlobPath + "|" + blobPath;
            imageRecordEntity.FileName = imageRecordEntity.FileName + "|" + filename;
            imageRecordEntity.FilePath = imageRecordEntity.FilePath + "|" + filepath;
            imageRecordEntity.Url = imageRecordEntity.Url + "|" + url;
            imageRecordEntity.Title = imageRecordEntity.Title + "|" + title;
            imageRecordEntity.Description = imageRecordEntity.Description + "|" + description;

            TableOperation operation = TableOperation.Replace((imageRecordEntity as TableEntity));

            try
            {
                
                //cloudTable.CreateIfNotExists();
                var tableResult = cloudTable.Execute(operation);
                response.isSuccess = true;

            }
            catch
            {
                response.isSuccess = false; //tableResult.;
            }


            return response;
        }

        #endregion

        #region Retrieve

        /// <summary>
        /// Retreive all image records for an image group type (essentially the entire table)
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="imageGroupTypeNameKey"></param>
        /// <returns></returns>
        internal static IEnumerable<ImageRecordTableEntity> RetrieveImageRecords(string accountId, string storagePartition, string imageFormatGroupTypeNameKey)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + ImageRecordTableName(imageFormatGroupTypeNameKey));

            //cloudTable.CreateIfNotExists();

            TableQuery<ImageRecordTableEntity> query = new TableQuery<ImageRecordTableEntity>();

            return cloudTable.ExecuteQuery(query);
        }

        /// <summary>
        /// Retreive all image records for an object
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="imageGroupTypeNameKey"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        internal static List<ImageRecordTableEntity> RetrieveImageRecords(string accountId, string storagePartition, string imageFormatGroupTypeNameKey, string objectId, bool listingsOnly = false)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = null;
            
            if(!listingsOnly)
            {
                cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + ImageRecordTableName(imageFormatGroupTypeNameKey));
            }
            else
            {
                cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + ImageRecordListingTableName(imageFormatGroupTypeNameKey));
            }

            //cloudTable.CreateIfNotExists();

            var imageRecordEntities = new List<ImageRecordTableEntity>();

            try
            {
                imageRecordEntities = (from record in cloudTable.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId) select record).ToList(); //new TableQuery<ImageRecordTableEntity>().AsQueryable().Where(p => p.PartitionKey == objectId).ToList();
            }
            catch
            {

            }

            //List<ImageRecordTableEntity> imageRecordEntities = cloudTable.ExecuteQuery(query);

            //List<ImageRecordTableEntity> imageRecordEntities = new TableQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId).ToList();
            //var imageRecordEntities = new TableQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId);

            /*
            if(imageRecordEntities != null)
            {
                return imageRecordEntities.ToList();
            }
            else
            {
                return new List<ImageRecordTableEntity>();
            }*/

            return imageRecordEntities;

        }

        internal static ImageRecordTableEntity RetrieveAnyImageRecordForFormatKey(string accountId, string storagePartition, string imageFormatGroupTypeNameKey, string imageRecordKey)
        {
            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = null;

            ImageRecordTableEntity imageRecordEntity = null;

            //We do not use the listing table as any duplicate records that are deleted in the "Main" table will also not exist there.
            cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + ImageRecordTableName(imageFormatGroupTypeNameKey));

            //var context = cloudTable.ServiceClient.GetTableServiceContext();
            //context.IgnoreResourceNotFoundException = true; //<--

            //cloudTableClient.DefaultRequestOptions.

            try
            {
                imageRecordEntity = (from record in cloudTable.CreateQuery<ImageRecordTableEntity>().Where(p => p.RowKey == imageRecordKey) select record).FirstOrDefault(); //new TableQuery<ImageRecordTableEntity>().AsQueryable().Where(p => p.PartitionKey == objectId).ToList();
            }
            catch(Exception e)
            {
                if(e.InnerException.Message.Contains("404"))
                {
                    imageRecordEntity = null; //<-- This particular exception ACTUALLY means that we checkd successfully BUT no records exist.
                }
                else
                {
                    imageRecordEntity = new ImageRecordTableEntity();//<-- We new up the object so that we don't unsafely delete a format in case of connection issues with Table Storage
                }
                
                
            }
           
            return imageRecordEntity;

        }


        #endregion

        #region Update

        internal static DataAccessResponseType UpdateImageRecordTitle(string accountId, string storagePartition, string imageGroupTypeNameKey, string objectId, string imageGroupNameKey, string imageFormatNameKey, string newTitle, bool isListing)
        {
            var response = new DataAccessResponseType();


            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy--------
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            #region Process on master image record

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ImageRecordTableStorage.ImageRecordTableName(imageGroupTypeNameKey));  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages

            //Get the entity to remove gallery index item from
            var imageRecordEntity = (from record in cloudTable.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId && p.ImageKey == imageGroupNameKey + "-" + imageFormatNameKey) select record).FirstOrDefault();


            imageRecordEntity.Title = newTitle;

            //Replace the record
            TableOperation operation = TableOperation.Replace((imageRecordEntity as TableEntity));

            try
            {

                //cloudTable.CreateIfNotExists();
                var tableResult = cloudTable.Execute(operation);
                response.isSuccess = true;

            }
            catch (Exception e)
            {
                var exceptionMessage = e.Message;
                response.isSuccess = false;
                return null;
            }


            #endregion


            #region Process on Listing image (if applicable)

            if(isListing)
            {
                CloudTable cloudTable2 = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ImageRecordTableStorage.ImageRecordListingTableName(imageGroupTypeNameKey));  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages

                //Get the entity to remove gallery index item from
                var imageRecordEntity2 = (from record in cloudTable2.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId && p.ImageKey == imageGroupNameKey + "-" + imageFormatNameKey) select record).FirstOrDefault();


                imageRecordEntity2.Title = newTitle;

                //Replace the record
                TableOperation operation2 = TableOperation.Replace((imageRecordEntity2 as TableEntity));

                try
                {

                    //cloudTable.CreateIfNotExists();
                    var tableResult2 = cloudTable2.Execute(operation2);
                    response.isSuccess = true;

                }
                catch(Exception e)
                {
                    var exceptionMessage = e.Message;
                    response.isSuccess = false;
                }
            }

            #endregion



            return response;
        }

        internal static DataAccessResponseType UpdateImageRecordDescription(string accountId, string storagePartition, string imageGroupTypeNameKey, string objectId, string imageGroupNameKey, string imageFormatNameKey, string newDescription, bool isListing)
        {
            var response = new DataAccessResponseType();

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy--------
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


            #region Process on master image record

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ImageRecordTableStorage.ImageRecordTableName(imageGroupTypeNameKey));  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages

            //Get the entity to remove gallery index item from
            var imageRecordEntity = (from record in cloudTable.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId && p.ImageKey == imageGroupNameKey + "-" + imageFormatNameKey) select record).FirstOrDefault();


            imageRecordEntity.Description = newDescription;

            //Replace the record
            TableOperation operation = TableOperation.Replace((imageRecordEntity as TableEntity));

            try
            {

                //cloudTable.CreateIfNotExists();
                var tableResult = cloudTable.Execute(operation);
                response.isSuccess = true;

            }
            catch (Exception e)
            {
                var exceptionMessage = e.Message;
                response.isSuccess = false; //tableResult.;
                return null;
            }

            #endregion


            #region Process on Listing image (if applicable)

            if (isListing)
            {
                CloudTable cloudTable2 = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ImageRecordTableStorage.ImageRecordListingTableName(imageGroupTypeNameKey));  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages

                //Get the entity to remove gallery index item from
                var imageRecordEntity2 = (from record in cloudTable2.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId && p.ImageKey == imageGroupNameKey + "-" + imageFormatNameKey) select record).FirstOrDefault();


                imageRecordEntity2.Description = newDescription;

                //Replace the record
                TableOperation operation2 = TableOperation.Replace((imageRecordEntity2 as TableEntity));

                try
                {

                    //cloudTable.CreateIfNotExists();
                    var tableResult2 = cloudTable2.Execute(operation2);
                    response.isSuccess = true;

                }
                catch (Exception e)
                {
                    var exceptionMessage = e.Message;
                    response.isSuccess = false;
                }
            }

            #endregion





            return response;
        }

        internal static DataAccessResponseType UpdateImageRecordGalleryTitle(string accountId, string storagePartition, string imageGroupTypeNameKey, string objectId, string imageGroupNameKey, string imageFormatNameKey, int imageIndex, string newTitle)
        {
            var response = new DataAccessResponseType();

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy--------
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ImageRecordTableStorage.ImageRecordTableName(imageGroupTypeNameKey));  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages

            //Get the entity to remove gallery index item from
            var imageRecordEntity = (from record in cloudTable.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId && p.ImageKey == imageGroupNameKey + "-" + imageFormatNameKey) select record).FirstOrDefault();

            //through each item, convert to array, update the index & serialize back
            var titleArray = imageRecordEntity.Title.Split('|').ToList();
            titleArray[imageIndex] = newTitle;
            imageRecordEntity.Title = String.Join("|", titleArray);

            //Replace the record
            TableOperation operation = TableOperation.Replace((imageRecordEntity as TableEntity));

            try
            {

                //cloudTable.CreateIfNotExists();
                var tableResult = cloudTable.Execute(operation);
                response.isSuccess = true;

            }
            catch
            {
                response.isSuccess = false; //tableResult.;
            }


            return response;
        }

        internal static DataAccessResponseType UpdateImageRecordGalleryDescription(string accountId, string storagePartition, string imageGroupTypeNameKey, string objectId, string imageGroupNameKey, string imageFormatNameKey, int imageIndex, string newDescription)
        {
            var response = new DataAccessResponseType();

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy--------
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ImageRecordTableStorage.ImageRecordTableName(imageGroupTypeNameKey));  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages

            //Get the entity to remove gallery index item from
            var imageRecordEntity = (from record in cloudTable.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId && p.ImageKey == imageGroupNameKey + "-" + imageFormatNameKey) select record).FirstOrDefault();

            //through each item, convert to array, update the index & serialize back
            var descriptionArray = imageRecordEntity.Description.Split('|').ToList();
            descriptionArray[imageIndex] = newDescription;
            imageRecordEntity.Description = String.Join("|", descriptionArray);

            //Replace the record
            TableOperation operation = TableOperation.Replace((imageRecordEntity as TableEntity));

            try
            {

                //cloudTable.CreateIfNotExists();
                var tableResult = cloudTable.Execute(operation);
                response.isSuccess = true;

            }
            catch
            {
                response.isSuccess = false; //tableResult.;
            }


            return response;
        }

        #endregion


        #region Reorder

        internal static DataAccessResponseType ReorderGalleryImages(string accountId, string storagePartition, string imageGroupTypeNameKey, string objectId, string imageGroupNameKey, string imageFormatNameKey, List<int> imageIndexOrder)
        {
            var response = new DataAccessResponseType();

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy--------
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ImageRecordTableStorage.ImageRecordTableName(imageGroupTypeNameKey));  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages

            //Get the entity to remove gallery index item from
            var imageRecordEntity = (from record in cloudTable.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId && p.ImageKey == imageGroupNameKey + "-" + imageFormatNameKey) select record).FirstOrDefault();

            //through each item, convert to array
            var _blobPathArray = imageRecordEntity.BlobPath.Split('|').ToList();
            var _fileNameArray = imageRecordEntity.FileName.Split('|').ToList();
            var _filePathArray = imageRecordEntity.FilePath.Split('|').ToList();
            var _urlArray = imageRecordEntity.Url.Split('|').ToList();
            var _titleArray = imageRecordEntity.Title.Split('|').ToList();
            var _descriptionArray = imageRecordEntity.Description.Split('|').ToList();

            //Ensure that new index array count matches amount of images currently in this gallery
            if (_blobPathArray.Count != imageIndexOrder.Count)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "New gallery order count does not match amount of images in gallery" };
            }

            //Create new versions of the arrays
            var blobPathArray = new List<string>();
            var fileNameArray = new List<string>();
            var filePathArray = new List<string>();
            var urlArray = new List<string>();
            var titleArray = new List<string>();
            var descriptionArray = new List<string>();

            //Reorder items into the new arrays
            foreach(int index in imageIndexOrder)
            {
                blobPathArray.Add(_blobPathArray.ElementAt(index));
                fileNameArray.Add(_fileNameArray.ElementAt(index));
                filePathArray.Add(_filePathArray.ElementAt(index));
                urlArray.Add(_urlArray.ElementAt(index));
                titleArray.Add(_titleArray.ElementAt(index));
                descriptionArray.Add(_descriptionArray.ElementAt(index));
            }

            //Serialize back to the entity
            imageRecordEntity.BlobPath = String.Join("|", blobPathArray);
            imageRecordEntity.FileName = String.Join("|", fileNameArray);
            imageRecordEntity.FilePath = String.Join("|", filePathArray);
            imageRecordEntity.Url = String.Join("|", urlArray);
            imageRecordEntity.Title = String.Join("|", titleArray);
            imageRecordEntity.Description = String.Join("|", descriptionArray);

            //Replace the record
            TableOperation operation = TableOperation.Replace((imageRecordEntity as TableEntity));

            try
            {
                //cloudTable.CreateIfNotExists();
                var tableResult = cloudTable.Execute(operation);
                response.isSuccess = true;
            }
            catch
            {
                response.isSuccess = false; //tableResult.;
            }

            return response;
        }


        #endregion


        #region Deletions

        /// <summary>
        /// Deletes all image records for an object in table storage
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="imageGroupTypeNameKey"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        internal static DataAccessResponseType DeleteAllImageRecordsForObject(string accountId, string storagePartition, string imageFormatGroupTypeNameKey, string objectId)
        {
            var response = new DataAccessResponseType();

            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;



            try
            {
                //Delete From MAIN table
                CloudTable cloudTable1 = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + ImageRecordTableName(imageFormatGroupTypeNameKey));
                //cloudTable1.CreateIfNotExists();
                List<TableEntity> productImageEntities1 = cloudTable1.CreateQuery<TableEntity>().Where(p => p.PartitionKey == objectId).ToList();

                foreach (TableEntity productImageEntity in productImageEntities1)
                {
                    cloudTable1.Execute(TableOperation.Delete(productImageEntity));
                }

                //Delete From LISTING table
                CloudTable cloudTable2 = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + ImageRecordListingTableName(imageFormatGroupTypeNameKey));
                //cloudTable2.CreateIfNotExists();
                List<TableEntity> productImageEntities2 = cloudTable2.CreateQuery<TableEntity>().Where(p => p.PartitionKey == objectId).ToList();

                foreach (TableEntity productImageEntity in productImageEntities2)
                {
                    cloudTable2.Execute(TableOperation.Delete(productImageEntity));
                }

                response.isSuccess = true;
            }
            catch (Exception e)
            {
                if(!e.Message.Contains("(404) Not Found"))
                {
                    PlatformLogManager.LogActivity(
                        CategoryType.Error,
                        ActivityType.Error_Exception,
                        e.Message,
                        "Exception while attempting to delete all image records for " + imageFormatGroupTypeNameKey + " '" + objectId + "' in table storage",
                        accountId,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                        null
                    );
                }
            }

            return response;
        }

        /// <summary>
        /// Deletes a single image record from table storage
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="objectId"></param>
        /// <param name="imageGroupTypeNameKey"></param>
        /// <param name="imageGroupNameKey"></param>
        /// <param name="imageFormatNameKey"></param>
        /// <returns></returns>
        internal static DataAccessResponseType DeleteImageRecord(string accountId, string storagePartition, string imageFormatGroupTypeNameKey, string objectId, string imageGroupNameKey, string imageFormatNameKey)
        {
            var response = new DataAccessResponseType();

            var imageKey = imageGroupNameKey + "-" + imageFormatNameKey;

            //CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;



            try
            {
                //Delete from MAIN table
                CloudTable cloudTable1 = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + ImageRecordTableName(imageFormatGroupTypeNameKey));
                //cloudTable1.CreateIfNotExists();
                TableEntity productImageEntity1 = cloudTable1.CreateQuery<TableEntity>().Where(p => p.PartitionKey == objectId && p.RowKey == imageKey).FirstOrDefault();
                cloudTable1.Execute(TableOperation.Delete(productImageEntity1));


                //Delete from LISTINGS table (We allow this to fail silently in case this is not a listing image
                try
                {
                    CloudTable cloudTable2 = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + ImageRecordListingTableName(imageFormatGroupTypeNameKey));
                    //cloudTable2.CreateIfNotExists();
                    TableEntity productImageEntity2 = cloudTable2.CreateQuery<TableEntity>().Where(p => p.PartitionKey == objectId && p.RowKey == imageKey).FirstOrDefault();
                    cloudTable2.Execute(TableOperation.Delete(productImageEntity2));
                }
                catch
                {

                }

                response.isSuccess = true;
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(
                    CategoryType.Error,
                    ActivityType.Error_Exception,
                    e.Message,
                   "Exception while attempting to delete image record/rowKey'" + imageKey + "' for " + imageFormatGroupTypeNameKey + " '" + objectId + "' in table storage",
                    accountId,
                    null,
                    null,
                    null,
                    null,
                    null,
                    System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                    null
                );
            }

            return response;
        }

        internal static DataAccessResponseType DeleteImageGalleryRecordItem(string accountId, string storagePartition, string imageGroupTypeNameKey, string objectId, string imageGroupNameKey, string imageFormatNameKey, int imageIndex)
        {
            var response = new DataAccessResponseType();

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy--------
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + Internal.ImageRecordTableStorage.ImageRecordTableName(imageGroupTypeNameKey));  //<-- accxxxxxproductimages / accxxxxxcategoryimages  / accxxxxxaccountimages

            //Get the entity to remove gallery index item from
            var imageRecordEntity = (from record in cloudTable.CreateQuery<ImageRecordTableEntity>().Where(p => p.PartitionKey == objectId && p.ImageKey == imageGroupNameKey + "-" + imageFormatNameKey) select record).FirstOrDefault();


            //through each item, convert to array, remove index & serialize back
            var blobPathArray = imageRecordEntity.BlobPath.Split('|').ToList();
            blobPathArray.RemoveAt(imageIndex);
            imageRecordEntity.BlobPath = String.Join("|", blobPathArray);


            //through each item, convert to array, remove index & serialize back
            var fileNameArray = imageRecordEntity.FileName.Split('|').ToList();
            fileNameArray.RemoveAt(imageIndex);
            imageRecordEntity.FileName = String.Join("|", fileNameArray);


            //through each item, convert to array, remove index & serialize back
            var filePathArray = imageRecordEntity.FilePath.Split('|').ToList();
            filePathArray.RemoveAt(imageIndex);
            imageRecordEntity.FilePath = String.Join("|", filePathArray);

            //through each item, convert to array, remove index & serialize back
            var urlArray = imageRecordEntity.Url.Split('|').ToList();
            urlArray.RemoveAt(imageIndex);
            imageRecordEntity.Url = String.Join("|", urlArray);


            //through each item, convert to array, remove index & serialize back
            var titleArray = imageRecordEntity.Title.Split('|').ToList();
            titleArray.RemoveAt(imageIndex);
            imageRecordEntity.Title = String.Join("|", titleArray);

            //through each item, convert to array, remove index & serialize back
            var descriptionArray = imageRecordEntity.Description.Split('|').ToList();
            descriptionArray.RemoveAt(imageIndex);
            imageRecordEntity.Description = String.Join("|", descriptionArray); 


            //Replace the record
            TableOperation operation = TableOperation.Replace((imageRecordEntity as TableEntity));

            try
            {
                
                //cloudTable.CreateIfNotExists();
                var tableResult = cloudTable.Execute(operation);
                response.isSuccess = true;

            }
            catch
            {
                response.isSuccess = false; //tableResult.;
            }


            return response;
        }

        #endregion



    }
}
