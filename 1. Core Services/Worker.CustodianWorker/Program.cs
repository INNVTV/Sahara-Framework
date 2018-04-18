using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Platform.Initialization;

namespace Worker.CustodianWorker
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                #region Startup Tasks

                // 1.)   ------
                System.Net.ServicePointManager.DefaultConnectionLimit = 12 * Environment.ProcessorCount; //<-- Allows us to marshal up more SearchService/SearchIndex Clients to avoid exhausting sockets.

                #region Working w/ Service Fabric Enviornment Variables

                //Pull in enviornment variables: -----------------------------------------------------------
                //var hostId = Environment.GetEnvironmentVariable("Fabric_ApplicationHostId");
                //var appHostType = Environment.GetEnvironmentVariable("Fabric_ApplicationHostType");
                //var tyoeEndpoint = Environment.GetEnvironmentVariable("Fabric_Endpoint_[YourServiceName]TypeEndpoint");
                //var nodeName = Environment.GetEnvironmentVariable("Fabric_NodeName");

                //Or print them all out (Fabric Only):
                /*
                foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
                {
                    if (de.Key.ToString().StartsWith("Fabric"))
                    {
                        ServiceEventSource.Current.ServiceMessage(this.Context, " Environment variable {0} = {1}", de.Key, de.Value);
                    }
                }
                */

                //EVERY SINGLE ONE:
                /*
                foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, " Environment variable {0} = {1}", de.Key, de.Value);

                }*/

                #endregion


                // 2.)   ------
                var nodeName = Environment.GetEnvironmentVariable("Fabric_NodeName");

                // 3.)   ------
                //string environment = "production"; //RoleEnvironment.GetConfigurationSettingValue("Environment");
                string environment = Environment.GetEnvironmentVariable("Env").ToLower();
                Sahara.Core.Settings.Startup.Initialize(environment);

                Sahara.Core.Settings.Azure.CurrentRoleInstance.Name = nodeName; // RoleEnvironment.CurrentRoleInstance.Role.Name;
                //Sahara.Core.Settings.Azure.CurrentRoleInstance.Id = nodeName; //instanceIndex;

                // TODO: Track down use of Sahara.Core.Settings.Azure.CurrentRoleInstance.Id and replace with nodeName or anotehr var type


                //Trace.TraceInformation("Sahara.CoreServices.WcfEndpoints node:" + nodeName + " entry point called on env: " + environment);

                




                //Log Activity:
                PlatformLogManager.LogActivity(
                    CategoryType.Custodian,
                    ActivityType.Custodian_Status_Update,
                    "Custodian Worker starting on node '" + nodeName + "' (env: " + environment + ")....",
                    "WORKER.CustodialWorker entry point called on env: " + environment + " (node:" + nodeName + ") (env: " + environment + ")"
                );


                #endregion


                ServiceRuntime.RegisterServiceAsync("CustodianWorkerType",
                    context => new CustodianWorker(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(CustodianWorker).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
