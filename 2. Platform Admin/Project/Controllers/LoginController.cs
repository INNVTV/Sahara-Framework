using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PlatformAdminSite.Models.Authentication;
using PlatformAdminSite.PlatformAuthenticationService;
using Microsoft.AspNet.Identity;
using System.ServiceModel;

namespace PlatformAdminSite.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            if(!CoreServices.Platform.Initialized)
            {
                return RedirectToAction("Index", "Initialization");
            }
            else
            {
                Login login = new Login();

                return View(login);
            }
            
        }

        //
        // POST: /Login/
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Login login, string returnUrl)
        {
            //RememberMe default is true
            login.RememberMe = true;

            //Sign user out (if signed in as another user)
            HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            if (ModelState.IsValid)
            {
                var authResponse = new AuthenticationResponse { isSuccess = false };
                var platformAuthenticationServiceClient = new PlatformAuthenticationService.PlatformAuthenticationServiceClient();

                try
                {
                    //Get the ip address and call origin
                    var ipAddress = Request.UserHostAddress;
                    var origin = "Web";

                    //Attempt to log in
                    platformAuthenticationServiceClient.Open();
                    authResponse = platformAuthenticationServiceClient.Authenticate(login.Email, login.Password, ipAddress, origin, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(platformAuthenticationServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(platformAuthenticationServiceClient, exceptionMessage, currentMethodString);

                    // Upate the response object
                    authResponse.isSuccess = false;
                    authResponse.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                    //auth//response.ErrorMessages[0] = exceptionMessage;

                    #endregion
                }

                if (authResponse.isSuccess)
                {
                    // Sign User In
                    var currentUtc = new SystemClock().UtcNow;
                    HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties()
                    {
                        IsPersistent = login.RememberMe,
                        ExpiresUtc = currentUtc.Add(TimeSpan.FromHours(6))
                    },
                        authResponse.ClaimsIdentity);


                    //Store the PlatformUser object as a cookie:
                    AuthenticationCookieManager.SaveAuthenticationCookie(authResponse.PlatformUser);

                    //After user is authenticated, send them to the page they were attempting to use, otherwise, send them to home:
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                }
                else
                {
                    //ModelState.AddModelError("", "Invalid username or password.");

                    //foreach (string error in authResponse.ErrorMessages)
                    //{
                        ModelState.AddModelError("CoreServiceError", authResponse.ErrorMessage);
                    //}

                    return View(login);
                }
            }
            else
            {
                return View(login);
            }




        }

        [HttpGet]
        public ActionResult LogOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            AuthenticationCookieManager.DestoryAuthenticationCookie();
            return View();
        }




        /*
        [HttpGet]
        public ActionResult CookieOnly()
        {
            AuthenticationCookieManager.DestoryAuthenticationCookie();
            return View();
        }

        [HttpGet]
        public ActionResult AuthOnly()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            return View();
        }*/
    }
}