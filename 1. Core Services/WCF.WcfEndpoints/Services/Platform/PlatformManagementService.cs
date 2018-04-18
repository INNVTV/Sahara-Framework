using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests.Models;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Platform.Users.Models;
using Sahara.Core.Platform.Users;
using Sahara.Core.Platform.Users.TableEntities;
using WCF.WcfEndpoints.Contracts.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Logging.PlatformLogs.Models;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Platform.Snapshots.Models;
using Sahara.Core.Platform.Snapshots.Public;
using Sahara.Core.Platform.Reports.Models;
using Sahara.Core.Platform.Reports.Public;
using Sahara.Core.Platform.Billing.Models;
using Sahara.Core.Platform.Billing;
using Sahara.Core.Platform.Partitioning.Models;
using Sahara.Core.Platform.Partitioning.Public;
using Sahara.Core.Accounts;
using Sahara.Core.Platform.Partitioning;
using Sahara.Core.Settings.Models.Partitions;

namespace WCF.WcfEndpoints.Service.Platform
{
    
    public class PlatformManagementService : IPlatformManagementService
    {
        #region Platform Snapshots

        public AccountsSnapshot GetAccountsShapshot(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformSnapshotsManager.GetAccountsSnapshot();
        }

        public InfrastructureSnapshot GetInfrastructureShapshot(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformSnapshotsManager.GetInfrastructureSnapshot();
        }

        public BillingSnapshot GetBillingShapshot(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformSnapshotsManager.GetBillingSnapshot();

        }

        #endregion

        #region Platform Reports

        public BillingReport GetBillingReport(int sinceHoursAgo, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformReportsManager.GetBillingReport(sinceHoursAgo);
        }


        #endregion

        #region Document Partitions


        #region Document Partitions

        //public DocumentPartition GetDocumentPartition(string documentPartitionId)
        //{
            //return DocumentPartitioningManager.GetDocumentPartition(documentPartitionId);
        //}

        #endregion

        #region Document Partition Properties

        public DocumentPartitionCollectionProperties GetDocumentPartitionProperties(string documentPartitionId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return DocumentPartitioningManager.GetDocumentPartitionProperties(documentPartitionId);
        }

        public DocumentPartitionTenantCollectionProperties GetDocumentPartitionTenantProperties(string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);
            return DocumentPartitioningManager.GetDocumentPartitionTenantProperties(accountId, account.DocumentPartition);
        }

        #endregion

        #endregion

        #region SQL Partitions

        public List<SqlPartition> GetSqlPartitions(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return SqlPartitioningManager.GetSqlPartitions(true);
        }

        public SqlPartition GetSqlPartition(string partitionName, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return SqlPartitioningManager.GetSqlPartition(partitionName);
        }

        public List<string> GetSqlPartitionSchemas(string databaseName, int maxResults, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return SqlPartitioningManager.GetSqlPartitionSchemas(databaseName, maxResults);
        }

        public string GetSqlSchemaVersion(string accountId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);

            var version = SqlPartitioningManager.GetSqlPartitionTenantSchemaVersion(account.SqlPartition, account.SchemaName);

            return String.Format("{0:0.0}", version);
        }

        public List<SqlSchemaLog> GetSqlSchemaLog(string accountId, int maxResults, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var account = AccountManager.GetAccount(accountId);

            return SqlPartitioningManager.GetSqlPartitionTenantSchemaLog(account.SqlPartition, account.SchemaName, maxResults);

        }

        #endregion

        #region Search Partitions

        public List<SearchPartition> GetSearchPartitions(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var searchPartitions = SearchPartitioningManager.GetSearchPartitions();

            //Remove Keys & Add Max
            foreach(var partition in searchPartitions)
            {
                partition.Key = "[Redacted]";


                partition.MaxTenants = Int32.Parse((partition.Plan.Substring(partition.Plan.LastIndexOf("-") + 1)));

                /* MAx Tenatnts are now pulled from the SarchPlan name

                if (partition.Plan.ToLower() == "free")
                {
                    partition.MaxTenants = Sahara.Core.Settings.Platform.Partitioning.MaximumTenantsPerFreeSearchService;
                }
                else if(partition.Plan.ToLower() == "basic")
                {
                    partition.MaxTenants = Sahara.Core.Settings.Platform.Partitioning.MaximumTenantsPerBasicSearchServiceShared;
                }
                else if(partition.Plan.ToLower() == "s1")
                {
                    partition.MaxTenants = Sahara.Core.Settings.Platform.Partitioning.MaximumTenantsPerS1SearchServiceShared;
                }
                else if (partition.Plan.ToLower() == "s2")
                {
                    partition.MaxTenants = Sahara.Core.Settings.Platform.Partitioning.MaximumTenantsPerS2SearchServiceShared;
                }
                else if (partition.Plan.ToLower() == "basic-dedicated")
                {
                    partition.MaxTenants = Sahara.Core.Settings.Platform.Partitioning.MaximumTenantsPerBasicSearchServiceDedicated;
                }
                else if (partition.Plan.ToLower() == "s1-dedicated")
                {
                    partition.MaxTenants = Sahara.Core.Settings.Platform.Partitioning.MaximumTenantsPerS1SearchServiceDedicated;
                }
                else if (partition.Plan.ToLower() == "s2-dedicated")
                {
                    partition.MaxTenants = Sahara.Core.Settings.Platform.Partitioning.MaximumTenantsPerS2SearchServiceDedicated;
                }*/
            }

            return searchPartitions;
        }

        #endregion

        #region Storage Partitions

        public List<StoragePartition> GetStoragePartitions(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var storagePartitions = StoragePartitioningManager.GetStoragePartitions();

            //Remove Keys & Add Max
            foreach (var partition in storagePartitions)
            {
                partition.Key = "[Redacted]";
                partition.MaxTenants = Sahara.Core.Settings.Platform.Partitioning.MaximumTenantsPerStorageAccount;
            }

            return storagePartitions;
        }

        #endregion

        #region Platform Security (Move to new service?)

        public List<string> GetAllowedIpAddresses(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var ipAddresses = new List<string>();

            return ipAddresses;
        }

        public DataAccessResponseType AddAllowedIpAddress(string ip, string name, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            //Add to DB for CoreServices Startup

            //Add to currently running static class

            //Allow ip in real time

            return new DataAccessResponseType();
        }

        #endregion

        #region Platform User Services

        #region Create Methods
        public DataAccessResponseType CreatePlatformUser(string email, string firstName, string lastName, string password, string roleName, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            var result = PlatformUserManager.CreatePlatformUser(email, firstName, lastName, password, roleName);

            #region Log Platform Activity

            if (result.isSuccess)
            {
                try
                {

                    PlatformLogManager.LogActivity(
                        CategoryType.PlatformUser,
                        ActivityType.PlatformUser_Created,
                        "User created",
                        requesterName + " created new user: '" + firstName + " " + lastName + "'",
                        null,
                        null,
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

        #region Get Methods

        public List<PlatformUser> GetPlatformUsers(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformUserManager.GetUsers();
        }

        // Mostly used to keep views in sync with latest user role & properties (WCF clients cannot consume Identity versions)
        public PlatformUser GetPlatformUser(string userId, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformUserManager.GetUser(userId);
        }

        public List<String> GetPlatformUserRoles(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return Sahara.Core.Settings.Platform.Users.Authorization.Roles.GetRoles();
        }

        #endregion

        #region Update Methods

        public DataAccessResponseType UpdatePlatformUserRole(string userId, string newRole, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            var result =  PlatformUserManager.UpdateUserRole(userId, newRole);

            #region Log Platform Activity

            if (result.isSuccess)
            {
                try
                {
                    var user = PlatformUserManager.GetUser(userId);

                    PlatformLogManager.LogActivity(
                        CategoryType.PlatformUser,
                        ActivityType.PlatformUser_Role_Updated,
                        "Role updated",
                        requesterName + " updated " + user.FullName + "'s role to " + newRole,
                        null,
                        null,
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

        public DataAccessResponseType UpdatePlatformUserFullName(string userId, string newFirstName, string newLastName, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            var user = PlatformUserManager.GetUser(userId);

            var result = PlatformUserManager.UpdateFullName(userId, newFirstName, newLastName);

            #region Log Platform Activity

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
                        logDescription = requesterName + " updated " + user.FullName + "'s name to " + newFirstName + " " + newLastName;
                    }

                    PlatformLogManager.LogActivity(
                        CategoryType.PlatformUser,
                        ActivityType.PlatformUser_Edited,
                        "Name updated",
                        logDescription,
                        null,
                        null,
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

        public DataAccessResponseType UpdatePlatformUserActiveState(string userId, bool isActive, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            var result = PlatformUserManager.UpdateActiveState(userId, isActive);

            #region Log Platform Activity

            if (result.isSuccess)
            {
                try
                {
                    var user = PlatformUserManager.GetUser(userId);

                    var description = string.Empty;

                    if (isActive)
                    {
                        description = requesterName + " activated " + user.FullName + "'s account";

                    }
                    else
                    {
                        description = requesterName + " deactivated " + user.FullName + "'s account";
                    }

                    PlatformLogManager.LogActivity(
                        CategoryType.PlatformUser,
                        ActivityType.PlatformUser_Edited,
                        "Active state updated",
                        description,
                        null,
                        null,
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

        public DataAccessResponseType UpdatePlatformUserEmail(string userId, string newEmail, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            var result = PlatformUserManager.UpdateEmail(userId, newEmail);

            #region Log Platoform Activity

            if (result.isSuccess)
            {
                try
                {
                    var logDescription = string.Empty;

                    if(userId == requesterId)
                    {
                        logDescription = requesterName + " has updated their email to " + newEmail;
                    }
                    else
                    {
                        var user = PlatformUserManager.GetUser(userId);
                        logDescription = requesterName + " updated " + user.FullName + "'s email to " + newEmail;
                    }
                    
                    PlatformLogManager.LogActivity(
                        CategoryType.PlatformUser,
                        ActivityType.PlatformUser_Edited,
                        "Email updated",
                        logDescription,
                        null,
                        null,
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
        public DataAccessResponseType UpdatePlatformUserProfilePhoto(string userId, byte[] imageByteArray, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion


            var result = PlatformUserManager.UpdateProfilePhoto(userId, imageByteArray);

            #region Log Platoform Activity

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
                        var user = PlatformUserManager.GetUser(userId);
                        logDescription = requesterName + " updated " + user.FullName + "'s profile photo";
                    }

                    PlatformLogManager.LogActivity(
                        CategoryType.PlatformUser,
                        ActivityType.PlatformUser_Edited,
                        "Photo updated",
                        logDescription,
                        null,
                        null,
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

        public DataAccessResponseType DeletePlatformUser(string userId, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            var user = PlatformUserManager.GetUser(userId);

            var result = PlatformUserManager.DeleteUser(userId);

            #region Log Platform Activity

            if (result.isSuccess)
            {
                try
                {
                    PlatformLogManager.LogActivity(
                        CategoryType.PlatformUser,
                        ActivityType.PlatformUser_Deleted,
                        "User deleted",
                        requesterName + " deleted " + user.FullName + "'s account",
                        null,
                        null,
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

        #region PlatformUser Invitations

        public DataAccessResponseType InvitePlatformUser(string email, string firstName, string lastName, string roleName, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            var result = PlatformUserManager.InviteUser(email, firstName, lastName, roleName);

            #region Log Platform Activity

            if (result.isSuccess)
            {
                try
                {
                    PlatformLogManager.LogActivity(
                        CategoryType.PlatformUser,
                        ActivityType.PlatformUser_Invited,
                        "User invited",
                        requesterName + " has invited " + firstName + " " + lastName + " to the platform",
                        null,
                        null,
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

        public List<PlatformInvitation> GetPlatformUserInvitations(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformUserManager.GetInvitations();
        }

        public PlatformInvitation GetPlatformUserInvitation(string invitationKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformUserManager.GetInvitation(invitationKey);
        }

        public DataAccessResponseType DeletePlatformUserInvitation(string invitationKey, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            return PlatformUserManager.DeleteInvitation(invitationKey);
        }

        public DataAccessResponseType ResendPlatformUserInvitation(string invitationKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformUserManager.ResendInvitation(invitationKey);
        }

        public DataAccessResponseType RegisterInvitedPlatformUser(string email, string firstName, string lastName, string password, string role, string invitationKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformUserManager.CreatePlatformUser(email, firstName, lastName, password, role, invitationKey);
        }



        #endregion

        #region Password Management

        public DataAccessResponseType UpdatePlatformUserPassword(string email, string currentPassword, string newPassword, string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            var result = PlatformUserManager.ChangePassword(email, currentPassword, newPassword);

            #region Log Platform Activity

            if (result.isSuccess)
            {
                try
                {
                    PlatformLogManager.LogActivity(
                        CategoryType.PlatformUser,
                        ActivityType.PlatformUser_Invited,
                        "Password changed",
                        requesterName + " has updated their password",
                        null,
                        null,
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

        public DataAccessResponseType ClaimLostPassword(string email, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformUserManager.ClaimLostPassword(email);
        }

        public List<PlatformPasswordResetClaim> GetLostPasswordClaims(string requesterId, RequesterType requesterType, string sharedClientKey)
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
                return null;

            }

            #endregion

            return PlatformUserManager.GetPasswordClaims();
         
        }

        public string GetLostPasswordClaim(string passwordClaimKey, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            try
            {
                return PlatformUserManager.GetPasswordClaim(passwordClaimKey).Email;
            }
            catch
            {
                return null;
            }
            
        }

        public DataAccessResponseType ResetLostPassword(string passwordClaimKey, string newPassword, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformUserManager.ResetPassword(passwordClaimKey, newPassword);
        }

        #endregion

        #endregion

        #region Platorm Logs (Future: Create a Log API Web App to take banddwidth off of WCF for read only data used by clients)


        public List<PlatformActivityLog> GetPlatformLog(int maxRecords, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformLogManager.GetPlatformLog(maxRecords);
        }


        public List<PlatformActivityLog> GetPlatformLogByCategory(string categoryType, int maxRecords, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformLogManager.GetPlatformLogByCategory((CategoryType)Enum.Parse(typeof(CategoryType), categoryType), maxRecords);
        }


        public List<PlatformActivityLog> GetPlatformLogByActivity(string activityType, int maxRecords, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformLogManager.GetPlatformLogByActivity((ActivityType)Enum.Parse(typeof(ActivityType), activityType), maxRecords);
        }


        public List<PlatformActivityLog> GetPlatformLogByAccount(string accountId, int maxRecords, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformLogManager.GetPlatformLogByAccount(accountId, maxRecords);
        }

        public List<PlatformActivityLog> GetPlatformLogByUser(string userId, int maxRecords, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return PlatformLogManager.GetPlatformLogByUser(userId, maxRecords);
        }


        //----- Types ----

        public List<string> GetPlatformLogCategories(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return (List<string>)Enum.GetNames(typeof(CategoryType)).ToList();
        }

        public List<string> GetPlatformLogActivities(string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return (List<string>)Enum.GetNames(typeof(ActivityType)).ToList();
        }

        #endregion
    }


}
