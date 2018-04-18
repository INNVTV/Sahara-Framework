using Sahara.Core.Application.ApiKeys.Models;
using Sahara.Core.Application.Categorization.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Application
{
    [ServiceContract]
    public interface IApplicationApiKeysService
    {
        [OperationContract]
        List<ApiKeyModel> GetApiKeys(string accountNameKey, string sharedClientKey);

        [OperationContract]
        string GenerateApiKey(string accountNameKey, string name, string description, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        string RegenenerateApiKey(string accountNameKey, string apiKey, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType DeleteApiKey(string accountNameKey, string apiKey, string requesterId, RequesterType requesterType, string sharedClientKey);
    }
}
