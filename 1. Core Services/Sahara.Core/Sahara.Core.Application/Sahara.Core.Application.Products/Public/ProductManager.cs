using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Spatial;
using Newtonsoft.Json;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Categorization.Models;
using Sahara.Core.Application.Categorization.Public;
using Sahara.Core.Application.DocumentModels.Product;
using Sahara.Core.Application.Images.Records;
using Sahara.Core.Application.Products.Internal;
using Sahara.Core.Application.Products.Models;
using Sahara.Core.Application.Products.TableEntities;
using Sahara.Core.Application.Properties.Models;
using Sahara.Core.Application.Search;
using Sahara.Core.Application.Search.Enums;
using Sahara.Core.Application.Search.Models.Product;
using Sahara.Core.Common.Methods;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Types;
using Sahara.Core.Common.Validation;
using Sahara.Core.Common.Validation.ResponseTypes;
using Sahara.Core.Imaging.Models;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Logging.PlatformLogs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Products.Public
{
    internal class NameList
    {
        public string FullyQualifiedName = string.Empty;
    }

    /// <summary>
    /// Each account has a set amount of products it can have (Determined by the account plan/tier they are on)
    /// Each product is represented by a document within the DocumentDB database for each account within the "ApplicationProducts" collection
    /// 
    /// </summary>
    public static class ProductManager
    {

        #region Create 

        public static DataAccessResponseType CreateProduct(Account account, string locationPath, string productName, bool isVisible)
        {
            #region Validate Product Name:

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(productName);
            if (!ojectNameValidationResponse.isValid)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = ojectNameValidationResponse.validationMessage,
                };
            }

            #endregion

            var result = new DataAccessResponseType();
            var productNameKey = Common.Methods.ObjectNames.ConvertToObjectNameKey(productName);
            var fullyQualifiedName = locationPath + "/" + productNameKey;

            #region Make sure important metadata is included

            if (String.IsNullOrEmpty(productName))
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Product must contain a title."
                };
            }

            if (String.IsNullOrEmpty(locationPath))
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Product must be placed within a categorization."
                };
            }

            #endregion

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);

            #region Ensure product does not already exist (Also select list for count purposes



            string productExistsAndCountSqlQuery = "SELECT p.FullyQualifiedName FROM Products p WHERE p.LocationPath ='" + locationPath + "'";
            var productExistsAndCountResults = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<NameList>(collectionUri.ToString(), productExistsAndCountSqlQuery);
            var productExistsAndCount = productExistsAndCountResults.AsEnumerable().ToList();

            if (productExistsAndCount.Any(p => p.FullyQualifiedName == fullyQualifiedName))
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "This product already exists within this catgorization."
                };
            }

            #endregion

            #region Ensure we are under the max allowed products per set (Moved to WCF Service Call)
            /*
            if (account.PaymentPlan.MaxProductsPerSet <= productExistsAndCount.Count)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "You have reached the maximum amount of products for this set (" + account.PaymentPlan.MaxProductsPerSet + "). Please upgrade your plan or contact support to increase your limits."
                };
            }
            */
            #endregion

            #region Get / Set OrderID based on ordering schema of existing products within the LocationPath

            int orderId = 0;

            string maxOrderByQuery = "SELECT Top 1 p.OrderID FROM p WHERE p.LocationPath = '" + locationPath + "' Order By p.OrderID Desc";

            dynamic maxOrderByResult = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<Document>(collectionUri.ToString(), maxOrderByQuery).AsEnumerable().FirstOrDefault();

            if(maxOrderByResult != null)
            {
                try
                {
                    int maxOrderByInt = (int)maxOrderByResult.OrderID;

                    if (maxOrderByInt > 0)
                    {
                        orderId = maxOrderByInt + 1;
                    }
                }
                catch
                {

                }
            }

            #endregion

            //Create Product Document Model
            ProductDocumentModel product = new ProductDocumentModel
            {
                //Assign an new product id
                Id = System.Guid.NewGuid().ToString(),

                //Assign the AccountID & DocumentType to the document:
                //AccountID = account.AccountID.ToString(),
                //AccountNameKey = account.AccountNameKey,
                DocumentType = "Product",

                Name = productName,
                NameKey = productNameKey,
                Visible = isVisible,

                OrderID = orderId,

                LocationPath = locationPath,
                FullyQualifiedName = fullyQualifiedName,

                DateCreated = DateTimeOffset.UtcNow

            };


            #region Get and assign the full categorization names

            var categoriesArray = locationPath.Split('/');

            switch (categoriesArray.Length)
            {
                case 4:

                    var subsubsubcategory = CategorizationManager.GetSubsubsubcategoryByFullyQualifiedName(account, categoriesArray[0], categoriesArray[1], categoriesArray[2], categoriesArray[3]);

                    product.SubsubsubcategoryName = subsubsubcategory.SubsubsubcategoryName;
                    product.SubsubcategoryName = subsubsubcategory.Subsubcategory.SubsubcategoryName;
                    product.SubcategoryName = subsubsubcategory.Subcategory.SubcategoryName;
                    product.CategoryName = subsubsubcategory.Category.CategoryName;

                    product.SubsubsubcategoryNameKey = categoriesArray[3];
                    product.SubsubcategoryNameKey = categoriesArray[2];
                    product.SubcategoryNameKey = categoriesArray[1];
                    product.CategoryNameKey = categoriesArray[0];


                    break;

                case 3:

                    var subsubcategory = CategorizationManager.GetSubsubcategoryByFullyQualifiedName(account, categoriesArray[0], categoriesArray[1], categoriesArray[2], false);

                    product.SubsubcategoryName = subsubcategory.SubsubcategoryName;
                    product.SubcategoryName = subsubcategory.Subcategory.SubcategoryName;
                    product.CategoryName = subsubcategory.Category.CategoryName;

                    product.SubsubcategoryNameKey = categoriesArray[2];
                    product.SubcategoryNameKey = categoriesArray[1];
                    product.CategoryNameKey = categoriesArray[0];

                    break;

                case 2:

                    var subcategory = CategorizationManager.GetSubcategoryByFullyQualifiedName(account, categoriesArray[0], categoriesArray[1], false);

                    product.SubcategoryName = subcategory.SubcategoryName;
                    product.CategoryName = subcategory.Category.CategoryName;

                    product.SubcategoryNameKey = categoriesArray[1];
                    product.CategoryNameKey = categoriesArray[0];

                    break;

                case 1:

                    var category = CategorizationManager.GetCategoryByName(account, categoriesArray[0], false);

                    product.CategoryName = category.CategoryName;

                    product.CategoryNameKey = categoriesArray[0];

                    break;
            }

            #endregion

            //Store the document and run the post trigger to increment document & applictionImage Counts
            //Create a RequestOptions w/ reference to the PostTrigger to run
            //Remoed since we no longer need to use triggers to maintain counts
            //string triggerId = "IncrementProductCount";
            //var requestOptions = new RequestOptions { PostTriggerInclude = new List<string> { triggerId } };
            Exception requestException = null;

            try
            {
                Document createDocumentResponse = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentAsync(collectionUri.ToString(), product).Result; //, requestOptions).Result;

                //Update Search Index
                #region Update Search Index (+Rollback if required)

                try
                {
                    ProductSearchManager.CreateProductDocumentInSearchIndex(account.SearchPartition, account.ProductSearchIndex, product);
                    result.isSuccess = true;
                }
                catch(Exception e)
                {
                    //Removed since we no longer need to use triggers to maintain counts
                    //string rollbackTriggerId = "DecrementProductCount";
                    //var rollbackRequestOptions = new RequestOptions { PostTriggerInclude = new List<string> { rollbackTriggerId } };

                    //ROLLBACK due to search issue

                    //Remove the product from DocDB and decriment the count:
                    var removeDocumentResponse = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.DeleteDocumentAsync(createDocumentResponse.SelfLink); //, rollbackRequestOptions).Result;

                    //Search issue, rollback
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating search index (product creation): Rollback was initiated after search index failure (see description). DocumentDB has been rolled back to remove the new product.",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                    );

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };


                }
                
                //var documentArray = new List<ProductDocumentModel>();
                //documentArray.Add(product);

                //ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.ProductSearchIndex, documentArray, ProductSearchIndexAction.Add);


                #endregion
            }
            #region Handle Exceptions

            catch (DocumentClientException de)
            {
                requestException = de.GetBaseException();
                result.isSuccess = false;
                result.ErrorMessage = requestException.Message;
            }
            catch (Exception e)
            {
                requestException = e;
                result.isSuccess = false;
                result.ErrorMessage = requestException.Message;
            }

            #region Log Exception

            if (requestException != null)
            {
                result.isSuccess = false;
                result.ErrorMessage = requestException.Message;
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    requestException,
                    "saving 'ProductDocumentModel' for '" + product.Id + "' to '" + account.DocumentPartition + "' collection for '" + account.AccountName + "'",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );

                #endregion
            }

            #endregion

            //Clear all associated caches
            if (result.isSuccess)
            {
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            //result.SuccessMessage = product.Id;
            result.SuccessMessage = product.FullyQualifiedName;
            result.ErrorMessages = new List<string>();
            result.ErrorMessages.Add(product.Id);

            return result;
        }

        #endregion

        #region Get

        public static int GetProductCount(Account account)
        {
            #region Legact DocDB "CollectionProperties" method (moved to using search)

            /* Move to search
            int count = 0;

            var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            client.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);

            //Get count from account properties document in collection
            var result = client.CreateDocumentQuery<dynamic>(collectionUri.ToString(), "SELECT p.ProductCount FROM CollectionProperties p WHERE p.id = 'CollectionProperties'");
            count = Convert.ToInt32(result.AsEnumerable().FirstOrDefault().ProductCount);
            return count;
            */

            #endregion

            int count = 0;

            //string searchFilter = "/any(s: s eq '')";

            var documentSearchResult = ProductSearchManager.SearchProducts(account.SearchPartition, account.ProductSearchIndex, "", null, "relevance", 0, 1);

            try
            {
                count = (int)documentSearchResult.Count;
            }
            catch
            {

            }
            

            return count;

        }

        public static int GetProductCount(Account account, string locationPath)
        {
            int count = 0;

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);

            string sqlQuery = "SELECT p.id from products p Where p.LocationPath = '" + locationPath + "'";

            var documentResults = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<Document>(collectionUri.ToString(), sqlQuery);

            //var accountCollection = client.Crea

            //applicationImages = result.ToList();
            var documents = documentResults.AsEnumerable().ToList();

            count = documents.Count();

            return count;

        }

        public static ProductDocumentModel GetProduct(Account account, string fullyQualifiedName)
        {
            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);

            string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "'";

            var productResults = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery, new FeedOptions { MaxItemCount = 1 });
            
            //var accountCollection = client.Crea

            //applicationImages = result.ToList();
            var product = productResults.AsEnumerable().FirstOrDefault();

            return product;

        }

        public static ProductResults GetProducts(Account account, int page = 0, int resultsPerPage = 20, string tagFilter = null, string propertyFilter = null) //<-- Pagination is awaiting future SDK
        {
            return new ProductResults();
            /*      
            var productResults = new ProductResults();
            productResults.page = page;
            productResults.resultsPerPage = resultsPerPage;

            //var account = AccountManager.GetAccount(accountId);

            var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            client.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);

            //Set up SQL for the call
            var sqlQuerySpec = new SqlQuerySpec
            {
                QueryText = "SELECT * FROM ApplicationImages a WHERE a.AccountID = '" + account.AccountID.ToString() + "' AND a.DocumentType ='ApplicationImage' AND a.CategoryID != null"
            };

            if (!String.IsNullOrEmpty(tagFilter))
            {
                sqlQuerySpec.QueryText += " AND ARRAY_CONTAINS(a.Tags, '" + tagFilter + "')";
            }

            if (!String.IsNullOrEmpty(propertyFilter))
            {
                sqlQuerySpec.QueryText += " AND ARRAY_CONTAINS(a.Properties, '" + propertyFilter + "')";
            }

            //Set up feed options for the call
            /*
            var feedOptions = new FeedOptions
            {
                MaxItemCount = maxResults,
                RequestContinuation = continuationToken
            };* /

            //Read in all Application Image Documents for the account
            var result = client.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuerySpec.ToString()); //, feedOptions);
            productResults.Products = result.ToList();
            //applicationImageResults.ContinuationToken = continuationToken;

            return productResults;
            */

        }

        public static ProductResults GetProducts(Account account, string locationPath, PropertyFilter propertyFilter, TagFilter tagFilter, int page = 0, int resultsPerPage = 20)
        {
            return new ProductResults();
        }

        #endregion

        #region Updates

        public static DataAccessResponseType UpdateProductVisibleState(Account account, string fullyQualifiedName, bool isVisible)
        {
            var response = new DataAccessResponseType();

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            /**/

            string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "' AND p.DocumentType = 'Product'";
                    
            //Run stored procedure to update documet visible state

            try
            {

                var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery, new FeedOptions { MaxItemCount = 1 }).AsEnumerable().FirstOrDefault();
                string rollbackCopy = null; //<-- copy of document before changes are made in case a rollback is required

                if (document != null)
                {
                    rollbackCopy = JsonConvert.SerializeObject(document); //<-- Used for Rollbacks; //<-- copy of document before changes are made in case a rollback is required

                    document.Visible = isVisible;

                    //Replace document:
                    var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;

                    response.isSuccess = true;

                    //Update Search Index
                    #region Update Search Index

                    var documentArray = new List<ProductDocumentModel>();
                    documentArray.Add(document);

                    try
                    {
                        ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, documentArray, ProductSearchIndexAction.Update);
                    }
                    catch(Exception e)
                    {
                        //ROLLBACK DOCUMENT(S)

                        var deserializedRollbackCopy = JsonConvert.DeserializeObject<ProductDocumentModel>(rollbackCopy);

                        //Replace document:
                        var rolledBack = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(deserializedRollbackCopy.SelfLink, deserializedRollbackCopy).Result;

                        //Search issue, rollback
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "updating search index (product visibility): Rollback was initiated after search index failure (see description). Visible state of product '" + deserializedRollbackCopy.FullyQualifiedName + " has been rolled back to org value",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        account.AccountID.ToString(),
                        account.AccountName
                        );

                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                    }
                    

                    #endregion
                }

            }

            #region Manage Exceptions For UpdateProduct Stored Procedure

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "updating product visiblity",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating product visiblity",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion




            //Clear all associated caches
            if (response.isSuccess)
            {
                //response.SuccessMessage = product.Name;
                response.isSuccess = true;
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;

        }

        public static DataAccessResponseType RenameProduct(Account account, string fullyQualifiedName, string newName)
        {

            #region Validate

            ValidationResponseType ojectNameValidationResponse = ValidationManager.IsValidObjectName(newName);
            if (!ojectNameValidationResponse.isValid)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = ojectNameValidationResponse.validationMessage,
                };
            }

            #endregion




            #region Get the product

            var response = new DataAccessResponseType();

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "' AND p.DocumentType = 'Product'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery, new FeedOptions { MaxItemCount = 1 }).AsEnumerable().FirstOrDefault();
            string rollbackCopy = null; //<-- copy of document before changes are made in case a rollback is required

            if (document == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Could not retrieve document to be updated."
                };
            }
            else
            {
                rollbackCopy = JsonConvert.SerializeObject(document); //<-- Used for Rollbacks //<-- copy of document before changes are made in case a rollback is required
            }

            var locationPath = document.LocationPath;
            var newProductNameKey = Common.Methods.ObjectNames.ConvertToObjectNameKey(newName);
            var newFullyQualifiedName = locationPath + "/" + newProductNameKey;

            #endregion


            #region Ensure product with new name does not already exist in this locationPath

            var productExists = GetProduct(account, newFullyQualifiedName);

            if (productExists != null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "This product name already exists within this catgorization."
                };
            }

            #endregion

            try
            {

                document.Name = newName;
                document.NameKey = newProductNameKey;
                document.FullyQualifiedName = newFullyQualifiedName;

                //Replace document:
                var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;

                response.isSuccess = true;

                //Update Search Index
                #region Update Search Index

                var documentArray = new List<ProductDocumentModel>();
                documentArray.Add(document);

                try
                {
                    ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, documentArray, ProductSearchIndexAction.Update);
                }
                catch(Exception e)
                {
                    //ROLLBACK DOCUMENT(S)

                    var deserializedRollbackCopy = JsonConvert.DeserializeObject<ProductDocumentModel>(rollbackCopy);

                    //Replace document:
                    var rolledBack = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(deserializedRollbackCopy.SelfLink, deserializedRollbackCopy).Result;             

                    //Search issue, rollback
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating search index (product rename): Rollback was initiated after search index failure (see description). Renaming of product '" + deserializedRollbackCopy.FullyQualifiedName + " to '" + newName + "' has been rolled back to org value",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                    );

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                }
                

                #endregion

            }

            #region Manage Exceptions For 'UpdateProduct' Stored Procedure

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "attempting to rename product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to rename product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion


            //Clear all associated caches
            if (response.isSuccess)
            {
                //Add the new ProductNameKey to the results object
                response.Results = new List<string>();
                response.Results.Add(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(newName));

                //response.SuccessMessage = product.Name;
                response.isSuccess = true;
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType ReorderProducts(Account account, Dictionary<string, int> productOrderingDictionary, string locationPath)
        {
            var response = new DataAccessResponseType();

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            /**/

            string sqlQuery = "SELECT * FROM Products p WHERE p.LocationPath = '" + locationPath + "' AND p.DocumentType = 'Product'";

            //Run stored procedure to update documet visible state

            try
            {
                var products = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery).AsEnumerable().ToList();


                if (products != null)
                {
                    string rollbackCopy = JsonConvert.SerializeObject(products); //<-- Used for Rollbacks //<-- copy of document before changes are made in case a rollback is required

                    var documentSearchIndexUpdateArray = new List<ProductDocumentModel>();

                    foreach (ProductDocumentModel product in products)
                    {
                        //Update product:
                        product.OrderID = productOrderingDictionary[product.Id.ToString()];
                        //Replace document:
                        var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(product.SelfLink, product).Result;

                        //Add to Search Index Update Array
                        documentSearchIndexUpdateArray.Add(product);
                    }

                    response.isSuccess = true;

                    try
                    {
                        //Update Search Index
                        ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, documentSearchIndexUpdateArray, ProductSearchIndexAction.Update);
                    }
                    catch(Exception e)
                    {
                        //ROLLBACK DOCUMENT(S)

                        var deserializedRollbackCopy = JsonConvert.DeserializeObject<List<ProductDocumentModel>>(rollbackCopy);

                        foreach (ProductDocumentModel rolledBackProduct in deserializedRollbackCopy)
                        {
                            //Replace document:
                            var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(rolledBackProduct.SelfLink, rolledBackProduct).Result;
                        }

                        //Search issue, rollback
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "updating search index (custom reordering of products): Rollback was initiated after search index failure (see description). Reordering of products in locationPath: '" + locationPath + " on DocumentDB have been rolled back to their org value",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        account.AccountID.ToString(),
                        account.AccountName
                        );

                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                    }

                }
            }

            #region Manage Exceptions For UpdateProduct Stored Procedure

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "resetting product ordering",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "resetting product ordering",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion


            //Clear all associated caches
            if (response.isSuccess)
            {
                //response.SuccessMessage = product.Name;
                response.isSuccess = true;
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;

        }

        public static DataAccessResponseType ResetProductOrdering(Account account, string locationPath)
        {
            var response = new DataAccessResponseType();

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            /**/

            string sqlQuery = "SELECT * FROM Products p WHERE p.LocationPath = '" + locationPath + "' AND p.DocumentType = 'Product'";

            //Run stored procedure to update documet visible state

            try
            {
                var products = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery).AsEnumerable().ToList();

                if (products != null)
                {
                    string rollbackCopy = JsonConvert.SerializeObject(products); //<-- Used for Rollbacks //<-- copy of document before changes are made in case a rollback is required

                    var documentSearchIndexUpdateArray = new List<ProductDocumentModel>();

                    foreach (ProductDocumentModel product in products)
                    {
                        //Update product:
                        product.OrderID = 0;
                        //Replace document:
                        var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(product.SelfLink, product).Result;

                        documentSearchIndexUpdateArray.Add(product);
                    }

                    response.isSuccess = true;

                    try
                    {
                        //Update Search Index
                        ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, documentSearchIndexUpdateArray, ProductSearchIndexAction.Update);
                    }
                    catch(Exception e)
                    {
                        //ROLLBACK DOCUMENT(S)
                        var deserializedRollbackCopy = JsonConvert.DeserializeObject<List<ProductDocumentModel>>(rollbackCopy);

                        foreach (ProductDocumentModel rolledBackProduct in deserializedRollbackCopy)
                        {
                            //Replace document:
                            var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(rolledBackProduct.SelfLink, rolledBackProduct).Result;
                        }

                        //Search issue, rollback
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "updating search index (custom product ordering reset): Rollback was initiated after search index failure (see description). Reset of product ordering in locationPath: '" + locationPath + " on DocumentDB have been rolled back to their org value",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        account.AccountID.ToString(),
                        account.AccountName
                        );

                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                    }

                }
            }

            #region Manage Exceptions For UpdateProduct Stored Procedure

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "resetting product ordering",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "resetting product ordering",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion


            //Clear all associated caches
            if (response.isSuccess)
            {
                //response.SuccessMessage = product.Name;
                response.isSuccess = true;
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;

        }

        #endregion

        #region Move

        public static DataAccessResponseType MoveProduct(Account account, string productId, string newLocationPath)
        {
            #region Get the product

            var response = new DataAccessResponseType();

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            string sqlQuery = "SELECT * FROM Products p WHERE p.id ='" + productId + "'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery).AsEnumerable().FirstOrDefault();
            string rollbackCopy = null; //<-- copy of document before changes are made in case a rollback is required

            if (document == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Could not retrieve product to be moved."
                };
            }
            else
            {
                rollbackCopy = JsonConvert.SerializeObject(document); //<-- Used for Rollbacks
            }

            var newFullyQualifiedName = newLocationPath + "/" + document.NameKey;

            #endregion


            #region Ensure product with new name does not already exist in this locationPath

            var productExists = GetProduct(account, newFullyQualifiedName);

            if (productExists != null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "This product name already exists within this catgorization."
                };
            }

            #endregion

            try
            {

                document.LocationPath = newLocationPath;
                document.FullyQualifiedName = newFullyQualifiedName;

                #region Get and assign the NEW full categorization names

                var categoriesArray = newLocationPath.Split('/');

                switch (categoriesArray.Length)
                {
                    case 4:

                        var subsubsubcategory = CategorizationManager.GetSubsubsubcategoryByFullyQualifiedName(account, categoriesArray[0], categoriesArray[1], categoriesArray[2], categoriesArray[3]);

                        document.SubsubsubcategoryName = subsubsubcategory.SubsubsubcategoryName;
                        document.SubsubcategoryName = subsubsubcategory.Subsubcategory.SubsubcategoryName;
                        document.SubcategoryName = subsubsubcategory.Subcategory.SubcategoryName;
                        document.CategoryName = subsubsubcategory.Category.CategoryName;

                        document.SubsubsubcategoryNameKey = categoriesArray[3];
                        document.SubsubcategoryNameKey = categoriesArray[2];
                        document.SubcategoryNameKey = categoriesArray[1];
                        document.CategoryNameKey = categoriesArray[0];


                        break;

                    case 3:

                        var subsubcategory = CategorizationManager.GetSubsubcategoryByFullyQualifiedName(account, categoriesArray[0], categoriesArray[1], categoriesArray[2], false);

                        document.SubsubcategoryName = subsubcategory.SubsubcategoryName;
                        document.SubcategoryName = subsubcategory.Subcategory.SubcategoryName;
                        document.CategoryName = subsubcategory.Category.CategoryName;

                        document.SubsubcategoryNameKey = categoriesArray[2];
                        document.SubcategoryNameKey = categoriesArray[1];
                        document.CategoryNameKey = categoriesArray[0];

                        break;

                    case 2:

                        var subcategory = CategorizationManager.GetSubcategoryByFullyQualifiedName(account, categoriesArray[0], categoriesArray[1], false);

                        document.SubcategoryName = subcategory.SubcategoryName;
                        document.CategoryName = subcategory.Category.CategoryName;

                        document.SubcategoryNameKey = categoriesArray[1];
                        document.CategoryNameKey = categoriesArray[0];

                        break;

                    case 1:

                        var category = CategorizationManager.GetCategoryByName(account, categoriesArray[0], false);

                        document.CategoryName = category.CategoryName;

                        document.CategoryNameKey = categoriesArray[0];

                        break;
                }

                #endregion

                //Replace document:
                var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;
                response.isSuccess = true;

                //Update Search Index
                #region Update Search Index

                var documentArray = new List<ProductDocumentModel>();
                documentArray.Add(document);

                try
                {
                    ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, documentArray, ProductSearchIndexAction.Update);
                }
                catch(Exception e)
                {
                    //ROLLBACK DOCUMENT

                    var deserializedRollbackCopy = JsonConvert.DeserializeObject<ProductDocumentModel>(rollbackCopy);

                    var rolledback = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(deserializedRollbackCopy.SelfLink, deserializedRollbackCopy).Result;

                    //Search issue, rollback
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating search index (product move): Rollback was initiated after search index failure (see description). Moving of product '" + deserializedRollbackCopy.FullyQualifiedName + "' has been rolled back to org value",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                    );

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                }

                #endregion

            }

            #region Manage Exceptions For 'UpdateProduct' Stored Procedure

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "attempting to move product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to move product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion


            //Clear all associated caches
            if (response.isSuccess)
            {
                //Add the Product Name to the results object
                response.SuccessMessage = document.Name;

                //response.SuccessMessage = product.Name;
                response.isSuccess = true;
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;
        }

        #endregion

        #region DELETE

        public static DataAccessResponseType DeleteProduct(Account account, string productId)
        {
            #region Get the product

            var result = new DataAccessResponseType();

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            string sqlQuery = "SELECT * FROM Products p WHERE p.id ='" + productId + "'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery).AsEnumerable().FirstOrDefault();

            if (document == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Could not retrieve document to be deleted."
                };
            }
            else
            {

            }

            #endregion

            //Delete the document and run the post trigger to decement document Count
            //Create a RequestOptions w/ reference to the PostTrigger to run

            //Removed as we now use search to get this figure and we also only base product limits on a per categorization basis
            //string triggerId = "DecrementProductCount";
            //var requestOptions = new RequestOptions { PostTriggerInclude = new List<string> { triggerId } };
            Exception requestException = null;

            try
            {
                var deleteDocumentResponse = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.DeleteDocumentAsync(document.SelfLink).Result; //, requestOptions).Result;
                

                //Update Search Index
                #region Update Search Index

                var documentArray = new List<ProductDocumentModel>();
                documentArray.Add(document);

                try
                {
                    ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, documentArray, ProductSearchIndexAction.Delete);
                    result.isSuccess = true;
                }
                catch(Exception e)
                {
                    //Removed since we no longer need to use triggers to maintain countsL
                    //string rollbackTriggerId = "IncrementProductCount";
                    //var rollbackRequestOptions = new RequestOptions { PostTriggerInclude = new List<string> { rollbackTriggerId } };

                    //ROLLBACK due to search issue

                    //Build a collection Uri out of the known IDs
                    //(These helpers allow you to properly generate the following URI format for Document DB:
                    //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"

                    //Put the product back into DocDB and incremebt the count:
                    var replaceDocumentResponse = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentAsync(collectionUri.ToString(), document).Result; //, rollbackRequestOptions).Result;

                    //Search issue, rollback
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating search index (product deletion): Rollback was initiated after search index failure (see description). Attempting to delete product '" + document.FullyQualifiedName + "'. DocumentDB has been rolled back to replace the product.",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                    );

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };

                }


                #endregion
            }

            #region Handle Exceptions

            catch (DocumentClientException de)
            {
                requestException = de.GetBaseException();
                result.isSuccess = false;
                result.ErrorMessage = requestException.Message;
            }
            catch (Exception e)
            {
                requestException = e;
                result.isSuccess = false;
                result.ErrorMessage = requestException.Message;
            }


            #region Log Exception

            if (requestException != null)
            {
                result.isSuccess = false;
                result.ErrorMessage = requestException.Message;
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    requestException,
                    "deleting 'ProductDocumentModel' for '" + productId + "' from '" + account.DocumentPartition + "' collection for '" + account.AccountName + "'",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
                #endregion

            #endregion

            //Clear all associated images & caches and return the product name
            if (result.isSuccess)
            {
                ImageRecordsManager.DeleteAllImageRecordsForObject(account, "product", productId);

                //Add the Product Name to the Success object
                result.SuccessMessage = document.Name;
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return result;
        }

        #endregion

        #region Manage Properties

        public static DataAccessResponseType UpdateProductProperty(Account account, string fullyQualifiedName, PropertyModel property, string propertyValue, ProductPropertyUpdateType updateType)
        {
            var response = new DataAccessResponseType();

            var propertySearchFieldType = ProductPropertySearchFieldType.String;


            #region Verify Property Value

            //Used later if this is a swatch
            PropertySwatchModel swatchModel = null;

            if(string.IsNullOrEmpty(propertyValue))
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Property must contain a value"
                };
            }

            if(property.PropertyTypeNameKey == "predefined")
            {
                propertySearchFieldType = ProductPropertySearchFieldType.Collection; //<--Must be converted into an Array when sent into search index for updates

                bool isAllowedValue = false;

                foreach(PropertyValueModel value in property.Values)
                {
                    if(value.PropertyValueName == propertyValue)
                    {
                        isAllowedValue = true;
                    }
                }

                if(!isAllowedValue)
                {
                    return new DataAccessResponseType
                    {
                        isSuccess = false,
                        ErrorMessage = "This is not one of the predefined values allowed for this property"
                    };
                }
            }
            if (property.PropertyTypeNameKey == "swatch")
            {
                var cdnEndpoint = Core.Settings.Azure.Storage.GetStoragePartition(account.StoragePartition).CDN;
                var cdn = "https://" + cdnEndpoint + "/";

                propertySearchFieldType = ProductPropertySearchFieldType.Collection; //<--Must be converted into an Array when sent into search index for updates

                bool isAllowedValue = false;

                foreach (PropertySwatchModel swatch in property.Swatches)
                {
                    //Append CDN url to front of
                    swatch.PropertySwatchImage = cdn + swatch.PropertySwatchImage;
                    swatch.PropertySwatchImageMedium = cdn + swatch.PropertySwatchImageMedium;
                    swatch.PropertySwatchImageSmall = cdn + swatch.PropertySwatchImageSmall;

                    if (swatch.PropertySwatchLabel == propertyValue)
                    {
                        swatchModel = swatch;
                        isAllowedValue = true;
                    }
                }

                if (!isAllowedValue)
                {
                    return new DataAccessResponseType
                    {
                        isSuccess = false,
                        ErrorMessage = "This is not one of the swatches allowed for this property"
                    };
                }
            }
            else if(property.PropertyTypeNameKey == "number")
            {
                //int n;
                //decimal d;
                double db;
                //bool isNumeric = int.TryParse(propertyValue, out n);
                //bool isDecimal = decimal.TryParse(propertyValue, out d);
                bool isDouble = double.TryParse(propertyValue, out db);

                if (!isDouble) //!isNumeric && !isDecimal)
                {
                    return new DataAccessResponseType
                    {
                        isSuccess = false,
                        ErrorMessage = "Value must be a valid number"
                    };
                }

            }
            else if (property.PropertyTypeNameKey == "string")
            {
                if(propertyValue.ToStringOrEmpty().Length > 80)
                {
                    return new DataAccessResponseType
                    {
                        isSuccess = false,
                        ErrorMessage = "Strings cannot be longer than 80 characters"
                    };
                }

            }
            else if (property.PropertyTypeNameKey == "paragraph")
            {
                if (propertyValue.ToStringOrEmpty().Length > 480)
                {
                    return new DataAccessResponseType
                    {
                        isSuccess = false,
                        ErrorMessage = "Paragraphs cannot be longer than 480 characters"
                    };
                }

            }
            else if (property.PropertyTypeNameKey == "datetime")
            {
                //Attempt to convert to a valid date/time string
                try
                {
                    //var dateTimeString = Convert.ToDateTime(propertyValue).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); //<-- OData V4 Format //.ToString("O");

                    //Keeping it to time submitted so it is always localized to the user.
                    var dateTimeString = Convert.ToDateTime(propertyValue).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    propertyValue = dateTimeString;
                }
                catch
                {
                    return new DataAccessResponseType
                    {
                        isSuccess = false,
                        ErrorMessage = "Not a valid date/time value"
                    };
                }                

            }
            #endregion

            #region Verify Update Type

            if(updateType == ProductPropertyUpdateType.Append)
            {
                if(property.Appendable == false)
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot append to an unappendable property" };
                }
            }

            #endregion

            #region Get the document

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "' AND p.DocumentType = 'Product'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery, new FeedOptions { MaxItemCount = 1 }).AsEnumerable().FirstOrDefault();

            if (document == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Could not retrieve document to be updated."
                };
            }
            else
            {

            }

            #endregion

            #region Update property

            //For Rollbacks ----------------------------------
            string rollbackType = null; //<-- If rollback occurs how to handle
            //Dictionary<string, string> previousPropertiesValue = null;
            //Dictionary<string, List<string>> previousPredefinedValue = null;
            //Dictionary<string, List<Swatch>> previousSwatchesValue = null;
            string previousValue = null;

            //ANY UPDATES BELOEW MUST ALSO BE REFLECTED IN THE ROLLBACK! -------
            switch (property.PropertyTypeNameKey)
            {
                case "swatch":

                    #region Swatch Property Type

                    //Create swatch object 
                    var swatch = new Swatch
                    {
                        Label = swatchModel.PropertySwatchLabel,
                        Image = swatchModel.PropertySwatchImage,
                        ImageMedium = swatchModel.PropertySwatchImageMedium,
                        ImageSmall = swatchModel.PropertySwatchImageSmall
                    };

                    if (document.Swatches != null)
                    {
                        //Find swatch to append to (if exists)
                        if (document.Swatches.ContainsKey(property.PropertyName))
                        {

                            //Makse sure this swatch isn't already on this property
                            foreach(var existingSwatch in document.Swatches)
                            {
                                foreach(var value in existingSwatch.Value)
                                {
                                    if(swatch.Label == value.Label)
                                    {
                                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This swatch already exists on this product." };
                                    }
                                }

                            }                              
                            
                            if(updateType == ProductPropertyUpdateType.Append)
                            {
                                rollbackType = "revert"; //<-- Used for Rollbacks
                                previousValue = JsonConvert.SerializeObject(document.Swatches); //<-- Used for Rollbacks

                                //Append Swatch to Swatch Property
                                document.Swatches[property.PropertyName].Add(swatch);
                            }
                            else if (updateType == ProductPropertyUpdateType.Replace)
                            {
                                rollbackType = "revert"; //<-- Used for Rollbacks
                                previousValue = JsonConvert.SerializeObject(document.Swatches); //<-- Used for Rollbacks

                                //Replace Swatch Property
                                document.Swatches[property.PropertyName] = new List<Swatch>();
                                document.Swatches[property.PropertyName].Add(swatch);
                            }
                                                       
                        }
                        else
                        {
                            rollbackType = "revert"; //<-- Used for Rollbacks
                            previousValue = JsonConvert.SerializeObject(document.Swatches); //<-- Used for Rollbacks

                            //Does not exist (add property and first listed item)
                            var swatches = new List<Swatch>();
                            swatches.Add(swatch);
                            document.Swatches.Add(property.PropertyName, swatches);
                        }
                    }
                    else
                    {
                        rollbackType = "nullify"; //<-- Used for Rollbacks

                        //This is the first swatch added to the document. Create the dictionary object and add the new swatch
                        document.Swatches = new Dictionary<string, List<Swatch>>();
                        var swatches = new List<Swatch>();
                        swatches.Add(swatch);
                        document.Swatches.Add(property.PropertyName, swatches);
                    }

                    #endregion

                    break;

                case "predefined":

                    #region Predefined Property Type

                    if (document.Predefined != null)
                    {
                        //Find poperty to update (if exists)
                        if (document.Predefined.ContainsKey(property.PropertyName))
                        {
                            //Makse sure this value isn't already on this property
                            foreach (var existingPredefined in document.Predefined)
                            {
                                foreach (var value in existingPredefined.Value)
                                {
                                    if (propertyValue == value)
                                    {
                                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This value already exists on this product." };
                                    }
                                }

                            }

                            if (updateType == ProductPropertyUpdateType.Append)
                            {
                                rollbackType = "revert"; //<-- Used for Rollbacks
                                previousValue = JsonConvert.SerializeObject(document.Predefined); //<-- Used for Rollbacks

                                //Append value to predefined Property
                                document.Predefined[property.PropertyName].Add(propertyValue);
                            }
                            else if (updateType == ProductPropertyUpdateType.Replace)
                            {
                                rollbackType = "revert"; //<-- Used for Rollbacks
                                previousValue = JsonConvert.SerializeObject(document.Predefined); //<-- Used for Rollbacks

                                //Replace Swatch Property
                                document.Predefined[property.PropertyName] = new List<string>();
                                document.Predefined[property.PropertyName].Add(propertyValue);
                            }

                        }
                        else
                        {
                            rollbackType = "revert"; //<-- Used for Rollbacks
                            previousValue = JsonConvert.SerializeObject(document.Predefined); //<-- Used for Rollbacks

                            //Does not exist (add property and first listed item)
                            var predefinedValueList = new List<string>();
                            predefinedValueList.Add(propertyValue);
                            document.Predefined.Add(property.PropertyName, predefinedValueList);
                        }
                    }
                    else
                    {
                        rollbackType = "nullify"; //<-- Used for Rollbacks

                        //This is the first property added to the document. Create the dictionary object and add the new appendable property
                        document.Predefined = new Dictionary<string, List<string>>();
                        var predefinedValueList = new List<string>();
                        predefinedValueList.Add(propertyValue);
                        document.Predefined.Add(property.PropertyName, predefinedValueList);
                    }

                    #endregion

                    break;

                default:

                    #region Basic Property Type

                    if (document.Properties != null)
                    {
                        //Find property to update (if exists)
                        if (document.Properties.ContainsKey(property.PropertyName))
                        {
                            rollbackType = "revert"; //<-- Used for Rollbacks
                            previousValue = JsonConvert.SerializeObject(document.Properties); //<-- Used for Rollbacks

                            document.Properties[property.PropertyName] = propertyValue;
                        }
                        else
                        {
                            rollbackType = "revert"; //<-- Used for Rollbacks
                            previousValue = JsonConvert.SerializeObject(document.Properties); //<-- Used for Rollbacks

                            //Does not exist (add property)
                            document.Properties.Add(property.PropertyName, propertyValue);
                        }
                    }
                    else
                    {
                        rollbackType = "nullify"; //<-- Used for Rollbacks

                        //This is the first property added to the document. Create the dictionary object and add the new proprty
                        document.Properties = new Dictionary<string, string>();
                        document.Properties.Add(property.PropertyName, propertyValue);
                    }

                    #endregion

                    break;
            }




            #endregion

            //Update the Indexed version of the properties (Only done on search index)
            //document.IndexedProperties = GenerateIndexedProperties(document.Properties);

            try
            {
                //Replace document:
                var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;
                response.isSuccess = true;

                //Update Search Index
                #region Update Search Index (+Rollback on search index errors)

                try
                {
                    ProductSearchManager.UpdateProductPropertyInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, document.Id, property.SearchFieldName, propertyValue, updateType, propertySearchFieldType);
                }
                catch(Exception e)
                {
                    
                    #region ROLLBACK Document (Must be updated ABOVE as well!!!!)

                    switch (property.PropertyTypeNameKey)
                    {
                        case "swatch":

                            switch(rollbackType)
                            {
                                case "revert":
                                    document.Swatches = JsonConvert.DeserializeObject<Dictionary<string, List<Swatch>>>(previousValue);
                                    break;
                                case "nullify":
                                    document.Swatches = null;
                                    break;
                            }

                            break;

                        case "predefined":

                            switch (rollbackType)
                            {
                                case "revert":
                                    document.Predefined = JsonConvert.DeserializeObject<Dictionary<string, List <string>>>(previousValue);
                                    break;
                                case "nullify":
                                    document.Predefined = null;
                                    break;
                            }

                            break;

                        default:

                            switch (rollbackType)
                            {
                                case "revert":
                                    document.Properties = JsonConvert.DeserializeObject<Dictionary<string, string>>(previousValue);
                                    break;
                                case "nullify":
                                    document.Properties = null;
                                    break;
                            }

                            break;
                    }


                    //ROLLBACK DOCUMENT
                    var rolledback = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;

                    #endregion

                    //Search issue, rollback
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating search index (product property update of type '"+ updateType + "'): Rollback was initiated after search index failure (see description). Updating (or appending) property '" + property.PropertyNameKey + "' to (or with) '" + propertyValue + "' for product '" + document.FullyQualifiedName + "' has been rolled back to org value(s) or removed/nullified",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                    );

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                }

                //var documentArray = new List<ProductDocumentModel>();
                //documentArray.Add(document);
                //ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.ProductSearchIndex, documentArray, ProductSearchIndexAction.Update);

                #endregion

            }
            #region Manage Exceptions

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "attempting to add a property to a product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to add a property to a product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion



            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType UpdateProductLocationProperty(Account account, string fullyQualifiedName, PropertyModel property, PropertyLocationValue propertyLocationValue)
        {
            var response = new DataAccessResponseType();

            var propertySearchFieldType = ProductPropertySearchFieldType.Location;


            #region Verify Property Values

            if (propertyLocationValue == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Property location must contain a value"
                };
            }


            if (propertyLocationValue.Name.ToStringOrEmpty().Contains(" || ") || propertyLocationValue.Address1.ToStringOrEmpty().Contains(" || ") || propertyLocationValue.Address2.ToStringOrEmpty().Contains(" || ") || propertyLocationValue.City.ToStringOrEmpty().Contains(" || ") || propertyLocationValue.State.ToStringOrEmpty().Contains(" || ") || propertyLocationValue.PostalCode.ToStringOrEmpty().Contains(" || ") || propertyLocationValue.Country.ToStringOrEmpty().Contains(" || "))
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Location metadata cannot contain ' || '"
                };
            }


            if (propertyLocationValue.Name.ToStringOrEmpty().Length > 55)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Name cannot be more than 55 characters"
                };
            }

            if (propertyLocationValue.Address1.ToStringOrEmpty().Length > 50)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Address1 cannot be more than 50 characters"
                };
            }
            if (propertyLocationValue.Address2.ToStringOrEmpty().Length > 50)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Address2 cannot be more than 50 characters"
                };
            }
            if (propertyLocationValue.City.ToStringOrEmpty().Length > 50)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "City cannot be more than 50 characters"
                };
            }
            if (propertyLocationValue.State.ToStringOrEmpty().Length > 50)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "State cannot be more than 50 characters"
                };
            }
            if (propertyLocationValue.PostalCode.ToStringOrEmpty().Length > 40)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "PostalCode cannot be more than 40 characters"
                };
            }
            if (propertyLocationValue.Country.ToStringOrEmpty().Length > 40)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Country cannot be more than 40 characters"
                };
            }
            #endregion


            #region Attempt to create GeographyPointValue for Search Index

            // Create GeographyPoint value
            GeographyPoint geographyPointValue;

            try
            {
                geographyPointValue = GeographyPoint.Create(Convert.ToDouble(propertyLocationValue.Lat), Convert.ToDouble(propertyLocationValue.Long));
                //new LocalGeometry { coordinates = new List<double> { Lat, Long }, type = "Point" }
            }
            catch
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "An exception occurred while attempting to create a geography point from your location data. Please check that your lat/longs are valid doubles." };
            }

            #endregion



            #region Get the document

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "' AND p.DocumentType = 'Product'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery, new FeedOptions { MaxItemCount = 1 }).AsEnumerable().FirstOrDefault();

            if (document == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Could not retrieve document to be updated."
                };
            }
            else
            {

            }

            #endregion

            #region Update property

            //For Rollbacks ----------------------------------
            string rollbackType = null; //<-- If rollback occurs how to handle
            string previousLocationValue = null;

            if (document.Locations != null)
            {
                //Find swatch to append to (if exists)
                if (document.Locations.ContainsKey(property.PropertyName))
                {
                    rollbackType = "revert"; //<-- Used for Rollbacks
                    previousLocationValue = JsonConvert.SerializeObject(document.Locations[property.PropertyName]); //<-- Used for Rollbacks;

                    //Replace Location Property
                    document.Locations[property.PropertyName] = propertyLocationValue;
                }
                else
                {
                    rollbackType = "remove"; //<-- Used for Rollbacks


                    //Does not exist (Add the propertyNameKey and LocationValue)
                    document.Locations.Add(property.PropertyName, propertyLocationValue);
                }
            }
            else
            {
                rollbackType = "nullify"; //<-- Used for Rollbacks

                //This is the first location added to the document. Create the dictionary object and add the new locationValue
                document.Locations = new Dictionary<string, PropertyLocationValue>();
                //var swatches = new List<Swatch>();
                //swatches.Add(swatch);
                document.Locations.Add(property.PropertyName, propertyLocationValue);

            }




            #endregion

            //Update the Indexed version of the properties (Only done on search index)
            //document.IndexedProperties = GenerateIndexedProperties(document.Properties);


            #region Generate additional search metadata for locations (will be unpackaged and merged on results that include location data by the API)

            StringBuilder additionalMetaData = new StringBuilder();

            additionalMetaData.Append(propertyLocationValue.Name.ToStringOrEmpty());
            additionalMetaData.Append(" || ");
            additionalMetaData.Append(propertyLocationValue.Address1.ToStringOrEmpty());
            additionalMetaData.Append(" || ");
            additionalMetaData.Append(propertyLocationValue.Address2.ToStringOrEmpty());
            additionalMetaData.Append(" || ");
            additionalMetaData.Append(propertyLocationValue.City.ToStringOrEmpty());
            additionalMetaData.Append(" || ");
            additionalMetaData.Append(propertyLocationValue.State.ToStringOrEmpty());
            additionalMetaData.Append(" || ");
            additionalMetaData.Append(propertyLocationValue.PostalCode.ToStringOrEmpty());
            additionalMetaData.Append(" || ");
            additionalMetaData.Append(propertyLocationValue.Country.ToStringOrEmpty());

            #region Legacy

            /*
            if (!String.IsNullOrEmpty(propertyLocationValue.Name))
            {
                additionalMetaData.Append(propertyLocationValue.Name);
            }
            if (!String.IsNullOrEmpty(propertyLocationValue.Address1))
            {
                if(additionalMetaData.ToString().Length > 0)
                {
                    additionalMetaData.Append(" ");
                }                
                additionalMetaData.Append(propertyLocationValue.Address1);
            }
            if (!String.IsNullOrEmpty(propertyLocationValue.Address2))
            {
                additionalMetaData.Append(" ");
                additionalMetaData.Append(propertyLocationValue.Address2);
            }
            if (!String.IsNullOrEmpty(propertyLocationValue.City))
            {
                additionalMetaData.Append(" ");
                additionalMetaData.Append(propertyLocationValue.City);
            }
            if (!String.IsNullOrEmpty(propertyLocationValue.State))
            {
                additionalMetaData.Append(" ");
                additionalMetaData.Append(propertyLocationValue.State);
            }
            if (!String.IsNullOrEmpty(propertyLocationValue.PostalCode))
            {
                additionalMetaData.Append(" ");
                additionalMetaData.Append(propertyLocationValue.PostalCode);
            }
            if (!String.IsNullOrEmpty(propertyLocationValue.Country))
            {
                additionalMetaData.Append(" ");
                additionalMetaData.Append(propertyLocationValue.Country);
            }
            */

            #endregion

            #endregion

            try
            {
                //Replace document:
                var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;
                response.isSuccess = true;

                //Update Search Index
                #region Update Search Index (+Rollback on search index errors)

                try
                {
                    ProductSearchManager.UpdateProductPropertyInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, document.Id, property.SearchFieldName, null, ProductPropertyUpdateType.Replace, propertySearchFieldType, additionalMetaData.ToString(), geographyPointValue);
                }
                catch(Exception e)
                {
                    #region ROLLBACK - Clear Property (Any updates below NEED to be mirrored on Function above AS WELL!!!!)

                    switch (rollbackType)
                    {
                        case "revert":
                            #region REVERT

                            document.Locations[property.PropertyName] = JsonConvert.DeserializeObject<PropertyLocationValue>(previousLocationValue);

                            #endregion
                            break;

                        case "remove":
                            #region REMOVE

                            document.Locations.Remove(property.PropertyName);

                            #endregion
                            break;

                        case "nullify":
                            #region NULLIFY

                            document.Locations = null;

                            #endregion
                            break;
                    }

                    //ROLLBACK DOCUMENT
                    var rolledback = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;

                    #endregion

                    //Search issue, rollback
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating search index (product location property): Rollback was initiated after search index failure (see description). Updating location property '" + property.PropertyNameKey + "' for product '" + document.FullyQualifiedName + "' has been rolled back to org value or removed/nullified",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                    );

                    return new DataAccessResponseType{ isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                }
                
                //var documentArray = new List<ProductDocumentModel>();
                //documentArray.Add(document);
                //ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.ProductSearchIndex, documentArray, ProductSearchIndexAction.Update);

                #endregion

            }
            #region Manage Exceptions

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "attempting to add a LOCATION property to a product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to add a LOCATION property to a product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion



            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType RemoveProductPropertyCollectionItem(Account account, string fullyQualifiedName, PropertyModel property, int collectionItemIndex)
        {
            var response = new DataAccessResponseType();

            #region Get the document

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "' AND p.DocumentType = 'Product'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery, new FeedOptions { MaxItemCount = 1 }).AsEnumerable().FirstOrDefault();

            if (document == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Could not retrieve document to be updated."
                };
            }
            else
            {

            }

            #endregion

            //Used for rollbacks---------------------------------:
            //Dictionary<string, List<string>> previousPredefinedValue = null;
            //Dictionary<string, List<Swatch>> previousSwatchesValue = null;
            string previousValue = null;


            #region Update property  (Any updates below NEED to be mirrored on ROLLBACK Function further down AS WELL!!!!)

            switch (property.PropertyTypeNameKey)
            {
                case "swatch":

                    #region Swatch Property Type

                    if (document.Swatches != null)
                    {
                        //Find swatch to append to (if exists)
                        if (document.Swatches.ContainsKey(property.PropertyName))
                        {
                            previousValue = JsonConvert.SerializeObject(document.Swatches); //<-- Snaphot of Swatches before removal (For rollbacks)

                            //Remove the Swatch item from this Property
                            document.Swatches[property.PropertyName].RemoveAt(collectionItemIndex);
                        }
                    }
                    else
                    {

                    }

                    #endregion

                    break;

                case "predefined":

                    #region Predefined Property Type

                    if (document.Predefined != null)
                    {
                        //Find swatch to append to (if exists)
                        if (document.Predefined.ContainsKey(property.PropertyName))
                        {
                            previousValue = JsonConvert.SerializeObject(document.Predefined); //<-- Snaphot of Predefined before removal (For rollbacks)

                            //Remove the Swatch item from this Property
                            document.Predefined[property.PropertyName].RemoveAt(collectionItemIndex);
                        }
                    }
                    else
                    {

                    }

                    #endregion

                    break;

                default:
                    #region Basic Property Type (No Allowed)
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot remove collection item from non collection type" };
                    #endregion
            }

            #endregion

            //Update the Indexed version of the properties (Only done on search index)
            //document.IndexedProperties = GenerateIndexedProperties(document.Properties);

            try
            {
                //Replace document:
                var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;
                response.isSuccess = true;

                //Update Search Index
                #region Update Search Index (+ Rollback Search on fail)
                try
                {
                    ProductSearchManager.RemoveProductPropertyCollectionItemInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, document.Id, property.SearchFieldName, collectionItemIndex);
                }
                catch(Exception e)
                {
                    #region ROLLBACK - Clear Property (Any updates below NEED to be mirrored on Function above AS WELL!!!!)

                    switch (property.PropertyTypeNameKey)
                    {
                        case "predefined":
                            #region ROLLBACK swatch property

                            document.Predefined = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(previousValue);


                            #endregion
                            break;

                        case "swatch":
                            #region ROLLBACK swatch property

                            document.Swatches = JsonConvert.DeserializeObject<Dictionary<string, List<Swatch>>>(previousValue);

                            #endregion
                            break;
                    }

                    //ROLLBACK DOCUMENT
                    var rolledback = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;

                    #endregion

                    //Search issue, rollback
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating search index (removing collection property item): Rollback was initiated after search index failure (see description). Removing collection property item (index item: " + collectionItemIndex + ") from '" + property.PropertyNameKey + "' from product '" + document.FullyQualifiedName + "' has been rolled back to org value",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                    );

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                }

                #endregion


            }
            #region Manage Exceptions

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "attempting to remove a property collection item from a property",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to remove a property collection item from a property",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion



            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;
        }


        public static DataAccessResponseType ClearProductProperty(Account account, string fullyQualifiedName, PropertyModel property)
        {
            var response = new DataAccessResponseType();

            #region Get the document

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "' AND p.DocumentType = 'Product'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery, new FeedOptions { MaxItemCount = 1 }).AsEnumerable().FirstOrDefault();

            if (document == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Could not retrieve document to be updated."
                };
            }
            else
            {

            }

            #endregion

            bool isSearchCollection = false;

            //Used for rollbacks---------------------------------:
            //string previousStringValue = null;
            //Dictionary<string, List<string>> previousPredefinedValue = null;
            //Dictionary<string, List<Swatch>> previousSwatchesValue = null;
            //Dictionary<string, PropertyLocationValue> previousLocationsValue = null;
            string previousValue = null;

            #region Clear Property (Any updates below NEED to be mirrored on ROLLBACK Function further down AS WELL!!!!)

            switch (property.PropertyTypeNameKey)
            {
                case "predefined":
                    #region Clear swatch property

                    isSearchCollection = true;

                    //Find property to update (if exists)
                    if (document.Predefined.ContainsKey(property.PropertyName))
                    {
                        previousValue = JsonConvert.SerializeObject(document.Predefined); //<-- Snaphot of Predefined before removal (For rollbacks)
                        document.Predefined.Remove(property.PropertyName);
                    }
                    else
                    {
                        //Does not exist!
                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Predefined property does not exist on this document" };
                    }

                    #endregion
                    break;

                case "swatch":
                    #region Clear swatch property

                    isSearchCollection = true;

                    //Find property to update (if exists)
                    if (document.Swatches.ContainsKey(property.PropertyName))
                    {
                        previousValue = JsonConvert.SerializeObject(document.Swatches); //<-- Snaphot of Swatches before removal (For rollbacks)
                        document.Swatches.Remove(property.PropertyName);
                    }
                    else
                    {
                        //Does not exist!
                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Swatch does not exist on this document" };
                    }

                    #endregion
                    break;

                case "location":
                    #region Clear location property

                    //Find property to update (if exists)
                    if (document.Locations.ContainsKey(property.PropertyName))
                    {
                        previousValue = JsonConvert.SerializeObject(document.Locations); //<-- Snaphot of Locations before removal (For rollbacks)
                        document.Locations.Remove(property.PropertyName);
                    }
                    else
                    {
                        //Does not exist!
                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Swatch does not exist on this document" };
                    }

                    #endregion
                    break;

                default:
                    #region Clear basic property

                    //Find property to update (if exists)
                    if (document.Properties.ContainsKey(property.PropertyName))
                    {
                        previousValue = document.Properties[property.PropertyName];
                        document.Properties.Remove(property.PropertyName);
                    }
                    else
                    {
                        //Does not exist!
                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Property does not exist on this document" };
                    }

                    #endregion
                    break;
            }

            #endregion


            //Update the Indexed version of the properties (Only done on search index)
            //document.IndexedProperties = GenerateIndexedProperties(document.Properties);

            try
            {
                //Replace document:
                var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;
                response.isSuccess = true;

                //Update Search Index
                #region Update Search Index (+ Rollback DocDB on fail)

                try
                {
                    ProductSearchManager.ClearProductPropertyInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, document.Id, property.SearchFieldName, isSearchCollection);
                }
                catch(Exception e)
                {

                    #region ROLLBACK - Clear Property (Any updates below NEED to be mirrored on Function above AS WELL!!!!)

                    switch (property.PropertyTypeNameKey)
                    {
                        case "predefined":
                            #region ROLLBACK swatch property

                            document.Predefined = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(previousValue);


                            #endregion
                            break;

                        case "swatch":
                            #region ROLLBACK swatch property

                            document.Swatches = JsonConvert.DeserializeObject<Dictionary<string, List<Swatch>>>(previousValue);

                            #endregion
                            break;

                        case "location":
                            #region ROLLBACK location property

                            document.Locations = JsonConvert.DeserializeObject<Dictionary<string, PropertyLocationValue>>(previousValue); ;

                            #endregion
                            break;

                        default:
                            #region ROLLBACK basic property

                            document.Properties.Add(property.PropertyName, previousValue);

                            #endregion
                            break;
                    }

                    //ROLLBACK DOCUMENT
                    var rolledback = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;

                    #endregion

                    //Search issue, rollback
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating search index (clearing product property): Rollback was initiated after search index failure (see description). Clearing of property '" + property.PropertyNameKey + "' from product '" + document.FullyQualifiedName + "' has been rolled back to org value",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                    );

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                }

                #endregion
            }

            #region Manage Exceptions

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "attempting to remove a property from a product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to remove a property from a product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion

            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;
        }



        #endregion

        #region Manage Tags

        public static DataAccessResponseType AddProductTag(Account account, string fullyQualifiedName, string tagName)
        {
            var response = new DataAccessResponseType();

            #region Get the document

            //Get the DocumentDB Client
            // var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "' AND p.DocumentType = 'Product'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery, new FeedOptions { MaxItemCount = 1 }).AsEnumerable().FirstOrDefault();
            string rollbackCopy = null; //<-- copy of document before changes are made in case a rollback is required

            if (document == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Could not retrieve document to be updated."
                };
            }
            else
            {
                rollbackCopy = JsonConvert.SerializeObject(document); //<-- copy of document before changes are made in case a rollback is required
            }

            #endregion

            #region Add tag

            //Add the tag (if not exists)
            if(document.Tags != null)
            {
                if (document.Tags.Contains(tagName))
                {
                    //Tag exists!
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Product is already tagged." };
                }
                else
                {
                    //Does not exist (add tag)
                    document.Tags.Add(tagName);
                    document.Tags.Sort(); //<-- Sort alphabetically
                }

            }
            else
            {
                //Does not exist (add tag)
                if(document.Tags == null)
                {
                    document.Tags = new List<string>();
                }
                document.Tags.Add(tagName);
            }

            #endregion

            try
            {
                //Replace document:
                var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;
                response.isSuccess = true;

                //Update Search Index
                #region Update Search Index

                var documentArray = new List<ProductDocumentModel>();
                documentArray.Add(document);

                try
                {
                    ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, documentArray, ProductSearchIndexAction.Update);
                }
                catch(Exception e)
                {
                    //ROLLBACK DOCUMENT

                    var deserializedRollbackCopy = JsonConvert.DeserializeObject<ProductDocumentModel>(rollbackCopy);

                    var rolledback = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(deserializedRollbackCopy.SelfLink, deserializedRollbackCopy).Result;

                    //Search issue, rollback
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating search index (tagging product): Rollback was initiated after search index failure (see description). Addition of tag '" + tagName + "' to product '" + deserializedRollbackCopy.FullyQualifiedName + "' has been rolled back.",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                    );

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                }
                

                #endregion
            }

            #region Manage Exceptions

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "attempting to add a tag to a product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to add a tag to a product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion

            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;
        }

        public static DataAccessResponseType RemoveProductTag(Account account, string fullyQualifiedName, string tagName)
        {
            var response = new DataAccessResponseType();

            #region Get the document

            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            string sqlQuery = "SELECT * FROM Products p WHERE p.FullyQualifiedName ='" + fullyQualifiedName + "' AND p.DocumentType = 'Product'";

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);
            //Uri storedProcUri = UriFactory.CreateStoredProcedureUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition, "UpdateProduct");

            var document = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<ProductDocumentModel>(collectionUri.ToString(), sqlQuery, new FeedOptions { MaxItemCount = 1 }).AsEnumerable().FirstOrDefault();
            string rollbackCopy = null; //<-- copy of document before changes are made in case a rollback is required

            if (document == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "Could not retrieve document to be updated."
                };
            }
            else
            {
                rollbackCopy = JsonConvert.SerializeObject(document); //<-- copy of document before changes are made in case a rollback is required
            }

            #endregion

            #region Remove tag

            //Add the tag (if not exists)
            if(document.Tags != null)
            {
                if (document.Tags.Contains(tagName))
                {
                    //Tag exists, remove it
                    document.Tags.Remove(tagName);
                }
                else
                {
                    //Does not exist (add tag)
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Product does not contain this tag." }; 
                }
            }
            else
            {
                //Does not exist (add tag)
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Product does not contain this tag." };
            }

            #endregion

            try
            {
                //Replace document:
                var updated = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(document.SelfLink, document).Result;
                response.isSuccess = true;

                //Update Search Index
                #region Update Search Index

                var documentArray = new List<ProductDocumentModel>();
                documentArray.Add(document);

                try
                {
                    ProductSearchManager.UpdateProductDocumentsInSearchIndex(account.AccountNameKey, account.SearchPartition, account.ProductSearchIndex, documentArray, ProductSearchIndexAction.Update);
                }
                catch(Exception e)
                {

                    //ROLLBACK DOCUMENT
                    var deserializedRollbackCopy = JsonConvert.DeserializeObject<ProductDocumentModel>(rollbackCopy);
                    var rolledback = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.ReplaceDocumentAsync(deserializedRollbackCopy.SelfLink, deserializedRollbackCopy).Result;

                    //Search issue, rollback
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "updating search index (untagging product): Rollback was initiated after search index failure (see description). Removal of tag '" + tagName + "' from product '" + deserializedRollbackCopy.FullyQualifiedName + "' has been rolled back to org value",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                    );

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Search index down. Please try again later." };
                }
                

                #endregion
            }

            #region Manage Exceptions

            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                //exceptionMessages = de.StatusCode + " " + de.Message + " " + baseException.Message;

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    baseException,
                    "attempting to remove a tag from a product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }
            catch (Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to remive a tag from a product",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );
            }

            #endregion

            //Clear all associated caches
            if (response.isSuccess)
            {
                Caching.InvalidateProductCaches(account.AccountNameKey);
            }

            return response;
        }


        #endregion

        #region Manage Images

        /*
        public static DataAccessResponseType AddProductImage(Account account, string productId, ImageFormatInstructions imageFormatInstructions, ImageSourceFile imageSourceFile, ImageCropCoordinates imageCropCoordinates, ImageEnhancementInstructions imageEnhancementInstructions)
        {
            var result = new DataAccessResponseType();
            bool imagingSucceeded = false;
            bool blobStorageSucceeded = false;

            #region (1) Verify that the ImageFormatInstructions are valid for this ImageGroupType


            #endregion

            #region (2) Pull image from Intermediary Storage


            #endregion

            #region (3) Run imaging processing


            #endregion

            #region (4) Store image into Blob storage


            #endregion


            #region (5) Insert record for image into table storage

            if (blobStorageSucceeded)
            {
                /*
                var productImageTableEntity = new ProductImageTableEntity(account.AccountID.ToString(), productId, imageFormatInstructions.GroupNameKey, imageFormatInstructions.FormatNameKey);
                productImageTableEntity.Title = imageSourceFile.Title;
                productImageTableEntity.Description = imageSourceFile.Description;
                var tableStorageResult = Internal.ProductImagesTableStorage.StoreProductImage(productImageTableEntity);

                if (!tableStorageResult.isSuccess)
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Recording of image data into table storage failed." };
                }
                * /
            }
            else
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Storage of image as a blob failed, skipped recording into table storage." };
            }

            #endregion

            result.isSuccess = true;

            return result;
        }
    */
        #endregion


        #region Helpers


        public static bool LocationPathContainsProducts(Account account, string locationPath)
        {
            //Get the DocumentDB Client
            //var client = Sahara.Core.Settings.Azure.DocumentDB.DocumentClients.AccountDocumentClient;
            //var dbSelfLink = Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseSelfLink;
            //Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.OpenAsync();

            //Build a collection Uri out of the known IDs
            //(These helpers allow you to properly generate the following URI format for Document DB:
            //"dbs/{xxx}/colls/{xxx}/docs/{xxx}"
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(Sahara.Core.Settings.Azure.DocumentDB.AccountPartitionDatabaseId, account.DocumentPartition);

            string sqlQuery = "SELECT Top 1 p.id FROM prodId p WHERE p.LocationPath ='" + locationPath + "'";

            var documentIdResults = Sahara.Core.Settings.Azure.DocumentDbClients.AccountDocumentClient.CreateDocumentQuery<Document>(collectionUri.ToString(), sqlQuery);

            //var accountCollection = client.Crea

            //applicationImages = result.ToList();
            var documentId = documentIdResults.AsEnumerable().FirstOrDefault();

            if(documentId == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion



    }

}
