using Microsoft.Azure.Documents.Client;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InventoryHawk.Account.Public.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            AreaRegistration.RegisterAllAreas();
            //GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            System.Net.ServicePointManager.DefaultConnectionLimit = 12 * Environment.ProcessorCount; //<-- Allows us to marshal up more SearchService/SearchIndex Clients to avoid exhausting sockets.


            //Initialize Local Environment
            EnvironmentSettings.CurrentEnvironment.Site = WebConfigurationManager.AppSettings["Environment"];

            //Require HTTPS if on stage or production:
            if (EnvironmentSettings.CurrentEnvironment.Site.ToLower() == "stage" || EnvironmentSettings.CurrentEnvironment.Site.ToLower() == "production" || EnvironmentSettings.CurrentEnvironment.Site.ToLower() == "release" || EnvironmentSettings.CurrentEnvironment.Site.ToLower() == "staging")
            {
                GlobalFilters.Filters.Add(new RequireHttpsAttribute());
            }

            //Initialize Shared Client Key for WCF Calls
            Common.SharedClientKey = ConfigurationManager.AppSettings["SharedClientKey"];

            #region Communicate with CoreServices and get static settings for this client to work with

            //Each client can get necessary settings and endpoint information from CoreServices for each enviornment.
            //This happens when the appliation is first initialized, and is stored in a static class for performance
            //If CoreServices are updated, a restart of each client may be necessary to get updated settings

            var platformSettingsServiceClient = new PlatformSettingsService.PlatformSettingsServiceClient(); // <-- We only use PlatformSettingsServiceClient in EnviornmentSettings because it is ONLY used at application startup:

            //Get & Initialize Settings from CoreServices:

            try
            {

                // PLATFORM SETTINGS SERVICE --------------------------------------

                platformSettingsServiceClient.Open();
                var platformSettingsResult = platformSettingsServiceClient.GetCorePlatformSettings(Common.SharedClientKey);



                CoreServices.PlatformSettings = platformSettingsResult;
                EnvironmentSettings.CurrentEnvironment.CoreServices = platformSettingsResult.Environment.Current;

                // CONFIGURE REDIS CONNECTIONS --------------
                // Because the  ConnectionMultiplexer  does a lot, it is designed to be shared and reused between callers.
                // You should not create a  ConnectionMultiplexer  per operation. It is fully thread-safe and ready for this usage.
                // In all the subsequent examples it will be assumed that you have a  ConnectionMultiplexer  instance stored away for re-use.
                CoreServices.RedisConnectionMultiplexers.RedisMultiplexer = ConnectionMultiplexer.Connect(platformSettingsResult.Redis.Unsecure);
                //CoreServices.RedisConnectionMultiplexers.PlatformManager_Multiplexer = ConnectionMultiplexer.Connect(platformSettingsResult.Redis.PlatformManager_Unsecure);
                //CoreServices.RedisConnectionMultiplexers.AccountManager_Multiplexer = ConnectionMultiplexer.Connect(platformSettingsResult.Redis.AccountManager_Unsecure);


                // CONFIGURE SEARCH CONNECTIONS ---------- (Moved to search partitions)
                /*
                CoreServices.SearchServiceQueryClient = new Microsoft.Azure.Search.SearchServiceClient(
                    platformSettingsResult.Search.SearchServiceName,
                    new Microsoft.Azure.Search.SearchCredentials(
                        platformSettingsResult.Search.ClientQueryKey
                        )
                );*/


                // CONFIGURE DOCUMENT DATABSE CONNECTIONS using READ ONLY keys -------------
                /* NOT USED BY API ANY LONGER - SEARCH ONLY
                ConnectionPolicy _connectionPolicy = new ConnectionPolicy
                {
                    //Since we are running within Azure we use Direct/TCP connections for performance.
                    //Web clients can alo use this.
                    //External clients like mobile phones that have ReadOnly Keys should use Gateway/Https
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp
                };

                CoreServices.DocumentDatabases.Accounts_DocumentClient = new Microsoft.Azure.Documents.Client.DocumentClient(
                    new Uri(platformSettingsResult.DocumentDB.AccountPartitionsReadOnlyAccountName),
                    platformSettingsResult.DocumentDB.AccountPartitionsReadOnlyAccountKey,
                    _connectionPolicy
                    );
                CoreServices.DocumentDatabases.Accounts_DocumentClient.OpenAsync();
                */

                //Close the connections
                WCFManager.CloseConnection(platformSettingsServiceClient); //<--Opens on init only


            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connections & manage the exceptions
                WCFManager.CloseConnection(platformSettingsServiceClient, exceptionMessage, currentMethodString);

                #endregion

                platformSettingsServiceClient.Close();
                EnvironmentSettings.CurrentEnvironment.CoreServices = "error: " + exceptionMessage;
            }

            #endregion


            //Force ALL results as JSON
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            //Register bundles last so options can be set by environment settings:
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
