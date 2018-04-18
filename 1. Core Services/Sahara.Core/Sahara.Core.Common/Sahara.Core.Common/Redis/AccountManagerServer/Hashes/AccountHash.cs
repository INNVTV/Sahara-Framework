using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Redis.AccountManagerServer.Hashes
{
    public static class AccountByIdHash
    {
        public static TimeSpan Expiration = TimeSpan.FromHours(80);

        public static string Key(string accountId)
        {
            return "accountbyid:" + accountId.Replace("-", "").ToLower();
        }

        public static class Fields
        {
            public static string Model = "model";

            public static string Users = "users";
            public static string Invitations = "invitations";
            public static string Owners = "owners";

            public static string CreditCard = "creditcard";

            //public static string Active = "active"; // Depricated
            //public static string Delinquent = "delinquent"; // Depricated

            public static string AccountName = "accountname";
            public static string AccountNameKey = "accountnameshort";
            
        }
    }

    public static class AccountByNameHash
    {
        public static TimeSpan Expiration = TimeSpan.FromHours(80);

        public static string Key(string accountName)
        {
            string accountNameKey = Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(accountName);

            return "accountbyname:" + accountNameKey;
        }

        public static class Fields
        {
            public static string Model = "model";
            public static string AccountId = "accountid";
        }
    }

    public static class AccountByStripeIdHash
    {
        public static TimeSpan Expiration = TimeSpan.FromHours(80);

        public static string Key(string stripeCustomerId)
        {
            return "accountbystripeid:" + stripeCustomerId;
        }

        public static class Fields
        {
            public static string Model = "model";
        }
    }

    public static class AccountInfoHash
    {
        public static TimeSpan Expiration = TimeSpan.FromMinutes(24);

        public static string Key(string accountIdOrStripeId)
        {
            return "accountinfo:" + accountIdOrStripeId;
        }

        public static class Fields
        {
            public static string UserIdList = "useridlist";
            public static string OwnerIdList = "owneridlist";
            public static string OwnerEmailList = "owneremaillist";
        }
    }
}
