using Sahara.Core.Accounts.PaymentPlans.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Contracts.Account
{
    [ServiceContract]
    public interface IAccountPaymentPlanService
    {

      /*==================================================================================
      * Get
      ==================================================================================*/

        [OperationContract]
        List<PaymentPlan> GetPaymentPlans(bool includeHiddenPlans, bool orderByRateAsc);

        [OperationContract]
        List<PaymentFrequency> GetPaymentFrequencies();

        [OperationContract]
        PaymentPlan GetPaymentPlan(string planName);

        [OperationContract]
        PaymentFrequency GetPaymentFrequency(string frequencyId);


        /*==================================================================================
        * Create
        ==================================================================================*/

        [OperationContract]
        DataAccessResponseType CreatePaymentPlan(string paymentPlanName, decimal monthlyRate, int maxUsers,
            int maxCategorizationsPerSet, int maxProductsPerSet, int maxProperties, int maxValuesPerProperty, int maxTags,
            bool allowSalesLeads, bool allowImageEnhancements, bool allowLocationData, bool allowCustomOrdering, bool allowThemes, int monthlySupportHours, int maxImageGroups, int maxImageFormats, int maxImageGalleries, int maxImagesPerGallery, bool visible,
            string requesterId, RequesterType requesterType, string sharedClientKey);

     /*==================================================================================
      * Update
      ==================================================================================*/

        //[OperationContract]
        //DataAccessResponseType UpdatePlanName(string paymentPlanName, string newName, string requesterId, RequesterType requesterType);


        [OperationContract]
        DataAccessResponseType UpdatePlanVisibility(string paymentPlanName, bool newVisibility, string requesterId, RequesterType requesterType, string sharedClientKey);
/*

        [OperationContract]
        DataAccessResponseType UpdatePlanMaxUsers(string paymentPlanName, int newUserMax, string requesterId, RequesterType requesterType);

        [OperationContract]
        DataAccessResponseType UpdatePlanMaxCategories(string paymentPlanName, int newCategoryMax, string requesterId, RequesterType requesterType);

        [OperationContract]
        DataAccessResponseType UpdatePlanMaxSubcategories(string paymentPlanName, int newSubcategoryMax, string requesterId, RequesterType requesterType);

        [OperationContract]
        DataAccessResponseType UpdatePlanMaxTags(string paymentPlanName, int newTagMax, string requesterId, RequesterType requesterType);

        [OperationContract]
        DataAccessResponseType UpdatePlanMaxImages(string paymentPlanName, int newImageMax, string requesterId, RequesterType requesterType);

        [OperationContract]
        DataAccessResponseType UpdatePlanAllowImageEnhancements(string paymentPlanName, bool allowEnhancements, string requesterId, RequesterType requesterType);
        */

     /*==================================================================================
      * Delete
      ==================================================================================*/

        [OperationContract]
        DataAccessResponseType DeletePaymentPlan(string paymentPlanName, string requesterId, RequesterType requesterType, string sharedClientKey);
    }

}
