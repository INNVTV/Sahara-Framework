using Newtonsoft.Json;
using Sahara.Core.Common.Services.Stripe;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Platform.Billing;
using Sahara.Core.Platform.Billing.Models;
using Sahara.Core.Platform.Reports.Models;
using StackExchange.Redis;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Reports.Public
{
    public static class PlatformReportsManager
    {
        public static BillingReport GetBillingReport(int sinceHoursAgo)
        {
            var billingReport = new BillingReport();

            #region get from Redis

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            Object cachedBillingReport = null;

            try
            {
                cachedBillingReport = cache.HashGet(
                           Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.ReportsHash.Key,
                           Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.ReportsHash.Fields.Billing(sinceHoursAgo)
                           );

                if (((RedisValue)cachedBillingReport).HasValue)
                {
                    billingReport = JsonConvert.DeserializeObject<BillingReport>((RedisValue)cachedBillingReport);
                }
            }
            catch
            {

            }

            #endregion

            #region Generate report

            if (((RedisValue)cachedBillingReport).IsNullOrEmpty)
            {
                var stripeManager = new StripeManager();


                //Counts & Totals:
                var chargeCount = 0;
                //var failedChargeCount = 0;
                var refundCount = 0;
                var transferCount = 0;
                var transferCount_Pending = 0;
                var transferCount_Complete = 0;

                decimal totalGross = 0;
                decimal totalFees = 0;
                decimal totalRefunds = 0;
                decimal totalNet = 0;
                decimal totalTransfers = 0;
                decimal totalTransfers_Pending = 0;
                decimal totalTransfers_Complete = 0;


                //Get charges from time period:
                /*var stripeCharges = new List<StripeCharge>();


                //Since Stripe limit is 100 we break up the call into 100 item increments until we reach the last item:
                var stripeChargesPending = stripeManager.GetCharges_SinceHoursAgo(sinceHoursAgo, null, null, true).ToList();

                stripeCharges.AddRange(stripeChargesPending);
                while (stripeChargesPending.Count == 100)
                {
                    stripeChargesPending = stripeManager.GetCharges_SinceHoursAgo(sinceHoursAgo, stripeChargesPending[99].Id, null, true).ToList();
                    stripeCharges.AddRange(stripeChargesPending);
                }
                */



                //Loop through all charges from time period and generate report data
                /*
                foreach(StripeCharge stripeCharge in stripeCharges)
                {
                    if (stripeCharge.Paid.HasValue)
                    {
                        if (stripeCharge.Paid.Value)
                        {
                            chargeCount++;
                            /*totalGross = totalGross + Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeCharge.Amount.ToString());
                            totalFees = totalFees + Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeCharge.BalanceTransaction.Fee.ToString());             
                            totalNet = totalNet + Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeCharge.BalanceTransaction.Net.ToString());
                            if (stripeCharge.AmountRefunded.HasValue)
                            {
                                totalRefunds = totalRefunds + Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDecimals(stripeCharge.AmountRefunded.Value.ToString());
                            }
                            if (stripeCharge.Refunded.HasValue)
                            {
                                if (stripeCharge.Refunded.Value)
                                {
                                    refundCount++;
                                }
                            }* /
                        }
                        else if (!stripeCharge.Paid.Value)
                        {
                            failedChargeCount++;
                        }
                    }

                }*/

                billingReport.BalanceTransactions = new List<Billing.Models.BalanceTransaction>();
                billingReport.BalanceTransactions_Created = new List<Billing.Models.BalanceTransaction>();
                billingReport.BalanceTransactions_Available = new List<Billing.Models.BalanceTransaction>();

                //Since Stripe limit is 100 we break up the call into 100 item increments until we reach the last item:
                var balanceTransactions = PlatformBillingManager.GetBalanceTransaction_AvailableSinceHourssAgo(sinceHoursAgo);
                
                billingReport.BalanceTransactions.AddRange(balanceTransactions);
                while(balanceTransactions.Count == 100)
                {
                    balanceTransactions = PlatformBillingManager.GetBalanceTransaction_AvailableSinceHourssAgo(sinceHoursAgo, balanceTransactions[99].BalanceTransactionID);
                    billingReport.BalanceTransactions.AddRange(balanceTransactions);
                }



                


                //Split all results into 2 camps ===============================================

                foreach(var balanceTransaction in billingReport.BalanceTransactions)
                {
                    if (balanceTransaction.Created >= DateTime.UtcNow.AddHours(sinceHoursAgo * -1))
                    {
                        billingReport.BalanceTransactions_Created.Add(balanceTransaction);
                    }
                    else{
                        billingReport.BalanceTransactions_Available.Add(balanceTransaction);
                    }
                }

                //=================================================================================================
                //Loop through all balance transactions from ENTIRE time period to generate daily activity data for transactions


                foreach (var btr in billingReport.BalanceTransactions)
                {
                    
                    if (btr.Type == "transfer")
                    {
                        transferCount++;
                        totalTransfers = totalTransfers + btr.Amount;

                        if (btr.Status == "pending")
                        {
                            transferCount_Pending++;
                            totalTransfers_Pending = totalTransfers_Pending + btr.Amount;
                        }
                        else
                        {
                            transferCount_Complete++;
                            totalTransfers_Complete = totalTransfers_Complete + btr.Amount;
                        }
                    }
                }


                //=================================================================================================
                //Loop through all balance transactions from the CREATED time period to generate daily activity data for charges & refunds

                foreach (var btr in billingReport.BalanceTransactions_Created)
                {
                    if (btr.Type == "charge")
                    {
                        chargeCount++;

                        totalGross = totalGross + btr.Amount;
                        totalFees = totalFees + btr.Fee;
                        totalNet = totalNet + btr.Net;
                    }
                    else if (btr.Type == "refund")
                    {
                        refundCount++;

                        totalRefunds = totalRefunds + btr.Amount;

                        //totalGross = totalGross + btr.Amount;
                        totalFees = totalFees + btr.Fee;
                        totalNet = totalNet + btr.Net;
                    }
                    else if (btr.Type == "adjustment")
                    {

                        totalGross = totalGross + btr.Amount;
                        totalFees = totalFees + btr.Fee;
                        totalNet = totalNet + btr.Net;
                    }
                    else if (btr.Type == "application_fee" || btr.Type == "application_fee_refund")
                    {

                        totalGross = totalGross + btr.Amount;
                        totalFees = totalFees + btr.Fee;
                        totalNet = totalNet + btr.Net;
                    }
                        /*
                    else if (btr.Type == "transfer")
                    {

                        transferCount++;
                        totalTransfers = totalTransfers + btr.Amount;
                        if (btr.Status == "pending")
                        {
                            transferCount_Pending++;
                            totalTransfers_Pending = totalTransfers_Pending + btr.Amount;
                        }
                        else
                        {
                            transferCount_Complete++;
                            totalTransfers_Complete = totalTransfers_Complete + btr.Amount;
                        }
                    }*/
                }

                billingReport.ChargeCount = chargeCount;
                billingReport.RefundCount = refundCount;
                //billingReport.FailedChargeCount = failedChargeCount;
                billingReport.TransferCount = transferCount;
                billingReport.TransferCount_Pending = transferCount_Pending;
                billingReport.TransferCount_Complete = transferCount_Complete;

                billingReport.TotalGross = totalGross;
                billingReport.TotalFees = Math.Abs(totalFees);
                billingReport.TotalRefunds = Math.Abs(totalRefunds);
                billingReport.TotalNet = totalNet;
                billingReport.TotalTransfers = Math.Abs(totalTransfers);
                billingReport.TotalTransfers_Pending = Math.Abs(totalTransfers_Pending);
                billingReport.TotalTransfers_Complete = Math.Abs(totalTransfers_Complete);

                //============================================


                billingReport.BillingIssues = false;

                //Check for any billing issues during this time period
                var latestBillingIssue = PlatformLogManager.GetPlatformLogByActivity(Logging.PlatformLogs.Types.ActivityType.Billing_Issue, 1);
                if (latestBillingIssue.Count > 0)
                {
                    if(latestBillingIssue[0].Timestamp >= DateTime.UtcNow.AddHours(sinceHoursAgo * -1))
                    {
                        billingReport.BillingIssues = true;
                    }
                }


                #region Store into Redis

                try
                {
                    //Store a copy in the Redis cache
                    cache.HashSet(
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.ReportsHash.Key,
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.ReportsHash.Fields.Billing(sinceHoursAgo),
                            JsonConvert.SerializeObject(billingReport),
                            When.Always,
                            CommandFlags.FireAndForget
                            );

                    //Expire cache after set time
                    cache.KeyExpire(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.ReportsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.ReportsHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }


                #endregion
            }



            /*/TESTS!
            billingReport.BalanceTransactions.Add(new BalanceTransaction { Type = "adjustment", Amount = Convert.ToDecimal(20.77), Fee = Convert.ToDecimal(20.77), Net = Convert.ToDecimal(20.77) });
            billingReport.BalanceTransactions.Add(new BalanceTransaction { Type = "application_fee", Amount = Convert.ToDecimal(20.77), Fee = Convert.ToDecimal(20.77), Net = Convert.ToDecimal(20.77) });
            billingReport.BalanceTransactions.Add(new BalanceTransaction { Type = "application_fee_refund", Amount = Convert.ToDecimal(20.77), Fee = Convert.ToDecimal(20.77), Net = Convert.ToDecimal(20.77) });
            billingReport.BalanceTransactions.Add(new BalanceTransaction { Type = "transfer_cancel", Amount = Convert.ToDecimal(20.77), Fee = Convert.ToDecimal(20.77), Net = Convert.ToDecimal(20.77) });
            billingReport.BalanceTransactions.Add(new BalanceTransaction { Type = "transfer_failure", Amount = Convert.ToDecimal(20.77), Fee = Convert.ToDecimal(20.77), Net = Convert.ToDecimal(20.77) });
            */


            #endregion

            return billingReport;
        }
    }
}
