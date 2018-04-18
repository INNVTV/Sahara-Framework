using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AccountAdminSite.Models.Authentication;
using System.ServiceModel;


namespace AccountAdminSite.Controllers
{
    public class PasswordController : Controller
    {

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Forgot()
        {
            var passwordClaim = new PasswordClaim();

            //Get the subdomain (if exists) for the site
            string subdomain = Common.GetSubDomain(Request.Url);

            if (!String.IsNullOrEmpty(subdomain))
            {
                //If subdomain exists, add the subdomain to the login object, otherwise View will ask user to manually enter it:
                passwordClaim.AccountNameKey = subdomain;
                return View(passwordClaim);
            }
            else
            {
                return Content("No account specified.");
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Forgot(PasswordClaim passwordClaim)
        {
            if (ModelState.IsValid)
            {
                
                var claimResponse = new AccountManagementService.DataAccessResponseType { isSuccess = false };
                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                
                try
                {
                    //Attempt to send lost password claim
                    accountManagementServiceClient.Open();
                    claimResponse = accountManagementServiceClient.ClaimLostPassword(passwordClaim.AccountNameKey, passwordClaim.Email, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(accountManagementServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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

                    return View(passwordClaim);
                }
            }
            else
            {
                return View(passwordClaim);
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
            //Get the subdomain (if exists) for the site
            string accountNameKey = Common.GetSubDomain(Request.Url);

            if (String.IsNullOrEmpty(accountNameKey))
            {
                return Content("No account specified.");
            }

            var email = String.Empty;
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();


            try
            {
                //Get the password claim email from CoreServices
                accountManagementServiceClient.Open();
                email = accountManagementServiceClient.GetLostPasswordClaim(accountNameKey, passwordClaimKey, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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
                    AccountNameKey = accountNameKey,
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

                var claimResponse = new AccountManagementService.DataAccessResponseType();
                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    //Attempt to send lost password claim
                    accountManagementServiceClient.Open();
                    claimResponse = accountManagementServiceClient.ResetLostPassword(resetPassword.AccountNameKey, resetPassword.PasswordClaimKey, resetPassword.Password, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(accountManagementServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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