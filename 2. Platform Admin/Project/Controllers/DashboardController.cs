using PlatformAdminSite.PlatformManagementService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Helpers;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {

        // GET: /Dashboard/
        public ActionResult Index()
        {
            //Check if Platform exists
            //TO DO: Move this to a static variable that checks on startup
            var platformInitializationServiceClient = new PlatformInitializationService.PlatformInitializationServiceClient();
            platformInitializationServiceClient.Open();
            var isInitialized = platformInitializationServiceClient.IsPlatformInitialized();

            //Close the connection
            WCFManager.CloseConnection(platformInitializationServiceClient);


            if (!isInitialized)
            {
                //If platform does not exist then rederect to Platform Initialization Controller
                return RedirectToAction("Index", "Initialization");
            }

            

            return View(GetAccountsSnapshot());
        }

        #region Chart Generation

        public ActionResult GenerateAccountDistributionChart()
        {
            var accountsSnapshot = GetAccountsSnapshot();

            #region Create xValues & yValues (we only use data that has more than 0 to avoid orphaned labels)

            var pieLabels = new List<String>();
            var pieValues = new List<String>();
            var pieColors = new List<String>();

            if (accountsSnapshot.Counts.PaidUp > 0)
            {
                var count = accountsSnapshot.Counts.PaidUp.ToString();

                pieLabels.Add("");//("Subscribed (" + count + ")");
                pieValues.Add(count);
                pieColors.Add("green");
            }

            if (accountsSnapshot.Counts.Unprovisioned > 0)
            {
                var count = accountsSnapshot.Counts.Unprovisioned.ToString();
                pieLabels.Add("");//("Unprovisioned (" + count + ")");
                pieValues.Add(count);
                pieColors.Add("#C9C9C9");
            }

            if (accountsSnapshot.Counts.PastDue > 0)
            {
                var count = accountsSnapshot.Counts.PastDue.ToString();
                pieLabels.Add("");//("Past Due (" + count + ")");
                pieValues.Add(count);
                pieColors.Add("orange");
            }

            if (accountsSnapshot.Counts.Unpaid > 0)
            {
                var count = accountsSnapshot.Counts.Unpaid.ToString();
                pieLabels.Add("");// ("Unpaid (" + count + ")");
                pieValues.Add(count);
                pieColors.Add("red");
            }

            if (pieColors.Count == 0)
            {
                //No Accounts
                pieLabels.Add("");// ("Unpaid (" + count + ")");
                pieValues.Add("100");
                pieColors.Add("#EDEDED");
            }

            var themeColors = new StringBuilder();

            foreach (string color in pieColors)
            {
                themeColors.Append(color + "; ");
            }

            #endregion

            #region Pie Theme & Colors

            var themeBuilder = new StringBuilder();

            themeBuilder.Append("<Chart Palette='None' PaletteCustomColors='" + themeColors.ToString() + "'>");

            //Hide labels
            //themeBuilder.Append("<Series>");
            //themeBuilder.Append("<Series Name='Series1' ChartType='Pie' CustomProperties='PieLabelStyle=Disabled'>");
            //themeBuilder.Append("</Series>");
            //themeBuilder.Append("</Series>");

            themeBuilder.Append("</Chart>");

            #region WIP Theme
            /*
            themeBuilder.Append("<Chart BackColor='#58A5CB' BackGradientStyle='TopBottom' BackSecondaryColor='White' BorderColor='26, 59, 105' BorderlineDashStyle='Solid' BorderWidth='2' Palette='None' PaletteCustomColors='Red'>");         
            themeBuilder.Append("</Chart>");
            <ChartAreas>
                <ChartArea Name=""Default"" _Template_=""All"" BackColor=""64, 165, 191, 228"" BackGradientStyle=""TopBottom"" BackSecondaryColor=""White"" BorderColor=""64, 64, 64, 64"" BorderDashStyle=""Solid"" ShadowColor=""Transparent"" /> 
            </ChartAreas>
            <Legends>
                <Legend _Template_=""All"" BackColor=""Transparent"" Font=""Trebuchet MS, 8.25pt, style=Bold"" IsTextAutoFit=""False"" /> 
            </Legends>
            <BorderSkin SkinStyle=""Emboss"" />   "");
            */

            //var themeBuilder = new StringBuilder();
            //themeBuilder.Append();

            #endregion

            var AccountDistributionTheme = themeBuilder.ToString();

            #endregion


            var chart = new Chart(width: 300, height: 300, theme: AccountDistributionTheme);

            chart.AddSeries(
                chartType: "pie",
                xValue: pieLabels.ToArray(),
                yValues: pieValues.ToArray()
                );

            var bytes = chart.GetBytes("png");

            return File(bytes, "image/png");

        }

        #endregion



        public static AccountsSnapshot GetAccountsSnapshot()
        {

            var accountsSnapshotLoclCacheKey = "accountsSnapshotcachekey";

            var accountsSnapshot = new AccountsSnapshot();

            if (System.Web.HttpRuntime.Cache.Get(accountsSnapshotLoclCacheKey) == null)
            {
                //if local cache is empty

                var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {
                    platformManagementServiceClient.Open();

                    accountsSnapshot = platformManagementServiceClient.GetAccountsShapshot(Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(platformManagementServiceClient);

                    //Store snapshot in local cache for 5 seconds (for charting components to use):
                    System.Web.HttpRuntime.Cache.Insert(accountsSnapshotLoclCacheKey, accountsSnapshot, null, DateTime.Now.AddSeconds(5), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);

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
            }
            else
            {
                // Get snapshot from cache
                accountsSnapshot = (AccountsSnapshot)System.Web.HttpRuntime.Cache.Get(accountsSnapshotLoclCacheKey);
            }


            return accountsSnapshot;
        }





        // Used for Detail variation
        // GET: /Dashboard/{id}
        //[Route("Dashboard/{id}")]
        //public ActionResult Details()
        //{
            //return View("Index"); //<---Redirect to main index, angular will take ouver routing
        //}



        #region JSON Services

        /*

        #region Initializaton

        [Route("Dashboard/Json/Get")]
        [HttpGet]
        public JsonNetResult GetDashboard()
        {

            var results = new string[] { "one", "two", "three", "four" };

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
         * jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = results;

            return jsonNetResult;

        }

        #endregion


        #region Details


        [Route("Dashboard/Json/Details/{id}")]
        public JsonNetResult Detail(string id)
        {

            var results = new string[] { "details 1", "details 2" };

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
         * jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = results;

            return jsonNetResult;
        }

        #endregion


        #region Updates

        [Route("Accounts/Json/UpdateSomething")]
        [HttpPost]
        public JsonNetResult UpdateSomething(string id, string name)
        {

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
         * jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = "DataAccessResponseType";

            return jsonNetResult;
        }

        #endregion

        */
        #endregion



    }
}

