using Stripe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace API.WebhooksApi.Controllers
{
    [ServiceRequestActionFilter]
    public class EnvironmentController : ApiController
    {
        #region Fiddler Settings

        /*
         * [POST] http://[url]/stripe
         *
         * Content-type: application/json
         * Authorization: Basic 
         * 
         * 
         * REQUEST BODY:
         {"created":1326853478,"livemode":false,"id":"evt_00000000000000","type":"charge.succeeded","object":"event","request":null,"data":{"object":{"id":"ch_00000000000000","object":"charge","created":1400109038,"livemode":false,"paid":true,"amount":19999,"currency":"usd","refunded":false,"card":{"id":"card_00000000000000","object":"card","last4":"4242","type":"Visa","exp_month":12,"exp_year":2015,"fingerprint":"2haIcwo8R7ZzRd7Q","country":"US","name":"Kazimir Dugandzic","address_line1":null,"address_line2":null,"address_city":null,"address_state":null,"address_zip":null,"address_country":null,"cvc_check":null,"address_line1_check":null,"address_zip_check":null,"customer":"cus_00000000000000"},"captured":true,"refunds":[],"balance_transaction":"txn_00000000000000","failure_message":null,"failure_code":null,"amount_refunded":0,"customer":"cus_00000000000000","invoice":"in_00000000000000","description":null,"dispute":null,"metadata":{},"statement_description":null}}}
         * 
         * 
         */

        #endregion

        /// <summary>
        /// Returns a string id of the Service Fabric node that executed this calls
        /// </summary>
        /// <returns></returns>
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [Route("environment")]
        [HttpGet]
        public string GetEnvironment()
        {

            //Pull in enviornment variables: -----------------------------------------------------------
            //var hostId = Environment.GetEnvironmentVariable("Fabric_ApplicationHostId");
            //var hostType = Environment.GetEnvironmentVariable("Fabric_ApplicationHostType");
            //var hostType = Environment.GetEnvironmentVariable("Fabric_Endpoint_[YourServiceName]TypeEndpoint");
            //var hostType = Environment.GetEnvironmentVariable("Fabric_NodeName");

            var environment = Environment.GetEnvironmentVariable("Env");

            //Or print them all out:
            /*
            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
            {
                if (de.Key.ToString().StartsWith("Fabric"))
                {
                    string key = de.Key.ToString();
                    string value = de.Value.ToString)_;
                }
            }*/

            return environment;

        }

    }
}
