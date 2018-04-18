using Sahara.Core.Accounts.Models;
using Sahara.Core.Logging.AccountLogs.Models;
using Sahara.Core.Logging.AccountLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Partitioning.Models;
using Sahara.Core.Platform.Requests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts.Capacity.Models;
using Sahara.Core.Accounts.Themes.Models;
using Sahara.Core.Accounts.DocumentModels;

namespace WCF.WcfEndpoints.Contracts.Account
{
    [ServiceContract]
    public interface IAccountManagementService
    {

        #region Account(s)

        /*==================================================================================
        * GET Methods for Account(s)
       ==================================================================================*/

        // Listing & Searches ----------------

        [OperationContract]
        List<Sahara.Core.Accounts.Models.Account> GetAccounts(int pageNumber, int pageSize, string orderBy, string sharedClientKey);

        [OperationContract]
        List<Sahara.Core.Accounts.Models.Account> GetAccountsByFilter(string columnName, string value, int pageNumber, int pageSize, string orderBy, string sharedClientKey);

        [OperationContract]
        List<Sahara.Core.Accounts.Models.Account> SearchAccounts(string query, int maxResults, string sharedClientKey);

        [OperationContract]
        int GetAccountCount(string sharedClientKey);

        [OperationContract]
        int GetAccountCountByFilter(string columnName, string value, string sharedClientKey);


        // Individual Accounts ----------------

        [OperationContract]
        Sahara.Core.Accounts.Models.Account GetAccount(string id, string sharedClientKey);

        //[OperationContract]
        //Decimal GetAccountSystemMessages(string id);


        /*==================================================================================
        * UPDATE Methods for Account object(s)
        ==================================================================================*/

        [OperationContract]
        DataAccessResponseType UpdateAccountName(string accountId, string newAccountName, string requesterId, RequesterType requesterType, string sharedClientKey);

        /*==================================================================================
        * Centralized Helper Methods
        ==================================================================================*/

        [OperationContract]
        string ConvertToAccountNameKey(string accountName);

        #endregion


        #region Partitions & Schemas

        /*==================================================================================
         * GET Methods for Account Partitions & Schema Versions
         * 
         * /* Used in scenarios where we isolate accounts in SQL across database partitions and schema versions
         * The default is to use schemaless DocumentDB
        ==================================================================================*/

        /*
        [OperationContract]
        Decimal GetAccountSchemaVersion(string id);
         
        [OperationContract]
        List<AccountDatabasePartition> GetAccountPartitions(bool includeTenantCount);

        [OperationContract]
        AccountDatabasePartition GetAccountPartition(string id, bool includeTenantCount);
        */

        #endregion


        #region Account User(s)

        /*==================================================================================
         * Account Users, Updates & Roles
        ==================================================================================*/

        /*==================================================================================
        * Create
        ==================================================================================*/

        //Bypasses invitation/verification system (otherwise use AccountRegistration Services)
        [OperationContract]
        DataAccessResponseType CreateAccountUser(string accountId, string email, string firstName, string lastName, string password, string roleName, bool isOwner, string requesterId, RequesterType requesterType, string sharedClientKey);

        /*==================================================================================
        * Get
        ==================================================================================*/

        [OperationContract]
        List<AccountUser> GetAccountUsers(string accountId, string sharedClientKey);

        [OperationContract]
        AccountUser GetAccountUser(string userId, string sharedClientKey); //<-- Mostly used to keep views in sync with latest user role & properties

        [OperationContract]
        List<UserAccount> GetAllAccountsForEmail(string email, string sharedClientKey);

        [OperationContract]
        List<String> GetAccountUserRoles(string sharedClientKey);

        /*==================================================================================
        * Delete
        ==================================================================================*/

        [OperationContract]
        DataAccessResponseType DeleteAccountUser(string accountId, string userId, string requesterId, RequesterType requesterType, string sharedClientKey);


        /*==================================================================================
        * Update
        ==================================================================================*/

        [OperationContract]
        DataAccessResponseType UpdateAccountUserRole(string accountId, string userId, string newRole, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateAccountUserFullName(string accountId, string userId, string newFirstName, string newLastName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateAccountOwnershipStatus(string accountId, string userId, bool isOwner, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateAccountUserActiveState(string accountId, string userId, bool isActive, string requesterId, RequesterType requesterType, string sharedClientKey);

        //Will update the users login email as well as their username which will disengage this email from other accounts associated with that email address
        [OperationContract]
        DataAccessResponseType UpdateAccountUserEmail(string accountId, string userId, string newEmail, string requesterId, RequesterType requesterType, string sharedClientKey);

        // Future performance update: have client upload image to intermediary storage, submit location with imag eid for WCF processing (similar to other imageing solutions)
        [OperationContract]
        DataAccessResponseType UpdateAccountUserProfilePhoto(string accountId, string userId, byte[] imageByteArray, string requesterID, RequesterType requesterType, string sharedClientKey);



        /*==================================================================================
         * AccountUser Invitations
         ==================================================================================*/

        [OperationContract] //This method kicks off the Invitation workflow
        DataAccessResponseType InviteAccountUser(string accountId, string email, string firstName, string lastName, string roleName, bool isOwner, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        List<UserInvitation> GetAccountUserInvitations(string accountId, string sharedClientKey);

        [OperationContract]
        UserInvitation GetAccountUserInvitation(string accountAttribute, string invitationKey, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType DeleteAccountUserInvitation(string accountAttribute, string invitationKey, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ResendAccountUserInvitation(string accountAttribute, string invitationKey, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RegisterInvitedAccountUser(string accountAttribute, string email, string firstName, string lastName, string password, string role, bool isOwner, string invitationKey, string sharedClientKey);




        /*==================================================================================
        * AccountUser Password Services 
        ==================================================================================*/

        [OperationContract]
        DataAccessResponseType UpdateAccountUserPassword(string accountId, string email, string currentPassword, string newPassword, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ClaimLostPassword(string accountAttribute, string email, string sharedClientKey);

        [OperationContract]
        List<UserPasswordResetClaim> GetLostPasswordClaims(string accountAttribute, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        String GetLostPasswordClaim(string accountAttribute, string passwordClaimKey, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ResetLostPassword(string accountAttribute, string passwordClaimKey, string newPassword, string sharedClientKey);


        #endregion


        #region Subscription & Card Management

        /*==================================================================================
        * Subscriptions & Credit Cards
        ==================================================================================*/


        [OperationContract]
        AccountCreditCardInfo GetCreditCardInfo(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType AddUpdateCreditCard(string accountId, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType CreateSubscripton(string accountId, string planName, string frequencyMonths, string cardName, string cardNumber, string cvc, string expirationMonth, string expirationYear, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateAccountPlan(string accountId, string planName, string frequencyMonths, string requesterId, RequesterType requesterType, string ipAddress, string origin, string sharedClientKey);

        #endregion

        
        #region Capacity & Limitation Info

        [OperationContract]
        AccountCapacity GetAccountCapacity(string accountId, string sharedClientKey);

        #endregion


        #region Settings

        [OperationContract]
        AccountSettingsDocumentModel GetAccountSettings(string accountNameKey, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateAccountSettings(string accountNameKey, AccountSettingsDocumentModel accountSettingsDocumentModel, string requesterId, RequesterType requesterType, string sharedClientKey);

        #endregion

        #region Themes

        [OperationContract]
        List<ThemeModel> GetThemes(string sharedClientKey);

        
        #endregion

        #region Account Logs

        [OperationContract]
        List<AccountActivityLog> GetAccountLog(string accountId, int maxRecords, string sharedClientKey);

        [OperationContract]
        List<AccountActivityLog> GetAccountLogByCategory(string accountId, string categoryType, int maxRecords, string sharedClientKey);

        [OperationContract]
        List<AccountActivityLog> GetAccountLogByActivity(string accountId, string activityType, int maxRecords, string sharedClientKey);

        [OperationContract]
        List<AccountActivityLog> GetAccountLogByUser(string accountId, string userId, int maxRecords, string sharedClientKey);

        [OperationContract]
        List<AccountActivityLog> GetAccountLogByObject(string accountId, string objectId, int maxRecords, string sharedClientKey);

        //-----------

        [OperationContract]
        List<string> GetAccountLogCategories(string sharedClientKey);

        [OperationContract]
        List<string> GetAccountLogActivities(string sharedClientKey);

        #endregion

        #region Account Closure

        [OperationContract]
        DataAccessResponseType CloseUnprovisionedAccount(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType CloseAccount(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ReactivateSubscription(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey);

        //[OperationContract]
        //List<Core.Accounts.Models.Account> GetAccountsForClosureApproval();

        [OperationContract]
        bool DoesAccountRequireClosureApproval(string acountId);

        [OperationContract]
        DataAccessResponseType UpdateAccountClosureApproval(string accountId, bool isApproved, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType AccelerateAccountClosure(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey);

        #endregion

    }



}
