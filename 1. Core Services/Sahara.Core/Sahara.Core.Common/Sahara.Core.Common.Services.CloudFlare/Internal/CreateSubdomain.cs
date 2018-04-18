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
    internal static class Create
    {
        internal static DataAccessResponseType AccountSubdomain(string accountNameKey, string content, string zoneId)
        {
            var response = new DataAccessResponseType();

            string type = "CNAME";

            string postContent = "{\"type\":\"" + type + "\",\"name\":\"" + accountNameKey + "\",\"content\":\"" + content + "\",\"ttl\":120,\"proxied\":true}";

            var httpContent = new StringContent(postContent, Encoding.UTF8, "application/json");

            httpContent.Headers.Add("X-Auth-Email", Settings.Services.CloudFlare.Account.email);
            httpContent.Headers.Add("X-Auth-Key", Settings.Services.CloudFlare.Account.apiKey);

            using (var httpClient = new HttpClient())
            {
                // Do the actual request and await the response
                var httpResponse = httpClient.PostAsync(Settings.Services.CloudFlare.Account.apiDomain + "/zones/" + zoneId + "/dns_records", httpContent).Result;

                // If the response contains content we want to read it!
                if (httpResponse.Content != null)
                {
                    //var responseContent = httpResponse.Content.ReadAsStringAsync();

                    string responseContent = httpResponse.Content.ReadAsStringAsync().Result;

                    DnsCreateResponse createResponse = JsonConvert.DeserializeObject<DnsCreateResponse>(responseContent);

                    response.isSuccess = createResponse.success;
                }
            }

            return response;
        }
    }
}
