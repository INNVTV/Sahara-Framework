using Account.Public.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Account.Public.Website.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        [Route("search")]
        public ActionResult Index()
        {
            var mainViewModel = new MainViewModel();

            //TO DO: Allow for custom domain lookups to get accountNameKey as well:

            mainViewModel.AccountNameKey = Common.GetSubDomain(Request.Url);

            if (mainViewModel.AccountNameKey == "")
            {
                return Content("Account does not exist");
            }

            return View(mainViewModel); //<---Redirect to main index, angular will take over routing
        }

        // Used for searchMode variation
        // GET: /Search/{searchMode}
        [Route("search/{searchMode}")]
        public ActionResult SearchMode(string searchMode)
        {
            var mainViewModel = new MainViewModel();

            //TO DO: Allow for custom domain lookups to get accountNameKey as well:

            mainViewModel.AccountNameKey = Common.GetSubDomain(Request.Url);

            if (mainViewModel.AccountNameKey == "")
            {
                return Content("Account does not exist");
            }

            if (searchMode == "list" || searchMode == "map")
            {
                return View("Index", mainViewModel); //<---Redirect to main index, angular will take ouver routing
            }
            else
            {
                return null;
            }

        }
    }
}