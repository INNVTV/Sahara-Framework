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
    /// <summary>
    /// All GET requests use NameKeys for ease of public access. (But Redis is the best option for clients and should be tried first)
    /// All UPDATE/EDIT/DELETE requests use ID's as an added level of security
    /// </summary>
    [ServiceContract]
    public interface IApplicationTagsService
    {

        [OperationContract]
        DataAccessResponseType CreateTag(string accountId, string tagName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        List<String> GetTags(string accountNameKey, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType DeleteTag(string accountId, string tagName, string requesterId, RequesterType requesterType, string sharedClientKey);


    }
}
