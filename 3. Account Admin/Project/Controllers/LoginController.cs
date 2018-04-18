using AccountAdminSite.AccountAuthenticationService;
using AccountAdminSite.Models.Authentication;
using Microsoft.Owin.Infrastructure;
using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.ServiceModel;


namespace AccountAdminSite.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            Login login = new Login();

            //Get the subdomain (if exists) for the site
            string subdomain = Common.GetSubDomain(Request.Url);

            if (!String.IsNullOrEmpty(subdomain))
            {
                //If subdomain exists, add the subdomain to the login object, (Removed: otherwise View will ask user to manually enter it):
                login.AccountName = subdomain;                
            }
            else
            {
                //If no subdomain exists ignore the login request, show a blank page (better security)
                return null;
            }

            /*
            if(login.AccountName.ToLower() == "accounts" || login.AccountName.ToLower() == "account")
            {
                //This is the generic login route, so we remove the account name and force the user to add it.
                login.AccountName = null;
            }*/

            return View(login);
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

                //Attempt to log in
                var accountAuthenticationServiceClient = new AccountAuthenticationService.AccountAuthenticationServiceClient();

                try
                {
                    //Get the ip address and call origin
                    var ipAddress = Request.UserHostAddress;
                    var origin = "Web";

                    accountAuthenticationServiceClient.Open();
                    authResponse = accountAuthenticationServiceClient.Authenticate(login.AccountName, login.Email, login.Password, ipAddress, origin, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(accountAuthenticationServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountAuthenticationServiceClient, exceptionMessage, currentMethodString);

                    // Upate the response object
                    authResponse.isSuccess = false;
                    authResponse.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                    //auth//response.ErrorMessages[0] = exceptionMessage;

                    #endregion
                }


                //ClaimsIdentity[] claimsIdentity = new ClaimsIdentity[0];
                //claimsIdentity[0] = authResponse.ClaimsIdentity;

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
                    AuthenticationCookieManager.SaveAuthenticationCookie(authResponse.AccountUser);

                    //-------------------- Uknown Accounts (Removed)-------
                    /*
                    string subdomain = Common.GetSubDomain(Request.Url);
                    if (subdomain.ToLower() == "accounts" || subdomain.ToLower() == "account")
                    {
                        //This is the generic login route....
                        //Since the user had to provide the account name, redirect to the new URL
                        return Redirect("https://" + authResponse.AccountUser.AccountNameKey + "." + CoreServices.PlatformSettings.Urls.AccountManagementDomain + "/inventory");
                    }               
                    else */
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        //After user is authenticated, send them to the page they were attempting to use, 
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        //otherwise, send them to home:
                        return RedirectToAction("Index", "Inventory");
                    }
                }
                else
                {
                    //ModelState.AddModelError("", "Invalid username or password.");

                    //foreach (string error in authResponse.ErrorMessages)
                    //{
                        ModelState.AddModelError("CoreServiceError", authResponse.ErrorMessage);
                    //}


                    //Removed
                    /*
                    string subdomain = Common.GetSubDomain(Request.Url);
                    if (subdomain.ToLower() == "accounts" || subdomain.ToLower() == "account")
                    {
                        //This is the generic login route, so we remove the account name and force the user to add it.
                        login.AccountName = null;
                    }*/

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