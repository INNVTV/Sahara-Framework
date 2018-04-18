using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountRegistration.Controllers
{
    public class RegistrationController : Controller
    {

        #region View Controllers

        // GET: /Registration/
        public ActionResult Index()
        {
            return View();
        }


        // Used for Detail variation
        // GET: /Registration/{id}
        [Route("Registration/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }

        #endregion

    }
}