using Sahara.Core.Accounts;
using Sahara.Core.Accounts.Capacity.Public;
//using Sahara.Core.Application.ApplicationImages;
using Sahara.Core.Application.Categorization.Models;
using Sahara.Core.Application.Categorization.Public;
using Sahara.Core.Application.Tags.Public;
//using Sahara.Core.Application.Testing.Public;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.AccountLogs;
using Sahara.Core.Logging.AccountLogs.Types;
using Sahara.Core.Platform.Requests;
using Sahara.Core.Platform.Requests.Models;
using WCF.WcfEndpoints.Contracts.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCF.WcfEndpoints.Service.Application
{
    public class ApplicationDataInjectionService : IApplicationDataInjectionService
    {
        #region DocumentDB

        /// <summary>
        /// Injecting batch documents for an account helps us to test partitions, DocDB Fault Tolerance & DocDB batch deletions during deprovisioning
        /// These docuents are injected as "Images" without any Category, Subcategory or Tag affiliations - so they will not show up on the imaging screen
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="documentInjectionCount"></param>
        /// <param name="requesterId"></param>
        /// <param name="requesterType"></param>
        /// <returns></returns>
        public DataAccessResponseType InjectImageDocumentsIntoAccount(string accountId, int imageDocumentInjectionCount, string requesterId, RequesterType requesterType, string sharedClientKey)
        {

            // Ensure the clients are certified.
            if (sharedClientKey != Sahara.Core.Platform.Requests.RequestManager.SharedClientKey)
            {
                return null;
            }

            return null;

            /*

            //Get ACCOUNT
            var account = AccountManager.GetAccount(accountId, true, AccountManager.AccountIdentificationType.AccountID);

            #region Validate Request

            var requesterName = string.Empty;
            var requesterEmail = string.Empty;

            var requestResponseType = RequestManager.ValidateRequest(requesterId,
                requesterType, out requesterName, out requesterEmail,
                Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin);

            if (!requestResponseType.isApproved)
            {
                //Request is not approved, send results:
                return new DataAccessResponseType { isSuccess = false, ErrorMessage = requestResponseType.requestMessage };
            }

            #endregion

            #region Validate Plan Capabilities

            //Verify that current document count + the injected document count is below maximum "images" allowed by this plan
            //if ((ApplicationImagesManager.GetApplicationImageCount(account) + imageDocumentInjectionCount) > account.PaymentPlan.MaxProducts) //<-- We base our document limit count on images
            //{
                //Log Limitation Issues (or send email) so that Platform Admins can immediatly contact Accounts that have hit their limits an upsell themm
                //Sahara.Core.Logging.PlatformLogs.Helpers.PlatformLimitationsHelper.LogLimitationAndAlertAdmins("images", account.AccountID.ToString(), account.AccountName);

                //return new DataAccessResponseType { isSuccess = false, ErrorMessage = "Your account plan does not allow for more than " + account.PaymentPlan.MaxProducts + " images, please update your plan to add more." };
            //}

            #endregion

            var result = DataInjectionManager.InjectDocuments(accountId, imageDocumentInjectionCount);

            #region Log Activity (IGNORED)

            /*
            if (result.isSuccess)
            {
                try
                {

                    //Object Log ---------------------------
                    AccountLogManager.LogActivity(
                        accountId,
                        CategoryType.ApplicationTests,
                        ActivityType.ApplicationTests_DocumentInjection,
                        documentInjectionCount + " test documents injected",
                        requesterName + " injected " + documentInjectionCount + " test documents",
                        requesterId,
                        requesterName,
                        requesterEmail,
                        null,
                        null,
                        result.SuccessMessage);
                }
                catch { }
             * 
             
            }* /

            #endregion

            #region Invalidate Account Capacity Cache

            AccountCapacityManager.InvalidateAccountCapacitiesCache(accountId);

            #endregion

            return result;
            */
        }

        #endregion
    }
}
