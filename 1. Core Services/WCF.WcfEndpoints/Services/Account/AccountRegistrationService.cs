using Sahara.Core.Accounts.Registration.Models;
using Sahara.Core.Accounts.Registration;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Validation;
using Sahara.Core.Common.Validation.ResponseTypes;
using Sahara.Core.Accounts;
using Sahara.Core.Common.MessageQueues.PlatformPipeline;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Platform.Requests.Models;

namespace WCF.WcfEndpoints.Service.Account
{
    public class AccountRegistrationService : WCF.WcfEndpoints.Contracts.Account.IAccountRegistrationService
    {
        #region Account Registration Methods

        public DataAccessResponseType RegisterAccount(RegisterNewAccountModel registerNewAccountModel, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var result = AccountRegistrationManager.RegisterNewAccount(registerNewAccountModel);

            return result;
        }
        #endregion

        #region Validation Methods

        public ValidationResponseType ValidateAccountName(string AccountName)
        {
            return ValidationManager.IsValidAccountName(AccountName);
        }

        public ValidationResponseType ValidateEmail(string Email)
        {
            return ValidationManager.IsValidEmail(Email);
        }

        public ValidationResponseType ValidatePhoneNumber(string PhoneNumber)
        {
            return ValidationManager.IsValidPhoneNumber(PhoneNumber);
        }

        public ValidationResponseType ValidateFirstName(string FirstName)
        {
            return ValidationManager.IsValidFirstName(FirstName);
        }

        public ValidationResponseType ValidateLastName(string LastName)
        {
            return ValidationManager.IsValidLastName(LastName);
        }

        #endregion

        #region Account Provisioning

        public DataAccessResponseType ProvisionAccount(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null); //<-- Only Platform Super Admins can provision accounts

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            if (accountId == null)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "AccountID cannot be null" };
            }

            var response = new DataAccessResponseType();

            // 0. Check to see if the account has been previously verified:
            var account = AccountManager.GetAccount(accountId, false, AccountManager.AccountIdentificationType.AccountID);

            if (account.Provisioned)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This account has already been provisioned";

                return response;
            }
            if (account.StripeSubscriptionID == null || account.StripeCustomerID == null || account.PaymentPlan.MonthlyRate == 0)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This account has not been assigned a payment plan or a Stripe CustomerID";

                return response;
            }
            else
            {
                // 1. Send provisioning request for the Worker via the PlatformQueue
                PlatformQueuePipeline.SendMessage.ProvisionAccount(accountId);

                // 2. Set Active state to TRUE to indicate that PlatformAdmin has activated the account and is now PENDING provisioning
                AccountManager.UpdateAccountActiveState(account.AccountID.ToString(), true);

                // 3. Invalidated/Update the cache for this account
                AccountManager.UpdateAccountDetailCache(accountId);

                // 4. Log the activity
                PlatformLogManager.LogActivity(Sahara.Core.Logging.PlatformLogs.Types.CategoryType.Account,
                    Sahara.Core.Logging.PlatformLogs.Types.ActivityType.Account_Provisioning_Requested,
                    "Provisioning request sent for: '" + account.AccountName + "'",
                    "Provisioning request sent.", accountId, account.AccountName);

                // 4. Return results!
                response.SuccessMessage = "Account provisioning request sent!";
                response.isSuccess = true;

            }

            return response;
        }

        #endregion

        #region Helper Methods

        public string ConvertToAccountNameKey(string AccountName)
        {
            return Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(AccountName);
        }

        #endregion
    }
}
