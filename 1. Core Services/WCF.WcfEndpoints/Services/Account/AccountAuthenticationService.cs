using Microsoft.AspNet.Identity;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts.Security.PublicMethods;
using Sahara.Core.Logging.AccountLogs;
using Sahara.Core.Logging.AccountLogs.Types;
using WCF.WcfEndpoints.Contracts.Account;
using System.Security.Claims;


namespace WCF.WcfEndpoints.Service.Account
{
    public class AccountAuthenticationService : IAccountAuthenticationService
    {
        public AuthenticationResponse Authenticate(string accountName, string email, string password, string ipAddress, string origin, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var authResponse = new AuthenticationResponse();

            #region Refactoring Notes

            /*
             * In scenarios where users are only one to an account we make the account name the "UserName"
             * We can then look up the email address associated with the account (or vice versa depending on if it's an email or username login scenario)
             * This lookup data can be cached in Redis
             * 
             */

            #endregion

            var result = AccountSecurityManager.AuthenticateUser(accountName, email, password);

            authResponse.isSuccess = result.isSuccess;
            authResponse.ErrorMessage = result.ErrorMessage;

            if (result.isSuccess)
            {
                //Get the IdentityUser from the ResponseObject:
                var accountUserIdentity = (AccountUserIdentity)result.ResponseObject;


                //Convert to non Identity version & add to response object:
                authResponse.AccountUser = AccountUserManager.TransformAccountUserIdentityToAccountUser(accountUserIdentity);

                //Get Claims based identity for the user
                System.Security.Claims.ClaimsIdentity identity = AccountUserManager.GetUserClaimsIdentity(
                    accountUserIdentity,
                    DefaultAuthenticationTypes.ApplicationCookie); //<-- Uses a cookie for the local web application

                // You can add to claims thusly:
                //identity.AddClaim(new Claim(ClaimTypes.Name, "Name"));
                
                authResponse.ClaimsIdentity = identity;

                #region Log Account Activity (AuthenticationPassed)

                    try
                    {

                    var account = AccountManager.GetAccount(authResponse.AccountUser.AccountID.ToString());

                    AccountLogManager.LogActivity(
                            account.AccountID.ToString(),
                            account.StoragePartition,
                            CategoryType.Authentication,
                            ActivityType.Authentication_Passed,
                            "Successfull log in.",
                            authResponse.AccountUser.FirstName + " successfully logged in.",
                            authResponse.AccountUser.Id,
                            authResponse.AccountUser.FirstName,
                            authResponse.AccountUser.Email,
                            ipAddress,
                            origin);
                    }
                    catch { }

                #endregion
            }
            else
            {
                #region Log Account Activity (AuthenticationFailed)

                try
                {
                    //var accountId = AccountManager.GetAccountID(accountName);
                    var account = AccountManager.GetAccount(accountName);

                    AccountLogManager.LogActivity(
                        account.AccountID.ToString(),
                        account.StoragePartition,
                        CategoryType.Authentication,
                        ActivityType.Authentication_Failed,
                        "An attempt to log into account '" + accountName + "' with email '" + email + "' has failed.",
                        result.ErrorMessage,
                        "Unknown",
                        "Unknown",
                        email,
                        ipAddress,
                        origin);
                }
                catch { }

                #endregion
            }


            return authResponse;
        }

        /// <summary>
        /// Used to get updated claims information on the username.
        /// Can be used when an action is denyed by attempting to check if updated claim will allow the action.
        /// You may also choose a frequency to check for updated claims on the client side.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ClaimsIdentity GetClaimsIdentity(string userName, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var accountUserIdentity = AccountUserManager.GetUserIdentity(userName);

            System.Security.Claims.ClaimsIdentity identity = AccountUserManager.GetUserClaimsIdentity(
                accountUserIdentity,
                DefaultAuthenticationTypes.ApplicationCookie); //<-- Uses a cookie for the local web application
            
            return identity;
        }


        /// <summary>
        /// Optional: used to tell Core Services that a user has logged out of the system
        /// </summary>
        /// <param name="username"></param>
        public void Logout(string username, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
            }
        }
    }
}
