using InventoryHawk.Account.Public.Api.AccountManagementService;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.DataAccess
{
    public static class AccountSettings
    {
        public static AccountSettingsDocumentModel GetAccountSettings(string accountNameKey)
        {

            AccountSettingsDocumentModel settingsDocument = null;

            if(settingsDocument == null)
            {
                #region From Redis

                try
                {
                    IDatabase cache = CoreServices.RedisConnectionMultiplexers.RedisMultiplexer.GetDatabase();

                    string hashMainKey = "account:settings";
                    string hashMainField = accountNameKey;

                    try
                    {
                        var redisValue = cache.HashGet(hashMainKey, hashMainField);

                        if (redisValue.HasValue)
                        {
                            settingsDocument = JsonConvert.DeserializeObject<AccountSettingsDocumentModel>(redisValue);
                        }
                    }
                    catch
                    {

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
            }

            return settingsDocument;
        }
    }
}