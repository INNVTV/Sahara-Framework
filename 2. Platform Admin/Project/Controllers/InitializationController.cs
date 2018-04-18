using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using PlatformAdminSite.Models.Authentication;
using PlatformAdminSite.Models.Users;
using PlatformAdminSite.PlatformAuthenticationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    [AllowAnonymous]
    public class InitializationController : Controller
        {
            //
            // GET: /Initialization/
            [HttpGet]
            public ActionResult Index()
            {
                //Check if Platform exists
                var platformInitializationServiceClient = new PlatformInitializationService.PlatformInitializationServiceClient();
                platformInitializationServiceClient.Open();
                var isInitialized = platformInitializationServiceClient.IsPlatformInitialized();

                //Close the connection
                WCFManager.CloseConnection(platformInitializationServiceClient);

                CoreServices.Platform.Initialized = isInitialized;

                if (!isInitialized)
                {
                    return View(new RegisterPlatformUserModel());
                }
                else
                {
                    return RedirectToAction("PlatformExists");
                }
            }


            [HttpPost]
            public ActionResult Index(RegisterPlatformUserModel registerPlatformUser)
            {
                if (ModelState.IsValid)
                {
                    var platformInitializationServiceClient = new PlatformInitializationService.PlatformInitializationServiceClient();
                    platformInitializationServiceClient.Open();

                    var response = platformInitializationServiceClient.ProvisionPlatform(registerPlatformUser.FirstName, registerPlatformUser.LastName, registerPlatformUser.Email, registerPlatformUser.Password);

                    //Close the connection
                    WCFManager.CloseConnection(platformInitializationServiceClient);

                    //var response = Sahara.Platform.Core.Initialization.PlatformInitializationManager.ProvisionPlatform(user, registerPlatformUser.Password);

                    if (response.isSuccess)
                    {
                        // Update PlatformStatus to Exists = true
                        //PlatformStatus.Exists = true;

                        //Get Claims based identity for the user
                        //var identity = PlatformUserManager.GetUserClaimsIdentity(user, DefaultAuthenticationTypes.ApplicationCookie); //<-- Uses a cookie for the local web application

                        // Sign User In
                        //var currentUtc = new SystemClock().UtcNow;
                        //HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties() { IsPersistent = true, ExpiresUtc = currentUtc.Add(TimeSpan.FromHours(Sahara.Core.EnvironmentSettings.Platform.PlatformUsers.Authentication.WebApplicationCookiePersistence)) }, identity);

                        CoreServices.Platform.Initialized = true;

                        return RedirectToAction("Success");
                        /* Or sign in automatically and send to dashboard:
                        //Attempt to log in
                        var login = new Login();
                        login.Email = registerPlatformUser.Email;
                        login.Password = registerPlatformUser.Password;

                        var platformAuthenticationServiceClient = new PlatformAuthenticationService.PlatformAuthenticationServiceClient();
                        platformAuthenticationServiceClient.Open();
                        var authResponse = platformAuthenticationServiceClient.Authenticate(login.Email, login.Password);
                        platformAuthenticationServiceClient.Close();


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

                            //After user is authenticated send them to the home page:
                            return RedirectToAction("Index", "Home");

                        }
                        else
                        {
                            return RedirectToAction("Error", new { message = "Authentication error." });
                        }*/



                    }
                    else
                    {
                        //return RedirectToAction("InitializationError");
                        return RedirectToAction("Error", new { message = "Iniialization error." });
                    }


                }
                else
                {
                    return View(registerPlatformUser);
                }
            }


            [HttpGet]
            public ActionResult TestError()
            {
                return RedirectToAction("Error", new { message = "Tesing the error" });
            }

            [HttpGet]
            public string PlatformExists()
            {
                return "Platform exists!";
            }

            [HttpGet]
            public ActionResult Success()
            {
                return View();
            }
            
            [HttpGet]
            public ActionResult Error(string message)
            {
                ViewBag.Error = message;
                return View();
            }
        

            [HttpGet]
            public string InitializationError()
            {
                return "There was an error while initializing the platform. Debug and try again.";
            }
            [HttpGet]
            public string AuthenticationError()
            {
                return "Platform initialized, but there was an error while attempting to authenticate.";
            }
        }
}