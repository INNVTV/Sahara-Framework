using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PlatformAdminSite.Models.Users;
using System.ServiceModel;

namespace PlatformAdminSite.Controllers
{
    public class InvitationsController : Controller
    {
        [HttpGet]
        [Route("invitations/{invitationCode}")]
        public ActionResult Index(string invitationCode)
        {

            if (invitationCode == null)
            {
                return Content("ERROR: No verification code present.");
            }
            else
            {
                var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();

                try
                {                   
                    platformManagementServiceClient.Open();
                    var invitedUser = platformManagementServiceClient.GetPlatformUserInvitation(invitationCode, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(platformManagementServiceClient);

                    if (invitedUser == null)
                    {
                        return Content("Not a valid invitation key, or key has expired");
                    }

                    var creatUser = new CreateUserFromInvitationModel();

                    creatUser.InvitationCode = invitedUser.InvitationKey;
                    creatUser.Email = invitedUser.Email;
                    creatUser.FirstName = invitedUser.FirstName;
                    creatUser.LastName = invitedUser.LastName;
                    creatUser.Role = invitedUser.Role;

                    return View(creatUser);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(platformManagementServiceClient, exceptionMessage, currentMethodString);

                    #endregion

                    return Content("ERROR: " + WCFManager.UserFriendlyExceptionMessage);
                }
            }
        }

        [HttpPost]
        [Route("invitations/{invitationCode}")]
        public ActionResult Index(CreateUserFromInvitationModel createUser)
        {
            if (ModelState.IsValid)
            {
                var response = new PlatformManagementService.DataAccessResponseType();
                var platformManagementServiceClient = new PlatformManagementService.PlatformManagementServiceClient();
                
                try
                {
                    platformManagementServiceClient.Open();
                    response = platformManagementServiceClient.RegisterInvitedPlatformUser(createUser.Email, createUser.FirstName, createUser.LastName, createUser.Password, createUser.Role, createUser.InvitationCode, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(platformManagementServiceClient);
                }
                catch (Exception e)
                {
                    #region Manage Exception

                    string exceptionMessage = e.Message.ToString();

                    var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                    string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                    // Abort the connection & manage the exception
                    WCFManager.CloseConnection(platformManagementServiceClient, exceptionMessage, currentMethodString);

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