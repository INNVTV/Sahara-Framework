using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Models;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Platform.Billing.Models;
using Sahara.Core.Platform.Partitioning.Models;
using Sahara.Core.Platform.Reports.Models;
using Sahara.Core.Platform.Requests.Models;
using Sahara.Core.Platform.Snapshots.Models;
using Sahara.Core.Platform.Users.Models;
using Sahara.Core.Settings.Models.Partitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Platform
{
    [ServiceContract]
    public interface IPlatformManagementService
    {
        /*==================================================================================
         * Platform Snapshots
         ==================================================================================*/

        [OperationContract]
        AccountsSnapshot GetAccountsShapshot(string sharedClientKey);

        [OperationContract]
        InfrastructureSnapshot GetInfrastructureShapshot(string sharedClientKey);

        [OperationContract]
        BillingSnapshot GetBillingShapshot(string sharedClientKey);

        /*==================================================================================
         * Platform Reports
         ==================================================================================*/

        [OperationContract]
        BillingReport GetBillingReport(int sinceHoursAgo, string sharedClientKey);

        /*==================================================================================
         * Document Partitions
         ==================================================================================*/

        //-- Partitions ----

        //[OperationContract]
        //DocumentPartition GetDocumentPartition(string documentPartitionId);

        // -- Partition Properties ---
        
        [OperationContract]
        DocumentPartitionCollectionProperties GetDocumentPartitionProperties(string documentPartitionId, string sharedClientKey);

        [OperationContract]
        DocumentPartitionTenantCollectionProperties GetDocumentPartitionTenantProperties(string accountId, string sharedClientKey);


        /*==================================================================================
         * SQL Partitions
         ==================================================================================*/

        [OperationContract]
        List<SqlPartition> GetSqlPartitions(string sharedClientKey);

        [OperationContract]
        SqlPartition GetSqlPartition(string databaseName, string sharedClientKey);

        [OperationContract]
        List<string> GetSqlPartitionSchemas(string databaseName, int maxResults, string sharedClientKey);

        [OperationContract]
        string GetSqlSchemaVersion(string accountId, string sharedClientKey);

        [OperationContract]
        List<SqlSchemaLog> GetSqlSchemaLog(string accountId, int maxResults, string sharedClientKey);

        /*==================================================================================
         * Search Partitions
         ==================================================================================*/

        [OperationContract]
        List<SearchPartition> GetSearchPartitions(string sharedClientKey);

        /*==================================================================================
         * Storage Partitions
         ==================================================================================*/

        [OperationContract]
        List<StoragePartition> GetStoragePartitions(string sharedClientKey);

        /*==================================================================================
         * IP Security Services
         ==================================================================================*/

        [OperationContract]
        List<string> GetAllowedIpAddresses(string sharedClientKey);

        [OperationContract]
        DataAccessResponseType AddAllowedIpAddress(string ip, string name, string sharedClientKey);


        /*==================================================================================
         
           Platform Users, Updates & Roles
         
        ==================================================================================*/

        /*==================================================================================
        * Create
        ==================================================================================*/

        //Bypasses invitation/verification system (otherwise use Invitation WOrkflow)

        [OperationContract]
        DataAccessResponseType CreatePlatformUser(string email, string firstName, string lastName, string password, string roleName, string requesterId, RequesterType requesterType, string sharedClientKey);


        /*==================================================================================
        * Get
        ==================================================================================*/

        [OperationContract]
        List<PlatformUser> GetPlatformUsers(string sharedClientKey);

        [OperationContract]
        PlatformUser GetPlatformUser(string userId, string sharedClientKey); //<-- Mostly used to keep views in sync with latest user role & properties

        [OperationContract]
        List<String> GetPlatformUserRoles(string sharedClientKey);

        /*==================================================================================
        * Delete
        ==================================================================================*/

        [OperationContract]
        DataAccessResponseType DeletePlatformUser(string userId, string requesterId, RequesterType requesterType, string sharedClientKey);


        /*==================================================================================
        * Update
        ==================================================================================*/

        [OperationContract]
        DataAccessResponseType UpdatePlatformUserRole(string userId, string newRole, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdatePlatformUserFullName(string userId, string newFirstName, string newLastName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdatePlatformUserActiveState(string userId, bool isActive, string requesterId, RequesterType requesterType, string sharedClientKey);

        //Will update the users login email as well as their username which will disengage this email from other accounts associated with that email address
        [OperationContract]
        DataAccessResponseType UpdatePlatformUserEmail(string userId, string newEmail, string requesterId, RequesterType requesterType, string sharedClientKey);

        // Future performance update: have client upload image to intermediary storage, submit location with imag eid for WCF processing (similar to other imageing solutions)
        [OperationContract]
        DataAccessResponseType UpdatePlatformUserProfilePhoto(string userId, byte[] imageByteArray, string requesterID, RequesterType requesterType, string sharedClientKey);


        /*==================================================================================
        * PlatformUser Invitations
        ==================================================================================*/

        [OperationContract] //This method kicks off the Invitation workflow
        DataAccessResponseType InvitePlatformUser(string email, string firstName, string lastName, string roleName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        List<PlatformInvitation> GetPlatformUserInvitations(string sharedClientKey);

        [OperationContract]
        PlatformInvitation GetPlatformUserInvitation(string invitationKey, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType DeletePlatformUserInvitation(string invitationKey, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ResendPlatformUserInvitation(string invitationKey, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RegisterInvitedPlatformUser(string email, string firstName, string lastName, string password, string role, string invitationKey, string sharedClientKey);



        /*==================================================================================
        * PlatformUser Password Services 
        ==================================================================================*/

        [OperationContract]
        DataAccessResponseType UpdatePlatformUserPassword(string email, string currentPassword, string newPassword, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ClaimLostPassword(string email, string sharedClientKey);

        [OperationContract]
        List<PlatformPasswordResetClaim> GetLostPasswordClaims(string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        String GetLostPasswordClaim(string passwordClaimKey, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ResetLostPassword(string passwordClaimKey, string newPassword, string sharedClientKey);



        /*==================================================================================
        * Platform Logs
        ==================================================================================*/

        [OperationContract]
        List<PlatformActivityLog> GetPlatformLog(int maxRecords, string sharedClientKey);
                                     
        [OperationContract]          
        List<PlatformActivityLog> GetPlatformLogByCategory(string categoryType, int maxRecords, string sharedClientKey);
                                     
        [OperationContract]          
        List<PlatformActivityLog> GetPlatformLogByActivity(string activityType, int maxRecords, string sharedClientKey);

        [OperationContract]
        List<PlatformActivityLog> GetPlatformLogByAccount(string accountId, int maxRecords, string sharedClientKey);
                             
        [OperationContract]          
        List<PlatformActivityLog> GetPlatformLogByUser(string userId, int maxRecords, string sharedClientKey);

        //-----

        [OperationContract]
        List<string> GetPlatformLogCategories(string sharedClientKey);

        [OperationContract]
        List<string> GetPlatformLogActivities(string sharedClientKey);
                                     
    }


}
