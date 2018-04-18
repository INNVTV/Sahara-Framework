using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.Photo;
using ImageProcessor.Imaging.Formats;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Imaging.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Sahara.Core.Imaging
{

    public class ApplicationImageProcessor
    {
        public DataAccessResponseType ProcessApplicationImage(string accountId, string storagePartition, string sourceContainerName, string sourceFileName, int formatWidth, int formatHeight, string folderName, string type, int quality, ImageCropCoordinates imageCropCoordinates, ImageEnhancementInstructions imageEnhancementInstructions)
        {

            //Create image id and response object
            string imageId = System.Guid.NewGuid().ToString();
            var response = new DataAccessResponseType();

            //var imageSpecs = Sahara.Core.Settings.Imaging.Images.ApplicationImage;
            //var setSizes = imageSpecs.SetSizes;
            //var folderName = imageSpecs.ParentName;


            //-------------------------------------------------
            // REFACTORING NOTES
            //-------------------------------------------------
            // In this scenario we have ApplicationImages hard coded to 600x300 for the LARGE size. We then create a medium and small dynmically (as well as a thumbnail hardcoded to 96px tall)(specific sizes can be factored in if required). 
            // In extended scenarios we can pass these specs from CoreServices to clients via a call or from a Redis Cache or WCF fallback.
            // In scenarios where accounts have "Flexible Schemas" we can retreive these properties from a Redis or WCF call for each account based on that accounts set properties.
            //-----------------------------------------------------------------------------


            //var largeSize = new ApplicationImageSize{X=600, Y=300, Name="Large", NameKey = "large"};
            //var mediumSize = new ApplicationImageSize { X = Convert.ToInt32(largeSize.X / 1.25), Y = Convert.ToInt32(largeSize.Y / 1.25), Name = "Medium", NameKey = "medium" }; //<-- Med is 1/1.25 of large
            //var smallSize = new ApplicationImageSize { X = largeSize.X / 2, Y = largeSize.Y / 2, Name = "Small", NameKey = "small" }; //<-- Small is 1/2 of large

            //We calculate thumbnail width based on a set height of 96px.
            //Must be a Double and include .0 in calculation or will ALWAYS calculate to 0!
            //double calculatedThumbnailWidth = ((96.0 / largeSize.Y) * largeSize.X); //<-- Must be a Double and include .0 or will calculate to 0!

            //var thumbnailSize = new ApplicationImageSize { X = (int)calculatedThumbnailWidth, Y = 96, Name = "Thumbnail", NameKey = "thumbnail" }; //<-- Thumbnail is 96px tall, width is calculated based on this

            //We are going to loop through each size during processing
            //var setSizes = new List<ApplicationImageSize> { largeSize, mediumSize, smallSize, thumbnailSize };

            //Folder name within Blob storage for application image collection
            //var folderName = "applicationimages"; //" + imageId; //<-- Each image lives within a folder named after it's id: /applicationimages/[imageid]/[size].jpg

            //Image Format Settings
            var outputFormatString = "." + type;


            
            ISupportedImageFormat outputFormatProcessorProperty; // = new JpegFormat { Quality = 90 }; // <-- Format is automatically detected though can be changed.
            //ImageFormat outputFormatSystemrawingFormat; // = System.Drawing.Imaging.ImageFormat.Jpeg;

                        
            if (type == "gif")
            {
                outputFormatProcessorProperty = new GifFormat { Quality = quality }; // <-- Format is automatically detected though can be changed.
                //outputFormatSystemrawingFormat = System.Drawing.Imaging.ImageFormat.Gif;
            }
            else if (type == "png")
            {
                outputFormatProcessorProperty = new PngFormat { Quality = quality }; // <-- Format is automatically detected though can be changed.
                //outputFormatSystemrawingFormat = System.Drawing.Imaging.ImageFormat.Png;
            }
            else
            {
                outputFormatProcessorProperty = new JpegFormat { Quality = quality }; // <-- Format is automatically detected though can be changed.
                //outputFormatSystemrawingFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            }

            /**/

            //TO DO: IF ASPECT RATIO EXISTS THEN WE DETRIMINE THE SET SIZE (1 only) by method



            //Create instance of ApplicationImageDocumentModel to store image details into DocumentDB
            // var applicationImageDocumentModel = new ApplicationImageDocumentModel();
            //applicationImageDocumentModel.Id = imageId;
            // applicationImageDocumentModel.FilePath = folderName; //<--Concatenates with each FileName for fil location

            //Note: We store all images in a single folder to make deletions more performant

            // applicationImageDocumentModel.FileNames = new ApplicationImageFileSizes();


            //Get source file as MemoryStream from Blob storage and apply image processing tasks to each spcification
            using (MemoryStream sourceStream = Common.GetAssetFromIntermediaryStorage(sourceContainerName, sourceFileName))
            {
                //var sourceBmp = new Bitmap(sourceStream); //<--Convert to BMP in order to run verifications

                //Verifiy Image Settings Align With Requirements
                /*
                var verifySpecsResponse = Common.VerifyCommonImageSpecifications(sourceBmp, imageSpecs);
                if (!verifySpecsResponse.isSuccess)
                {
                    //return if fails
                    return verifySpecsResponse;
                }*/


                //Assign properties to the DocumentDB Document representation of this image
                //applicationImageDocumentModel.FileNames.Large = "large" + outputFormatString;
                //applicationImageDocumentModel.FileNames.Medium = "medium" + outputFormatString;
                //applicationImageDocumentModel.FileNames.Small = "small" + outputFormatString;
                //a/pplicationImageDocumentModel.FileNames.Thumbnail = "thumbnail" + outputFormatString;

                //Create 3 sizes (org, _sm & _xs)
                //List<Size> imageSizes = new List<Size>();

                Dictionary<Size, string> imageSizes = new Dictionary<Size, string>();

                imageSizes.Add(new Size(formatWidth, formatHeight), ""); //<-- ORG (no apennded file name)
                imageSizes.Add(new Size(formatWidth/2, formatHeight/2), "_sm"); //<-- SM (Half Size)
                imageSizes.Add(new Size(formatWidth/4, formatHeight/4), "_xs"); //<-- XS (Quarter Size)

                foreach(KeyValuePair<Size, string> entry in imageSizes)
                {
                    Size processingSize = entry.Key;
                    string appendedFileName = entry.Value;

                    #region Image Processor & Storage

                    //Final location for EACH image will be: http://[uri]/[containerName]/[imageId]_[size].[format]

                    var filename = imageId + appendedFileName + outputFormatString; //<-- [imageid][_xx].xxx
                    var fileNameFull = folderName + "/" + filename; //<-- /applicationimages/[imageid][_xx].xxx

                    //Size processingSize = new Size(formatWidth, formatHeight);
                    var resizeLayer = new ResizeLayer(processingSize) { ResizeMode = ResizeMode.Crop, AnchorPosition = AnchorPosition.Center };


                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory())
                        {

                            // Load, resize, set the format and quality and save an image.
                            // Applies all in order...
                            sourceStream.Position = 0; //<-- Have to set position to 0 to makse sure imageFactory.Load starts from the begining.
                            imageFactory.Load(sourceStream);

                            if (imageCropCoordinates != null)
                            {
                                var cropLayer = new ImageProcessor.Imaging.CropLayer(imageCropCoordinates.Left, imageCropCoordinates.Top, imageCropCoordinates.Right, imageCropCoordinates.Bottom, CropMode.Pixels);
                                //var cropRectangle = new Rectangle(imageCropCoordinates.Top, imageCropCoordinates.Left);

                                imageFactory.Crop(cropLayer);//<-- Crop first
                                imageFactory.Resize(resizeLayer);//<-- Then resize
                            }
                            else
                            {
                                imageFactory.Resize(resizeLayer);//<-- Resize
                            }

                            //Convert to proper format
                            imageFactory.Format(outputFormatProcessorProperty);

                            if (imageEnhancementInstructions != null)
                            {

                                //Basics ---

                                if (imageEnhancementInstructions.Brightness != 0)
                                {
                                    imageFactory.Brightness(imageEnhancementInstructions.Brightness);
                                }
                                if (imageEnhancementInstructions.Contrast != 0)
                                {
                                    imageFactory.Contrast(imageEnhancementInstructions.Contrast);
                                }
                                if (imageEnhancementInstructions.Saturation != 0)
                                {
                                    imageFactory.Saturation(imageEnhancementInstructions.Saturation);
                                }

                                // Sharpness ---
                                if (imageEnhancementInstructions.Sharpen != 0)
                                {
                                    imageFactory.GaussianSharpen(imageEnhancementInstructions.Sharpen);
                                }


                                //Filters ----

                                if (imageEnhancementInstructions.Sepia == true)
                                {
                                    imageFactory.Filter(MatrixFilters.Sepia);
                                }

                                if (imageEnhancementInstructions.Polaroid == true)
                                {
                                    imageFactory.Filter(MatrixFilters.Polaroid);
                                }
                                if (imageEnhancementInstructions.Greyscale == true)
                                {
                                    imageFactory.Filter(MatrixFilters.GreyScale);
                                }

                            }

                            imageFactory.Save(outStream);

                        }


                        //Store image size to BLOB STORAGE ---------------------------------
                        #region BLOB STORAGE

                        //CloudBlobClient blobClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudBlobClient();
                        CloudBlobClient blobClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudBlobClient();

                        //Create and set retry policy
                        IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(400), 6);
                        blobClient.DefaultRequestOptions.RetryPolicy = exponentialRetryPolicy;

                        //Creat/Connect to the Blob Container
                        blobClient.GetContainerReference(accountId).CreateIfNotExists(BlobContainerPublicAccessType.Blob); //<-- Create and make public
                        CloudBlobContainer blobContainer = blobClient.GetContainerReference(accountId);

                        //Get reference to the picture blob or create if not exists. 
                        CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileNameFull);

                        
                        if(type == "gif")
                        {
                            blockBlob.Properties.ContentType = "image/gif";
                        }
                        else if(type == "png")
                        {
                            blockBlob.Properties.ContentType = "image/png";
                        }
                        else
                        {
                            blockBlob.Properties.ContentType = "image/jpg";
                        }

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
            
            //Send the new imageId in the response object, along with the ApplicationImageDocumentModel for DocumentDB Storage
            response.isSuccess = true;
            response.SuccessCode = imageId;
            response.SuccessMessage = imageId;
            //response.ResponseObject = applicationImageDocumentModel;
            return response;
        }
    }
}
