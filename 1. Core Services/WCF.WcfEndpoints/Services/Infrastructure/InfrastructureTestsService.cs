using Microsoft.AspNet.Identity;
using Sahara.Core.Platform.Requests.Models;
using Sahara.Core.Platform.Requests;
using WCF.WcfEndpoints.Contracts.Infrastructure;
using WCF.WcfEndpoints.Contracts.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts;

namespace WCF.WcfEndpoints.Service.Infrastructure
{
    public class InfrastructureTestsService : IInfrastructureTestsService
    {

        public string PerformanceTest_InternalCaching_GetAccounts(bool useCache, string sharedClientKey)
        {
            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return AccountManager.GetAllAccounts(1, 20, "AccountName", useCache).ToString();
        }

        public string GetCurrentNodeName()
        {
            var nodeName = Environment.GetEnvironmentVariable("Fabric_NodeName");

            return nodeName;
        }


        public bool SendTestEmail(string email, string subject, string body)
        {
            Sahara.Core.Common.Services.SendGrid.EmailManager.Send(email, "[Config_TestEmail]", "Test", subject, body, true);

            return true;
        }

    }
}
