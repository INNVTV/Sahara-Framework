using Newtonsoft.Json;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Common.Methods;
using Sahara.Core.Common.Redis;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Internal
{
    internal static class AccountCreditCardInfoCaching
    {

        public static AccountCreditCardInfo GetAccountCreditCardInfoCache(string accountId)
        {

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            AccountCreditCardInfo cachedCreditCardInfo = null;

            try
            {
                var redisValue = cache.HashGet(AccountByIdHash.Key(accountId), AccountByIdHash.Fields.CreditCard); 
                if (redisValue.HasValue)
                {
                    cachedCreditCardInfo = JsonConvert.DeserializeObject<AccountCreditCardInfo>(redisValue);
                }
            }
            catch
            {

            }

            return cachedCreditCardInfo;
        }


        public static void UpdateAccountCreditCardInfoCache(string accountId, AccountCreditCardInfo accountCreditCardInfo)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.HashSet(
                    AccountByIdHash.Key(accountId),
                    AccountByIdHash.Fields.CreditCard,
                    JsonConvert.SerializeObject(accountCreditCardInfo),
                    When.Always,
                    CommandFlags.FireAndForget
                    );

                // Set or extend expiration
                cache.KeyExpire(AccountByStripeIdHash.Key(accountId), AccountByStripeIdHash.Expiration);
            }
            catch
            {

            }
        }

        public static void InvalidateAccountCreditCardInfoCache(string accountId)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.HashDelete(
                    AccountByIdHash.Key(accountId),
                    AccountByIdHash.Fields.CreditCard,
                    CommandFlags.FireAndForget
                    );
            }
            catch
            {

            }
        }
    }
}
