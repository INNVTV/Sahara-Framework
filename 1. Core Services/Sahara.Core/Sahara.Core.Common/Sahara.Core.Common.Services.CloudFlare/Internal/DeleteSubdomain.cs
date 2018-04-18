using Newtonsoft.Json;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Services.CloudFlare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Services.CloudFlare
{
    internal static class Delete
    {
        internal static DataAccessResponseType DeleteSubdomain(string accountNameKey, string zoneId, string recordId)
        {
            var response = new DataAccessResponseType();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-Auth-Email", Settings.Services.CloudFlare.Account.email);
                httpClient.DefaultRequestHeaders.Add("X-Auth-Key", Settings.Services.CloudFlare.Account.apiKey);

                // Do the actual request and await the response
                var httpResponse = httpClient.DeleteAsync(Settings.Services.CloudFlare.Account.apiDomain + "/zones/" + zoneId + "/dns_records/" + recordId).Result;

                // If the response contains content we want to read it!
                if (httpResponse.Content != null)
                {
                    //var responseContent = httpResponse.Content.ReadAsStringAsync();

                    string responseContent = httpResponse.Content.ReadAsStringAsync().Result;

                    DnsDeleteResponse deleteResponse = JsonConvert.DeserializeObject<DnsDeleteResponse>(responseContent);

                    response.isSuccess = deleteResponse.success;
                }
            }

            return response;
        }
    }
}
