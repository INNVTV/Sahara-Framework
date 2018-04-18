using Sahara.Api.Accounts.Registration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;


namespace Sahara.Api.Accounts.Registration.Controllers
{
    [RoutePrefix("validation")]
    public class ValidationController : ApiController
    {
        #region Fiddler Settings

        /*
         * [POST] url/accountname
         *
         * Content-type: application/x-www-form-urlencoded
         * Authorization: Basic 
         * 
         * 
         * REQUEST BODY:
         * AccountName=Account Name
         * 
         */

        #endregion

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("accountname")]
        [HttpPost]
        public AccountNameValidationResponse AccountNameValidation(AccountNameValidator accountNameValidator)
        {
            string accountName = accountNameValidator.AccountName;

            var response = new AccountNameValidationResponse();

            var accountRegistrationServiceClient = new AccountRegistrationService.AccountRegistrationServiceClient();

            try
            {
                accountRegistrationServiceClient.Open();

                var validationResponse = accountRegistrationServiceClient.ValidateAccountName(accountName);


                if (validationResponse.isValid)
                {
                    response.valid = true;
                    response.message = accountName + " is available!";
                }
                else
                {
                    response.valid = false;
                    response.message = validationResponse.validationMessage;
                }


                //CommonMethods.MethodsClient commonMethodsClient = new CommonMethods.MethodsClient();
                response.subdomain = accountRegistrationServiceClient.ConvertToAccountNameKey(accountName);

                //Close the connection
                WCFManager.CloseConnection(accountRegistrationServiceClient);

            }
            catch(Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountRegistrationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.valid = false;
                response.message = WCFManager.UserFriendlyExceptionMessage;

                #endregion
                
            }


            return response;
        }


        #region Fiddler Settings

        /*
         * [POST] url/email
         *
         * Content-type: application/x-www-form-urlencoded
         * Authorization: Basic 
         * 
         * 
         * REQUEST BODY:
         * Email=email@email.com
         * 
         */

        #endregion

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("email")]
        [HttpPost]
        public ValidationResponse PostEmailValidation(EmailValidator emailValidator)
        {
            var response = new ValidationResponse();

            var accountRegistrationServiceClient = new AccountRegistrationService.AccountRegistrationServiceClient();

            try
            {
                accountRegistrationServiceClient.Open();
                var validationResponse = accountRegistrationServiceClient.ValidateEmail(emailValidator.Email);

                //Close the connection
                WCFManager.CloseConnection(accountRegistrationServiceClient);


                response.valid = validationResponse.isValid;
                response.message = validationResponse.validationMessage;
            }
            catch(Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountRegistrationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.valid = false;
                response.message = WCFManager.UserFriendlyExceptionMessage;

                #endregion

            }

            return response;
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("phonenumber")]
        [HttpPost]
        public ValidationResponse PostPhoneValidation(PhoneValidator phoneValidator)
        {
            var response = new ValidationResponse();

            var accountRegistrationServiceClient = new AccountRegistrationService.AccountRegistrationServiceClient();

            try
            {
                accountRegistrationServiceClient.Open();
                var validationResponse = accountRegistrationServiceClient.ValidatePhoneNumber(phoneValidator.PhoneNumber);

                //Close the connection
                WCFManager.CloseConnection(accountRegistrationServiceClient);


                response.valid = validationResponse.isValid;
                response.message = validationResponse.validationMessage;
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountRegistrationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.valid = false;
                response.message = WCFManager.UserFriendlyExceptionMessage;

                #endregion

            }

            return response;
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("firstname")]
        [HttpPost]
        public ValidationResponse PostFirstNameValidation(FirstNameValidator nameValidator)
        {
            var response = new ValidationResponse();

            var accountRegistrationServiceClient = new AccountRegistrationService.AccountRegistrationServiceClient();

            try
            {
                accountRegistrationServiceClient.Open();
                var validationResponse = accountRegistrationServiceClient.ValidateFirstName(nameValidator.FirstName);


                //Close the connection
                WCFManager.CloseConnection(accountRegistrationServiceClient);

                response.valid = validationResponse.isValid;
                response.message = validationResponse.validationMessage;

            }
            catch(Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountRegistrationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.valid = false;
                response.message = WCFManager.UserFriendlyExceptionMessage;

                #endregion

            }

            return response;
        }

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("lastname")]
        [HttpPost]
        public ValidationResponse PostLastNameValidation(LastNameValidator nameValidator)
        {
            var response = new ValidationResponse();

            var accountRegistrationServiceClient = new AccountRegistrationService.AccountRegistrationServiceClient();

            try
            {
                accountRegistrationServiceClient.Open();
                var validationResponse = accountRegistrationServiceClient.ValidateLastName(nameValidator.LastName);

                //Close the connection
                WCFManager.CloseConnection(accountRegistrationServiceClient);


                response.valid = validationResponse.isValid;
                response.message = validationResponse.validationMessage;
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connection & manage the exception
                WCFManager.CloseConnection(accountRegistrationServiceClient, exceptionMessage, currentMethodString);

                // Upate the response object
                response.valid = false;
                response.message = WCFManager.UserFriendlyExceptionMessage;

                #endregion
            }

            return response;
        }
    }
}
