using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AccountAdminSite.Models.Users;
using System.ServiceModel;
using AccountAdminSite.AccountManagementService;

namespace AccountAdminSite.Controllers
{
    public class InvitationsController : Controller
    {
        [HttpGet]
        [Route("invitations/{invitationCode}")]
        public ActionResult Index(string invitationCode)
        {
            //Get the subdomain (if exists) for the site
            string accountNameKey = Common.GetSubDomain(Request.Url);

            if (String.IsNullOrEmpty(accountNameKey))
            {
                return Content("No account specified.");
            }


            if (invitationCode == null)
            {
                return Content("ERROR: No verification code present.");
            }
            else
            {
                try
                {
                    UserInvitation invitedUser = null;

                    var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();

                    try
                    {
                        accountManagementServiceClient.Open();
                        invitedUser = accountManagementServiceClient.GetAccountUserInvitation(accountNameKey, invitationCode, Common.SharedClientKey);

                        //Close the connection
                        WCFManager.CloseConnection(accountManagementServiceClient);
                    }
                    catch (Exception e)
                    {
                        #region Manage Exception

                        string exceptionMessage = e.Message.ToString();

                        var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                        string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                        // Abort the connection & manage the exception
                        WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                        return Content(WCFManager.UserFriendlyExceptionMessage);

                        #endregion
                    }



                    if (invitedUser == null)
                    {
                        return Content("Not a valid invitation key, or key has expired");
                    }
                    else
                    {
                        var creatUser = new CreateUserFromInvitationModel();

                        creatUser.AccountNameKey = accountNameKey;
                        creatUser.InvitationCode = invitedUser.InvitationKey;
                        creatUser.Email = invitedUser.Email;
                        creatUser.FirstName = invitedUser.FirstName;
                        creatUser.LastName = invitedUser.LastName;
                        creatUser.Role = invitedUser.Role;
                        creatUser.Owner = invitedUser.Owner;

                        return View(creatUser);
                    }
                    
                }
                catch (Exception e)
                {
                    return Content("ERROR: " + e.Message);
                }
            }
        }

        [HttpPost]
        [Route("invitations/{invitationCode}")]
        public ActionResult Index(CreateUserFromInvitationModel createUser)
        {
            if (ModelState.IsValid)
            {
                var response = new AccountManagementService.DataAccessResponseType();
                var accountManagementServiceClient = new AccountManagementService.AccountManagementServiceClient();
  
                try
                {
                    accountManagementServiceClient.Open();
                    response = accountManagementServiceClient.RegisterInvitedAccountUser(createUser.AccountNameKey, createUser.Email, createUser.FirstName, createUser.LastName, createUser.Password, createUser.Role, createUser.Owner, createUser.InvitationCode, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(accountManagementServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(accountManagementServiceClient, exceptionMessage, currentMethodString);

                    // Upate the response object
                    response.isSuccess = false;
                    response.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                    //response.ErrorMessages[0] = exceptionMessage;

                    #endregion
                }


                if (response.isSuccess)
                {

                    return RedirectToAction("Success", "Invitations");
                }
                else
                {
                    foreach (string error in response.ErrorMessages)
                    {
                        ModelState.AddModelError("Errors", error);
                    }

                    return View(createUser);
                }

            }
            else
            {
                return View(createUser);
            }
        }

        [HttpGet]
        [Route("invitations/success")]
        public ActionResult Success()
        {
            return View();
        }
    }
}