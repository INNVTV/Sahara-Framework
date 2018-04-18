using Newtonsoft.Json;
using PlatformAdminSite.PlatformBillingService;
using PlatformAdminSite.PlatformManagementService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {

        #region View Controllers

        // GET: /Billing/
        public ActionResult Index()
        {
            return View();
        }


        // Used for Detail variation
        // GET: /Billing/{id}
        [Route("Billing/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }

        #endregion


        #region JSON Services

        #region Snapshots

        [Route("Billing/Json/GetSnapshot")]
        [HttpGet]
        public JsonNetResult GetSnapshot()
        {

            #region Get data from WCF


            var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            var infrastructureSnapshot = new BillingSnapshot();

            try
            {
                PlatformManagementServiceClient.Open();
                infrastructureSnapshot = PlatformManagementServiceClient.GetBillingShapshot(Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(PlatformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = infrastructureSnapshot;

            return jsonNetResult;

        }

        #endregion

        #region Reports


        [Route("Billing/Json/GetReport")]
        [HttpGet]
        public JsonNetResult GetReport(int sinceHoursAgo)
        {

            #region Get data from WCF


            var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
            var billingReport = new BillingReport();

            try
            {
                PlatformManagementServiceClient.Open();
                billingReport = PlatformManagementServiceClient.GetBillingReport(sinceHoursAgo, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(PlatformManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = billingReport;

            return jsonNetResult;

        }


        #endregion 


        #region Reports


        [Route("Billing/Json/GetBalanceTransactionsForSource")]
        [HttpGet]
        public JsonNetResult GetBalanceTransactionsForSource(string sourceId)
        {

            #region Get data from WCF

            var platformBillingServiceClient = new PlatformBillingService.PlatformBillingServiceClient();
            var sourceBalanceTransactions = new SourceBalanceTransactions();

            try
            {
                platformBillingServiceClient.Open();
                sourceBalanceTransactions = platformBillingServiceClient.GetBalanceTransactionsForSource(sourceId, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(platformBillingServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformBillingServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = sourceBalanceTransactions;

            return jsonNetResult;

        }


        #endregion 

        #endregion


    }
}