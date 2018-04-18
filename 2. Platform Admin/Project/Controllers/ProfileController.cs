using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PlatformAdminSite.PlatformManagementService;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.ServiceModel;
using System.IO;
using System.Drawing;
using System.Text;

namespace PlatformAdminSite.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult Index()
        {
            return View();
        }

        #region JSON Feeds


        #region Get (Shared globally)

        /// <summary>
        /// Gets the profile of the currently logged in user. Used by Angular Views to display role based proeprties/abilities and up to date user properties. Should sync to WCF Services everytime a new angular view is rendered, or once every 1-2 minutes.
        /// </summary>
        /// <returns></returns>
        [Route("Profile/Json/GetCurrentUser")]
        [HttpGet]
        public JsonNetResult GetCurrentUser()
        {
            //This is used often by the UI to refresh the user, wrap in a Try/Catch in case of intermitent failures:

            try
            {
                string userId = AuthenticationCookieManager.GetAuthenticationCookie().Id;
                PlatformUser user = null;

                #region (Plan A) Get data from Redis Cache

                try
                {
                    //First we attempt to get the user from the Redis Cache

                    IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                    string hashKey = "user:" + userId.Replace("-", "");
                    string hashField = "model";

                    var redisValue = cache.HashGet(hashKey, hashField);
                    
                    //con.Close();
                    
                    if (redisValue.HasValue)
                    {
                        user = JsonConvert.DeserializeObject<PlatformUser>(redisValue);
                    }
                    
                }
                catch(Exception e)
                {
                    var error = e.Message;
                }

                #endregion

                if (user == null)
                {
                    #region (Plan B) Get data from WCF

                    //If a failure occurs, or the redis cache is empty we get the user from the WCF service
                    var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                    try
                    {
                        platformManagementServiceClient.Open();
                        user = platformManagementServiceClient.GetPlatformUser(userId, Common.SharedClientKey);

                        //Close the connection
                        WCFManager.CloseConnection(platformManagementServiceClient);
                    }
                    catch (Exception e)
                    {
                        #region Manage Exception

                        string exceptionMessage = e.Message.ToString();

                        var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                        string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                        // Abort the connection & manage the exception
                        WCFManager.CloseConnection(platformManagementServiceClient, exceptionMessage, currentMethodString);

                        #endregion
                    }

                    #endregion

                }

                //Update the local cookies:
                AuthenticationCookieManager.SaveAuthenticationCookie(user);

                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = user;

                return jsonNetResult;
            }
            catch
            {
                // Use cookies instead
                JsonNetResult jsonNetResult = new JsonNetResult();
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResult.Data = AuthenticationCookieManager.GetAuthenticationCookie();

                return jsonNetResult;
            }
        }

        #endregion

        #region Updates

        [Route("Profile/Json/UpdateName")]
        [HttpPost]
        public JsonNetResult UpdateName(string firstName, string lastName)
        {
            var response = new DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            var user = AuthenticationCookieManager.GetAuthenticationCookie();

            try
            {
                platformManagementServiceClient.Open();

                response = platformManagementServiceClient.UpdatePlatformUserFullName(
                    user.Id, firstName,
                    lastName,
                    user.Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.Exempt, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(platformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformManagementServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Profile/Json/UpdateEmail")]
        [HttpPost]
        public JsonNetResult UpdateEmail(string email)
        {
            var response = new DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            var user = AuthenticationCookieManager.GetAuthenticationCookie();

            try
            {
                platformManagementServiceClient.Open();

                response = platformManagementServiceClient.UpdatePlatformUserEmail(
                    user.Id,
                    email,
                    user.Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.Exempt, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(platformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformManagementServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Profile/Json/UpdatePassword")]
        [HttpPost]
        public JsonNetResult UpdatePassword(string currentPassword, string newPassword)
        {
            var response = new DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                var user = AuthenticationCookieManager.GetAuthenticationCookie();

                response = platformManagementServiceClient.UpdatePlatformUserPassword(
                    user.Email,
                    currentPassword,
                    newPassword,
                    user.Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.Exempt, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(platformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformManagementServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }



        #endregion


        #region Imaging

        [Route("Profile/Json/UploadPhoto")]
        [HttpPost]
        public JsonNetResult UploadPhoto() //  HttpPostedFileBase file)     //  Object file)
        {
            var length = Request.ContentLength;
            var bytes = new byte[length];

            Request.InputStream.Read(bytes, 0, length);


            //foreach (string file in Request.Files)
            //{
                //HttpPostedFile hpf = Request.Files[file] as HttpPostedFile;
                //var hpf = Request.Files[file];
                //if (hpf.ContentLength == 0)
                    //continue;
                //string savedFileName = Path.Combine(
                   //AppDomain.CurrentDomain.BaseDirectory,
                   //Path.GetFileName(hpf.FileName));
                //hpf.SaveAs(savedFileName);

                
            //}


            //byte[] newByteArray = Request.Files[0].InputStream.CopyTo(ms);//Encoding.ASCII.GetBytes(byteArray);
            /*
            byte[] byteArray;

            using (var ms = new MemoryStream())
            {
                Request.Files[0].InputStream.CopyTo(ms); //<-- We only expect one file so we use the 0 index rather than looping trough the length
                byteArray = ms.ToArray();
            }*/


            var response = new DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            JsonNetResult jsonNetResult = new JsonNetResult();

            //Verify size & image
            #region Verify image format & size

            /*
            try
            {

                Bitmap bmpSource;

                //Convert byte[] to BMP
                using (var ms = new MemoryStream(byteArray))
                {
                    bmpSource = new Bitmap(ms);
                }

                
                var format = bmpSource.RawFormat;
                if (!format.Equals(System.Drawing.Imaging.ImageFormat.Jpeg)
                    && !format.Equals(System.Drawing.Imaging.ImageFormat.Gif)
                    && !format.Equals(System.Drawing.Imaging.ImageFormat.Png)
                    && !format.Equals(System.Drawing.Imaging.ImageFormat.Bmp)
                    && !format.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
                {
                    //File is not a supported image type, return error
                    response.isSuccess = false;
                    response.ErrorMessage = "Please use a supported image file format.";

 
                    jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
             * jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                    jsonNetResult.Data = response;

                    return jsonNetResult;
                }
            }
            catch(Exception e)
            {
                //File is not an image, return error
                response.isSuccess = false;
                response.ErrorMessage = "Please use an image file.";

                
                jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResult.Data = response;

                return jsonNetResult;
            }*/

            #endregion

            try
            {
                platformManagementServiceClient.Open();

                var user = AuthenticationCookieManager.GetAuthenticationCookie();

                response = platformManagementServiceClient.UpdatePlatformUserProfilePhoto(
                    user.Id,
                    bytes,
                    user.Id,
                    PlatformAdminSite.PlatformManagementService.RequesterType.Exempt, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(platformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformManagementServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        /*
        [Route("Profile/Json/CropPhoto")]
        [HttpPost]
        public JsonNetResult CropPhoto(string sourceImageId, double x, double y, double x2, double y2)
        {
            var response = new DataAccessResponseType();
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                platformManagementServiceClient.Open();

                var user = AuthenticationCookieManager.GetAuthenticationCookie();

                //response = platformManagementServiceClient.CropPlatformUserPhoto(
                    //sourceImageId,
                    //user.Id,
                    //PlatformAdminSite.PlatformManagementService.RequesterType.Exempt);

                //Close the connection
                WCFManager.CloseConnection(platformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformManagementServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        */
        #endregion

        #endregion
    }
}