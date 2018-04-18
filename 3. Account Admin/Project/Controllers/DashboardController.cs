using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {

        #region View Controllers

        // GET: /Dashboard/
        public ActionResult Index()
        {
            return View();
        }


        // Used for Detail variation
        // GET: /Dashboard/{id}
        [Route("Dashboard/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }

        #endregion


        #region JSON Services

        #region Initializaton

        [Route("Dashboard/Json/Get")]
        [HttpGet]
        public JsonNetResult GetDashboard()
        {
            var results = new string[] {"one", "two", "three", "four"};

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = results;

            return jsonNetResult;

        }

        #endregion


        #region Details


        [Route("Dashboard/Json/Details/{id}")]
        public JsonNetResult Detail(string id)
        {

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