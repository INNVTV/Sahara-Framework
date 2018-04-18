using Sahara.Core.Application.DocumentModels.Product;
using Sahara.Core.Application.Leads.Models;
using Sahara.Core.Application.Products.Models;
using Sahara.Core.Application.Search.Models.Product;
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
    public interface IApplicationLeadsService
    {
        // ------------ SETTINGS (Labels) -----------

        [OperationContract]
        DataAccessResponseType CreateLabel(string accountNameKey, string labelName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RemoveLabel(string accountNameKey, string labelName, string requesterId, RequesterType requesterType, string sharedClientKey);

        // ------------ GET (Sales Leads)

        //[OperationContract]
        //List<SalesLead> GetLeads(string accountId, string label, int skip, int take);
    }
}
