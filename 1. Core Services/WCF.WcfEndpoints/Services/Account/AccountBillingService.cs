using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts;
using Sahara.Core.Common.Services.Stripe;
using WCF.WcfEndpoints.Contracts.Account;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Platform.Requests.Models;
using Sahara.Core.Platform.Billing.Models;
using Sahara.Core.Platform.Billing;

namespace WCF.WcfEndpoints.Service.Account
{
    public class AccountBillingService : IAccountBillingService
    {
        #region Get Payments/Charges

        public Charge GetPayment(string chargeId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetPayment(chargeId);
        }

        public List<Charge> GetPaymentHistory(int itemLimit, string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetPaymentHistory(itemLimit, accountId);
        }

        public List<Charge> GetPaymentHistory_Next(int itemLimit, string startingAfterChargeId, string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetPaymentHistory_Next(itemLimit, startingAfterChargeId, accountId);
        }

        public List<Charge> GetPaymentHistory_Last(int itemLimit, string endingBeforeChargeId, string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetPaymentHistory_Last(itemLimit, endingBeforeChargeId, accountId);
        }

        #endregion

        #region Refund Payment/Charge

        public DataAccessResponseType RefundPayment(string accountId, string chargeId, decimal refundAmount, string requesterID, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            if(String.IsNullOrEmpty(accountId))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Must include an accountId" };
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterID,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin, //<-- Only Platform SuperAdmins can refund payments
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            return PlatformBillingManager.RefundPayment(accountId, chargeId, refundAmount);

        }

        #endregion

        #region Get Invoices

        public Invoice GetInvoice(string invoiceId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetInvoice(invoiceId);
        }

        public List<Invoice> GetInvoiceHistory(int itemLimit, string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetInvoiceHistory(itemLimit, accountId);
        }

        public List<Invoice> GetInvoiceHistory(int itemLimit, DateTime startDate, DateTime endDate, string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetInvoiceHistory(itemLimit, startDate, endDate, accountId);
        }


        public List<Invoice> GetInvoiceHistory_Next(int itemLimit, string startingAfterInvoiceId, string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetInvoiceHistory_Next(itemLimit, startingAfterInvoiceId, accountId);
        }

        public List<Invoice> GetInvoiceHistory_Last(int itemLimit, string endingBeforeInvoiceId, string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetInvoiceHistory_Last(itemLimit, endingBeforeInvoiceId, accountId);
        }


        public List<Invoice> GetInvoiceHistory_ByDateRange_Next(int itemLimit, string startingAfterInvoiceId, DateTime startDate, DateTime endDate, string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetInvoiceHistory_ByDateRange_Next(itemLimit, startingAfterInvoiceId, startDate, endDate, accountId);
        }

        public List<Invoice> GetInvoiceHistory_ByDateRange_Last(int itemLimit, string endingBeforeInvoiceId, DateTime startDate, DateTime endDate, string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetInvoiceHistory_ByDateRange_Last(itemLimit, endingBeforeInvoiceId, startDate, endDate, accountId);
        }


        public Invoice GetUpcomingInvoice(string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetUpcomingInvoice(accountId);
        }

        #endregion

        #region Dunning


        public int GetDunningAttemptsCount(string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return -1;
            }

            var account = AccountManager.GetAccount(accountId);
            return PlatformBillingManager.GetAutomaticDunningAttemptCount(accountId, account.StoragePartition);
        }


        public List<DunningAttempt> GetDunningAttempts(string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);
            return PlatformBillingManager.GetAutomaticDunningAttempts(accountId, account.StoragePartition);
        }


        #endregion


    }
}
