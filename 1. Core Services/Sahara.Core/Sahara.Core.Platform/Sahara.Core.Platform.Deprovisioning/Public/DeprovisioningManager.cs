using System;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Common.Services.Stripe;
using Sahara.Core.Platform.Deprovisioning.Internal;
using Newtonsoft.Json;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Common.Services.CloudFlare.Public;

namespace Sahara.Core.Platform.Deprovisioning
{
    
    public static class DeprovisioningManager
    {
        public static DataAccessResponseType DeprovisionAccount(Account account)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            if(account.AccountID == Guid.Empty)
            {
                response.isSuccess = false;
                response.ErrorMessage = "No account to deprovision";
                return response;
            }

            try
            {

                // 0. Get Account Users:
                account.Users = AccountUserManager.GetUsers(account.AccountID.ToString());



                #region LOG ACTIVITY (Deprovisioning Started)

                
                if (account.Provisioned)
                {
                    PlatformLogManager.LogActivity(
                            CategoryType.Account,
                            ActivityType.Account_Deprovisioning_Started,
                            "Deprovisioning of '" + account.AccountName + "' has started",
                            "AccountID: '" + account.AccountID +
                            "' SqlPartition: '" + account.SqlPartition +
                            "' DocumentPartition: '" + account.DocumentPartition +
                            "' StripeCustomerID: '" + account.StripeCustomerID +
                            "'"
                        );
                }
                else if (!account.Provisioned)
                {
                    //Account has not been provisioned, only delete the account and user objects, This will be a simple purge and not a full deprovisioning
                    PlatformLogManager.LogActivity(CategoryType.Account, ActivityType.Account_Purge_Started, "Purging of unprovisioned account '" + account.AccountName + "' has started", "AccountID: '" + account.AccountID + "' Account Owner: '" + account.Users[0].UserName + "'");
                }
                 
                #endregion


                // Owners of accounts that have been provisioned will get an email, create a list of owner emails before all users are delted
                var accountOwnerEmails = AccountManager.GetAccountOwnerEmails(account.AccountID.ToString());

                string accountOwners = string.Empty;

                foreach (string ownerEmail in accountOwnerEmails)
                {
                    accountOwners += ownerEmail + " ";
                }


                #region STEPS 1-3 DELETE ACCOUNT USERS AND ACCOUNT

                // 1. Delete All Account Users
                AccountDeprovisioning.DeleteAllAccountUsers(account);

                // 2. Delete Account 
                AccountDeprovisioning.DeleteAccount(account);

                // 3. Delete Customer in Stripe if the account has a StripeCustomerID
                if (account.StripeCustomerID != null)
                {
                    try
                    {
                        var stripeManager = new StripeManager();
                        stripeManager.DeleteCustomer(account.StripeCustomerID);
                    }
                    catch
                    {

                    }
                }

                #endregion

                if (!account.Provisioned)
                {
                    #region Log closure if account is not provisioned
                    //Account has never been provisioned, since we already deleted the account and all associated users, we are done. Log activity completion and return result:
                    PlatformLogManager.LogActivity(
                        CategoryType.Account,
                        ActivityType.Account_Purged,
                        "Purging of unprovisioned account '" + account.AccountName + "' has completed",
                        "Account Owners: '" + accountOwners + "'",
                        account.AccountID.ToString(),
                        account.AccountName,
                        null,
                        null,
                        null,
                        null,
                        null,
                        JsonConvert.SerializeObject(account)
                    );


                    //Log the closed account
                    PlatformLogManager.LogActivity(
                        CategoryType.Account,
                        ActivityType.Account_Closed,
                        "Unprovisioned",
                        account.AccountNameKey,
                        account.AccountID.ToString(),
                        account.AccountName,
                        null,
                        null,
                        null,
                        null,
                        null,
                        JsonConvert.SerializeObject(account));

                    response.isSuccess = true;
                    response.SuccessMessage = "Purging of account '" + account.AccountID + "' Complete!";


                    #endregion
                }
                else
                {
                    // 4. Clear SQL Data Schema & 
                    AccountDeprovisioning.DestroySqlSchemaAndTables(account);

                    // 5. Clear Table Storage Data
                    AccountDeprovisioning.DestroyTableStorageData(account);

                    // 6. Clear Blob Storage Data
                    AccountDeprovisioning.DestroyBlobStorageData(account);

                    // 7. Clear Document Data (Retired)
                    AccountDeprovisioning.DestroyDocumentCollection(account);

                    // 8. Clear Search Indexes
                    AccountDeprovisioning.DestroySearchIndexes(account);

                    // 9. Decriment both STORAGE & SEARCH Partitions
                    Sql.Statements.UpdateStatements.UpdatePartitionsTenantCounts(account.StoragePartition, account.SearchPartition);

                    // 10. Log Activity
                    #region Logging


                    PlatformLogManager.LogActivity(
                        CategoryType.GarbageCollection,
                        ActivityType.GarbageCollection_ClosedAccounts,
                        "All resources for account '" + account.AccountName + "' have been destroyed",
                        "Purged resources now available to new accounts",
                        account.AccountID.ToString(),
                        account.AccountName,
                        null,
                        null,
                        null,
                        null,
                        null,
                        JsonConvert.SerializeObject(account)
                        );


                    PlatformLogManager.LogActivity(
                        CategoryType.Account,
                        ActivityType.Account_Deprovisioned,
                        "Deprovisioning of '" + account.AccountName + "' has completed",
                        "SqlPartition: '" + account.SqlPartition + "'",
                        account.AccountID.ToString(),
                        account.AccountName,
                        null,
                        null,
                        null,
                        null,
                        null,
                        JsonConvert.SerializeObject(account)
                        );

                    //Log the closed account
                    PlatformLogManager.LogActivity(
                        CategoryType.Account,
                        ActivityType.Account_Closed,
                        "Deprovisioned",
                        account.AccountNameKey + " | " + account.PaymentPlanName,
                        account.AccountID.ToString(),
                        account.AccountName,
                        null,
                        null,
                        null,
                        null,
                        null,
                        JsonConvert.SerializeObject(account));

                    #endregion

                    // 11. Email all account users regarding closure:
                    EmailManager.Send(
                        accountOwnerEmails,
                        Settings.Endpoints.Emails.FromAlerts,
                        Settings.Copy.EmailMessages.DeprovisioningComplete.FromName,
                        Settings.Copy.EmailMessages.DeprovisioningComplete.Subject,
                        String.Format(Settings.Copy.EmailMessages.DeprovisioningComplete.Body, account.AccountName),
                        true);

                    // 12. Destroy ALL caches associated with an account
                    AccountManager.DestroyAccountCaches(account.AccountID.ToString(), account.AccountNameKey, account.StripeCustomerID);

                    // 13. Destroy subdomains
                    try
                    {
                        var cloudFlareResult = CloudFlareManager.RemoveSubdomains(account.AccountNameKey);

                        if(cloudFlareResult.isSuccess == false)
                        {
                            //Log exception and email platform admins
                            PlatformExceptionsHelper.LogErrorAndAlertAdmins(
                                cloudFlareResult.ErrorMessage,
                                "attempting to remove cloudflare subdomains for the '" + account.AccountName + "' account during deprovisioning.",
                                System.Reflection.MethodBase.GetCurrentMethod(),
                                account.AccountID.ToString(),
                                account.AccountName
                            );
                        }
                    }
                    catch (Exception e)
                    {
                        //Log exception and email platform admins
                        PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                            e,
                            "attempting to remove cloudflare subdomains for the '" + account.AccountName + "' account during deprovisioning.",
                            System.Reflection.MethodBase.GetCurrentMethod(),
                            account.AccountID.ToString(),
                            account.AccountName
                        );
                    }

                    response.isSuccess = true;
                    response.SuccessMessage = "Deprovisioning of account '" + account.AccountID + "' Complete!";
                }

                //TODO: Log purged account into ClosedAccounts Table


            }
            catch(Exception e)
            {
                #region LOG ERROR (Deprovisioning Errors)

                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to deprovision or purge account: " + account.AccountName,
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName
                );

                #endregion

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
            }

            // Archive the closed account
            //ClosedAccountsStorageManager.ArchiveClosedAccount(account);


            return response;
        }
    }
    
}
