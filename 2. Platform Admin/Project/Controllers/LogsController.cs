using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    [Authorize]
    public class LogsController : Controller
    {

        #region View Controllers

        // GET: /Logs/
        public ActionResult Index()
        {
            // Log JSON feeds are in PlatformController
            return View();
        }

        #endregion

    }
}