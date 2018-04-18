using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    [Authorize]
    public class ScaffoldController : Controller
    {

        #region View Controllers

        // GET: /Scaffold/
        public ActionResult Index()
        {
            return View();
        }


        // Used for Detail variation
        // GET: /Scaffold/{id}
        [Route("Scaffold/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }

        #endregion


        #region JSON Services

        #region Initializaton

        [Route("Scaffold/Json/Get")]
        [HttpGet]
        public JsonNetResult GetScaffold()
        {
            /*
            
            var account = new Account();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                account = accountManagementServiceClient.GetAccount(name);

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



            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
	    jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = account;

            return jsonNetResult; 
             
            */

            var results = new string[] {"one", "two", "three", "four"};

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
	    jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = results;

            return jsonNetResult;

        }

        #endregion


        #region Details


        [Route("Scaffold/Json/Details/{id}")]
        public JsonNetResult Detail(string id)
        {
            /*
            var account = new Account();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                account = accountManagementServiceClient.GetAccount(name);

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



            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
	    jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = account;

            return jsonNetResult;
            */

            var results = new string[] {"details 1", "details 2"};

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
	    jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = results;

            return jsonNetResult;
        }

        #endregion


        #region Updates

        [Route("Accounts/Json/UpdateSomething")]
        [HttpPost]
        public JsonNetResult UpdateSomething(string id, string name)
        {
            /*
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountOwnershipStatus(accountId, userId, isOwner,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser);

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
                response.isSuccess = false;
                response.ErrorMessage = "An exception occurred while attempting to use core services";
                response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
             */

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
	    jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = "DataAccessResponseType";

            return jsonNetResult;
        }

        #endregion


        #endregion



    }
}