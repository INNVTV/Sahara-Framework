using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Capacity.Public;
using Sahara.Core.Application.Categorization.Models;
using Sahara.Core.Application.Categorization.Public;
using Sahara.Core.Application.Tags.Public;
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
    public class ApplicationTagsService : IApplicationTagsService
    {

        public DataAccessResponseType CreateTag(string accountId, string tagName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Validate Plan Capabilities

            //Verify that current tag count is below maximum allowed by this plan
            if (TagManager.GetTagCount(account.AccountNameKey) >= account.PaymentPlan.MaxTags)
            {
                //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
                Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("tags", account.AccountID.ToString(), account.AccountName);


                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for more than " + account.PaymentPlan.MaxTags + " tags, please update your plan to add more." };
            }

            #endregion

            var result = TagManager.CreateTag(account, tagName);

            #region Log Account Activity


            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_TagCreated,
                        "Tag '" + tagName + "' created",
                        requesterName + " created '" + tagName + "' tag",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessage);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

        public List<String> GetTags(string accountNameKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return TagManager.GetTags(accountNameKey);
        }

        public DataAccessResponseType DeleteTag(string accountId, string tagName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var result = TagManager.DeleteTag(account, tagName);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Inventory,
                        ActivityType.Inventory_TagDeleted,
                        "Tag '" + tagName + "' deleted",
                        requesterName + " deleted the '" + tagName + "' tag",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessage);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            #region Invalidate Account API Caching Layer

            Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

            #endregion

            return result;
        }

    }
}
