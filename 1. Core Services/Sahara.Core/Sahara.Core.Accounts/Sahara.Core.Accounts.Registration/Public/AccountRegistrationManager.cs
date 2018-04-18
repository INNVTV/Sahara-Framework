using System;
using Sahara.Core.Accounts.Models;
using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Registration.Models;
using Sahara.Core.Accounts.Registration.Sql.Statements;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.SendGrid;
using Sahara.Core.Common.Validation;
using Sahara.Core.Common.Validation.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;

namespace Sahara.Core.Accounts.Registration
{
    public static class AccountRegistrationManager
    {
        public static DataAccessResponseType RegisterNewAccount(RegisterNewAccountModel model)
        {

            //trim the name of whitespaces (start & end):
            model.AccountName = model.AccountName.Trim();

            #region Refactoring notes

            /*

                With some refactoring you can start them directly with a chosen payment plan by passing a planid parameter to the Registration site (and ultimatly into this method) along with C.C. info
                
                This method will then check for a MonthlyRate > 0 and attempt to process the C.C.
                note: You would only add a Credit Card capture form to the Registration site if a plan with a MonthlyRate above 0 is selected -->
              
             

            	-- Adding a new "AllowRegistration" bool to the PaymentPlan object will allow for validation of selected plans coming in from users on this method for scenarios where users can choose a plan while signing up to avoid passing in ID's for plans such as "Unlimited" which must be approved by a Platform Admin

            */

            #endregion

            var response = new DataAccessResponseType {isSuccess = true};

            try
            {

                #region Validate Account Info
                //Validate Registration Data:

                #region Refactoring notes

                /*

                 
            	    -- Adding a new "AllowRegistration" bool to the PaymentPlan object will allow for validation of selected plans coming in from users on AccountRegistrationManager for scenarios where users can choose a plan while signing up to avoid passing in ID's for plans such as "Unlimited" which must be approved by a Platform Admin
                
                                > response.ErrorMessages.Add("Not a valid payment plan for public registration");
                
                 */

                #endregion


                #region Validate Password(s) Match
                if (model.Password != model.ConfirmPassword)
                {
                    response.isSuccess = false;

                    response.ErrorMessages.Add("Password and password confirmation do not match");
                }
                #endregion

                #region Validate Account Name:

                ValidationResponseType accountNameValidationResponse = ValidationManager.IsValidAccountName(model.AccountName);
                if (!accountNameValidationResponse.isValid)
                {
                    response.isSuccess = false;

                    response.ErrorMessages.Add(accountNameValidationResponse.validationMessage);

                    //return response;
                }

                #endregion

                #region Validate User Name

                ValidationResponseType firstNameValidationResponse = ValidationManager.IsValidFirstName(model.FirstName);
                if (!firstNameValidationResponse.isValid)
                {
                    response.isSuccess = false;

                    response.ErrorMessages.Add(firstNameValidationResponse.validationMessage);

                    //return response;
                }

                ValidationResponseType lastNameValidationResponse = ValidationManager.IsValidLastName(model.LastName);
                if (!lastNameValidationResponse.isValid)
                {
                    response.isSuccess = false;

                    response.ErrorMessages.Add(lastNameValidationResponse.validationMessage);

                    //return response;
                }

                #endregion


                #region Validate Email Unique (Optional)

                /*
                var userValidation = AccountUserManager.GetUserIdentity(model.Email);
                if (userValidation != null)
                {
                    response.isSuccess = false;
                    response.ErrorMessages.Add("Another account is associated with that email address, please provide another");
                }
                 */

                #endregion


                //If validation(s) fails, return the response:
                if (response.isSuccess == false)
                {
                    //Log Platform Activity
                    string errors = string.Empty;

                    foreach (string error in response.ErrorMessages)
                    {
                        errors += error + "|";
                    }

                    PlatformLogManager.LogActivity(CategoryType.Registration,
                    ActivityType.Registration_Failed,
                        String.Format("Registration failed for: '{0}' by: {1} from: {2}", model.AccountName, model.Email, model.Origin),
                        String.Format("Errors:{0}", errors
                        ));

                    //Return the response
                    response.ErrorMessage = "Could not register this account";
                    return response;
                }

                #endregion


                // Generate AccountID ====================================
                Guid accountId = Guid.NewGuid();

                #region Register Initial AccountUser (AKA: AccountOwner)


                #region Validate & Create Account Owner User

                // Further validations and account owner creation:

                var registerUserResponse = AccountUserManager.RegisterAccountOwner(
                        model.FirstName,
                        model.LastName,
                        accountId.ToString(),
                        model.AccountName,
                        model.Email,
                        model.Password
                    );

                #endregion

                if (!registerUserResponse.isSuccess)
                {


                        //Log Platform Activity
                    string errors = string.Empty;

                    foreach (string error in registerUserResponse.ErrorMessages)
                    {
                        errors += error + "|";
                    }

                    PlatformLogManager.LogActivity(CategoryType.Registration,
                    ActivityType.Registration_Failed,
                        String.Format("Registration failed for: '{0}' by: {1} from: {2}", model.AccountName, model.Email, model.Origin),
                        String.Format("Errors:{0}", errors
                        ));

                    //Return the response
                    response.isSuccess = false;
                    response.ErrorMessage = registerUserResponse.ErrorMessage;

                    response.ErrorMessages = registerUserResponse.ErrorMessages;

                    return response;
                }

                //Get user back from result
                var user = (AccountUserIdentity)registerUserResponse.ResponseObject; 

                #endregion

                #region Create Account

                try
                {

                    // Create Accounts =============================================================

                    InsertStatements insertStatements = new InsertStatements();
                    var insertResult = insertStatements.InsertNewAccount(model, accountId);

                    if (insertResult.isSuccess)
                    {
                        // (Optional) for certain scenrios
                        //Add user to account, make them the owner, and assign them as SuperAdmin role:
                        //AccountManager.AddUserToAccount(user.Id, AccountID.ToString(), true); // <-- Only for certain scenarios

                        response.isSuccess = true;
                        response.SuccessMessage = Sahara.Core.Settings.Copy.PlatformMessages.AccountRegistration.SuccessMessage;

                        var origin = "";
                        if(model.Origin != null)
                        {
                            origin = "<br/><br/><b>Origin:</b> " + model.Origin;
                        }

                        var name = "<br/><br/><b>Name:</b> " + model.FirstName + " " + model.LastName;
                        var email = "<br/><br/><b>Email:</b> " + model.Email;

                        var phone = "";
                        if (model.PhoneNumber != null)
                        {
                            phone = "<br/><br/><b>Phone:</b> " + model.PhoneNumber;
                        }

                        try
                        {
                            //Send an alert to the platform admin(s):
                            EmailManager.Send(
                                    Settings.Endpoints.Emails.PlatformEmailAddresses,
                                    Settings.Endpoints.Emails.FromRegistration,
                                    "Registration",
                                    "New Registrant",
                                    "A new account named <b>'" + model.AccountName + "'</b> has just been registered." + name + email + phone + origin,
                                    true
                                );
                        }
                        catch
                        {

                        }

                        //Log The Activity ------------ :
                        //PlatformLogManager.LogActivity(CategoryType.Registration,
                        //ActivityType.Registration_Succeeded,
                            //String.Format("Registration completed for: '{0}' by: {1} from: {2}", model.AccountName, model.Email, model.Origin),
                            //String.Format("Name:'{0}', Email:'{1}', Origin:{2}", model.AccountName, model.Email, model.Origin));

                        PlatformLogManager.LogActivity(CategoryType.Account,
                        ActivityType.Account_Registered,
                            String.Format("Registration completed for: '{0}' by: {1} from: {2}", model.AccountName, model.Email, model.Origin),
                            String.Format("Name:'{0}', Email:'{1}', Origin:{2}", model.AccountName, model.Email, model.Origin),
                            accountId.ToString(),
                            model.AccountName,
                            null,
                            null,
                            null,
                            null,
                            model.Origin);


                        return response;
                    }
                    else
                    {
                        #region Error Handling
                        string error = insertResult.ErrorMessage;

                        AccountUser outUser = null;

                        //rollback user creation:
                        AccountUserManager.DeleteUser(user.Id, false, out outUser);

                        //Log The Activity ------------ :
                        PlatformLogManager.LogActivity(CategoryType.Registration,
                        ActivityType.Registration_Failed,
                            String.Format("Registration failed for: '{0}' by: {1} from: {2}", model.AccountName, model.Email, model.Origin),
                            String.Format("Error:{0}", error));

                        //PlatformLogManager.LogActivity(ErrorLogActivity.PlatformError,
                            //String.Format("Registration failed for: '{0}' by: {1} from: {2}", model.AccountName, model.Email, model.Origin),
                            //String.Format("Error:{0}", error));


                        response.isSuccess = false;
                        response.ErrorMessage = error;

                        response.ErrorMessages.Add(error);

                        return response;
                        #endregion
                    }

                }
                catch (Exception e)
                {
                    #region Error Handling
                    string error = String.Empty;

                    AccountUser outUser = null;

                    //rollback user creation:
                    AccountUserManager.DeleteUser(user.Id, false, out outUser);

                    try
                    {
                        error = e.Message;
                    }
                    catch
                    {
                        error = "An error occured";
                    }

                    //rollback user:
                    //To Do: AccountUserManager.DeleteUser(model.Email);

                    string errorDetails = String.Format("Registration failed for: '{0}' by: {1} from: {2}", model.AccountName, model.Email, model.Origin);

                    //Log The Error(s) ------------ :
                    PlatformLogManager.LogActivity(CategoryType.Registration,
                    ActivityType.Registration_Error,
                        errorDetails,
                        String.Format("Error:{0}", error));



                    PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "registering a new account for " + model.AccountName + " / " + model.Email + " / " + model.FirstName + " " + model.LastName + " from: " + model.Origin,
                    System.Reflection.MethodBase.GetCurrentMethod());


                    response.isSuccess = false;
                    response.ErrorMessage = error;

                    response.ErrorMessages.Add(error);

                    return response;
                    #endregion
                }



                #endregion

            }
            catch(Exception e)
            {
                //Log The Error(s) ------------ :
                PlatformLogManager.LogActivity(CategoryType.Registration,
                    ActivityType.Registration_Error,
                    String.Format("Registration failed for: '{0}' by: {1} from: {2}", model.AccountName, model.Email, model.Origin),
                    String.Format("Error:{0}", e.Message));



                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "registering a new account for " + model.AccountName + " / " + model.Email + " / " + model.FirstName + " " + model.LastName + " from: " + model.Origin,
                    System.Reflection.MethodBase.GetCurrentMethod());



                response.isSuccess = false;
                response.ErrorMessage = "An error occured when creating the account";

                response.ErrorMessages.Add(e.Message);

                try
                {
                    response.ErrorMessages.Add(e.InnerException.InnerException.Message);
                }
                catch
                {

                }

                return response;
            }
        }
    }
}
