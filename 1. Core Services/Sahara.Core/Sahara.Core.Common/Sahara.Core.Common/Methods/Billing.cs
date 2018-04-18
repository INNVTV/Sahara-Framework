using Stripe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Methods
{
    public static class Billing
    {
       
        public static string GenerateStripePlanID(string planName, int frequencyIntervalCount, string frequencyInterval)
        {
            return
                planName.Replace(" ", "").ToLower()
                + "-"
                + frequencyIntervalCount.ToString().Replace(" ", "-")
                + "-"
                + frequencyInterval.Replace(" ", "-").ToLower();
        }

        public static string GenerateStripePlanName(string planName, string frequencyName)
        {
            return
                planName + " (" + frequencyName + ")";
        }

        public static string GenerateStripePlanAmountInCents(decimal monthlyRate, int frequencyInMonths, decimal priceBreak)
        {
            //calculate monthly or yearly rates with price breaks and convert to cents
            return ((monthlyRate * frequencyInMonths) - ((monthlyRate * frequencyInMonths) * priceBreak)).ToString("F");
        }

        public static string ConvertStripeAmountToDollars(string amount)
        {
            try
            {
                var amountConverted = (Convert.ToDecimal(amount) / (decimal)100).ToString();

                var amountString = "$" + String.Format("{0:#,###,###.00}", Convert.ToDouble(amountConverted));

                if (amountString.Contains("-"))
                {
                    amountString = "-" + amountString.Replace("-", ""); //move the minus symbol out to the front of the dollar sign for better readability
                }

                return amountString;
            }
            catch
            {
                //If a parsing error occurs we simply append a dollar symbol:
                return "$" + amount.ToString();
            }

        }

        public static decimal ConvertStripeAmountToDecimals(string amount)
        {
            try
            {
                return ((Convert.ToDecimal(amount) / (decimal)100));
            }
            catch
            {
                return 0;
            }
            
        }

        public static bool IsDecimalInTwoPlaces(decimal decimalIn)
        {
            try
            {
                var amountStr = decimalIn.ToString();

                int decimalOccuranceCount = amountStr.Split('.').Length - 1;
                if (decimalOccuranceCount != 1)
                {
                    throw new Exception("You must use a value with 1 decimal using 2 decimal places");
                }

		
                int decimalPlaceCount = amountStr.Substring(amountStr.IndexOf(".") + 1, amountStr.Length - amountStr.IndexOf(".") - 1).Length;

                if (decimalPlaceCount == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

        public static Int32 ConvertDecimalToStripeAmount(decimal decimalIn)
        {
            try
            {
                var amountStr = decimalIn.ToString();

                int decimalOccuranceCount = amountStr.Split('.').Length - 1;
                if (decimalOccuranceCount != 1)
                {
                    throw new Exception("You must use a value with 1 decimal using 2 decimal places");
                }


                int decimalPlaceCount = amountStr.Substring(amountStr.IndexOf(".") + 1, amountStr.Length - amountStr.IndexOf(".") - 1).Length;

                if (decimalPlaceCount == 2)
                {
                    int intOut = Convert.ToInt32(decimalIn.ToString().Replace(".", ""));
                    return intOut;
                }
                else
                {
                    throw new Exception("You must use a value with 2 decimal places");
                }
            }
            catch(Exception)
            {
                throw;
            }

            
        }

        public static string GenerateStripeHTMLInvoice(StripeInvoice stripeInvoice)
        {
            var invoiceHtml = new StringBuilder();

            #region Build out Invoice HTML

            //Generate dollar amounts
            var invoiceSubtotal = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(stripeInvoice.Subtotal.ToString());
            var invoiceTotal = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(stripeInvoice.Total.ToString());
            var invoiceAmountDue = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(stripeInvoice.AmountDue.ToString());


            invoiceHtml.Append("<table style='width:90%; border-spacing:0px; font-size:13px;'>");

            invoiceHtml.Append("<tr>");

            invoiceHtml.Append("<td style='padding:2px; width:80%; border-bottom: solid 2px black; border-top: solid 3px black;'>");
            invoiceHtml.Append("<strong>Description</strong>");
            invoiceHtml.Append("</td>");

            ///invoiceHtml.Append("<td style='border-bottom: solid 1px black; border-top: solid 3px black;'>");
            ///invoiceHtml.Append("Quantity");
            //invoiceHtml.Append("</td>");

            invoiceHtml.Append("<td style='padding:2px; width:20%; border-bottom: solid 2px black; border-top: solid 3px black; text-align:center'>");
            invoiceHtml.Append("<strong>Price</strong>");
            invoiceHtml.Append("</td>");

            invoiceHtml.Append("</tr>");


           

            
            var last = stripeInvoice.StripeInvoiceLineItems.Data.Last();
            foreach(var lineItem in stripeInvoice.StripeInvoiceLineItems.Data)
            {
                //Generate dollar amount
                var lineItemAmount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(lineItem.Amount.ToString());

                //Generate description
                string descriptionCopy = lineItem.Description;

                if (String.IsNullOrEmpty(descriptionCopy))
                {
                    if (lineItem.Plan != null)
                    {
                        descriptionCopy = lineItem.Plan.Name + " Plan";
                    }
                    else
                    {
                        descriptionCopy = "No description available";
                    }
                   
                }

                invoiceHtml.Append("<tr>");

                if (!lineItem.Equals(last))
                {
                    invoiceHtml.Append("<td style='padding:10px; color:grey; border-bottom: dashed 1px #D1D1D1;'>");
                }
                else{
                    invoiceHtml.Append("<td style='padding:10px; color:grey; border-bottom: solid 2px black;'>");
                }

                invoiceHtml.Append(descriptionCopy);
                invoiceHtml.Append("</td>");

                //invoiceHtml.Append("<td style='color:grey'>"); ;
                //invoiceHtml.Append(lineItem.Quantity);
                //invoiceHtml.Append("</td>");

                if (!lineItem.Equals(last))
                {
                    invoiceHtml.Append("<td style='padding:10px; color:grey; border-bottom: dashed 1px #D1D1D1; text-align:center'>");
                }
                else
                {
                    invoiceHtml.Append("<td style='padding:10px; color:grey; border-bottom: solid 2px black; text-align:center'>");
                }

                invoiceHtml.Append(lineItemAmount);
                invoiceHtml.Append("</td>");

                invoiceHtml.Append("</tr>");
            }
            


            //--------------------
            invoiceHtml.Append("<tr>");

            //invoiceHtml.Append("<td>");
            //invoiceHtml.Append("");
            //invoiceHtml.Append("</td>");

            invoiceHtml.Append("<td style='text-align:right'>");
            invoiceHtml.Append("Subtotal:");
            invoiceHtml.Append("</td>");

            invoiceHtml.Append("<td style='padding:6px; text-align:center'>");
            invoiceHtml.Append(invoiceSubtotal);
            invoiceHtml.Append("</td>");

            invoiceHtml.Append("</tr>");

            //--------------------
            invoiceHtml.Append("<tr>");

            //invoiceHtml.Append("<td>");
            //invoiceHtml.Append("");
            //invoiceHtml.Append("</td>");

            invoiceHtml.Append("<td style='text-align:right'>");
            invoiceHtml.Append("Total:");
            invoiceHtml.Append("</td>");

            invoiceHtml.Append("<td style='padding:6px; text-align:center'>");
            invoiceHtml.Append(invoiceTotal);
            invoiceHtml.Append("</td>");

            invoiceHtml.Append("</tr>");

            //--------------------
            invoiceHtml.Append("<tr>");

            //invoiceHtml.Append("<td>");
            //invoiceHtml.Append("");
            //invoiceHtml.Append("</td>");

            invoiceHtml.Append("<td style='text-align:right'>");
            invoiceHtml.Append("<strong>Amount Due:</strong>");
            invoiceHtml.Append("</td>");

            invoiceHtml.Append("<td style='padding:8px; text-align:center; font-size:15px'><strong>");
            invoiceHtml.Append(invoiceAmountDue);
            invoiceHtml.Append("</strong></td>");

            invoiceHtml.Append("</tr>");


            //------------------
            invoiceHtml.Append("<table>");

            #endregion

            return invoiceHtml.ToString();
        }

        public static string GenerateStripeHTMLChargeDetails(StripeCharge stripeCharge)
        {
            var chargeHtml = new StringBuilder();

            #region Build out Invoice HTML

            //Generate dollar amounts
            //var chargeeSubtotal = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(stripeCharge.Subtotal.ToString());
            //var chargeTotal = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(stripeCharge.Total.ToString());
            var chargeTotalAmount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(stripeCharge.Amount.ToString());


            chargeHtml.Append("<table style='width:90%; border-spacing:0px; font-size:13px;'>");

            chargeHtml.Append("<tr>");

            chargeHtml.Append("<td style='padding:2px; width:80%; border-bottom: solid 2px black; border-top: solid 3px black;'>");
            chargeHtml.Append("<strong>Description</strong>");
            chargeHtml.Append("</td>");


            chargeHtml.Append("<td style='padding:2px; width:20%; border-bottom: solid 2px black; border-top: solid 3px black; text-align:center'>");
            chargeHtml.Append("<strong>Price</strong>");
            chargeHtml.Append("</td>");

            chargeHtml.Append("</tr>");


            //Generate dollar amount
            var lineItemAmount = Sahara.Core.Common.Methods.Billing.ConvertStripeAmountToDollars(stripeCharge.Amount.ToString());

            //Generate description
            string descriptionCopy = stripeCharge.Description;

            if (String.IsNullOrEmpty(descriptionCopy))
            {               
                descriptionCopy = "No description available";    
            }

            chargeHtml.Append("<tr>");

            chargeHtml.Append("<td style='padding:10px; color:grey; border-bottom: solid 2px black;'>");


            chargeHtml.Append(descriptionCopy);
            chargeHtml.Append("</td>");

            chargeHtml.Append("<td style='padding:10px; color:grey; border-bottom: solid 2px black; text-align:center'>");

            chargeHtml.Append(lineItemAmount);
            chargeHtml.Append("</td>");

            chargeHtml.Append("</tr>");




            //--------------------
            //chargeHtml.Append("<tr>");

            //invoiceHtml.Append("<td>");
            //invoiceHtml.Append("");
            //invoiceHtml.Append("</td>");

            //chargeHtml.Append("<td style='text-align:right'>");
            //chargeHtml.Append("Subtotal:");
            //chargeHtml.Append("</td>");

            //chargeHtml.Append("<td style='padding:6px; text-align:center'>");
            //chargeHtml.Append(invoiceSubtotal);
            //chargeHtml.Append("</td>");

            //chargeHtml.Append("</tr>");

            //--------------------
            //chargeHtml.Append("<tr>");

            //invoiceHtml.Append("<td>");
            //invoiceHtml.Append("");
            //invoiceHtml.Append("</td>");

            /*
            invoiceHtml.Append("<td style='text-align:right'>");
            invoiceHtml.Append("Total:");
            invoiceHtml.Append("</td>");

            invoiceHtml.Append("<td style='padding:6px; text-align:center'>");
            invoiceHtml.Append(invoiceTotal);
            invoiceHtml.Append("</td>");

            invoiceHtml.Append("</tr>");
*/
            //--------------------
            chargeHtml.Append("<tr>");
            
            //invoiceHtml.Append("<td>");
            //invoiceHtml.Append("");
            //invoiceHtml.Append("</td>");

            chargeHtml.Append("<td style='text-align:right'>");
            chargeHtml.Append("<strong>Total:</strong>");
            chargeHtml.Append("</td>");

            chargeHtml.Append("<td style='padding:8px; text-align:center; font-size:15px'><strong>");
            chargeHtml.Append(chargeTotalAmount);
            chargeHtml.Append("</strong></td>");

            chargeHtml.Append("</tr>");


            //------------------
            chargeHtml.Append("<table>");

            #endregion

            return chargeHtml.ToString();
        }

        /*
        public static string GetPlanNameFromStripePlanName(string stripePlanName)
        {
            return stripePlanName.Substring(0, stripePlanName.IndexOf("-"));
        }

        public static string GetPlanFrequencyFromStripePlanIntervals(string interval, int intervalCount)
        {
            if(intervalCount > 1)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                return intervalCount + " " + textInfo.ToTitleCase(interval); ;
            }
            else
            {
                switch(interval)
                {
                    case "year":
                        return "Yearly";
                    case "month":
                        return "Monthly";
                    case "week":
                        return "Weekly";
                    default :
                        return "";
                }
            }
        }*/
    }
}
