using ImageProcessor;
using ImageProcessor.Imaging.Filters.Photo;
using ImageProcessor.Imaging.Formats;
using Imaging_API.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Imaging_API.Controllers
{
    [RoutePrefix("enhancement")]
    public class EnhancementController : ApiController
    {
        /// <summary>
        /// Generates a preview URL of image enhancements on source images in intermediary storage
        /// </summary>
        /// <param name="imageId"></param>
        /// <param name="containerName"></param>
        /// <param name="imageFormat"></param>
        /// <param name="brightness"></param>
        /// <param name="contrast"></param>
        /// <param name="saturation"></param>
        /// <param name="sharpness"></param>
        /// <param name="sepia"></param>
        /// <param name="polaroid"></param>
        /// <param name="greyscale"></param>
        /// <param name="tint"></param>
        /// <param name="color"></param>
        /// <returns></returns>

        [HttpGet]
        [Route("preview")]
        public ImagePreviewResponse Preview(string imageId, string containerName, string imageFormat, int brightness, int contrast, int saturation, int sharpness, bool sepia, bool polaroid, bool greyscale)
        {
            var response = new ImagePreviewResponse();

            //Create the id for the enhanced "preview" version of the source image
            string enhancedImageId = Guid.NewGuid().ToString();
            var outputFormatString = string.Empty;
            ISupportedImageFormat outputFormatProcessorProperty = null;
            System.Drawing.Imaging.ImageFormat outputFormatSystemrawingFormat = null;

            #region Select image format, or send error

            switch (imageFormat)
            {
                case "jpg":
                    outputFormatString = ".jpg";
                    outputFormatProcessorProperty = new JpegFormat { Quality = 90 };
                    outputFormatSystemrawingFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;

                case "gif":
                    outputFormatString = ".gif";
                    outputFormatProcessorProperty = new GifFormat { };
                    outputFormatSystemrawingFormat = System.Drawing.Imaging.ImageFormat.Gif;
                    break;

                case "png":
                    outputFormatString = ".png";
                    outputFormatProcessorProperty = new PngFormat { };
                    outputFormatSystemrawingFormat = System.Drawing.Imaging.ImageFormat.Png;
                    break;

                case "bmp":
                    outputFormatString = ".bmp";
                    outputFormatProcessorProperty = new BitmapFormat { };
                    outputFormatSystemrawingFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                    break;

                case "tiff":
                    outputFormatString = ".tiff";
                    outputFormatProcessorProperty = new TiffFormat { };
                    outputFormatSystemrawingFormat = System.Drawing.Imaging.ImageFormat.Tiff;
                    break;

                default:
                    return new ImagePreviewResponse { isSuccess = false, ErrorMessage = "Please please use a supported file format: jpg, png, gif, bmp or tiff." };
            }

            #endregion

            #region verify all input parameters, or send error

            if(
                brightness < -100 || brightness > 100 ||
                sharpness < -100 || sharpness > 100 ||
                contrast < -100 || contrast > 100 ||
                saturation < -100 || saturation > 100
                )
            {
                return new ImagePreviewResponse { isSuccess = false, ErrorMessage = "Please pass in a value between -100 and 100 for all enhancement parameters" };
            }

            #endregion

            //Build out response object
            response.ImageID = enhancedImageId;
            response.FileName = enhancedImageId + outputFormatString;
            response.SourceFile = imageId + outputFormatString;

            #region Connect to Intermediary Storage

            // Credentials are from centralized CoreServiceSettings
            StorageCredentials storageCredentials = new StorageCredentials(CoreServices.PlatformSettings.Storage.IntermediaryName, CoreServices.PlatformSettings.Storage.IntermediaryKey);
            var storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            #endregion


            try
            {
                //Pull down the source image as MemoryStream from Blob storage and apply image enhancement properties
                using (MemoryStream sourceStream = Common.GetAssetFromIntermediaryStorage(response.SourceFile, containerName, blobClient))
                {
                    #region Image Processor & Storage

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory())
                        {

                            // Load, resize, set the format and quality and save an image.
                            // Applies all in order...
                            sourceStream.Position = 0;
                            imageFactory.Load(sourceStream);

                            if (brightness != 0)
                            {
                                imageFactory.Brightness(brightness);
                            }
                            if (contrast != 0)
                            {
                                imageFactory.Contrast(contrast);
                            }
                            if (saturation != 0)
                            {
                                imageFactory.Saturation(saturation);
                            }

                            //Sharpness
                            if (sharpness != 0)
                            {
                                imageFactory.GaussianSharpen(sharpness);
                            }

                            //Filters
                            if(sepia == true)
                            {
                                imageFactory.Filter(MatrixFilters.Sepia);
                            }
                            if (polaroid == true)
                            {
                                imageFactory.Filter(MatrixFilters.Polaroid);
                            }
                            if (greyscale == true)
                            {
                                imageFactory.Filter(MatrixFilters.GreyScale);
                            }

                            imageFactory.Save(outStream);

                        }


                        //Store each image size to BLOB STORAGE ---------------------------------
                        #region BLOB STORAGE

                        //Creat/Connect to the Blob Container
                        //blobClient.GetContainerReference(containerName).CreateIfNotExists(BlobContainerPublicAccessType.Blob); //<-- Create and make public
                        CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);

                        //Get reference to the picture blob or create if not exists. 
                        CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(response.FileName);

                        //Save to storage
                        //Convert final BMP to byteArray
                        Byte[] finalByteArray;

                        finalByteArray = outStream.ToArray();

                        //Store the enhanced "preview" into the same container as the soure
                        blockBlob.UploadFromByteArray(finalByteArray, 0, finalByteArray.Length);

                        #endregion
                    }

                    #endregion
                }
            }
            catch(Exception e)
            {
                return new ImagePreviewResponse { isSuccess = false, ErrorMessage = e.Message };
            }

            //Marke as successful and return the new ImageID and FileName back to the client for previewing:
            response.isSuccess = true;
            return response;
        }
    }
}
