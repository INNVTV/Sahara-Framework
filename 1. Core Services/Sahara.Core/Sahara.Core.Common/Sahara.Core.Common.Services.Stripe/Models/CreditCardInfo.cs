using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Services.Stripe.Models
{

    public class CreditCardInfo
    {
        public string CardName;
        public string CardBrand;

        public string Last4;
        public string ExpirationMonth;
        public string ExpirationYear;

        public string StripeCardId;

    }
}
