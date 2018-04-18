using PlatformAdminSite.PlatformManagementService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    public class ClosedAccountsController : Controller
    {
        /*
        // GET: /ClosedAccounts/Deprovisioned
        public ActionResult Deprovisioned()
        {
            var logs = new List<ClosedAccount>();
            var platformLogsServiceClient = new PlatformLogsService.PlatformLogsServiceClient();

            try
            {
                platformLogsServiceClient.Open();
                logs = platformLogsServiceClient.GetClosedAccounts(PlatformLogsService.ClosedAccountTypes.Deprovisioned, 300).ToList();

                //Close the connection
                WCFManager.CloseConnection(platformLogsServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformLogsServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            return View(logs);

        }

        // GET: /ClosedAccounts/Unverified
        public ActionResult Unverified()
        {
            var logs = new List<ClosedAccount>();
            var platformLogsServiceClient = new PlatformLogsService.PlatformLogsServiceClient();

            try
            {
                platformLogsServiceClient.Open();
                logs = platformLogsServiceClient.GetClosedAccounts(PlatformLogsService.ClosedAccountTypes.Unverified, 300).ToList();

                //Close the connection
                WCFManager.CloseConnection(platformLogsServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." +currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(platformLogsServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            return View(logs);
        }

        */
    }
}