using AccountAdminSite.AccountManagementService;
using AccountAdminSite.ApplicationProductService;
using AccountAdminSite.ApplicationPropertiesService;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using AccountAdminSite.Models.Product;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace AccountAdminSite.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {

        #region View Controllers

        // GET: /Inventory/
        public ActionResult Index()
        {
            return View();
        }


        // Used for Detail variation
        // GET: /Inventory/{id}
        [Route("Inventory/{id}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing
        }

        // Used for Category Details variation
        // GET: /Category/
        [Route("Categories")]
        public ActionResult CategoryList()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing (via local "app.js" file)
        }

        // Used for Category Details variation
        // GET: /Category/
        [Route("Category/{categoryNameKey}")]
        public ActionResult CategoryDetails()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing (via local "app.js" file)
        }

        // Used for Subcategory Details variation
        // GET: /Subcategory/
        [Route("Subcategory/{categoryNameKey}/{subcategoryNameKey}")]
        public ActionResult SubcategoryDetails()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing (via local "app.js" file)
        }

        // Used for Subsubcategory Details variation
        // GET: /Subsubcategory/
        [Route("Subsubcategory/{categoryNameKey}/{subcategoryNameKey}/{subsubcategoryNameKey}")]
        public ActionResult SubsubcategoryDetails()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing (via local "app.js" file)
        }

        // Used for Subsubsubcategory Details variation
        // GET: /Subsubsubcategory/
        [Route("Subsubsubcategory/{categoryNameKey}/{subcategoryNameKey}/{subsubcategoryNameKey}/{subsubsubcategoryNameKey}")]
        public ActionResult SubsubsubcategoryDetails()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing (via local "app.js" file)
        }

        #region Product Variations
        [Route("Item/{categoryNameKey}/{productNameKey}")]
        public ActionResult ProductDetails1()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing (via local "app.js" file)
        }
        [Route("Item/{categoryNameKey}/{subCategoryNameKey}/{productNameKey}")]
        public ActionResult ProductDetails2()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing (via local "app.js" file)
        }
        [Route("Item/{categoryNameKey}/{subCategoryNameKey}/{subSubCategoryNameKey}/{productNameKey}")]
        public ActionResult ProductDetails3()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing (via local "app.js" file)
        }
        [Route("Item/{categoryNameKey}/{subCategoryNameKey}/{subSubCategoryNameKey}/{subSubSubCategoryNameKey}/{productNameKey}")]
        public ActionResult ProductDetails4()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing (via local "app.js" file)
        }

        #endregion

        #endregion

        #region JSON Services for Products

        #region Get

        #region Switched to using search
        /*
[Route("Products/Json/GetProducts")]
[HttpGet]
public JsonNetResult GetProducts(string locationPath)
{

    //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
    var accountNameKey = Common.GetSubDomain(Request.Url);

    //ProductResults productResults = null;
    List<ProductDocumentModel> products = null;

    //We do not use WCF to get a list of images, to keep the load off WCF we pull down a copy of the Account object from Redis Cache it for 5-10 minutes and use the SelfLinks to get a list of ApplicationImage directly from DocumentDB
    Account account = Common.GetAccountObject(accountNameKey);

    if (account != null)
    {
        #region Get Object(s) from DocumentDB using the accounts DocumentPartitionID

        //var client = CoreServices.DocumentDatabases.Accounts_DocumentClient;
        //CoreServices.DocumentDatabases.Accounts_DocumentClient.OpenAsync();

        //Build a collection Uri out of the known IDs
        //(These helpers allow you to properly generate the following URI format for Document DB:
        //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
        Uri collectionUri = UriFactory.CreateDocumentCollectionUri(CoreServices.PlatformSettings.DocumentDB.AccountPartitionsDatabaseId, account.DocumentPartition);

        string sqlQuery = "SELECT * FROM Products p WHERE p.DocumentType ='Product' AND p.LocationPath='" + locationPath + "' ORDER BY p.Name Asc";

        var productResults = CoreServices.DocumentDatabases.Accounts_DocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery);

        //var accountCollection = client.Crea

        //applicationImages = result.ToList();
        products = productResults.ToList();

        /*============================================================================================================================================================
          Since we can only order by 1 item we check and see if we also have to order by OrderID on the product list by seeing if any of the order ids are > 0
        ============================================================================================================================================================* /
        if(products.Any(p => p.OrderID > 0))
        {
            products = products.OrderBy(p => p.OrderID).ToList();
        }

        #endregion
    }
    else
    {
        //Could not resolve Account
    }

    #region Legacy Options
    /*

    #region (Plan A) Get data from Redis Cache

    try
    {
        //First we attempt to get tags from the Redis Cache

        IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

        string hashKey = accountNameKey + ":products";
        string hashField = "list:category:categoryname";

        var redisValue = cache.HashGet(hashKey, hashField);

        if (redisValue.HasValue)
        {
            productResults = JsonConvert.DeserializeObject<ProductResults>(redisValue);
        }


    }
    catch (Exception e)
    {
        var error = e.Message;
        //Log error message for Redis call
    }

    #endregion

    if (productResults == null)
    {
        #region (Plan B) Get data from DocumentDB

        #endregion
    }

    if (productResults == null)
    {
        #region (Plan C) Get data from WCF

        var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();

        try
        {
            applicationProductServiceClient.Open();
            productResults = applicationProductServiceClient.GetProducts(accountNameKey);

            //Close the connection
            WCFManager.CloseConnection(applicationProductServiceClient);

        }
        catch (Exception e)
        {
            #region Manage Exception

            string exceptionMessage = e.Message.ToString();

            var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
            string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

            // Abort the connection & manage the exception
            WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

            #endregion
        }

        #endregion
    }

    * /
        #endregion

    JsonNetResult jsonNetResult = new JsonNetResult();
    jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
    jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
    jsonNetResult.Data = products;

    return jsonNetResult;

}
*/
        #endregion


        [Route("Products/Json/GetProduct")]
        [HttpGet]
        public JsonNetResult GetProduct(string fullyQualifiedName)
        {

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            //ProductResults productResults = null;
            ProductDocumentModel product = null;

            //We do not use WCF to get a list of images, to keep the load off WCF we pull down a copy of the Account object from Redis Cache it for 5-10 minutes and use the SelfLinks to get a list of ApplicationImage directly from DocumentDB

            Account account = Common.GetAccountObject(accountNameKey);

            if (account != null)
            {
                #region Get Object(s) from DocumentDB using the accounts DocumentPartitionID

                //var client = CoreServices.DocumentDatabases.Accounts_DocumentClient;
                //CoreServices.DocumentDatabases.Accounts_DocumentClient.OpenAsync();

                //Build a collection Uri out of the known IDs
                //(These helpers allow you to properly generate the following URI format for Document DB:
                //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(CoreServices.PlatformSettings.DocumentDB.AccountPartitionsDatabaseId, account.DocumentPartition);

                string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "'";

                //var feedOptions = new FeedOptions { EnableCrossPartitionQuery = false };

                // Tools > Options > Projects & Solutions > WebProjects > CHECK BOX FOR: Use 64 Bit Version of IIS Express for website & Projects
                var productResults = CoreServices.DocumentDatabases.Accounts_DocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery); //, feedOptions);

                //var accountCollection = client.Crea

                //applicationImages = result.ToList();
                try
                {
                    product = productResults.AsEnumerable().FirstOrDefault();
                }
                catch(Exception e)
                {
                    var exc = e.Message;
                }


                #endregion
            }
            else
            {
                //Could not resolve Account
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = product;

            return jsonNetResult;

        }


        /// <summary>
        /// Only used by lead details to get latest product details
        /// </summary>
        /// <param name="fullyQualifiedName"></param>
        /// <returns></returns>
        [Route("Products/Json/GetProductById")]
        [HttpGet]
        public JsonNetResult GetProductById(string id)
        {

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            //ProductResults productResults = null;
            ProductDocumentModel product = null;

            //We do not use WCF to get a list of images, to keep the load off WCF we pull down a copy of the Account object from Redis Cache it for 5-10 minutes and use the SelfLinks to get a list of ApplicationImage directly from DocumentDB

            Account account = Common.GetAccountObject(accountNameKey);

            if (account != null)
            {
                #region Get Object(s) from DocumentDB using the accounts DocumentPartitionID

                //var client = CoreServices.DocumentDatabases.Accounts_DocumentClient;
                //CoreServices.DocumentDatabases.Accounts_DocumentClient.OpenAsync();

                //Build a collection Uri out of the known IDs
                //(These helpers allow you to properly generate the following URI format for Document DB:
                //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(CoreServices.PlatformSettings.DocumentDB.AccountPartitionsDatabaseId, account.DocumentPartition);

                string sqlQuery = "SELECT * FROM Products p WHERE p.id ='" + id + "'";

                var productResults = CoreServices.DocumentDatabases.Accounts_DocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery);

                //var accountCollection = client.Crea

                //applicationImages = result.ToList();
                product = productResults.AsEnumerable().FirstOrDefault();

                #endregion
            }
            else
            {
                //Could not resolve Account
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = product;

            return jsonNetResult;

        }

        [Route("Products/Json/GetProductProperties")]
        [HttpGet]
        public JsonNetResult GetProductProperties(string fullyQualifiedName)
        {

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url);

            //ProductResults productResults = null;
            ProductDocumentModel product = null;

            //We do not use WCF to get a list of images, to keep the load off WCF we pull down a copy of the Account object from Redis Cache it for 5-10 minutes and use the SelfLinks to get a list of ApplicationImage directly from DocumentDB

            Account account = Common.GetAccountObject(accountNameKey);

            if (account != null)
            {
                #region Get Object(s) from DocumentDB using the accounts DocumentPartitionID

                //var client = CoreServices.DocumentDatabases.Accounts_DocumentClient;
                //CoreServices.DocumentDatabases.Accounts_DocumentClient.OpenAsync();

                //Build a collection Uri out of the known IDs
                //(These helpers allow you to properly generate the following URI format for Document DB:
                //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(CoreServices.PlatformSettings.DocumentDB.AccountPartitionsDatabaseId, account.DocumentPartition);

                string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "'";

                var productResults = CoreServices.DocumentDatabases.Accounts_DocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery);

                //var accountCollection = client.Crea

                //applicationImages = result.ToList();
                product = productResults.AsEnumerable().FirstOrDefault();

                #endregion
            }
            else
            {
                //Could not resolve Account
            }

            //Merge Properties List with Product Properties
            var properties = SettingsCommon.GetProperties(accountNameKey);

            var productProperties = new List<ProductProperty>();

            foreach (PropertyModel property in properties)
            {
                //Create a local representation of PropertyModel
                var productProperty = new ProductProperty();

                productProperty.ValueType = property.PropertyTypeNameKey;
                productProperty.Name = property.PropertyName;
                productProperty.NameKey = property.PropertyNameKey;

                productProperty.Appendable = property.Appendable;

                productProperty.Symbol = property.Symbol;
                productProperty.SymbolPlacement = property.SymbolPlacement;

                switch(productProperty.ValueType)
                {
                    case "string":
                    case "number":
                    case "paragraph":
                        #region String, Paragraph & Number Types

                        //See if a coresponding version exists on the product apply it's value
                        if (product.Properties != null)
                        {
                            foreach (System.Collections.Generic.KeyValuePair<string, string> propertyValue in product.Properties)
                            {
                                if (propertyValue.Key == property.PropertyName)
                                {
                                    productProperty.AssignedValue = propertyValue.Value;

                                }
                            }
                        }
                        
                        #endregion
                        break;

                    case "datetime":
                        #region DateTime Types

                        //See if a coresponding version exists on the product apply it's value
                        if (product.Properties != null)
                        {
                            foreach (System.Collections.Generic.KeyValuePair<string, string> propertyValue in product.Properties)
                            {
                                if (propertyValue.Key == property.PropertyName)
                                {
                                    productProperty.AssignedValue = propertyValue.Value;

                                }
                            }
                        }
                        
                        #endregion
                        break;

                    case "predefined":
                        #region Predefined Types

                        productProperty.PredefinedValues = new List<string>();

                        foreach (PropertyValueModel value in property.Values)
                        {
                            productProperty.PredefinedValues.Add(value.PropertyValueName);
                        }
                        
                        //See if a coresponding version exists on the product apply it's value
                        if (product.Predefined != null)
                        {
                            foreach (System.Collections.Generic.KeyValuePair<string, string[]> propertyValue in product.Predefined)
                            {
                                if (propertyValue.Key == property.PropertyName)
                                {
                                    if (propertyValue.Value.Length > 0)
                                    {
                                        productProperty.AssignedValues = new List<string>();
                                        foreach (string value in propertyValue.Value)
                                        {
                                            productProperty.AssignedValues.Add(value);
                                        }
                                    }

                                }
                            }
                        }

                        #endregion
                        break;

                    case "swatch":
                        #region Swatch types
                    
                            productProperty.AvailableSwatches = new List<SwatchObject>();

                            foreach (PropertySwatchModel swatch in property.Swatches)
                            {
                                productProperty.AvailableSwatches.Add(new SwatchObject
                                {
                                    Label = swatch.PropertySwatchLabel,
                                    Image = swatch.PropertySwatchImageMedium,
                                    NameKey = swatch.PropertySwatchNameKey
                                });
                            }
                    
                            //See if a coresponding version exists on the product apply it's value
                            if (product.Swatches != null)
                            {
                                productProperty.AssignedSwatches = new List<SwatchObject>();

                                foreach (System.Collections.Generic.KeyValuePair<string, Swatch[]> swatchArray in product.Swatches)
                                {
                                    if (swatchArray.Key == property.PropertyName)
                                    {
                                        if (swatchArray.Value.Length > 0)
                                        {                                    
                                            foreach (Swatch value in swatchArray.Value)
                                            {
                                                productProperty.AssignedSwatches.Add(
                                                    new SwatchObject
                                                    {
                                                        Label = value.Label,
                                                        Image = value.ImageMedium,
                                                        NameKey= ""
                                                    }
                                                );
                                            }
                                        }

                                    }
                                }
                    
                            }

                        #endregion
                        break;

                    case "location":
                        #region Location Types
                        //See if a coresponding version exists on the product apply it's value
                        if (product.Locations != null)
                        {
                            foreach (System.Collections.Generic.KeyValuePair<string, PropertyLocationValue> propertyLocationValue in product.Locations)
                            {
                                if (propertyLocationValue.Key == property.PropertyName)
                                {
                                    productProperty.LocationData = new LocationObject
                                    {
                                        MapUrl= "https://www.google.com/maps/embed/v1/place?" +
                                            "q=" +
                                            propertyLocationValue.Value.Lat + ", " +
                                            propertyLocationValue.Value.Long + "&zoom=16&key=" + 
                                            CoreServices.PlatformSettings.GoogleMapsAPIKey,

                                        Name = propertyLocationValue.Value.Name,
                                        Address1 = propertyLocationValue.Value.Address1,
                                        Address2 = propertyLocationValue.Value.Address2,
                                        City = propertyLocationValue.Value.City,
                                        State = propertyLocationValue.Value.State,
                                        PostalCode = propertyLocationValue.Value.PostalCode,
                                        Country = propertyLocationValue.Value.Country,
                                        Lat = propertyLocationValue.Value.Lat,
                                        Long = propertyLocationValue.Value.Long,
                                    };
                                }
                            }
                        }

                        //Test data (REMOVE!) =================== REMOVE  ---------  R E M O V E ! ! ! ! ! ----------
                        /*
                        if (product.Locations == null)
                        {
                            //foreach (System.Collections.Generic.KeyValuePair<string, PropertyLocationValue> propertyLocationValue in product.Locations)
                            //{
                                //if (propertyLocationValue.Key == property.PropertyName)
                                //{
                                    productProperty.LocationData = new LocationObject
                                    {
                                        MapUrl = "https://www.google.com/maps/embed/v1/place?" +
                                            "q=34.160785, " +
                                            "-118.436378" + "&zoom=16&key=" +
                                            CoreServices.PlatformSettings.GoogleMapsAPIKey,

                                        Name = "The house",
                                        Address1 = "13931 Morrison St.",
                                        Address2 = null,
                                        City = "Sherman Oals",
                                        State = "CA",
                                        PostalCode = "91423",
                                        Lat = "34.160785",
                                        Long = "-118.436378",
                                    };
                                //}
                            //}
                        }*/


                        #endregion
                        break;
                }

                productProperties.Add(productProperty);
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = productProperties;

            return jsonNetResult;

        }

        #endregion

        #region Search (Depricated DocumentDB Search)
        /*
        [Route("Products/Json/SearchProducts")]
        [HttpGet]
        public JsonNetResult SearchProducts(string queryString, List<string> tags, Dictionary<string,string> properties)
        {

            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            var accountNameKey = Common.GetSubDomain(Request.Url).ToLower();

            //ProductResults productResults = null;
            List<ProductDocumentModel> products = null;

            //We do not use WCF to get a list of images, to keep the load off WCF we pull down a copy of the Account object from Redis Cache it for 5-10 minutes and use the SelfLinks to get a list of ApplicationImage directly from DocumentDB

            Account account = Common.GetAccountObject(accountNameKey);

            if (account != null)
            {
                #region Get Object(s) from DocumentDB using the accounts DocumentPartitionID

                //var client = CoreServices.DocumentDatabases.Accounts_DocumentClient;
                //CoreServices.DocumentDatabases.Accounts_DocumentClient.OpenAsync();

                //Build a collection Uri out of the known IDs
                //(These helpers allow you to properly generate the following URI format for Document DB:
                //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(CoreServices.PlatformSettings.DocumentDB.AccountPartitionsDatabaseId, account.DocumentPartition);

                StringBuilder sqlQuery = new StringBuilder();

                sqlQuery.Append("SELECT TOP 30 * FROM Products p ");

                sqlQuery.Append("WHERE p.AccountID = '");
                sqlQuery.Append(account.AccountID.ToString());
                sqlQuery.Append("' AND p.DocumentType ='Product'");
                sqlQuery.Append("' AND p.Name ='Product'");


                var productResults = CoreServices.DocumentDatabases.Accounts_DocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery.ToString());

                //var accountCollection = client.Crea

                //applicationImages = result.ToList();
                products = productResults.ToList();

                #endregion
            }
            else
            {
                //Could not resolve Account
            }


            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = products;

            return jsonNetResult;
           
        } */

        #endregion

        #region Create

        [Route("Products/Json/CreateProduct")]
        [HttpPost]
        public JsonNetResult CreateProduct(string locationPath, string productName, bool isVisible)
        {

            var response = new ApplicationProductService.DataAccessResponseType();
            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();

            var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
            var accountId = authCookie.AccountID.ToString();
            var userId = authCookie.Id.ToString();

            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.CreateProduct(accountId, locationPath, productName, isVisible,
                    userId,
                    ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }


        #endregion

        #region Update

        [Route("Products/Json/UpdateProductVisibleState")]
        [HttpPost]
        public JsonNetResult UpdateProductVisibleState(string fullyQualifiedName, string productName, bool isVisible)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.UpdateProductVisibleState(accountId, fullyQualifiedName, productName, isVisible,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Products/Json/UpdateProductName")]
        [HttpPost]
        public JsonNetResult UpdateProductName(string fullyQualifiedName, string oldName, string newName)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.RenameProduct(accountId, fullyQualifiedName, oldName, newName,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Products/Json/ReorderProducts")]
        [HttpPost]
        public JsonNetResult ReorderProducts(List<string> productOrder, string locationPath)
        {
            if(productOrder.Count > 60)
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Formatting.Indented;
                jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultError.Data = new ApplicationProductService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot custom reorder sets of items greater than 60" };

                return jsonNetResultError;
            }

            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var productOrderDictionary = new Dictionary<string, int>();

            foreach (String id in productOrder)
            {
                productOrderDictionary.Add(id, productOrderDictionary.Count);
            }

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.ReorderProducts(accountId, productOrderDictionary, locationPath,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Products/Json/ResetProductOrder")]
        [HttpPost]
        public JsonNetResult ResetProductOrder(string locationPath)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.ResetProductOrdering(accountId, locationPath,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        #endregion

        #region Move

        [Route("Products/Json/MoveProduct")]
        [HttpPost]
        public JsonNetResult MoveProduct(string productId, string newLocationPath)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.MoveProduct(accountId, productId, newLocationPath,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #region DELETE

        [Route("Products/Json/DeleteProduct")]
        [HttpPost]
        public JsonNetResult DeleteProduct(string productId)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.DeleteProduct(accountId, productId,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #region Manage Properties & Tags

        [Route("Products/Json/UpdateProductProperty")]
        [HttpPost]
        public JsonNetResult UpdateProductProperty(string fullyQualifiedName, string productName, string propertyTypeNameKey, string propertyNameKey, string propertyValue, string updateType)
        {
            #region Sanitize Data

            if(propertyTypeNameKey == "datetime")
            {
                propertyValue = propertyValue.Replace(":am", "").Replace(":pm", "").Replace("\"", "");
            }

            var productPropertyUpdateType = ProductPropertyUpdateType.Replace;

            if (updateType.ToLower() == "append")
            {
                productPropertyUpdateType = ProductPropertyUpdateType.Append;
            }                       

            #endregion

            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.UpdateProductProperty(accountId, fullyQualifiedName, productName, propertyNameKey, propertyValue, productPropertyUpdateType,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Products/Json/UpdateProductLocationProperty")]
        [HttpPost]
        public JsonNetResult UpdateProductLocationProperty(string fullyQualifiedName, string productName, string propertyTypeNameKey, string propertyNameKey, string name, string address1, string address2, string city, string state, string postalCode, string country, string lat, string lng)
        {

            var propertyLocationValue = new PropertyLocationValue
            {
                Name = name,
                Address1 = address1,
                Address2 = address2,
                City = city,
                State = state,
                PostalCode = postalCode,
                Country = country,
                Lat = lat,
                Long = lng
            }; 

            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.UpdateProductLocationProperty(accountId, fullyQualifiedName, productName, propertyNameKey, propertyLocationValue,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Products/Json/RemoveProductPropertyCollectionItem")]
        [HttpPost]
        public JsonNetResult RemoveProductPropertyCollectionItem(string fullyQualifiedName, string productName, string propertyNameKey, int collectionItemIndex)
        {

            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.RemoveProductPropertyCollectionItem(accountId, fullyQualifiedName, productName, propertyNameKey, collectionItemIndex,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Products/Json/ClearProductProperty")]
        [HttpPost]
        public JsonNetResult ClearProductProperty(string fullyQualifiedName, string productName, string propertyNameKey)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.ClearProductProperty(accountId, fullyQualifiedName, productName, propertyNameKey,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Products/Json/AddProductTag")]
        [HttpPost]
        public JsonNetResult AddProductTag(string fullyQualifiedName, string productName, string tagName)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.AddProductTag(accountId, fullyQualifiedName, productName, tagName,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Products/Json/RemoveProductTag")]
        [HttpPost]
        public JsonNetResult RemoveProductTag(string fullyQualifiedName, string productName, string tagName)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationProductService.DataAccessResponseType();

            var applicationProductServiceClient = new ApplicationProductService.ApplicationProductServiceClient();
            try
            {
                applicationProductServiceClient.Open();
                response = applicationProductServiceClient.RemoveProductTag(accountId, fullyQualifiedName, productName, tagName,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationProductService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationProductServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationProductServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }



        #endregion

        #endregion

        #region JSON Services for Categories

        #region Get

        [Route("Categories/Json/GetCategoryTree")]
        [HttpGet]
        public JsonNetResult GetCategoryTree(bool includeHidden = true)
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            //var schemaId = "acc" + accountId.Replace("-", "");
            var accountNameKey = Common.GetSubDomain(Request.Url);

            List<ApplicationCategorizationService.CategoryTreeModel> categoryTree = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":categories";
                string hashField = String.Empty;

                if (includeHidden)
                {
                    hashField = "tree:private";
                }
                else
                {
                    hashField = "tree:public";
                }
                

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    categoryTree = JsonConvert.DeserializeObject<List<ApplicationCategorizationService.CategoryTreeModel>>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (categoryTree == null)
            {
                #region (Plan B) Get data from WCF

                var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();

                try
                {
                    applicationCategorizationServiceClient.Open();
                    categoryTree = applicationCategorizationServiceClient.GetCategoryTree(accountNameKey, includeHidden, Common.SharedClientKey).ToList();

                    WCFManager.CloseConnection(applicationCategorizationServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = categoryTree;

            return jsonNetResult;

        }


        [Route("Categories/Json/GetCategories")]
        [HttpGet]
        public JsonNetResult GetCategories()
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            //var schemaId = "acc" + accountId.Replace("-", "");
            var accountNameKey = Common.GetSubDomain(Request.Url);

            List<ApplicationCategorizationService.CategoryModel> categories = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":categories";
                string hashField = "list:private";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    categories = JsonConvert.DeserializeObject<List<ApplicationCategorizationService.CategoryModel>>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (categories == null)
            {
                #region (Plan B) Get data from WCF

                var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();

                try
                {
                    applicationCategorizationServiceClient.Open();
                    categories = applicationCategorizationServiceClient.GetCategories(accountNameKey, true, Common.SharedClientKey).ToList();

                    WCFManager.CloseConnection(applicationCategorizationServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = categories;

            return jsonNetResult;

        }

        [Route("Categories/Json/GetCategory")]
        [HttpGet]
        public JsonNetResult GetCategory(string categoryNameKey)
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            //var schemaId = "acc" + accountId.Replace("-", "");
            var accountNameKey = Common.GetSubDomain(Request.Url);

            ApplicationCategorizationService.CategoryModel category = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":categories";
                string hashField = categoryNameKey + ":private";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    category = JsonConvert.DeserializeObject<ApplicationCategorizationService.CategoryModel>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (category == null)
            {
                #region (Plan B) Get data from WCF

                var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();

                try
                {
                    applicationCategorizationServiceClient.Open();
                    category = applicationCategorizationServiceClient.GetCategoryByName(accountNameKey, categoryNameKey, true, Common.SharedClientKey);

                    WCFManager.CloseConnection(applicationCategorizationServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = category;

            return jsonNetResult;

        }


        [Route("Categories/Json/GetSubcategory")]
        [HttpGet]
        public JsonNetResult GetSubcategory(string categoryNameKey, string subcategoryNameKey)
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            //var schemaId = "acc" + accountId.Replace("-", "");
            var accountNameKey = Common.GetSubDomain(Request.Url);

            ApplicationCategorizationService.SubcategoryModel subcategory = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":categories";
                string hashField = categoryNameKey + "/" + subcategoryNameKey + ":private";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    subcategory = JsonConvert.DeserializeObject<ApplicationCategorizationService.SubcategoryModel>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (subcategory == null)
            {
                #region (Plan B) Get data from WCF

                var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();

                try
                {
                    applicationCategorizationServiceClient.Open();
                    subcategory = applicationCategorizationServiceClient.GetSubcategoryByNames(accountNameKey, categoryNameKey, subcategoryNameKey, true, Common.SharedClientKey);

                    WCFManager.CloseConnection(applicationCategorizationServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = subcategory;

            return jsonNetResult;

        }

        [Route("Categories/Json/GetSubsubcategory")]
        [HttpGet]
        public JsonNetResult GetSubsubcategory(string categoryNameKey, string subcategoryNameKey, string subsubcategoryNameKey)
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            //var schemaId = "acc" + accountId.Replace("-", "");
            var accountNameKey = Common.GetSubDomain(Request.Url);

            ApplicationCategorizationService.SubsubcategoryModel subsubcategory = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":categories";
                string hashField = categoryNameKey + "/" + subcategoryNameKey + "/" + subsubcategoryNameKey + ":private";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    subsubcategory = JsonConvert.DeserializeObject<ApplicationCategorizationService.SubsubcategoryModel>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (subsubcategory == null)
            {
                #region (Plan B) Get data from WCF

                var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();

                try
                {
                    applicationCategorizationServiceClient.Open();
                    subsubcategory = applicationCategorizationServiceClient.GetSubsubcategoryByNames(accountNameKey, categoryNameKey, subcategoryNameKey, subsubcategoryNameKey, true, Common.SharedClientKey);

                    WCFManager.CloseConnection(applicationCategorizationServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = subsubcategory;

            return jsonNetResult;

        }

        [Route("Categories/Json/GetSubsubsubcategory")]
        [HttpGet]
        public JsonNetResult GetSubsubsubcategory(string categoryNameKey, string subcategoryNameKey, string subsubcategoryNameKey, string subsubsubcategoryNameKey)
        {
            //var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();
            //var schemaId = "acc" + accountId.Replace("-", "");
            var accountNameKey = Common.GetSubDomain(Request.Url);

            ApplicationCategorizationService.SubsubsubcategoryModel subsubsubcategory = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = accountNameKey + ":categories";
                string hashField = categoryNameKey + "/" + subcategoryNameKey + "/" + subsubcategoryNameKey + "/" + subsubsubcategoryNameKey;

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    subsubsubcategory = JsonConvert.DeserializeObject<ApplicationCategorizationService.SubsubsubcategoryModel>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (subsubsubcategory == null)
            {
                #region (Plan B) Get data from WCF

                var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();

                try
                {
                    applicationCategorizationServiceClient.Open();
                    subsubsubcategory = applicationCategorizationServiceClient.GetSubsubsubcategoryByNames(accountNameKey, categoryNameKey, subcategoryNameKey, subsubcategoryNameKey, subsubsubcategoryNameKey, Common.SharedClientKey);

                    WCFManager.CloseConnection(applicationCategorizationServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = subsubsubcategory;

            return jsonNetResult;

        }

        #endregion

        #region Details


        [Route("Categories/Json/Details/{id}")]
        public JsonNetResult Detail(string id)
        {

            var results = new string[] { "details 1", "details 2" };

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = results;

            return jsonNetResult;
        }

        #endregion

        #region Create

        [Route("Categories/Json/CreateCategory")]
        [HttpPost]
        public JsonNetResult CreateCategory(string categoryName, bool isVisible)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.CreateCategory(accountId, categoryName, isVisible,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/CreateSubcategory")]
        [HttpPost]
        public JsonNetResult CreateSubcategory(string categoryId, string subcategoryName, bool isVisible)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.CreateSubcategory(accountId, categoryId, subcategoryName, isVisible,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/CreateSubsubcategory")]
        [HttpPost]
        public JsonNetResult CreateSubsubcategory(string subcategoryId, string subsubcategoryName, bool isVisible)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.CreateSubsubcategory(accountId, subcategoryId, subsubcategoryName, isVisible,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/CreateSubsubsubcategory")]
        [HttpPost]
        public JsonNetResult CreateSubsubsubcategory(string subsubcategoryId, string subsubsubcategoryName, bool isVisible)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.CreateSubsubsubcategory(accountId, subsubcategoryId, subsubsubcategoryName, isVisible,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #region Updates

        [Route("Categories/Json/UpdateCategoryName")]
        [HttpPost]
        public JsonNetResult UpdateCategoryName(string categoryId, string newName)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.RenameCategory(accountId, categoryId, newName,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/UpdateCategoryVisibleState")]
        [HttpPost]
        public JsonNetResult UpdateCategoryVisibleState(string categoryId, bool isVisible)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.UpdateCategoryVisibleState(accountId, categoryId, isVisible,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/UpdateCategoryDescription")]
        [HttpPost]
        public JsonNetResult UpdateCategoryDescription(string categoryId, string newDescription)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.UpdateCategoryDescription(accountId, categoryId, newDescription,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/ReorderCategories")]
        [HttpPost]
        public JsonNetResult ReorderCategories(List<String> categoryOrder)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var categoryOrderDictionary = new Dictionary<string, int>();

            foreach(String id in categoryOrder)
            {
                categoryOrderDictionary.Add(id, categoryOrderDictionary.Count);
            }

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.ReorderCategories(accountId, categoryOrderDictionary,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/ResetCategoryOrder")]
        [HttpPost]
        public JsonNetResult ResetCategoryOrder()
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.ResetCategoryOrdering(accountId,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/UpdateSubcategoryName")]
        [HttpPost]
        public JsonNetResult UpdateSubcategoryName(string subcategoryId, string newName)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.RenameSubcategory(accountId, subcategoryId, newName,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Categories/Json/UpdateSubcategoryVisibleState")]
        [HttpPost]
        public JsonNetResult UpdateSubcategoryVisibleState(string subcategoryId, bool isVisible)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.UpdateSubcategoryVisibleState(accountId, subcategoryId, isVisible,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/UpdateSubcategoryDescription")]
        [HttpPost]
        public JsonNetResult UpdateSubcategoryDescription(string subcategoryId, string newDescription)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.UpdateSubcategoryDescription(accountId, subcategoryId, newDescription,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/ReorderSubcategories")]
        [HttpPost]
        public JsonNetResult ReorderSubcategories(List<String> subcategoryOrder)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var subcategoryOrderDictionary = new Dictionary<string, int>();

            foreach (String id in subcategoryOrder)
            {
                subcategoryOrderDictionary.Add(id, subcategoryOrderDictionary.Count);
            }

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.ReorderSubcategories(accountId, subcategoryOrderDictionary,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/ResetSubcategoryOrder")]
        [HttpPost]
        public JsonNetResult ResetSubcategoryOrder(string categoryId)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.ResetSubcategoryOrdering(accountId, categoryId,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/UpdateSubsubcategoryName")]
        [HttpPost]
        public JsonNetResult UpdateSubsubcategoryName(string subsubcategoryId, string newName)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.RenameSubsubcategory(accountId, subsubcategoryId, newName,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }
                                
        [Route("Categories/Json/UpdateSubsubcategoryVisibleState")]
        [HttpPost]
        public JsonNetResult UpdateSubsubcategoryVisibleState(string subsubcategoryId, bool isVisible)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.UpdateSubsubcategoryVisibleState(accountId, subsubcategoryId, isVisible,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/UpdateSubsubcategoryDescription")]
        [HttpPost]
        public JsonNetResult UpdateSubsubcategoryDescription(string subsubcategoryId, string newDescription)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.UpdateSubsubcategoryDescription(accountId, subsubcategoryId, newDescription,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Categories/Json/ReorderSubsubcategories")]
        [HttpPost]
        public JsonNetResult ReorderSubsubcategories(List<String> subsubcategoryOrder)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var subsubcategoryOrderDictionary = new Dictionary<string, int>();

            foreach (String id in subsubcategoryOrder)
            {
                subsubcategoryOrderDictionary.Add(id, subsubcategoryOrderDictionary.Count);
            }


            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.ReorderSubsubcategories(accountId, subsubcategoryOrderDictionary,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/ResetSubsubcategoryOrder")]
        [HttpPost]
        public JsonNetResult ResetSubsubcategoryOrder(string subcategoryId)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.ResetSubsubcategoryOrdering(accountId, subcategoryId,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Categories/Json/UpdateSubsubsubcategoryName")]
        [HttpPost]
        public JsonNetResult UpdateSubsubsubcategoryName(string subsubsubcategoryId, string newName)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.RenameSubsubsubcategory(accountId, subsubsubcategoryId, newName,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/UpdateSubsubsubcategoryVisibleState")]
        [HttpPost]
        public JsonNetResult UpdateSubsubsubcategoryVisibleState(string subsubsubcategoryId, bool isVisible)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.UpdateSubsubsubcategoryVisibleState(accountId, subsubsubcategoryId, isVisible,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/UpdateSubsubsubcategoryDescription")]
        [HttpPost]
        public JsonNetResult UpdateSubsubsubcategoryDescription(string subsubsubcategoryId, string newDescription)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.UpdateSubsubsubcategoryDescription(accountId, subsubsubcategoryId, newDescription,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Categories/Json/ReorderSubsubsubcategories")]
        [HttpPost]
        public JsonNetResult ReorderSubsubsubcategories(List<string> subsubsubcategoryOrder)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();


            var subsubsubcategoryOrderDictionary = new Dictionary<string, int>();

            foreach (String id in subsubsubcategoryOrder)
            {
                subsubsubcategoryOrderDictionary.Add(id, subsubsubcategoryOrderDictionary.Count);
            }

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.ReorderSubsubsubcategories(accountId, subsubsubcategoryOrderDictionary,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/ResetSubsubsubcategoryOrder")]
        [HttpPost]
        public JsonNetResult ResetSubsubsubcategoryOrder(string subsubcategoryId)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.ResetSubsubsubcategoryOrdering(accountId, subsubcategoryId,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #region Delete

        [Route("Categories/Json/DeleteCategory")]
        [HttpPost]
        public JsonNetResult DeleteCategory(string categoryId)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.DeleteCategory(accountId, categoryId,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/DeleteSubcategory")]
        [HttpPost]
        public JsonNetResult DeleteSubcategory(string subcategoryId)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.DeleteSubcategory(accountId, subcategoryId,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("Categories/Json/DeleteSubsubcategory")]
        [HttpPost]
        public JsonNetResult DeleteSubsubcategory(string subsubcategoryId)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.DeleteSubsubcategory(accountId, subsubcategoryId,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Categories/Json/DeleteSubsubsubcategory")]
        [HttpPost]
        public JsonNetResult DeleteSubsubsubcategory(string subsubsubcategoryId)
        {
            var accountId = AuthenticationCookieManager.GetAuthenticationCookie().AccountID.ToString();

            var response = new ApplicationCategorizationService.DataAccessResponseType();

            var applicationCategorizationServiceClient = new ApplicationCategorizationService.ApplicationCategorizationServiceClient();
            try
            {
                applicationCategorizationServiceClient.Open();
                response = applicationCategorizationServiceClient.DeleteSubsubsubcategory(accountId, subsubsubcategoryId,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.ApplicationCategorizationService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationCategorizationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationCategorizationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }
        #endregion

        #endregion


    }
}