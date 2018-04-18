using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class PaymentPlansHash
    {
        public static string Key = "paymentplans";

        public static class Fields
        {
            public static string PaymentPlansList(bool includeHiddenPlans, bool orderByRateDesc)
            {
                return "list:" + includeHiddenPlans.ToString() + ":" + orderByRateDesc.ToString();
            }

            public static string PaymentFrequenciesList()
            {
                return "freq:list";
            }

            public static string PaymentPlan(string name)
            {
                return "plan:" + name;
            }

            public static string PaymentFrequency(string name)
            {
                return "freq:" + name;
            }
        }
    }
}
