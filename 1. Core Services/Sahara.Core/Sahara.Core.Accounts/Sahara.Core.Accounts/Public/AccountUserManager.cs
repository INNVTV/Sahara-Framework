using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Sahara.Core.Accounts.DataContext;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts.Internal;
using Sahara.Core.Accounts.TableEntities;
using Sahara.Core.Common;
using Sahara.Core.Common.Methods;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Common.Validation;
using Sahara.Core.Common.ResponseTypes;
using StackExchange.Redis;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Sahara.Core.Common.Redis;
using Sahara.Core.Common.Redis.AccountManagerServer.Strings;
using Newtonsoft.Json;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Logging.PlatformLogs;

namespace Sahara.Core.Accounts
{
    public static class AccountUserManager
    {
        


        #region Authentication & Claims

        /// <summary>
        /// Used to log users into client applications
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static DataAccessResponseType GetUserWithLogin(string userName, string password)
        {
            var response = new DataAccessResponseType();

            try
            {
                var userManager = new UserManager<AccountUserIdentity>(
                    new UserStore<AccountUserIdentity>(new AccountUserIdentityDbContext()));

                AccountUserIdentity user = userManager.Find(userName, password);

                if (user != null)
                {
                    response.isSuccess = true;
                    response.ResponseObject = user; //<-- ResponseObject can be converted to AccountUser by consuming application

                    //Store non-dentity version of user in Cache
                    AccountUserManager.GetUserIdentity(user.Id.ToString());

                    return response;
                }
                else
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "Invalid login, please try again.";

                    return response;
                }


            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "log a user into their account. UserName: " + userName,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );


                response.isSuccess = false;
                response.ErrorMessage = e.Message;

                return response;
            }


        }

        // For MVC/WebForms based web applications:
        public static ClaimsIdentity GetUserClaimsIdentity(AccountUserIdentity user, string authenticationType)
        {
            try
            {
                var userManager = new UserManager<AccountUserIdentity>(
                        new UserStore<AccountUserIdentity>(new AccountUserIdentityDbContext()));

                //return userManager.CreateIdentityAsync(user, authenticationType).Result;
                return userManager.CreateIdentity(user, authenticationType);

            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to get a user claim identity for UserID: " + user.Id + " of Account: " + user.AccountName,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return null;
            }
        }


        #endregion

        #region Invitations

        /// <summary>
        /// Used by account admin to invite users, users will then varify and create there password
        /// </summary>
        /// <param name="accountAttribute"></param>
        /// <param name="accountName"></param>
        /// <param name="email"></param>
        /// <param name="FirstName"></param>
        /// <param name="LastName"></param>
        /// <returns></returns>
        public static DataAccessResponseType InviteUser(string accountID, string email, string firstName, string lastName, string roleName, bool isOwner)
        {
            var result = new DataAccessResponseType();


            //Validate all varialbles
            if (String.IsNullOrEmpty(accountID) || String.IsNullOrEmpty(email) || String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName) || String.IsNullOrEmpty(roleName))
            {
                result.isSuccess = false;
                result.ErrorMessage = "Not all parameters contain a value.";
                return result;
            }


            //Validate UserName(Email)
            var emailValidation = ValidationManager.IsValidEmail(email);
            if (!emailValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = emailValidation.validationMessage;
                return result;
            }


            var account = AccountManager.GetAccount(accountID);

            //Validate account existance
            if (account == null)
            {
                result.isSuccess = false;
                result.ErrorMessage = "Account does not exist.";
                return result;
            }

            //Verify that the account is in good standing
            if (!account.Delinquent)
            {
                //Validate that email does not alraedy exist as a user on this account
                string globalUniqueUserName = Sahara.Core.Common.Methods.AccountUserNames.GenerateGlobalUniqueUserName(email, account.AccountID.ToString());
                if (AccountUserManager.GetUserIdentity(globalUniqueUserName) != null)
                {
                    result.isSuccess = false;
                    result.ErrorMessage = "This email is registered to another user on this account";
                    return result;
                }

                //Verify email does not already exist in the invitation list 
                if (UserInvitationsManager.DoesEmailExist(account.AccountID.ToString(), account.StoragePartition, email))
                {
                    result.isSuccess = false;
                    result.ErrorMessage = "This email address has a pending invite.";
                    return result;
                }

                string invitationKey = String.Empty;

                //Store user in invited users table on account side
                try
                {
                    invitationKey = UserInvitationsManager.StoreInvitedUser(account.AccountID.ToString(), account.StoragePartition, email, firstName, lastName, roleName, isOwner);
                }
                catch(Exception e)
                {
                    result.isSuccess = false;
                    result.ErrorMessage = e.Message;

                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "inviting a new account user",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        account.AccountID.ToString(),
                        account.AccountName);

                    return result;
                }

                //Send invite an email (or override for tests)
                bool emailSent;
                try
                {
                    emailSent = EmailInvitation(account, email, firstName, invitationKey);
                }
                catch
                {
                    emailSent = false;
                }

                if(!emailSent)
                {
                    result.isSuccess = false;
                    result.ErrorMessage = "Couldn't send an invite to this email, please try again.";
                    //Clear the invite
                    DeleteInvitation(account.AccountID.ToString(), invitationKey);
                    //return the failure
                    return result;
                }

                //Clear invitations from Cache:
                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    cache.HashDelete(AccountByIdHash.Key(accountID), AccountByIdHash.Fields.Invitations, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                result.isSuccess = true;
                result.SuccessMessage = invitationKey;
            }
            else
            {
                result.ErrorMessage = "Account is not in good standing. Cannnot invite new users.";
            }

            return result;
        }


        public static UserInvitation GetInvitation(string accountAttribute, string invitationKey)
        {

            var account = AccountManager.GetAccount(accountAttribute);
            if (account == null)
            {
                return null;
            }

            var userInvitationTableEntity = UserInvitationsManager.GetInvitedUser(account.AccountID.ToString(), account.StoragePartition, invitationKey);
            var userInvitation = Transformations.TransformToUserInvitation(userInvitationTableEntity, accountAttribute);
            return userInvitation;

        }

        public static List<UserInvitation> GetInvitations(string accountId, string storagePartition)
        {
            List<UserInvitation> invitations = null;
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                var redisValue = cache.HashGet(AccountByIdHash.Key(accountId), AccountByIdHash.Fields.Invitations);
                if (redisValue.HasValue)
                {
                    invitations = JsonConvert.DeserializeObject<List<UserInvitation>>(redisValue);
                }
            }
            catch
            {

            }

            //var results = new List<UserInvitation>();

            if (invitations == null)
            {
                invitations = new List<UserInvitation>();

                try
                {
                    foreach (var userInvitation in UserInvitationsManager.GetInvitedUsers(accountId, storagePartition))
                    {
                        invitations.Add(Transformations.TransformToUserInvitation(userInvitation));
                    }

                    try
                    {
                        cache.HashSet(
                        AccountByIdHash.Key(accountId),
                        AccountByIdHash.Fields.Invitations,
                        JsonConvert.SerializeObject(invitations),
                        When.Always,
                        CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }
                }
                catch (Exception e)
                {
                    //Log exception and email platform admins
                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                        e,
                        "attempting to get invitations for the account",
                        System.Reflection.MethodBase.GetCurrentMethod(),
                        accountId
                    );

                    invitations = null;
                }
            }

            return invitations;
        }

        public static DataAccessResponseType ResendInvitation(string accountAttribute, string invitationKey)
        {
            var response = new DataAccessResponseType();

            var account = AccountManager.GetAccount(accountAttribute);

            if (account == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "This account does not exist."
                };
            }

            try
            {
                var invitation = GetInvitation(account.AccountID.ToString(), invitationKey);

                EmailInvitation(account, invitation.Email, invitation.FirstName, invitationKey);

                response.isSuccess = true;
            }
            catch(Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "resending an account user invitiation",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName);

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            return response;
        }

        public static DataAccessResponseType DeleteInvitation(string accountAttribute, string invitationKey)
        {
            var result = new DataAccessResponseType();

            var account = AccountManager.GetAccount(accountAttribute);
            if (account == null)
            {
                result.isSuccess = false;
                result.ErrorMessage = "This account does not exist.";
                return result;
            }

            //Clear the invitation from Cache:
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            try
            {
                cache.HashDelete(AccountByIdHash.Key(account.AccountID.ToString()), AccountByIdHash.Fields.Invitations, CommandFlags.FireAndForget);
            }
            catch
            {

            }

            result.isSuccess = UserInvitationsManager.DeleteInvitedUser(account.AccountID.ToString(), account.StoragePartition, invitationKey);

            return result;
        }

        #endregion

        #region Creation
        /// <summary>
        /// Only use when registering a user along with new account creation
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="accountID"></param>
        /// <param name="accountName"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static DataAccessResponseType RegisterAccountOwner(string firstName, string lastName, string accountID, string accountName, string email, string password)
        {
            var user = new AccountUserIdentity
            {
                AccountID = new Guid(accountID),
                AccountName = accountName,
                AccountNameKey = Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(accountName),
                UserName = Sahara.Core.Common.Methods.AccountUserNames.GenerateGlobalUniqueUserName(email, accountID),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                AccountOwner = true, // <-- Initial Account Registrars are always set as the default account owner
                CreatedDate = DateTime.UtcNow,
                Active = true
            };

            var result = new DataAccessResponseType();
            result.isSuccess = true; // <-- set to true before validations begin

            //Validate Email
            var emailValidation = ValidationManager.IsValidEmail(user.Email);
            if (!emailValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = emailValidation.validationMessage;

                result.ErrorMessages.Add(emailValidation.validationMessage);
            }

            //Validate Password
            var passwordValidation = ValidationManager.IsValidAccountUserPassword(password);
            if (!passwordValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = passwordValidation.validationMessage;

                result.ErrorMessages.Add(passwordValidation.validationMessage);
            }

            //Validate FirstName
            var firstNameValidation = ValidationManager.IsValidFirstName(firstName);
            if (!firstNameValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = firstNameValidation.validationMessage;

                result.ErrorMessages.Add(firstNameValidation.validationMessage);
            }

            //Validate LastName
            var lastNameValidation = ValidationManager.IsValidFirstName(lastName);
            if (!lastNameValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = lastNameValidation.validationMessage;

                result.ErrorMessages.Add(lastNameValidation.validationMessage);
            }

            //If no validations pass, return the result with error messages:
            if(!result.isSuccess)
            {
                return result;
            }


            //Create
            var identityResult = CreateUserInternal(user, password);
            if (!identityResult.Succeeded)
            {
                result.isSuccess = false;

                result.ErrorMessages = identityResult.Errors.ToList();
            }

            AccountUser outUser = null;
            //Make the initial account user an Admin
            AccountUserManager.UpdateUserRole(user.Id.ToString(), Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin, out outUser);


            result.isSuccess = true;

            result.ResponseObject = user;

            return result;
        }

        //leave out invitation key if you are bypassing invitation/verification
        public static DataAccessResponseType CreateAccountUser(string accountID, string email, string firstName, string lastName, string password, string roleName, bool isOwner = false, string invitationKey = null, bool skipProvisioningRequirement = false)
        {
            DataAccessResponseType result = new DataAccessResponseType();


            //Verify input
            if (String.IsNullOrEmpty(accountID) || String.IsNullOrEmpty(email) || String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(roleName))
            {
                result.isSuccess = false;
                result.ErrorMessage = "Not all inputs contain a value";
                return result;
            }



            //Get associated account
            //Account account = AccountManager.GetAccountByID(accountID, false);
            Account account = AccountManager.GetAccount(accountID);

            if (invitationKey != null)
            {
                //Make sure invitation is still valid if passing in an invitationKey:
                var invitation = UserInvitationsManager.GetInvitedUser(accountID, account.StoragePartition, invitationKey);
                if (invitation == null)
                {
                    result.isSuccess = false;
                    result.ErrorMessage = "This invitation is no longer valid";
                    return result;
                }
            }


            //Validate Account
            if(!skipProvisioningRequirement)
            {
                if (!account.Provisioned)
                {
                    result.isSuccess = false;
                    result.ErrorMessage = "This account has not yet been provisioned and cannot add users";
                    return result;
                }
            }
            


            string globalUniqueUserName = Sahara.Core.Common.Methods.AccountUserNames.GenerateGlobalUniqueUserName(email, accountID);

            var user = new AccountUserIdentity
            {
                AccountID = new Guid(accountID),
                AccountName = account.AccountName,
                AccountNameKey = Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(account.AccountName),
                UserName = globalUniqueUserName,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                AccountOwner = isOwner,
                //Verified = true,
                CreatedDate = DateTime.UtcNow,
                Active = true

            };

            //Set result to success before validating
            result.isSuccess = true;

            //Validate Email
            var emailValidation = ValidationManager.IsValidEmail(user.Email);
            if (!emailValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = emailValidation.validationMessage;

                result.ErrorMessages.Add(emailValidation.validationMessage);
            }

            //Validate Email/UserName Unique
            if (AccountUserManager.GetUserIdentity(globalUniqueUserName) != null)
            {
                result.isSuccess = false;
                result.ErrorMessage = "This email is already registered.";

                result.ErrorMessages.Add("This email is already registered.");
            }

            //Validate Password
            var passwordValidation = ValidationManager.IsValidAccountUserPassword(password);
            if (!passwordValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = passwordValidation.validationMessage;

                result.ErrorMessages.Add(passwordValidation.validationMessage);
            }

            //Validate FirstName
            var firstNameValidation = ValidationManager.IsValidFirstName(firstName);
            if (!firstNameValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = firstNameValidation.validationMessage;

                result.ErrorMessages.Add(firstNameValidation.validationMessage);
            }

            //Validate LastName
            var lastNameValidation = ValidationManager.IsValidLastName(lastName);
            if (!lastNameValidation.isValid)
            {
                result.isSuccess = false;
                result.ErrorMessage = lastNameValidation.validationMessage;

                result.ErrorMessages.Add(lastNameValidation.validationMessage);
            }

            //If no validations pass, return the result with error messages:
            if (!result.isSuccess)
            {
                return result;
            }

            //Validation passed, Create User:
            var identityResult = CreateUserInternal(user, password);

            if (identityResult.Succeeded)
            {
                //Creation of user succeeded
                result.isSuccess = true;


                AccountUser outUser = null;
                //Assign user to role:
                AccountUserManager.UpdateUserRole(user.Id.ToString(), roleName, out outUser);

                //Clear cache for this account
                AccountUserCaching.ClearAssociatedAccountAndUserListCaches(accountID);

                if (invitationKey != null)
                {
                    //Clear the invitation from table storage:
                    var clearRecord = UserInvitationsManager.DeleteInvitedUser(accountID, account.StoragePartition, invitationKey);

                    //Clear the invitation from Cache:
                    //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                    IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
                    try
                    {
                        cache.HashDelete(AccountByIdHash.Key(accountID), AccountByIdHash.Fields.Invitations, CommandFlags.FireAndForget);
                    }
                    catch
                    {

                    }
                    
                }
            }
            else
            {
                //Creation of user failed
                result.isSuccess = false;

                result.ErrorMessages = identityResult.Errors.ToList();
            }




            return result;
        }


        internal static IdentityResult CreateUserInternal(AccountUserIdentity user, string password)
        {

            var userManager = new UserManager<AccountUserIdentity>(
                new UserStore<AccountUserIdentity>(new AccountUserIdentityDbContext()));

            //Allows for use of email address as username:
            userManager.UserValidator = new UserValidator<AccountUserIdentity>(userManager) { AllowOnlyAlphanumericUserNames = false };


            var idResult = userManager.Create(user, password);

            return idResult;

        }

        #endregion

        #region Get/Delete


        public static List<AccountUser> GetUsers(string accountID, bool useCachedVersion = true)
        {

            List<AccountUser> accountUserListCache = null;
            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();
            //DataCache dataCache = new DataCache(AccountUserCaching.accountUserCacheName);
            //string cacheID = AccountUserCacheID.AccountUserList(accountID.ToString());

            if (useCachedVersion)
            {
                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();


                try
                {
                    var redisValue = cache.HashGet(AccountByIdHash.Key(accountID), AccountByIdHash.Fields.Users); 
                    if (redisValue.HasValue)
                    {
	                    accountUserListCache = JsonConvert.DeserializeObject<List<AccountUser>>(redisValue);
                    }
                }
                catch
                {

                }

            }

            if (accountUserListCache == null)
            {
                //var db = new AccountUserIdentityDbContext();
                //var users = db.Users.Where(a => a.AccountID == accountID).ToList();

                var users = Sql.Statements.SelectStatements.SelectAllAccountUsers(accountID);

                //Get roles for each user (usually in cache)
                for (int i = 0; i < users.Count; i++)
                {
                    users[i].Role = GetUserRole(users[i].Id);
                }

                try
                {
                    //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
                    //IDatabase cache = con.GetDatabase();
                    cache.HashSet(
                        AccountByIdHash.Key(accountID),
                        AccountByIdHash.Fields.Users,
                        JsonConvert.SerializeObject(users),
                        When.Always,
                        CommandFlags.FireAndForget);
                    //dataCache.Put(cacheID, users);

                    cache.KeyExpire(AccountByIdHash.Key(accountID), AccountByIdHash.Expiration);
                }
                catch
                {

                }

                return users;
            }
            else
            {
                return accountUserListCache;
            }
        }


        /// <summary>
        /// Only pulls from Cache created by GetUserIdentity. Mostly used to keep views in sync with latest user role & properties
        /// </summary>
        /// <param name="userId"></param>
        public static AccountUser GetUser(string userId, bool useCachedVersion = true)
        {
            if (!useCachedVersion)
            {
                //cache non cached version is requested, transform it from UserIdentity (Will cache the latest version for us)
                return TransformAccountUserIdentityToAccountUser(GetUserIdentity(userId));
            }

            AccountUser accountUserCache = null;

            //Get user from Cache, if doesn't exist, get Idetity version, convert and cache it for next call
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            try
            {
                var redisValue = cache.HashGet(UserHash.Key(userId), UserHash.Fields.Model()); //<-- only cache the "model" or "non-Identity" version by ID (This is for WCF clients that cannot consume Identity veresions of users)
                if (redisValue.HasValue)
                {
	                accountUserCache = JsonConvert.DeserializeObject<AccountUser>(redisValue);
                }
            }
            catch
            {

            }

            if (accountUserCache != null)
            {
                //return the cached version of the User
                return accountUserCache;
            }
            else{

                //cache is empty, transform a fresh version of AccountUserIdentity (will create a cache)
                return TransformAccountUserIdentityToAccountUser(GetUserIdentity(userId));
            }

        }

        public static AccountUserIdentity GetUserIdentity(string userNameOrId)
        {
            var db = new AccountUserIdentityDbContext();

            AccountUserIdentity user = null;

            #region Get User From DB/Entity

            try
            {
                try
                {
                    user = db.Users.FirstOrDefault(u => u.Id == userNameOrId);
                }
                catch
                {
                    //Get by UserName
                    user = db.Users.FirstOrDefault(u => u.UserName == userNameOrId);
                }



                if (user == null)
                {
                    //Get by UserName
                    user = db.Users.FirstOrDefault(u => u.UserName == userNameOrId);
                }


                #region Removed for now
                //user.Account = AccountManager.GetAccount(user.AccountNameKey);
                //user.Accounts = AccountUserManager.GetAllAccountsForEmail(user.Email);
                #endregion



            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to get the user identity for " + userNameOrId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                //user = null;
            }

            if (user != null)
            {

                //Store Non Identity Version Into Cache
                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();

                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                try
                {
                    cache.HashSet(UserHash.Key(user.Id.ToString()), UserHash.Fields.Model(), JsonConvert.SerializeObject(TransformAccountUserIdentityToAccountUser(user)), When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }
            }

            #endregion


            return user;
        }

        /// <summary>
        /// Al la carte method for getting all accounts an email is associated with
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static List<UserAccount> GetAllAccountsForEmail(string email, bool useCachedVersion = true)
        {

            List<UserAccount> accountsListCache = null;
            //DataCache dataCache = new DataCache(AccountUserCaching.accountUserAllUserAccountsForEmailCacheName);
            //string cacheID = AccountUserCacheID.AllUserAccountsForEmail(email);

            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            if (useCachedVersion)
            {
                try
                {
                    var redisValue = cache.StringGet(AccountsForEmailString.Key(email)); 
                    if (redisValue.HasValue)
                    {
                        accountsListCache = JsonConvert.DeserializeObject<List<UserAccount>>(redisValue);
                    }
                
                    //accountsListCache = RedisSerializer.Deserialize<List<UserAccount>>(cache.StringGet(AccountsForEmailString.Key(email)));
                }
                catch
                {

                }
            }

            if (accountsListCache == null)
            {
                //var accounts = AccountManager.GetAllAccountsForEmail(email);
                var accountsForEmail = Sql.Statements.SelectStatements.SelectAllAccountsForEmail(email);

                try
                {
                    //dataCache.Put(cacheID, accountsForEmail);
                    cache.StringSet(
                        AccountsForEmailString.Key(email),
                        JsonConvert.SerializeObject(accountsForEmail),
                        AccountsForEmailString.Expiration,
                        When.Always,
                        CommandFlags.FireAndForget);
                }
                catch
                {

                }

                return accountsForEmail;
            }
            else
            {
                return accountsListCache;
            }
        }

        /*
        public static AccountUser GetAccountUser(string userId, bool includeOtherAccounts)
        {
            var userIdentity = GetAccountUserIdentity(userId, includeOtherAccounts);

            Sahara.Core.Common.
        }*/


        public static DataAccessResponseType DeleteUser(string userId, bool processWithVerifications, out AccountUser user)
        {
            var response = new DataAccessResponseType();
            user = null;

            try
            {

                user = GetUser(userId, false); //<-- Get fresh version, not from any cache

                if (processWithVerifications) //<-- Only skipped when deleting an entire account
                {
                    #region Verifications

                    //Verify that the user is not the only remaining account owner:
                    if (user.AccountOwner && Sql.Statements.SelectStatements.SelectAccountOwnersCount(user.AccountID.ToString()) <= 2) //<-- Changed from 1 to 2 to account for platfrom admin hidden user
                    {
                        // Send Transaction Error Message
                        response.isSuccess = false;
                        response.ErrorMessage = "You must have at least one owner on your account!";

                        return response;
                    }

                    #endregion
                }


                var userManager = new UserManager<AccountUserIdentity>(
                    new UserStore<AccountUserIdentity>(new AccountUserIdentityDbContext()));

                // Remove user from all associated roles:
                Sql.Statements.DeleteStatements.DeleteUserRoles(userId);

                // Delete user
                response.isSuccess = Sql.Statements.DeleteStatements.DeleteUser(userId);

                if(response.isSuccess)
                {
                    //Clear/Update Cache(s):
                    AccountUserCaching.DeleteAllUserCacheReferences(user);
                    AccountUserCaching.ClearAssociatedAccountAndUserListCaches(user.AccountID.ToString());
                }
            }
            catch(Exception e)
            {
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "deleting an account user",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    user.AccountID.ToString(),
                    user.AccountName);

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            return response;
        }

        #endregion

        #region Update

        public static DataAccessResponseType UpdateFullName(string userId, string newFirstName, string newLastName, out string previousFullName)
        {
            var response = new DataAccessResponseType();

            previousFullName = string.Empty;

            //Verify Input
            if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(newFirstName) || String.IsNullOrEmpty(newLastName))
            {
                response.isSuccess = false;
                response.ErrorMessage = "Not all inputs contain a value.";
                return response;
            }

            //Verify change
            var userCheck = GetUser(userId);

            previousFullName = userCheck.FullName;

            if(userCheck.FirstName == newFirstName && userCheck.LastName == newLastName)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This is already the users name.";
                return response;
            }


            //Validate name(s)
            var fNameValidation = ValidationManager.IsValidFirstName(newFirstName);
            var lNameValidation = ValidationManager.IsValidLastName(newLastName);

            if(!fNameValidation.isValid && !lNameValidation.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This is not a valid name.";
                response.ErrorMessages.Add(fNameValidation.validationMessage);
                response.ErrorMessages.Add(lNameValidation.validationMessage);
                return response;

            }
            else if(!fNameValidation.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = fNameValidation.validationMessage;
                response.ErrorMessages.Add(fNameValidation.validationMessage);
                return response;
            }
            else if(!lNameValidation.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = lNameValidation.validationMessage;
                response.ErrorMessages.Add(lNameValidation.validationMessage);
                return response;
            }

            response.isSuccess = Sql.Statements.UpdateStatements.UpdateUserFullName(userId, newFirstName, newLastName);

            if (response.isSuccess)
            {
                var user = GetUser(userId, false); //<--Updates cache for us
                //AccountUserCaching.DeleteAccountUsersIdentityReferencesInCache(user);
                //AccountUserCaching.UpdateAccountUserHashReferenceInCache(user);

                AccountUserCaching.ClearAssociatedAccountAndUserListCaches(user.AccountID.ToString());

                //Clear cache of ALL users on the account:
                AccountUserCaching.ClearAllUserCaches(user.AccountID.ToString());
                AccountCaching.InvalidateAccountListsCache();
            }

            return response;
        }

        //Will update the users login email as well as their username which will disengage this email from other accounts associated with that email address
        public static DataAccessResponseType UpdateEmail(string accountId, string userId, string email, out AccountUser user)
        {
            var response = new DataAccessResponseType();

            user = null;

            //Verify Input
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(accountId) || String.IsNullOrEmpty(userId))
            {
                response.isSuccess = false;
                response.ErrorMessage = "Not all inputs contain a value.";
                return response;
            }

            //Verify that this is a different email before performing the transaction
            var userCheck = GetUser(userId);

            if(userCheck.Email == email)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This user is already assigned this email address.";
                return response;
            }

            //Validate Email
            var emailValidation = ValidationManager.IsValidEmail(email);
            if (!emailValidation.isValid)
            {
                response.isSuccess = false;
                response.ErrorMessage = emailValidation.validationMessage;

                response.ErrorMessages.Add(emailValidation.validationMessage);

                return response;
            }



            //Validate that the AccountID is for a valid and active Account ----------------------------------------------------------------------
            var account = AccountManager.GetAccount(accountId, false);

            if (account == null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This account does not exist.";
                response.ErrorMessages.Add("This account does not exist.");

                return response;
            }

            if (!account.Active)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This account is not active.";
                response.ErrorMessages.Add("This account is not active.");

                return response;
            }


            //Validate that the new Email/UserName is Unique to this account-----------------------------------------------------------------
            string globalUniqueUserName = Sahara.Core.Common.Methods.AccountUserNames.GenerateGlobalUniqueUserName(email, accountId);

            if (AccountUserManager.GetUserIdentity(globalUniqueUserName) != null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "This email is already in use on this account.";

                response.ErrorMessages.Add("This email is already in use on this account.");

                return response;
            }



            response.isSuccess = Sql.Statements.UpdateStatements.UpdateUserEmail(accountId, userId, email);

            if (response.isSuccess)
            {
                //var user = GetUserIdentity(userID, false);

                AccountUserCaching.DeleteUserHashReference(userId);
                user = GetUser(userId, false); //<-- Updates cache for us
                //AccountUserCaching.UpdateAccountUserHashReferenceInCache(userForCache);

                //AccountUserCaching.DeleteAccountUsersIdentityReferencesInCache(user);

                //AccountUserCaching.DeleteGlobalUserNameReferencesInCache(userCheck.UserName);
                //AccountUserCaching.UpdateAccountUserIdentityReferencesInCache(user, false);

                AccountUserCaching.ClearAssociatedAccountAndUserListCaches(accountId);

                //Clear cache of ALL users on the account:
                AccountUserCaching.ClearAllUserCaches(accountId);
                AccountCaching.InvalidateAccountListsCache();

            }

            return response;
        }

        public static DataAccessResponseType UpdateOwnerStatus(string accountId, string userId, bool isOwner, out AccountUser user)
        {
            var result = new DataAccessResponseType(); // = Sql.Statements.UpdateStatements.UpdateUserEmail(accountId, userID, email);

            user = GetUser(userId);

            //Verify that the user is not already set to requested active state
            if (user.AccountOwner == isOwner)
            {
                // Set Transaction Error Message
                result.isSuccess = false;
                if (isOwner)
                {
                    result.ErrorMessage = "This user is already an account owner.";
                }
                else
                {
                    result.ErrorMessage = "This user is already set as a non owner.";
                }

                // Set Transaction Error Message
                return result;
            }

            if(!isOwner)
            {
                //If we are removing Owner status then we verify that another owner exists before removing the status from this user (Account must always have at least ONE owner)
                if (Sql.Statements.SelectStatements.SelectAccountOwnersCount(accountId) <= 1)
                {
                    // Send Transaction Error Message
                    result.isSuccess = false;
                    result.ErrorMessage = "You must have at least one owner on your account!";

                    return result;
                }
            }

            // Make the update
            result.isSuccess = Sql.Statements.UpdateStatements.UpdateUserOwnerStatus(userId, isOwner);

            if (result.isSuccess == true)
            {
                AccountUserCaching.DeleteUserHashReference(userId);

                user = GetUser(userId, false); //<--Updates cache for us
                //AccountUserCaching.UpdateAccountUserHashReferenceInCache(userForCache);

                //var userForCache = GetUserIdentity(userId, false, false);
                //AccountUserCaching.DeleteAccountUsersIdentityReferencesInCache(userForCache);
                //AccountUserCaching.UpdateAccountUserIdentityReferencesInCache(userForCache, false);

                AccountUserCaching.ClearAssociatedAccountAndUserListCaches(accountId);

                //Clear cache of ALL users on the account:
                AccountUserCaching.ClearAllUserCaches(accountId);
                AccountCaching.InvalidateAccountListsCache();
            }

            return result;
        }

        public static DataAccessResponseType UpdateActiveState(string accountId, string userId, bool isActive, out AccountUser user)
        {
            var result = new DataAccessResponseType(); // = Sql.Statements.UpdateStatements.UpdateUserEmail(accountId, userID, email);

            user = null;

            //Verify Input
            if (String.IsNullOrEmpty(accountId) || String.IsNullOrEmpty(userId))
            {
                result.isSuccess = false;
                result.ErrorMessage = "Not all inputs contain a value.";
                return result;
            }

            user = GetUser(userId);

            //Verify that the user is not already set to requested active state
            if (user.Active == isActive)
            {
                // Send Transaction Error Message
                result.isSuccess = false;
                result.ErrorMessage = "This user is already set to " + isActive + ".";

                return result;
            }

            if(!isActive)
            {
                // Only perform if we are setting a user to inactive 
                var userIdentity = GetUserIdentity(userId);

                if (userIdentity.AccountOwner)
                {
                    //If we are making an  AccountOwner inactive then we verify that another owner exists that is active before updating the status (Account must always have at least ONE active account owner)
                    if (Sql.Statements.SelectStatements.SelectActiveAccountOwnersCount(accountId) <= 1)
                    {
                        // Send Transaction Error Message
                        result.isSuccess = false;
                        result.ErrorMessage = "You must have at least one active owner on your account!";

                        return result;
                    }
                }
            }

            // Make the update
            result.isSuccess = Sql.Statements.UpdateStatements.UpdateUserActiveState(userId, isActive);

            if (result.isSuccess == true)
            {
                AccountUserCaching.DeleteUserHashReference(userId);

                user = GetUser(userId, false); //<--Updates cache for us
                //AccountUserCaching.UpdateAccountUserHashReferenceInCache(userForCache);


                //var userForCache = GetUserIdentity(userId, false, false);
                //AccountUserCaching.DeleteAccountUsersIdentityReferencesInCache(userForCache);
                //AccountUserCaching.UpdateAccountUserIdentityReferencesInCache(userForCache, false);

                AccountUserCaching.ClearAssociatedAccountAndUserListCaches(accountId);

                //Clear cache of ALL users on the account:
                AccountUserCaching.ClearAllUserCaches(accountId);
                AccountCaching.InvalidateAccountListsCache();
            }

            return result;
        }

        public static DataAccessResponseType UpdateProfilePhoto(string accountId, string userId, byte[] byteArray, out AccountUser user)
        {
            var response = new DataAccessResponseType();
            user = null;

            //Verify, Edit & Store the image:
            var accountUserPhotoProcessor = new Imaging.AccountUserProfilePhotoProcessor();
            var account = AccountManager.GetAccount(accountId);
            response = accountUserPhotoProcessor.ProcessAccountUserProfilePhoto(accountId, account.StoragePartition, byteArray);

            if (!response.isSuccess)
            {
                return response; //<--return error
            }

            //Update ImageID of Photo for user:
            try
            {
                //var cdnEndpoint = Settings.Azure.Storage.GetStoragePartition(account.StoragePartition).CDN;
                response.isSuccess = Sql.Statements.UpdateStatements.UpdateUserPhoto(accountId, userId, response.SuccessMessage);
            }
            catch(Exception e)
            {
                #region Handle/Log exception

                response.isSuccess = false;
                response.ErrorMessage = "An error occured while resizing & saving images";
                response.ErrorMessages.Add(e.Message);

                var accountName = string.Empty;

                try
                {
                    accountName = AccountManager.GetAccountName(accountId);
                }
                catch
                {

                }

                //Log the error
                //Log Issues For Administrator Analysis:
                PlatformLogManager.LogActivity(
                        CategoryType.Error,
                        ActivityType.Error_Imaging,
                        "Resizing & saving an account user profile image.",
                        e.Message,
                        accountId,
                        accountName,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name
                    );


                PlatformLogManager.LogActivity(
                        CategoryType.Imaging,
                        ActivityType.Imaging_Error,
                        "Resizing & saving an account user profile image.",
                        e.Message,
                        accountId,
                        accountName,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name
                    );

                //Return the response
                return response;

                #endregion
            }

            if (response.isSuccess)
            {
                //Invalidate Caches:
                AccountUserCaching.DeleteUserHashReference(userId);


                user = GetUser(userId, false); //<--Updates cache for us
                //AccountUserCaching.UpdateAccountUserHashReferenceInCache(userForCache);


                //var userForCache = GetUserIdentity(userId, false, false);
                //AccountUserCaching.DeleteAccountUsersIdentityReferencesInCache(userForCache);
                //AccountUserCaching.UpdateAccountUserIdentityReferencesInCache(userForCache, false);

                AccountUserCaching.ClearAssociatedAccountAndUserListCaches(accountId);

                //Clear cache of ALL users on the account:
                AccountUserCaching.ClearAllUserCaches(accountId);
                AccountCaching.InvalidateAccountListsCache();
            }
            else
            {
                response.ErrorMessage = "Photo was uploaded, but we ran into an issue when updating the user. Please try again.";
            }

            return response;
        }

        #endregion

        #region Passwords

        #region Reset Lost Passwords

        /// <summary>
        /// Sends an email to a user claiming a lost password with a PasswordClaimKey that allows for a password reset
        /// </summary>
        /// <param name="accountAttribute"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static DataAccessResponseType ClaimLostPassword(string accountAttribute, string email)
        {
            var result = new DataAccessResponseType();

            var account = AccountManager.GetAccount(accountAttribute);

            //verify that the user & account exists, and that the account is in good standing:
            string globalUniqueUserName = Sahara.Core.Common.Methods.AccountUserNames.GenerateGlobalUniqueUserName(email, account.AccountID.ToString());
            var user = GetUserIdentity(globalUniqueUserName);

            if(user == null)
            {
                string errorMessage = "This user or account does not exist.";
                result.ErrorMessage = errorMessage;

                result.ErrorMessages.Add(errorMessage);

                return result;
            }

            if (!account.Active)
            {
                string errorMessage = "The account is inactive or not in good standing.";
                result.ErrorMessage = errorMessage;

                result.ErrorMessages.Add(errorMessage);

                return result;
            }

            //Delete any existing claims on this email for this account
            var passwordClaimKey = PasswordClaimManager.StorePasswordClaim(account.AccountID.ToString(), account.StoragePartition, email);

            //Send email to user to reset password:
            EmailPasswordClaim(account, email, user.FirstName, passwordClaimKey);

            result.isSuccess = true;
            result.SuccessMessage = "An email has been sent to the address for this user along with a link to reset the password.";

            return result;
        }


        public static List<UserPasswordResetClaim> GetPasswordClaims(string accountAttribute)
        {
            var account = AccountManager.GetAccount(accountAttribute);

            var userPasswordReserClaims = new List<UserPasswordResetClaim>();

            foreach(PasswordClaimTableEntity passwordClaimTableEntity in PasswordClaimManager.GetPasswordClaims(account.AccountID.ToString(), account.StoragePartition))
            {
                userPasswordReserClaims.Add(Transformations.TransformToPasswordResetClaim(passwordClaimTableEntity));

            }

            return userPasswordReserClaims;
        }

        public static PasswordClaimTableEntity GetPasswordClaim(string accountAttribute, string passwordClaimKey)
        {
            var account = AccountManager.GetAccount(accountAttribute);

            return PasswordClaimManager.GetPasswordClaim(account.AccountID.ToString(), account.StoragePartition, passwordClaimKey);
        }

        /// <summary>
        /// used by password reset link, sent to user that created claim
        /// </summary>
        /// <param name="accountAttribute"></param>
        /// <param name="resetPassword"></param>
        /// <param name="confirmResetPassword"></param>
        /// <returns></returns>
        public static DataAccessResponseType ResetPassword(string accountAttribute, string passwordClaimKey, string resetPassword)
        {
            var account = AccountManager.GetAccount(accountAttribute);

            var result = new DataAccessResponseType();


            if(resetPassword.Length < Sahara.Core.Settings.Accounts.Registration.PasswordMinimumLength)
            {
                result.isSuccess = false;
                result.SuccessMessage = "Passwords must be " + Sahara.Core.Settings.Accounts.Registration.PasswordMinimumLength + " characters or more.";
                result.ErrorMessage = "Passwords must be " + Sahara.Core.Settings.Accounts.Registration.PasswordMinimumLength + " characters or more.";

                return result;
            }

            try
            {
                //Get the claim:
                var passwordClaim = PasswordClaimManager.GetPasswordClaim(account.AccountID.ToString(), account.StoragePartition, passwordClaimKey);

                //Update the user password:

                string globalUniqueUserName = Sahara.Core.Common.Methods.AccountUserNames.GenerateGlobalUniqueUserName(passwordClaim.Email, account.AccountID.ToString());

                UpdatePassword(globalUniqueUserName, resetPassword);

                //Delete the claim:
                PasswordClaimManager.DeletePasswordClaim(account.AccountID.ToString(), account.StoragePartition, passwordClaimKey);

                result.isSuccess = true;
                result.SuccessMessage = "Your password has been updated, please log into your account to verify.";

                return result;

            }
            catch
            {
                string errorMessage = "There was an error verifying the password claim, or claim does not exist";
                result.ErrorMessage = errorMessage;

                result.ErrorMessages.Add(errorMessage);

                return result;
            }


        }


        #endregion

        #region Change Password

        /// <summary>
        /// Used by users who have their password after authentication
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <param name="confirmNewPassword"></param>
        /// <returns></returns>
        public static DataAccessResponseType ChangePassword(string accountID, string email, string currentPassword, string newPassword)
        {
            var result = new DataAccessResponseType();

            //Verify password is valid
            var passwordValidation = ValidationManager.IsValidAccountUserPassword(newPassword);
            if (!passwordValidation.isValid)
            {
                string errorMessage = "Password must be at least " + Sahara.Core.Settings.Accounts.Registration.PasswordMinimumLength + " characters in length";
                result.ErrorMessage = errorMessage;

                result.ErrorMessages.Add(errorMessage);

                return result;
            }



            //Verify password by logging user in:

            string globalUniqueUserName = Sahara.Core.Common.Methods.AccountUserNames.GenerateGlobalUniqueUserName(email, accountID);

            var response = AccountUserManager.GetUserWithLogin(globalUniqueUserName, currentPassword);

            if (!response.isSuccess)
            {
                string errorMessage = "Current password is incorrect.";
                result.ErrorMessage = errorMessage;

                result.ErrorMessages.Add(errorMessage);

                return result;
            }


            //Update the user password:

            result.isSuccess = UpdatePassword(globalUniqueUserName, newPassword);

            if(result.isSuccess)
            {
                result.SuccessMessage = "Your password has been updated.";
            }
            else
            {
                result.ErrorMessage = "An error occured while updating the password.";
            }

            return result;

        }

        #endregion

        #region Private Password Methods

        private static bool UpdatePassword(string userName, string newPassword)
        {
            try
            {
                
                UserStore<AccountUserIdentity> store = new UserStore<AccountUserIdentity>(new AccountUserIdentityDbContext());

                var userManager = new UserManager<AccountUserIdentity>(
                new UserStore<AccountUserIdentity>(new AccountUserIdentityDbContext()));

                String hashedNewPassword = userManager.PasswordHasher.HashPassword(newPassword);

                AccountUserIdentity user = store.FindByNameAsync(userName).Result;

                store.SetPasswordHashAsync(user, hashedNewPassword);
                store.UpdateAsync(user);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #endregion

        #region Roles


        public static string GetUserRole(string userId, bool useCachedVersion = true)
        {
            string role = string.Empty;

            string accountUserRoleCache = null;

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            if (useCachedVersion)
            {
                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();



                //string redisKey = UserRoleKeys.UserRole(userId);
                try
                {
                    accountUserRoleCache = cache.HashGet(UserHash.Key(userId), UserHash.Fields.Role());
                }
                catch
                {

                }

                //con.Close();
            }

            if (accountUserRoleCache == null)
            {

                var userManager = new UserManager<AccountUserIdentity>(
                        new UserStore<AccountUserIdentity>(new AccountUserIdentityDbContext()));

                //Allows for use of email address as username:
                userManager.UserValidator = new UserValidator<AccountUserIdentity>(userManager) { AllowOnlyAlphanumericUserNames = false };

                //role = userManager.GetRoles(userId).ToList()[0];
                try
                {
                    role = userManager.GetRoles(userId).ToList()[0]; //<-- Only one role per user should exist

                    //cache or re-cache:
                    //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
                    //IDatabase cache = con.GetDatabase();

                    //string redisKey = UserRoleKeys.UserRole(userId);
                    try
                    {
                        cache.HashSet(UserHash.Key(userId), UserHash.Fields.Role(), role);
                    }
                    catch
                    {

                    }
                    

                    //con.Close();

                }
                catch
                {
                    role = null;
                }

            }
            else
            {
                role = accountUserRoleCache;
            }

            return role;
        }

        



        public static async Task<bool> CreateRoles(List<string> roles)
        {
            var roleManager = new RoleManager<IdentityRole>(
            new RoleStore<IdentityRole>(new AccountUserIdentityDbContext()));

            var idResult = new IdentityResult();

            foreach (string role in roles)
            {
                idResult = await roleManager.CreateAsync(new IdentityRole(role));
            }

            return idResult.Succeeded;
        }



        public static DataAccessResponseType UpdateUserRole(string userId, string roleName, out AccountUser user)
        {
            var response = new DataAccessResponseType();
            
            var currentUserRole = GetUserRole(userId);

            user = null;

            //verify user is not already set to this role before performing transaction:
            if (currentUserRole == roleName)
            {
                // Send Transaction Error Message
                response.isSuccess = false;
                response.ErrorMessage = "User is already assigned to this role.";

                
                return response;
            }


            var userManager = new UserManager<AccountUserIdentity>(
                new UserStore<AccountUserIdentity>(new AccountUserIdentityDbContext()));

            //Allows for use of email address as username:
            userManager.UserValidator = new UserValidator<AccountUserIdentity>(userManager) { AllowOnlyAlphanumericUserNames = false };


            //Clear user of all roles (Users are only ever in ONE role at a time):
            Sql.Statements.DeleteStatements.DeleteUserRoles(userId);

            //Assign user to new role
            var result = userManager.AddToRole(userId, roleName);

            response.isSuccess = result.Succeeded;

            if(response.isSuccess)
            {
                //Inalidate user role in cache
                //AccountUserCaching.InvalidateUserRoleCache(userId);

                //AccountUserCaching.DeleteUserHashReference(userId);

                user = GetUser(userId, false);
                //AccountUserCaching.UpdateAccountUserHashReferenceInCache(userForCache);

                //Update user role in UserHash on Redis
                //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.AccountManager_RedisConfiguration);
                //IDatabase cache = con.GetDatabase();

                //Clear cache of ALL users on the account:
                AccountUserCaching.ClearAllUserCaches(user.AccountID.ToString());
                AccountCaching.InvalidateAccountListsCache();

                //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.AccountManager_Multiplexer.GetDatabase();
                IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

                //string redisKey = UserRoleKeys.UserRole(userId);
                try
                {
                    cache.HashSet(UserHash.Key(userId), UserHash.Fields.Role(), roleName, When.Always, CommandFlags.FireAndForget);
                }
                catch
                {

                }

                //string redisKey = UserRoleKeys.UserRole(userId);


                //Invalidate all user lists in cache
                //var user = GetUserIdentity(userId, false, false);
                //AccountUserCaching.DeleteAccountUsersIdentityReferencesInCache(user);
                //AccountUserCaching.UpdateAccountUserIdentityReferencesInCache(user, false);

                AccountUserCaching.ClearAssociatedAccountAndUserListCaches(user.AccountID.ToString());


            }

            if(!result.Succeeded)
            {
                try
                {
                    response.ErrorMessage = result.Errors.ToList()[0];
                }
                catch
                {

                }
                try
                {
                    response.ErrorMessages = result.Errors.ToList();
                }
                catch
                {

                }
            }

            return response;
        }

        #endregion

        #region Send Emails

        public static DataAccessResponseType SendEmailToUser(string userId, string fromEmail, string fromName, string emailSubject, string emailMessage, bool isImportant = false)
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
            if (String.IsNullOrEmpty(userId))
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Please include a user Id." };
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
                EmailManager.Send(GetUser(userId).Email, fromEmail, fromName, emailSubject, emailMessage, true, isImportant);
                return new DataAccessResponseType { isSuccess = true };
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to send an email to UserID: " + userId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }

        }

        #endregion

        #region Counts

        public static int GetUserCount()
        {

            //Check the cache first
            //ConnectionMultiplexer con = ConnectionMultiplexer.Connect(Sahara.Core.Settings.Azure.Redis.RedisConnections.PlatformManager_RedisConfiguration);
            //IDatabase cache = con.GetDatabase();

            //IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.PlatformManager_Multiplexer.GetDatabase();
            IDatabase cache = Sahara.Core.Settings.Azure.Redis.RedisMultiplexers.RedisMultiplexer.GetDatabase();

            Object cachedUserCount = null;

            try
            {
                cachedUserCount = cache.StringGet(
                       Sahara.Core.Common.Redis.PlatformManagerServer.Strings.GlobalUserCount.Key()
                       );
            }
            catch
            {

            }

            if (((RedisValue)cachedUserCount).HasValue)
            {
                return JsonConvert.DeserializeObject<int>((RedisValue)cachedUserCount);
            }
            else
            {
                int count = Sql.Statements.SelectStatements.SelectGlobalUsersCount();

                try
                {
                    //Store a copy in the cache
                    cache.StringSet(
                        Sahara.Core.Common.Redis.PlatformManagerServer.Strings.GlobalUserCount.Key(),
                        JsonConvert.SerializeObject(count),
                        Sahara.Core.Common.Redis.PlatformManagerServer.Strings.GlobalUserCount.Expiration,
                        When.Always,
                        CommandFlags.FireAndForget
                        );
                }
                catch
                {

                }

                return count;
            }
        }

        #endregion

        #region Private Methods

        public static AccountUser TransformAccountUserIdentityToAccountUser(AccountUserIdentity accountUserIdentity)
        {
            AccountUser accountUser = new AccountUser();

            //accountUser.Account = accountUserIdentity.Account;
            accountUser.AccountID = accountUserIdentity.AccountID;
            accountUser.AccountName = accountUserIdentity.AccountName;
            accountUser.AccountNameKey = accountUserIdentity.AccountNameKey;
            accountUser.AccountOwner = accountUserIdentity.AccountOwner;
            accountUser.Active = accountUserIdentity.Active;
            accountUser.Email = accountUserIdentity.Email;
            accountUser.FirstName = accountUserIdentity.FirstName;
            accountUser.LastName = accountUserIdentity.LastName;
            accountUser.Id = accountUserIdentity.Id;       
            accountUser.Role = AccountUserManager.GetUserRole(accountUserIdentity.Id);
            accountUser.Photo = accountUserIdentity.Photo;
            accountUser.UserName = accountUserIdentity.UserName;
            //accountUser.Verified = accountUserIdentity.;
            accountUser.CreatedDate = accountUserIdentity.CreatedDate;

            accountUser.FullName = accountUserIdentity.FullName;

            return accountUser;

        }

        private static bool EmailInvitation(Account account, string email, string firstName, string invitationKey)
        {

            EmailManager.Send(
                email,
                Settings.Endpoints.Emails.FromInvitations,
                account.AccountName,
                String.Format(Settings.Copy.EmailMessages.AccountUserInvitation.Subject, account.AccountName),
                String.Format(Settings.Copy.EmailMessages.AccountUserInvitation.Body, firstName, account.AccountName, account.AccountNameKey, invitationKey),
                true
            );

            return true;
        }

        private static bool EmailPasswordClaim(Account account, string email, string firstName, string passwordClaimKey)
        {

            EmailManager.Send(
                email,
                Settings.Endpoints.Emails.FromPasswordClaims,
                account.AccountName,
                String.Format(Settings.Copy.EmailMessages.AccountUserPasswordClaim.Subject),
                String.Format(Settings.Copy.EmailMessages.AccountUserPasswordClaim.Body, firstName, account.AccountName, account.AccountNameKey, passwordClaimKey),
                true
            );

            return true;
        }

        #endregion
    }
}
