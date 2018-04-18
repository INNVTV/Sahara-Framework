using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Settings.Accounts;
using Sahara.Core.Settings.Azure;
using Sahara.Core.Settings.Copy;
using Sahara.Core.Settings.Endpoints;
using Sahara.Core.Settings.Platform;
using Sahara.Core.Settings.Services;

namespace Sahara.Core.Settings
{
    public static class Startup
    {
        /// <summary>
        /// Called during startup by each shared project
        /// </summary>
        public static void Initialize(string environment)
        {

            //Initialize all static settings classes based on current environment
            //See "instructions.txt" for details  on the configurations to use per environment.
            //Host application must pass in the current environment name to initialize settings


            Environment.Current = environment;

           

            //---------------------------------------------------------------------

            //URLs & Email Addresses are initialized first as they are used in copy within other settings

            // Endpoint Settings ---------------
            URLs.Initialize();
            Emails.Initialize();


            //---------------------------------------------------------------------

            // Remaining settings are then initialized...

            // AZURE Services -----------------
            Databases.Initialize();
            DocumentDB.Initialize();
            Redis.Initialize();
            Storage.Initialize();
            Search.Initialize();

            // EXTERNAL Services ---------------
            Stripe.Initialize();
            SendGrid.Initialize();
            MailChimp.Initialize();
            GoogleMaps.Initialize();
            CloudFlare.Initialize();
            
            Registration.Initialize();
            GarbageCollection.Initialize();
            Custodian.Initialize();
            Worker.Initialize();
            Partitioning.Initialize();
            Payments.Initialize();

            // Platform Settings ---------------
            Application.Initialize();
            Platform.Users.Initialize();
            Accounts.Users.Initialize();
            Commerce.Credits.Initialize();

            //Copy Settings ---------------
            EmailMessages.Initialize();
            NotificationMessages.Initialize();
            PlatformMessages.Initialize();

            //Image Settings ------------
            Imaging.Images.Initialize();


            // Flush ALL Redis caches when we initialize CoreServices
            // This allows new updates to be forced into cached objects 
            var redisEndpoints = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetEndPoints(true);
            var redisserver = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetServer(redisEndpoints[0]);

            try
            {
                redisserver.FlushAllDatabases();
            }
            catch(Exception e)
            {
                string exception = e.Message;
            }
            

            /*
            // Flush ALL Redis caches when we initialize CoreServices
            // This allows new updates to be forced into cached objects 
            var accountsRedisEndpoints = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetEndPoints(true);
            var accountRedisserver = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetServer(accountsRedisEndpoints[0]);
            accountRedisserver.FlushAllDatabases();

            var platformRedisEndpoints = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetEndPoints(true);
            var platformRedisserver = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetServer(platformRedisEndpoints[0]);
            platformRedisserver.FlushAllDatabases();
            */


        }
    }
}
