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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("passwords")]
    public class PasswordsController : ApiController
    {
        #region Fiddler Settings

        /*
         * [POST] url/passwords/lost
         *
         * Content-type: application/x-www-form-urlencoded
         * Authorization: Basic 
         * 
         * 
         * REQUEST BODY:
         * AccountID=xxxxxxx&Email=email@email.com
         * 
         */

        #endregion

        // '/passwords/lost' MOVE TO ACCOUNT MANAGEMENT API

        /*
        [EnableCors(origins:"*", headers:"*", methods:"*")]
        [Route("lost")]
        [HttpPost]
        public HttpResponseMessage PostEmailClaim(LostPassword lostPassword)
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            //AccountRegistrationService.AccountRegistrationServiceClient registrationServiceClient = new AccountRegistrationService.AccountRegistrationServiceClient();

            var accountRegistrationServiceClient = new AccountRegistrationService.AccountRegistrationServiceClient();
            accountRegistrationServiceClient.Open();
            var passwordClaimResponse = accountRegistrationServiceClient.ClaimLostPassword(lostPassword.AccountID, lostPassword.Email);
            accountRegistrationServiceClient.Close();

            //var passwordClaimResponse = registrationServiceClient.ClaimLostPassword(lostPassword.AccountID, lostPassword.Email);
            //registrationServiceClient.Close();

            if (passwordClaimResponse.isSuccess)
            {
                httpResponse = Request.CreateResponse(HttpStatusCode.Created, passwordClaimResponse);
            }
            else
            {
                httpResponse = Request.CreateResponse(HttpStatusCode.OK, passwordClaimResponse);
            }

            return httpResponse;
        }*/
    }
}
