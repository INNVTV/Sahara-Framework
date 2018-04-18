using Sahara.Core.Accounts.Models;
//using Sahara.Core.Application.ApplicationImages.Models;
using Sahara.Core.Application.Images.Formats.Models;
using Sahara.Core.Application.Images.Records.Models;
using Sahara.Core.Application.Images.Records.TableEntities;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Imaging.Models;
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
    public interface IApplicationImageRecordsService
    {

        // Get ---

            /// <summary>
            /// Used by Clients to get reference to the ImageRecordGroup/ImageRecord Models
            /// (Clients should merge records with formats locally to avoid WCF call)
            /// </summary>
            /// <param name="accountNameKey"></param>
            /// <param name="imageFormatGroupTypeNameKey"></param>
            /// <param name="objectId"></param>
            /// <returns></returns>
        [OperationContract]
        List<ImageRecordGroupModel> GetImageRecordsForObject(string accountNameKey, string imageFormatGroupTypeNameKey, string objectId);


        //Only use to expose TableEntity object to clients
        [OperationContract]
        ImageRecordTableEntity GetEmptyImageRecordTableEntityReference();
    }
}
