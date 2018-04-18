using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Logging.AccountLogs.Types
{
    [DataContract]
    public enum ActivityType
    {
        #region Authentication

        [EnumMember]
        Authentication_Passed,

        [EnumMember]
        Authentication_Failed,

        #endregion

        #region Account

        //[EnumMember]
        //Account_Subscription_Created,

        [EnumMember]
        Account_Plan_Updated,

        [EnumMember]
        Account_CreditCard_AddedUpdated,

        [EnumMember]
        Account_CreditCard_Deleted,

        [EnumMember]
        Account_Closure_Requested,

        #endregion

        #region AccountUser

        [EnumMember]
        AccountUser_Invited,

        [EnumMember]
        AccountUser_Created,

        [EnumMember]
        AccountUser_Deleted,

        [EnumMember]
        AccountUser_Edited,

        [EnumMember]
        AccountUser_Role_Updated,

        [EnumMember]
        AccountUser_OwnershipStatus_Updated,

        [EnumMember]
        AccountUser_ApplicationImage_Created,

        #endregion

        #region Credits

        [EnumMember]
        Credits_Purchased,

        [EnumMember]
        Credits_Spent,

        [EnumMember]
        Credits_Traded,

        [EnumMember]
        Credits_Received,

        #endregion

        #region Schema

        [EnumMember]
        Schema_Initialized,

        [EnumMember]
        Schema_Upgraded,

        #endregion

        #region Application

        #region Categorization

        [EnumMember]
        Inventory_CategoryCreated,

        [EnumMember]
        Inventory_CategoryRenamed,

        [EnumMember]
        Inventory_CategoryVisibilityChanged,

        [EnumMember]
        Inventory_CategoryUpdated,

        [EnumMember]
        Inventory_CategoryDeleted,

        [EnumMember]
        Inventory_SubcategoryCreated,

        [EnumMember]
        Inventory_CategoriesReordered,

        [EnumMember]
        Inventory_CategoryOrderingReset,

        [EnumMember]
        Inventory_SubcategoryRenamed,

        [EnumMember]
        Inventory_SubcategoryUpdated,

        [EnumMember]
        Inventory_SubcategoryVisibilityChanged,

        [EnumMember]
        Inventory_SubcategoryDeleted,

        [EnumMember]
        Inventory_SubcategoriesReordered,

        [EnumMember]
        Inventory_SubcategoryOrderingReset,

        [EnumMember]
        Inventory_SubsubcategoryCreated,

        [EnumMember]
        Inventory_SubsubcategoryRenamed,

        [EnumMember]
        Inventory_SubsubcategoryUpdated,

        [EnumMember]
        Inventory_SubsubcategoryVisibilityChanged,

        [EnumMember]
        Inventory_SubsubcategoryDeleted,

        [EnumMember]
        Inventory_SubsubcategoriesReordered,

        [EnumMember]
        Inventory_SubsubcategoryOrderingReset,

        [EnumMember]
        Inventory_SubsubsubcategoryCreated,

        [EnumMember]
        Inventory_SubsubsubcategoryRenamed,

        [EnumMember]
        Inventory_SubsubsubcategoryUpdated,

        [EnumMember]
        Inventory_SubsubsubcategoryVisibilityChanged,

        [EnumMember]
        Inventory_SubsubsubcategoryDeleted,

        [EnumMember]
        Inventory_SubsubsubcategoriesReordered,


        [EnumMember]
        Inventory_SubsubsubcategoryOrderingReset,


        #endregion

        #region ApiKeys

        [EnumMember]
        ApiKeys_KeyGenerated,

        [EnumMember]
        ApiKeys_KeyRegenerated,

        [EnumMember]
        ApiKeys_KeyDeleted,

        #endregion

        #region Products

        [EnumMember]
        Inventory_ProductCreated,

        [EnumMember]
        Inventory_ProductImageAdded,

        [EnumMember]
        Inventory_ProductVisibilityChanged,

        [EnumMember]
        Inventory_ProductRenamed,

        [EnumMember]
        Inventory_ProductsReordered,

        [EnumMember]
        Inventory_ProductOrderingReset,

        #endregion

        #region Tags

        [EnumMember]
        Inventory_TagCreated,

        [EnumMember]
        Inventory_TagAdded,

        [EnumMember]
        Inventory_TagAddedToProduct,

        [EnumMember]
        Inventory_TagRemovedFromProduct,

        [EnumMember]
        Inventory_TagDeleted,

        #endregion

        #region Properties

        [EnumMember]
        Inventory_PropertyCreated,

        [EnumMember]
        Inventory_PropertyUpdatedOnProduct,

        [EnumMember]
        Inventory_PropertyRemovedOnProduct,

        [EnumMember]
        Inventory_PropertyValueCreated,

        [EnumMember]
        Inventory_PropertyDeleted,

        [EnumMember]
        Inventory_PropertyVisibilityChanged,

        [EnumMember]
        Inventory_PropertyValueDeleted,

        #endregion

        #region Sales

        [EnumMember]
        Sales_LeadCreated,

        [EnumMember]
        Sales_LeadFollowedUp,

        #endregion

        #region Customers

        [EnumMember]
        Customer_CustomerCreated,

        [EnumMember]
        Customer_CustomerEdited,

        #endregion

        #region ApplicationImage

        /*
        [EnumMember]
        ApplicationImage_Created,

        [EnumMember]
        ApplicationImage_Deleted,
        */

        #endregion


        #endregion


    }

}
