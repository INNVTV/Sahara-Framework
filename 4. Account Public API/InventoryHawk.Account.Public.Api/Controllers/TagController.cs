using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    public class TagController : Controller
    {

        #region POST (w/ APIKey)

        #region  Fiddler Settings

        /*
        
        https://[accountNameKey].[domain].com/tag
         
        [PUT][DELETE]
        Content-Type: application/x-www-form-urlencoded

        RequestBody:
        apiKey=[apiKey]&propertyNameKey=[nameKey]&propertyValue=[value]
        */

        #endregion

        [Route("tag")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpPut]
        public HttpStatusCode CreateTag(string apiKey, string tagName)
        {
            return HttpStatusCode.OK;
        }

        [Route("tag")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpDelete]
        public HttpStatusCode DeleteTag(string apiKey, string tagName)
        {
            return HttpStatusCode.OK;
        }

        #endregion


        //Get tag "/tag/{id}"
        /*
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage Get()
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            //Get the subdomain (if exists) for the api call
            string subdomain = Common.GetSubDomain(Request.RequestUri);

            JObject tags = JObject.Parse("{'tag':{'id':'02872db6-18fe-4ff4-9635-86123e88ac5e','name':'test','products':[]}, 'generated':'5/12/2013 11:24:38 PM'}");

            httpResponse = Request.CreateResponse(HttpStatusCode.OK, tags);

            return httpResponse;
        }*/
    }
}
