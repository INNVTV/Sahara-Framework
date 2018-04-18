using AccountAdminSite.AccountManagementService;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AccountAdminSite.Controllers
{
    public class AccountSettingsController : Controller
    {
        #region Notes

        /*
        
            We share a singe get/update internal methods to make it VERY EASY to alter the account settings document.
            
        */

        #endregion

        #region GET

        [Route("AccountSettings/Json/GetAccountSettings")]
        [HttpGet]
        public JsonNetResult GetAccountSettings()
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var accountSettngs = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = accountSettngs;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/GetThemes")]
        [HttpGet]
        public JsonNetResult GetThemes()
        {

            List<ThemeModel> themes = null;

            #region From Redis

            IDatabase platformCache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();
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

            #endregion

            #region From WCF

            if (themes == null)
            {
                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();

                    themes = accountManagementServiceClient.GetThemes(Common.SharedClientKey).ToList();

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

            #endregion

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = themes;

            return jsonNetResult;
        }

        #endregion

        #region UPDATE

        [Route("AccountSettings/Json/UpdateContactInfo")]
        [HttpPost]
        public JsonNetResult UpdateContactInfo(string phoneNumber, string email, string address1, string address2, string city, string state, string postalCode)
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            //Make updates:
            accountSettings.ContactSettings.ContactInfo = new ContactInfoModel();
            accountSettings.ContactSettings.ContactInfo.PhoneNumber = phoneNumber;
            accountSettings.ContactSettings.ContactInfo.Email = email;
            accountSettings.ContactSettings.ContactInfo.Address1 = address1;
            accountSettings.ContactSettings.ContactInfo.Address2 = address2;
            accountSettings.ContactSettings.ContactInfo.City = city;
            accountSettings.ContactSettings.ContactInfo.State = state;
            accountSettings.ContactSettings.ContactInfo.PostalCode = postalCode;

            var response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/UpdateTheme")]
        [HttpPost]
        public JsonNetResult UpdateTheme(string themeName)
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var account = Common.GetAccountObject(accountNameKey);

            var response = new DataAccessResponseType();

            if (account.PaymentPlan.AllowThemes == false)
            {
                response = new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for additional or custom themes." };
            }
            else
            {
                var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

                //Make updates:
                accountSettings.Theme = themeName;

                response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);
            }


            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/UpdateShowPhoneNumber")]
        [HttpPost]
        public JsonNetResult UpdateShowPhoneNumber(bool showPhoneNumber)
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            //Make updates:
            accountSettings.ContactSettings.ShowPhoneNumber = showPhoneNumber;

            var response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/UpdateShowEmail")]
        [HttpPost]
        public JsonNetResult UpdateShowEmail(bool showEmail)
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            //Make updates:
            accountSettings.ContactSettings.ShowEmail = showEmail;

            var response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/UpdateShowAddress")]
        [HttpPost]
        public JsonNetResult UpdateShowAddress(bool showAddress)
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            //Make updates:
            accountSettings.ContactSettings.ShowAddress = showAddress;

            var response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/UpdateUseSalesLeads")]
        [HttpPost]
        public JsonNetResult UpdateUseSalesLeads(bool useSalesLeads)
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            #region Make sure account plan allow for sales leads

            var account = Common.GetAccountObject(accountNameKey);

            if (useSalesLeads == true && account.PaymentPlan.AllowSalesLeads == false)
            {
                JsonNetResult jsonNetResultRestricted = new JsonNetResult();
                jsonNetResultRestricted.Formatting = Formatting.Indented;
                jsonNetResultRestricted.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultRestricted.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = "Account plan does not allow for sales leads" };

                return jsonNetResultRestricted;
            }

            #endregion

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            //Make updates:
            accountSettings.SalesSettings.UseSalesLeads = useSalesLeads;
            if(!useSalesLeads)
            {
                accountSettings.SalesSettings.UseSalesAlerts = useSalesLeads;
            }

            var response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/UpdateUseSalesAlerts")]
        [HttpPost]
        public JsonNetResult UpdateUseSalesAlerts(bool useSalesAlerts)
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);


            #region Make sure account plan allow for sales leads

            var account = Common.GetAccountObject(accountNameKey);

            if (useSalesAlerts == true && account.PaymentPlan.AllowSalesLeads == false)
            {
                JsonNetResult jsonNetResultRestricted = new JsonNetResult();
                jsonNetResultRestricted.Formatting = Formatting.Indented;
                jsonNetResultRestricted.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultRestricted.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = "Account plan does not allow for sales leads" };

                return jsonNetResultRestricted;
            }

            #endregion

            //Make updates:
            accountSettings.SalesSettings.UseSalesAlerts = useSalesAlerts;

            var response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/AddSalesAlertEmail")]
        [HttpPost]
        public JsonNetResult AddSalesAlertEmail(string email)
        {
            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            #region Make sure account plan allow for sales leads

            var account = Common.GetAccountObject(accountNameKey);

            if (account.PaymentPlan.AllowSalesLeads == false)
            {
                JsonNetResult jsonNetResultRestricted = new JsonNetResult();
                jsonNetResultRestricted.Formatting = Formatting.Indented;
                jsonNetResultRestricted.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultRestricted.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = "Account plan does not allow for sales leads" };

                return jsonNetResultRestricted;
            }

            #endregion

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            if(!email.Contains('@') || !email.Contains('.'))
            {
                JsonNetResult jsonNetResultError = new JsonNetResult();
                jsonNetResultError.Formatting = Formatting.Indented;
                jsonNetResultError.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultError.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage= "Please use a valid email address" };

                return jsonNetResultError;
            }

            //Make updates:
            if(accountSettings.SalesSettings.AlertEmails == null)
            {
                accountSettings.SalesSettings.AlertEmails = new List<string>();
            }

            if (accountSettings.SalesSettings.AlertEmails.Count >= 15)
            {
                JsonNetResult jsonNetResultError2 = new JsonNetResult();
                jsonNetResultError2.Formatting = Formatting.Indented;
                jsonNetResultError2.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultError2.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = "You've reached the limit of 15 alert emails" };

                return jsonNetResultError2;
            }

            foreach(string emailTest in accountSettings.SalesSettings.AlertEmails)
            {
                if(emailTest == email)
                {
                    JsonNetResult jsonNetResultError3 = new JsonNetResult();
                    jsonNetResultError3.Formatting = Formatting.Indented;
                    jsonNetResultError3.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                    jsonNetResultError3.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = "This email is already on the alert list" };

                    return jsonNetResultError3;
                }
            }


            accountSettings.SalesSettings.AlertEmails.Add(email);

            var response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/RemoveSalesAlertEmail")]
        [HttpPost]
        public JsonNetResult RemoveSalesAlertEmail(int index)
        {
            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            //Make updates:
            accountSettings.SalesSettings.AlertEmails.RemoveAt(index);

            var response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/UpdateSalesLeadsButtonCopy")]
        [HttpPost]
        public JsonNetResult UpdateSalesLeadsButtonCopy(string buttonCopy)
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var account = Common.GetAccountObject(accountNameKey);

            #region Make sure account plan allow for sales leads

            if (account.PaymentPlan.AllowSalesLeads == false)
            {
                JsonNetResult jsonNetResultRestricted = new JsonNetResult();
                jsonNetResultRestricted.Formatting = Formatting.Indented;
                jsonNetResultRestricted.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultRestricted.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = "Account plan does not allow for sales leads" };

                return jsonNetResultRestricted;
            }

            #endregion

            var response = new DataAccessResponseType();
          
            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            //Make updates:
            accountSettings.SalesSettings.ButtonCopy = buttonCopy;

            response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);
            
            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }

        [Route("AccountSettings/Json/UpdateSalesLeadsDescriptionCopy")]
        [HttpPost]
        public JsonNetResult UpdateSalesLeadsDescriptionCopy(string descriptionCopy)
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var account = Common.GetAccountObject(accountNameKey);

            #region Make sure account plan allow for sales leads

            if (account.PaymentPlan.AllowSalesLeads == false)
            {
                JsonNetResult jsonNetResultRestricted = new JsonNetResult();
                jsonNetResultRestricted.Formatting = Formatting.Indented;
                jsonNetResultRestricted.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
                jsonNetResultRestricted.Data = new DataAccessResponseType { isSuccess = false, ErrorMessage = "Account plan does not allow for sales leads" };

                return jsonNetResultRestricted;
            }

            #endregion

            var response = new DataAccessResponseType();

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            //Make updates:
            accountSettings.SalesSettings.DescriptionCopy = descriptionCopy;

            response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }


        [Route("AccountSettings/Json/UpdateCustomDomain")]
        [HttpPost]
        public JsonNetResult UpdateCustomDomain(string customDomain)
        {

            var accountNameKey = AuthenticationCookieManager.GetAuthenticationCookie().AccountNameKey;

            var account = Common.GetAccountObject(accountNameKey);

            var response = new DataAccessResponseType();

            var accountSettings = AccountSettings.GetAccountSettings_Internal(accountNameKey);

            //Make updates:
            accountSettings.CustomDomain = customDomain;

            response = AccountSettings.UpdateAccountSettings_Internal(accountNameKey, accountSettings);

            JsonNetResult jsonNetResult = new JsonNetResult();
            jsonNetResult.Formatting = Formatting.Indented;
            jsonNetResult.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local; //<-- Convert UTC times to LocalTime
            jsonNetResult.Data = response;

            return jsonNetResult;
        }
        #endregion

    }

    //Shared and used by portions of the admin tool as well as settings page
    public static class AccountSettings
    {
        #region Shared/Common Account Settings Methods

        public static AccountSettingsDocumentModel GetAccountSettings_Internal(string accountNameKey)
        {

            AccountSettingsDocumentModel settingsDocument = null;

            #region From Redis

            try
            {
                IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                string hashMainKey = "account:settings";
                string hashMainField = accountNameKey;

                var redisValue = cache.HashGet(hashMainKey, hashMainField);

                if (redisValue.HasValue)
                {
                    settingsDocument = JsonConvert.DeserializeObject<AccountSettingsDocumentModel>(redisValue);
                }

            }
            catch (Exception e)
            {
                var error = e.Message;
                //TODO: Log: error message for Redis call
            }

            #endregion

            if (settingsDocument == null)
            {
                #region From WCF

                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                try
                {
                    accountManagementServiceClient.Open();

                    settingsDocument = accountManagementServiceClient.GetAccountSettings(accountNameKey, Common.SharedClientKey);
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

            #region Instantiate instances of classes that are null

            if (settingsDocument.ContactSettings == null)
            {
                settingsDocument.ContactSettings = new ContactSettingsModel();
            }
            if (settingsDocument.ContactSettings.ContactInfo == null)
            {
                settingsDocument.ContactSettings.ContactInfo = new ContactInfoModel();
            }
            if (settingsDocument.SalesSettings == null)
            {
                settingsDocument.SalesSettings = new SalesSettingsModel();
            }

            #endregion

            return settingsDocument;
        }

        public static DataAccessResponseType UpdateAccountSettings_Internal(string accountNameKey, AccountSettingsDocumentModel accountSettingsDocumentModel)
        {
            var response = new AccountManagementService.DataAccessResponseType();

            var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

            try
            {
                accountManagementServiceClient.Open();
                response = accountManagementServiceClient.UpdateAccountSettings(accountNameKey, accountSettingsDocumentModel,
                AuthenticationCookieManager.GetAuthenticationCookie().Id,
                AccountAdminSite.AccountManagementService.RequesterType.AccountUser, Common.SharedClientKey);

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

            return response;
        }

        #endregion
    }

}