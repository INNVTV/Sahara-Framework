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
    public class RegistrationController : ApiController
    {
        #region Fiddler Settings

        /*
         * [POST] url/register
         *
         * Content-type: application/x-www-form-urlencoded
         * Authorization: Basic 
         * 
         * 
         * REQUEST BODY:
         * AccountName=Account Name&Email=email@email.com&etc=etc...
         * 
         */

        #endregion

        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("register")]
        [HttpPost]
        public HttpResponseMessage Post(AccountRegistrationService.RegisterNewAccountModel registerNewAccountModel)
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            if (ModelState.IsValid)
            {
                //AccountRegistrationService.AccountRegistrationServiceClient accountRegistrationClient = new AccountRegistrationService.AccountRegistrationServiceClient();
                //var registrationServiceResponse = accountRegistrationClient.RegisterAccount(registerNewAccountModel);
                //accountRegistrationClient.Close();

                var accountRegistrationServiceClient = new AccountRegistrationService.AccountRegistrationServiceClient();

                try
                {
                    accountRegistrationServiceClient.Open();
                    var registrationServiceResponse = accountRegistrationServiceClient.RegisterAccount(registerNewAccountModel, Common.SharedClientKey);

                    //Close the connection
                    WCFManager.CloseConnection(accountRegistrationServiceClient);

                    if (registrationServiceResponse.isSuccess)
                    {
                        httpResponse = Request.CreateResponse(HttpStatusCode.Created, registrationServiceResponse);
                    }
                    else
                    {
                        httpResponse = Request.CreateResponse(HttpStatusCode.OK, registrationServiceResponse);
                    }

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
                    var exceptionResponse = new Sahara.Api.Accounts.Registration.AccountRegistrationService.DataAccessResponseType();
                    exceptionResponse.isSuccess = false;
                    exceptionResponse.ErrorMessage = WCFManager.UserFriendlyExceptionMessage;
                    exceptionResponse.ErrorMessages[0] = exceptionMessage;

                    #endregion

                    httpResponse = Request.CreateResponse(HttpStatusCode.OK, exceptionResponse);
                }


            }
            else
            {
                httpResponse = Request.CreateErrorResponse(HttpStatusCode.OK, ModelState);
            }

            return httpResponse;

        }
    }
}
