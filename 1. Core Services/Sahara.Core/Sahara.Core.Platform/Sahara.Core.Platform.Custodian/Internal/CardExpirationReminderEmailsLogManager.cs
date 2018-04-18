using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Accounts;
using Sahara.Core.Platform.Custodian.TableEntities;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;

namespace Sahara.Core.Platform.Custodian.Internal
{
    internal class CardExpirationReminderEmailsLogManager
    {
        internal static string ReminderEmailsCardExpirationLogTableName = "cardexpirationemailreminderslog";

        /// <summary>
        /// Returns true if reminder email for this account has already been sent for this reminder date
        /// </summary>
        /// <returns></returns>
        internal static bool HasEmailBeenSent(string accountID, int daysTillExpiration)
        {
            if(GetReminderEmailForCardExpiration(accountID, daysTillExpiration) != null)
            {
                //Reminder exists, user has alreadey been sent this email, return true;
                return true;
            }
            else
            {
                //Reminder does not exists, log immediatly to ensure duplicates arn't sent by other custodian instances & return false;

                //var account = AccountManager.GetAccountByID(accountID, false);
                var account = AccountManager.GetAccount(accountID);

                StoreReminderEmailForCardExpiration(accountID, account.AccountName, account.CardExpiration, daysTillExpiration);

                return false;
            }
        }

        internal static bool ClearReminderEmailsLog(int amountOfDays)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(ReminderEmailsCardExpirationLogTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<CardExpirationEmailRemindersLogTableEntity> query = new TableQuery<CardExpirationEmailRemindersLogTableEntity>()
                .Where(TableQuery.GenerateFilterConditionForDate("DateTimeUTC", QueryComparisons.LessThanOrEqual, DateTimeOffset.UtcNow.AddDays(amountOfDays * -1)));

            var cardExirationReminderEmails = cloudTable.ExecuteQuery(query);

            int count = cardExirationReminderEmails.Count();

            foreach (var log in cardExirationReminderEmails)
            {
                cloudTable.Execute(TableOperation.Delete(log));
            }

            if (count > 0)
            {

                //Log Garbage Collection
                    PlatformLogManager.LogActivity(
                        CategoryType.GarbageCollection,
                        ActivityType.GarbageCollection_CreditCardExpirationRemindersLog,
                        "Purged " + count.ToString("#,##0") + " item(s) from the credit card expiration reminders logs",
                        count.ToString("#,##0") + " credit card expiration reminder(s) past " + Sahara.Core.Settings.Platform.GarbageCollection.CreditCardExpirationReminderEmailsLogDaysToPurge + " days have been purged"
                        );
            }

            return true;
        }

        #region Private Methods

        private static CardExpirationEmailRemindersLogTableEntity GetReminderEmailForCardExpiration(string accountID, int daysTillExpiration)
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(ReminderEmailsCardExpirationLogTableName);

            cloudTable.CreateIfNotExists();

            TableQuery<CardExpirationEmailRemindersLogTableEntity> query = new TableQuery<CardExpirationEmailRemindersLogTableEntity>()
                .Where(TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, accountID),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, daysTillExpiration.ToString())
                    ));

            CardExpirationEmailRemindersLogTableEntity cardExirationReminderEmailLog = cloudTable.ExecuteQuery(query).FirstOrDefault();

            return cardExirationReminderEmailLog;

        }

        private static bool StoreReminderEmailForCardExpiration(string accountID, string accountName, DateTime CardExirationDate, int daysTillExpiration)
        {

            var cardExirationReminderEmailLog = new CardExpirationEmailRemindersLogTableEntity(accountID, accountName, CardExirationDate, daysTillExpiration);

            TableOperation operation = TableOperation.Insert((cardExirationReminderEmailLog as TableEntity));

            try
            {
                cardExirationReminderEmailLog.cloudTable.Execute(operation);
                return true;
            }
            catch(Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to store reminder email for card expiration to table storage",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    accountID,
                    accountName
                );

                return false;
            }

        }



        #endregion
    }
}
