using Sahara.Core.Accounts;
using Sahara.Core.Accounts.PaymentPlans.Public;
using Sahara.Core.Platform.Constraints.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Constraints
{
    public static class AccountConstraintsManager
    {
        public static ConstraintResponseType CheckUserConstraint(string accountId)
        {
            var response = new ConstraintResponseType { isConstrained = false };

            // Get the Account & the associated Plan:
            var account = AccountManager.GetAccount(accountId);
            var plan = PaymentPlanManager.GetPaymentPlan(account.PaymentPlanName);

            int maxUsers = plan.MaxUsers;

            int userCount = account.Users.Count;
            int invitationCount = AccountUserManager.GetInvitations(accountId, account.StoragePartition).Count;

            int totalAssociatedUsers = (userCount + invitationCount) - 1; //<--Offset by 1 to hide: platformadmin@[Config_PlatformEmail]  LEGACY: //<-- We remove 1 user from the validation as the account creator does not count in the constraint

            if (maxUsers <= totalAssociatedUsers) 
            {
                response.isConstrained = true;
                response.constraintMessage = "You have reached your limit of " + maxUsers + " users.";
            }


            return response;
        }

        public static ConstraintResponseType CheckObjectConstraint(string accountId)
        {
            var response = new ConstraintResponseType { isConstrained = false };

            //compare existing object count to max allowed

            return response;
        }

        public static ConstraintResponseType CheckStorageConstraint(string accountId)
        {
            var response = new ConstraintResponseType { isConstrained = false };

            // Easy way to calculate amount of storage used on a blob container?
            //blob.FetchAttributes()
            //http://blogs.msdn.com/b/avkashchauhan/archive/2012/04/27/windows-azure-blob-size-return-0-even-when-blob-is-accessible-and-downloaded-without-any-problem.aspx

            //May have to loop through on containers nightly and block use when maxing out? 
            //http://stackoverflow.com/questions/17444037/how-to-find-out-azure-blob-storage-container-size-in-visual-studio-with-azure-sd


            /* We'll cache this once per hour to avoid overheating this call. when an acount get's to 80% > of storage we send a warning, when 95% we send a final alert (Custodian nightly)
             * public static long GetSpaceUsed(string containerName)
        {

            var container= CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString)
                             .CreateCloudBlobClient()
                             .GetContainerReference(containerName);
            if (container.Exists())
            {
                return (from CloudBlockBlob blob in
                            container.ListBlobs(useFlatBlobListing: true)
                        select blob.Properties.Length
                  ).Sum();
            }
            return 0;
        }
             * 
             */

            return response;
        }
    }
}
