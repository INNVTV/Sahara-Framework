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
    internal static class AccountCaching
    {
        internal static void UpdateAccountInCache(Account account, bool includeNameIdReferences = false, string nameReferenceToClear = null)
        {
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {  
                cache.HashSet(
                    AccountByIdHash.Key(account.AccountID.ToString()),
                    AccountByIdHash.Fields.Model,
                    JsonConvert.SerializeObject(account),
                    When.Always,
                    CommandFlags.FireAndForget);

                // Set Expiration
                cache.KeyExpire(AccountByIdHash.Key(account.AccountID.ToString()), AccountByIdHash.Expiration);

                cache.HashSet(
                    AccountByNameHash.Key(account.AccountName),
                    AccountByNameHash.Fields.Model,
                    JsonConvert.SerializeObject(account),
                    When.Always,
                    CommandFlags.FireAndForget);
                // Set Expiration
                cache.KeyExpire(AccountByNameHash.Key(account.AccountID.ToString()), AccountByNameHash.Expiration);

                if (!String.IsNullOrEmpty(account.StripeCustomerID))
                {
                    cache.HashSet(
                        AccountByStripeIdHash.Key(account.StripeCustomerID),
                        AccountByStripeIdHash.Fields.Model,
                        JsonConvert.SerializeObject(account),
                        When.Always,
                        CommandFlags.FireAndForget);
                    // Set Expiration
                    cache.KeyExpire(AccountByStripeIdHash.Key(account.AccountID.ToString()), AccountByStripeIdHash.Expiration);
                }

                if (includeNameIdReferences)
                {
                    //Used when account name is changed

                    cache.HashSet(
                        AccountByIdHash.Key(account.AccountID.ToString()),
                        AccountByIdHash.Fields.AccountNameKey,
                        account.AccountNameKey,
                        When.Always,
                        CommandFlags.FireAndForget
                    );

                    cache.HashSet(
                        AccountByIdHash.Key(account.AccountID.ToString()),
                        AccountByIdHash.Fields.AccountName,
                        account.AccountName,
                        When.Always,
                        CommandFlags.FireAndForget
                    );

                    cache.HashSet(
                        AccountByNameHash.Key(account.AccountName),
                        AccountByNameHash.Fields.AccountId,
                        account.AccountID.ToString(),
                        When.Always,
                        CommandFlags.FireAndForget
                    );

                }
                if (nameReferenceToClear != null)
                {
                    cache.KeyDelete(
                    AccountByNameHash.Key(nameReferenceToClear),
                    CommandFlags.FireAndForget
                    );
                }

            }
            catch
            {

            }

        }


        internal static void InvalidateAccountListsCache()
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.KeyDelete(
                    Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Key,
                    CommandFlags.FireAndForget
                    );
            }
            catch
            {

            }
        }


        public static void InvalidateAccountCacheById(string id)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.KeyDelete(
                    AccountByIdHash.Key(id),
                    CommandFlags.FireAndForget
                    );
            }
            catch
            {

            }
        }
        public static void InvalidateAccountCacheByNameKey(string nameKey)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.KeyDelete(
                    AccountByNameHash.Key(nameKey),
                    CommandFlags.FireAndForget
                    );
            }
            catch
            {

            }
        }
        public static void InvalidateAccountCacheByStripeId(string stripeId)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.KeyDelete(
                    AccountByStripeIdHash.Key(stripeId),
                    CommandFlags.FireAndForget
                    );
            }
            catch
            {

            }
        }



        public static Account UpdateAccountDetailCache(string accountAttribute)
        {
            //select a fresh version of the account from database (will re-cache)
            return AccountManager.GetAccount(accountAttribute, false);
        }


        /* DEPRICATED
        public static void InvaldateAccountActiveStateCache(string id)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();

            cache.HashDelete(
                AccountByIdHash.Key(id),
                AccountByIdHash.Fields.Active,
                CommandFlags.FireAndForget
                );
        }

        public static void UpdateAccountActiveStateCache(string id, bool isActive)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();

            cache.HashSet(
                AccountByIdHash.Key(id),
                AccountByIdHash.Fields.Active,
                JsonConvert.SerializeObject(isActive),
                When.Always,
                CommandFlags.FireAndForget);
            // Set Expiration
            cache.KeyExpire(AccountByIdHash.Key(id), AccountByNameHash.Expiration);

        }

        public static void UpdateAccountDelinquentStateCache(string id, bool isDelinquent)
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();

            cache.HashSet(
                AccountByIdHash.Key(id),
                AccountByIdHash.Fields.Delinquent,
                JsonConvert.SerializeObject(isDelinquent),
                When.Always,
                CommandFlags.FireAndForget);
            // Set Expiration
            cache.KeyExpire(AccountByIdHash.Key(id), AccountByNameHash.Expiration);

        }*/



    }
}
