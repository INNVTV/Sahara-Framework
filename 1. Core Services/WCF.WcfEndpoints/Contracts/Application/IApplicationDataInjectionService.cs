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
    public interface IApplicationDataInjectionService
    {
        #region Image Documents

        [OperationContract]
        DataAccessResponseType InjectImageDocumentsIntoAccount(string accountId, int imageDocumentInjectionCount, string requesterId, RequesterType requesterType, string sharedClientKey);

        #endregion

    }
}
