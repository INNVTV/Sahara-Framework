using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountRegistration.Controllers
{
    /// <summary>
    /// This controller allows you to check settings once deployed
    /// </summary>
    public class DeploymentEnvironmentsController : Controller
    {
        [HttpGet]
        public string Local()
        {
            return EnvironmentSettings.CurrentEnvironment.Site;

        }
        public string CoreServices()
        {
            return EnvironmentSettings.CurrentEnvironment.CoreServices;
        }
    }
}