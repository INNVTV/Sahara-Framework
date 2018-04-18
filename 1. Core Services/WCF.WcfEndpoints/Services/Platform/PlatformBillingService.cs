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
using WCF.WcfEndpoints.Contracts.Platform;
using Sahara.Core.Platform.Billing.Models;
using Sahara.Core.Platform.Billing;

namespace WCF.WcfEndpoints.Service.Platform
{
    public class PlatformBillingService : IPlatformBillingService
    {

        #region Get Transfers

        public Transfer GetTransfer(string transferId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetTransfer(transferId);
        }


        public List<Transfer> GetTransferHistory(int itemLimit, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetTransferHistory(itemLimit);
        }

        public List<Transfer> GetTransferHistory(int itemLimit, DateTime startDate, DateTime endDate, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetTransferHistory(itemLimit, startDate, endDate);
        }


        public List<Transfer> GetTransferHistory_Next(int itemLimit, string startingAfterTransferId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetTransferHistory_Next(itemLimit, startingAfterTransferId);
        }

        public List<Transfer> GetTransferHistory_Last(int itemLimit, string endingBeforeTransferId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetTransferHistory_Last(itemLimit, endingBeforeTransferId);
        }


        public List<Transfer> GetTransferHistory_ByDateRange_Next(int itemLimit, string startingAfterTransferId, DateTime startDate, DateTime endDate, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetTransferHistory_ByDateRange_Next(itemLimit, startingAfterTransferId, startDate, endDate);
        }

        public List<Transfer> GetTransferHistory_ByDateRange_Last(int itemLimit, string endingBeforeTransferId, DateTime startDate, DateTime endDate, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetTransferHistory_ByDateRange_Last(itemLimit, endingBeforeTransferId, startDate, endDate);
        }




        #endregion

        #region BalanceTransactions

        #region Get BalanceTransactions for Source

        public SourceBalanceTransactions GetBalanceTransactionsForSource(string sourceId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformBillingManager.GetBalanceTransactionsForSource(sourceId);
        }

        #endregion

        #endregion

    }
}
