using Sahara.Core.Common.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Common.Services.CloudFlare.Public
{
    public static class CloudFlareManager
    {
        public static DataAccessResponseType RegisterSubdomains(string accountNameKey)
        {
            var response = new DataAccessResponseType();

            var accountAdminCreate = Create.AccountSubdomain(accountNameKey, Settings.Services.CloudFlare.Domain_AccountAdmin.Content, Settings.Services.CloudFlare.Domain_AccountAdmin.ZoneId);
            var accountApiCreate = Create.AccountSubdomain(accountNameKey, Settings.Services.CloudFlare.Domain_AccountApi.Content, Settings.Services.CloudFlare.Domain_AccountApi.ZoneId);
            var accountWebsiteCreate = Create.AccountSubdomain(accountNameKey, Settings.Services.CloudFlare.Domain_AccountWebsite.Content, Settings.Services.CloudFlare.Domain_AccountWebsite.ZoneId);


            if(accountAdminCreate.isSuccess && accountApiCreate.isSuccess && accountWebsiteCreate.isSuccess)
            {
                response.isSuccess = true;
            }
            else
            {
                response.isSuccess = false;
                response.ErrorMessage = "One or more subdomain entries for '" + accountNameKey + "' failed during creation. Please update CloudFlare manually.";
            }

            return response;
        }

        public static DataAccessResponseType RemoveSubdomains(string accountNameKey)
        {
            var response = new DataAccessResponseType();

            //Get id's for all 3 subdomain records
            var accountAdminRecord = Get.GetSubdomainDnsRecord(accountNameKey, Settings.Services.CloudFlare.Domain_AccountAdmin.Domain, Settings.Services.CloudFlare.Domain_AccountAdmin.ZoneId);
            var accountApiRecord = Get.GetSubdomainDnsRecord(accountNameKey, Settings.Services.CloudFlare.Domain_AccountApi.Domain, Settings.Services.CloudFlare.Domain_AccountApi.ZoneId);
            var accountWebsiteRecord = Get.GetSubdomainDnsRecord(accountNameKey, Settings.Services.CloudFlare.Domain_AccountWebsite.Domain, Settings.Services.CloudFlare.Domain_AccountWebsite.ZoneId);

            if (accountAdminRecord == null && accountApiRecord == null && accountWebsiteRecord == null)
            {
                response.isSuccess = false;
                response.ErrorMessage = "One or more subdomain entries for '" + accountNameKey + "' failed during deletion. Could not find DNS Records. Please update CloudFlare manually.";
            }


            //Remove records using record id's
            var accountAdminDelete = Delete.DeleteSubdomain(accountNameKey, Settings.Services.CloudFlare.Domain_AccountAdmin.ZoneId, accountAdminRecord.id);
            var accountApiDelete = Delete.DeleteSubdomain(accountNameKey, Settings.Services.CloudFlare.Domain_AccountApi.ZoneId, accountApiRecord.id);
            var accountWebsiteDelete = Delete.DeleteSubdomain(accountNameKey, Settings.Services.CloudFlare.Domain_AccountWebsite.ZoneId, accountWebsiteRecord.id);

            if (accountAdminDelete.isSuccess && accountApiDelete.isSuccess && accountWebsiteDelete.isSuccess)
            {
                response.isSuccess = true;
            }
            else
            {
                response.isSuccess = false;
                response.ErrorMessage = "One or more subdomain entries for '" + accountNameKey + "' failed during deletion. Please update CloudFlare manually.";
            }

            return response;
        }
    }
}
