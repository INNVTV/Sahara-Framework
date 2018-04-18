using System;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Platform.Custodian.Internal;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Platform.Custodian.TableEntities
{
    internal class CardExpirationEmailRemindersLogTableEntity : TableEntity
    {
        public CardExpirationEmailRemindersLogTableEntity()
        {
        }

        internal CardExpirationEmailRemindersLogTableEntity(string accountID, string accountName, DateTime cardExpirationDate, int timeLeftTillExpiration)
        {
            AccountID = accountID;
            AccountName = accountName;
            ReminderDays = timeLeftTillExpiration.ToString();

            CardExpirationDateUTC = cardExpirationDate;
            CardExpirationDateLocal = TimeZoneInfo.ConvertTimeFromUtc(cardExpirationDate, Sahara.Core.Settings.Application.LocalTimeZone);

            DateTimeUTC = DateTime.UtcNow;
            DateTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Sahara.Core.Settings.Application.LocalTimeZone);

            //Create the cloudtable instance and  name for the entity operate against:
            var cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 5);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            cloudTable = cloudTableClient.GetTableReference(CardExpirationReminderEmailsLogManager.ReminderEmailsCardExpirationLogTableName);
            cloudTable.CreateIfNotExists();
        }

        public string AccountID
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string ReminderDays
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string AccountName { get; set; }
        public DateTime CardExpirationDateUTC { get; set; }
        public DateTime CardExpirationDateLocal { get; set; }
        public DateTime DateTimeUTC { get; set; }
        public DateTime DateTimeLocal { get; set; }

        internal CloudTable cloudTable { get; set; }
    }
}
