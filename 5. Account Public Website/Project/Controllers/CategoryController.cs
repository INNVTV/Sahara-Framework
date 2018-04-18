using Account.Public.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Account.Public.Website.Controllers
{
    public class CategoryController : Controller
    {
        // GET: "/category/{id}"
        [Route("category/{categoryNameKey}")]
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
    }
}