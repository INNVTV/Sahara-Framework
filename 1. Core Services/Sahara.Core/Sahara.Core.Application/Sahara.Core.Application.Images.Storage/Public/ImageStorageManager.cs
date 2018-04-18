using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Sahara.Core.Common.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Storage.Public
{
    public static class ImageStorageManager
    {
        public static DataAccessResponseType DeleteImageBlobs(string storagePartition, string containerName, string blobPath)
        {

            //CloudBlobClient blobClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudBlobClient();
            CloudBlobClient blobClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudBlobClient();

            //Create and set retry policy
            IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(400), 6);
            blobClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

            //Creat/Connect to the Blob Container
            blobClient.GetContainerReference(containerName).CreateIfNotExists(BlobContainerPublicAccessType.Blob); //<-- Create and make public
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);

            var blockBlobMainUri = blobPath;
            var blockBlobSMUri = blobPath.Replace(".jpg", "_sm.jpg").Replace(".gif", "_sm.gif").Replace(".png", "_sm.png");
            var blockBlobXSUri = blobPath.Replace(".jpg", "_xs.jpg").Replace(".gif", "_xs.gif").Replace(".png", "_xs.png");

            //Get references to the blobs. 
            CloudBlockBlob blockBlobMain = blobContainer.GetBlockBlobReference(blockBlobMainUri);
            CloudBlockBlob blockBlobSM = blobContainer.GetBlockBlobReference(blockBlobSMUri);
            CloudBlockBlob blockBlobXS = blobContainer.GetBlockBlobReference(blockBlobXSUri);

            try { blockBlobMain.Delete(); } catch { }
            try { blockBlobSM.Delete(); } catch { }
            try { blockBlobXS.Delete(); } catch { }

            return new DataAccessResponseType { isSuccess = true };

        }
    }
}
