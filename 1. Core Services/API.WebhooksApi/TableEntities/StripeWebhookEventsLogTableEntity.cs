using API.WebhooksApi.Internal;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.WebhooksApi.TableEntities
{
    /// <summary>
    /// Used to log that an event has been run to avoid running an event twice
    /// </summary>
    internal class StripeWebhookEventsLogTableEntity : TableEntity
    {
        public StripeWebhookEventsLogTableEntity()
        {

        }

        internal StripeWebhookEventsLogTableEntity(string eventId)//, string monthYear)
        {
            EventID = eventId;
            //MonthYear = monthYear; //<-- Format= "08-2015" (for August 2015) or "12-2014" (for December 2014) - used for 6 month cleanup by Custodian

            //Create the cloudtable instance and  name for the entity operate against:
            var cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            RowKey = eventId;
            RetryCount = 0;

            DateTimeUTC = DateTime.UtcNow;
            DateTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Sahara.Core.Settings.Application.LocalTimeZone);

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 5);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            cloudTable = cloudTableClient.GetTableReference(StripeWebhookEventsLogManager.StripeWebhookEventsLogTableName);
            cloudTable.CreateIfNotExists();
        }

        public string EventID
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        //public string MonthYear
        //{
        //get { return RowKey; }
        //set { RowKey = value; }
        //}

        public int RetryCount { get; set; }

        public DateTime DateTimeUTC { get; set; }
        public DateTime DateTimeLocal { get; set; }

        internal CloudTable cloudTable { get; set; }
    }
}
