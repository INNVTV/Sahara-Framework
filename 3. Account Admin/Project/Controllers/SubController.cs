using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    public class SubController : Controller
    {
        // GET: Sub (Used to subscribe users)
        [Route("sub/{planName}/{frequencyMonths}/{accountId}")]
        public ActionResult Index(string planName, int frequencyMonths, string accountId)
        {
            ViewBag.planName = planName;
            ViewBag.frequencyMonths = frequencyMonths;
            ViewBag.accountId = accountId;

            return View();
        }
    }
}