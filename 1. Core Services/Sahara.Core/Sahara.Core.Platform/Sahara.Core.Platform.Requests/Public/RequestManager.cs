using Sahara.Core.Accounts;
using Sahara.Core.Platform.Requests.Models;
using Sahara.Core.Platform.Requests.ResponseTypes;
using Sahara.Core.Platform.Users;

namespace Sahara.Core.Platform.Requests
{
    public static class RequestManager
    {
        public static string SharedClientKey = "[Config_SharedClientKey]"; //<-- Used to verify calls for WCF service methods NOT tied to a user login.

        public static RequestResponseType ValidateRequest(string requesterId, RequesterType requsterType, out string requesterName, out string requesterEmail, string lowestRoleRequirementPlatform = null, string lowestRoleRequirementAccount = null, bool requiresAccountOwner = false, bool ignoreAccountActiveState = false)
        {
            var response = new RequestResponseType();

            requesterName = string.Empty;
            requesterEmail = string.Empty;

            #region validate request

            if (string.IsNullOrEmpty(requesterId))
            {
                return new RequestResponseType { isApproved = false, requestMessage = "A valid RequesterID must be used with this action" };
            }

            /*
            if (requsterType == null)
            {
                return new RequestResponseType { isApproved = false, requestMessage = "A RequesterType must be used with this action" };
            }*/

            #endregion

            

            switch(requsterType)
            {
                //Request is exempt from further validation
                case RequesterType.Exempt:
                    {
                        response.isApproved = true;
                        response.requestMessage = "This request is exempt from validation.";

                        #region get the requester info for the out object for logging purposes in WCF services

                        try
                        {
                            var requestUser = AccountUserManager.GetUser(requesterId);
                            
                            if (requestUser != null)
                            {
                                requesterName = requestUser.FirstName;
                                requesterEmail = requestUser.Email;
                            }
                            else
                            {
                                var plaformUser = PlatformUserManager.GetUser(requesterId);
                                requesterName = plaformUser.FirstName;
                                requesterEmail = plaformUser.Email;
                            }

                            
                        }
                        catch
                        {
                            var requestUser = PlatformUserManager.GetUser(requesterId);
                            requesterName = requestUser.FirstName;
                            requesterEmail = requestUser.Email;
                        }

                        #endregion

                        break;
                    }

                //validate request for a PlatformUser:
                case RequesterType.PlatformUser:
                    {
                        if (lowestRoleRequirementPlatform != null)
                        {
                            //userRole = PlatformUserManager.GetUserRole(requesterId);

                            var platformUser = PlatformUserManager.GetUser(requesterId);

                            requesterName = platformUser.FirstName;
                            requesterEmail = platformUser.Email;

                            //userRole = AccountUserManager.GetUserRole(requesterId);

                            //Check requester Active state:
                            if (!platformUser.Active)
                            {
                                response.isApproved = false;
                                response.requestMessage = "You must be an active platform user to make this request.";

                                //immediatley return the failed result
                                return response;
                            }


                            //Check requester role:
                            response.isApproved = Internal.RoleChecker.IsRoleAllowed(requsterType, platformUser.Role, lowestRoleRequirementPlatform);

                            if (response.isApproved)
                            {
                                response.requestMessage = "This request is valid.";
                            }
                            else
                            {
                                response.requestMessage = "This request is not valid for this platform user role.";
                            }
                        }
                        else
                        {
                            response.isApproved = false;
                            response.requestMessage = "This request is not valid for platform users";
                        }

                        break;
                    }


                //Validate request(s) for an AccountUser:
                case RequesterType.AccountUser:
                    {
                        var accountUser = AccountUserManager.GetUser(requesterId);

                        requesterName = accountUser.FirstName;
                        requesterEmail = accountUser.Email;

                        var account = AccountManager.GetAccount(accountUser.AccountID.ToString(), true, AccountManager.AccountIdentificationType.AccountID);

                        //Ensure that the account is Active (and Active state is not ignored):
                        if (!ignoreAccountActiveState && !account.Active)
                        {
                            response.isApproved = false;
                            response.requestMessage = "This account is not currently active.";
                            //Immediately return the failed result
                            return response;
                        }

                        if(!account.Provisioned)
                        {
                            response.isApproved = false;
                            response.requestMessage = "This account is not yet provisioned.";
                            //Immediately return the failed result
                            return response;
                        }

                        /*
                        //Ensure that the account is Active (and Active state is not ignored):
                        if (!ignoreAccountActiveState && !AccountManager.IsAccountActive(accountUser.AccountID.ToString()))
                        {
                            response.isApproved = false;
                            response.requestMessage = "This account is not currently active.";
                            //Immediately return the failed result
                            return response;
                        }

                        */

                        if (requiresAccountOwner)
                        {
                            //Check if the user is an account owner
                            if (accountUser.AccountOwner)
                            {
                                response.isApproved = true;
                                response.requestMessage = "This request is valid.";
                            }
                            else
                            {
                                response.isApproved = false;
                                response.requestMessage = "Only account owners can make this request or update.";

                                //Immediately return the failed result
                                return response;

                            }
                        }
                        else if (lowestRoleRequirementAccount != null)
                        {


                            //Check requester Active state:
                            if (!accountUser.Active)
                            {
                                response.isApproved = false;
                                response.requestMessage = "You must be an active account user to make this request.";

                                //Immediately return the failed result
                                return response;
                            }

                            //Check requester role:
                            response.isApproved = Internal.RoleChecker.IsRoleAllowed(requsterType, accountUser.Role, lowestRoleRequirementAccount);

                            if (response.isApproved)
                            {
                                response.requestMessage = "This request is valid.";
                            }
                            else
                            {
                                response.requestMessage = "This request is not valid for this account user role.";

                                //immediatly return the failed result
                                return response;
                            }
                        }
                        else
                        {
                            response.isApproved = false;
                            response.requestMessage = "This request is not valid for account users";

                            //immediatly return the failed result
                            return response;
                        }

                        break;
                    }

                   
                default:
                    {
                        response.isApproved = false;
                        response.requestMessage = "Cannot validate this request with the parameters given.";
                        break;
                    }
                    
                    
            }


            return response;

        }

    }
}
