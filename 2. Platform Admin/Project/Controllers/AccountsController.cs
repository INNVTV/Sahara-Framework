using Newtonsoft.Json;
using PlatformAdminSite.AccountManagementService;
using PlatformAdminSite.AccountRegistrationService;
using PlatformAdminSite.PlatformManagementService;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{

    [Authorize]
    public class AccountsController : Controller
    {
    
        #region View Controllers
        //
        // GET: /Accounts/
        public ActionResult Index()
        {
            return View();
        }

        // Used for Account Details variation
        // GET: /Account/
        [Route("Account/{name}")]
        public ActionResult Details()
        {
            return View("Index"); //<---Redirect to main index, angular will take ouver routing (via local "app.js" file)
        }

        #endregion

        #region Json Services

        #region Counts, Lists & Searches

        #region Counts


        [Route("Accounts/Json/Count")]
        [HttpGet]
        public JsonResult GetAccountCount()
        {
            int count = 0;
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                count = accountManagementServiceClient.GetAccountCount(Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            return Json(count, JsonRequestBehavior.AllowGet);
        }

        [Route("Accounts/Json/Count/ByFilter")]
        [HttpGet]
        public JsonResult GetAccountsCountByFilter(string filter, string value)
        {
            int count = 0;
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                count = accountManagementServiceClient.GetAccountCountByFilter(filter, value, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            return Json(count, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Account Lists(s)

        [Route("Accounts/Json/Get")]
        [HttpGet]
        public JsonNetResult GetAccounts(int page, int amount, string sortBy, string direction)
        {
            var accounts = new List<AccountManagementService.Account>();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                accounts = accountManagementServiceClient.GetAccounts(page, amount, sortBy + " " + direction, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime:
            jsonNetResult.Data = accounts;

            return jsonNetResult;

        }




        [Route("Accounts/Json/Get/ByFilter")]
        [HttpGet]
        public JsonNetResult GetAccountsByFilter(int page, int amount, string filter, string value, string sortBy, string direction)
        {
            var accounts = new List<AccountManagementService.Account>();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                accounts = accountManagementServiceClient.GetAccountsByFilter(filter, value, page, amount, sortBy + " " + direction, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }


            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime:
            jsonNetResult.Data = accounts;

            return jsonNetResult;

            //return Json(accounts, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)] //<-- Allows for special characters to be sent is as search queries, such as "&"
        [Route("Accounts/Json/Search")]
        [HttpGet]
        public JsonNetResult SearchAccounts(string query, int maxResults)
        {
            var accounts = new List<AccountManagementService.Account>();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                accounts = accountManagementServiceClient.SearchAccounts(query, maxResults, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime:
            jsonNetResult.Data = accounts;

            return jsonNetResult;

            //return Json(accounts, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region Account

        #region Account Details

        [Route("Accounts/Json/Details/{name}")]
        public JsonNetResult Detail(string name)
        {
            var account = new AccountManagementService.Account();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                account = accountManagementServiceClient.GetAccount(name, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }
            
            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime:
            jsonNetResult.Data = account;

            return jsonNetResult;
        }

        #region Account Capacity

        [Route("Accounts/Json/GetAccountCapacity")]
        [HttpGet]
        public JsonNetResult GetAccountCapacity(string accountId)
        {

            AccountCapacity accountCapacity = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                //First we attempt to get tags from the Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "capacities";
                string hashField = "account:" + accountId;

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    accountCapacity = JsonConvert.DeserializeObject<AccountCapacity>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //Log error message for Redis call
            }

            #endregion

            if (accountCapacity == null)
            {
                #region (Plan B) Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    accountCapacity = accountManagementServiceClient.GetAccountCapacity(accountId, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(accountManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = accountCapacity;

            return jsonNetResult;

        }

        #endregion


        #endregion

        #region Account Updates


        [Route("Accounts/Json/UpdateAccountName")]
        [HttpPost]
        public JsonNetResult UpdateAccountName(string accountId, string newName)
        {
            //Confirm name
            #region Confirm
            /*
            if (password != confirmPassword)
            {
                var error = new DataAccessResponseType();
                error.isSuccess = false;
                error.ErrorMessage = "Password and confirmation password do not match.";

                JsonNetResult jsonNetErrorResult = new JsonNetResult();
                jsonNetErrorResult.Formatting = Formatting.Indented;
                jsonNetErrorResult.Data = error;

                return jsonNetErrorResult;

            }*/
            #endregion

            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountName(accountId, newName,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection//response.ErrorMessages[0] = exceptionMessage;
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                ////response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #region Close Account

        [Route("Accounts/Json/CloseUnprovisionedAccount")]
        [HttpGet]
        public JsonNetResult CloseUnprovisionedAccount(string accountId)
        {

            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            var response = new PlatformAdminSite.AccountManagementService.DataAccessResponseType();

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.CloseUnprovisionedAccount(accountId, user.Id, AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        [Route("Accounts/Json/CloseAccount")]
        [HttpGet]
        public JsonNetResult CloseAccount(string accountId)
        {

            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            var response = new PlatformAdminSite.AccountManagementService.DataAccessResponseType();

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.CloseAccount(accountId, user.Id, AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }




            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        [Route("Accounts/Json/DoesAccountRequireClosureApproval")]
        [HttpGet]
        public JsonNetResult DoesAccountRequireClosureApproval(string accountId)
        {

            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            bool response = false;

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.DoesAccountRequireClosureApproval(accountId);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }




            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        [Route("Accounts/Json/ApproveAccountClosure")]
        [HttpGet]
        public JsonNetResult ApproveAccountClosure(string accountId)
        {

            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            var response = new PlatformAdminSite.AccountManagementService.DataAccessResponseType();

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountClosureApproval(accountId, true, user.Id, AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }




            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        [Route("Accounts/Json/ReversAccounteClosureApproval")]
        [HttpGet]
        public JsonNetResult ReversAccounteClosureApproval(string accountId)
        {

            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            var response = new PlatformAdminSite.AccountManagementService.DataAccessResponseType();

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountClosureApproval(accountId, false, user.Id, AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }




            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        [Route("Accounts/Json/ReactivateSubscription")]
        [HttpGet]
        public JsonNetResult ReactivateSubscription(string accountId)
        {

            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            var response = new PlatformAdminSite.AccountManagementService.DataAccessResponseType();

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.ReactivateSubscription(accountId, user.Id, AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }




            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        [Route("Accounts/Json/AccelerateAccountClosure")]
        [HttpGet]
        public JsonNetResult AccelerateAccountClosure(string accountId)
        {

            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            var response = new PlatformAdminSite.AccountManagementService.DataAccessResponseType();

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.AccelerateAccountClosure(accountId, user.Id, AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;

        }

        #endregion

        #endregion

        #region Account Provisioning

        [Route("Accounts/Json/ProvisionAccount")]
        [HttpPost]
        public JsonNetResult ProvisionAccount(string accountId)
        {
            var response = new AccountRegistrationService.DataAccessResponseType();
            var accountRegistrationServiceClient = new AccountRegistrationService.AccountRegistrationServiceClient();

            try
            {
                var id = AuthenticationCookieManager.GetAuthenticationCookie().Id;


                accountRegistrationServiceClient.Open();

                response = accountRegistrationServiceClient.ProvisionAccount(accountId, id, AccountRegistrationService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountRegistrationServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountRegistrationServiceClient, exceptionMessage, currentMethodString);

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

        /// <summary>
        /// Check to see if partitions are available for an account plan before provisioning is allowed.
        /// </summary>
        /// <param name="searchPlan"></param>
        /// <returns></returns>
        [Route("Accounts/Json/PartitionsAvailable")]
        [HttpGet]
        public JsonNetResult PartitionsAvailable(string searchPlan)
        {
            var response = new AccountRegistrationService.DataAccessResponseType();

            List<StoragePartition> storagePartitions = null;
            List<SearchPartition> searchPartitionsFull = null;

            #region Get list of Search and Storage Partitions available

            var PlatformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

            try
            {
                PlatformManagementServiceClient.Open();

                storagePartitions = PlatformManagementServiceClient.GetStoragePartitions(Common.SharedClientKey).ToList();
                searchPartitionsFull = PlatformManagementServiceClient.GetSearchPartitions(Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(PlatformManagementServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(PlatformManagementServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                //response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }

            #endregion

            response.isSuccess = true;

            #region Ensure that there is a storage partition available and select next available spot

            //Sort with lowest tenant count at the top:
            storagePartitions = storagePartitions.OrderBy(o => o.TenantCount).ToList();

            if (storagePartitions.Count > 0)
            {
                if (storagePartitions[0].TenantCount >= storagePartitions[0].MaxTenants)
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "There are no storage partitions available for provisioning. ";
                }
            }
            else
            {
                response.isSuccess = false;
                response.ErrorMessage = "There are no storage partitions available for provisioning. ";
            }

            #endregion

            #region Ensure that there is a search partition available and select next available spot

            var searchPartitionsFiltered = new List<SearchPartition>();

            //Filter search partitions down to only those that are of the same plan as required
            foreach(var partition in searchPartitionsFull)
            {
                if(partition.Plan == searchPlan)
                {
                    searchPartitionsFiltered.Add(partition);
                }
            }


            //Sort with lowest tenant count at the top:
            searchPartitionsFiltered = searchPartitionsFiltered.OrderBy(o => o.TenantCount).ToList();

            if (searchPartitionsFiltered.Count > 0)
            {
                if (searchPartitionsFiltered[0].TenantCount >= searchPartitionsFiltered[0].MaxTenants)
                {
                    response.isSuccess = false;
                    response.ErrorMessage += "There are no '" + searchPlan + "' search partitions available for provisioning.";
                }
            }
            else
            {
                response.isSuccess = false;
                response.ErrorMessage += "There are no '" + searchPlan + "' search partitions available for provisioning.";
            }
            

            #endregion



            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #region Account Users Lists, Invitations, Creations & Roles

        /*
        [Route("Accounts/Json/Roles")]
        [HttpGet]
        public JsonNetResult GetUserRoles()
        {

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            accountManagementServiceClient.Open();

            var roles = accountManagementServiceClient.GetAccountUserRoles();
            accountManagementServiceClient.Close();

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.Data = roles;

            return jsonNetResult;
        }*/


        #region Users



        [Route("Accounts/Json/GetUsers/{accountId}")]
        [HttpGet]
        public JsonNetResult GetUsers(string accountId)
        {
            var users = new List<AccountManagementService.AccountUser>();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            var response = new AccountManagementService.DataAccessResponseType();
            try
            {
                accountManagementServiceClient.Open();
                users = accountManagementServiceClient.GetAccountUsers(accountId, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = users;

            return jsonNetResult;
        }



        [Route("Accounts/Json/CreateUser")]
        [HttpPost]
        public JsonNetResult CreateUser(string accountId, string email, string firstName, string lastName, string role, bool isOwner, string password, string confirmPassword)
        {
            //Confirm passwords before sending to core services
            #region Confirm
            if (password != confirmPassword)
            {
                var error = new AccountManagementService.DataAccessResponseType();
                error.isSuccess = false;
                error.ErrorMessage = "Password and confirmation password do not match.";

                JsonNetResult jsonNetErrorResult = new JsonNetResult();
                jsonNetErrorResult.Formatting = Newtonsoft.Json.Formatting.Indented;
                jsonNetErrorResult.Data = error;

                return jsonNetErrorResult;

            }
            #endregion

            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.CreateAccountUser(accountId, email, firstName, lastName, password, role, isOwner,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                ////response.ErrorMessages[0] = exceptionMessage;

                #endregion
            }


            



            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Accounts/Json/DeleteUser")]
        [HttpPost]
        public JsonNetResult DeleteUser(string accountId, string userId)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.DeleteAccountUser(accountId, userId,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.isSuccess = false;
                response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Accounts/Json/UpdateName")]
        [HttpPost]
        public JsonNetResult UpdateName(string accountId, string userId, string firstName, string lastName)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountUserFullName(accountId, userId, firstName, lastName,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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


        [Route("Accounts/Json/UpdateEmail")]
        [HttpPost]
        public JsonNetResult UpdateEmail(string accountId, string userId, string email)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountUserEmail(accountId, userId, email,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Accounts/Json/UpdateRole")]
        [HttpPost]
        public JsonNetResult UpdateRole(string accountId, string userId, string role)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {           
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountUserRole(accountId,
                    userId,
                    role,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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


        [Route("Accounts/Json/ChangeOwnerStatus")]
        [HttpPost]
        public JsonNetResult ChangeOwnerStatus(string accountId, string userId, bool isOwner)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountOwnershipStatus(accountId, userId, isOwner,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Accounts/Json/ChangeActiveState")]
        [HttpPost]
        public JsonNetResult ChangeActiveState(string accountId, string userId, bool isActive)
        {

            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
 
            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountUserActiveState(accountId, userId, isActive,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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


        [Route("Accounts/Json/SendPasswordLink")]
        [HttpPost]
        public JsonNetResult SendPasswordLink(string accountId, string email)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.ClaimLostPassword(accountId, email, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);

            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Accounts/Json/GetAllAccountsForEmail")]
        [HttpPost]
        public JsonNetResult GetAllAccountsForEmail(string email)
        {
            var response = new List<UserAccount>();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.GetAllAccountsForEmail(email, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;


            return jsonNetResult;
        }

        #endregion

        #region Invitations

        [Route("Accounts/Json/GetInvitations/{accountId}")]
        [HttpGet]
        public JsonNetResult GetInvitations(string accountId)
        {
            var response = new List<UserInvitation>();
            var accountManagementServiceClient = new AccountManagementServiceClient();
            
            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.GetAccountUserInvitations(accountId, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("Accounts/Json/InviteUser")]
        [HttpPost]
        public JsonNetResult InviteUser(string accountId, string email, string firstName, string lastName, string role, bool isOwner)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.InviteAccountUser(accountId, email, firstName, lastName, role, isOwner,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Accounts/Json/DeleteInvitation")]
        [HttpPost]
        public JsonNetResult DeleteInvitation(string accountId, string invitationKey)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.DeleteAccountUserInvitation(accountId, invitationKey,
                    AuthenticationCookieManager.GetAuthenticationCookie().Id,
                    AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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

        [Route("Accounts/Json/ResendInvitation")]
        [HttpPost]
        public JsonNetResult ResendInvitation(string accountId, string invitationKey)
        {

            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementServiceClient();

            try
            {

                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.ResendAccountUserInvitation(accountId, invitationKey, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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

        #region Password Claims

        [Route("Accounts/Json/GetPasswordClaims/{accountId}")]
        [HttpGet]
        public JsonNetResult GetPasswordClaims(string accountId)
        {
            var response = new List<UserPasswordResetClaim>();
            var accountManagementServiceClient = new AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.GetLostPasswordClaims(accountId, AuthenticationCookieManager.GetAuthenticationCookie().Id, AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        #endregion

        #endregion

        #region Account Credit Cards, Subscriptions

        [Route("Accounts/Json/GetCardInfo/{accountId}")]
        [HttpGet]
        public JsonNetResult GetCardInfo(string accountId)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            AccountCreditCardInfo accountCreditCardInfo = null;

            #region (Plan A) Get data from Redis Cache

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashKey = "accountbyid:" + accountId.ToString().Replace("-", "");
                string hashField = "creditcard";

                var redisValue = cache.HashGet(hashKey, hashField);

                if (redisValue.HasValue)
                {
                    accountCreditCardInfo = JsonConvert.DeserializeObject<AccountCreditCardInfo>(redisValue);
                }
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (accountCreditCardInfo == null)
            {
                #region (Plan B) Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    accountCreditCardInfo = accountManagementServiceClient.GetCreditCardInfo(
                        accountId.ToString(),
                        user.Id,
                        PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(accountManagementServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }




                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = accountCreditCardInfo;

            return jsonNetResult;
        }


        [Route("Accounts/Json/AddUpdateCard")]
        [HttpPost]
        public JsonNetResult AddUpdateCard(string accountId, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear)
        {
            var response = new AccountManagementService.DataAccessResponseType();
            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                //Get the ip address and call origin
                var ipAddress = Request.UserHostAddress;
                var origin = "Web";
                var id = AuthenticationCookieManager.GetAuthenticationCookie().Id;


                accountManagementServiceClient.Open();

                response = accountManagementServiceClient.AddUpdateCreditCard(
                    accountId,
                    cardName,
                    cardNumber,
                    cvc,
                    expirationMonth,
                    expirationYear,
                    id,
                    AccountManagementService.RequesterType.PlatformUser,
                    ipAddress,
                    origin, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

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

        #region AccountLogs

        // Logs -----

        [Route("Accounts/Json/GetAccountLog")]
        [HttpGet]
        public JsonNetResult GetAccountLog(string accountId, int maxRecords)
        {

            #region Get data from WCF

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<PlatformAdminSite.AccountManagementService.AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLog(accountId, maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Accounts/Json/GetAccountLogByCategory")]
        [HttpGet]
        public JsonNetResult GetAccountLogByCategory(string accountId, string categoryType, int maxRecords)
        {

            #region Get data from WCF

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<PlatformAdminSite.AccountManagementService.AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLogByCategory(accountId, categoryType, maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Accounts/Json/GetAccountLogByActivity")]
        [HttpGet]
        public JsonNetResult GetAccountLogByActivity(string accountId, string activityType, int maxRecords)
        {

            #region Get data from WCF

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<PlatformAdminSite.AccountManagementService.AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLogByActivity(accountId, activityType, maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Accounts/Json/GetAccountLogByUser")]
        [HttpGet]
        public JsonNetResult GetAccountLogByUser(string accountId, string userId, int maxRecords)
        {

            #region Get data from WCF

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<PlatformAdminSite.AccountManagementService.AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLogByUser(accountId, userId, maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        [Route("Accounts/Json/GetAccountLogByObject")]
        [HttpGet]
        public JsonNetResult GetAccountLogByObject(string accountId, string objectId, int maxRecords)
        {

            #region Get data from WCF

            /*
                 
                 Update Web.Config to below if you get this error:
                 
                 The maximum message size quota for incoming messages (65536) has been exceeded. To increase the quota, use the MaxReceivedMessageSize property on the appropriate binding element
                 
                 <binding name="NetTcpBinding_IAccountManagementService" maxBufferSize="262114" maxReceivedMessageSize="262114"> <!-- FOR LARGE LOG DATA-->
                    <security mode="None" />
                </binding>
                <binding name="NetTcpBinding_IPlatformManagementService" maxBufferSize="262114" maxReceivedMessageSize="262114"> <!-- FOR LARGE LOG DATA -->
                    <security mode="None" />
                </binding>
                  
                 
                 */

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
            var creditsTransactonLog = new List<PlatformAdminSite.AccountManagementService.AccountActivityLog>();

            try
            {
                accountManagementServiceClient.Open();
                creditsTransactonLog = accountManagementServiceClient.GetAccountLogByObject(accountId, objectId, maxRecords, Common.SharedClientKey).ToList();

                //Close the connection
                WCFManager.CloseConnection(accountManagementServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                /*
                 
                 Update Web.Config to below if you get this error:
                 
                 The maximum message size quota for incoming messages (65536) has been exceeded. To increase the quota, use the MaxReceivedMessageSize property on the appropriate binding element
                 
                 <binding name="NetTcpBinding_IAccountManagementService" maxBufferSize="262114" maxReceivedMessageSize="262114"> <!-- FOR LARGE LOG DATA-->
                    <security mode="None" />
                </binding>
                <binding name="NetTcpBinding_IPlatformManagementService" maxBufferSize="262114" maxReceivedMessageSize="262114"> <!-- FOR LARGE LOG DATA -->
                    <security mode="None" />
                </binding>
                  
                 
                 */

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                #endregion
            }

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = creditsTransactonLog;

            return jsonNetResult;

        }

        // Log Types -----

        [Route("Accounts/Json/GetAccountLogCategories")]
        [HttpGet]
        public JsonNetResult GetAccountLogCategories()
        {
            var categories = new List<string>();

            var categoryCacheKey = "accountLogCategories";
            var cacheHours = 8;

            if (System.Web.HttpRuntime.Cache.Get(categoryCacheKey) == null)
            {
                #region Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    categories = accountManagementServiceClient.GetAccountLogCategories(Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountManagementServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                //Store into local cache for 8 hours
                System.Web.HttpRuntime.Cache.Insert(categoryCacheKey, categories, null, DateTime.Now.AddHours(cacheHours), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);


                #endregion
            }
            else
            {
                //Get data from cache
                categories = (List<string>)System.Web.HttpRuntime.Cache.Get(categoryCacheKey);    
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = categories;

            return jsonNetResult;

        }

        [Route("Accounts/Json/GetAccountLogActivities")]
        [HttpGet]
        public JsonNetResult GetAccountLogActivities()
        {
            var activities = new List<string>();

            var activityCacheKey = "accountLogActivities";
            var cacheHours = 8;

            if (System.Web.HttpRuntime.Cache.Get(activityCacheKey) == null)
            {
                #region Get data from WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();
                    activities = accountManagementServiceClient.GetAccountLogActivities(Common.SharedClientKey).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountManagementServiceClient);

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }


                #endregion

                #region Add Breaks for Dropdown Sections

                //Add breaks for sections in dropdowns
                var currentSectionName = String.Empty;

                for (int i = 0; i < activities.Count; i++)
                {
                    var thisSection = activities[i].Substring(0, activities[i].IndexOf("_"));

                    if (currentSectionName != thisSection && i != 0)
                    {
                        //Add a break
                        activities.Insert(i, "break");
                    }

                    currentSectionName = thisSection;

                }

                #endregion

                //Store into local cache for 8 hours
                System.Web.HttpRuntime.Cache.Insert(activityCacheKey, activities, null, DateTime.Now.AddHours(cacheHours), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);

            }
            else
            {
                //Get data from cache
                activities = (List<string>)System.Web.HttpRuntime.Cache.Get(activityCacheKey);
            }



            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = activities;

            return jsonNetResult;

        }

        #endregion

        #region Data Injection

        [Route("Accounts/Json/InjectImageDocuments")]
        [HttpGet]
        public JsonNetResult InjectImageDocuments(string accountId, int imageDocumentCount)
        {

            var response = new ApplicationDataInjectionService.DataAccessResponseType();
            var applicationDataInjectionServiceClient = new ApplicationDataInjectionService.ApplicationDataInjectionServiceClient();
            try
            {
                applicationDataInjectionServiceClient.Open();
                response = applicationDataInjectionServiceClient.InjectImageDocumentsIntoAccount(accountId, imageDocumentCount,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                PlatformAdminSite.ApplicationDataInjectionService.RequesterType.PlatformUser, Common.SharedClientKey);

                //Close the connection
                WCFManager.CloseConnection(applicationDataInjectionServiceClient);
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(applicationDataInjectionServiceClient, exceptionMessage, currentMethodString);

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