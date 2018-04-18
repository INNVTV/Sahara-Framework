using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Imaging_API
{
    public static class Common
    {
        public static string SharedClientKey = "";

        internal static MemoryStream GetAssetFromIntermediaryStorage(string fileName, string containerName, CloudBlobClient blobClient)
        {

            //Creat/Connect to the Blob Container
            blobClient.GetContainerReference(containerName).CreateIfNotExists(BlobContainerPublicAccessType.Blob); //<-- Create and make public
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);

            //Get reference to the picture blob or create if not exists. 
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);

            //using(var ms = new MemoryStream())
            //{ 
            var ms = new MemoryStream();
            blockBlob.DownloadToStream(ms);

            return ms;

        }
    }
}