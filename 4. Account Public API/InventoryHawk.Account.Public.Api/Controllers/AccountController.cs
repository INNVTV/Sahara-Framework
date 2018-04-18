using InventoryHawk.Account.Public.Api.AccountManagementService;
using InventoryHawk.Account.Public.Api.Models.Json.Account;
using InventoryHawk.Account.Public.Api.Models.Json.Common;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    public class AccountController : Controller
    {
        [Route("account")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpGet]
        public JsonNetResult GetAccount()
        {
            AccountDetailsJson accountDetailsJson = null;

            ExecutionType executionType = ExecutionType.local;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Get the subdomain (if exists) for the api call
            string accountNameKey = Common.GetSubDomain(Request.Url);
            if (String.IsNullOrEmpty(accountNameKey))
            {
                return new JsonNetResult { Data = "Not found" }; //return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            string localCacheKey = accountNameKey + ":accountDetails";

            #region (Plan A) Get json from local cache

            try
            {
                accountDetailsJson = (AccountDetailsJson)HttpRuntime.Cache[localCacheKey];
            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for local cache call
            }

            #endregion

            if (accountDetailsJson == null)
            {
                #region (Plan B) Get Public json from second layer of Redis Cache

                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string pathAndQuery = Common.GetApiPathAndQuery(Request.Url);

                string hashApiKey = accountNameKey + ":apicache";
                string hashApiField = pathAndQuery;

                try
                {
                    var redisApiValue = cache.HashGet(hashApiKey, hashApiField);

                    if (redisApiValue.HasValue)
                    {
                        accountDetailsJson = JsonConvert.DeserializeObject<AccountDetailsJson>(redisApiValue);
                        executionType = ExecutionType.redis_secondary;
                    }
                }
                catch
                {

                }


                #endregion

                if (accountDetailsJson == null)
                {
                    #region (Plan C) Create Account Object, Get Settings & Images

                    var account = Common.GetAccountObject(accountNameKey);

                    accountDetailsJson = new AccountDetailsJson();
                    accountDetailsJson.account = new AccountJson();
                    accountDetailsJson.account.contactSettings = new ContactSettingsJson();
                    accountDetailsJson.account.contactSettings.contactDetails = new ContactDetailsJson();
                    accountDetailsJson.account.salesSettings = new SalesSettingsJson();
                    accountDetailsJson.account.accountName = account.AccountName;
                    accountDetailsJson.account.accountNameKey = account.AccountNameKey;


                    //accountDetailsJson.account.paymentPlan = account.PaymentPlan;
                    accountDetailsJson.account.paymentPlan = account.PaymentPlan.PaymentPlanName;
                    //accountDetailsJson.account.paymentPlanFrequency = account.PaymentPlan.;


                    #region  Get Account Settings Document

                    AccountSettingsDocumentModel settingsDocument = DataAccess.AccountSettings.GetAccountSettings(accountNameKey);

                    #endregion

                    #region Build Sales Settings

                    if(settingsDocument.SalesSettings != null)
                    {

                        accountDetailsJson.account.salesSettings.useSalesLeads = settingsDocument.SalesSettings.UseSalesLeads;

                        if(accountDetailsJson.account.salesSettings.useSalesLeads == true)
                        {
                            accountDetailsJson.account.salesSettings.buttonCopy = settingsDocument.SalesSettings.ButtonCopy;
                            accountDetailsJson.account.salesSettings.leadsDescription = settingsDocument.SalesSettings.DescriptionCopy;
                        }
                        

                    }

                    #endregion

                    //Build Theme Object (removed)
                    //accountDetailsJson.account.themeSettings = new ThemeSettingsJson();
                    //accountDetailsJson.account.themeSettings.name = settingsDocument.Theme;

                    //ToDo: Pull theme info from Redis or WCF
                    #region Get Themes

                    //List<ThemeModel> themes = null;

                    #region From Redis
                    /*
                    IDatabase platformCache = CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer.GetDatabase();
                    string themesHashMainKey = "themes";
                    string themesHashMainField = "list";

                    try
                    {
                        var themesRedis = platformCache.HashGet(themesHashMainKey, themesHashMainField);

                        if (themesRedis.HasValue)
                        {
                            themes = JsonConvert.DeserializeObject<List<ThemeModel>>(themesRedis);
                        }
                    }
                    catch
                    {

                    }
                    */
                    #endregion

                    #region From WCF
                    /*
                    if(themes == null)
                    {
                        var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                        try
                        {
                            accountManagementServiceClient.Open();

                            themes = accountManagementServiceClient.GetThemes().ToList();

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
                    }
                    */
                    #endregion

                    #endregion

                    #region Build Theme
                    /*
                    foreach(ThemeModel theme in themes)
                    {
                        if(theme.Name == accountDetailsJson.account.themeSettings.name)
                        {
                            accountDetailsJson.account.themeSettings.font = theme.Font;

                            accountDetailsJson.account.themeSettings.colors.background = theme.Colors.Background;
                            accountDetailsJson.account.themeSettings.colors.backgroundGradientBottom = theme.Colors.BackgroundGradientBottom;
                            accountDetailsJson.account.themeSettings.colors.backgroundGradientTop = theme.Colors.BackgroundGradianetTop;
                            accountDetailsJson.account.themeSettings.colors.foreground = theme.Colors.Foreground;
                            accountDetailsJson.account.themeSettings.colors.highlight = theme.Colors.Highlight;
                            accountDetailsJson.account.themeSettings.colors.overlay = theme.Colors.Overlay;
                            accountDetailsJson.account.themeSettings.colors.shadow = theme.Colors.Shadow;
                            accountDetailsJson.account.themeSettings.colors.trim = theme.Colors.Trim;

                        }
                    }
                    */
                    #endregion

                    accountDetailsJson.account.customDomain = settingsDocument.CustomDomain;


                    #region Build Sort Settings (removed)

                    /*

                    if (settingsDocument.SortSettings != null)
                    {
                        accountDetailsJson.account.sortSettings = new SortSettingsJson();

                        accountDetailsJson.account.sortSettings.truncatedList = new SortJson {
                            name = settingsDocument.SortSettings.TruncatedListing.Name,
                            value = settingsDocument.SortSettings.TruncatedListing.Value,
                        };

                        accountDetailsJson.account.sortSettings.mixedList = new SortJson
                        {
                            name = settingsDocument.SortSettings.MixedListing.Name,
                            value = settingsDocument.SortSettings.MixedListing.Value,
                        };

                        accountDetailsJson.account.sortSettings.fullList = new SortJson
                        {
                            name = settingsDocument.SortSettings.FullListing.Name,
                            value = settingsDocument.SortSettings.FullListing.Value,
                        };
                    }*/

                    #endregion

                    #region Build Contact Settings & Info

                    if (settingsDocument.ContactSettings != null)
                    {
                        accountDetailsJson.account.contactSettings.showPhoneNumber = settingsDocument.ContactSettings.ShowPhoneNumber;
                        accountDetailsJson.account.contactSettings.showAddress = settingsDocument.ContactSettings.ShowAddress;
                        accountDetailsJson.account.contactSettings.showEmail = settingsDocument.ContactSettings.ShowEmail;

                        if (settingsDocument.ContactSettings.ContactInfo != null)
                        {
                            accountDetailsJson.account.contactSettings.contactDetails.phoneNumber = settingsDocument.ContactSettings.ContactInfo.PhoneNumber;
                            accountDetailsJson.account.contactSettings.contactDetails.email = settingsDocument.ContactSettings.ContactInfo.Email;

                            accountDetailsJson.account.contactSettings.contactDetails.address1 = settingsDocument.ContactSettings.ContactInfo.Address1;
                            accountDetailsJson.account.contactSettings.contactDetails.address2 = settingsDocument.ContactSettings.ContactInfo.Address2;
                            accountDetailsJson.account.contactSettings.contactDetails.city = settingsDocument.ContactSettings.ContactInfo.City;

                            accountDetailsJson.account.contactSettings.contactDetails.state = settingsDocument.ContactSettings.ContactInfo.State;
                            accountDetailsJson.account.contactSettings.contactDetails.postalCode = settingsDocument.ContactSettings.ContactInfo.PostalCode;
                        }

                    }                  

                    #endregion

                    //Get Images:
                    accountDetailsJson.account.images = Dynamics.Images.BuildDynamicImagesListForJson(accountNameKey, "account", account.AccountID.ToString(), false);

                    #endregion

                    #region Cache into redisAPI layer

                    try
                    {
                        cache.HashSet(hashApiKey, hashApiField, JsonConvert.SerializeObject(accountDetailsJson), When.Always, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }

                    #endregion
                }

                #region Cache locally

                HttpRuntime.Cache.Insert(localCacheKey, accountDetailsJson, null, DateTime.Now.AddMinutes(Common.AccountSettingsCacheTimeInMinutes), TimeSpan.Zero);

                #endregion

            }


            


            //Add execution data
            stopWatch.Stop();
            accountDetailsJson.executionType = executionType.ToString();
            accountDetailsJson.executionTime = stopWatch.Elapsed.TotalMilliseconds + "ms";

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Newtonsoft.Json.Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = accountDetailsJson;

            return jsonNetResult;
        }
    }
}