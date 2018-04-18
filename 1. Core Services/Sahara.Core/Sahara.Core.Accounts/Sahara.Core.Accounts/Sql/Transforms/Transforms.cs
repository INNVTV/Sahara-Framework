using System;
using System.Data.SqlClient;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts.PaymentPlans.Public;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Logging.PlatformLogs.Helpers;

namespace Sahara.Core.Accounts.Sql.Transforms
{
    internal static class Transforms
    {

        public static Account DataReader_to_Account(SqlDataReader reader)
        {
            Account account = new Account();
            account.AccountName = (String)reader["AccountName"];
            account.AccountNameKey = (String)reader["AccountNameKey"];
            account.AccountID = (Guid)reader["AccountID"];


            #region Legacy Document Database Properties

            /*
            if (reader["DocumentDatabaseLink"] != DBNull.Value)
            {
                account.DocumentDatabaseLink = (String)reader["DocumentDatabaseLink"];
            }
            else
            {
                account.DocumentDatabaseLink = null;
            }

            if (reader["PropertiesCollectionLink"] != DBNull.Value)
            {
                account.PropertiesCollectionLink = (String)reader["PropertiesCollectionLink"];
            }
            else
            {
                account.PropertiesCollectionLink = null;
            }


            if (reader["SelfLinkReferencesDocumentLink"] != DBNull.Value)
            {
                account.SelfLinkReferencesDocumentLink = (String)reader["SelfLinkReferencesDocumentLink"];
            }
            else
            {
                account.SelfLinkReferencesDocumentLink = null;
            }
            */

            #endregion


            #region Transform Stripe Payments Properties

            if (reader["StripeCustomerID"] != DBNull.Value)
            {
                account.StripeCustomerID = (String)reader["StripeCustomerID"];
            }
            else
            {
                account.StripeCustomerID = null;
            }

            if (reader["StripeSubscriptionID"] != DBNull.Value)
            {
                account.StripeSubscriptionID = (String)reader["StripeSubscriptionID"];
            }
            else
            {
                account.StripeSubscriptionID = null;
            }


            if (reader["StripeCardID"] != DBNull.Value)
            {
                account.StripeCardID = (String)reader["StripeCardID"];
            }
            else
            {
                account.StripeCardID = null;
            }

            #endregion

            account.Active = (Boolean)reader["Active"];
            account.Provisioned = (Boolean)reader["Provisioned"];

            account.CreatedDate = (DateTime)reader["CreatedDate"];
            
            account.Closed = ((Boolean)reader["Closed"]);
            account.ClosureApproved = ((Boolean)reader["ClosureApproved"]);
            account.Delinquent = ((Boolean)reader["Delinquent"]);


            if (reader["CardExpiration"] != DBNull.Value)
            {
                account.CardExpiration = (DateTime)reader["CardExpiration"];
            }

            if (reader["LockedDate"] != DBNull.Value)
            {
                account.Locked = true;
            }
            else
            {
                account.Locked = false;
            }

            if (reader["Logo"] != DBNull.Value)
            {
                account.Logo = (String)reader["Logo"];
            }
            else
            {
                account.Logo = null;
            }


            account.PaymentPlanName = ((string)reader["PaymentPlanName"]);
            account.PaymentFrequencyMonths = ((int)reader["PaymentFrequencyMonths"]);



            if (reader["AccountEndDate"] != DBNull.Value)
            {
                account.AccountEndDate = (DateTime)reader["AccountEndDate"];
            }

            if (reader["PhoneNumber"] != DBNull.Value)
            {
                account.PhoneNumber = (String)reader["PhoneNumber"];
            }


            if (reader["SqlPartition"] != DBNull.Value)
            {
                account.SqlPartition = (String)reader["SqlPartition"];
            }

            if (reader["StoragePartition"] != DBNull.Value)
            {
                account.StoragePartition = (String)reader["StoragePartition"];
            }

            if (reader["SearchPartition"] != DBNull.Value)
            {
                account.SearchPartition = (String)reader["SearchPartition"];
            }

            /*
            if (reader["DocumentPartition"] != DBNull.Value)
             {
                account.DocumentPartition = (String)reader["DocumentPartition"];
                //account.ProductSearchIndex = Common.Methods.Strings.ConvertDocumentCollectionNameToSearchIndexName(account.DocumentPartition);
            }*/


            if (reader["ProvisionedDate"] != DBNull.Value)
            {
                account.ProvisionedDate = (DateTime)reader["ProvisionedDate"];
            }

            //Assign PaymentPlan & Frequency Objects
            try
            {
                account.PaymentPlan = PaymentPlanManager.GetPaymentPlan(account.PaymentPlanName);
                account.PaymentFrequency = PaymentPlanManager.GetPaymentFrequency(account.PaymentFrequencyMonths.ToString());
            }
            catch(Exception e)
            {
                #region Log Exception

                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to retreive paymentplan or paymentfrequency object during transformations for Account object",
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    account.AccountID.ToString(),
                    account.AccountName);

                #endregion
            }
            

            return account;
        }

        public static UserAccount DataReader_to_UserAccount(SqlDataReader reader)
        {
            UserAccount userAccount = new UserAccount();

            userAccount.AccountName = (String)reader["AccountName"];
            userAccount.AccountNameKey = (String)reader["AccountNameKey"];
            userAccount.AccountID = reader["AccountID"].ToString();
            userAccount.AccountOwner = (Boolean)reader["AccountOwner"];
            userAccount.AccountURL = Common.Methods.EndPoints.AccountManagementURL(userAccount.AccountNameKey);

            return userAccount;
        }



        public static AccountUser DataReader_to_BasicAccountUser(SqlDataReader reader)
        {
            AccountUser accountUser = new AccountUser();
            accountUser.UserName = (String)reader["UserName"];
            accountUser.Id = (String)reader["Id"];

            if (reader["Photo"] != DBNull.Value)
            {
                accountUser.Photo = (String)reader["Photo"];
            }
            else
            {
                accountUser.Photo = null;
            }

            accountUser.AccountID = (Guid)reader["AccountID"];
            accountUser.AccountName = (String)reader["AccountName"];
            accountUser.AccountNameKey = (String)reader["AccountNameKey"];
            accountUser.Email = (String)reader["Email"];

            accountUser.FirstName = (String)reader["FirstName"];
            accountUser.LastName = (String)reader["LastName"];
            accountUser.AccountOwner = (Boolean)reader["AccountOwner"];
            accountUser.Active = (Boolean)reader["Active"];
            accountUser.CreatedDate = (DateTime)reader["CreatedDate"];

            return accountUser;
        }
    }
}