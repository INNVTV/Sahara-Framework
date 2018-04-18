using Sahara.Core.Accounts.PaymentPlans.Public;
using Sahara.Core.Common.Methods;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.PaymentPlans.Internal
{
    internal static class PaymentPlanCaching
    {

        internal static TimeSpan DefaultCacheLength = TimeSpan.FromDays(1);

        public static void InvalidateAllCaches(bool flushCache = false)
        {

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            if (flushCache)
            {
                //Flush entire cache so that objects (like Accounts) that hold payment plan values MUST be recreated with new properties
                //var endpoints = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetEndPoints(true);
                //var server = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetServer(endpoints[0]);

                var endpoints = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetEndPoints(true);
                var server = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetServer(endpoints[0]);

                server.FlushAllDatabases();
            }
            else
            {
                try
                {
                    //Clear the entire Hash for PaymentPlans
                    cache.KeyDelete(PaymentPlansHash.Key, CommandFlags.FireAndForget);
                }
                catch
                {

                }

            }
           

        }

        public static void InvalidatePaymentPlanListCaches()
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.HashDelete(PaymentPlansHash.Key, PaymentPlansHash.Fields.PaymentPlansList(true, true));
                cache.HashDelete(PaymentPlansHash.Key, PaymentPlansHash.Fields.PaymentPlansList(false, false));
                cache.HashDelete(PaymentPlansHash.Key, PaymentPlansHash.Fields.PaymentPlansList(true, false));
                cache.HashDelete(PaymentPlansHash.Key, PaymentPlansHash.Fields.PaymentPlansList(false, true));
            }
            catch
            {

            }

            //Close the connection
            //con.Close();

        }

        public static void InvalidatePaymentFrequencyListCaches()
        {
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.HashDelete(PaymentPlansHash.Key, PaymentPlansHash.Fields.PaymentFrequenciesList());

                //Close the connection
                //con.Close();
            }
            catch
            {

            }
        }

    }
}
