using Newtonsoft.Json;
using PlatformAdminSite.AccountBillingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    public class InvoicesController : Controller
    {
        #region JSON Services

        [Route("Invoices/Json/GetInvoice")]
        [HttpGet]
        public JsonNetResult GetInvoice(string invoiceId)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            Invoice invoice = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

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
            }*/

            #endregion

            if (invoice == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    invoice = accountBillingServiceClient.GetInvoice(
                        invoiceId, Common.SharedClientKey);//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser);

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = invoice;

            return jsonNetResult;
        }

        [Route("Invoices/Json/GetNextInvoice")]
        [HttpGet]
        public JsonNetResult GetNextInvoice(string accountId)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            Invoice invoice = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

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
            }*/

            #endregion

            if (invoice == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    invoice = accountBillingServiceClient.GetUpcomingInvoice(
                        accountId, Common.SharedClientKey);//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser);

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = invoice;

            return jsonNetResult;
        }

        [Route("Invoices/Json/GetInvoiceHistory")]
        [HttpGet]
        public JsonNetResult GetInvoiceHistory(int itemLimit, string accountId)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Invoice> invoices = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

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
            }*/

            #endregion

            if (invoices == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    invoices = accountBillingServiceClient.GetInvoiceHistory(
                            itemLimit,
                            accountId, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }




                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = invoices;

            return jsonNetResult;
        }

        [Route("Invoices/Json/GetInvoiceHistory_Next")]
        [HttpGet]
        public JsonNetResult GetInvoiceHistory_Next(int itemLimit, string accountId, string startingAfter)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Invoice> invoices = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

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
            }*/

            #endregion

            if (invoices == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    invoices = accountBillingServiceClient.GetInvoiceHistory_Next(
                            itemLimit,
                            startingAfter,
                            accountId, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = invoices;

            return jsonNetResult;
        }


        [Route("Invoices/Json/GetInvoiceHistory_Last")]
        [HttpGet]
        public JsonNetResult GetInvoiceHistory_Last(int itemLimit, string accountId, string endingBefore)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Invoice> invoices = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

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
            }*/

            #endregion

            if (invoices == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    invoices = accountBillingServiceClient.GetInvoiceHistory_Last(
                            itemLimit,
                            endingBefore,
                            accountId, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = invoices;

            return jsonNetResult;
        }


        [Route("Invoices/Json/GetInvoiceHistory_ByDateRange")]
        [HttpGet]
        public JsonNetResult GetInvoiceHistory_ByDateRange(int itemLimit, string accountId, string startDate, string endDate)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Invoice> invoices = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

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
            }*/

            #endregion

            if (invoices == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    invoices = accountBillingServiceClient.GetInvoiceHistory_ByDateRange(
                            itemLimit,
                            Convert.ToDateTime(startDate).ToUniversalTime(), //<-- We must convert to UTC to get accurrate results
                            Convert.ToDateTime(endDate).ToUniversalTime().AddDays(1), //<-- We must convert to UTC to get accurrate results (We add 1 day to end dates to also get resuts from this day and not just up to midnight the night before)
                            accountId, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }




                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = invoices;

            return jsonNetResult;
        }


        [Route("Invoices/Json/GetInvoiceHistory_ByDateRange_Next")]
        [HttpGet]
        public JsonNetResult GetInvoiceHistory_ByDateRange_Next(int itemLimit, string accountId, string startingAfter, string startDate, string endDate)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Invoice> invoices = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

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
            }*/

            #endregion

            if (invoices == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    invoices = accountBillingServiceClient.GetInvoiceHistory_ByDateRange_Next(
                            itemLimit,
                            startingAfter,
                            Convert.ToDateTime(startDate).ToUniversalTime(), //<-- We must convert to UTC to get accurrate results 
                            Convert.ToDateTime(endDate).ToUniversalTime().AddDays(1), //<-- We must convert to UTC to get accurrate results (We add 1 day to end dates to also get resuts from this day and not just up to midnight the night before)
                            accountId, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = invoices;

            return jsonNetResult;
        }


        [Route("Invoices/Json/GetInvoiceHistory_ByDateRange_Last")]
        [HttpGet]
        public JsonNetResult GetInvoiceHistory_ByDateRange_Last(int itemLimit, string accountId, string endingBefore, string startDate, string endDate)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Invoice> invoices = null;

            #region (Plan A) Get data from Redis Cache
            /*
            try
            {
                IDatabase cache = CoreServiceSettings.RedisConnectionMultiplexers.AccountManager_Multiplexer.GetDatabase();

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
            }*/

            #endregion

            if (invoices == null)
            {
                #region (Plan B) Get data from WCF

                var accountBillingServiceClient = new AccountBillingService.AccountBillingServiceClient();

                try
                {
                    accountBillingServiceClient.Open();
                    invoices = accountBillingServiceClient.GetInvoiceHistory_ByDateRange_Last(
                            itemLimit,
                            endingBefore,
                            Convert.ToDateTime(startDate).ToUniversalTime(), //<-- We must convert to UTC to get accurrate results
                            Convert.ToDateTime(endDate).ToUniversalTime().AddDays(1), //<-- We must convert to UTC to get accurrate results (We add 1 day to end dates to also get resuts from this day and not just up to midnight the night before)
                            accountId, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(accountBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = invoices;

            return jsonNetResult;
        }


        #endregion
    }
}