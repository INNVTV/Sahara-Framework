using Newtonsoft.Json;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Commerce.Public;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Platform.Billing;
using Sahara.Core.Platform.Snapshots.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Snapshots.Public
{
    public static class PlatformSnapshotsManager
    {
        public static AccountsSnapshot GetAccountsSnapshot()
        {
            var accountsSnapshot = new AccountsSnapshot();

            #region Get from Redis Cache

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            Object cachedAccountsSnapshot = null;

            try
            {
                cachedAccountsSnapshot = cache.HashGet(
                           Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Key,
                           Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Fields.Accounts
                           );

                if (((RedisValue)cachedAccountsSnapshot).HasValue)
                {
                    accountsSnapshot = JsonConvert.DeserializeObject<AccountsSnapshot>((RedisValue)cachedAccountsSnapshot);
                }
            }
            catch
            {

            }


            #endregion


            #region Generate Snapshot

            if (((RedisValue)cachedAccountsSnapshot).IsNullOrEmpty)
            {
                #region Generate Account Snapshot

                accountsSnapshot.PastDue = new List<Accounts.Models.Account>();
                accountsSnapshot.Unpaid = new List<Accounts.Models.Account>();


                //Assign Counts
                accountsSnapshot.Counts = new PlatformSnapshotAccountCounts();


                //Account #'s
                accountsSnapshot.Counts.Total = AccountManager.GetAccountCount();

                accountsSnapshot.Counts.Unprovisioned = AccountManager.GetAccountCount("PaymentPlanName", "Unprovisioned");
                accountsSnapshot.Counts.Inactive = AccountManager.GetAccountCount("Active", "0");

                accountsSnapshot.Counts.Delinquent = AccountManager.GetAccountCount("Delinquent", "1");
                if (accountsSnapshot.Counts.Delinquent > 0)
                {
                    var delinquentAccounts = AccountManager.GetAllAccountsByFilter("Delinquent", "1", 0, 0, "Delinquent Desc");
                    //var unpaidAccounts = AccountManager.GetAllAccountsByFilter("Delinquent", "1", 0, 0, "Delinquent Desc");

                    foreach (Account account in delinquentAccounts)
                    {
                        if (account.Active == false)
                        {
                            //Account is Delinquent AND no longer active
                            accountsSnapshot.Counts.Unpaid++;
                            accountsSnapshot.Unpaid.Add(account);
                        }
                        else
                        {
                            //Account is just Delinquent
                            accountsSnapshot.Counts.PastDue++;
                            accountsSnapshot.PastDue.Add(account);
                        }
                    }
                }

                accountsSnapshot.Counts.Subscribed = accountsSnapshot.Counts.Total - accountsSnapshot.Counts.Unprovisioned;

                //remove delinquent (unpaid and past_due) accounts from subscribed number to get the PaidUP amount:
                accountsSnapshot.Counts.PaidUp = accountsSnapshot.Counts.Subscribed - accountsSnapshot.Counts.Delinquent;


                //Signups since counts ------------------------------------------------------------------
                accountsSnapshot.Counts.Signups_Last24Hours = AccountManager.GetAccountCountCreatedSince(DateTime.UtcNow.AddDays(-1));
                accountsSnapshot.Counts.Signups_Last3Days = AccountManager.GetAccountCountCreatedSince(DateTime.UtcNow.AddDays(-3));
                accountsSnapshot.Counts.Signups_Last7Days = AccountManager.GetAccountCountCreatedSince(DateTime.UtcNow.AddDays(-7));
                accountsSnapshot.Counts.Signups_Last30Days = AccountManager.GetAccountCountCreatedSince(DateTime.UtcNow.AddDays(-30));

                //User #'s -----------------------------------------------------------------------
                //accountsSnapshot.Counts.GlobalUsersCount = AccountUserManager.GetUserCount();


                //Accounts scheduled for closure and/or requiring closure approval
                var accountsPendingClosure = AccountManager.GetAccountsPendingClosure();

                accountsSnapshot.RequiresClosureApproval = new List<Account>();
                accountsSnapshot.ScheduledForClosure = new List<Account>();
                foreach (var account in accountsPendingClosure)
                {

                    if (!account.ClosureApproved)
                    {
                        accountsSnapshot.RequiresClosureApproval.Add(account);
                    }
                    else
                    {
                        accountsSnapshot.ScheduledForClosure.Add(account);
                    }
                }

                //Latest registrations
                accountsSnapshot.LatestRegistered = new List<Account>();
                var registrationLogs = PlatformLogManager.GetPlatformLogByActivity(Logging.PlatformLogs.Types.ActivityType.Account_Registered, 10);

                foreach (var log in registrationLogs)
                {
                    var account = AccountManager.GetAccount(log.AccountID);

                    if (account == null)
                    {
                        //If the account no longer exists we create an account object with the name along with NULL accountId to indiicate that the account is likely purged, deprovisioned or closed.
                        account = new Account();
                        account.AccountName = log.AccountName;
                        account.CreatedDate = log.Timestamp.DateTime;
                    }

                    accountsSnapshot.LatestRegistered.Add(account);
                }


                //Account closures
                accountsSnapshot.LatestClosures = new List<AccountClosure>();
                var closureLogs = PlatformLogManager.GetPlatformLogByActivity(Logging.PlatformLogs.Types.ActivityType.Account_Closed, 10);

                foreach (var log in closureLogs)
                {
                    var account = AccountManager.GetAccount(log.AccountID);

                    if (account == null)
                    {
                        //If the account no longer exists we create an account object with the name along with NULL accountId to indiicate that the account is likely purged, deprovisioned or closed.
                        account = new Account();
                        account.AccountStatus = AccountStatus.Closed;
                        account.AccountName = log.AccountName;
                        account.CreatedDate = log.Timestamp.DateTime;
                    }

                    accountsSnapshot.LatestClosures.Add(new AccountClosure { Account = account, Decription = log.Description, Timestamp = log.Timestamp.DateTime });
                }





                #region Store in Redis

                try
                {
                    //Store a copy in the Redis cache
                    cache.HashSet(
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Key,
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Fields.Accounts,
                            JsonConvert.SerializeObject(accountsSnapshot),
                            When.Always,
                            CommandFlags.FireAndForget
                            );

                    //Expire cache after set time
                    cache.KeyExpire(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                #endregion


                #endregion
            }


            #endregion

            return accountsSnapshot;
        }

        /// <summary>
        /// Used to clear the snapshot cache when major account updates occur.
        /// </summary>
        public static void DestroyAccountSnapshotCache()
        {
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                cache.HashDelete(
                    Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Key,
                    Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Fields.Accounts,
                    CommandFlags.FireAndForget
                );
            }
            catch
            {

            }
        }

        public static InfrastructureSnapshot GetInfrastructureSnapshot()
        {
            var infrastructureSnapshot = new InfrastructureSnapshot();

            #region Get from Redis Cache

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            Object cachedInfrastructureSnapshot = null;

            try
            {
                cachedInfrastructureSnapshot = cache.HashGet(
                           Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Key,
                           Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Fields.Infrastructure
                           );

                if (((RedisValue)cachedInfrastructureSnapshot).HasValue)
                {
                    infrastructureSnapshot = JsonConvert.DeserializeObject<InfrastructureSnapshot>((RedisValue)cachedInfrastructureSnapshot);
                }
            }
            catch
            {

            }


            #endregion


            #region Generate Snapshot

            if (((RedisValue)cachedInfrastructureSnapshot).IsNullOrEmpty)
            {
                
                #region Generate Infrastructure Snapshot

                //Latest error data:
                infrastructureSnapshot.Errors_Log = PlatformLogManager.GetPlatformLogByCategory(Logging.PlatformLogs.Types.CategoryType.Error, 30);
                infrastructureSnapshot.Errors_Last7Days = false;

                foreach (var log in infrastructureSnapshot.Errors_Log)
                {
                    if (log.Timestamp >= DateTime.UtcNow.AddDays(-1))
                    {
                        infrastructureSnapshot.Errors_Last24Hours = true;
                    }
                    if (log.Timestamp >= DateTime.UtcNow.AddDays(-3))
                    {
                        infrastructureSnapshot.Errors_Last3Days = true;
                    }
                    if (log.Timestamp >= DateTime.UtcNow.AddDays(-7))
                    {
                        infrastructureSnapshot.Errors_Last7Days = true;
                    }
                    if (log.Timestamp >= DateTime.UtcNow.AddDays(-30))
                    {
                        infrastructureSnapshot.Errors_Last30Days = true;
                    }
                }

                //Latest billing issue data:
                /*
                billingSnapshotIssues = new List<PlatformSnapshotBillingIssue>();
                var billingIssuesLogs = PlatformLogManager.GetPlatformLogByActivity(Logging.PlatformLogs.Types.ActivityType.Billing_Issue, 10);

                foreach (var log in billingIssuesLogs)
                {
                    billingSnapshotIssues.Add(
                        new PlatformSnapshotBillingIssue
                        {
                            Decription = log.Description,
                            Details = log.Details,
                            Timestamp = log.Timestamp.DateTime
                        });
                }*/


                //Custodian info:
                infrastructureSnapshot.Custodian = new CustodianSnapshot();
                var custodianWorkerLogs = PlatformLogManager.GetPlatformLogByCategory(Logging.PlatformLogs.Types.CategoryType.Custodian, 1);

                try
                {
                    if (custodianWorkerLogs[0].Activity == Sahara.Core.Logging.PlatformLogs.Types.ActivityType.Custodian_Sleeping.ToString())
                    {
                        infrastructureSnapshot.Custodian.IsSleeping = true;
                        infrastructureSnapshot.Custodian.IsRunning = false;

                        infrastructureSnapshot.Custodian.LastRun = custodianWorkerLogs[0].Timestamp.UtcDateTime;
                        infrastructureSnapshot.Custodian.NextRun = custodianWorkerLogs[0].Timestamp.UtcDateTime.AddMilliseconds(Sahara.Core.Settings.Platform.Custodian.Frequency.Length);
                    }
                    else
                    {
                        infrastructureSnapshot.Custodian.IsSleeping = false;
                        infrastructureSnapshot.Custodian.IsRunning = true;

                        infrastructureSnapshot.Custodian.LastRun = DateTime.UtcNow;
                        infrastructureSnapshot.Custodian.NextRun = DateTime.UtcNow.AddMilliseconds(Sahara.Core.Settings.Platform.Custodian.Frequency.Length);

                    }
                }
                catch
                {

                }


                infrastructureSnapshot.Custodian.FrequencyMilliseconds = Sahara.Core.Settings.Platform.Custodian.Frequency.Length;
                infrastructureSnapshot.Custodian.FrequencyDescription = Sahara.Core.Settings.Platform.Custodian.Frequency.Description;


                #region Store in Redis

                try
                {
                    //Store a copy in the Redis cache
                    cache.HashSet(
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Key,
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Fields.Infrastructure,
                            JsonConvert.SerializeObject(infrastructureSnapshot),
                            When.Always,
                            CommandFlags.FireAndForget
                            );

                    //Expire cache after set time
                    cache.KeyExpire(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                #endregion

                #endregion
            }



            #endregion


            return infrastructureSnapshot;
        }

        public static BillingSnapshot GetBillingSnapshot()
        {
            var  billingSnapshot = new BillingSnapshot();

            #region Generate Billing Snapshot

            #region Get from Redis Cache

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            Object cachedBillingSnapshot = null;

            try
            {
                cachedBillingSnapshot = cache.HashGet(
                           Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Key,
                           Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Fields.Billing
                           );

                if (((RedisValue)cachedBillingSnapshot).HasValue)
                {
                    billingSnapshot = JsonConvert.DeserializeObject<BillingSnapshot>((RedisValue)cachedBillingSnapshot);
                }
            }
            catch
            {

            }


            #endregion

            #region Generate Snapshot

            if (((RedisValue)cachedBillingSnapshot).IsNullOrEmpty)
            {
                billingSnapshot = new BillingSnapshot();

                billingSnapshot.CreditsInCirculation = AccountCreditsManager.GetCreditsInCirculation();
                billingSnapshot.CreditsInCirculationDollarAmount = Sahara.Core.Common.Methods.Commerce.ConvertCreditsAmountToDollars(billingSnapshot.CreditsInCirculation);

                billingSnapshot.Balance = PlatformBillingManager.GetBalance();

                billingSnapshot.UpcomingTransfers = new List<Billing.Models.Transfer>();
                billingSnapshot.LatestTransfers = new List<Billing.Models.Transfer>();

                try
                {
                    var topTransfers = PlatformBillingManager.GetTransferHistory(10);

                    foreach(var transfer in topTransfers)
                    {
                        if (transfer.Status == "pending")
                        {
                            billingSnapshot.UpcomingTransfers.Add(transfer);
                        }
                        else if (transfer.Status == "paid" && billingSnapshot.LatestTransfers.Count < 2) //<-- We only show the latest 2 available transfers
                        {
                            billingSnapshot.LatestTransfers.Add(transfer);
                        }
                    }

                    //We reverse the upcoming list so the latest transfers show up first
                    billingSnapshot.UpcomingTransfers.Reverse();
                }
                catch
                {
                    billingSnapshot.LatestTransfers = null;
                    billingSnapshot.UpcomingTransfers = null;
                }

                #region Store in Redis

                try
                {
                    //Store a copy in the Redis cache
                    cache.HashSet(
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Key,
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Fields.Billing,
                            JsonConvert.SerializeObject(billingSnapshot),
                            When.Always,
                            CommandFlags.FireAndForget
                            );

                    //Expire cache after set time
                    cache.KeyExpire(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.SnapshotsHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                #endregion
            }


            #endregion

            #endregion

            return billingSnapshot;
        }
    }
}
