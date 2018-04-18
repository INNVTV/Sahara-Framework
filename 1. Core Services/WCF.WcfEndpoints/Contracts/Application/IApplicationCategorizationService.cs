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
    public interface IApplicationCategorizationService
    {
        #region full tree view

        [OperationContract]
        List<CategoryTreeModel> GetCategoryTree(string accountNameKey, bool includeHidden, string sharedClientKey);

        #endregion

        #region Categories

        [OperationContract]
        DataAccessResponseType CreateCategory(string accountId, string categoryName, bool isVisible, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RenameCategory(string accountId, string categoryId, string newCategoryName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateCategoryVisibleState(string accountId, string categoryId, bool visibleState, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateCategoryDescription(string accountId, string categoryId, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ReorderCategories(string accountId, Dictionary<string, int> categoryOrderingDictionary, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ResetCategoryOrdering(string accountId, string requesterId, RequesterType requesterType, string sharedClientKey);

        //[OperationContract]
        //int GetCategoryCount(string accountId);

        [OperationContract]
        List<CategoryModel> GetCategories(string accountNameKey, bool includeHidden, string sharedClientKey);

        [OperationContract]
        CategoryModel GetCategoryByName(string accountNameKey, string categoryName, bool includeHiddenSubcategories, string sharedClientKey);

        //[OperationContract]
        //CategoryModel GetCategory(string accountId, string categoryAttribute, bool includeHiddenSubcategories);

        [OperationContract]
        DataAccessResponseType DeleteCategory(string accountId, string categoryId, string requesterId, RequesterType requesterType, string sharedClientKey);

        #endregion

        #region Subcategories

        [OperationContract]
        DataAccessResponseType CreateSubcategory(string accountId, string categoryId, string subcategoryName, bool isVisible, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RenameSubcategory(string accountId, string subcategoryId, string newSubcategoryName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateSubcategoryVisibleState(string accountId, string subcategoryId, bool visibleState, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateSubcategoryDescription(string accountId, string subcategoryId, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ReorderSubcategories(string accountId, Dictionary<string, int> subcategoryOrderingDictionary, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ResetSubcategoryOrdering(string accountId, string categoryId, string requesterId, RequesterType requesterType, string sharedClientKey);

        //[OperationContract]
        //int GetSubcategoryCount(string accountId);

        //[OperationContract]
        //List<SubcategoryModel> GetSubcategories(string accountId, string categoryAttribute, bool includeHidden);

        [OperationContract]
        SubcategoryModel GetSubcategoryByNames(string accountNameKey, string categoryName, string subcategoryName, bool includeHidden, string sharedClientKey);

        [OperationContract]
        SubcategoryModel GetSubcategoryByFullyQualifiedName(string accountNameKey, string fullyQaulifiedName, bool includeHidden, string sharedClientKey);

        //[OperationContract]
        //SubcategoryModel GetSubcategory(string accountId, string categoryAttribute, string subcategoryAttribute, bool includeHidden);

        [OperationContract]
        DataAccessResponseType DeleteSubcategory(string accountId, string subcategoryId, string requesterId, RequesterType requesterType, string sharedClientKey);

        #endregion

        #region Subsubcategories

        [OperationContract]
        DataAccessResponseType CreateSubsubcategory(string accountId, string subcategoryId, string subsubcategoryName, bool isVisible, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RenameSubsubcategory(string accountId, string subsubcategoryId, string newSubsubcategoryName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateSubsubcategoryVisibleState(string accountId, string subsubcategoryId, bool visibleState, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateSubsubcategoryDescription(string accountId, string subsubcategoryId, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ReorderSubsubcategories(string accountId, Dictionary<string, int> subsubcategoryOrderingDictionary, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ResetSubsubcategoryOrdering(string accountId, string subcategoryId, string requesterId, RequesterType requesterType, string sharedClientKey);

        //[OperationContract]
        //int GetSubsubcategoryCount(string accountId);

        //[OperationContract]
        //List<SubsubcategoryModel> GetSubsubcategories(string accountId, string categoryAttribute, string subcategoryAttribute, bool includeHidden);

        [OperationContract]
        SubsubcategoryModel GetSubsubcategoryByNames(string accountNameKey, string categoryName, string subcategoryName, string subsubcategoryName, bool includeHidden, string sharedClientKey);

        [OperationContract]
        SubsubcategoryModel GetSubsubcategoryByFullyQualifiedName(string accountNameKey, string fullyQualifiedName, bool includeHidden, string sharedClientKey);


        //[OperationContract]
        //SubsubcategoryModel GetSubsubcategory(string accountId, string categoryAttribute, string subcategoryAttribute, string subsubcategoryAttribute, bool includeHidden);

        [OperationContract]
        DataAccessResponseType DeleteSubsubcategory(string accountId, string subsubcategoryId, string requesterId, RequesterType requesterType, string sharedClientKey);

        #endregion

        #region Subsubsubcategories

        [OperationContract]
        DataAccessResponseType CreateSubsubsubcategory(string accountId, string subsubcategoryId, string subsubsubcategoryName, bool isVisible, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType RenameSubsubsubcategory(string accountId, string subsubsubcategoryId, string newSubsubcategoryName, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateSubsubsubcategoryVisibleState(string accountId, string subsubsubcategoryId, bool visibleState, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType UpdateSubsubsubcategoryDescription(string accountId, string subsubsubcategoryId, string newDescription, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ReorderSubsubsubcategories(string accountId, Dictionary<string, int> subsubsubcategoryOrderingDictionary, string requesterId, RequesterType requesterType, string sharedClientKey);

        [OperationContract]
        DataAccessResponseType ResetSubsubsubcategoryOrdering(string accountId, string subsubcategoryId, string requesterId, RequesterType requesterType, string sharedClientKey);

        //[OperationContract]
        //int GetSubsubsubcategoryCount(string accountId);

        //[OperationContract]
        //List<SubsubcategoryModel> GetSubsubcategories(string accountId, string categoryAttribute, string subcategoryAttribute, bool includeHidden);

        [OperationContract]
        SubsubsubcategoryModel GetSubsubsubcategoryByNames(string accountNameKey, string categoryName, string subcategoryName, string subsubcategoryName, string subsubsubcategoryName, string sharedClientKey);

        [OperationContract]
        SubsubsubcategoryModel GetSubsubsubcategoryByFullyQualifiedName(string accountNameKey, string fullyQUalifiedDame, string sharedClientKey);

        //[OperationContract]
        //SubsubsubcategoryModel GetSubsubsubcategoryByID(string accountId, string subsubsubcategoryId);

        //[OperationContract]
        //SubsubsubcategoryModel GetSubsubsubcategory(string accountId, string categoryAttribute, string subcategoryAttribute, string subsubcategoryAttribute, string subsubsubcategoryAttribute, bool includeHidden);

        [OperationContract]
        DataAccessResponseType DeleteSubsubsubcategory(string accountId, string subsubsubcategoryId, string requesterId, RequesterType requesterType, string sharedClientKey);

        #endregion

        #region Shared

        [OperationContract]
        int GetCategorizationCount(string accountId);

        #endregion
    }
}
