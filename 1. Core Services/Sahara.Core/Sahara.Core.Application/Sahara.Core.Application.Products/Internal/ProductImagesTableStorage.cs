//using Microsoft.WindowsAzure.Storage.RetryPolicies;
//using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Products.TableEntities;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Products.Internal
{
    /* Depricated
    public static class ProductImagesTableStorage
    {
        internal static string ProductImagesTableName = "productimages";

        public static DataAccessResponseType StoreProductImage(ProductImageTableEntity productImageEntity)
        {
            var response = new DataAccessResponseType();

            CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            TableOperation operation = TableOperation.Insert((productImageEntity as TableEntity));

            try
            {
                var tableResult = productImageEntity.cloudTable.Execute(operation);
                response.isSuccess = true; //tableResult.;
            }
            catch
            {
                response.isSuccess = false; //tableResult.;
                response.ErrorMessage = "Product image exists";
            }


            return response;
        }

        internal static IEnumerable<ProductImageTableEntity> RetrieveProductImages(string accountId)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + ProductImagesTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<ProductImageTableEntity> query = new TableQuery<ProductImageTableEntity>();

            return cloudTable.ExecuteQuery(query);
        }


        /// <summary>
        /// Retreive all image records for a product id
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static List<ProductImageTableEntity> RetrieveProductImages(string accountId, string productId)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + ProductImagesTableName);

            cloudTable.CreateIfNotExists();

            List<ProductImageTableEntity> productImageEntities = new TableQuery<ProductImageTableEntity>().Where(p => p.PartitionKey == productId).ToList();

            return productImageEntities;
        }

        /// <summary>
        /// Deletes all image records for a productId in table storage
        /// </summary>
        /// <param name="account"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        internal static DataAccessResponseType DeleteProductImages(Account account, string productId)
        {
            var response = new DataAccessResponseType();

            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(account.AccountID.ToString()) + ProductImagesTableName);

            cloudTable.CreateIfNotExists();

            try
            {

                var e = new TableEntity() { PartitionKey = "foo", RowKey = "bar", ETag = "*" };
                cloudTable.Execute(TableOperation.Delete(e));

                List<TableEntity> productImageEntities = cloudTable.CreateQuery<TableEntity>().Where(p => p.PartitionKey == productId).ToList();

                foreach(ProductImageTableEntity productImageEntity in productImageEntities)
                {
                    cloudTable.Execute(TableOperation.Delete(productImageEntity));
                }
                
                response.isSuccess = true;
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(
                    CategoryType.Error,
                    ActivityType.Error_Exception,
                    e.Message,
                    "Exception while attempting to delete all image records for product '" + productId + "' in table storage",
                    account.AccountID.ToString(),
                    account.AccountName,
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

        /// <summary>
        /// Deletes a single product image record from table storage
        /// </summary>
        /// <param name="account"></param>
        /// <param name="productId"></param>
        /// <param name="imageGroupNameKey"></param>
        /// <param name="imageFormatNameKey"></param>
        /// <returns></returns>
        internal static DataAccessResponseType DeleteProductImage(Account account, string productId, string imageGroupNameKey, string imageFormatNameKey)
        {
            var response = new DataAccessResponseType();

            var rowKey = imageGroupNameKey + "-" + imageFormatNameKey;

            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.AccountIdToTableStorageName(account.AccountID.ToString()) + ProductImagesTableName);

            cloudTable.CreateIfNotExists();

            try
            {
                TableEntity productImageEntity = cloudTable.CreateQuery<TableEntity>().Where(p => p.PartitionKey == productId && p.RowKey == rowKey).FirstOrDefault();

                cloudTable.Execute(TableOperation.Delete(productImageEntity));
                
                response.isSuccess = true;
            }
            catch (Exception e)
            {
                PlatformLogManager.LogActivity(
                    CategoryType.Error,
                    ActivityType.Error_Exception,
                    e.Message,
                   "Exception while attempting to delete image record/rowKey'" + rowKey + "' for product '" + productId + "' in table storage",
                    account.AccountID.ToString(),
                    account.AccountName,
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
    }

    */
}
