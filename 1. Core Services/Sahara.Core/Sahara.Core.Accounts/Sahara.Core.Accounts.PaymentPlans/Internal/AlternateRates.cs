using Sahara.Core.Accounts.PaymentPlans.Models;
using Sahara.Core.Accounts.PaymentPlans.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.PaymentPlans.Internal
{
    public static class AlternateRates
    {
        public static List<AlternateRate> AssignAlternateRates(decimal MonthlyRate)
        {
            var results = new List<AlternateRate>();

            if(MonthlyRate > 0)
            {
                var frequencies = PaymentPlanManager.GetPaymentFrequencies();

                foreach(var frequency in frequencies)
                {
                    //Ignore the FREE and base MONTHLY rates or any rate at a month or below
                    if(frequency.PaymentFrequencyMonths > 1)
                    {
                        decimal price = Convert.ToDecimal(Sahara.Core.Common.Methods.Billing.GenerateStripePlanAmountInCents(MonthlyRate, frequency.PaymentFrequencyMonths, frequency.PriceBreak));
                        string savingsPercentage = frequency.PriceBreak.ToString().Substring((frequency.PriceBreak.ToString().IndexOf(".") + 1));

                        string frequencyInterval = string.Empty;

                        if(frequency.IntervalCount > 1)
                        {
                            frequencyInterval = frequency.IntervalCount + " " + frequency.Interval.ToLower() + "s";
                        }
                        else
                        {
                            frequencyInterval = frequency.Interval;
                        }

                        results.Add(
                            new AlternateRate {
                                    FrequencyInMonths = frequency.PaymentFrequencyMonths,
                                    FrequencyName = frequency.PaymentFrequencyName,
                                    FrequencyInterval = frequencyInterval,
                                    DiscountedPrice = price,
                                    SavingsPercentage = savingsPercentage,
                                    Description = "Save " + savingsPercentage + "% per " + frequency.Interval.ToLower() + "!"
                                }
                            );
                    }
                }

            }

            return results;
        }
    }
}
