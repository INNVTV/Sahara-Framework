using Sahara.Core.Accounts.Commerce.Public;
using Sahara.Core.Logging.AccountLogs;
using Sahara.Core.Logging.AccountLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Platform.Requests.Models;
using WCF.WcfEndpoints.Contracts.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts;

namespace WCF.WcfEndpoints.Service.Account
{
    public class AccountCommerceService : IAccountCommerceService
    {
        public int GetDollarsToCreditsExchangeRate(decimal dollarAmount, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return -1;
            }

            return Sahara.Core.Common.Methods.Commerce.ConvertDollarAmountToCredits(dollarAmount);
        }

        public int GetCredits(string accountID, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return -1;
            }

            return AccountCreditsManager.GetCredits(accountID);
        }

        public DataAccessResponseType BuyCredits(string accountId, decimal dollarAmount, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            //Only Platform SuperAdmins and Account Admins can buy credits       
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin); 

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            var result = AccountCreditsManager.BuyCredits(accountId, dollarAmount);

            #region Log Account Activity

            if (result.isSuccess)
            {
                /*try{

                    
                    var creditsAmount = Sahara.Core.Common.Methods.Commerce.ConvertDollarAmountToCredits(dollarAmount);

                    AccountLogManager.LogActivity(
                        accountId,
                        CategoryType.Credits,
                        ActivityType.Credits_Purchased,
                        creditsAmount + " credits purchased",
                        requesterName + " purchased " + creditsAmount + " credits for $" + dollarAmount,
                        requesterId,
                        requesterName,
                        requesterEmail,
                        ipAddress,
                        origin);
                }catch{}*/
            }

            #endregion

            return result;

        }

        public DataAccessResponseType SpendCredits(string accountId, int creditAmount, string description, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            //Only Platform Admins and Account Users and up can spend credits (a little more lax than purchasing)    
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.User);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            var result = AccountCreditsManager.SpendCredits(accountId, creditAmount, description);

            #region Log Account Activity

            if (result.isSuccess)
            {
                /*try
                {
                    AccountLogManager.LogActivity(
                        accountId,
                        CategoryType.Credits,
                        ActivityType.Credits_Spent,
                        creditAmount + " credits spent",
                        requesterName + " spent " + creditAmount + " credits on '" + description + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        ipAddress,
                        origin);
                }
                catch { }*/
            }

            #endregion

            return result;
        }

        public DataAccessResponseType TradeCredits(string fromAccountId, string toAccountId, int creditAmount, string description, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            //Only Platform Admins and Account Users and up can spend credits (a little more lax than purchasing)    
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.User);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            Sahara.Core.Accounts.Models.Account receiverAccount;

            var result = AccountCreditsManager.TradeCredits(fromAccountId, toAccountId, creditAmount, description, out receiverAccount);

            #region Log Account Activity

            if (result.isSuccess)
            {
                /*try
                {
                    //Log the activity for both giver and receiver

                    //Trader
                    AccountLogManager.LogActivity(
                        fromAccountId,
                        CategoryType.Credits,
                        ActivityType.Credits_Traded,
                        creditAmount + " credits traded",
                        requesterName + " traded " + creditAmount + " credits to '" + receiverAccount.AccountName + "' for '" + description + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        ipAddress,
                        origin);

                    var fromAccountName = "Another account";

                    try
                    {
                        fromAccountName = AccountManager.GetAccountName(fromAccountId);
                    }
                    catch(Exception e)
                    {

                    }*/

                    //Tradee
                    /*
                    AccountLogManager.LogActivity(
                        toAccountId,
                        CategoryType.Credits,
                        ActivityType.Credits_Received,
                        creditAmount + " credits received",
                        requesterName + " from '" + fromAccountName + "' sent you " + creditAmount + " credits for '" + description + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        ipAddress,
                        origin);
                }
                catch { }*/
            }

            #endregion

            return result;
        }

    }
}
