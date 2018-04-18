using Newtonsoft.Json;
using PlatformAdminSite.PlatformBillingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformAdminSite.Controllers
{
    public class TransfersController : Controller
    {
        #region JSON Services

        [Route("Transfers/Json/GetTransfer")]
        [HttpGet]
        public JsonNetResult GetTransfer(string transferId)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            Transfer transfer = null;

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

            if (transfer == null)
            {
                #region (Plan B) Get data from WCF

                var platformBillingServiceClient = new PlatformBillingService.PlatformBillingServiceClient();

                try
                {
                    platformBillingServiceClient.Open();
                    transfer = platformBillingServiceClient.GetTransfer(
                        transferId, Common.SharedClientKey);//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser);

                    //Close the connection
                    WCFManager.CloseConnection(platformBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(platformBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = transfer;

            return jsonNetResult;
        }


        [Route("Transfers/Json/GetTransferHistory")]
        [HttpGet]
        public JsonNetResult GetTransferHistory(int itemLimit)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Transfer> transfers = null;

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

            if (transfers == null)
            {
                #region (Plan B) Get data from WCF

                var platformBillingServiceClient = new PlatformBillingService.PlatformBillingServiceClient();

                try
                {
                    platformBillingServiceClient.Open();
                    transfers = platformBillingServiceClient.GetTransferHistory(
                            itemLimit, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(platformBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(platformBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }




                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = transfers;

            return jsonNetResult;
        }

        [Route("Transfers/Json/GetTransferHistory_Next")]
        [HttpGet]
        public JsonNetResult GetTransferHistory_Next(int itemLimit, string startingAfter)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Transfer> transfers = null;

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

            if (transfers == null)
            {
                #region (Plan B) Get data from WCF

                var platformBillingServiceClient = new PlatformBillingService.PlatformBillingServiceClient();

                try
                {
                    platformBillingServiceClient.Open();
                    transfers = platformBillingServiceClient.GetTransferHistory_Next(
                            itemLimit,
                            startingAfter, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(platformBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(platformBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = transfers;

            return jsonNetResult;
        }


        [Route("Transfers/Json/GetTransferHistory_Last")]
        [HttpGet]
        public JsonNetResult GetTransferHistory_Last(int itemLimit, string endingBefore)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Transfer> transfers = null;

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

            if (transfers == null)
            {
                #region (Plan B) Get data from WCF

                var platformBillingServiceClient = new PlatformBillingService.PlatformBillingServiceClient();

                try
                {
                    platformBillingServiceClient.Open();
                    transfers = platformBillingServiceClient.GetTransferHistory_Last(
                            itemLimit,
                            endingBefore, Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(platformBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(platformBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = transfers;

            return jsonNetResult;
        }


        [Route("Transfers/Json/GetTransferHistory_ByDateRange")]
        [HttpGet]
        public JsonNetResult GetTransferHistory_ByDateRange(int itemLimit, string startDate, string endDate)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Transfer> transfers = null;

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

            if (transfers == null)
            {
                #region (Plan B) Get data from WCF

                var platformBillingServiceClient = new PlatformBillingService.PlatformBillingServiceClient();

                try
                {
                    platformBillingServiceClient.Open();
                    transfers = platformBillingServiceClient.GetTransferHistory_ByDateRange(
                            itemLimit,
                            Convert.ToDateTime(startDate).ToUniversalTime(), //<-- We must convert to UTC to get accurrate results
                            Convert.ToDateTime(endDate).ToUniversalTime().AddDays(1) //<-- We must convert to UTC to get accurrate results (We add 1 day to end dates to also get resuts from this day and not just up to midnight the night before)
                            , Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(platformBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(platformBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }




                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = transfers;

            return jsonNetResult;
        }


        [Route("Transfers/Json/GetTransferHistory_ByDateRange_Next")]
        [HttpGet]
        public JsonNetResult GetTransferHistory_ByDateRange_Next(int itemLimit, string startingAfter, string startDate, string endDate)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Transfer> transfers = null;

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

            if (transfers == null)
            {
                #region (Plan B) Get data from WCF

                var platformBillingServiceClient = new PlatformBillingService.PlatformBillingServiceClient();

                try
                {
                    platformBillingServiceClient.Open();
                    transfers = platformBillingServiceClient.GetTransferHistory_ByDateRange_Next(
                            itemLimit,
                            startingAfter,
                            Convert.ToDateTime(startDate).ToUniversalTime(), //<-- We must convert to UTC to get accurrate results 
                            Convert.ToDateTime(endDate).ToUniversalTime().AddDays(1) //<-- We must convert to UTC to get accurrate results (We add 1 day to end dates to also get resuts from this day and not just up to midnight the night before)
                            , Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(platformBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(platformBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = transfers;

            return jsonNetResult;
        }


        [Route("Transfers/Json/GetTransferHistory_ByDateRange_Last")]
        [HttpGet]
        public JsonNetResult GetTransferHistory_ByDateRange_Last(int itemLimit, string endingBefore, string startDate, string endDate)
        {
            var user = AuthenticationCookieManager.GetAuthenticationCookie();
            List<Transfer> transfers = null;

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

            if (transfers == null)
            {
                #region (Plan B) Get data from WCF

                var platformBillingServiceClient = new PlatformBillingService.PlatformBillingServiceClient();

                try
                {
                    platformBillingServiceClient.Open();
                    transfers = platformBillingServiceClient.GetTransferHistory_ByDateRange_Last(
                            itemLimit,
                            endingBefore,
                            Convert.ToDateTime(startDate).ToUniversalTime(), //<-- We must convert to UTC to get accurrate results
                            Convert.ToDateTime(endDate).ToUniversalTime().AddDays(1) //<-- We must convert to UTC to get accurrate results (We add 1 day to end dates to also get resuts from this day and not just up to midnight the night before)
                            , Common.SharedClientKey
                        ).ToList();//,
                    //user.Id,
                    //PlatformAdminSite.AccountManagementService.RequesterType.PlatformUser).ToList();

                    //Close the connection
                    WCFManager.CloseConnection(platformBillingServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(platformBillingServiceClient, exceptionMessage, currentMethodString);

                    #endregion
                }

                #endregion
            }

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = transfers;

            return jsonNetResult;
        }


        #endregion
    }
}