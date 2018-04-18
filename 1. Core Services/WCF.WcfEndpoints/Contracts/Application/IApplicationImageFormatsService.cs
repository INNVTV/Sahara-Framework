using Sahara.Core.Accounts.Models;
//using Sahara.Core.Application.ApplicationImages.Models;
using Sahara.Core.Application.Images.Formats.Models;
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
    public interface IApplicationImageFormatsService
    {
        #region Image Group Types (Global)

        [OperationContract]
        List<ImageFormatGroupTypeModel> GetImageFormatGroupTypes();

        #endregion

        #region Image Groups & Formats (Account Specific)

        // Get ---

        [OperationContract]
        List<ImageFormatGroupModel> GetImageFormats(string accountNameKey, string imageFormatGroupTypeNameKey);

        // Create --

        [OperationContract]
        DataAccessResponseType CreateImageGroup(string accountNameKey, string imageGroupTypeNameKey, string imageGroupName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType CreateImageFormat(string accountNameKey, string imageGroupTypeNameKey, string imageGroupNameKey, string imageFormatName, int width, int height, bool listing, bool gallery, string requesterId, RequesterType requesterType, string sharedClientKey);


        // Delete --

        [OperationContract]
        DataAccessResponseType DeleteImageGroup(string accountNameKey, string imageGroupTypeNameKey, string imageGroupNameKey, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType DeleteImageFormat(string accountNameKey, string imageGroupTypeNameKey, string imageGroupNameKey, string imageFormatNameKey, string requesterId, RequesterType requesterType, string sharedClientKey);

        #endregion


    }
}
