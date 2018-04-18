using AccountAdminSite.AccountManagementService;
using AccountAdminSite.ApplicationLeadsService;
using AccountAdminSite.TableEntities.SalesLeads;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    [Authorize]
    public class LeadsController : Controller
    {

        #region View Controllers

        // GET: /Leads/
        public ActionResult Index()
        {
            return View();
        }

        #endregion

        #region Settings

        [Route("Leads/Json/AddSalesLeadLabel")]
        [HttpPost]
        public JsonNetResult AddSalesLeadLabel(string label)
        {
            var response = new ApplicationLeadsService.DataAccessResponseType();
            var applicationLeadsServiceClient = new ApplicationLeadsService.ApplicationLeadsServiceClient();

            var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
            var accountNameKey = authCookie.AccountNameKey;
            var userId = authCookie.Id.ToString();

            try
            {
                applicationLeadsServiceClient.Open();
                response = applicationLeadsServiceClient.CreateLabel(accountNameKey, label,
                    userId,
                    ApplicationLeadsService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationLeadsServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationLeadsServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Leads/Json/RemoveSalesLeadLabel")]
        [HttpPost]
        public JsonNetResult RemoveSalesLeadLabel(int index)
        {
            var response = new ApplicationLeadsService.DataAccessResponseType();
            var applicationLeadsServiceClient = new ApplicationLeadsService.ApplicationLeadsServiceClient();

            var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
            var accountNameKey = authCookie.AccountNameKey;
            var userId = authCookie.Id.ToString();

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);
            var label = accountSettings.SalesSettings.LeadLabels[index];

            try
            {
                applicationLeadsServiceClient.Open();
                response = applicationLeadsServiceClient.RemoveLabel(accountNameKey, label,
                    userId,
                    ApplicationLeadsService.RequesterType.AccountUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationLeadsServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationLeadsServiceClient, exceptionMessage, currentMethodString);

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

        #region Get

        [Route("Leads/Json/GetSalesLeads")]
        [HttpPost]
        public JsonNetResult GetSalesLeads(string label, int take, string lastPartitionKey = null, string lastRowKey = null)
        {
            var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
            //var accountNameKey = authCookie.AccountNameKey;
            //var accountId = authCookie.AccountID;
            var account = Common.GetAccountObject(authCookie.AccountNameKey);
            var userId = authCookie.Id.ToString(); //<-- For future logging purposes

            List<SalesLeadTableEntity> salesLeads = null;

            #region Make sure account plan allow for sales leads

            if (account.PaymentPlan.AllowSalesLeads == false)
            {
                JsonNetResult jsonNetResultRestricted = new JsonNetResult();
                jsonNetResultRestricted.Formatting = Formatting.Indented;
                jsonNetResultRestricted.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultRestricted.Data = salesLeads;

                return jsonNetResultRestricted;
            }

            #endregion

            

            #region Connect to table storage and retreive sales leads

            #region Generate Leads Table Name

            var leadsTableName = "acc" + account.AccountID.ToString().Replace("-", "").ToLower() + "Leads" + label.Replace("-","").Replace(" ", "");

            #endregion

            #region Connect to table storage (Legacy)
            /*
            CloudStorageAccount storageAccount;
            // Credentials are from centralized CoreServiceSettings
            StorageCredentials storageCredentials = new StorageCredentials(CoreServices.PlatformSettings.Storage.AccountName, CoreServices.PlatformSettings.Storage.AccountKey);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(leadsTableName);

            cloudTable.CreateIfNotExists();
*/

            #endregion

            #region Connect to table storage partition & Set Retry Policy 

            #region Get Storage Partition

            if (CoreServices.PlatformSettings.StorageParitions == null || CoreServices.PlatformSettings.StorageParitions.ToList().Count == 0)
            {
                //No Storage Partitions Available in Static List, refresh list from Core Services
                Common.RefreshPlatformSettings();
            }

            //Get the storage partition for this account
            var storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);

            if (storagePartition == null)
            {
                //May be a new partition, refresh platform setting and try again
                Common.RefreshPlatformSettings();
                storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);
            }

            #endregion

            CloudStorageAccount storageAccount;
            StorageCredentials storageCredentials = new StorageCredentials(storagePartition.Name, storagePartition.Key);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(leadsTableName);

            cloudTable.CreateIfNotExists();

            #endregion

            var continuationToken = new TableContinuationToken();
            continuationToken.NextPartitionKey = lastPartitionKey;
            continuationToken.NextRowKey = lastRowKey;

            if(lastPartitionKey != null && lastRowKey != null)
            {
                take = take + 1; //<-- we add 1 if using continuation since the first row will be a duplicate of the last
            }


            TableQuery<SalesLeadTableEntity> query = new TableQuery<SalesLeadTableEntity>().Take(take);


            salesLeads = cloudTable.ExecuteQuerySegmented(query, continuationToken).ToList();
            #endregion

            if (lastPartitionKey != null && lastRowKey != null)
            {
                salesLeads.RemoveAt(0); //<-- we remove the first item if using continuation since the first row will be a duplicate of the last
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = salesLeads;

            return jsonNetResult;

        }

        #endregion

        #region Move

        [Route("Leads/Json/MoveSalesLead")]
        [HttpPost]
        public JsonNetResult MoveSalesLead(string partitionKey, string rowKey, string fromLabel, string toLabel)
        {
            var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
            //var accountNameKey = authCookie.AccountNameKey;
            //var accountId = authCookie.AccountID;
            var account = Common.GetAccountObject(authCookie.AccountNameKey);
            var userId = authCookie.Id.ToString(); //<-- For future logging purposes

            #region Make sure account plan allow for sales leads

            if (account.PaymentPlan.AllowSalesLeads == false)
            {
                JsonNetResult jsonNetResultRestricted = new JsonNetResult();
                jsonNetResultRestricted.Formatting = Formatting.Indented;
                jsonNetResultRestricted.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultRestricted.Data = new AccountManagementService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Account plan does not allow for sales leads" };

                return jsonNetResultRestricted;
            }

            #endregion

            SalesLeadTableEntity salesLeadToMoveAndDelete = null;
            AccountManagementService.DataAccessResponseType response = new AccountManagementService.DataAccessResponseType();

            #region Connect to table storage and retreive sales lead to move and delete from origin

            #region Generate BOTH Leads Table Name

            var leadsFromTableName = "acc" + account.AccountID.ToString().Replace("-", "").ToLower() + "Leads" + fromLabel.Replace("-", "").Replace(" ", "");
            var leadsToTableName = "acc" + account.AccountID.ToString().Replace("-", "").ToLower() + "Leads" + toLabel.Replace("-", "").Replace(" ", "");

            #endregion

            #region Connect to table storage (Legacy)
            /*
            CloudStorageAccount storageAccount;
            // Credentials are from centralized CoreServiceSettings
            StorageCredentials storageCredentials = new StorageCredentials(CoreServices.PlatformSettings.Storage.AccountName, CoreServices.PlatformSettings.Storage.AccountKey);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;
*/
            #endregion

            #region Connect to table storage partition & Set Retry Policy 

            #region Get Storage Partition

            if (CoreServices.PlatformSettings.StorageParitions == null || CoreServices.PlatformSettings.StorageParitions.ToList().Count == 0)
            {
                //No Storage Partitions Available in Static List, refresh list from Core Services
                Common.RefreshPlatformSettings();
            }

            //Get the storage partition for this account
            var storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);

            if (storagePartition == null)
            {
                //May be a new partition, refresh platform setting and try again
                Common.RefreshPlatformSettings();
                storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);
            }

            #endregion

            CloudStorageAccount storageAccount;
            StorageCredentials storageCredentials = new StorageCredentials(storagePartition.Name, storagePartition.Key);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


            #endregion

            CloudTable cloudTable = cloudTableClient.GetTableReference(leadsFromTableName);
            cloudTable.CreateIfNotExists();

            TableQuery<SalesLeadTableEntity> query = new TableQuery<SalesLeadTableEntity>().Where(
                TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(
                        "PartitionKey",
                        QueryComparisons.Equal,
                        partitionKey),

                        TableOperators.And,

                        TableQuery.GenerateFilterCondition(
                        "RowKey",
                        QueryComparisons.Equal,
                        rowKey)
                    )
                    
                );

            try
            {
                salesLeadToMoveAndDelete = cloudTable.ExecuteQuery(query).FirstOrDefault();
            }
            catch(Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;

                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultError.Data = response;

                return jsonNetResultError;
            }

            if (salesLeadToMoveAndDelete == null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "Could not locate source lead";

                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultError.Data = response;

                return jsonNetResultError;
            }

            #endregion

            #region Move source lead (then delete origin)

            try
            {
                cloudTable = cloudTableClient.GetTableReference(leadsToTableName);
                cloudTable.CreateIfNotExists();

                //Create copy for insert
                //var salesLeadToMoveAndDeleteCopy = salesLeadToMoveAndDelete;

                //create an insert operation for each entity, assign to designated CloudTable, and add to our list of tasks:
                TableOperation insertOperation = TableOperation.Insert(salesLeadToMoveAndDelete);
                var tableResultMove = cloudTable.Execute(insertOperation);

                if (tableResultMove.HttpStatusCode == 204)
                {
                    #region Delete From Origin

                    try
                    {
                        cloudTable = cloudTableClient.GetTableReference(leadsFromTableName);
                        cloudTable.CreateIfNotExists();

                        //Geta fresh copy from org table (so we don't get an issue with changing ETag)
                        salesLeadToMoveAndDelete = cloudTable.ExecuteQuery(query).FirstOrDefault();

                        //create an insert operation for each entity, assign to designated CloudTable, and add to our list of tasks:
                        TableOperation deleteOperation = TableOperation.Delete(salesLeadToMoveAndDelete);
                        var tableResultDelete = cloudTable.Execute(deleteOperation);

                        if (tableResultDelete.HttpStatusCode == 204)
                        {
                            //Both leads were moved successfully:
                            response.isSuccess = true;
                        }
                        else
                        {
                            response.isSuccess = false;
                            response.ErrorMessage = "Lead duplicated, but could not delete from origin for an unknown reason";

                            JsonNetResult jsonNetResultError = new JsonNetResult();
                            jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                            jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                            jsonNetResultError.Data = response;

                            return jsonNetResultError;
                        }
                    }
                    catch(Exception e)
                    {
                        response.isSuccess = false;
                        response.ErrorMessage = "Lead duplicated, but could not delete from origin: " + e.Message;

                        JsonNetResult jsonNetResultError = new JsonNetResult();
                        jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                        jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                        jsonNetResultError.Data = response;

                        return jsonNetResultError;
                    }


                    #endregion

                    }
            }
            catch(Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = "Could not move lead to new label: " + e.Message;

                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultError.Data = response;

                return jsonNetResultError;
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        #endregion

        #region Update

        [Route("Leads/Json/UpdateSalesLead")]
        [HttpPost]
        public JsonNetResult UpdateSalesLead(string partitionKey, string rowKey, string label, string field, string value)
        {
            var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
            //var accountNameKey = authCookie.AccountNameKey;
            //var accountId = authCookie.AccountID;
            var account = Common.GetAccountObject(authCookie.AccountNameKey);
            var userId = authCookie.Id.ToString(); //<-- For future logging purposes

            #region Make sure account plan allow for sales leads

            if (account.PaymentPlan.AllowSalesLeads == false)
            {
                JsonNetResult jsonNetResultRestricted = new JsonNetResult();
                jsonNetResultRestricted.Formatting = Formatting.Indented;
                jsonNetResultRestricted.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultRestricted.Data = new AccountManagementService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Account plan does not allow for sales leads" };

                return jsonNetResultRestricted;
            }

            #endregion

            SalesLeadTableEntity salesLeadToUpdate = null;
            AccountManagementService.DataAccessResponseType response = new AccountManagementService.DataAccessResponseType();

            #region Connect to table storage, retreive sales lead and make the update

            #region Generate Leads Table Name

            var leadsTableName = "acc" + account.AccountID.ToString().Replace("-", "").ToLower() + "Leads" + label.Replace("-", "").Replace(" ", "");

            #endregion

            #region Connect to table storage (Legacy)
            /*
            CloudStorageAccount storageAccount;
            // Credentials are from centralized CoreServiceSettings
            StorageCredentials storageCredentials = new StorageCredentials(CoreServices.PlatformSettings.Storage.AccountName, CoreServices.PlatformSettings.Storage.AccountKey);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;
            */

            #endregion

            #region Connect to table storage partition & Set Retry Policy 

            #region Get Storage Partition

            if (CoreServices.PlatformSettings.StorageParitions == null || CoreServices.PlatformSettings.StorageParitions.ToList().Count == 0)
            {
                //No Storage Partitions Available in Static List, refresh list from Core Services
                Common.RefreshPlatformSettings();
            }

            //Get the storage partition for this account
            var storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);

            if (storagePartition == null)
            {
                //May be a new partition, refresh platform setting and try again
                Common.RefreshPlatformSettings();
                storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);
            }

            #endregion

            CloudStorageAccount storageAccount;
            StorageCredentials storageCredentials = new StorageCredentials(storagePartition.Name, storagePartition.Key);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


            #endregion

            #region Select the Entity to update

            CloudTable cloudTable = cloudTableClient.GetTableReference(leadsTableName);
            cloudTable.CreateIfNotExists();

            TableQuery<SalesLeadTableEntity> query = new TableQuery<SalesLeadTableEntity>().Where(
                TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(
                        "PartitionKey",
                        QueryComparisons.Equal,
                        partitionKey),

                        TableOperators.And,

                        TableQuery.GenerateFilterCondition(
                        "RowKey",
                        QueryComparisons.Equal,
                        rowKey)
                    )

                );

            try
            {
                salesLeadToUpdate = cloudTable.ExecuteQuery(query).FirstOrDefault();
            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;

                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultError.Data = response;

                return jsonNetResultError;
            }

            if (salesLeadToUpdate == null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "Could not locate lead to update";

                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultError.Data = response;

                return jsonNetResultError;
            }

            #endregion

            #endregion

            #region Make the updates (on allowed fields)

            switch(field)
            {
                case "FirstName":
                    #region Manage Update

                    if (value.Length > 35)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 35);

                        #endregion
                    }

                    salesLeadToUpdate.FirstName = value;

                    #endregion
                    break;

                case "LastName":
                    #region Manage Update

                    if (value.Length > 35)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 35);

                        #endregion
                    }

                    salesLeadToUpdate.LastName = value;

                    #endregion
                    break;

                case "CompanyName":
                    #region Manage Update

                    if (value.Length > 60)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 60);

                        #endregion
                    }

                    salesLeadToUpdate.CompanyName = value;

                    #endregion
                    break;

                case "Comments":
                    #region Manage Update

                    if (value.Length > 600)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 600);

                        #endregion
                    }

                    salesLeadToUpdate.Comments = value;

                    #endregion
                    break;

                case "Notes":
                    #region Manage Update

                    if (value.Length > 600)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 600);

                        #endregion
                    }

                    salesLeadToUpdate.Notes = value;

                    #endregion
                    break;

                case "Phone":
                    #region Manage Update

                    if(value.Length > 40)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 40);

                        #endregion
                    }

                    salesLeadToUpdate.Phone = value;

                    #endregion
                    break;

                case "Email":
                    #region Manage Update

                    if (value.Length > 40)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 40);

                        #endregion
                    }

                    salesLeadToUpdate.Email = value;

                    #endregion
                    break;

                case "LocationPath":
                    #region Manage Update

                    if (value.Length > 300)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 300);

                        #endregion
                    }

                    salesLeadToUpdate.LocationPath = value;

                    #endregion
                    break;

                case "FullyQualifiedName":
                    #region Manage Update

                    if (value.Length > 400)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 400);

                        #endregion
                    }

                    salesLeadToUpdate.FullyQualifiedName = value;

                    #endregion
                    break;

                case "Origin":
                    #region Manage Update

                    if (value.Length > 35)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 35);

                        #endregion
                    }

                    salesLeadToUpdate.Origin = value;

                    #endregion
                    break;

                case "IPAddress":
                    #region Manage Update

                    if (value.Length > 120)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 120);

                        #endregion
                    }

                    salesLeadToUpdate.IPAddress = value;

                    #endregion
                    break;

                case "ProductName":
                    #region Manage Update

                    if (value.Length > 240)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 240);

                        #endregion
                    }

                    salesLeadToUpdate.ProductName = value;

                    #endregion
                    break;

                case "ProductID":
                    #region Manage Update

                    if (value.Length > 120)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 120);

                        #endregion
                    }

                    salesLeadToUpdate.ProductID = value;

                    #endregion
                    break;

                case "Object":
                    #region Manage Update

                    if (value.Length > 10000)
                    {
                        #region Too long (Truncate)

                        value = value.Substring(0, 10000);

                        #endregion
                    }

                    salesLeadToUpdate.Object = value;

                    #endregion
                    break;

                default:
                    #region Not allowed, return result

                    response.isSuccess = false;
                    response.ErrorMessage = "Can not make updates to fields titled " + field;

                    JsonNetResult jsonNetResultError = new JsonNetResult();
                    jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                    jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                    jsonNetResultError.Data = response;

                    return jsonNetResultError;

                    #endregion
            }

            #endregion


            try
            {

                TableOperation updateOperation = TableOperation.Replace(salesLeadToUpdate);
                var tableResultUpdate = cloudTable.Execute(updateOperation);

                if(tableResultUpdate.HttpStatusCode == 204)
                {
                    response.isSuccess = true;
                }
                else
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Could not make update. HttpStatusCode:" + tableResultUpdate.HttpStatusCode;
                }

            }
            catch (Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = "Could not make update: " + e.Message;

                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultError.Data = response;

                return jsonNetResultError;
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        #endregion

        #region Submit

        [Route("Leads/Json/SubmitSalesLead")]
        [HttpPost]
        public JsonNetResult SubmitSalesLead(string label = "New", string firstName = "", string lastName = "", string companyName = "", string phone = "", string email = "", string comments = "", string notes = "", string productName = "", string productId = "", string fullyQualifiedName = "", string locationPath = "", string origin = "", string ipAddress = "", string userId = "", string userName = "")
        {

            var response = new AccountManagementService.DataAccessResponseType();

            var authCookie = AuthenticationCookieManager.GetAuthenticationCookie();
            //var accountNameKey = authCookie.AccountNameKey;
            //var accountId = authCookie.AccountID;
            var account = Common.GetAccountObject(authCookie.AccountNameKey);

            #region Make sure account plan allow for sales leads

            if (account.PaymentPlan.AllowSalesLeads == false)
            {
                JsonNetResult jsonNetResultRestricted = new JsonNetResult();
                jsonNetResultRestricted.Formatting = Formatting.Indented;
                jsonNetResultRestricted.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultRestricted.Data = new AccountManagementService.DataAccessResponseType { isSuccess = false, ErrorMessage = "Account plan does not allow for sales leads" };

                return jsonNetResultRestricted;
            }

            #endregion

            if (label == "" || label == null)
            {
                label = "New";
            }
            if(userId == "" || userId == null)
            {
                userId = authCookie.Id.ToString();
            }
            if (userName == "" || userName == null)
            {
                userName = authCookie.FirstName + " " + authCookie.LastName;
            }


            #region Generate Leads Table Name

            var newLeadsTableName = "acc" + account.AccountID.ToString().Replace("-", "").ToLower() + "Leads" + label;

            #endregion

            #region Connect to table storage (Legacy)
            /*
            CloudStorageAccount storageAccount;
            // Credentials are from centralized CoreServiceSettings
            StorageCredentials storageCredentials = new StorageCredentials(CoreServices.PlatformSettings.Storage.AccountName, CoreServices.PlatformSettings.Storage.AccountKey);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;
*/
            #endregion

            #region Connect to table storage partition & Set Retry Policy 

            #region Get Storage Partition

            if (CoreServices.PlatformSettings.StorageParitions == null || CoreServices.PlatformSettings.StorageParitions.ToList().Count == 0)
            {
                //No Storage Partitions Available in Static List, refresh list from Core Services
                Common.RefreshPlatformSettings();
            }

            //Get the storage partition for this account
            var storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);

            if (storagePartition == null)
            {
                //May be a new partition, refresh platform setting and try again
                Common.RefreshPlatformSettings();
                storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);
            }

            #endregion

            CloudStorageAccount storageAccount;
            StorageCredentials storageCredentials = new StorageCredentials(storagePartition.Name, storagePartition.Key);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


            #endregion

            #region Create TableEntity for Lead

            SalesLeadTableEntity salesLead = new SalesLeadTableEntity(cloudTableClient, newLeadsTableName);

            salesLead.FirstName = firstName;
            salesLead.LastName = lastName;
            salesLead.CompanyName = companyName;

            salesLead.Origin = origin;
            salesLead.Phone = phone;
            salesLead.Email = email;

            salesLead.Comments = comments;
            salesLead.Notes = notes;

            salesLead.ProductID = productId;
            salesLead.ProductName = productName;
            salesLead.IPAddress = ipAddress;
            salesLead.FullyQualifiedName = fullyQualifiedName;
            salesLead.LocationPath = locationPath;

            salesLead.UserID = userId;
            salesLead.UserName = userName;

            #endregion

            #region Insert Entity

            try
            {
                //create an insert operation for each entity, assign to designated CloudTable, and add to our list of tasks:
                TableOperation operation = TableOperation.Insert(salesLead);
                var tableResult = salesLead.cloudTable.Execute(operation);

                if (tableResult.HttpStatusCode == 204)
                {
                    response.isSuccess = true;
                }
                else
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Could not insert lead. HttpStatusCode: " + tableResult.HttpStatusCode;
                }
            }
            catch(Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = "Could not insert lead: " + e.Message;
            }



            #endregion

            #region Send email alerts (If ON) using BackgroundTask (We don't do this on the ADMIN side - just on the site

            //if (isSuccess)
            //{
            //BackgroundTasks.SalesAlerts.SendSalesAlerts(accountNameKey, firstName, lastName, companyName, phone, email, comments, productName, productId, fullyQualifiedName, locationPath, origin, ipAddress);
            //}

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion
    }


}