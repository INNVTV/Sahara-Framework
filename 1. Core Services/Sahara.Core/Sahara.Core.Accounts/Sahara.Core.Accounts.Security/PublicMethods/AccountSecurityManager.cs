using System;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;

namespace Sahara.Core.Accounts.Security.PublicMethods
{
    public static class AccountSecurityManager
    {
        public static DataAccessResponseType AuthenticateUser(string accountName, string email, string password)
        {

            var response = new DataAccessResponseType();


            #region Refactoring Notes

            /*
             * In scenarios where users are only one to an account we make the account name the "UserName"
             * We can then look up the email address associated with the account (or vice versa depending on if it's an email or username login scenario)
             * This lookup data can be cached in Redis
             * 
             */

            #endregion


            //Verifiy all prameters
            if (string.IsNullOrEmpty(accountName))
            {
                response.ErrorMessages.Add("Please include an account name.");
            }
            if (string.IsNullOrEmpty(email))
            {
                response.ErrorMessages.Add("Please include an email.");
            }
            if (string.IsNullOrEmpty(password))
            {
                response.ErrorMessages.Add("Please include a password.");
            }

            if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                response.isSuccess = false;
                response.ErrorMessage = "Not all parameters contain a value!";
                return response;
            }








            try
            {
                //Get the account associated with the attempted login
                //var account = AccountManager.GetAccountByNameKey(Sahara.Core.Common.AccountNames.ConvertToAccountNameKey(login.AccountName), false);
                var account = AccountManager.GetAccount(accountName, false, AccountManager.AccountIdentificationType.AccountName);


                if(account == null)
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "This account does not exist.";
                    response.ErrorMessages.Add("This account does not exist.");
                    return response;
                }

                //Deny access if account is marked for closure/deprovisioning
                if (String.IsNullOrEmpty(account.PaymentPlanName))
                {
                    response.isSuccess = false;
                    response.ErrorMessage = "This account is closed.";
                    response.ErrorMessages.Add("This account is closed.");
                    return response;
                }

                //Derive UserName from Email + AccountID
                string globalUniqueUserName = Sahara.Core.Common.Methods.AccountUserNames.GenerateGlobalUniqueUserName(email, account.AccountID.ToString());

                //Get user with 'Login' info (username + password)
                response = AccountUserManager.GetUserWithLogin(globalUniqueUserName, password);

                if (response.isSuccess)
                {
                    var user = (AccountUserIdentity)response.ResponseObject; //<-- ResponseObject can be converted to AccountUser by consuming application

                    //Add the Account object to the user:
                    //user.Account = account;

                    if (!account.Provisioned)
                    {
                        response.isSuccess = false;
                        response.ErrorMessage = "Account is not yet provisioned.";
                        response.ErrorMessages.Add("Account is not yet provisioned. Please try again after you get notice that provisioning is complete.");
                    }
                    if (!account.Active && account.Activated)
                    {
                        response.isSuccess = false;
                        response.ErrorMessage = "This account is no longer active.";
                        response.ErrorMessages.Add("This account is no longer active.");
                    }


                    //Validate that the user is active
                    if (!user.Active)
                    {
                        response.isSuccess = false;
                        response.ErrorMessage = "This user is not currently active.";
                        response.ErrorMessages.Add("This user is not currently active.");
                    }


                    return response;
                }
                else
                {
                    return response;
                }
            }
            catch(Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to authenticate: " + email + " on: " + accountName,
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    null,
                    accountName
                );



                var exceptionResponse = new DataAccessResponseType();

                exceptionResponse.isSuccess = false;
                exceptionResponse.ErrorMessage = e.Message;

                return exceptionResponse;
            }

        }
    }
}
