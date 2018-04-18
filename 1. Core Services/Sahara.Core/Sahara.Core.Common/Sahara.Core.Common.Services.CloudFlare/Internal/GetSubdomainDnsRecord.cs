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
    internal static class Get
    {
        internal static DnsRecord GetSubdomainDnsRecord(string accountNameKey, string domain, string zoneId)
        {
            //var httpContent = new StringContent("");

            //httpContent.Headers.Add("X-Auth-Email", email);
            //httpContent.Headers.Add("X-Auth-Key", apiKey);

            //string cnameRecordId = null;

            DnsRecord dnsRecord = null;

            using (var httpClient = new HttpClient())
            {

                httpClient.DefaultRequestHeaders.Add("X-Auth-Email", Settings.Services.CloudFlare.Account.email);
                httpClient.DefaultRequestHeaders.Add("X-Auth-Key", Settings.Services.CloudFlare.Account.apiKey);

                // Do the actual request and await the response
                var httpResponse = httpClient.GetAsync(Settings.Services.CloudFlare.Account.apiDomain + "/zones/" + zoneId + "/dns_records?type=CNAME&name=" + accountNameKey + "." + domain).Result;

                // If the response contains content we want to read it!
                if (httpResponse.Content != null)
                {
                    string responseContent = httpResponse.Content.ReadAsStringAsync().Result;

                    DnsListingResponse listingResponse = JsonConvert.DeserializeObject<DnsListingResponse>(responseContent);
                    dnsRecord = listingResponse.result[0];

                    //cnameRecordId = dnsRecord.id;
                }
            }

            return dnsRecord;
        }
    }
}
