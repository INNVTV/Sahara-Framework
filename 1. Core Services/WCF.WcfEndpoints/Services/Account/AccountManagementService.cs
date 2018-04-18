using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.TableEntities;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.Stripe;
using Sahara.Core.Platform.Partitioning;
using Sahara.Core.Platform.Partitioning.Models;
using Sahara.Core.Platform.Requests.Models;
using Sahara.Core.Platform.Requests;
using WCF.WcfEndpoints.Contracts.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using Sahara.Core.Platform.Constraints;
using Sahara.Core.Logging.AccountLogs.Models;
using Sahara.Core.Logging.AccountLogs.Types;
using Sahara.Core.Logging.AccountLogs;
using Sahara.Core.Accounts.PaymentPlans.Public;
using Sahara.Core.Application.Categorization.Public;
using Sahara.Core.Application.Tags.Public;
//using Sahara.Core.Application.ApplicationImages;
using StackExchange.Redis;
using Sahara.Core.Common.Redis.AccountManagerServer.Hashes;
using Newtonsoft.Json;
using Sahara.Core.Accounts.Capacity;
using Sahara.Core.Accounts.Capacity.Public;
using Sahara.Core.Accounts.Capacity.Models;
using Sahara.Core.Platform.Snapshots.Public;
using Sahara.Core.Accounts.Themes.Public;
using Sahara.Core.Accounts.Themes.Models;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Platform.Users;
using Sahara.Core.Accounts.Settings;
using Sahara.Core.Accounts.DocumentModels;

namespace WCF.WcfEndpoints.Service.Account
{
    public class AccountManagementService : IAccountManagementService
    {
        #region GET/SEARCH Account(s) & Account Related Objects(s)

        #region Listsings & Searches


        public List<Sahara.Core.Accounts.Models.Account> GetAccounts(int pageNumber, int pageSize, string orderBy, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            if(orderBy == null || orderBy == "")
            {
                orderBy = "AccountNameKey Asc";
            }

            return AccountManager.GetAllAccounts(pageNumber, pageSize, orderBy);
        }

        public List<Sahara.Core.Accounts.Models.Account> SearchAccounts(string query, int maxResults, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return AccountManager.SearchAccounts(query, "AccountNameKey Asc", maxResults);
        }

        public List<Sahara.Core.Accounts.Models.Account> GetAccountsByFilter(string columnName, string value, int pageNumber, int pageSize, string orderBy, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            if (orderBy == null || orderBy == "")
            {
                orderBy = "AccountNameKey Asc";
            }

            return AccountManager.GetAllAccountsByFilter(columnName, value, pageNumber, pageSize, orderBy);
        }

        public int GetAccountCount(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return -1;
            }

            return AccountManager.GetAccountCount();
        }

        public int GetAccountCountByFilter(string columnName, string value, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return -1;
            }

            return AccountManager.GetAccountCount(columnName, value);
        }


        /* Used in scenarios where we isolate accounts in SQL across database partitions and schema versions (default is to use schemaless DocumentDB)
        public List<AccountDatabasePartition> GetAccountPartitions(bool includeTenantCount)
        {
            var partitions = PartitioningManager.GetAccountPartitions(includeTenantCount);

            return partitions;
        }*/


        #endregion

        #region Individual Accounts


        public Sahara.Core.Accounts.Models.Account GetAccount(string id, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return AccountManager.GetAccount(id);
        }


        /* Used in scenarios where we isolate accounts in SQL across database partitions and schema versions (default is to use schemaless DocumentDB)
         * 
         
        public Decimal GetAccountSchemaVersion(string id)
        {
            return AccountSchemaManager.GetSchemaVersion(id);
        }

        public AccountDatabasePartition GetAccountPartition(string id, bool includeTenantCount)
        {
            var partition = PartitioningManager.GetAccountPartition(id, includeTenantCount);

            return partition;
        }*/


        #endregion

        #endregion

        #region Update Account Methods

        public DataAccessResponseType UpdateAccountName(string accountId, string newAccountName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null,
                true); //<-- Only AccountOwners can change the account name

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            return AccountManager.UpdateAccountName(accountId, newAccountName);  
        }

        #endregion

        #region Account Users, Updates & Roles

        #region Create Methods

        //Used to bypass the invitation/registration system (should only be used by platform admin)
        //(otherwise use AccountRegistration Services)
        public DataAccessResponseType CreateAccountUser(string accountId, string email, string firstName, string lastName, string password, string roleName, bool isOwner, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null); //<-- Account owners must always use invitations by default

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var account = AccountManager.GetAccount(accountId);

            if(!account.Provisioned)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot add users to unprovisioned accounts." };
            }

            #region Check Account Constraints/Capabilities

            var constraintsResponse = AccountConstraintsManager.CheckUserConstraint(accountId);
            if (constraintsResponse.isConstrained)
            {
                //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
                Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("users", account.AccountID.ToString(), account.AccountName);

                return new DataAccessResponseType { isSuccess = false, ErrorCode = Sahara.Core.Common.Codes.DataAccessErrorCode.Constraint.ToString(), ErrorMessage = constraintsResponse.constraintMessage };
            }

            #endregion

            var result = AccountUserManager.CreateAccountUser(accountId, email, firstName, lastName, password, roleName, isOwner);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {

                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.AccountUser,
                        ActivityType.AccountUser_Created,
                        "User created",
                        requesterName + " created new user: '" + firstName + " " + lastName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            return result;

        }

        #endregion

        #region Get Methods

        public List<AccountUser> GetAccountUsers(string accountID, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return AccountUserManager.GetUsers(accountID);
        }

        // Mostly used to keep views in sync with latest user role & properties (WCF clients cannot consume Identity versions)
        public AccountUser GetAccountUser(string userId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return AccountUserManager.GetUser(userId);

        }

        public List<UserAccount> GetAllAccountsForEmail(string email, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return AccountUserManager.GetAllAccountsForEmail(email);
        }

        public List<String> GetAccountUserRoles(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return Sahara.Core.Settings.Accounts.Users.Authorization.Roles.GetRoles();
        }

        #endregion

        #region Update methods

        public DataAccessResponseType UpdateAccountUserRole(string accountId, string userId, string newRole, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var account = AccountManager.GetAccount(accountId);

            AccountUser user = null;
            var result = AccountUserManager.UpdateUserRole(userId, newRole, out user);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.AccountUser,
                        ActivityType.AccountUser_Role_Updated,
                        "Role updated",
                        requesterName + " updated " + user.FullName + "'s role to " + newRole,
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion

            return result;
        }

        public DataAccessResponseType UpdateAccountUserFullName(string accountId, string userId, string newFirstName, string newLastName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var account = AccountManager.GetAccount(accountId);

            //var user = AccountUserManager.GetUser(userId);
            string previousFullName = string.Empty;

            var result = AccountUserManager.UpdateFullName(userId, newFirstName, newLastName, out previousFullName);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    var logDescription = string.Empty;

                    if (userId == requesterId)
                    {
                        logDescription = requesterName + " has updated their name to " + newFirstName + " " + newLastName;
                    }
                    else
                    {
                        logDescription = requesterName + " updated " + previousFullName + "'s name to " + newFirstName + " " + newLastName;
                    }

                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.AccountUser,
                        ActivityType.AccountUser_Edited,
                        "Name updated",
                        logDescription,
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion

            return result;
            
        }

        public DataAccessResponseType UpdateAccountOwnershipStatus(string accountId, string userId, bool isOwner, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var account = AccountManager.GetAccount(accountId);
            AccountUser user = null;

            var result = AccountUserManager.UpdateOwnerStatus(accountId, userId, isOwner, out user);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    var description = string.Empty;

                    if (isOwner)
                    {
                        description = requesterName + " made " + user.FullName + " an account owner";

                    }
                    else
                    {
                        description = requesterName + " removed " + user.FullName + " as an account owner";
                    }

                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.AccountUser,
                        ActivityType.AccountUser_OwnershipStatus_Updated,
                        "Ownerhip status updated",
                        description,
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion

            return result;
            
        }

        public DataAccessResponseType UpdateAccountUserActiveState(string accountId, string userId, bool isActive, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
 
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var account = AccountManager.GetAccount(accountId);
            AccountUser user = null;

            var result = AccountUserManager.UpdateActiveState(accountId, userId, isActive, out user);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    var description = string.Empty;

                    if (isActive)
                    {
                        description = requesterName + " activated " + user.FullName + "'s account";

                    }
                    else
                    {
                        description = requesterName + " deactivated " + user.FullName + "'s account";
                    }

                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.AccountUser,
                        ActivityType.AccountUser_Edited,
                        "Active state updated",
                        description,
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion

            return result;
        }


        //Will update the users login email as well as their username which will disengage this email from other accounts associated with that email address
        public DataAccessResponseType UpdateAccountUserEmail(string accountId, string userId, string newEmail, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var account = AccountManager.GetAccount(accountId);
            AccountUser user = null;

            var result = AccountUserManager.UpdateEmail(accountId, userId, newEmail, out user);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    var logDescription = string.Empty;

                    if (userId == requesterId)
                    {
                        logDescription = requesterName + " has updated their email to " + newEmail;
                    }
                    else
                    {
                        logDescription = requesterName + " updated " + user.FullName + "'s email to " + newEmail;
                    }

                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.AccountUser,
                        ActivityType.AccountUser_Edited,
                        "Email updated",
                        logDescription,
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion

            return result;
            
        }

        // Future performance update: have client upload image to intermediary storage, submit location with imag eid for WCF processing (similar to other imageing solutions)
        public DataAccessResponseType UpdateAccountUserProfilePhoto(string accountId, string userId, byte[] imageByteArray, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            AccountUser user = null;
            var account = AccountManager.GetAccount(accountId);

            var result = AccountUserManager.UpdateProfilePhoto(accountId, userId, imageByteArray, out user);


            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    var logDescription = string.Empty;

                    if (userId == requesterId)
                    {
                        logDescription = requesterName + " has updated their profile photo";
                    }
                    else
                    {
                        logDescription = requesterName + " updated " + user.FullName + "'s profile photo";
                    }

                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.AccountUser,
                        ActivityType.AccountUser_Edited,
                        "Photo updated",
                        logDescription,
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion

            return result;
        }

        #endregion

        #region Delete Methods

        public DataAccessResponseType DeleteAccountUser(string accountId, string userId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            AccountUser user = null;
            var account = AccountManager.GetAccount(accountId);

            var result = AccountUserManager.DeleteUser(userId, true, out user);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.AccountUser,
                        ActivityType.AccountUser_Deleted,
                        "User deleted",
                        requesterName + " deleted " + user.FullName + "'s account",
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            return result;

        }
        #endregion

        #region AccountUser Invitations

        public DataAccessResponseType InviteAccountUser(string accountId, string email, string firstName, string lastName, string roleName, bool isOwner, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var account = AccountManager.GetAccount(accountId);

            if (!account.Provisioned)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Cannot invite users to unprovisioned accounts." };
            }

            #region Check Account Constraints

            var constraintsResponse = AccountConstraintsManager.CheckUserConstraint(accountId);
            if (constraintsResponse.isConstrained)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorCode = Sahara.Core.Common.Codes.DataAccessErrorCode.Constraint.ToString(), ErrorMessage = constraintsResponse.constraintMessage };
            }

            #endregion

            var result = AccountUserManager.InviteUser(accountId, email, firstName, lastName, roleName, isOwner);

            #region Log Account Activity

            if (result.isSuccess)
            {
                try
                {
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.AccountUser,
                        ActivityType.AccountUser_Invited,
                        "User invited",
                        requesterName + " has invited " + firstName + " " + lastName + " to the account",
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            return result;

        }


        public List<UserInvitation> GetAccountUserInvitations(string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = GetAccount(accountId, sharedClientKey);
            return AccountUserManager.GetInvitations(accountId, account.StoragePartition);
        }


        public UserInvitation GetAccountUserInvitation(string accountAtribute, string invitationKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return AccountUserManager.GetInvitation(accountAtribute, invitationKey);
        }


        public DataAccessResponseType DeleteAccountUserInvitation(string accountAttribute, string invitationKey, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            return AccountUserManager.DeleteInvitation(accountAttribute, invitationKey);

        }

        public DataAccessResponseType ResendAccountUserInvitation(string accountAttribute, string invitationKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return AccountUserManager.ResendInvitation(accountAttribute, invitationKey);
        }

        public DataAccessResponseType RegisterInvitedAccountUser(string accountAttribute, string email, string firstName, string lastName, string password, string role, bool Owner, string invitationKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountAttribute);

            if (account == null)
            {
                return new DataAccessResponseType
                {
                    isSuccess = false,
                    ErrorMessage = "This account does not exist."
                };
            }
            else
            {
                return AccountUserManager.CreateAccountUser(account.AccountID.ToString(), email, firstName, lastName, password, role, Owner, invitationKey);
            }


        }



        #endregion

        #region Password Management

        public DataAccessResponseType UpdateAccountUserPassword(string accountId, string email, string currentPassword, string newPassword, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);
            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var result = AccountUserManager.ChangePassword(accountId, email, currentPassword, newPassword);
            

            #region Log Account Activity

            if (result.isSuccess)
            {
                var account = AccountManager.GetAccount(accountId);

                try
                {
                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.AccountUser,
                        ActivityType.AccountUser_Invited,
                        "Password changed",
                        requesterName + " has updated their password",
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion

            return result;
        }


        public DataAccessResponseType ClaimLostPassword(string accountAttribute, string Email, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var result = AccountUserManager.ClaimLostPassword(accountAttribute, Email);

            return result;
        }

        public List<UserPasswordResetClaim> GetLostPasswordClaims(string accountAttribute, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                //return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
                return null;
            }

            #endregion

            return AccountUserManager.GetPasswordClaims(accountAttribute);
        }

        public string GetLostPasswordClaim(string accountAttribute, string passwordClaimKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            try
            {
                return AccountUserManager.GetPasswordClaim(accountAttribute, passwordClaimKey).Email;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public DataAccessResponseType ResetLostPassword(string accountAttribute, string passwordClaimKey, string newPassword, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var result = AccountUserManager.ResetPassword(accountAttribute, passwordClaimKey, newPassword);

            return result;
        }


        #endregion

        #endregion

        #region Subscription & Credit Card Methods


        public AccountCreditCardInfo GetCreditCardInfo(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.User,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                //return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
                return new AccountCreditCardInfo(); //<-- Return empty Card Info object
            }

            #endregion

            return AccountManager.GetAccountCreditCardInfo(accountId);
        }


        public DataAccessResponseType AddUpdateCreditCard(string accountId, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = "";
            var requesterEmail = "";

            if (requesterType == RequesterType.PlatformUser)
            {
                var user = PlatformUserManager.GetUser(requesterId);
                requesterName = user.FirstName;
                requesterEmail = user.Email;
            }
            else if (requesterType == RequesterType.AccountUser)
            {
                var user = AccountUserManager.GetUser(requesterId);
                requesterName = user.FirstName;
                requesterEmail = user.Email;
            }

            // We DO NOT validate this request so that anyone can update a card on an inactive, past_due or unpaid subscription subscription.
            // Presumably the UI can hide this feature from user roles that are not allowed

            /*
            
            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Admin,
                true);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
             * 
            */

            #endregion

            var result = AccountManager.AddUpdateCreditCard(accountId, cardName, cardNumber, cvc, expirationMonth, expirationYear);
            var account = AccountManager.GetAccount(accountId);

            #region Log Account Activity

            if (result.isSuccess)
            {
                //Invalidate the accounts snapshot cache so platform admins see the update quicker:
                PlatformSnapshotsManager.DestroyAccountSnapshotCache();

                try
                {

                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Account,
                        ActivityType.Account_CreditCard_AddedUpdated,
                        "Account credit card has been added or updated",
                        requesterName + " had added or updated the account credit card",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        ipAddress,
                        origin);
                }
                catch { }
            }

            #endregion

            return result;
        }


        public DataAccessResponseType CreateSubscripton(string accountId, string planName, string frequencyMonths, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request 

            //We allow a full exemption in this case in aorder to allow new customers to submit credit card data prior to account provisioning
            if(requesterType != RequesterType.Exempt)
            {
                var requesterName = string.Empty;
                var requesterEmail = string.Empty;

                var requestResponseType = RequestManager.ValidateRequest(requesterId,
                    requesterType, out requesterName, out requesterEmail,
                    Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                    null,
                    true,
                    true); //<--We ignore active state.

                if (!requestResponseType.isApproved)
                {
                    //Request is not approved, send results:
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
                }
            }
            else
            {
                //Make sure the account isn't already paying for a subscripton
                var account = AccountManager.GetAccount(accountId);

                if(account.StripeSubscriptionID != null)
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "You are already signed up for a subscription!" };
                }
            }


            #endregion

            var result = AccountManager.CreateSubscripton(accountId, planName, frequencyMonths, cardName, cardNumber, cvc, expirationMonth, expirationYear);

            //NOTE: Do not log anything for accounts here yet as they do not have a storage partition to log to yet!!!!

            return result;
        }


        public DataAccessResponseType UpdateAccountPlan(string accountId, string planName, string frequencyMonths, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Admin,
                null,
                true);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Check if this is a downgrade (REMOVED AS DOWNGRADES ARE NOT ALLOWD. CONSTRAINTS DO NOT NEED CHECKING, DOWNGRADE CHECK OCCURS ONE LEVEL DEEPER ANYWAY)
            /*
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);
            var newPlan = PaymentPlanManager.GetPaymentPlan(planName);

            if (account.PaymentPlan.MonthlyRate > newPlan.MonthlyRate)
            {
                #region Ensure that limitation contraints will not exist after downgrade

                var accountCapacity = AccountCapacityManager.GetAccountCapacity(accountId, false);

                bool limitationIssues = false;
                var objectArray = new List<string>();

                // Check User Contraints ------
                if ((accountCapacity.UsersCount -1) > newPlan.MaxUsers) //<--Offset by 1 to hide: platformadmin@[Config_PlatformEmail]
                {
                    limitationIssues = true;
                    objectArray.Add("users");
                }

                // Check Product Contraints ------
                //if (accountCapacity.ProductsCount > newPlan.MaxProducts)
                //{
                    //limitationIssues = true;
                    //objectArray.Add("products");
                //}

                // Check Categorization Contraints ------
                if (accountCapacity.CategorizationsCount > newPlan.MaxCategorizations)
                {
                    limitationIssues = true;
                    objectArray.Add("categorizatons");
                }


                // Check Properties Contraints ------
                if (accountCapacity.PropertiesCount > newPlan.MaxProperties)
                {
                    limitationIssues = true;
                    objectArray.Add("properties");
                }

                // Check Tag Contraints ------
                if (accountCapacity.TagsCount > newPlan.MaxTags)
                {
                    limitationIssues = true;
                    objectArray.Add("tags");
                }

                
                if(limitationIssues)
                {
                    

                    int count = 0;
                    int totalCount = objectArray.Count;
                    string itemsString = ""; ;

                    //Generate message:
                    foreach(string item in objectArray)
                    {
                        count ++;

                        if(count == totalCount)
                        {
                            itemsString += item;
                        }
                        else
                        {
                            if (count == (totalCount - 1))
                            {
                                itemsString += item + " and ";
                            }
                            else
                            {
                                itemsString += item + ", ";
                            }
                        }
                        
                    }

                    var downgradeIssuesMessage = "Cannot downgrade your account. You are over the maximum allowed " + itemsString + ". ";
                    downgradeIssuesMessage += "Please bring these items at or below the limits of the new plan before you downgrade.";

                    //Alert the user regarding the downgrade/limitation issues:
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = downgradeIssuesMessage };
                }

                #endregion
            }
            */
            #endregion

            var result = AccountManager.UpdateAccountPlan(accountId, planName, frequencyMonths);

            #region Log Account Activity

            if (result.isSuccess)
            {
                var account = AccountManager.GetAccount(accountId);

                try
                {
                    var stripePlanName = Sahara.Core.Common.Methods.Billing.GenerateStripePlanName(planName, PaymentPlanManager.GetPaymentFrequency(frequencyMonths).PaymentFrequencyName);

                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Account,
                        ActivityType.Account_Plan_Updated,
                        "Account plan updated to '" + stripePlanName + "'",
                        requesterName + " updated account plan to '" + stripePlanName + "'",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        ipAddress,
                        origin);
                }
                catch { }
            }

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            return result;

        }

        #endregion

        #region Capacity & Limitation Info

        public AccountCapacity GetAccountCapacity(string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return AccountCapacityManager.GetAccountCapacity(accountId);
        }

        #endregion


        #region Settings

        public AccountSettingsDocumentModel GetAccountSettings(string accountNameKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            // Get ACCOUNT
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            return AccountSettingsManager.GetAccountSettings(account);
        }

        public DataAccessResponseType UpdateAccountSettings(string accountNameKey, AccountSettingsDocumentModel accountSettingsDocumentModel, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.Manager,
                Sahara.Core.Settings.Accounts.Users.Authorization.Roles.Manager);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            // Get ACCOUNT
            var account = AccountManager.GetAccount(accountNameKey, true, AccountManager.AccountIdentificationType.AccountName);

            var response = AccountSettingsManager.UpdateAccountSettings(account, accountSettingsDocumentModel);

            if (response.isSuccess)
            {
                #region Invalidate Account API Caching Layer

                Sahara.Core.Common.Redis.ApiRedisLayer.InvalidateAccountApiCacheLayer(account.AccountNameKey);

                #endregion
            }

            return response;
        }

        #endregion

        #region Themes

        
        public List<ThemeModel> GetThemes(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return ThemesManager.GetThemes();
        }


        #endregion

        #region Service Consumer Helper Methods

        public string ConvertToAccountNameKey(string accountName)
        {
            return Sahara.Core.Common.Methods.AccountNames.ConvertToAccountNameKey(accountName);
        }

        #endregion

        #region Account Logs (Future: Create a Log API Web App to take banddwidth off of WCF for read only data used by clients)


        public List<AccountActivityLog> GetAccountLog(string accountId, int maxRecords, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);
            return AccountLogManager.GetAccountLog(accountId, account.StoragePartition, maxRecords);
        }


        public List<AccountActivityLog> GetAccountLogByCategory(string accountId, string categoryType, int maxRecords, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);
            return AccountLogManager.GetAccountLogByCategory(accountId, account.StoragePartition, (CategoryType)Enum.Parse(typeof(CategoryType), categoryType), maxRecords);
        }


        public List<AccountActivityLog> GetAccountLogByActivity(string accountId, string activityType, int maxRecords, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);
            return AccountLogManager.GetAccountLogByActivity(accountId, account.StoragePartition, (ActivityType)Enum.Parse(typeof(ActivityType), activityType), maxRecords);
        }


        public List<AccountActivityLog> GetAccountLogByUser(string accountId, string userId, int maxRecords, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);
            return AccountLogManager.GetAccountLogByUser(accountId, account.StoragePartition, userId, maxRecords);
        }

        public List<AccountActivityLog> GetAccountLogByObject(string accountId, string objectId, int maxRecords, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);
            return AccountLogManager.GetAccountLogByObject(accountId, account.StoragePartition, objectId, maxRecords);
        }

        //----- Types ----

        public List<string> GetAccountLogCategories(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return (List<string>)Enum.GetNames(typeof(CategoryType)).ToList();
        }

        public List<string> GetAccountLogActivities(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return (List<string>)Enum.GetNames(typeof(ActivityType)).ToList();
        }

        #endregion

        #region Account Closure

        public DataAccessResponseType CloseUnprovisionedAccount(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin, //<-- Only PlatformAdmin SuperAdmins can close an account
                null);//,
                      //true); //<-- Only AccountOwners that are Admins can close an account

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            var account = AccountManager.GetAccount(accountId, false, AccountManager.AccountIdentificationType.AccountID);

            var result = AccountManager.CloseUnprovisionedAccount(account);

            #region Log Account Activity, Send Emails, Etc...


            try
            {
                PlatformLogManager.LogActivity(
                    Sahara.Core.Logging.PlatformLogs.Types.CategoryType.Account,
                    Sahara.Core.Logging.PlatformLogs.Types.ActivityType.Account_UnprovisionedClosure,
                    "Unprovisioned account closed",
                    requesterName + " closed the unprovisioned account of: " + account.AccountName,
                    account.AccountID.ToString(),
                    account.AccountName,
                    requesterId,
                    requesterName,
                    requesterEmail
                    );
            }
            catch { }
            
            #endregion

            if(result.isSuccess)
            {
                PlatformSnapshotsManager.DestroyAccountSnapshotCache();
            }

            return result;
        }

        public DataAccessResponseType CloseAccount(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin, //<-- Only PlatformAdmin SuperAdmins can close an account
                null);//,
                //true); //<-- Only AccountOwners that are Admins can close an account

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            var account = AccountManager.GetAccount(accountId, false, AccountManager.AccountIdentificationType.AccountID);

            var result = AccountManager.CloseAccount(account);

            #region Log Account Activity, Send Emails, Etc...

            if (result.isSuccess && requesterType == RequesterType.AccountUser)
            {

                //Invalidate the accounts snapshot cache so platform admins see the update quicker:
                PlatformSnapshotsManager.DestroyAccountSnapshotCache();

                bool isPaid = false;

                //Log Closure and Alert Admins
                if(account.PaymentPlan.MonthlyRate == 0)
                {
                    //If account is Unprovisioned we close immediatly
                    AccountManager.CloseAccount(account);
                }
                else
                {
                    isPaid = true;
                    //If account is Paid we request closure and allow a Platform Admin to verify before commiting
                }

                Sahara.Core.Logging.PlatformLogs.Helpers.PlatformAccountClosureHelper.LogAccountClosureAndAlertAdmins(
                        account.AccountID.ToString(),
                        account.AccountName,
                        account.AccountNameKey,
                        requesterName,
                        requesterId,
                        requesterEmail,
                        isPaid
                    );

                try
                {

                    AccountLogManager.LogActivity(
                        accountId, account.StoragePartition,
                        CategoryType.Account,
                        ActivityType.Account_Closure_Requested,
                        "Account closure requested",
                        requesterName + " has requested account closure",
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }
            else if (result.isSuccess && requesterType == RequesterType.PlatformUser)
            {
                try
                {
                    PlatformLogManager.LogActivity(
                        Sahara.Core.Logging.PlatformLogs.Types.CategoryType.Account,
                        Sahara.Core.Logging.PlatformLogs.Types.ActivityType.Account_ClosureRequested,
                        "Account closure requested",
                        requesterName + " requested closure of the " + account.AccountName + " account",
                        account.AccountID.ToString(),
                        account.AccountName,
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }




            #endregion

            return result;

            #region A more conservative option is to have Platform Admins verify closures

            /*
            if(account.PaymentPlan.MonthlyRate == 0)
            {
                //If account is Unprovisioned
                
            we close immediatly
                AccountManager.CloseAccount(account);
            }
            else
            {
                //If account is Paid we request closure and allow a Platform Admin to verify before commiting
            }*/

            #endregion

        }

        public DataAccessResponseType ReactivateSubscription(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin //<-- Only PlatformAdmin SuperAdmins can reactivate a subscription
            );

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            var account = AccountManager.GetAccount(accountId, false, AccountManager.AccountIdentificationType.AccountID);

            #region Validate that account has a closure date

            if (account.AccountEndDate == null)
            {
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Account is not marked for closure" };
            }

            #endregion

            var result = AccountManager.ReactivateSubscription(account);

            #region Log Platform Activity

            if (result.isSuccess)
            {
                //Invalidate the accounts snapshot cache so platform admins see the update quicker:
                PlatformSnapshotsManager.DestroyAccountSnapshotCache();

                try
                {
                    PlatformLogManager.LogActivity(
                        Sahara.Core.Logging.PlatformLogs.Types.CategoryType.Account,
                        Sahara.Core.Logging.PlatformLogs.Types.ActivityType.Account_Subscription_Reactivated,
                        "Account subscription reactivated",
                        requesterName + " reactivated the subscription for " + account.AccountName,
                        account.AccountID.ToString(),
                        account.AccountName,
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                }
                catch { }
            }

            #endregion


            return result;
        }

        //public List<Core.Accounts.Models.Account> GetAccountsForClosureApproval()
       // {
            //return AccountManager.GetAccountsForClosureApproval();
        //}

        public bool DoesAccountRequireClosureApproval(string acountId)
        {

            return AccountManager.DoesAccountRequireClosureApproval(acountId);
        }

        public DataAccessResponseType UpdateAccountClosureApproval(string accountId, bool isApproved, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = GetAccount(accountId, sharedClientKey);

            //Platform Admin User MUST approve an account closure in order for a Custodian to deprovision and clear all data.

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin); //<-- Only Platform SuperAdmins can update approval status for an account closures

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            var result = AccountManager.UpdateAccountClosureApproval(accountId, isApproved);

            #region Log Platform Activity

            if (result.isSuccess)
            {
                //Invalidate the accounts snapshot cache so platform admins see the update quicker:
                PlatformSnapshotsManager.DestroyAccountSnapshotCache();

                try
                {
                    if (isApproved)
                    {
                        PlatformLogManager.LogActivity(
                        Sahara.Core.Logging.PlatformLogs.Types.CategoryType.Account,
                        Sahara.Core.Logging.PlatformLogs.Types.ActivityType.Account_Closure_Approved,
                        "Account closure approved",
                        requesterName + " approved the closure of the " + account.AccountName + " account",
                        account.AccountID.ToString(),
                        account.AccountName,
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                    }
                    else{
                        PlatformLogManager.LogActivity(
                        Sahara.Core.Logging.PlatformLogs.Types.CategoryType.Account,
                        Sahara.Core.Logging.PlatformLogs.Types.ActivityType.Account_Closure_Unapproved,
                        "Account closure unapproved",
                        requesterName + " unapproved the closure of the " + account.AccountName + " account",
                        account.AccountID.ToString(),
                        account.AccountName,
                        requesterId,
                        requesterName,
                        requesterEmail
                        );
                    }

                }
                catch { }
            }

            #endregion

            return result;

        }

        public DataAccessResponseType AccelerateAccountClosure(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin, //<-- Only PlatformAdmin SuperAdmins can close an account
                null);//,
                      //true); //<-- Only AccountOwners that are Admins can close an account

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            var account = AccountManager.GetAccount(accountId, false, AccountManager.AccountIdentificationType.AccountID);

            var result = AccountManager.AccelerateAccountClosure(account);

            #region Log Account Activity, Send Emails, Etc...


            try
            {
                PlatformLogManager.LogActivity(
                    Sahara.Core.Logging.PlatformLogs.Types.CategoryType.Account,
                    Sahara.Core.Logging.PlatformLogs.Types.ActivityType.Account_Closure_Accelerated,
                    "Account closure has been accelerated",
                    requesterName + " accelerated the closure of the " + account.AccountName + " account",
                    account.AccountID.ToString(),
                    account.AccountName,
                    requesterId,
                    requesterName,
                    requesterEmail
                    );
            }
            catch { }

            #endregion

            return result;
        }

        #endregion


    }
}
