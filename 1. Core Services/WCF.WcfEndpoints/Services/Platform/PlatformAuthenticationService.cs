using Microsoft.AspNet.Identity;
using Sahara.Core.Platform.Users.Models;
using Sahara.Core.Platform.Users;
using Sahara.Core.Platform.Users.Security;
using WCF.WcfEndpoints.Contracts.Platform;
using System.Security.Claims;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;


namespace WCF.WcfEndpoints.Service.Platform
{
    public class PlatformAuthenticationService : IPlatformAuthenticationService
    {
        public AuthenticationResponse Authenticate(string email, string password, string ipAddress, string origin, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            var authResponse = new AuthenticationResponse();

            //From here you may log the fact that the user has authenticated at this time (if this is something you wish to track)
            //login.IP; <-- IP address for logging purposes

            var result = PlatformSecurityManager.AuthenticateUser(email, password);

            authResponse.isSuccess = result.isSuccess;
            authResponse.ErrorMessage = result.ErrorMessage;

            if (result.isSuccess)
            {
                //Get the IdentityUser from the ResponseObject:
                var platformUserIdentity = (PlatformUserIdentity)result.ResponseObject;


                //Convert to non Identity version & add to response object:
                authResponse.PlatformUser = PlatformUserManager.TransformPlatformUserIdentityToPlatformUser(platformUserIdentity);

                //Get Claims based identity for the user
                System.Security.Claims.ClaimsIdentity identity = PlatformUserManager.GetUserClaimsIdentity(
                    platformUserIdentity,
                    DefaultAuthenticationTypes.ApplicationCookie); //<-- Uses a cookie for the local web application

                // You can add to claims thusly:
                //identity.AddClaim(new Claim(ClaimTypes.Name, "Name"));
                
                authResponse.ClaimsIdentity = identity;

                #region Log Platform user Activity (AuthenticationPassed)

                try
                {
                    PlatformLogManager.LogActivity(
                        CategoryType.Authentication,
                        ActivityType.Authentication_Passed,
                        "Successfull log in.",
                        authResponse.PlatformUser.FirstName + " successfully logged in.",
                        null,
                        null,
                        authResponse.PlatformUser.Id,
                        authResponse.PlatformUser.FirstName,
                        authResponse.PlatformUser.Email,
                        ipAddress,
                        origin);
                }
                catch { }

                #endregion
            }
            else
            {
                #region Log Platform User Activity (AuthenticationFailed)

                try
                {

                    PlatformLogManager.LogActivity(
                        CategoryType.Authentication,
                        ActivityType.Authentication_Failed,
                        "An attempt to log into the platform admin with email '" + email + "' has failed.",
                        result.ErrorMessage,
                        null,
                        null,
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

            var platformUserIdentity = PlatformUserManager.GetUserIdentity(userName);

            System.Security.Claims.ClaimsIdentity identity = PlatformUserManager.GetUserClaimsIdentity(
                platformUserIdentity,
                DefaultAuthenticationTypes.ApplicationCookie); //<-- Uses a cookie for the local web application
            
            return identity;
        }


        /// <summary>
        /// Optional: used to tell Core Services that a user has logged out of the system
        /// </summary>
        /// <param name="username"></param>
        public void Logout(string username, string sharedClientKey)
        {
        }
    }
}
