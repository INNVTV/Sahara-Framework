using Sahara.Core.Accounts;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Common.Services.Stripe;
using Sahara.Core.Platform.Billing.Internal;
using Sahara.Core.Platform.Billing.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Billing
{
    public static class PlatformBillingManager
    {
        #region Get Payments/Charges

        public static Charge GetPayment(string chargeId)
        {
            var charge = new Charge();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();

            //string stripeCustomerId = null;


            var stripeCharge = stripeManager.GetCharge(chargeId);

            if (stripeCharge != null)
            {
                return Transformations.TransformStripeChargeToCharge(stripeCharge, false);
            }


            #endregion

            return charge;
        }

        public static List<Charge> GetPaymentHistory(int itemLimit, string accountId = null)
        {
            var charges = new List<Charge>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();

            string stripeCustomerId = null;

            bool addAccountDetails = false;

            if (!String.IsNullOrEmpty(accountId))
            {
                stripeCustomerId = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;
            }
            else
            {
                addAccountDetails = true;
            }

            var stripeCharges = stripeManager.GetCharges(itemLimit, stripeCustomerId);

            if (stripeCharges != null)
            {
                foreach (var stripeCharge in stripeCharges)
                {
                    charges.Add(Transformations.TransformStripeChargeToCharge(stripeCharge, addAccountDetails));
                }
            }



            #endregion

            return charges;
        }

        public static List<Charge> GetPaymentHistory_Next(int itemLimit, string startingAfterChargeId, string accountId = null)
        {
            var charges = new List<Charge>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis CacheInvoice

            var stripeManager = new StripeManager();
            string stripeCustomerId = null;

            bool addAccountDetails = false;

            if (!String.IsNullOrEmpty(accountId))
            {
                stripeCustomerId = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;
            }
            else
            {
                addAccountDetails = true;
            }

            var stripeCharges = stripeManager.GetCharges_Next(itemLimit, startingAfterChargeId, stripeCustomerId);

            if (stripeCharges != null)
            {
                foreach (var stripeCharge in stripeCharges)
                {
                    charges.Add(Transformations.TransformStripeChargeToCharge(stripeCharge, addAccountDetails));
                }
            }



            #endregion

            return charges;
        }

        public static List<Charge> GetPaymentHistory_Last(int itemLimit, string endingBeforeChargeId, string accountId = null)
        {
            var charges = new List<Charge>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();
            string stripeCustomerId = null;

            bool addAccountDetails = false;

            if (!String.IsNullOrEmpty(accountId))
            {
                stripeCustomerId = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;
            }
            else
            {
                addAccountDetails = true;
            }

            var stripeCharges = stripeManager.GetCharges_Last(itemLimit, endingBeforeChargeId, stripeCustomerId);

            if (stripeCharges != null)
            {
                foreach (var stripeCharge in stripeCharges)
                {
                    charges.Add(Transformations.TransformStripeChargeToCharge(stripeCharge, addAccountDetails));
                }
            }



            #endregion

            return charges;
        }

        #endregion

        #region Refund Payment/Charge

        public static DataAccessResponseType RefundPayment(string accountId, string chargeId, decimal refundAmount)
        {


            var account = AccountManager.GetAccount(accountId);

            if(account == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Can only makre refunds for valid accounts." };
            }


            //Confirm that refund amount is greater than 0
            if (refundAmount <= 0)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Refund amount must be greater than 0." };
            }

            //Confirm that the refund amount is correctly formated
            if (Sahara.Core.Common.Methods.Billing.IsDecimalInTwoPlaces(refundAmount))
            {
                //Submit the refund
                var stripeManager = new StripeManager();

                var refundAmountStripeInteger = Sahara.Core.Common.Methods.Billing.ConvertDecimalToStripeAmount(refundAmount);

                StripeRefund refundedCharge;
                var refundResponse = stripeManager.RefundPayment(chargeId, refundAmountStripeInteger, out refundedCharge);

                if (refundResponse.isSuccess)
                {
                   
                    //Email all acount owners regarding the refund.

                    //Get account ownerEmail(s):
                    var ownerEmails = AccountManager.GetAccountOwnerEmails(account.AccountID.ToString());

                    string refundAmountDollars = string.Empty;

                    if (refundAmount.ToString().Substring(0, 1) == ".")
                    {
                        refundAmountDollars = refundAmount + " cents.";
                    }
                    else
                    {
                        refundAmountDollars = "$" + refundAmount;
                    }

                    EmailManager.Send(
                            ownerEmails,
                            Settings.Endpoints.Emails.FromRefunds,
                            Settings.Copy.EmailMessages.ChargeRefunded.FromName,
                            Settings.Copy.EmailMessages.ChargeRefunded.Subject,
                            String.Format(Settings.Copy.EmailMessages.ChargeRefunded.Body, account.AccountName, refundAmountDollars),
                            true);
                }

                return refundResponse;
            }
            else
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Refund amount must be a monetary value with 2 decimal places. Examples: '2.25' or '.50'" };
            }


        }

        #endregion

        #region Get Invoices

        public static Invoice GetInvoice(string stripeInvoiceId)
        {
            var invoice = new Invoice();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            if (!String.IsNullOrEmpty(stripeInvoiceId))
            {
                var stripeManager = new StripeManager();
                var stripeInvoice = stripeManager.GetInvoice(stripeInvoiceId);
                invoice = Transformations.TransformStripeInvoiceToInvoice(stripeInvoice);
            }

            #endregion

            return invoice;
        }

        public static Invoice GetUpcomingInvoice(string accountId)
        {
            var invoice = new Invoice();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();
            string stripeCustomerId = null;

            bool addAccountDetails = false;

            if (!String.IsNullOrEmpty(accountId))
            {
                stripeCustomerId = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;
            }
            else
            {
                addAccountDetails = true;
            }

            //var stripeCustomerId = AccountManager.GetAccount(accountId, false, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;

            if (!String.IsNullOrEmpty(stripeCustomerId))
            {
                var stripeInvoice = stripeManager.GetUpcomingInvoice(stripeCustomerId);

                if (stripeInvoice != null)
                {
                    invoice = Transformations.TransformStripeInvoiceToInvoice(stripeInvoice, addAccountDetails);
                }

            }

            #endregion

            return invoice;
        }

        public static List<Invoice> GetInvoiceHistory(int itemLimit, string accountId = null)
        {
            var invoices = new List<Invoice>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();

            string stripeCustomerId = null;

            bool addAccountDetails = false;

            if (!String.IsNullOrEmpty(accountId))
            {
                stripeCustomerId = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;
            }
            else
            {
                addAccountDetails = true;
            }

            var stripeInvoices = stripeManager.GetInvoices(itemLimit, stripeCustomerId);

            if (stripeInvoices != null)
            {
                foreach (var stripeInvoice in stripeInvoices)
                {
                    invoices.Add(Transformations.TransformStripeInvoiceToInvoice(stripeInvoice, addAccountDetails));
                }
            }



            #endregion

            return invoices;
        }

        public static List<Invoice> GetInvoiceHistory(int itemLimit, DateTime startDate, DateTime endDate, string accountId = null)
        {
            var invoices = new List<Invoice>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();

            string stripeCustomerId = null;

            bool addAccountDetails = false;

            if (!String.IsNullOrEmpty(accountId))
            {
                stripeCustomerId = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;
            }
            else
            {
                addAccountDetails = true;
            }


            var stripeInvoices = stripeManager.GetInvoices(itemLimit, startDate, endDate, stripeCustomerId);

            if (stripeInvoices != null)
            {
                foreach (var stripeInvoice in stripeInvoices)
                {
                    invoices.Add(Transformations.TransformStripeInvoiceToInvoice(stripeInvoice, addAccountDetails));
                }
            }


            #endregion

            return invoices;
        }

        public static List<Invoice> GetInvoiceHistory_Next(int itemLimit, string startingAfterInvoiceId, string accountId = null)
        {
            var invoices = new List<Invoice>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();
            string stripeCustomerId = null;

            bool addAccountDetails = false;

            if (!String.IsNullOrEmpty(accountId))
            {
                stripeCustomerId = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;
            }
            else
            {
                addAccountDetails = true;
            }

            var stripeInvoices = stripeManager.GetInvoices_Next(itemLimit, startingAfterInvoiceId, stripeCustomerId);

            if (stripeInvoices != null)
            {
                foreach (var stripeInvoice in stripeInvoices)
                {
                    invoices.Add(Transformations.TransformStripeInvoiceToInvoice(stripeInvoice, addAccountDetails));
                }
            }



            #endregion

            return invoices;
        }

        public static List<Invoice> GetInvoiceHistory_Last(int itemLimit, string endingBeforeInvoiceId, string accountId = null)
        {
            var invoices = new List<Invoice>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();
            string stripeCustomerId = null;

            bool addAccountDetails = false;

            if (!String.IsNullOrEmpty(accountId))
            {
                stripeCustomerId = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;
            }
            else
            {
                addAccountDetails = true;
            }

            var stripeInvoices = stripeManager.GetInvoices_Last(itemLimit, endingBeforeInvoiceId, stripeCustomerId);

            if (stripeInvoices != null)
            {
                foreach (var stripeInvoice in stripeInvoices)
                {
                    invoices.Add(Transformations.TransformStripeInvoiceToInvoice(stripeInvoice, addAccountDetails));
                }
            }



            #endregion

            return invoices;
        }


        public static List<Invoice> GetInvoiceHistory_ByDateRange_Next(int itemLimit, string startingAfterInvoiceId, DateTime startDate, DateTime endDate, string accountId = null)
        {
            var invoices = new List<Invoice>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();
            string stripeCustomerId = null;

            bool addAccountDetails = false;

            if (!String.IsNullOrEmpty(accountId))
            {
                stripeCustomerId = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;
            }
            else
            {
                addAccountDetails = true;
            }

            var stripeInvoices = stripeManager.GetInvoices_ByDateRange_Next(itemLimit, startingAfterInvoiceId, startDate, endDate, stripeCustomerId);

            if (stripeInvoices != null)
            {
                foreach (var stripeInvoice in stripeInvoices)
                {
                    invoices.Add(Transformations.TransformStripeInvoiceToInvoice(stripeInvoice, addAccountDetails));
                }
            }



            #endregion

            return invoices;
        }

        public static List<Invoice> GetInvoiceHistory_ByDateRange_Last(int itemLimit, string endingBeforeInvoiceId, DateTime startDate, DateTime endDate, string accountId = null)
        {
            var invoices = new List<Invoice>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();
            string stripeCustomerId = null;

            bool addAccountDetails = false;

            if (!String.IsNullOrEmpty(accountId))
            {
                stripeCustomerId = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID).StripeCustomerID;
            }
            else
            {
                addAccountDetails = true;
            }

            var stripeInvoices = stripeManager.GetInvoices_ByDateRange_Last(itemLimit, endingBeforeInvoiceId, startDate, endDate, stripeCustomerId);

            if (stripeInvoices != null)
            {
                foreach (var stripeInvoice in stripeInvoices)
                {
                    invoices.Add(Transformations.TransformStripeInvoiceToInvoice(stripeInvoice, addAccountDetails));
                }
            }

            #endregion

            return invoices;
        }



        #endregion

        #region Dunning (For Automated/Subscription Payments)

        public static int GetAutomaticDunningAttemptCount(string accountId, string storagePartition)
        {
            return AutomaticDunningManager.GetDunningAttemptsCount(accountId, storagePartition);
        }

        public static List<DunningAttempt> GetAutomaticDunningAttempts(string accountId, string storagePartition)
        {
            var dunningAttempts = new List<DunningAttempt>();

            var dunningAttmptTableEntities = AutomaticDunningManager.GetDunningAttempts(accountId, storagePartition);

            if (dunningAttmptTableEntities != null)
            {
                foreach (var tableEntity in dunningAttmptTableEntities)
                {
                    dunningAttempts.Add(Transformations.TransformAutomaticDunningAttemptsTableEntityToDunningAttempt(tableEntity));
                }
            }

            return dunningAttempts;
        }

        public static bool StoreAutomaticDunningAttempt(string accountId, string storagePartition, string stripeChargeId, string chargeAmount, string stripeSubscriptionId, string stripeEventId, string failureMessage)
        {
            return AutomaticDunningManager.StoreDunningAttempt(accountId, storagePartition, stripeChargeId, chargeAmount, stripeSubscriptionId, stripeEventId, failureMessage);
        }

        public static bool ClearAutomaticDunningAttempt(string accountId, string storagePartition)
        {
            return AutomaticDunningManager.ClearDunningAttempts(accountId, storagePartition);
        }

        #endregion

        #region BalanceTransactions

        public static SourceBalanceTransactions GetBalanceTransactionsForSource(string sourceId)
        {
            var sourceBalanceTransactions = new SourceBalanceTransactions();
            sourceBalanceTransactions.Transactions = new List<BalanceTransaction>();


            decimal totalGross = 0;
            decimal totalFees = 0;
            decimal totalNet = 0;

            //Get all balance transactions for the source
            var stripeBalanceTransactions = new StripeManager().GetBalanceTransactionsForSource(sourceId);

            if (stripeBalanceTransactions != null)
            {
                foreach (var stripeBalanceTransaction in stripeBalanceTransactions)
                {
                    var balanceTransaction = Transformations.TransformStripeBalanceTransactionToBalanceTransaction(stripeBalanceTransaction);

                    sourceBalanceTransactions.Transactions.Add(balanceTransaction);

                    totalGross = totalGross + balanceTransaction.Amount;
                    totalFees = totalFees + balanceTransaction.Fee;
                    totalNet = totalNet + balanceTransaction.Net;
                }
            }


            sourceBalanceTransactions.TotalGross = totalGross;
            sourceBalanceTransactions.TotalFees = totalFees;
            sourceBalanceTransactions.TotalNet = totalNet;


            return sourceBalanceTransactions;

        }

        public static List<BalanceTransaction> GetBalanceTransaction_CreatedSinceHourssAgo(int sinceHoursAgo, string startingAfterBalanceTransactionId = null)
        {
            var balanceTransactions = new List<BalanceTransaction>();
            var stripeBalanceTransactions = new StripeManager().GetBalanceTransactions_CreatedSinceHoursAgo(sinceHoursAgo, startingAfterBalanceTransactionId);

            if (stripeBalanceTransactions != null)
            {
                foreach (var stripeBalanceTransaction in stripeBalanceTransactions)
                {
                    balanceTransactions.Add(Transformations.TransformStripeBalanceTransactionToBalanceTransaction(stripeBalanceTransaction));
                }
            }

            return balanceTransactions;

        }

        public static List<BalanceTransaction> GetBalanceTransaction_AvailableSinceHourssAgo(int sinceHoursAgo, string startingAfterBalanceTransactionId = null)
        {
            var balanceTransactions = new List<BalanceTransaction>();
            var stripeBalanceTransactions = new StripeManager().GetBalanceTransactions_AvailableSinceHoursAgo(sinceHoursAgo, startingAfterBalanceTransactionId);

            if (stripeBalanceTransactions != null)
            {
                foreach (var stripeBalanceTransaction in stripeBalanceTransactions)
                {
                    balanceTransactions.Add(Transformations.TransformStripeBalanceTransactionToBalanceTransaction(stripeBalanceTransaction));
                }
            }

            return balanceTransactions;

        }

        #endregion

        #region Balances

        public static Balance GetBalance()
        {
            var balance = new Balance();
            var stripeBalance = new StripeManager().GetBalance();

            if (stripeBalance != null)
            {
                balance = Transformations.TransformStripeBalanceToBalance(stripeBalance);
            }

            return balance;

        }

        #endregion

        #region Transfers

        public static Transfer GetTransfer(string transferId)
        {
            var transfer = new Transfer();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();


            var stripeTransfer = stripeManager.GetTransfer(transferId);

            if (stripeTransfer != null)
            {
                return Transformations.TransformStripeTransferToTransfer(stripeTransfer);
            }


            #endregion

            return transfer;
        }

        public static List<Transfer> GetTransferHistory(int limit)
        {
            var transfers = new List<Transfer>();
            var stripeTransfers = new StripeManager().GetTransfers(limit);


            if (stripeTransfers != null)
            {
                foreach (var stripeTransfer in stripeTransfers)
                {
                    transfers.Add(Transformations.TransformStripeTransferToTransfer(stripeTransfer));
                }
            }

            return transfers;

        }

        public static List<Transfer> GetTransferHistory(int itemLimit, DateTime startDate, DateTime endDate, string accountId = null)
        {
            var transfers = new List<Transfer>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();


            var stripeTransfers = stripeManager.GetTransfers(itemLimit, startDate, endDate);

            if (stripeTransfers != null)
            {
                foreach (var stripeTransfer in stripeTransfers)
                {
                    transfers.Add(Transformations.TransformStripeTransferToTransfer(stripeTransfer));
                }
            }


            #endregion

            return transfers;
        }

        public static List<Transfer> GetTransferHistory_Next(int itemLimit, string startingAfterTransferId)
        {
            var transfers = new List<Transfer>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis CacheTransfer

            var stripeManager = new StripeManager();

            var stripeTransfers = stripeManager.GetTransfers_Next(itemLimit, startingAfterTransferId);

            if (stripeTransfers != null)
            {
                foreach (var stripeTransfer in stripeTransfers)
                {
                    transfers.Add(Transformations.TransformStripeTransferToTransfer(stripeTransfer));
                }
            }



            #endregion

            return transfers;
        }

        public static List<Transfer> GetTransferHistory_Last(int itemLimit, string endingBeforeTransferId)
        {
            var transfers = new List<Transfer>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();

            var stripeTransfers = stripeManager.GetTransfers_Last(itemLimit, endingBeforeTransferId);

            if (stripeTransfers != null)
            {
                foreach (var stripeTransfer in stripeTransfers)
                {
                    transfers.Add(Transformations.TransformStripeTransferToTransfer(stripeTransfer));
                }
            }



            #endregion

            return transfers;
        }


        public static List<Transfer> GetTransferHistory_ByDateRange_Next(int itemLimit, string startingAfterTransferId, DateTime startDate, DateTime endDate, string accountId = null)
        {
            var transfers = new List<Transfer>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();

            var stripeTransfers = stripeManager.GetTransfers_ByDateRange_Next(itemLimit, startingAfterTransferId, startDate, endDate);

            if (stripeTransfers != null)
            {
                foreach (var stripeTransfer in stripeTransfers)
                {
                    transfers.Add(Transformations.TransformStripeTransferToTransfer(stripeTransfer));
                }
            }



            #endregion

            return transfers;
        }

        public static List<Transfer> GetTransferHistory_ByDateRange_Last(int itemLimit, string endingBeforeTransferId, DateTime startDate, DateTime endDate, string accountId = null)
        {
            var transfers = new List<Transfer>();

            #region (Plan A) Check Redis Cache First

            #endregion

            #region (Plan B) Call Stripe API, Transform Data & Store in Redis Cache

            var stripeManager = new StripeManager();

            var stripeTransfers = stripeManager.GetTransfers_ByDateRange_Last(itemLimit, endingBeforeTransferId, startDate, endDate);

            if (stripeTransfers != null)
            {
                foreach (var stripeTransfer in stripeTransfers)
                {
                    transfers.Add(Transformations.TransformStripeTransferToTransfer(stripeTransfer));
                }
            }

            #endregion

            return transfers;
        }



        #endregion

        /*
        public static List<Fee> GetFees()
        {
            var fees = new List<Fee>();

            
            var stipeApplicationFees = new StripeManager().GetApplicationFeeHistory();


            if (stipeApplicationFees != null)
            {
                foreach (var stipeApplicationFee in stipeApplicationFees)
                {
                    fees.Add(Transformations.TransformStripeApplicationFeeToFee(stipeApplicationFee));
                }
            }
            
              
            return fees;

        }*/

    }   
}
