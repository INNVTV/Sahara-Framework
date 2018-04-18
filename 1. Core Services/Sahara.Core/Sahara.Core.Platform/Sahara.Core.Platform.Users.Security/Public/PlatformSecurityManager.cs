using System;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Users.Models;
using Sahara.Core.Platform.Users.Internal;
using Sahara.Core.Logging.PlatformLogs.Helpers;

namespace Sahara.Core.Platform.Users.Security
{
    public static class PlatformSecurityManager
    {
        public static DataAccessResponseType AuthenticateUser(string email, string password)
        {

            var response = new DataAccessResponseType();

            //Verifiy all prameters
            if (string.IsNullOrEmpty(email))
            {
                response.ErrorMessages.Add("Please include an email.");
            }
            if (string.IsNullOrEmpty(password))
            {
                response.ErrorMessages.Add("Please include a password.");
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                response.isSuccess = false;
                response.ErrorMessage = "Not all parameters contain a value!";
                return response;
            }


            try
            {
                //Get user with 'Login' info (username + password)
                response = PlatformUserManager.GetUserWithLogin(email, password);

                if (response.isSuccess)
                {
                    var user = (PlatformUserIdentity)response.ResponseObject; //<-- ResponseObject can be converted to PlatformUser by consuming application

                    //Validate that the user is active
                    if (!user.Active)
                    {
                        response.isSuccess = false;
                        response.ErrorMessages.Add("This user is not active.");
                    }


                    return response;
                }
                else
                {
                    return response;
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to authenticate platform user with email: " + email,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                return new DataAccessResponseType { isSuccess = false, ErrorMessage = e.Message };
            }

        }
    }
}
