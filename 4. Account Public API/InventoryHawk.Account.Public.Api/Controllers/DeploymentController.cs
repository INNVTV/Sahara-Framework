using InventoryHawk.Account.Public.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Cors;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    [RoutePrefix("Deployment")]
    public class DeploymentController : ApiController
    {
        #region Fiddler Settings

        /*
         * [POST] url/accountname
         *
         * Content-type: application/x-www-form-urlencoded
         * Authorization: Basic 
         * 
         * 
         * REQUEST BODY:
         * AccountName=Account Name
         * 
         */

        #endregion

        //Get ALL deployment details "/deployment"
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage Get()
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            //Get the subdomain (if exists) for the api call
            string subdomain = Common.GetSubDomain(Request.RequestUri);

            var deployment = new DeploymentModel
            {
                Subdomain = subdomain,
                Environment = new EnvironmentModel
                {
                    Local = EnvironmentSettings.CurrentEnvironment.Site,
                    CoreServices = EnvironmentSettings.CurrentEnvironment.CoreServices
                }
            };

            //var json = LowercaseJsonSerializer.SerializeObject(deployment);
            /*
            var json = JsonConvert.SerializeObject(deployment,
                Formatting.Indented,
                new JsonSerializerSettings {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }
                );*/

            httpResponse = Request.CreateResponse(HttpStatusCode.OK, deployment);

            return httpResponse;
        }

        //Get partial settings "/deployment/{name}"
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("Environment/Local")]
        [HttpGet]
        public HttpResponseMessage EnvironmentLocal()
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            httpResponse = Request.CreateResponse(HttpStatusCode.OK, EnvironmentSettings.CurrentEnvironment.Site);

            return httpResponse;
        }

        //Get partial settings "/deployment/{name}"
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("Environment/Core")]
        [HttpGet]
        public HttpResponseMessage EnvironmentCore()
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            httpResponse = Request.CreateResponse(HttpStatusCode.OK, EnvironmentSettings.CurrentEnvironment.CoreServices);

            return httpResponse;
        }



    }
}
