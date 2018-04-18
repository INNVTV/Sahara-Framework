using InventoryHawk.Account.Public.Api.ApplicationProductService;
using InventoryHawk.Account.Public.Api.Models.Json.Common;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.DataAccess
{
    public static class Product
    {
        public static IDictionary<string, object> GetProductDetail(string accountNameKey, string idProperty, string idValue)
        {

            var account = Common.GetAccountObject(accountNameKey);

            //ToDo:
            //Determine propertyType and build filter accordingly. Reject Paragraphs, Etc...

            string filter = "(" + idProperty + " eq '" + idValue + "')";

            var searchResults = DataAccess.Search.SearchProducts(account, null, filter, "orderId asc", 0, 1, false, null, true);

            if(searchResults.Results.Count > 0)
            {
                return Dynamics.Products.TransformDynamicProductDetailsForJson(searchResults.Results[0]);
            }
            else
            {
                return null;
            }

            #region Moved to using search results

            /*

            IDictionary<string, object> product = new System.Dynamic.ExpandoObject();

            ProductDocumentModel productDocument = null;

            #region Get Object(s) from DocumentDB using the accounts DocumentPartitionID

            //var client = CoreServices.DocumentDatabases.Accounts_DocumentClient;
            //CoreServices.DocumentDatabases.Accounts_DocumentClient.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(CoreServices.PlatformSettings.DocumentDB.AccountPartitionsDatabaseId, accountNameKey);

            string sqlQuery = String.Empty;

            if (idProperty.ToLower() == "fullyqualifiedname")
            {
                sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + idValue + "'";
            }
            else if(idProperty.ToLower() == "id")
            {
                sqlQuery = "SELECT * FROM Products p WHERE p.id ='" + idValue + "'";
            }
            else
            {
                return null;
            }

            var productResults = CoreServices.DocumentDatabases.Accounts_DocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery);

            //var accountCollection = client.Crea

            //applicationImages = result.ToList();
            productDocument = productResults.AsEnumerable().FirstOrDefault();

            if(productDocument == null)
            {
                return null;
            }

            #endregion


            product["id"] = productDocument.Id;
            product["name"] = productDocument.Name;
            product["NameKey"] = productDocument.NameKey;
            product["locationPath"] = productDocument.LocationPath;
            product["fullyQualifiedName"] = productDocument.FullyQualifiedName;
            product["orderId"] = productDocument.OrderID;

            product["visible"] = productDocument.Visible;

            product["categoryName"] = productDocument.CategoryName;
            product["categoryNameKey"] = productDocument.CategoryNameKey;
            product["subcategoryName"] = productDocument.SubcategoryName;
            product["subcategoryNameKey"] = productDocument.SubcategoryNameKey;
            product["subsubcategoryName"] = productDocument.SubsubcategoryName;
            product["subsubcategoryNameKey"] = productDocument.SubsubcategoryNameKey;
            product["subsubsubcategoryName"] = productDocument.SubsubsubcategoryName;
            product["subsubsubcategoryNameKey"] = productDocument.SubsubsubcategoryNameKey;

            product["tags"] = productDocument.Tags;

            product["dateCreated"] = productDocument.DateCreated;

            try
            {
                foreach (KeyValuePair<string, string> property in productDocument.Properties)
                {
                    product[property.Key] = property.Value;
                }
            }
            catch
            {

            }

            try
            {
                foreach (KeyValuePair<string, string[]> predefined in productDocument.Predefined)
                {
                    product[predefined.Key] = predefined.Value;
                }
            }
            catch
            {

            }


            try
            {
                foreach (KeyValuePair<string, Swatch[]> swatch in productDocument.Swatches)
                {
                    product[swatch.Key] = swatch;
                }
            }
            catch
            {

            }


            try
            {
                foreach (KeyValuePair<string, PropertyLocationValue> location in productDocument.Locations)
                {
                    product[location.Key] = location;
                }
            }
            catch
            {

            }

            

            product["images"] = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "product", productDocument.Id, false);

            return product;

            */

            /*
            ///Switch to using DocumentDB ----
            ///

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

            string localCacheKey = accountNameKey + ":product:" + propertyValue;

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

                string filter = "(" + propertyName + " eq '" + propertyValue + "')";

                var searchResults = DataAccess.Products.Search(account, null, filter, "relevance", 0, 50, false, null, true);

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

            return jsonNetResult;*/

            #endregion
        }
    }
}