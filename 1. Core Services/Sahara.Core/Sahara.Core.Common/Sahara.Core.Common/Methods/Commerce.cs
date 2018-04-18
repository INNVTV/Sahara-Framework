using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Methods
{
    public static class Commerce
    {
        public static int ConvertDollarAmountToCredits(decimal dollarAmount)
        {
            double credits = Convert.ToDouble(dollarAmount) * Settings.Commerce.Credits.creditsPerDollar;
            return Convert.ToInt32(Math.Ceiling(credits)); //<-- We round up and convert to a whole integer, but UI should only ever wllow whole amounts in.
        }

        public static int ConvertCreditsAmountToDollars(int creditsAmount)
        {
            return (creditsAmount / Settings.Commerce.Credits.creditsPerDollar);
        }
    }
}
