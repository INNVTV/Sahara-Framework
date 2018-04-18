using InventoryHawk.Account.Public.Api.Models.Json.Common;
using InventoryHawk.Account.Public.Api.Models.Search;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    public class ProductsController : Controller
    {
        /*

        [Route("products/{searchTerm}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult GetProducts(string searchTerm, bool includeHidden = false)
        {

            return new JsonNetResult();

            #region Not Used

            /*

            ExecutionType executionType = ExecutionType.local;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Get the subdomain (if exists) for the api call
            string accountNameKey = Common.GetSubDomain(Request.Url);
            if (String.IsNullOrEmpty(accountNameKey))
            {
                return new JsonNetResult { Data = "Not found" }; //return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            //CategoryModel category = null;
            SearchResults searchResultsObjectJson = null;

            string localCacheKey = accountNameKey + ":products:searchTerm:" + searchTerm + ":public";

            if (includeHidden == true)
            {
                localCacheKey = accountNameKey + ":products:searchTerm:" + searchTerm + ":private";
            }

            #region (Plan A) Get json from local cache

            try
            {
                searchResultsObjectJson = (SearchResults)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (searchResultsObjectJson == null)
            {
                #region (Plan B) Get Public json from search index

                var account = Common.GetAccountObject(accountNameKey);

                //string filter = "(" + propertyName + " eq '" + propertyValue + "')";

                var searchResults = DataAccess.Search.SearchProducts(account, searchTerm, null, "orderId asc");

                #endregion

                #region Transform into json object, add images & cache locally


                //Transform categories into JSON and cache locally
                searchResultsObjectJson = searchResults;
                //searchResultsObjectJson = Transforms.Json.CategorizationTransforms.Category(accountNameKey, category);
                HttpRuntime.Cache.Insert(localCacheKey, searchResultsObjectJson, null, DateTime.Now.AddMinutes(Common.SearchResultsCacheTimeInMinutes), TimeSpan.Zero);


                #endregion

                executionType = ExecutionType.searchIndex;
            }

            //Add execution data
            stopWatch.Stop();
            //searchResultsObjectJson.executionType = executionType.ToString();
            //searchResultsObjectJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = searchResultsObjectJson;

            return jsonNetResult;

            * /


            #endregion


        }

    */
    }
}