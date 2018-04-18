using Sahara.Core.Accounts.Models;
using Sahara.Core.Application.Images.Formats.Models;
//using Sahara.Core.Application.ApplicationImages.Models;
//using Sahara.Core.Application.Images.Formats.Models;
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
    public interface IApplicationImagesService
    {

        [OperationContract]
        DataAccessResponseType ProcessImage(string accountId, ImageProcessingManifestModel imageManifest, ImageCropCoordinates imageCropCoordinates, string requesterId, RequesterType requesterType, ImageEnhancementInstructions imageEnhancementInstructions, string sharedClientKey);

        #region Management

        #region Update 

        [OperationContract]
        DataAccessResponseType UpdateImageRecordTitle(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, string newTitle, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateImageRecordDescription(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateImageRecordGalleryTitle(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex, string newTitle, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateImageRecordGalleryDescription(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ReorderImageRecordGallery(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, List<int> imageIndexOrder, string requesterId, RequesterType requesterType, string sharedClientKey);


        #endregion

        #region Delete

        [OperationContract]
        DataAccessResponseType DeleteImageRecord(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType DeleteGalleryImage(string accountId, string objectType, string objectId, string groupNameKey, string formatNameKey, int imageIndex, string requesterId, RequesterType requesterType, string sharedClientKey);

        #endregion

        #endregion

    }
}
