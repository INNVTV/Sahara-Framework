using AccountAdminSite.AccountManagementService;
using AccountAdminSite.ApplicationImagesService;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents;
using System.Drawing;
using AccountAdminSite.Models.Imaging;
using System.Text;
using System.Dynamic;
using System.Drawing.Drawing2D;

namespace AccountAdminSite.Controllers
{
    [Authorize]
    public class ImagingController : Controller
    {

        #region View Controllers

        // GET: /Imaging/
        /*
        public ActionResult Index()
        {
            return View();
        }*/


        // Used for Detail variation, We set it as off in order to allow MVC routing to take over.
        // GET: /Imaging/{id}
        /*
        [Route("Imaging/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }*/


        #region GET Source and Assign Instructions (Loaded into iFrame for the object that has images added)

        /// <summary>
        /// We upload the source image to intermediary storage (gets garbage collected by Custodian on set interval)
        /// Crop view pulls source in to apply/generate cropping and enhancement data 
        /// Job is then submited job to WCF for verification, processing and logging.
        /// </summary>
        /// <param name="imageSourceFile"></param>
        /// <returns></returns>
        [Route("Imaging/Instructions")]
        [HttpGet]
        public ActionResult Instructions(ImagingInstructionsModel imagingInstructions)
        {
            return View(imagingInstructions);
        }

        #endregion

        #endregion


        #region POST Source Image Controller

        /// <summary>
        /// Uploads a source image stored to intermediary storage for immediate cropping/enhancement instruction development.
        /// These instructions are then sent to CoreServices using "ProcessImage" for generation of final images.
        /// Core Services Custodian clears folders based on dates at set intervals.
        /// </summary>
        /// <returns></returns>
        [Route("Imaging/UploadIntermediaryImageForObjectRecord")]
        [HttpPost]
        public JsonNetResult UploadIntermediaryImageForObjectRecord(string type, int formatWidth, int formatHeight) //  HttpPostedFileBase file)     //  Object file)
        {
            try
            {
                dynamic response = new ExpandoObject();

                #region Add properties to response object that are also used to create the intermediary file

                response.isSuccess = false;
                response.SourceContainerName = DateTime.UtcNow.Month + "-" + DateTime.UtcNow.Day + "-" + DateTime.UtcNow.Year;
                response.ImageId = Guid.NewGuid().ToString();
                response.FileName = response.ImageId + "." + type; // ".jpg";   //<-- .jpg, .png or .gif
                response.IntermediateURL = CoreServices.PlatformSettings.Urls.IntermediaryImagingCdnUri + "/" + response.SourceContainerName + "/" + response.FileName;

                #endregion

                #region Convert to image, resize (to improve performance on WCF) and save to storage

                using (Stream inputStream = Request.InputStream)
                {
                    System.Drawing.Image image = System.Drawing.Image.FromStream(inputStream);

                    //if image.PixelFormat = System.Drawing.Imaging.PixelFormat.rg

                    #region Determin if image requires sizing down (based on format requirements)

                    if(type == "gif")
                    {
                        //Ignore resizing on Gifs
                    }
                    else if(formatWidth == 0)
                    {
                        // Resize VARIABLE WIDTH Images: ---------------------

                        //But only if height is too large or pixel dimensions call for it
                        if(image.Height > formatHeight || image.HorizontalResolution >= 300 || image.VerticalResolution >= 300)
                        {
                            //Detect new size required to MAINTAIN ASPECT RATIO at the new height:
                            var newSize = CalculateResizeToSameAspectRatio(new Size(image.Width, image.Height), formatHeight);

                            try
                            {
                                //This can be skipped, but resizing upfront is much faster that forcing WCF to rezise and crop a file that's too large and could hold up other WCF users as well as crash those services.
                                image = ScaleImageAndResolution(image, newSize.Width, newSize.Height, 96);
                            }
                            catch (Exception e)
                            {
                                #region Handle resizing exceptions

                                response.isSuccess = false;
                                response.ErrorMessage = e.Message;

                                if (response.ErrorMessage.ToLower() == "out of memory.")
                                {
                                    response.ErrorMessage = "Out of memory. Please ensure that color profile is RGB (not CMYK) and that pixel dimensions are modest.";
                                }

                                JsonNetResult jsonNetResultError = new JsonNetResult();

                                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                                jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                                jsonNetResultError.Data = response;

                                return jsonNetResultError;

                                #endregion
                            }
                        }


                    }
                    else if(image.Width > formatWidth || image.Height > formatHeight || image.HorizontalResolution >= 300 || image.VerticalResolution >= 300)
                    {
                        // Resize FIXED SIZE Images: ---------------------

                        //Detect new size required to FILL the format and offer cropping (if needed)
                        var newSize = CalculateResizeToFitAndFill(new Size(image.Width, image.Height), new Size(formatWidth, formatHeight));

                        try
                        {
                            //This can be skipped, but resizing upfront is much faster that forcing WCF to rezise and crop a file that's too large and could hold up other WCF users as well as crash those services.
                            image = ScaleImageAndResolution(image, newSize.Width, newSize.Height, 96);
                        }
                        catch(Exception e)
                        {
                            #region Handle resizing exceptions

                            response.isSuccess = false;
                            response.ErrorMessage = e.Message;

                            if (response.ErrorMessage.ToLower() == "out of memory.")
                            {
                                response.ErrorMessage = "Out of memory. Please ensure that color profile is RGB (not CMYK) and that pixel dimensions are modest.";
                            }

                            JsonNetResult jsonNetResultError = new JsonNetResult();

                            jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                            jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                            jsonNetResultError.Data = response;

                            return jsonNetResultError;

                            #endregion
                        }
                    }


                    #endregion

                    #region Upload to Intermediary Storage

                    CloudStorageAccount storageAccount;

                    // Credentials are from centralized CoreServiceSettings
                    StorageCredentials storageCredentials = new StorageCredentials(CoreServices.PlatformSettings.Storage.IntermediaryName, CoreServices.PlatformSettings.Storage.IntermediaryKey);
                    storageAccount = new CloudStorageAccount(storageCredentials, false);

                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                    //Creat/Connect to the Blob Container
                    blobClient.GetContainerReference(response.SourceContainerName).CreateIfNotExists(BlobContainerPublicAccessType.Blob); //<-- Create and make public
                    CloudBlobContainer blobContainer = blobClient.GetContainerReference(response.SourceContainerName);

                    //Get reference to the picture blob or create if not exists. 
                    CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(response.FileName);

                    //Save to storage
                    //Convert final BMP to byteArray
                    //Byte[] sourceByteArray;

                    //using (var binaryReader = new BinaryReader(Request.InputStream))
                    //{
                    //sourceByteArray = binaryReader.ReadBytes(Request.ContentLength);
                    //}

                    response.SourceWidth = image.Width;
                    response.SourceHeight = image.Height;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                    
                        if (type == "gif")
                        {
                            blockBlob.Properties.ContentType = "image/gif";
                            image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Gif);
                        }
                        else if (type == "png")
                        {
                            blockBlob.Properties.ContentType = "image/png";
                                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        else
                        {
                            blockBlob.Properties.ContentType = "image/jpg";
                            image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }



                        //Option 1 (smaller file).
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        blockBlob.UploadFromStream(memoryStream);


                        //Option 2 (Much larger file).
                        //byte[] imageBytes = memoryStream.GetBuffer();
                        //blockBlob.UploadFromByteArray(imageBytes, 0, imageBytes.Length);

                        Request.InputStream.Dispose();
                        inputStream.Dispose();
                        image.Dispose();
                        memoryStream.Dispose();
                    }

                    //blockBlob.UploadFromByteArray(sourceByteArray, 0, sourceByteArray.Length);

                    #endregion

                }

                #endregion

                #region Upload to Intermediary Storage (Moved to using above)

                /*

                CloudStorageAccount storageAccount;

                // Credentials are from centralized CoreServiceSettings
                StorageCredentials storageCredentials = new StorageCredentials(CoreServices.PlatformSettings.Storage.IntermediaryName, CoreServices.PlatformSettings.Storage.IntermediaryKey);
                storageAccount = new CloudStorageAccount(storageCredentials, false);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                //Creat/Connect to the Blob Container
                blobClient.GetContainerReference(response.SourceContainerName).CreateIfNotExists(BlobContainerPublicAccessType.Blob); //<-- Create and make public
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(response.SourceContainerName);

                //Get reference to the picture blob or create if not exists. 
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(response.FileName);

                //Save to storage
                //Convert final BMP to byteArray
                Byte[] sourceByteArray;

                using (var binaryReader = new BinaryReader(Request.InputStream))
                {
                    sourceByteArray = binaryReader.ReadBytes(Request.ContentLength);
                }

                if (type == "gif")
                {
                    blockBlob.Properties.ContentType = "image/gif";
                }
                else if (type == "png")
                {
                    blockBlob.Properties.ContentType = "image/png";
                }
                else
                {
                    blockBlob.Properties.ContentType = "image/jpg";
                }

                //blockBlob.Properties.col

                blockBlob.UploadFromByteArray(sourceByteArray, 0, sourceByteArray.Length);
            
                */

                #endregion

                response.isSuccess = true;



                JsonNetResult jsonNetResult = new JsonNetResult();

                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.None;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = response;

                return jsonNetResult;
            }
            catch(Exception e)
            {
                dynamic responseError = new ExpandoObject();


                responseError.isSuccess = false;
                responseError.ErrorMessage = e.Message;

                if (responseError.ErrorMessage.ToLower() == "out of memory.")
                {
                    responseError.ErrorMessage = "Out of memory. Please try a smaller image or refresh the page and attempt to reupload.";
                }

                JsonNetResult jsonNetResultError = new JsonNetResult();

                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultError.Data = responseError;

                return jsonNetResultError;
            }
        }


        #region Image Helper Functions


        public static Size CalculateResizeToFitAndFill(Size imageSize, Size formatSize)
        {
            // TODO: Check for arguments (for null and <=0)
            var widthScale = formatSize.Width / (double)imageSize.Width;
            var heightScale = formatSize.Height / (double)imageSize.Height;
            var scale = Math.Max(widthScale, heightScale);
            return new Size(
                (int)Math.Round((imageSize.Width * scale)),
                (int)Math.Round((imageSize.Height * scale))
                );
        }


        public static Size CalculateResizeToSameAspectRatio(Size imageSize, int newHeight)
        {
            // TODO: Check for arguments (for null and <=0)
            double dividend = (double)imageSize.Height / (double)newHeight;

            double newWidth = imageSize.Width / dividend;

            int newWidthInt = (int)newWidth;

            return new Size(newWidthInt, newHeight);
        }



        //Change image scale by Width and Height and Set New Resoltion too.
        public static Image ScaleImageAndResolution(Image img, int outputWidth, int outputHeight, float resolution)
        {
            Bitmap outputImage = new Bitmap(outputWidth, outputHeight, img.PixelFormat);
            outputImage.SetResolution(resolution, resolution);
            Graphics graphics = Graphics.FromImage(outputImage);
            graphics.InterpolationMode = InterpolationMode.Bilinear;
            graphics.DrawImage(img, new Rectangle(0, 0, outputWidth, outputHeight), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
            graphics.Dispose();
            return outputImage;
        }


        #endregion


        #endregion


        #region JSON Services

        #region Process

        [Route("Imaging/Json/ProcessImage")]
        [HttpGet]
        public JsonNetResult ProcessImage(string imageId, string objectType, string objectId, string imageGroupNameKey, string imageFormatNameKey,  string containerName, string imageFormat, string type, int quality, float top, float left, float right, float bottom, string title, string description, string tags, int brightness, int contrast, int saturation, int sharpness, bool sepia, bool polaroid, bool greyscale)
        {
            var response = new ApplicationImagesService.DataAccessResponseType();


            #region Generate Job Models

            var imageProcessingManifest = new ImageProcessingManifestModel
            {
                SourceContainerName = containerName,
                FileName = imageId + "." + imageFormat,

                GroupTypeNameKey = objectType,
                ObjectId = objectId,

                Type = type,
                Quality = quality,

                GroupNameKey = imageGroupNameKey,
                FormatNameKey = imageFormatNameKey,

                ImageId = imageId,
                Title = title,
                Description = description
            };


            var imageCropCoordinates = new ImageCropCoordinates
            {
                Top = top,
                Left = left,
                Right = right,
                Bottom = bottom
            };

            var imageEnhancementInstructions = new ImageEnhancementInstructions();

            if (brightness != 0 || contrast != 0 || saturation != 0 || saturation != 0 || sharpness != 0 || sepia == true || polaroid == true || greyscale == true)
            {
                imageEnhancementInstructions.Brightness = brightness;
                imageEnhancementInstructions.Contrast = contrast;
                imageEnhancementInstructions.Saturation = saturation;
                imageEnhancementInstructions.Sharpen = sharpness;
                imageEnhancementInstructions.Sepia = sepia;
                imageEnhancementInstructions.Greyscale = greyscale;
                imageEnhancementInstructions.Polaroid = polaroid;
            }
            else
            {

                imageEnhancementInstructions = null;
            }



            #endregion

            #region Submit Job

            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            var account = Common.GetAccountObject(user.AccountNameKey);

            try
            {
                var applicationImagesServicesClient = new ApplicationImagesServiceClient();
                response = applicationImagesServicesClient.ProcessImage(user.AccountID.ToString(), imageProcessingManifest, imageCropCoordinates, user.Id, ApplicationImagesService.RequesterType.AccountUser, imageEnhancementInstructions, Common.SharedClientKey);

            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            #endregion

            #region Handle Caching Expiration on Account image type

            if(objectType == "account")
            {
                //Expire accountimages cache
                RedisCacheKeys.AccountImages.Expire(user.AccountNameKey);
            }

            #endregion

            //Append account CDN url to Success message (newImageUrlPath)
            var storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);
            response.SuccessMessage = "https://" + storagePartition.CDN + "/" + response.SuccessMessage;

            #region Handle Results

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

            #endregion
        }

        #endregion


        #region Update

        [Route("Imaging/Json/UpdateImageRecordTitle")]
        [HttpGet]
        public JsonNetResult UpdateImageRecordTitle(string objectType, string objectId, string groupNameKey, string formatNameKey, string newTitle)
        {
            var response = new ApplicationImagesService.DataAccessResponseType();

            #region Update

            var user = AuthenticationCookieManager.GetAuthenticationCookie();

            try
            {
                var applicationImagesServicesClient = new ApplicationImagesServiceClient();
                response = applicationImagesServicesClient.UpdateImageRecordTitle(user.AccountID.ToString(), objectType, objectId, groupNameKey, formatNameKey, newTitle, user.Id, ApplicationImagesService.RequesterType.AccountUser, Common.SharedClientKey);

            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            #endregion

            #region Handle Caching Expiration on Account image type

            if (objectType == "account")
            {
                //Expire accountimages cache
                RedisCacheKeys.AccountImages.Expire(user.AccountNameKey);
            }

            #endregion

            #region Handle Results

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

            #endregion
        }

        [Route("Imaging/Json/UpdateImageRecordDescription")]
        [HttpGet]
        public JsonNetResult UpdateImageRecordDescription(string objectType, string objectId, string groupNameKey, string formatNameKey, string newDescription)
        {
            var response = new ApplicationImagesService.DataAccessResponseType();

            #region Update

            var user = AuthenticationCookieManager.GetAuthenticationCookie();

            try
            {
                var applicationImagesServicesClient = new ApplicationImagesServiceClient();
                response = applicationImagesServicesClient.UpdateImageRecordDescription(user.AccountID.ToString(), objectType, objectId, groupNameKey, formatNameKey, newDescription, user.Id, ApplicationImagesService.RequesterType.AccountUser, Common.SharedClientKey);

            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            #endregion

            #region Handle Caching Expiration on Account image type

            if (objectType == "account")
            {
                //Expire accountimages cache
                RedisCacheKeys.AccountImages.Expire(user.AccountNameKey);
            }

            #endregion

            #region Handle Results

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

            #endregion
        }


        [Route("Imaging/Json/UpdateImageRecordGalleryTitle")]
        [HttpGet]
        public JsonNetResult UpdateImageRecordGalleryTitle(string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex, string newTitle)
        {
            var response = new ApplicationImagesService.DataAccessResponseType();

            #region Update

            var user = AuthenticationCookieManager.GetAuthenticationCookie();

            try
            {
                var applicationImagesServicesClient = new ApplicationImagesServiceClient();
                response = applicationImagesServicesClient.UpdateImageRecordGalleryTitle(user.AccountID.ToString(), objectType, objectId, groupNameKey, formatNameKey, imageIndex, newTitle, user.Id, ApplicationImagesService.RequesterType.AccountUser, Common.SharedClientKey);

            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            #endregion

            #region Handle Caching Expiration on Account image type

            if (objectType == "account")
            {
                //Expire accountimages cache
                RedisCacheKeys.AccountImages.Expire(user.AccountNameKey);
            }

            #endregion

            #region Handle Results

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

            #endregion
        }

        [Route("Imaging/Json/UpdateImageRecordGalleryDescription")]
        [HttpGet]
        public JsonNetResult UpdateImageRecordGalleryDescription(string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex, string newDescription)
        {
            var response = new ApplicationImagesService.DataAccessResponseType();

            #region Update

            var user = AuthenticationCookieManager.GetAuthenticationCookie();

            try
            {
                var applicationImagesServicesClient = new ApplicationImagesServiceClient();
                response = applicationImagesServicesClient.UpdateImageRecordGalleryDescription(user.AccountID.ToString(), objectType, objectId, groupNameKey, formatNameKey, imageIndex, newDescription, user.Id, ApplicationImagesService.RequesterType.AccountUser, Common.SharedClientKey);

            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            #endregion

            #region Handle Caching Expiration on Account image type

            if (objectType == "account")
            {
                //Expire accountimages cache
                RedisCacheKeys.AccountImages.Expire(user.AccountNameKey);
            }

            #endregion

            #region Handle Results

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

            #endregion
        }


        #endregion

        #region Reorder

        [Route("Imaging/Json/ReorderImageGallery")]
        [HttpGet]
        public JsonNetResult ReorderImageGallery(string objectType, string objectId, string groupNameKey, string formatNameKey, List<int> imageIndexOrder)
        {
            var response = new ApplicationImagesService.DataAccessResponseType();

            #region Reorder Images

            var user = AuthenticationCookieManager.GetAuthenticationCookie();

            try
            {
                var applicationImagesServicesClient = new ApplicationImagesServiceClient();
                response = applicationImagesServicesClient.ReorderImageRecordGallery(user.AccountID.ToString(), objectType, objectId, groupNameKey, formatNameKey, imageIndexOrder.ToArray(), user.Id, ApplicationImagesService.RequesterType.AccountUser, Common.SharedClientKey);

            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            #endregion

            #region Handle Caching Expiration on Account image type

            if (objectType == "account")
            {
                //Expire accountimages cache
                RedisCacheKeys.AccountImages.Expire(user.AccountNameKey);
            }

            #endregion

            #region Handle Results

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

            #endregion
        }

        #endregion

        #region Delete

        [Route("Imaging/Json/DeleteImageRecordForObject")]
        [HttpGet]
        public JsonNetResult DeleteImageRecordForObject(string objectType, string objectId, string groupNameKey, string formatNameKey)
        {
            var response = new ApplicationImagesService.DataAccessResponseType();

            #region Delete Image

            var user = AuthenticationCookieManager.GetAuthenticationCookie();

            try
            {
                var applicationImagesServicesClient = new ApplicationImagesServiceClient();
                response = applicationImagesServicesClient.DeleteImageRecord(user.AccountID.ToString(), objectType, objectId, groupNameKey, formatNameKey, user.Id, ApplicationImagesService.RequesterType.AccountUser, Common.SharedClientKey);

            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            #endregion

            #region Handle Caching Expiration on Account image type

            if (objectType == "account")
            {
                //Expire accountimages cache
                RedisCacheKeys.AccountImages.Expire(user.AccountNameKey);
            }

            #endregion

            #region Handle Results

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

            #endregion
        }


        [Route("Imaging/Json/DeleteGalleryImageForObject")]
        [HttpGet]
        public JsonNetResult DeleteGalleryImageForObject(string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex)
        {
            var response = new ApplicationImagesService.DataAccessResponseType();

            #region Delete Image

            var user = AuthenticationCookieManager.GetAuthenticationCookie();

            try
            {
                var applicationImagesServicesClient = new ApplicationImagesServiceClient();
                response = applicationImagesServicesClient.DeleteGalleryImage(user.AccountID.ToString(), objectType, objectId, groupNameKey, formatNameKey, imageIndex, user.Id, ApplicationImagesService.RequesterType.AccountUser, Common.SharedClientKey);

            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            #endregion

            #region Handle Caching Expiration on Account image type

            if (objectType == "account")
            {
                //Expire accountimages cache
                RedisCacheKeys.AccountImages.Expire(user.AccountNameKey);
            }

            #endregion

            #region Handle Results

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

            #endregion
        }

        #endregion

        #endregion

    }
}