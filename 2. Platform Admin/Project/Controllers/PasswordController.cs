using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PlatformAdminSite.Models.Authentication;
using System.ServiceModel;

namespace PlatformAdminSite.Controllers
{
    public class PasswordController : Controller
    {

        [HttpGet]
        public ActionResult Forgot()
        {
            var resetPassword = new PasswordClaim();

            return View(resetPassword);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Forgot(PasswordClaim password)
        {
            if (ModelState.IsValid)
            {
                var claimResponse = new PlatformManagementService.DataAccessResponseType();
                var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
                
                try
                {
                    //Attempt to send lost password claim
                    platformManagementServiceClient.Open();
                    claimResponse = platformManagementServiceClient.ClaimLostPassword(password.Email, Common.SharedClientKey);

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
                    claimResponse.isSuccess = false;
                    claimResponse.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                    //claimresponse.ErrorMessages[0] = exceptionMessage;

                    #endregion
                }


                if (claimResponse.isSuccess)
                {
                    return RedirectToAction("sent", "password");
                }
                else
                {
                    //ModelState.AddModelError("", "Invalid username or password.");

                    //foreach (string error in authResponse.ErrorMessages)
                    //{
                    ModelState.AddModelError("CoreServiceError", claimResponse.ErrorMessage);
                    //}

                    return View(password);
                }
            }
            else
            {
                return View(password);
            }
        }

        [HttpGet]
        public ActionResult Sent()
        {
            return View();
        }


        [HttpGet]
        [Route("password/reset/{passwordClaimKey}")]
        public ActionResult Reset(string passwordClaimKey)
        {
            var email = string.Empty;
            var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                //Get the password claim email from CoreServices
                platformManagementServiceClient.Open();

                email = platformManagementServiceClient.GetLostPasswordClaim(passwordClaimKey, Common.SharedClientKey);

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

            if (String.IsNullOrEmpty(email))
            {
                return Content("Password claim key does not exist.");
            }
            else
            {
                var resetPassword = new ResetPassword
                {
                    Email = email,
                    PasswordClaimKey = passwordClaimKey
                };

                return View(resetPassword);
            }
 
        }



        [HttpPost]
        [Route("password/reset/{passwordClaimKey}")]
        public ActionResult Reset(ResetPassword resetPassword)
        {
            if (ModelState.IsValid)
            {

                //Confirm passwords:
                if (resetPassword.Password != resetPassword.PasswordConfirm)
                {
                    ModelState.AddModelError("CoreServiceError", "Password and confirmation do not match!");
                    return View(resetPassword);
                }


                
                var claimResponse = new PlatformManagementService.DataAccessResponseType();
                var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();


                
                try
                {
                    //Attempt to send lost password claim
                    platformManagementServiceClient.Open();
                    claimResponse = platformManagementServiceClient.ResetLostPassword(resetPassword.PasswordClaimKey,resetPassword.Password, Common.SharedClientKey);

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
                    claimResponse.isSuccess = false;
                    claimResponse.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                    //claimresponse.ErrorMessages[0] = exceptionMessage;

                    #endregion
                }

                if (claimResponse.isSuccess)
                {
                    return RedirectToAction("Success", "Password");
                }
                else
                {
                    //ModelState.AddModelError("", "Invalid username or password.");

                    //foreach (string error in authResponse.ErrorMessages)
                    //{
                    ModelState.AddModelError("CoreServiceError", claimResponse.ErrorMessage);
                    //}

                    return View(resetPassword);
                }
            }
            else
            {
                return View(resetPassword);
            }
        }

        [HttpGet]
        public ActionResult Success()
        {
            return View();
        }
    }
}