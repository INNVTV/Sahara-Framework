using API.WebhooksApi.TableEntities;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Logging.PlatformLogs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.WebhooksApi.Internal
{
    internal class StripeWebhookEventsLogManager
    {
        internal static string StripeWebhookEventsLogTableName = "stripewebhookeventslog";

        /// <summary>
        /// Returns true if event has been logged in the past
        /// </summary>
        /// <returns></returns>
        internal static bool HasEventBeenLogged(string eventId)
        {
            var stripeWebhookEventsLog = GetWebhookEvent(eventId);

            if (stripeWebhookEventsLog != null)
            {
                //Event exists and has been run by our webhook API, increase retry count by 1 and return true:

                try
                {
                    //Update retry count +1
                    stripeWebhookEventsLog.RetryCount = stripeWebhookEventsLog.RetryCount + 1;
                    TableOperation operation = TableOperation.InsertOrReplace(stripeWebhookEventsLog);

                    //Create the cloudtable instance and  name for the entity operate against:
                    var cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

                    //Create and set retry policy
                    //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
                    IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 5);
                    cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

                    stripeWebhookEventsLog.cloudTable = cloudTableClient.GetTableReference(StripeWebhookEventsLogManager.StripeWebhookEventsLogTableName);
                    stripeWebhookEventsLog.cloudTable.CreateIfNotExists();

                    stripeWebhookEventsLog.cloudTable.Execute(operation);
                }
                catch (Exception e)
                {
                    PlatformLogManager.LogActivity(
                            CategoryType.Error,
                            ActivityType.Error_Exception,
                            "An exception occurred while attempting to increment retry count on an existing stripe event log for idempotenancy",
                            "Incrementing retry count on stripe idempotent log for: " + stripeWebhookEventsLog.EventID,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            null,
                            JsonConvert.SerializeObject(e)
                        );
                }

                //return true:
                return true;
            }
            else
            {
                //Event does not exists, return false;
                return false;
            }
        }

        /* REVISIT FOR PURGE --
        internal static bool ClearStripeWebhookEventsLog(int amountOfDays)
        {

            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(StripeWebhookEventsLogTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<TableEntity> query = new TableQuery<TableEntity>()
                .Where(TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, DateTimeOffset.UtcNow.AddDays(amountOfDays).Date));

            var stripeWebhookEventsLog = cloudTable.ExecuteQuery(query);

            foreach (var log in stripeWebhookEventsLog)
            {
                cloudTable.Execute(TableOperation.Delete(log));
            }


            PlatformLogManager.LogActivity(
                CategoryType.StripeEvent,
                ActivityType.StripeEvent_IdempotentLogPurged,
                "Purge initiated for logs older than " + Math.Abs(amountOfDays) + " days. Resulted in " + stripeWebhookEventsLog.Count() + " purged items.",
                stripeWebhookEventsLog.Count() + " logs purged from events over " + Math.Abs(amountOfDays) + " days ago."
                );

            return true;
        }*/


        internal static bool LogWebhookEvent(string eventId)
        {

            var stripeWebhookEventsLog = new StripeWebhookEventsLogTableEntity(eventId);

            TableOperation operation = TableOperation.Insert((stripeWebhookEventsLog as TableEntity));

            try
            {
                stripeWebhookEventsLog.cloudTable.Execute(operation);
                return true;
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to store stripe webhook event log for idempotency purposes",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return false;
            }

        }

        #region Private Methods

        private static StripeWebhookEventsLogTableEntity GetWebhookEvent(string eventId)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(StripeWebhookEventsLogTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<StripeWebhookEventsLogTableEntity> query = new TableQuery<StripeWebhookEventsLogTableEntity>()
                .Where(TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, eventId),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, eventId)
                    ));

            StripeWebhookEventsLogTableEntity stripeWebhookEventsLog = cloudTable.ExecuteQuery(query).FirstOrDefault();

            return stripeWebhookEventsLog;

        }




        #endregion
    }
}
