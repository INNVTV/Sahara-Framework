using Account.Public.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Account.Public.Website.Controllers
{
    public class TabletsController : Controller
    {
        // GET: "/tablets"
        //[Route("")]

        [Route("t")]
        [Route("m")]
        [Route("tablets")]
        //[Route("tablets/{categoryNameKey}")]
        //[Route("tablets/{categoryNameKey}/{subcategoryNameKey}")]
        //[Route("tablets/{categoryNameKey}/{subcategoryNameKey}/{subsubcategoryNameKey}")]
        //[Route("tablets/{categoryNameKey}/{subcategoryNameKey}/{subsubcategoryNameKey}/{subsubsubcategoryNameKey}")]

        //[Route("details/{categoryNameKey}/{productNameKey}")]
        //[Route("details/{categoryNameKey}/{subcategoryNameKey}/{productNameKey}")]
        //[Route("details/{categoryNameKey}/{subcategoryNameKey}/{subsubcategoryNameKey}/{productNameKey}")]
        //[Route("details/{categoryNameKey}/{subcategoryNameKey}/{subsubcategoryNameKey}/{subsubsubcategoryNameKey}/{productNameKey}")]

        public ActionResult Index()
        {
            var mainViewModel = new MainViewModel();

            //TO DO: Allow for custom domain lookups to get accountNameKey as well:

            mainViewModel.AccountNameKey = Common.GetSubDomain(Request.Url);

            if (mainViewModel.AccountNameKey == "")
            {
                return Content("Account does not exist");
            }

            return View(mainViewModel);
        }
    }
}