using Sahara.Core.Platform.Requests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Infrastructure
{
    [ServiceContract]
    public interface IInfrastructureTestsService
    {
        /*==================================================================================
         * Performance Tests
         ==================================================================================*/

        [OperationContract]
        string PerformanceTest_InternalCaching_GetAccounts(bool useCache, string sharedClientKey);

        [OperationContract]
        string GetCurrentNodeName();

        [OperationContract]
        bool SendTestEmail(string email, string subject, string body);
    }




}
