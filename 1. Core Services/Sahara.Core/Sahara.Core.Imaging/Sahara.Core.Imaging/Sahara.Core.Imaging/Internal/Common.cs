using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Settings.Models.Imaging;
using System;
using System.Drawing;
using System.IO;

namespace Sahara.Core.Imaging
{
    internal static class Common
    {
        internal static MemoryStream GetAssetFromIntermediaryStorage(string containerName, string fileName)
        {
            //Bitmap sourceBitmap;

            CloudBlobClient blobClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.IntermediateStorage.CreateCloudBlobClient();

            //Create and set retry policy
            IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(500), 8);
            blobClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;


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

                //sourceBitmap = new Bitmap(ms);
            //}

            //return sourceBitmap;
        }

        internal static DataAccessResponseType VerifyCommonImageSpecifications(Bitmap bmpSource, ImageFormatModel imageModel)
        {

            var response = new DataAccessResponseType();

            System.Drawing.Imaging.ImageFormat format;

            #region Verify image formats

            try
            {
                format = bmpSource.RawFormat;
                if (!format.Equals(System.Drawing.Imaging.ImageFormat.Jpeg)
                    && !format.Equals(System.Drawing.Imaging.ImageFormat.Gif)
                    && !format.Equals(System.Drawing.Imaging.ImageFormat.Png)
                    && !format.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
                    //&& !format.Equals(System.Drawing.Imaging.ImageFormat.Tiff)) //<-- Tiff will be added in future 
                {
                    //File is not a supported image type, return error
                    response.isSuccess = false;
                    response.ErrorMessage = "Please use a supported image file format.";

                    return response;
                }
            }
            catch
            {
                //File is not an image, return error
                response.isSuccess = false;
                response.ErrorMessage = "Not a supported image file. Must be of type Jpeg, Gif, Png or Bmp.";     // or Tiff.";<-- Tiff will be added in future 

                return response;
            }

            #endregion

            #region Verify image minimum dimension settings

            if (imageModel.MinSize != null)
            {
                if (bmpSource.Width < imageModel.MinSize.X && bmpSource.Height < imageModel.MinSize.Y)
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Image must be at least " + imageModel.GetMinSize();

                    return response;
                }
                else if (bmpSource.Width < imageModel.MinSize.X)
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Image must be at least " + imageModel.MinSize.X + "pixels wide";

                    return response;
                }
                else if (bmpSource.Height < imageModel.MinSize.Y)
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Image must be at least " + imageModel.MinSize.Y + "pixels tall";

                    return response;
                }
            }


            #endregion

            #region Verify image maximum dimension settings

            //ToDo: Centralize this
            if (imageModel.MaxSize != null)
            {
                if (bmpSource.Width > imageModel.MaxSize.X && bmpSource.Height > imageModel.MaxSize.Y)
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Image must be smaller than " + imageModel.GetMaxSize();

                    return response;
                }
                else if (bmpSource.Width > imageModel.MaxSize.X)
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Image must be less than " + imageModel.MaxSize.X + "pixels wide";

                    return response;
                }
                else if (bmpSource.Height > imageModel.MaxSize.Y)
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Image must be less than " + imageModel.MaxSize.Y + "pixels tall";

                    return response;
                }

            }

            #endregion

            response.isSuccess = true;
            return response;
        }
    }
}
