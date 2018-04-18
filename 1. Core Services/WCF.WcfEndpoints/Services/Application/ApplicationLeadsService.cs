using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Capacity.Public;
using Sahara.Core.Application.Leads;
using Sahara.Core.Application.Leads.Models;
using Sahara.Core.Application.Properties;
using Sahara.Core.Application.Properties.Models;
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
    public class ApplicationLeadsService : IApplicationLeadsService
    {

        #region Settings: Labels

        public DataAccessResponseType CreateLabel(string accountNameKey, string labelName, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var result = LeadsManager.CreateLabel(account, labelName);

            #region Log Account Activity

            /*
            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyCreated,
                        "Property '" + propertyName + "' created",
                        requesterName + " created '" + propertyName + "' property",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessage);
                }
                catch { }
            }
            */
            #endregion


            return result;

        }

        public DataAccessResponseType RemoveLabel(string accountNameKey, string labelName, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var result = LeadsManager.RemoveLabel(account, labelName);

            #region Log Account Activity

            /*
            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId,
                        CategoryType.Inventory,
                        ActivityType.Inventory_PropertyCreated,
                        "Property '" + propertyName + "' created",
                        requesterName + " created '" + propertyName + "' property",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessage);
                }
                catch { }
            }
            */
            #endregion


            return result;

        }

        #endregion


        #region Get Sales Leads

        /* Account Admin now deals with SALES LEADS DIRECTLY

        public List<SalesLead> GetLeads(string accountId, string label, int skip, int take)
        {
            return LeadsManager.GetSalesLeads(accountId, label, skip, take);
        }
        
        */
        

        #endregion

    }
}
