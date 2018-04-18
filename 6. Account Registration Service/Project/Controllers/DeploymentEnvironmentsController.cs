using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Sahara.Api.Accounts.Registration.Controllers
{
    [RoutePrefix("DeploymentEnvironments")]
    public class DeploymentEnvironmentsController : ApiController
    {
        [Route("Local")]
        [HttpGet]
        public HttpResponseMessage Local()
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            httpResponse = Request.CreateResponse(HttpStatusCode.OK, EnvironmentSettings.CurrentEnvironment.Local);

            return httpResponse;
        }

        [Route("CoreServices")]
        [HttpGet]
        public HttpResponseMessage CoreServices()
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            httpResponse = Request.CreateResponse(HttpStatusCode.OK, EnvironmentSettings.CurrentEnvironment.CoreServices);

            return httpResponse;
        }
    }
}
