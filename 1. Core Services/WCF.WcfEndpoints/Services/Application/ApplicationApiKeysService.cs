using Sahara.Core.Accounts;
using Sahara.Core.Application.ApiKeys;
using Sahara.Core.Application.ApiKeys.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.AccountLogs;
using Sahara.Core.Logging.AccountLogs.Types;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Platform.Requests.Models;
using WCF.WcfEndpoints.Contracts.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Service.Application
{
    public class ApplicationApiKeysService : IApplicationApiKeysService
    {

        public List<ApiKeyModel> GetApiKeys(string accountNameKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT 
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            return ApiKeysManager.GetApiKeys(account);
        }

        public string GenerateApiKey(string accountNameKey, string name, string description, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT 
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                //return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
                return null;
            }

            #endregion

            var result = ApiKeysManager.GenerateApiKey(account, name, description);

            #region Log Account Activity


            if (!string.IsNullOrEmpty(result))
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        account.AccountID.ToString(), account.StoragePartition,
                        CategoryType.ApiKeys,
                        ActivityType.ApiKeys_KeyGenerated,
                        "API Key '" + name + "' generate",
                        requesterName + " generated api key '" + result + "'",
                        requesterId,
                        requesterName,
                        requesterEmail);
                }
                catch { }
            }

            #endregion

            return result;
        }

        public string RegenenerateApiKey(string accountNameKey, string apiKey, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT 
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                //return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
                return null;
            }

            #endregion

            var newKey = ApiKeysManager.RegenerateApiKey(account, apiKey);

            #region Log Account Activity


            if (!string.IsNullOrEmpty(newKey))
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        account.AccountID.ToString(), account.StoragePartition,
                        CategoryType.ApiKeys,
                        ActivityType.ApiKeys_KeyGenerated,
                        "API Key '" + apiKey + "' regenerated. New key: '" + newKey + "'",
                        requesterName + " regenerated previous api key to '" + newKey + "'",
                        requesterId,
                        requesterName,
                        requesterEmail);
                }
                catch { }
            }

            #endregion


            return newKey;
        }

        public DataAccessResponseType DeleteApiKey(string accountNameKey, string apiKey, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT 
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                //return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
                return null;
            }

            #endregion

            var result = ApiKeysManager.DeleteApiKey(account, apiKey);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        account.AccountID.ToString(), account.StoragePartition,
                        CategoryType.ApiKeys,
                        ActivityType.ApiKeys_KeyGenerated,
                        "API Key '" + apiKey + "' deleted.",
                        requesterName + " deleted api key '" + apiKey + "'",
                        requesterId,
                        requesterName,
                        requesterEmail);
                }
                catch { }
            }

            #endregion


            return result;
        }
    }
}
