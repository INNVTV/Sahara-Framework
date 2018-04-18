using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    [Authorize]
    public class MarketplaceController : Controller
    {

        #region View Controllers

        // GET: /Marketplace/
        public ActionResult Index()
        {
            return View();
        }


        // Used for Detail variation
        // GET: /Marketplace/{id}
        [Route("Marketplace/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }

        #endregion

        //No JSON Feeds (Angular uses shared CommerceController/Services for better centralization)
    }
}