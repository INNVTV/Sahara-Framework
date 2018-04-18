using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Sahara.Core.Platform.Initialization;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Platform.Snapshots.Public;
using Sahara.Core.Platform.Custodian;
using System.Diagnostics;
using System.Collections;

namespace Worker.CustodianWorker
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CustodianWorker : StatelessService
    {
        internal static string workerName = "Custodial Worker";
        //internal static int minutesToSleep = 15; //<--Switch to using local var in Sahara 2

        public CustodianWorker(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            

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
                
            }
*/

            #endregion

            //Check if platform is initialized
            bool platformInitialized = PlatformInitializationManager.isPlatformInitialized();

            //Tests/References ------
            ConfigurationPackage configPackage = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("config");
            var configurationSetting = configPackage.Settings.Sections["CustodialConfigSection"].Parameters["MyConfigSetting"].Value;
            string environmentVariable = Environment.GetEnvironmentVariable("MyEnvVariable");

            var nodeName = Environment.GetEnvironmentVariable("Fabric_NodeName");
            ServiceEventSource.Current.ServiceMessage(this.Context, String.Format("{0} waking up on {1}", workerName, nodeName));

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (platformInitialized)
                {
                    #region RUN TASKS

                    // Create new stopwatch
                    Stopwatch stopwatch = new Stopwatch();

                    // Begin timing tasks
                    stopwatch.Start();


                    #if DEBUG
                    //Trace.TraceInformation("Custodian Working...");
                    #endif

                    //Log
                    //PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Scheduled_Tasks_Started,
                    //"Starting Scheduled Tasks");


                    //========================================================================
                    //  TASK 1:   Deprovision Closed Accounts:
                    //========================================================================

                    var deprovisionClosedAccountsResponse = CustodianManager.DeprovisionClosedAccounts();
                    if (deprovisionClosedAccountsResponse.isSuccess)
                    {
                        //PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Scheduled_Task, "Task # 4 Complete (Deprovision Closed Accounts)");
                    }
                    else
                    {
                        PlatformLogManager.LogActivity(CategoryType.Error, ActivityType.Error_Custodian, "Task # 1 Had Errors (Deprovision Closed Accounts)", deprovisionClosedAccountsResponse.ErrorMessage);
                    }



                    //========================================================================
                    //  TASK 2:   Email Accounts Dunning Reminder(s) About Upcoming Credit Card Expiration Dtes:
                    //========================================================================

                    var creditCardExpirationRemindersResponse = CustodianManager.SendCreditCardExpirationReminders();
                    if (creditCardExpirationRemindersResponse.isSuccess)
                    {
                        //PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Scheduled_Task, "Task # 6 Complete (Credit Card Expiration Date Reminders)");
                    }
                    else
                    {
                        PlatformLogManager.LogActivity(CategoryType.Error, ActivityType.Error_Custodian, "Task # 2 Had Errors (Credit Card Expiration Date Reminders)", creditCardExpirationRemindersResponse.ErrorMessage);
                    }


                    //========================================================================
                    //  TASK 3:   Clean Up Credit Card Expirations Email Log
                    //========================================================================

                    var creditCardExpirationRemindersLogCleanupResponse = CustodianManager.ClearCreditCardExpirationRemindersLog();
                    if (creditCardExpirationRemindersLogCleanupResponse.isSuccess)
                    {
                        //PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Scheduled_Task, "Task # 7 Complete (Cleaned Up Credit Card Expiration Date Reminders Log )");
                    }
                    else
                    {
                        PlatformLogManager.LogActivity(CategoryType.Error, ActivityType.Error_Custodian, "Task # 3 Had Errors (Cleaned Up Credit Card Expiration Date Reminders Log )", creditCardExpirationRemindersLogCleanupResponse.ErrorMessage);

                    }



                    //========================================================================
                    //  TASK 4:   Clean up Intermediary Storage of Source Files
                    //========================================================================
                    var intermediaryCleanupResponse = CustodianManager.ClearIntermediaryStorage();
                    if (intermediaryCleanupResponse.isSuccess)
                    {
                        //PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Scheduled_Task, "Task # 8 Complete (Intermediary Storage Cleanup)");
                    }
                    else
                    {
                        PlatformLogManager.LogActivity(CategoryType.Error, ActivityType.Error_Custodian, "Task # 4 Had Errors (Log Cleanup)", intermediaryCleanupResponse.ErrorMessage);
                    }

                    //========================================================================
                    //  TASK 5:   Clean up Intermediary Storage of Source Files
                    //========================================================================
                    var stripeWebhookEventsCleanupResponse = CustodianManager.ClearStripeWebhooksLog();
                    if (stripeWebhookEventsCleanupResponse.isSuccess)
                    {
                        //PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Scheduled_Task, "Task # 8 Complete (Intermediary Storage Cleanup)");
                    }
                    else
                    {
                        PlatformLogManager.LogActivity(CategoryType.Error, ActivityType.Error_Custodian, "Task # 5 Had Errors (Log Cleanup)", stripeWebhookEventsCleanupResponse.ErrorMessage);
                    }


                    //========================================================================
                    //  TASK 6:   Ping sites to keep them awake for user traffic:
                    //========================================================================

                    var pingSitesResponse = CustodianManager.PingSites();

                    /*
                    if (pingSitesResponse.isSuccess)
                    {
                        PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Scheduled_Task, "Task # 6 Complete (Ping Sites)");
                    }
                    else
                    {
                        PlatformLogManager.LogActivity(CategoryType.Error, ActivityType.Error_Custodian, "Task # 6 Had Errors (Ping sites)", pingSitesResponse.ErrorMessage);
                    }
                    */

                    //========================================================================
                    //  TASK 7:   Load up subset of account data so it is cached and ready for API calls:
                    //========================================================================


                    var cacheAccountsResponse = CustodianManager.CacheAccountData();



                    //========================================================================
                    //  TASK 0:   Cleanup Logs:
                    //========================================================================

                    var logCleanupResponse = CustodianManager.ClearLogs();
                    if (logCleanupResponse.isSuccess)
                    {
                        //Not yet implemented - is it even necessary?
                        //PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Scheduled_Task, "Task # 0 Complete (Log Cleanup)");
                    }
                    else
                    {
                        PlatformLogManager.LogActivity(CategoryType.Error, ActivityType.Error_Custodian, "Task # 0 Had Errors (Log Cleanup)", logCleanupResponse.ErrorMessage);
                    }





                    //========================================================================
                    //  TASK 10:   Cleanup Source Images:
                    //========================================================================





                #if DEBUG
                    //Trace.TraceInformation("Tasks complete.");
                    //Trace.TraceInformation("Custodian sleeping for: " + Sahara.Core.Settings.Platform.Custodian.Frequency.Description);
                #endif

                    stopwatch.Stop();

                    //PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Scheduled_Tasks_Complete,
                    //"Scheduled Tasks Started & Completed in " + stopwatch.ElapsedMilliseconds.ToString("#,##0") + " milliseconds");

                    //Invalidate the accounts snapshot cache so platform admins see any account updates quicker:
                    PlatformSnapshotsManager.DestroyAccountSnapshotCache();

                    //Sleep for 4 seconds to put next_run log activity at the top:
                    //Thread.Sleep(4000); <-- No longer required since we are only logging one activity

                    //Sleep for 4 seconds to put next_run log activity at the top:
                    //Thread.Sleep(4000); <-- No longer required since we are only logging one activity

                    //Log
                    PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_Sleeping,
                        "Scheduled tasks complete. Sleeping for " + Sahara.Core.Settings.Platform.Custodian.Frequency.Description,
                        "Tasks completed in " + stopwatch.ElapsedMilliseconds.ToString("#,##0") + " milliseconds");

                    // PlatformLogManager.LogActivity(CategoryType.Custodian, ActivityType.Custodian_NextRun,
                    //"Custodian next run in " + Sahara.Core.Settings.Platform.Custodian.Frequency.Description,
                    //DateTime.UtcNow.AddMilliseconds(Sahara.Core.Settings.Platform.Custodian.Frequency.Length).ToString()
                    //);

                    #endregion

                }
                else
                {
                    //Check if platform is initialized again
                    platformInitialized = PlatformInitializationManager.isPlatformInitialized();
                }

                //Sleep
                Thread.Sleep(Sahara.Core.Settings.Platform.Custodian.Frequency.Length);
            }
        }
    }
}
