using Sahara.Core.Common.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Platform
{
    [ServiceContract]
    public interface IPlatformInitializationService
    {
        [OperationContract]
        bool IsPlatformInitialized();

        [OperationContract]
        DataAccessResponseType ProvisionPlatform(string FirstName, string LastName, string Email, string Password);

        [OperationContract]
        DataAccessResponseType PurgePlatform();
    }
}
