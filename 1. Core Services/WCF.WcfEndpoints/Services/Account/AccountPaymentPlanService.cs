using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts;
using Sahara.Core.Common.Services.Stripe;
using WCF.WcfEndpoints.Contracts.Account;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Requests.Models;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Accounts.PaymentPlans.Models;
using Sahara.Core.Accounts.PaymentPlans.Public;

namespace WCF.WcfEndpoints.Service.Account
{
    public class AccountPaymentPlanService : IAccountPaymentPlanService
    {
        #region Get

        public List<PaymentPlan> GetPaymentPlans(bool includeHiddenPlans, bool orderByRateAsc)
        {
            return PaymentPlanManager.GetPaymentPlans(includeHiddenPlans, orderByRateAsc);
        }

        public List<PaymentFrequency> GetPaymentFrequencies()
        {
            return PaymentPlanManager.GetPaymentFrequencies();
        }

        public PaymentPlan GetPaymentPlan(string planName)
        {
            return PaymentPlanManager.GetPaymentPlan(planName);
        }

        public PaymentFrequency GetPaymentFrequency(string frequencyId)
        {
            return PaymentPlanManager.GetPaymentFrequency(frequencyId);
        }

        #endregion

        #region Create

        public DataAccessResponseType CreatePaymentPlan(string paymentPlanName, decimal monthlyRate, int maxUsers,
            int maxCategorizationsPerSet, int maxProductsPerSet, int maxProperties, int maxValuesPerProperty, int maxTags,
            bool allowSalesLeads, bool allowImageEnhancements, bool allowLocationData, bool allowCustomOrdering, bool allowThemes, int monthlySupportHours,
            int maxImageGroups, int maxImageFormats, int maxImageGalleries, int maxImagesPerGallery,
            bool visible,
            string requesterId, RequesterType requesterType, string sharedClientKey)
        {


            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            var paymentPlan = new PaymentPlan
            {
                PaymentPlanName = paymentPlanName,
                MonthlyRate = monthlyRate,
                MaxUsers = maxUsers,
                
                MaxCategorizationsPerSet = maxCategorizationsPerSet,
                MaxProductsPerSet = maxProductsPerSet,

                MaxProperties = maxProperties,
                MaxValuesPerProperty = maxValuesPerProperty,

                MaxTags = maxTags,
                AllowImageEnhancements = allowImageEnhancements,
                MonthlySupportHours = monthlySupportHours,
                //BasicSupport = basicSupport,
                //EnhancedSupport = enhancedSupport,
                AllowSalesLeads = allowSalesLeads,
                AllowLocationData = allowLocationData,
                AllowCustomOrdering = allowCustomOrdering,
                AllowThemes = allowThemes,

                MaxImageGroups = maxImageGroups,
                MaxImageFormats = maxImageFormats,
                MaxImageGalleries = maxImageGalleries,
                MaxImagesPerGallery = maxImagesPerGallery,

                Visible = visible
            };

            return PaymentPlanManager.CreatePaymentPlan(paymentPlan);
        }


        #endregion

        #region Update

        /*
        public DataAccessResponseType UpdatePlanName(string paymentPlanName, string newName, string requesterId, RequesterType requesterType)
        {
            #region Validate Request

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            return PaymentPlanManager.UpdatePlanName(paymentPlanName, newName);
        }
        */

        public DataAccessResponseType UpdatePlanVisibility(string paymentPlanName, bool newVisibility, string requesterId, RequesterType requesterType, string sharedClientKey)
        {

            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            return PaymentPlanManager.UpdatePlanVisibility(paymentPlanName, newVisibility);
        }

        #region Limitations

        /*
        public DataAccessResponseType UpdatePlanMaxUsers(string paymentPlanName, int newUserMax, string requesterId, RequesterType requesterType)
        {
            #region Validate Request
            
            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            return PaymentPlanManager.UpdatePlanMaxUsers(paymentPlanName, newUserMax);
        }

        public DataAccessResponseType UpdatePlanMaxCategories(string paymentPlanName, int newCategoryMax, string requesterId, RequesterType requesterType)
        {
            #region Validate Request
            
            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            return PaymentPlanManager.UpdatePlanMaxCategories(paymentPlanName, newCategoryMax);
        }

        public DataAccessResponseType UpdatePlanMaxSubcategories(string paymentPlanName, int newSubcategoryMax, string requesterId, RequesterType requesterType)
        {
            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            return PaymentPlanManager.UpdatePlanMaxSubcategories(paymentPlanName, newSubcategoryMax);
        }

        public DataAccessResponseType UpdatePlanMaxTags(string paymentPlanName, int newTagMax, string requesterId, RequesterType requesterType)
        {
            #region Validate Request
            
            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            return PaymentPlanManager.UpdatePlanMaxTags(paymentPlanName, newTagMax);
        }



        public DataAccessResponseType UpdatePlanMaxImages(string paymentPlanName, int newImageMax, string requesterId, RequesterType requesterType)
        {
            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            return PaymentPlanManager.UpdatePlanMaxImages(paymentPlanName, newImageMax);
        }


        public DataAccessResponseType UpdatePlanAllowImageEnhancements(string paymentPlanName, bool allowEnhancements, string requesterId, RequesterType requesterType)
        {
            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };

            }

            #endregion

            return PaymentPlanManager.UpdatePlanAllowImageEnhancements(paymentPlanName, allowEnhancements);
        }

    */
        #endregion


        #endregion

        #region Delete

        public DataAccessResponseType DeletePaymentPlan(string paymentPlanName, string requesterId, RequesterType requesterType, string sharedClientKey)
        {

            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;
            
            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin,
                null);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }
            #endregion

            return PaymentPlanManager.DeletePaymentPlan(paymentPlanName);
        }

        #endregion
    }
}
