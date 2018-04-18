//using Sahara.Core.Application.Properties.Models;
using Sahara.Core.Application.Properties.Models;
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
    public interface IApplicationPropertiesService
    {       
        [OperationContract]
        DataAccessResponseType CreateProperty(string accountId, string propertyTypeNameKey, string propertyName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        int GetPropertyCount(string accountId, string sharedClientKey);

        [OperationContract]
        List<PropertyTypeModel> GetPropertyTypes(string sharedClientKey);

        [OperationContract]
        List<PropertyModel> GetProperties(string accountNameKey, PropertyListType listType, string sharedClientKey);

        [OperationContract]
        PropertyModel GetProperty(string accountNameKey, string propertyNameKey, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType CreatePropertyValue(string accountId, string propertyNameKey, string propertyValueName, string requesterId, RequesterType requesterType, string sharedClientKey);

        #region Swatch Property Type  

        // Future performance update: have client upload image to intermediary storage, submit location with imag eid for WCF processing (similar to other imageing solutions)
        //Used to upload swatch image and display it prior to creating a label and submitting
        //Step 1:
        [OperationContract]
        string UploadPropertySwatchImage(string accountId, byte[] imageByteArray, string requesterID, RequesterType requesterType, string sharedClientKey);

        //Step 2:
        [OperationContract]
        DataAccessResponseType CreateSwatchValue(string accountId, string propertyNameKey, string swatchImage, string swatchLabel, string requesterID, RequesterType requesterType, string sharedClientKey);

        #endregion


        //-- UPDATE --

        [OperationContract]
        DataAccessResponseType UpdatePropertyListingState(string accountId, string propertyNameKey, bool listingState, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdatePropertyDetailsState(string accountId, string propertyNameKey, bool detailsState, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdatePropertyFacetInterval(string accountId, string propertyNameKey, int newFacetInterval, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdatePropertyFacetableState(string accountId, string propertyNameKey, bool isFacetable, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdatePropertySortableState(string accountId, string propertyNameKey, bool isSortable, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdatePropertyAppendableState(string accountId, string propertyNameKey, bool isAppendable, string requesterId, RequesterType requesterType, string sharedClientKey);

        //[OperationContract]
        //DataAccessResponseType UpdatePropertyHighlightedState(string accountId, string propertyNameKey, bool isHighlighted, string requesterId, RequesterType requesterType);

        [OperationContract]
        DataAccessResponseType UpdatePropertySymbol(string accountId, string propertyNameKey, string symbol, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdatePropertySymbolPlacement(string accountId, string propertyNameKey, string symbolPlacement, string requesterId, RequesterType requesterType, string sharedClientKey);

        //-- FEATURED --

        [OperationContract]
        DataAccessResponseType UpdateFeaturedProperties(string accountId, Dictionary<string, int> featuredOrderingDictionary, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ResetFeaturedProperties(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey);





        //-- DELETE --

        [OperationContract]
        DataAccessResponseType DeleteProperty(string accountId, string propertyId, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType DeletePropertyValue(string accountId, string propertyNameKey, string propertyValueNameKey, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType DeletePropertySwatch(string accountId, string propertyNameKey, string propertySwatchNameKey, string requesterId, RequesterType requesterType, string sharedClientKey);
    }
}
