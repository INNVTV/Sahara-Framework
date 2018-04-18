using Sahara.Core.Accounts;
using Sahara.Core.Platform.Billing.Models;
using Sahara.Core.Platform.Billing.TableEntities;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Billing.Internal
{
    public static class Transformations
    {
        internal static Charge TransformStripeChargeToCharge(StripeCharge stripeCharge, bool includeAccount = false)
        {
            var charge = new Charge
            {
                ChargeID = stripeCharge.Id,
                InvoiceID = stripeCharge.InvoiceId,
                StripeCustomerID = stripeCharge.CustomerId,

                Description = stripeCharge.Description,
                Date = stripeCharge.Created,
                Prorated = false
            };


            if (stripeCharge.Invoice != null)
            {
                charge.Invoice = TransformStripeInvoiceToInvoice(stripeCharge.Invoice);
            }

            if(String.IsNullOrEmpty(charge.Description) && stripeCharge.Invoice != null)
            {
                try
                {
                    charge.Description = stripeCharge.Invoice.StripeInvoiceLineItems.Data[0].Plan.Name;

                    if (stripeCharge.Invoice.StripeInvoiceLineItems.Data[0].Proration)
                    {
                        charge.Prorated = true;
                    }

                    /*
                    if (stripeCharge.Invoice.StripeInvoiceLineItems.Data[0].Proration)
                    {
                        charge.Description = stripeCharge.Invoice.StripeInvoiceLineItems.Data[0].Plan.Name + " [Prorated] ";
                    }
                    else
                    {
                        charge.Description = stripeCharge.Invoice.StripeInvoiceLineItems.Data[0].Plan.Name;
                    }*/                    
                }
                catch
                {

                }
     
            }

            charge.BalanceTransactionID = stripeCharge.BalanceTransactionId;

            try
            {
                charge.CardBrand = stripeCharge.Source.Card.Brand;
                charge.CardLast4 = stripeCharge.Source.Card.Last4;
            }
            catch
            {

            }

            // Only used when getting ALL charges by PlatformAdmin
            if (includeAccount)
            {
                try
                {
                    var account = AccountManager.GetAccount(stripeCharge.CustomerId);
                    charge.AccountName = account.AccountName;
                    charge.AccountID = account.AccountID.ToString();
                }
                catch
                {
                    // undefined or account no longer exists
                }

            }

            charge.Paid = stripeCharge.Paid;


            charge.Refunded = stripeCharge.Refunded;
            

            if (!String.IsNullOrEmpty(stripeCharge.FailureCode))
            {
                charge.Failure = true;
                charge.FailureCode = stripeCharge.FailureCode;
                charge.FailureMessage = stripeCharge.FailureMessage;
            }


            charge.Amount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeCharge.Amount.ToString());


            charge.TotalRefunded = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeCharge.AmountRefunded.ToString());




            return charge;
        }

        internal static Invoice TransformStripeInvoiceToInvoice(StripeInvoice stripeInvoice, bool includeAccount = false)
        {
            var invoice = new Invoice
            {
                InvoiceID = stripeInvoice.Id,
                StripeCustomerID = stripeInvoice.CustomerId,

                Description = stripeInvoice.Description,
                Attempts = stripeInvoice.AttemptCount,
                Date = stripeInvoice.Date.Value,
                LineItems = new List<InvoiceLineItem>()
            };




            // Only used when getting ALL invoices by PlatformAdmin
            if (includeAccount)
            {
                try
                {
                    var account = AccountManager.GetAccount(stripeInvoice.CustomerId);
                    invoice.Account = account;
                }
                catch
                {
                    // undefined or account no longer exists
                }

            }

            invoice.Paid = stripeInvoice.Paid;
            
            invoice.Subtotal = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeInvoice.Subtotal.ToString());
            
            invoice.Total = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeInvoice.Total.ToString());

            invoice.AmountDue = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeInvoice.AmountDue.ToString());
            
            invoice.StartingBalance = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeInvoice.StartingBalance.ToString());
            
            if (stripeInvoice.EndingBalance.HasValue)
            {
                invoice.EndingBalance = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeInvoice.EndingBalance.Value.ToString());
            }

            if (stripeInvoice.NextPaymentAttempt.HasValue)
            {
                invoice.NextAttempt = stripeInvoice.NextPaymentAttempt.Value;
            }

            foreach (var stripeInvoiceItem in stripeInvoice.StripeInvoiceLineItems.Data)
            {
                invoice.LineItems.Add(TransformStripeInvoiceItemToInvoiceLineItem(stripeInvoiceItem));
            }

            return invoice;
        }

        internal static InvoiceLineItem TransformStripeInvoiceItemToInvoiceLineItem(StripeInvoiceLineItem stripeInvoiceItem)
        {
            var invoiceLineItem = new InvoiceLineItem
            {
                Description = stripeInvoiceItem.Description,
                Proration = stripeInvoiceItem.Proration,
                Amount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(stripeInvoiceItem.Amount.ToString())
            };

            if (stripeInvoiceItem.Plan != null)
            {
                invoiceLineItem.PlanDescription = stripeInvoiceItem.Plan.Name;

                invoiceLineItem.PlanInterval = stripeInvoiceItem.Plan.Interval;
                invoiceLineItem.PlanIntervalCount = stripeInvoiceItem.Plan.IntervalCount;
            }

            return invoiceLineItem;
        }

        internal static DunningAttempt TransformAutomaticDunningAttemptsTableEntityToDunningAttempt(AutomaticDunningAttemptsTableEntity tableEntity)
        {
            var dunningAttempt = new DunningAttempt();

            dunningAttempt.WarningDate = tableEntity.Timestamp;
            dunningAttempt.Description = tableEntity.FailureMessage;
            dunningAttempt.Amount = tableEntity.ChargeAmount;

            dunningAttempt.StripeChargeId = tableEntity.StripeChargeID;
            dunningAttempt.StripeEventId = tableEntity.StripeEventID;
            dunningAttempt.StripeSubscriptionId = tableEntity.StripeSubscriptionID;

            return dunningAttempt;

        }

        internal static BalanceTransaction TransformStripeBalanceTransactionToBalanceTransaction(StripeBalanceTransaction stripeBalanceTransaction)
        {

            try
            {
                var balanceTransaction = new BalanceTransaction
                {
                    BalanceTransactionID = stripeBalanceTransaction.Id,
                    Amount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeBalanceTransaction.Amount.ToString()),
                    Net = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeBalanceTransaction.Net.ToString()),
                    Fee = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeBalanceTransaction.Fee.ToString()),

                    Type = stripeBalanceTransaction.Type,
                    Status = stripeBalanceTransaction.Status,

                    Created = stripeBalanceTransaction.Created,
                    Available = stripeBalanceTransaction.AvailableOn
                };

                if(balanceTransaction.Type == "transfer")
                {
                    balanceTransaction.Amount = Math.Abs(balanceTransaction.Amount);
                    balanceTransaction.Net = Math.Abs(balanceTransaction.Net);
                }

                return balanceTransaction;
            }
            catch
            {
                return null;
            }


        }

        internal static Balance TransformStripeBalanceToBalance(StripeBalance stripeBalance)
        {
            var balance = new Balance();

            balance.Available = new List<decimal>();
            balance.Pending = new List<decimal>();

            foreach(var available in stripeBalance.Available)
            {
                balance.Available.Add(Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(available.Amount.ToString()));
            }

            foreach(var pending in stripeBalance.Pending)
            {
                balance.Pending.Add(Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(pending.Amount.ToString()));
            }

            return balance;
        }

        internal static Transfer TransformStripeTransferToTransfer(StripeTransfer stripeTransfer)
        {
            var transfer = new Transfer
            {
                TransferID = stripeTransfer.Id,
                Amount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeTransfer.Amount.ToString()),
                Status = stripeTransfer.Status.ToString(),
                Type = stripeTransfer.Type.ToString(),
                Date = stripeTransfer.Date,
                Created = stripeTransfer.Created,
            };

            if (stripeTransfer.Description != null)
            {
                transfer.Description = stripeTransfer.Description;
            }
            if (stripeTransfer.Destination != null)
            {
                transfer.Destination = stripeTransfer.Destination;
            }
            if (stripeTransfer.DestinationPayment != null)
            {
                transfer.DestinationPayment = stripeTransfer.DestinationPayment;
            }
            if (stripeTransfer.BalanceTransactionId != null)
            {
                transfer.BalanceTransactionID = stripeTransfer.BalanceTransactionId;
            }
            //if(stripeTransfer.BalanceTransaction != null)
            //{
                //transfer.BalanceTransaction = TransformStripeBalanceTransactionToBalanceTransaction(stripeTransfer.BalanceTransaction);
            //}
            if (stripeTransfer.FailureCode != null)
            {
                transfer.FailureCode = stripeTransfer.FailureCode;
            }
            if (stripeTransfer.FailureMessage != null)
            {
                transfer.FailureMessage = stripeTransfer.FailureMessage;
            }

            return transfer;
        }

        internal static Fee TransformStripeApplicationFeeToFee(StripeApplicationFee stripeApplicationFee)
        {
            var fee = new Fee
            {
                Id = stripeApplicationFee.Id,
                Amount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeApplicationFee.Amount.ToString()),
                Refunded = stripeApplicationFee.Refunded,
                Created = stripeApplicationFee.Created,
                ChargeId = stripeApplicationFee.ChargeId,
                BalanceTransactionId = stripeApplicationFee.BalanceTransactionId,
            };

            return fee;
        }
    }
}
