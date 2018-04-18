using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Sahara.Core.Common.ResponseTypes;
using System;
using System.Drawing;
using System.IO;

namespace Sahara.Core.Imaging
{
    public class PlatformUserProfilePhotoProcessor
    {
        
        /// <summary>
        /// For platform users we auto crop to profile photo specs, platform user does not use cropping tools
        /// </summary>
        /// <param name="imageByteArray"></param>
        /// <returns></returns>
        public DataAccessResponseType ProcessPlatformUserProfilePhoto(byte[] imageByteArray)
        {
            //Create image id and response object
            string imageId = System.Guid.NewGuid().ToString(); //<-- Stored on "Photo" column of user table
            var response = new DataAccessResponseType();

            var imageSpecs = Sahara.Core.Settings.Imaging.Images.PlatformUserProfilePhoto;
            var setSizes = imageSpecs.SetSizes;
            var containerName = imageSpecs.ParentName;

            //Image Format Settings
            var outputFormatString = ".jpg";
            ISupportedImageFormat outputFormatProcessorProperty = new JpegFormat { Quality = 90 }; // <-- Format is automatically detected though can be changed.
            var outputFormatSystemrawingFormat = System.Drawing.Imaging.ImageFormat.Jpeg;

            Bitmap bmpSource;

            //Convert source byte[] to MemoryStream
            using (var sourceStream = new MemoryStream(imageByteArray))
            {
                bmpSource = new Bitmap(sourceStream); //<--Convert to BMP in order to run verifications
                
                //Verifiy Image Settings Align With Requirements
                var verifySpecsResponse = Common.VerifyCommonImageSpecifications(bmpSource, imageSpecs);
                if (!verifySpecsResponse.isSuccess)
                {
                    //return if fails
                    return verifySpecsResponse;
                }


                //TO DO: IF ASPECT RATIO EXISTS THEN WE DETRIMINE THE SET SIZE (1 only) by method

                //Loop through all image sizes, resize & save a version for each one
                foreach (var size in setSizes)
                {

                    //Final location for EACH image will be: http://[uri]/[containerName]/[imageId]_[w]x[h].[format]

                    var filename = imageId + "_" + size.GetSize() + outputFormatString; //<-- guid_12x24.xxx

                    #region Image Processor & Storage

                    Size processingSize = new Size(size.X, size.Y);
                    var resizeLayer = new ResizeLayer(processingSize) { ResizeMode = ResizeMode.Crop, AnchorPosition = AnchorPosition.Center };


                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory())
                        {
                            //var cropLayer = new ImageProcessor.Imaging.CropLayer(0,0,885,700,CropMode.Pixels);

                            // Load, resize, set the format and quality and save an image.
                            // Applies all in order...
                            sourceStream.Position = 0;
                            imageFactory.Load(sourceStream)
                                        .Resize(resizeLayer) //<-- Then resize
                                        .Format(outputFormatProcessorProperty)
                                        .Save(outStream);
                        }


                        //Store each image size to BLOB STORAGE ---------------------------------
                        #region BLOB STORAGE

                        CloudBlobClient blobClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudBlobClient();

                        //Create and set retry policy
                        IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(400), 6);
                        blobClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

                        //Creat/Connect to the Blob Container
                        blobClient.GetContainerReference(containerName).CreateIfNotExists(BlobContainerPublicAccessType.Blob); //<-- Create and make public
                        CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);

                        //Get reference to the picture blob or create if not exists. 
                        CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);

                        //Save to storage
                        //Convert final BMP to byteArray
                        Byte[] finalByteArray;

                        finalByteArray = outStream.ToArray();

                        blockBlob.UploadFromByteArray(finalByteArray, 0, finalByteArray.Length);

                        #endregion
                    }


                    #endregion
                }
            }

            

            //Send the imageId in the response object
            response.isSuccess = true;
            response.SuccessCode = imageId;
            response.SuccessMessage = imageId;
            return response;
        }

    }
}
