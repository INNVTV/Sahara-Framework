using System;
using System.Collections.Generic;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Common;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Common.Methods;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Common.Services.Stripe;
using Sahara.Core.Common.Validation;
using Sahara.Core.Common.ResponseTypes;
//using Sahara.Core.Infrastructure.Cache;
using Sahara.Core.Accounts.Internal;
using Sahara.Core.Accounts.PaymentPlans.Public;
using StackExchange.Redis;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Common.Redis;
using Newtonsoft.Json;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Common.MessageQueues.PlatformPipeline;
using Sahara.Core.Platform.Partitioning.Public;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using Sahara.Core.Accounts.DocumentModels;
using Stripe;
using Sahara.Core.Application.Search;
using Sahara.Core.Application.DocumentModels.Product;

namespace Sahara.Core.Accounts
{
    public static class AccountManager
    {

        #region Get Methods

        #region Get Account

        public enum AccountIdentificationType
        {
            Unknown,

            AccountID,
            AccountName,
            StripeCustomerID
        }

        /// <summary>
        /// accountAttribute can be an 'AccountNameKey', 'AccountID' or 'AccountName'
        /// The preferred method for getting an account is by AccountNameKey as this is the clustered index
        /// </summary>
        /// <param name="accountAttribute"></param>
        /// <param name="useCachedVersion"></param>
        /// <param name="accountIdentificationType"></param>
        /// <returns></returns>
        public static Account GetAccount(string accountAttribute, bool useCachedVersion = true, AccountIdentificationType accountIdentificationType = AccountIdentificationType.Unknown)
        {
            Account account = null;
            Account cachedAccount = null;

            if (accountIdentificationType == AccountIdentificationType.Unknown)
            {
                #region Detrmine if ID is an AccountID, a StripeCustomerID or an AccountName

                Guid accountId;

                string testCustStr = String.Empty;

                if (accountAttribute.Length >= 4)
                {
                    //Check to see if accountAttribute starts with "cus_":
                    testCustStr = accountAttribute.Substring(0, 4);
                }

                //Check to see if accountAttribute starts with "cus_":
                if (testCustStr == "cus_")
                {
                    //accountAttribute is a StripeCustomerID:
                    accountIdentificationType = AccountIdentificationType.StripeCustomerID;
                }
                else if (Guid.TryParse(accountAttribute, out accountId))
                {
                    //accountAttribute is an AccountID:
                    accountIdentificationType = AccountIdentificationType.AccountID;
                }
                else
                {
                    //accountAttribute is an AccountName:
                    accountIdentificationType = AccountIdentificationType.AccountName;
                }


                #endregion
            }

            if (useCachedVersion)
            {
                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();

                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                Object redisValue = null;

                //Retreive Account from cache based on the accountAttribute type
                try
                {
                    switch (accountIdentificationType)
                    {
                        case AccountIdentificationType.AccountName:
                            redisValue = cache.HashGet(
                                    AccountByNameHash.Key(accountAttribute),
                                    AccountByNameHash.Fields.Model
                                );
                            break;
                        case AccountIdentificationType.AccountID:
                            redisValue = cache.HashGet(
                                    AccountByIdHash.Key(accountAttribute),
                                    AccountByIdHash.Fields.Model
                                );
                            break;
                        case AccountIdentificationType.StripeCustomerID:
                            redisValue = cache.HashGet(
                                    AccountByStripeIdHash.Key(accountAttribute),
                                    AccountByStripeIdHash.Fields.Model
                                );
                            break;
                        default:
                            break;
                    }

                    if (((RedisValue)redisValue).HasValue)
                    {
                        cachedAccount = JsonConvert.DeserializeObject<Account>((RedisValue)redisValue);
                    }

                }
                catch (Exception e)
                {
                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to retrieve an account from the Redis cache",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        accountAttribute,
                        accountAttribute
                    );

                    //Set cached account to null
                    cachedAccount = null;
                }
            }

            if (cachedAccount == null)
            {
                //Account is not in cache, retreive it
                #region get account & store in cache (3 ways)

                //string idType = String.Empty;

                //Select the Account based on the accountAttribute type
                try
                {
                    switch (accountIdentificationType)
                    {
                        case AccountIdentificationType.AccountName:
                            account = Sql.Statements.SelectStatements.SelectAccountByNameKey(accountAttribute, true);
                            break;
                        case AccountIdentificationType.AccountID:
                            account = Sql.Statements.SelectStatements.SelectAccountByID(accountAttribute, true);
                            break;
                        case AccountIdentificationType.StripeCustomerID:
                            account = Sql.Statements.SelectStatements.SelectAccountByStripeCustomerID(accountAttribute, true);
                            break;
                        default:
                            break;
                    }

                }
                catch (Exception e)
                {
                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to retrieve an account from SQL",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        accountAttribute,
                        accountAttribute
                    );

                    //Set accont to null
                    account = null;
                }

                #endregion
            }
            else
            {
                //Account is in cache, cast returned object as Account and return:
                account = (Account)cachedAccount;
                return account;
            }

            if (account != null)
            {
                // Update the account Status using internal Update() method 
                account.Update();

                // Generate SelfLink References by Deserializing the "SelfLinkReferences" from "AccountProperties" collection back into SelfLinkReferencesDocumentModel object
                //We only do this with a fresh from the datbase version of the Account object. If the referecnes document needs to change then a purge of the Redi Cache must occur to force this
                //account.GenerateSelfLinkReferences();

                // Store account into the Cache
                AccountCaching.UpdateAccountInCache(account);

                //Return the account
                return account;
            }
            else
            {
                return null;
            }

        }

        #endregion

        #region Get Account Lists

        public static List<Account> GetAllAccounts(int pageNumber, int pageSize, string orderBy, bool useCachedVersion = true)
        {
            List<Account> cachedAccountList = null;

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            if (useCachedVersion)
            {
                try
                {
                    var redisValue = cache.HashGet(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Fields.All(pageNumber, pageSize, orderBy)
                        );

                    if (redisValue.HasValue)
                    {
                        cachedAccountList = JsonConvert.DeserializeObject<List<Account>>(redisValue);
                    }
                }
                catch
                {

                }

            }

            if (cachedAccountList == null)
            {
                List<Account> accounts = Sql.Statements.SelectStatements.SelectAllAccounts(pageNumber, pageSize, orderBy);

                try
                {
                    cache.HashSet(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Fields.All(pageNumber, pageSize, orderBy),
                        JsonConvert.SerializeObject(accounts),
                        When.Always,
                        CommandFlags.FireAndForget
                        );

                    cache.KeyExpire(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }


                return accounts;
            }
            else
            {
                return cachedAccountList;
            }

        }



        public static List<Account> GetAllAccountsByFilter(string columnName, string value, int pageNumber, int pageSize, string orderBy)  //<-- EXAMPLES: ("PaymentPlanId", "0/1/2/3/4/5...", "AccountNameKey Desc") ("Active", "0/1", "AccountNameKey Desc")  ("Verified", "0/1", "AccountNameKey Desc") ("Provisioned", "0/1", "AccountNameKey Desc")
        {
            List<Account> cachedAccountList = null;

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                var redisValue = cache.HashGet(
                       Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Key,
                       Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Fields.Filter(columnName, value, pageNumber, pageSize, orderBy)
                       ); 

                if (redisValue.HasValue)
                {
                    cachedAccountList = JsonConvert.DeserializeObject<List<Account>>(redisValue);
                }
            }
            catch
            {

            }


            if (cachedAccountList == null)
            {
                List<Account> accounts = Sql.Statements.SelectStatements.SelectAllAccounts(pageNumber, pageSize, columnName, value, orderBy);

                try
                {
                    cache.HashSet(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Fields.Filter(columnName, value, pageNumber, pageSize, orderBy),
                        JsonConvert.SerializeObject(accounts),
                        When.Always,
                        CommandFlags.FireAndForget
                        );

                    cache.KeyExpire(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                return accounts;
            }
            else
            {
                return cachedAccountList;
            }

        }


        public static List<Account> GetAllLockedAccounts()
        {
            //List<Account> accounts = Sql.Statements.SelectStatements.SelectAllAccounts("LockedDate Desc", "Locked", "1");
            List<Account> accounts = Sql.Statements.SelectStatements.SelectLockedAccounts();


            return accounts;
        }


        #endregion

        #region Get Single Property

        public static int GetAccountCount(string columnName = null, string value = null)
        {

            //Check the cache first
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            Object cachedAccountCount = null;

            try
            {
                if(columnName == null)
                {
                    cachedAccountCount = cache.HashGet(
                       Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Key,
                       Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Fields.CountAll()
                       );
                }
                else
                {
                    cachedAccountCount = cache.HashGet(
                       Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Key,
                       Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Fields.CountFilter(columnName, value)
                       );
                }
                
            }
            catch
            {

            }

            if (((RedisValue)cachedAccountCount).HasValue)
            {
                return JsonConvert.DeserializeObject<int>((RedisValue)cachedAccountCount);
            }
            else
            {
                int count = Sql.Statements.SelectStatements.SelectAccountCount(columnName, value);

                try
                {

                    //Store a copy in the cache
                    if (columnName == null)
                    {
                        cache.HashSet(
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Key,
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Fields.CountAll(),
                            JsonConvert.SerializeObject(count),
                            When.Always,
                            CommandFlags.FireAndForget
                            );
                    }
                    else
                    {
                        cache.HashSet(
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Key,
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Fields.CountFilter(columnName, value),
                            JsonConvert.SerializeObject(count),
                            When.Always,
                            CommandFlags.FireAndForget
                            );
                    }

                    cache.KeyExpire(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                return count;
            }
        }

        public static int GetAccountCountCreatedSince(DateTime sinceDateTime)
        {

            //Check the cache first
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            Object cachedAccountCount = null;

            try
            {

                cachedAccountCount = cache.HashGet(
                           Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Key,
                           Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Fields.CountSinceDateTime(sinceDateTime)
                           );
            }
            catch
            {

            }



            if (((RedisValue)cachedAccountCount).HasValue)
            {
                return JsonConvert.DeserializeObject<int>((RedisValue)cachedAccountCount);
            }
            else
            {
                int count = Sql.Statements.SelectStatements.SelectAccountCountCreatedSinceDateTime(sinceDateTime);

                try
                {

                    //Store a copy in the cache
                    cache.HashSet(
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Key,
                            Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Fields.CountSinceDateTime(sinceDateTime),
                            JsonConvert.SerializeObject(count),
                            When.Always,
                            CommandFlags.FireAndForget
                            );

                    cache.KeyExpire(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountCountsHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                return count;
            }
        }

        public static string GetAccountID(string accountName)
        {
            string nameKey = Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(accountName);

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            string cachedAccountId = null;

            try
            {

                cachedAccountId = cache.HashGet(
                    AccountByNameHash.Key(nameKey),
                    AccountByNameHash.Fields.AccountId);
            }
            catch
            {

            }

            //We cache this data forever as it never/rarely changes (a cache buster exists for name changes)
            //DataCache cacheForever = new DataCache(NamedCache.Forever);
            //object cachedAccountId = cacheForever.Get(AccountCacheID.AccountIDbyNameKey(nameKey));

            if (cachedAccountId != null)
            {
                return cachedAccountId;
            }
            else
            {
                string id = Sql.Statements.SelectStatements.SelectAccountIDByAcountNameKey(nameKey);

                try
                {
                    //Store a copy in the cache
                    //cacheForever.Put(AccountCacheID.AccountIDbyNameKey(nameKey), id);
                    cache.HashSet(
                        AccountByNameHash.Key(nameKey),
                        AccountByNameHash.Fields.AccountId,
                        id,
                        When.Always,
                        CommandFlags.FireAndForget
                    );
                }
                catch
                {

                }

                return id;
            }
        }

        public static string GetAccountName(string accountID)
        {
            //return Sql.Statements.SelectStatements.SelectAccountNameByID(accountID);

            //We cache this data forever as it never/rarely changes (a cache buster exists for name changes)
            //DataCache cacheForever = new DataCache(NamedCache.Forever);
            //object cachedAccountName = cacheForever.Get(AccountCacheID.AccountNameByID(accountID));

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            string cachedAccountName = null;

            try
            {
                cachedAccountName = cache.HashGet(
                    AccountByIdHash.Key(accountID),
                    AccountByIdHash.Fields.AccountName);
            }
            catch
            {

            }

            if (cachedAccountName != null)
            {
                return cachedAccountName;
            }
            else
            {
                string name = Sql.Statements.SelectStatements.SelectAccountNameByID(accountID);

                try
                {
                    //Store a copy in the cache
                    cache.HashSet(
                        AccountByIdHash.Key(accountID),
                        AccountByIdHash.Fields.AccountName,
                        name,
                        When.Always,
                        CommandFlags.FireAndForget
                    );
                }
                catch
                {

                }

                return name;
            }



        }

        public static string GetAccountNameKey(string accountID, bool useCachedVersion = true)
        {
            //return Sql.Statements.SelectStatements.SelectAccountNameKeyByID(accountID);

            //We cache this data forever as it never/rarely changes (a cache buster exists for name changes)
            //DataCache cacheForever = new DataCache(NamedCache.Forever);
            //object cachedAccountNameKey = null;

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            string cachedAccountNameKey = String.Empty;

            if (useCachedVersion)
            {
                try
                {
                    cachedAccountNameKey = cache.HashGet(
                        AccountByIdHash.Key(accountID),
                        AccountByIdHash.Fields.AccountNameKey
                    );
                }
                catch
                {

                }
            }

            if (!String.IsNullOrEmpty(cachedAccountNameKey))
            {
                return cachedAccountNameKey;
            }
            else
            {
                string nameKey = Sql.Statements.SelectStatements.SelectAccountNameKeyByID(accountID);

                try
                {
                    //Store a copy in the cache
                    cache.HashSet(
                        AccountByIdHash.Key(accountID),
                        AccountByIdHash.Fields.AccountNameKey,
                        nameKey,
                        When.Always,
                        CommandFlags.FireAndForget
                    );
                }
                catch
                {

                }

                return nameKey;
            }
        }


        #endregion

        #region Get Listed Properties

        /// <summary>
        /// Gets all owner(s) of an account
        /// Used when sending status emails to all account owners
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<String> GetAccountOwnerEmails(string id) //<-- AccountID or StripeCustomerID
        {
            List<String> cachedAccountOwnerEmails = null;

            //Check if this list has been cached in the last 15 minutes:
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                var redisValue = cache.HashGet(AccountInfoHash.Key(id), AccountInfoHash.Fields.OwnerEmailList); 
                if (redisValue.HasValue)
                {
                    cachedAccountOwnerEmails = JsonConvert.DeserializeObject<List<String>>(redisValue);
                }
            }
            catch
            {

            }


            if (cachedAccountOwnerEmails != null)
            {
                return cachedAccountOwnerEmails;
            }
            else
            {
                List<String> accountOwnerEmails = new List<string>();

                //check if it's a valid account id (GUID), if not then get use as a StripeCustomerID
                Guid accountID;// = new Guid();
                if (Guid.TryParse(id, out accountID))
                {
                    accountOwnerEmails = Sql.Statements.SelectStatements.SelectAllAccountOwnerEmailsByAccountID(id);
                }
                else
                {
                    accountOwnerEmails = Sql.Statements.SelectStatements.SelectAllAccountOwnerEmailsByStripeCustomerID(id);
                }


                try
                {
                    //Cache the data
                    cache.HashSet(
                        AccountInfoHash.Key(id),
                        AccountInfoHash.Fields.OwnerEmailList,
                        JsonConvert.SerializeObject(accountOwnerEmails),
                        When.Always,
                        CommandFlags.FireAndForget
                    );
                    //Set Expiration
                    cache.KeyExpire(
                        AccountInfoHash.Key(id),
                        AccountInfoHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                //Return Results:
                return accountOwnerEmails;
            }

        }

        public static List<String> GetAccountOwnerIds(string id) //<-- AccountID or StripeCustomerID
        {
            List<String> cachedAccountOwnerIDs = null;

            //Check if this list has been cached in the last 15 minutes:
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                var redisValue = cache.HashGet(AccountInfoHash.Key(id), AccountInfoHash.Fields.OwnerIdList); 
                if (redisValue.HasValue)
                {
                    cachedAccountOwnerIDs = JsonConvert.DeserializeObject<List<String>>(redisValue);
                }
            }
            catch
            {

            }

            if (cachedAccountOwnerIDs != null)
            {
                return cachedAccountOwnerIDs;
            }
            else
            {
                List<String> accountOwnerIDs = new List<string>();

                //check if it's a valid account id (GUID), if not then get use as a StripeCustomerID
                Guid accountID;// = new Guid();
                if (Guid.TryParse(id, out accountID))
                {
                    accountOwnerIDs = Sql.Statements.SelectStatements.SelectAllAccountOwnerIDsByAccountID(id);
                }
                else
                {
                    accountOwnerIDs = Sql.Statements.SelectStatements.SelectAllAccountOwnerIDsByStripeCustomerID(id);
                }

                //Cache the data
                //cacheLong.Put(AccountCacheID.AccountsOwnerIds(id), accountOwnerIDs);

                try
                {
                    //Cache the data
                    cache.HashSet(
                        AccountInfoHash.Key(id),
                        AccountInfoHash.Fields.OwnerIdList,
                        JsonConvert.SerializeObject(accountOwnerIDs),
                        When.Always,
                        CommandFlags.FireAndForget
                    );
                    //Set Expiration
                    cache.KeyExpire(
                        AccountInfoHash.Key(id),
                        AccountInfoHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                //Return Results:
                return accountOwnerIDs;
            }

        }

        public static List<String> GetAccountUserIds(string id) //<-- AccountID or StripeCustomerID
        {
            
            List<String> cachedAccountUserIDs = null;

            //Check if this list has been cached in the last 15 minutes:
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                var redisValue = cache.HashGet(AccountInfoHash.Key(id), AccountInfoHash.Fields.UserIdList); 
                if (redisValue.HasValue)
                {
                    cachedAccountUserIDs = JsonConvert.DeserializeObject<List<String>>(redisValue);
                }
            }
            catch
            {

            }

            if (cachedAccountUserIDs != null)
            {
                return cachedAccountUserIDs;
            }
            else
            {
                List<String> accountOwnerIDs = new List<string>();

                //check if it's a valid account id (GUID), if not then get use as a StripeCustomerID
                Guid accountID;// = new Guid();
                if (Guid.TryParse(id, out accountID))
                {
                    accountOwnerIDs = Sql.Statements.SelectStatements.SelectAllAccountUserIDsByAccountID(id);
                }
                else
                {
                    accountOwnerIDs = Sql.Statements.SelectStatements.SelectAllAccountUserIDsByStripeCustomerID(id);
                }

                //Cache the data
                //cacheLong.Put(AccountCacheID.AccountsUserIds(id), accountOwnerIDs);

                try
                {
                    //Cache the data
                    cache.HashSet(
                        AccountInfoHash.Key(id),
                        AccountInfoHash.Fields.UserIdList,
                        JsonConvert.SerializeObject(accountOwnerIDs),
                        When.Always,
                        CommandFlags.FireAndForget
                    );
                    //Set Expiration
                    cache.KeyExpire(
                        AccountInfoHash.Key(id),
                        AccountInfoHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                //Return Results:
                return accountOwnerIDs;
            }

        }

        #region used with bulk IDs in worker role

        public static List<String> GetUserIDsFromAllProvisionedAccounts(bool accountOwnersOnly)
        {
            return Sql.Statements.SelectStatements.SelectUserIDsFromAllProvisionedAccounts(accountOwnersOnly);
        }

        public static List<String> GetUserIDsFromProvisionedAccountsByFilter(bool accountOwnersOnly, string columnName, string columnValue)
        {
            return Sql.Statements.SelectStatements.SelectUserIDsFromAllProvisionedAccountsByFilter(columnName, columnValue, accountOwnersOnly);
        }

        #endregion

        #region used with bulk emails in worker role

        public static List<String> GetUserEmailsFromAllProvisionedAccounts(bool accountOwnersOnly)
        {
            return Sql.Statements.SelectStatements.SelectUserEmailsFromAllProvisionedAccounts(accountOwnersOnly);
        }

        public static List<String> GetUserEmailsFromProvisionedAccountsByFilter(bool accountOwnersOnly, string columnName, string columnValue)
        {
            return Sql.Statements.SelectStatements.SelectUserEmailsFromAllProvisionedAccountsByFilter(columnName, columnValue, accountOwnersOnly);
        }

        #endregion

        #endregion

        #endregion

        #region Search Methods

        public static List<Account> SearchAccounts(string query, string orderBy, int maxResults)
        {
            //Retreive Search Results from Cache:
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            List<Account> cachedAccountList = null;

            try
            {
                var redisValue = cache.HashGet(
                    Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Key,
                    Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Fields.Search(query, orderBy, maxResults));
                if (redisValue.HasValue)
                {
	                cachedAccountList = JsonConvert.DeserializeObject<List<Account>>(redisValue);
                }
            }
            catch
            {

            }

            if (cachedAccountList == null)
            {
                List<Account> accounts = Sql.Statements.SelectStatements.SearchAccounts(query, orderBy, maxResults);

                try
                {
                    cache.HashSet(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Fields.Search(query, orderBy, maxResults),
                        JsonConvert.SerializeObject(accounts),
                        When.Always,
                        CommandFlags.FireAndForget
                        );

                    cache.KeyExpire(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Key,
                        Sahara.Core.Common.Redis.PlatformManagerServer.Hashes.AccountListsHash.Expiration,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                return accounts;
            }
            else
            {
                return (List<Account>)cachedAccountList;
            }
        }


        #endregion

        #region Update Methods


        public static DataAccessResponseType UpdateAccountName(string accountID, string newAccountName)
        {
            var response = new DataAccessResponseType();

            //Is this just a casing update?
            string currentNameKey = GetAccountNameKey(accountID, false);
            string newNameKey = Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(newAccountName);

            if (currentNameKey == newNameKey)
            {
                //Names are the same, this is likely a Casing or modified spelling update. Bypass the name validation:
            }
            else
            {
                //This is no longer allowed, if name key changes we do not allow a name change

                response.isSuccess = false;
                response.ErrorMessage = "Account name can only be edited to adjust casing and spacing.";
                return response;

                /*
                //THis is a new name. Verify that name is available:
                var validation = ValidationManager.IsValidAccountName(newAccountName);
                if (!validation.isValid)
                {
                    response.isSuccess = false;
                    response.ErrorMessage = validation.validationMessage; //"This account name is either invalid or already in use.";
                    //response.ErrorMessages = validation.validationMessage

                    return response;
                }*/
            }


            //Make the update:
            response.isSuccess = Sql.Statements.UpdateStatements.UpdateAccountName(accountID, newAccountName);


            if (response.isSuccess)
            {
                //select a fresh version of the account with it's new name from database (will recache)
                //var account = Sql.Statements.SelectStatements.SelectAccountByID(accountID, true);
                var account = GetAccount(accountID, false, AccountIdentificationType.AccountID);

                //Add the new AccountNameKey to the results object
                response.Results = new List<string>();
                response.Results.Add(account.AccountNameKey);

                if (!String.IsNullOrEmpty(account.StripeCustomerID))
                {
                    //Update account name in Stripe (IF EXISTS) as well:
                    var stripeManager = new StripeManager();

                    //If account is associated with a Stripe customer id, update that to the new account name as well
                    stripeManager.UpdateCustomerName(account.StripeCustomerID, newAccountName);
                }


                //Update the Account Details Cache(s):
                AccountCaching.UpdateAccountInCache(account, true, currentNameKey);

                //Invalidate the previous cache using old name:
                AccountCaching.InvalidateAccountCacheByNameKey(currentNameKey);

                //Clear all caches within "accountListCacheName" named "AccountsList" so this name is updated in all listings and searches:
                //InvalidateCacheByKeyContents(accountListCacheName, "AccountsList");
                //CacheInspectionManager.InvalidateCacheByKeyContents(AccountCaching.AccountListCacheName, "AccountsList");

                //Invalidate all keys in this cache, may affect other cached items withn same time span. Faster than table scan method commented out above
                //CacheInspectionManager.InvalidateAllKeysInCache(AccountCaching.AccountListCacheName);
                AccountCaching.InvalidateAccountListsCache();


                //Clear cache of ALL users on the account:
                AccountUserCaching.ClearAllUserCaches(accountID);

            }

            return response;
        }


        public static DataAccessResponseType UpdateAccountActiveState(string accountId, bool activeState)
        {
            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.UpdateStatements.UpdateAccountActiveStatus(accountId, activeState);

            if (response.isSuccess)
            {

                //Invalidated/Update the cache for this account
                AccountCaching.UpdateAccountDetailCache(accountId);

                //Clear cache of ALL users on the account:
                AccountUserCaching.ClearAllUserCaches(accountId);

                //Clear all caches within "accountListCacheName" named "AccountsList" so this name is updated in all listings and searches:
                //InvalidateCacheByKeyContents(accountListCacheName, "AccountsList");
                //CacheInspectionManager.InvalidateCacheByKeyContents(AccountCaching.AccountListCacheName, "AccountsList");

                //Invalidate all keys in this cache, may affect other cached items withn same time span. Faster than table scan method commented out above
                //CacheInspectionManager.InvalidateAllKeysInCache(AccountCaching.AccountListCacheName);
                AccountCaching.InvalidateAccountListsCache();

                //Update Active State in Cache
                //AccountCaching.UpdateAccountActiveStateCache(accountId, activeState);

                //Update account name in Stripe (IF EXISTS) as well:
                //var stripeManager = new StripeManager();
                //var stripeCustomerID = Sql.Statements.SelectStatements.SelectStripeCustomerIDByAccountID(accountID);
                //if (stripeCustomerID != String.Empty)
                //{
                //If account is associated with a Stripe customer id, update that to the new account name as well
                //stripeManager.UpdateCustomerName(stripeCustomerID, newAccountName);
                //}
            }

            return response;
        }


        public static DataAccessResponseType UpdateAccountDelinquentStatus(string accountId, bool isDelinquent)
        {
            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.UpdateStatements.UpdateAccountDelinquentState(accountId, isDelinquent);

            if (response.isSuccess)
            {

                //Invalidated/Update the cache for this account
                AccountCaching.UpdateAccountDetailCache(accountId);

                //Clear cache of ALL users on the account:
                AccountUserCaching.ClearAllUserCaches(accountId);

                //Clear all caches within "accountListCacheName" named "AccountsList" so this name is updated in all listings and searches:
                //InvalidateCacheByKeyContents(accountListCacheName, "AccountsList");
                //CacheInspectionManager.InvalidateCacheByKeyContents(AccountCaching.AccountListCacheName, "AccountsList");

                //Invalidate all keys in this cache, may affect other cached items withn same time span. Faster than table scan method commented out above
                //CacheInspectionManager.InvalidateAllKeysInCache(AccountCaching.AccountListCacheName);
                AccountCaching.InvalidateAccountListsCache();

                //Update Delinquent State in Cache (DEPRICATED)
                //AccountCaching.UpdateAccountDelinquentStateCache(accountId, isDelinquent);

            }

            return response;
        }

        public static void UpdateAccountDetailCache(string id)
        {
            AccountCaching.UpdateAccountDetailCache(id);
        }

        public static void DestroyAccountCaches(string accountId, string accountNameKey, string stripeCustomerId)
        {
            try
            {
                AccountCaching.InvalidateAccountCacheById(accountId);
            }
            catch
            {

            }

            try
            {
                AccountCaching.InvalidateAccountCacheByNameKey(accountNameKey);
            }
            catch
            {

            }

            try
            {
                if (!String.IsNullOrEmpty(stripeCustomerId))
                {
                    AccountCaching.InvalidateAccountCacheByStripeId(stripeCustomerId);
                }
                
            }
            catch
            {

            }

            try
            {
                //Clear cache of ALL users on the account:
                AccountUserCaching.ClearAllUserCaches(accountId);
            }
            catch
            {

            }

            try
            {
                //Destroy API cache
                Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(accountNameKey);

            }
            catch
            {

            }

            AccountCaching.InvalidateAccountListsCache();
            
            
        }

        #endregion

        #region SEND EMAILS

        public static DataAccessResponseType SendEmailToBulkAccounts(string fromEmail, string fromName, string emailSubject, string emailMessage, bool accountOwnersOnly, bool isImportant, string columnName, string columnValue)
        {

            #region Validate Parameters

            if (String.IsNullOrEmpty(emailSubject))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include an email message." };
            }
            if (String.IsNullOrEmpty(emailMessage))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include an email subject." };
            }
            if (String.IsNullOrEmpty(fromEmail))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include a name that the email is from." };
            }
            if (String.IsNullOrEmpty(fromEmail))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include an email the message is from." };
            }

            #endregion

            try
            {
                //We offload this task for the Worker to process
                Sahara.Core.Common.MessageQueues.PlatformPipeline.PlatformQueuePipeline.SendMessage.SendEmailToBulkAccounts(fromEmail, fromName, emailSubject, emailMessage, accountOwnersOnly, isImportant, columnName, columnValue);

                return new DataAccessResponseType { isSuccess = true };

            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to email ALL accounts",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                //Return failure
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }

        }


        public static DataAccessResponseType SendEmailToAccount(string accountId, string fromEmail, string fromName, string emailSubject, string emailMessage, bool accountOwnersOnly = true, bool isImportant = false)
        {

            #region Validate Parameters

            if (String.IsNullOrEmpty(emailSubject))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include an email message." };
            }
            if (String.IsNullOrEmpty(emailMessage))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include an email subject." };
            }
            if (String.IsNullOrEmpty(fromEmail))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include a name that the email is from." };
            }
            if (String.IsNullOrEmpty(fromEmail))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include an email the message is from." };
            }
            if (String.IsNullOrEmpty(accountId))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include an accountId." };
            }

            #endregion

            try
            {
                var emails = Sql.Statements.SelectStatements.SelectUserEmailsFromAccount(accountId, accountOwnersOnly);
                
                EmailManager.Send(emails, fromEmail, fromName, emailSubject, emailMessage, true, isImportant);
                //AccountUserManager.SendEmailToUser(userId, fromEmail, fromName, emailSubject, emailMessage, isImportant); //<-- Since this could be delayed in worked role we have to be careful about users that may no longer exist when this runs.
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to send an email to a group of account users",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    accountId
                );

                //Return failure
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }

            return new DataAccessResponseType { isSuccess = true };

        }


        #endregion


        #region Subscriptions / Payment Plans / Credit Cards


        public static AccountCreditCardInfo GetAccountCreditCardInfo(string accountId)
        {

            #region Verify Incoming Parameters

            if (String.IsNullOrEmpty(accountId))
            {
                return null;
            }

            #endregion

            var cacheObject = Internal.AccountCreditCardInfoCaching.GetAccountCreditCardInfoCache(accountId);

            if (cacheObject != null)
            {
                //Object exists in cache, return it.
                return (AccountCreditCardInfo)cacheObject;
            }
            else
            {
                #region Get card info from Stripe and store into Cache

                var account = GetAccount(accountId);
                var stripeManager = new StripeManager();

                var cardInfo = stripeManager.GetCustomerDefaultCard(account.StripeCustomerID);

                if (cardInfo != null)
                {
                    var accountCreditCardInfo = new AccountCreditCardInfo { CardName = cardInfo.CardName, CardBrand = cardInfo.CardBrand, Last4 = cardInfo.Last4, ExpirationMonth = cardInfo.ExpirationMonth, ExpirationYear = cardInfo.ExpirationYear };

                    accountCreditCardInfo.ExpirationDescription = accountCreditCardInfo.ExpirationMonth + "/" + accountCreditCardInfo.ExpirationYear;
                    accountCreditCardInfo.CardDescription = accountCreditCardInfo.CardBrand + " - ****" + accountCreditCardInfo.Last4;

                    //Store into cache
                    Internal.AccountCreditCardInfoCaching.UpdateAccountCreditCardInfoCache(accountId, accountCreditCardInfo);

                    return accountCreditCardInfo;
                }

                return null;

                #endregion
            }

        }

        public static DataAccessResponseType AddUpdateCreditCard(string accountId, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear)
        {
            var response = new DataAccessResponseType();


            #region Verify Incoming Parameters

            if (String.IsNullOrEmpty(accountId))
            {
                response.isSuccess = false;
                response.ErrorMessage = "You must include the AccountID.";
                return response;
            }

            if (String.IsNullOrEmpty(cardNumber))
            {
                response.isSuccess = false;
                response.ErrorMessage = "You must include the Credit Card Number.";
                return response;
            }

            if (String.IsNullOrEmpty(cvc))
            {
                response.isSuccess = false;
                response.ErrorMessage = "You must include the CVC Number.";
                return response;
            }

            if (String.IsNullOrEmpty(expirationMonth))
            {
                response.isSuccess = false;
                response.ErrorMessage = "You must include the Expiration Month.";
                return response;
            }

            if (String.IsNullOrEmpty(expirationYear))
            {
                response.isSuccess = false;
                response.ErrorMessage = "You must include the Expiration Year.";
                return response;
            }

            if (String.IsNullOrEmpty(cardName))
            {
                response.isSuccess = false;
                response.ErrorMessage = "You must include the Name on the Card.";
                return response;
            }


            #endregion

            #region Verify & Get Account

            //GetAccount
            //var account = GetAccountByID(accountId, false);
            var account = GetAccount(accountId);
            if (account == null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "No such account exists.";
                return response;
            }
            #endregion

            var stripeManager = new StripeManager();

            if (account.StripeCustomerID == null)
            {
                //Customer does not have a customer ID on stripe, this is a first card. Let's create the customer:

                string stripeCustomerId = string.Empty;
                string stripeCardId = string.Empty;

                response = stripeManager.CreateCustomerAndCard(account.AccountID.ToString(), account.AccountName, cardName, cardNumber, cvc, expirationMonth, expirationYear, out stripeCustomerId, out stripeCardId);

                if(response.isSuccess)
                {
                    //Assign new stripe customer properties to customer:
                    var sqlResult = Sql.Statements.UpdateStatements.CreateAccountStripeCustomer(accountId, stripeCustomerId, stripeCardId);


                    //Email account owners about the change:
                    //Get account ownerEmail(s)
                    var ownerEmails = GetAccountOwnerEmails(accountId);

                    EmailManager.Send(
                    ownerEmails,
                    Settings.Endpoints.Emails.FromBilling,
                    Settings.Copy.EmailMessages.CreditCardAdded.FromName,
                    Settings.Copy.EmailMessages.CreditCardAdded.Subject,
                    String.Format(Settings.Copy.EmailMessages.CreditCardAdded.Body, account.AccountName),
                    true);
                }
                
            }
            else
            {
                string newStripeCardId = string.Empty;

                //Customer already exists. Replace the card:
                response = stripeManager.UpdateCustomerDefaultCreditCard(account.StripeCustomerID, cardName, cardNumber, cvc, expirationMonth, expirationYear, out newStripeCardId);

                //Assign new stripe card properties to customer:
                var sqlResult = Sql.Statements.UpdateStatements.UpdateAccountStripeCardID(accountId, newStripeCardId);

                if (response.isSuccess)
                {
                    //Email account owners about the change:
                    //Get account ownerEmail(s)
                    var ownerEmails = GetAccountOwnerEmails(accountId);

                    EmailManager.Send(
                    ownerEmails,
                    Settings.Endpoints.Emails.FromBilling,
                    Settings.Copy.EmailMessages.CreditCardUpdated.FromName,
                    Settings.Copy.EmailMessages.CreditCardUpdated.Subject,
                    String.Format(Settings.Copy.EmailMessages.CreditCardUpdated.Body, account.AccountName),
                    true);
                }
            }

            if (response.isSuccess)
            {
                //Invalidate the Credit Card Cache:
                Internal.AccountCreditCardInfoCaching.InvalidateAccountCreditCardInfoCache(accountId);

                try
                {
                    //Update credit card expiration for future dunning purposes.
                    Sql.Statements.UpdateStatements.UpdateAccountCreditCardExpirationDate(accountId, expirationMonth, expirationYear);

                    // Clear all caches for the account:
                    AccountCaching.InvalidateAccountCacheById(account.AccountID.ToString());
                    AccountCaching.InvalidateAccountCacheByNameKey(account.AccountNameKey);
                    AccountCaching.InvalidateAccountCacheByStripeId(account.StripeCustomerID);
                    //Update the cache:
                    AccountCaching.UpdateAccountDetailCache(account.AccountNameKey);

                    if(!account.Active || account.Delinquent || account.Closed || account.Locked)
                    {
                        // Retry past_due, unpaid and pending invoices on Stripe
                        //new StripeManager().RetryInvoices(account.StripeCustomerID);

                        //Send a message to the queue to retry unpaid invoices for this account
                        PlatformQueuePipeline.SendMessage.RetryUnpaidInvoices(account.StripeCustomerID);
                    }
                    

                }
                catch (Exception e)
                {
                    #region Manage Exception

                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "adding or updating a credit card",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        account.AccountID.ToString(),
                        account.AccountName
                    );

                    #endregion

                    response.isSuccess = false;
                    response.ErrorMessage = "An error occurred updating the credit card on the " + account.AccountName + " account.";
                    response.ErrorMessages[0] = e.Message;
                }

                return response;
            }
            else
            {
                if (response.isSuccess)
                {
                    //Invalidate/Update the cache:
                    AccountCaching.UpdateAccountDetailCache(account.AccountNameKey);
                }

                return response;
            }
            

        }

        public static DataAccessResponseType CreateSubscripton(string accountID, string planName, string frequencyMonths, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear)
        {
            var stripeResponse = new DataAccessResponseType();
            

            #region Verify Incoming Parameters

            if (String.IsNullOrEmpty(accountID))
            {
                return new DataAccessResponseType { ErrorMessage = "You must include the AccountID." };
            }

            if (String.IsNullOrEmpty(planName))
            {
                return new DataAccessResponseType { ErrorMessage = "You must include the plan name." };
            }

            if (String.IsNullOrEmpty(frequencyMonths))
            {
                return new DataAccessResponseType { ErrorMessage = "You must include the frequency in months." };
            }

            if (String.IsNullOrEmpty(cardName))
            {
                return new DataAccessResponseType { ErrorMessage = "You must include the Name on the Card." };
            }

            if (String.IsNullOrEmpty(cardNumber))
            {
                return new DataAccessResponseType { ErrorMessage = "You must include the Credit Card Number." };
            }

            if (String.IsNullOrEmpty(cvc))
            {
                return new DataAccessResponseType { ErrorMessage = "You must include the CVC Number." };
            }

            if (String.IsNullOrEmpty(expirationMonth))
            {
                return new DataAccessResponseType { ErrorMessage = "You must include the Expiration Month." };
            }

            if (String.IsNullOrEmpty(expirationYear))
            {
                return new DataAccessResponseType { ErrorMessage = "You must include the Expiration Year." };
            }




            #endregion

            #region Create & Verify Subscription Objects

            //GetPlan from local DB
            var paymentPlan = PaymentPlanManager.GetPaymentPlan(planName);
            if (paymentPlan == null)
            {
                return new DataAccessResponseType { ErrorMessage = "No such plan exists." };
            }

            //GetFrequency
            var paymentFrequency = PaymentPlanManager.GetPaymentFrequency(frequencyMonths);
            if (paymentFrequency == null)
            {
                return new DataAccessResponseType { ErrorMessage = "No such frequency exists." };
            }

            //GetAccount
            //var account = GetAccountByID(accountID, false);
            //var account = Sql.Statements.SelectStatements.SelectAccountByID(accountID, true);
            var account = GetAccount(accountID, false, AccountIdentificationType.AccountID);
            if (account == null)
            {
                return new DataAccessResponseType { ErrorMessage = "No such account exists." };
            }

            //Verify account has not already been subscribed to a plan (if it has use the Update method instead)
            if (account.StripeSubscriptionID != null)
            {
                return new DataAccessResponseType { ErrorMessage = "Account has already been subscried to a plan, please use the UpdateAccountPlan method instead." };
            }


            //Verify account does not already belong to this plan/frequency combo
            if (account.PaymentPlanName == paymentPlan.PaymentPlanName && account.PaymentFrequencyMonths == paymentFrequency.PaymentFrequencyMonths)
            {
                return new DataAccessResponseType { ErrorMessage = "Account already belongs to this plan and payment frequency." };
            }

            //Retrieve Plan from Stripe:
            var stripeManager = new StripeManager();
            var stripePlanID = Sahara.Core.Common.Methods.Billing.GenerateStripePlanID(paymentPlan.PaymentPlanName, paymentFrequency.IntervalCount, paymentFrequency.Interval);

            var stripePlanCheck = stripeManager.PlanExists(stripePlanID);

            if (!stripePlanCheck.isSuccess)
            {
                return new DataAccessResponseType { ErrorMessage = stripePlanCheck.ErrorMessage };
            }

            #endregion

            #region Validate that account is not scheduled to close

            if (account.AccountEndDate != null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Account is marked for closure" };
            }

            #endregion

            //Create subscription, customer & card on Stripe:

            string stripeCustomerId = string.Empty;
            string stripeSubscriptionId = string.Empty;
            string stripeCardId = string.Empty;

            if (account.StripeCustomerID == null && account.StripeSubscriptionID == null)
            {
                //Create the Customer, Subscription & Card objects on Stripe
                stripeResponse = stripeManager.CreateCustomerSubscriptionAndCard(account.AccountID.ToString(), account.AccountName, stripePlanID, cardName, cardNumber, cvc, expirationMonth, expirationYear, out stripeCustomerId, out stripeSubscriptionId, out stripeCardId);
            }
            else if (account.StripeCustomerID != null)
            {
                //Account already has a CustomerID, Just create the Subscription with a new Card
                stripeResponse = stripeManager.CreateSubscriptionAndCard(account.StripeCustomerID, account.AccountName, stripePlanID, cardName, cardNumber, cvc, expirationMonth, expirationYear, out stripeCustomerId, out stripeSubscriptionId, out stripeCardId);
            }


            if (stripeResponse.isSuccess)
            {

                bool sqlResult = false;

                try
                {
                    #region Make updates to SQL, Migrate to 'Shared' DoccumentCollectionTier & Send Communication to AccountUser(s) 

                    //Assign new account subscription properties to customer:
                    sqlResult = Sql.Statements.UpdateStatements.CreateAccountStripeSubscription(accountID, stripeCustomerId, stripeSubscriptionId, stripeCardId, planName, frequencyMonths);                

                    if (sqlResult)
                    {
                        #region Send Communication to Account Users, Invalidate Caches

                        //Email the owners regarding subscription creation:
                        //Get account ownerEmail(s)
                        var ownerEmails = AccountManager.GetAccountOwnerEmails(accountID);

                        EmailManager.Send(
                            ownerEmails,
                            Settings.Endpoints.Emails.FromSubscriptions,
                            Settings.Copy.EmailMessages.SubscriptionCreated.FromName,
                            Settings.Copy.EmailMessages.SubscriptionCreated.Subject,
                            String.Format(Settings.Copy.EmailMessages.SubscriptionCreated.Body, account.AccountName, paymentPlan.PaymentPlanName),
                            true);

                        try
                        {
                            //Store credit card expiration for future dunning purposes
                            Sql.Statements.UpdateStatements.UpdateAccountCreditCardExpirationDate(accountID, expirationMonth, expirationYear);
                        }
                        catch
                        {

                        }

                        //if the account is not active when a subscription is created we make it active
                        //MOVED TO PROVISIONING REQUEST
                        //if (!account.Active)
                        //{
                            //AccountManager.UpdateAccountActiveState(account.AccountID.ToString(), true);
                        //}

                        //Update the AccountCache with the very latest Account from the DB
                        //account = Sql.Statements.SelectStatements.SelectAccountByNameKey(account.AccountNameKey, true);
                        //account = GetAccount(account.AccountNameKey, false, AccountIdentificationType.AccountName);
                        // Update the account Status using internal Update() method 
                        //account.Update();

                        //Clear & Update all Caches that are for the Account
                        //AccountCaching.UpdateAccountInCache(account);

                        // Clear all caches for the account:
                        AccountCaching.InvalidateAccountCacheById(account.AccountID.ToString());
                        AccountCaching.InvalidateAccountCacheByNameKey(account.AccountNameKey);
                        AccountCaching.InvalidateAccountCacheByStripeId(account.StripeCustomerID);


                        PlatformLogManager.LogActivity(
                            CategoryType.Account,
                            ActivityType.Account_Subscribed,
                            account.AccountName + " subscribed to the " + planName + " plan",
                            "Plan: " + planName + " | Frequency: " + frequencyMonths + " months",
                            account.AccountID.ToString(),
                            account.AccountName
                        );

                        return new DataAccessResponseType { isSuccess = true };

                        #endregion                     
                    }
                    else
                    {
                        var errorResponse = new DataAccessResponseType { isSuccess = true };

                        #region Manage unknown SQL Failure

                        errorResponse.isSuccess = false;
                        errorResponse.ErrorMessage = "An unknown error occurred creating a new subscription while updating PaymentPlanName or StripeCustomerID on the " + account.AccountName + " account.";

                        //Log Error:
                        PlatformLogManager.LogActivity(CategoryType.Error,
                            ActivityType.Error_Other,
                            errorResponse.ErrorMessage,
                            "StripeCustomerID: " + stripeCustomerId + " | PlanName: " + paymentPlan.PaymentPlanName + " | Frequency: " + paymentFrequency.PaymentFrequencyMonths,
                            account.AccountID.ToString(),
                            account.AccountName,
                            null,
                            null,
                            null,
                            null,
                            System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name
                            );

                        //Send an alert to the platform admin(s):
                        /*EmailManager.Send(
                                Settings.Endpoints.Emails.PlatformEmailAddresses,
                                Settings.Endpoints.Emails.FromSubscriptions,
                                "New Subscriptions " + Sahara.Core.Settings.Application.Name,
                                errorResponse.ErrorMessage,
                                "<b>'" + errorResponse.ErrorMessage + "'</b><br/><br/>" +
                                "AccountID: '" + account.AccountID + "' StripeCustomerID: '" + stripeCustomerId + "' PlanName: '" + paymentPlan.PaymentPlanName + "' Frequency: '" + paymentFrequency.PaymentFrequencyMonths + "'",
                                true
                            );
                        */
                        #endregion

                        #region ROLLBACK Stripe updates

                        var stripeRollbackResponse = stripeManager.Rollback_CreateCustomerSubscriptionAndCard(stripeCustomerId);

                        if (!stripeRollbackResponse.isSuccess)
                        {
                            #region Alert Platform Admins if Rollback not successfull

                            //Log Error:
                            PlatformLogManager.LogActivity(CategoryType.Error,
                                ActivityType.Error_Other,
                                stripeRollbackResponse.ErrorMessage,
                                "Unable to rollback Stripe updates. StripeCustomerID: " + stripeCustomerId + " | PlanName: " + paymentPlan.PaymentPlanName + " | Frequency: " + paymentFrequency.PaymentFrequencyMonths,
                                account.AccountID.ToString(),
                                account.AccountName,
                                null,
                                null,
                                null,
                                null,
                                System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name
                                );

                            /*
                            EmailManager.Send(
                                    Settings.Endpoints.Emails.PlatformEmailAddresses,
                                    Settings.Endpoints.Emails.FromSubscriptions,
                                    "New Subscriptions " + Sahara.Core.Settings.Application.Name,
                                    "unable to rollback Stripe updates for account",
                                    "<b>'" + errorResponse.ErrorMessage + "'</b><br/><br/>" +
                                    "AccountID: '" + account.AccountID + "' StripeCustomerID: '" + stripeCustomerId + "' PlanName: '" + paymentPlan.PaymentPlanName + "' Frequency: '" + paymentFrequency.PaymentFrequencyMonths + "'",
                                    true
                                );
                             * */

                            #endregion                           
                        }

                        #endregion

                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "An error occured while attemping to update the database. All updates have been rolled back." };
                    }

                    #endregion

                }
                catch(Exception e)
                {
                    
                    #region Manage SQL Exception

                    string attemptedAction = "updating SQL with subscription info for StripeCustomerID: " + stripeCustomerId + " | PlanName: " + paymentPlan.PaymentPlanName + " | Frequency: '" + paymentFrequency.PaymentFrequencyMonths;

                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        attemptedAction,
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        account.AccountID.ToString(),
                        account.AccountName
                    );

                    #endregion

                    #region ROLLBACK Stripe updates

                    var stripeRollbackResponse = stripeManager.Rollback_CreateCustomerSubscriptionAndCard(stripeCustomerId);

                    if (!stripeRollbackResponse.isSuccess)
                    {
                        #region Log Error & Alert Platform Admins if Rollback not successfull

                        PlatformLogManager.LogActivity(
                            CategoryType.Error,
                            ActivityType.Error_Other,
                            "Unable to rollback Stripe updates using API after failed SQL updates",
                            attemptedAction,
                            account.AccountID.ToString(),
                            account.AccountName,
                            null,
                            null,
                            null,
                            null,
                            System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name
                        );


                        #endregion
                    }

                    #endregion

                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "An error occured while attemping to update the database. All updates have been rolled back." };
                }
                

            }
            else
            {

                return stripeResponse;
            }

        }

        public static DataAccessResponseType UpdateAccountPlan(string accountId, string newPlanName, string newPlanfrequencyMonths)
        {


            string updateType = string.Empty;
            var stripeResponse = new DataAccessResponseType();
            var response = new DataAccessResponseType();
            var stripeMetaData = new Dictionary<string, string>();

            #region Create & Verify Subscription Objects

            //GetPlan from local DB
            var paymentPlan = PaymentPlanManager.GetPaymentPlan(newPlanName);
            if (paymentPlan == null)
            {
                return new DataAccessResponseType { ErrorMessage = "No such plan exists." };
            }

            //GetFrequency
            var paymentFrequency = PaymentPlanManager.GetPaymentFrequency(newPlanfrequencyMonths);
            if (paymentFrequency == null)
            {
                return new DataAccessResponseType { ErrorMessage = "No such frequency exists." };
            }

            //GetAccount
            //var account = GetAccountByID(accountID, false);
            var account = Sql.Statements.SelectStatements.SelectAccountByID(accountId);
            if (account == null)
            {
                return new DataAccessResponseType { ErrorMessage = "No such account exists." };
            }

            var previousPlanName = account.PaymentPlanName;

            //Verify account has a StripeCustomerID,
            // is subscribed to a current payment plan
            // & is not marked for closure
            if (account.StripeCustomerID == null)
            {
                return new DataAccessResponseType { ErrorMessage = "Account has not been subscried to a plan, please use the CreateAccountSubscription method instead." };
            }


            if (account.AccountEndDate != null)
            {
                return new DataAccessResponseType { ErrorMessage = "Account is marked for closure, and cannot change plans." };
            }

            //Verify account does not already belong to this plan/frequency combo
            if (account.PaymentPlanName == paymentPlan.PaymentPlanName && account.PaymentFrequencyMonths == paymentFrequency.PaymentFrequencyMonths)
            {
                return new DataAccessResponseType { ErrorMessage = "Account already belongs to this plan and payment frequency." };
            }

            //Retrieve Plan from Stripe:
            var stripeManager = new StripeManager();
            var updatedStripePlanID = Sahara.Core.Common.Methods.Billing.GenerateStripePlanID(paymentPlan.PaymentPlanName, paymentFrequency.IntervalCount, paymentFrequency.Interval);
            //var cuurentStripePlanId = Sahara.Core.Common.Methods.PaymentPlans.GenerateStripePlanID(account.PaymentPlanName, account.PaymentFrequency.PaymentFrequencyName);

            var stripePlanCheck = stripeManager.PlanExists(updatedStripePlanID);

            if (!stripePlanCheck.isSuccess)
            {
                return new DataAccessResponseType { ErrorMessage = stripePlanCheck.ErrorMessage };
            }

            #endregion

            #region Validate that account is not scheduled to close

            if (account.AccountEndDate != null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Account is marked for closure, subscription cannot be updated." };
            }

            #endregion


            #region Determine updateType (upgrade, downgrade or frequnecy) 

            if (account.PaymentPlan.MonthlyRate < paymentPlan.MonthlyRate)
            {
                updateType = "upgrade";
                stripeMetaData.Add("TransactionDescription", "Upgraded plan to " + newPlanName);
            }
            else if (account.PaymentPlan.MonthlyRate > paymentPlan.MonthlyRate)
            {
                updateType = "downgrade";
                stripeMetaData.Add("TransactionDescription", "Downgraded plan to " + newPlanName);
            }
            else if (account.PaymentPlanName == paymentPlan.PaymentPlanName)
            {
                updateType = "frequency";
                stripeMetaData.Add("TransactionDescription", "Changed frequency to " + PaymentPlanManager.GetPaymentFrequency(newPlanfrequencyMonths).PaymentFrequencyName);
            }

            #endregion


            /* ============================================================
            DISALLOW DOWNGRADES AND LIMIT UPGRADES TO PLANS WITHIN THE SAME "SearchPlan" AS DATA MIGRATION IS NOT AUTOMATED
            ==============================================================*/

            //We do not allow downgrades by default. Remove this line if you want to allow downgrades
            if (updateType == "downgrade")
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Downgrades are not allowed" };
            }

            if (account.PaymentPlan.SearchPlan != paymentPlan.SearchPlan)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You can only upgrade to plans within the same search tier." };
            }




            #region Verify downgrade will not cause constraint issues

            if (updateType == "downgrade")
            {
                //LOGIC MOVED TO WCF DUE TO CIRCULAR REFERENCE ISSUES

                //var accountCapacity = AccountCapacityManager

                //if ( paymentPlan.MaxUsers)
                //{

                //}

                //var message = "Your have too many x, x and x to downgrade to this plan. Please remove";

            }

            // Downgrade account (Verify that user does not have limitations)
            //ToDo: Check if user is past lmitations on the downgraded account, if so return a message stating that they need to trim their data to go down in plan, or they can wipe their data and start fresh:
            //Check limitations, can accoun be downgraded? If not then return error message to user.
            //This checj can also be done by the front end.
            //response.isSuccess = false;
            //response.ErrorMessage = "You cannot downgrade your account without data loss. Please currate your account data and user count until you are below the maximum allowed by the new subscription plan.";
            //foreach into response.ErrorMessages
            //return response;

            #endregion

            //Update account on Stripe

            stripeResponse = stripeManager.UpdateCustomerSubscription(account.StripeCustomerID, account.StripeSubscriptionID, updatedStripePlanID, stripeMetaData);

            if (stripeResponse.isSuccess)
            {
                //Update IDs for account on SQL:

                bool sqlUpdate = Sql.Statements.UpdateStatements.UpdateAccountPaymentPlan(accountId, newPlanName, newPlanfrequencyMonths);

                if (!sqlUpdate)
                {
                    var errorResponse = new DataAccessResponseType { };

                    #region Manage SQL Errors
                    errorResponse.isSuccess = false;
                    errorResponse.ErrorMessage = "An error occurred creating a new subscription while updating PaymentPlanName or StripeCustomerID on the " + account.AccountName + " account.";

                    #region Log Issue

                    string errorDetails = "PlanName: " + paymentPlan.PaymentPlanName + " | Frequency: " + paymentFrequency.PaymentFrequencyMonths;

                    PlatformLogManager.LogActivity(
                        CategoryType.Error,
                        ActivityType.Error_Other,
                        errorResponse.ErrorMessage,
                        errorDetails,
                        account.AccountID.ToString(),
                        account.AccountName,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name
                    );

                    #endregion

                    #endregion

                    return errorResponse;
                }

                #region Send Communication to the Account user(s) And log the activity
                //Get account ownerEmail(s)
                var ownerEmails = AccountManager.GetAccountOwnerEmails(accountId);

                //Determine update type and send appopriate messages
                switch (updateType)
                {
                    case "upgrade":

                        //On upgrades we bill for the pro-rated amount remaining immediatly:
                        stripeManager.PayUpcomingInvoice(account.StripeCustomerID);
                        //We can choose NOT to do this and we will have a 1 hour delay between the time the Invoice is "Payed" and the final Charge is "Payed" so that we can add additional invoice line items when the webhook request comes through before the final charge is processed on that Invoice.

                        EmailManager.Send(
                            ownerEmails,
                            Settings.Endpoints.Emails.FromSubscriptions,
                            Settings.Copy.EmailMessages.SubscriptionUpgraded.FromName,
                            Settings.Copy.EmailMessages.SubscriptionUpgraded.Subject,
                            String.Format(Settings.Copy.EmailMessages.SubscriptionUpgraded.Body, account.AccountName, paymentPlan.PaymentPlanName),
                            true);


                        PlatformLogManager.LogActivity(
                                CategoryType.Account,
                                ActivityType.Account_Upgraded,
                                account.AccountName + " upgraded their payment plan",
                                account.AccountName + " upgraded to the " + paymentPlan.PaymentPlanName + " plan from the " + previousPlanName + " plan",
                                accountId,
                                account.AccountName
                            );

                        response = new DataAccessResponseType { isSuccess = true, SuccessMessage = "Account has been upgraded to " + paymentPlan.PaymentPlanName + ". You will be pro-rated any amount already payed during the current subscription cycle." };

                        break;

                    case "downgrade":

                        EmailManager.Send(
                            ownerEmails,
                            Settings.Endpoints.Emails.FromSubscriptions,
                            Settings.Copy.EmailMessages.SubscriptionDowngraded.FromName,
                            Settings.Copy.EmailMessages.SubscriptionDowngraded.Subject,
                            String.Format(Settings.Copy.EmailMessages.SubscriptionDowngraded.Body, account.AccountName, paymentPlan.PaymentPlanName),
                            true);

                        PlatformLogManager.LogActivity(
                                CategoryType.Account,
                                ActivityType.Account_Downgraded,
                                account.AccountName + " downgraded their payment plan",
                                account.AccountName + " downgraded to the " + paymentPlan.PaymentPlanName + " plan from the " + previousPlanName + " plan",
                                accountId,
                                account.AccountName
                            );

                        response = new DataAccessResponseType { isSuccess = true, SuccessMessage = "Account has been downgraded to " + paymentPlan.PaymentPlanName + ". You will be pro-rated any credits you have on your next billing date." };

                        break;

                    case "frequency":

                        EmailManager.Send(
                           ownerEmails,
                           Settings.Endpoints.Emails.FromSubscriptions,
                           Settings.Copy.EmailMessages.SubscriptionFrequencyChanged.FromName,
                           Settings.Copy.EmailMessages.SubscriptionFrequencyChanged.Subject,
                           String.Format(Settings.Copy.EmailMessages.SubscriptionFrequencyChanged.Body, account.AccountName, paymentFrequency.PaymentFrequencyName),
                           true);

                        response = new DataAccessResponseType { isSuccess = true, SuccessMessage = "Subscription frequency has been changed to " + paymentFrequency.PaymentFrequencyName + "." };

                        break;
                }

                #endregion

            }
            else
            {
                #region Manage Errors

                string errors = string.Empty;
                //Build Stripe Error String for Logs:
                foreach (string error in stripeResponse.ErrorMessages)
                {
                    errors += error + " | ";
                }

                //Log Error:
                PlatformLogManager.LogActivity(
                    CategoryType.Error,
                    ActivityType.Error_Other,
                    stripeResponse.ErrorMessage + " Stripe Errors: " + errors,
                    "StripeCustomerID: '" + account.StripeCustomerID + "' PlanName: '" + paymentPlan.PaymentPlanName + "' Frequency: '" + paymentFrequency.PaymentFrequencyMonths + "'", account.AccountID.ToString(), account.AccountName);

                //Send an alert to the platform admin(s):
                EmailManager.Send(
                        Settings.Endpoints.Emails.PlatformEmailAddresses,
                        Settings.Endpoints.Emails.FromSubscriptions,
                        "New Subscriptions " + Sahara.Core.Settings.Application.Name,
                        stripeResponse.ErrorMessage,
                        "<b>'" + stripeResponse.ErrorMessage + "'</b><br/><br/>" +
                        "Errors: " + errors + "<br/><br/>" +
                        "AccountID: '" + account.AccountID + "' StripeCustomerID: '" + account.StripeCustomerID + "' PlanName: '" + paymentPlan.PaymentPlanName + "' Frequency: '" + paymentFrequency.PaymentFrequencyMonths + "'",
                        true
                    );

                #endregion

                return stripeResponse;
            }


            //Invalidate/Update the cache:
            AccountCaching.UpdateAccountDetailCache(account.AccountNameKey);

            response.isSuccess = true;
            return response;


        }

        public static DataAccessResponseType ReactivateSubscription(Account account)
        {
            #region Validate that account is scheduled to close

            if (account.AccountEndDate == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Account is not marked for closure, subscription cannot be reactivated." };
            }

            #endregion

            var stripeResponse = new DataAccessResponseType();
            var response = new DataAccessResponseType();
            var stripeMetaData = new Dictionary<string, string>();

            stripeMetaData.Add("TransactionDescription", "Account plan reactivated");

            var stripeManager = new StripeManager();

            //Get the subscription from Stripe:
            StripeSubscription customerSubscription = null;

            try
            {
                customerSubscription = stripeManager.GetSubscription(account.StripeCustomerID, account.StripeSubscriptionID);
            }
            catch
            {

            }


            if (account.PaymentPlanName.ToLower() == "unprovisioned")
            {
                //This is not a subsscribes account and does not have a subscription plan, stripe can be ignored
                stripeResponse.isSuccess = true;
            }
            else if(customerSubscription != null)
            {
                //Subscription has not been cancelled - Update the plan to reactivate:          
                stripeResponse = stripeManager.ReactivateExistingSubscription(account.StripeCustomerID, account.StripeSubscriptionID, account.StripePlanID, stripeMetaData);
            }
            else
            {
                
                    //Subscription no longer exists. Create a new version of the plan/subscription to reactivate the account:  

                    //First get our card:
                    if (account.StripeCardID != null)
                    {
                        string stripeSubscriptionId = string.Empty;
                        stripeResponse = stripeManager.ReactivateClosedSubscription(account.StripeCustomerID, account.StripePlanID, stripeMetaData, out stripeSubscriptionId);

                        if (stripeResponse.isSuccess)
                        {
                            //Assign new SubscriptionID to account:
                            var sqlResult = Sql.Statements.UpdateStatements.UpdateAccountStripeSubscriptionID(account.AccountID.ToString(), stripeSubscriptionId);
                            if (!sqlResult)
                            {
                                #region Log Manual Task

                                PlatformLogManager.LogActivity(
                                    CategoryType.ManualTask,
                                    ActivityType.ManualTask_SQL,
                                    "Failed to update new subscription id during accoutn reactivation",
                                    "Please set 'StripeSubscriptionID' = '" + stripeSubscriptionId + "'",
                                    account.AccountID.ToString(),
                                    account.AccountName,
                                    null,
                                    null,
                                    null,
                                    null,
                                    System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name
                                );

                                #endregion
                            }
                        }

                    }
                    else
                    {
                        return new DataAccessResponseType { isSuccess = false, ErrorMessage = "No card on file. Account must be manually reactivated." };
                    }
                
            }

            
            if (stripeResponse.isSuccess)
            {
                //Update IDs for account on SQL:
                bool sqlUpdate = Sql.Statements.UpdateStatements.ReverseAccountClosure(account.AccountID.ToString());

                if (!sqlUpdate)
                {
                    var errorResponse = new DataAccessResponseType { };

                    #region Manage SQL Errors
                    errorResponse.isSuccess = false;
                    errorResponse.ErrorMessage = "An error occurred reactivating an existing subscription. The subscription is active - but some manual SQL updated have been logged";

                    //Invalidate/Update the cache:
                    AccountCaching.UpdateAccountDetailCache(account.AccountNameKey);

                    #region Log Manual Task

                    PlatformLogManager.LogActivity(
                        CategoryType.ManualTask,
                        ActivityType.ManualTask_SQL,
                        errorResponse.ErrorMessage,
                        "Please set 'AccountEndDate' = NULL and 'ClosureApproval' = FALSE",
                        account.AccountID.ToString(),
                        account.AccountName,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name
                    );

                    #endregion

                    #endregion

                    return errorResponse;
                }

                #region Send Communication to the Account user(s)
                //Get account ownerEmail(s)
                var ownerEmails = AccountManager.GetAccountOwnerEmails(account.AccountID.ToString());

                //Email account owners reagrding reactivation
                EmailManager.Send(
                            ownerEmails,
                            Settings.Endpoints.Emails.FromSubscriptions,
                            Settings.Copy.EmailMessages.SubscriptionReactivated.FromName,
                            Settings.Copy.EmailMessages.SubscriptionReactivated.Subject,
                            String.Format(Settings.Copy.EmailMessages.SubscriptionReactivated.Body, account.AccountName, account.PaymentPlan.PaymentPlanName),
                            true);

                response = new DataAccessResponseType { isSuccess = true, SuccessMessage = "Account has been reactivated!. Billing cycle will continue as before." };


                #endregion

            }
            else
            {
                #region Manage Errors

                string errors = string.Empty;
                //Build Stripe Error String for Logs:
                foreach (string error in stripeResponse.ErrorMessages)
                {
                    errors += error + " | ";
                }

                //Log Error:
                PlatformLogManager.LogActivity(
                    CategoryType.Error,
                    ActivityType.Error_Other,
                    stripeResponse.ErrorMessage + " Stripe Errors: " + errors,
                    "StripeCustomerID: '" + account.StripeCustomerID + "' PlanName: '" + account.PaymentPlan.PaymentPlanName + "' Frequency: '" + account.PaymentFrequencyMonths + "'", account.AccountID.ToString(), account.AccountName);

                //Send an alert to the platform admin(s):
                EmailManager.Send(
                        Settings.Endpoints.Emails.PlatformEmailAddresses,
                        Settings.Endpoints.Emails.FromSubscriptions,
                        "Reactivating Subscription for " + Sahara.Core.Settings.Application.Name,
                        stripeResponse.ErrorMessage,
                        "<b>'" + stripeResponse.ErrorMessage + "'</b><br/><br/>" +
                        "Errors: " + errors + "<br/><br/>" +
                        "AccountID: '" + account.AccountID + "' StripeCustomerID: '" + account.StripeCustomerID + "' PlanName: '" + account.PaymentPlan.PaymentPlanName + "' Frequency: '" + account.PaymentFrequencyMonths + "'",
                        true
                    );

                #endregion

                return stripeResponse;
            }


            //Invalidate/Update the cache:
            AccountCaching.UpdateAccountDetailCache(account.AccountNameKey);

            response.isSuccess = true;
            response.SuccessMessage = "Account subscription has been reactivated!";
            return response;
        }

        #endregion

        #region Close Account

        /// <summary>
        /// Used to close accounts that have registered, but have not set up a payment plan or have been provisioned
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static DataAccessResponseType CloseUnprovisionedAccount(Account account)
        {

            if (account.Provisioned || account.Active || !String.IsNullOrEmpty(account.StripeCustomerID))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Account cannot be closed in this manner as it has already been provisioned." };
            }

            var response = new DataAccessResponseType();

            
            response.isSuccess = Sql.Statements.DeleteStatements.DeleteUnprovisionedAccount(account.AccountID.ToString());


            if (response.isSuccess)
            {
                //Delete all users
                foreach (AccountUser user in account.Users)
                {
                    AccountUser outUser = null;
                    AccountUserManager.DeleteUser(user.Id, false, out outUser);
                }
                
                //Invalidate/Update the cache:
                AccountCaching.UpdateAccountDetailCache(account.AccountNameKey);
                DestroyAccountCaches(account.AccountID.ToString(), account.AccountNameKey, account.StripeCustomerID);
                
            }
            else
            {
                response.ErrorMessage = "An error occurred when updating the account end date.";
                return response;
            }

            response.isSuccess = true;
            response.SuccessMessage = "Unprovisioned account has been closed";

            return response;

        }

        /// <summary>
        /// Used by Account Holders to close an account (can also be used by Platform Admins)
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static DataAccessResponseType CloseAccount(Account account)
        {

            if(account.AccountEndDate != null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This account is already marked for closure" };
            }


            var response = new DataAccessResponseType();

            //var account = GetAccountByID(accountId, false);
            //var account = Sql.Statements.SelectStatements.SelectAccountByID(accountId, false);

            string closureDateDescription = String.Empty;

            // ToDo: Determine if account has a balance remaining before closure
            // Has a StripeCustomerID?

            DateTime accountEndDate;
            bool closureApproved = false; //<-- By default Platform Admins must approve a closure in order for Custodian to pick it up

            //if account has a StripeCustomerID and is not past due or delinquent: we cancel the subscription, and set AccountEndDate to start of next billing cycle
            if (!String.IsNullOrEmpty(account.StripeCustomerID) && account.Delinquent != true)
            {
                var stripeManager = new StripeManager();

                //if subscription
                if(!String.IsNullOrEmpty(account.StripeSubscriptionID))
                {
                    //Get next billing date from Stripe
                    accountEndDate = stripeManager.GetNextBillingDate(account.StripeCustomerID);

                    //Cancel the Stripe subscription
                    stripeManager.CancelSubscription(account.StripeCustomerID, account.StripeSubscriptionID);

                    string accountClosureDate = TimeZoneInfo.ConvertTimeFromUtc(accountEndDate, Settings.Application.LocalTimeZone).ToString("dddd") + " " +
                        TimeZoneInfo.ConvertTimeFromUtc(accountEndDate, Settings.Application.LocalTimeZone).ToString("M") + ", " + TimeZoneInfo.ConvertTimeFromUtc(accountEndDate, Settings.Application.LocalTimeZone).ToString("yyyy");

                    response.SuccessMessage = "Your account will be closed at the end of your next billing cycle on " + accountClosureDate + ". ";// +
                    //"We will email you when your account is closed.";

                    closureDateDescription = " at the end of the current billing cycle on: " + accountClosureDate;
                }
                else
                {
                    //We only end subscriptions here. Stripe customer data is purged during custodial deprovisioning of the account...

                    accountEndDate = DateTime.UtcNow; //<-- Since this is a FREE acccount we set the closure to occur now
                    response.SuccessMessage = "Your account has been marked for closure. All data and any remaining credits will be deleted."; // We will email you when this is complete (usually within " + Settings.Platform.Custodian.Frequency.Description + ")";
                    closureDateDescription = "within " + Sahara.Core.Settings.Platform.Custodian.Frequency.Description;
                }

            }
            else if (!String.IsNullOrEmpty(account.StripeCustomerID) && !String.IsNullOrEmpty(account.StripeSubscriptionID) && account.Delinquent == true)
            {
                //Account is past due or delinquent:, close immediatly, set AccountEndDate to NOW  - & - approve the closure automatically:
                closureApproved = true; //<-- Since this is a FREE acccount we approve the closure right away
                accountEndDate = DateTime.UtcNow; //<-- Since this is a DELINQUENT acccount we set the closure to occur now

                var stripeManager = new StripeManager();
                stripeManager.CancelSubscription(account.StripeCustomerID, account.StripeSubscriptionID);

                response.SuccessMessage = "Your account has been marked for closure. All data will be deleted."; // We will email you when this is complete (usually within " + Settings.Platform.Custodian.Frequency.Description + ")";

                closureDateDescription = "within " + Sahara.Core.Settings.Platform.Custodian.Frequency.Description;
            }
            else
            {
                //Account is not a paid account: close immediatly, set AccountEndDate to NOW  - & - approve the closure automatically:
                closureApproved = true; //<-- Since this is a FREE acccount we approve the closure right away
                accountEndDate = DateTime.UtcNow; //<-- Since this is an account not tied to a plan  we set the closure to occur now
                response.SuccessMessage = "Your account has been marked for closure. All data will be deleted."; // We will email you when this is complete (usually within " + Settings.Platform.Custodian.Frequency.Description + ")";

                closureDateDescription = "within " + Sahara.Core.Settings.Platform.Custodian.Frequency.Description;
            }

            response.isSuccess = Sql.Statements.UpdateStatements.UpdateAccountEndDate(account.AccountID.ToString(), accountEndDate, closureApproved);


            if (response.isSuccess)
            {
                //Invalidate/Update the cache:
                AccountCaching.UpdateAccountDetailCache(account.AccountNameKey);

            }
            else
            {
                response.ErrorMessage = "An error occurred when updating the account end date.";
                return response;
            }


            //Deactivate the account:
            //UpdateAccountActiveState(accountId, false);  //<-- We no longer do this, account remains active until the AccountEndDate is reached. (Active is reserved for billing and other issues)

            //Turn off billing, if account has a StripeCustomerID
            /*
            if(!String.IsNullOrEmpty(account.StripeCustomerID))
            {
                var stripeManager = new StripeManager();
                stripeManager.DeleteCustomer(account.StripeCustomerID);
            }
            */


            //Alert all account owners regarding closure:
            //Get account ownerEmail(s)
            var ownerEmails = AccountManager.GetAccountOwnerEmails(account.AccountID.ToString());

            if(!account.Delinquent)
            {
                //We only send this alert if an account is not delinquent so they have sometime to contact us to reverse a closure, otherwise they will just get the closure info email.
                EmailManager.Send(
                    ownerEmails,
                    Settings.Endpoints.Emails.FromSubscriptions,
                    Settings.Copy.EmailMessages.AccountClosureAlert.FromName,
                    Settings.Copy.EmailMessages.AccountClosureAlert.Subject,
                    String.Format(Settings.Copy.EmailMessages.AccountClosureAlert.Body, account.AccountName, closureDateDescription),
                    true);
            }


            response.isSuccess = true;



            return response;


        }



        /// <summary>
        /// Updates the account closure date to NOW. This bypasses the pro rated wait time on accounts and deprovisions all infrastructure assets on the next garbage collection round made by the Custodian
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static DataAccessResponseType AccelerateAccountClosure(Account account)
        {

            if (!account.ClosureApproved || account.AccountEndDate == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "This account closure cannot be accelerated. It is either not set to close, or closure has not yet been approved by a platform admin." };
            }

            var response = new DataAccessResponseType();


            response.isSuccess = Sql.Statements.UpdateStatements.UpdateAccountEndDate(account.AccountID.ToString(), DateTime.UtcNow, true);


            if (response.isSuccess)
            {
                //Invalidate/Update the cache:
                AccountCaching.UpdateAccountDetailCache(account.AccountNameKey);

            }
            else
            {
                response.ErrorMessage = "An error occurred when updating the account end date.";
                return response;
            }

            response.isSuccess = true;
            response.SuccessMessage = "Account closure has been accelerated";

            return response;

        }


        /// <summary>
        /// Used by platform admins to get a list of AccountIDs requiring closure approval
        /// </summary>
        /// <returns></returns>
        ///
        /*
        public static List<Account> GetAccountsForClosureApproval()
        {
           // var response = new DataAccessResponseType();

            return Sql.Statements.SelectStatements.SelectAccountsForClosureApproval();

        }*/

        /// <summary>
        /// Used by platform admins to get a list of AccountIDs pending closure and/or requiring closure approval
        /// </summary>
        /// <returns></returns>
        public static List<Account> GetAccountsPendingClosure()
        {
            // var response = new DataAccessResponseType();

            return Sql.Statements.SelectStatements.SelectAccountsPendingClosure();

        }

        public static bool DoesAccountRequireClosureApproval(string accountId)
        {
            var closureApproved = Sql.Statements.BoolStatements.SelectAccountClosureApproval(accountId);

            if (closureApproved)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Used by Platform Admins to approve/reverse approvla on accounts that have requested closure.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="isApproved"></param>
        /// <returns></returns>
        public static DataAccessResponseType UpdateAccountClosureApproval(string accountId, bool isApproved)
        {
            var response = new DataAccessResponseType();

            response.isSuccess = Sql.Statements.UpdateStatements.UpdateAccountClosureApproval(accountId, isApproved);

            return response;

        }

        #endregion
    }
}
 