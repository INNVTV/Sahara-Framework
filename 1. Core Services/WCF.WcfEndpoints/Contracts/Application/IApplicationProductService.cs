using Sahara.Core.Application.DocumentModels.Product;
using Sahara.Core.Application.Products.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Common.Types;
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
    /// <summary>
    /// All GET requests are left out of WCF to ensure that direct DocumentDB calls are used by clients (Read only keys are available in the PlatformServices Call). 
    /// All UPDATE/EDIT/DELETE requests use ID's as an added level of security
    /// </summary>
    [ServiceContract]
    public interface IApplicationProductService
    {       
        [OperationContract]
        DataAccessResponseType CreateProduct(string accountId, string locationPath, string productName, bool isVisible, string requesterId, RequesterType requesterType, string sharedClientKey);

        //Not used, but needed to expose 'ProductDocumentModel' type to clients
        [OperationContract]
        ProductDocumentModel GetProduct(string productId, string sharedClientKey);

        #region Depricated
        /*
        [OperationContract]
        int GetProductCount(string accountId);

        --- Use direct DocumentDB calls for this ---
        [OperationContract]
        ProductResults GetProducts(string accountNameKey);

        [OperationContract]
        ProductDocumentModel GetProduct(string productId);
        */
        #endregion

        //--UPDATES--

        [OperationContract]
        DataAccessResponseType UpdateProductVisibleState(string accountId, string fullyQualifiedName, string productName, bool visibleState, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RenameProduct(string accountId, string fullyQualifiedName, string oldName, string newName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ReorderProducts(string accountId, Dictionary<string, int> productOrderingDictionary, string locationPath, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ResetProductOrdering(string accountId, string locationPath, string requesterId, RequesterType requesterType, string sharedClientKey);
       
        //--Properties & Tags ---

        [OperationContract]
        DataAccessResponseType UpdateProductProperty(string accountId, string fullyQualifiedName, string productName, string propertyNameKey, string propertyValue, ProductPropertyUpdateType updateType, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateProductLocationProperty(string accountId, string fullyQualifiedName, string productName, string propertyNameKey, PropertyLocationValue propertyLocationValue, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RemoveProductPropertyCollectionItem(string accountId, string fullyQualifiedName, string productName, string propertyNameKey, int collectionItemIndex, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ClearProductProperty(string accountId, string fullyQualifiedName, string productName, string propertyNameKey, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType AddProductTag(string accountId, string fullyQualifiedName, string productName, string tagName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RemoveProductTag(string accountId, string fullyQualifiedName, string productName, string tagName, string requesterId, RequesterType requesterType, string sharedClientKey);

        // -- Images --

        //[OperationContract] MOVED
        //DataAccessResponseType AddProductImage(string accountId, string productId, ImageFormatInstructions imageFormatInstructions,  ImageSourceFile imageSourceFile, ImageCropCoordinates imageCropCoordinates, ImageEnhancementInstructions imageEnhancementInstructions);

        //--MOVE--
        [OperationContract]
        DataAccessResponseType MoveProduct(string accountId, string productId, string newLocationPath, string requesterId, RequesterType requesterType, string sharedClientKey);

        //--DELETE--
        [OperationContract]
        DataAccessResponseType DeleteProduct(string accountId, string productId, string requesterId, RequesterType requesterType, string sharedClientKey);
    }
}
