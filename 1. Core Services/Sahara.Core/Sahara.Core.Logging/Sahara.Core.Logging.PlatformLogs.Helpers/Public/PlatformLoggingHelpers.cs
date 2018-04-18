using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Common.Services.SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;

namespace Sahara.Core.Logging.PlatformLogs.Helpers
{
    public static class PlatformExceptionsHelper
    {
        
        /// <summary>
        /// Helper function to Logs exceptions AND send emails to alert platform admins
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="details"></param>
        /// <param name="codeLocation"></param>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        public static bool LogExceptionAndAlertAdmins(Exception exception, string attemptedAction, MethodBase methodBase, string accountId = null, string accountName = null)
        {
            var serializedException = JsonConvert.SerializeObject(exception);
            attemptedAction = "An exception occurred while " + attemptedAction;

            #region Log The Error

            PlatformLogManager.LogActivity(

                CategoryType.Error,
                ActivityType.Error_Exception,
                exception.Message,
                attemptedAction,
                accountId,
                accountName,
                null,
                null,
                null,
                null,
                methodBase.ReflectedType.FullName + "." + methodBase.Name,
                serializedException
                );

            #endregion


            #region Email Platform Admins

            EmailManager.Send(
                Settings.Endpoints.Emails.PlatformEmailAddresses,
                Settings.Endpoints.Emails.FromExceptions,
                "Platform Exception",
                "Exception Alert!",

                "Exception location: <b>" + methodBase.ReflectedType.FullName + "." + methodBase.Name + "</b>" + 
                "<br/><br/>" +
                attemptedAction +
                "<br/><br/><b>" +
                exception.Message +
                "</b><br/><br/>" +
                accountId +
                "<br/><br/>" +
                accountName +
                "<br/><br/>" +
                serializedException,

                true,
                true

            );

            #endregion


            return true;
        }

        /// <summary>
        /// Helper function to Logs exceptions AND send emails to alert platform admins
        /// </summary>
        /// <param name="error"></param>
        /// <param name="details"></param>
        /// <param name="codeLocation"></param>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        public static bool LogErrorAndAlertAdmins(string error, string attemptedAction, MethodBase methodBase, string accountId = null, string accountName = null)
        {
            attemptedAction = "An error occurred while " + attemptedAction;

            #region Log The Error

            PlatformLogManager.LogActivity(

                CategoryType.Error,
                ActivityType.Error_Other,
                error,
                attemptedAction,
                accountId,
                accountName,
                null,
                null,
                null,
                null,
                methodBase.ReflectedType.FullName + "." + methodBase.Name,
                null
                );

            #endregion


            #region Email Platform Admins

            EmailManager.Send(
                Settings.Endpoints.Emails.PlatformEmailAddresses,
                Settings.Endpoints.Emails.FromExceptions,
                "Platform Error",
                "Error Alert!",

                "Error location: <b>" + methodBase.ReflectedType.FullName + "." + methodBase.Name + "</b>" +
                "<br/><br/>" +
                attemptedAction +
                "<br/><br/><b>" +
                error +
                "</b><br/><br/>" +
                accountId +
                "<br/><br/>" +
                accountName,

                true,
                true

            );

            #endregion


            return true;
        }
    }

    public static class PlatformLimitationsHelper
    {

        /// <summary>
        /// Helper function to log when an account reaches a limitation AND send emails to alert platform admins for upsell opportunities
        /// </summary>
        /// <param name="limitationReached"></param>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <returns></returns>
        public static bool LogLimitationAndAlertAdmins(string limitationObjectName, string accountId = null, string accountName = null)
        {

            #region Log The Limitation

            PlatformLogManager.LogActivity(

                CategoryType.Account,
                ActivityType.Account_LimitationReached,
                accountName + " has reached an account limitation",
                accountName + " has reached a limitation on their maximum allowed " + limitationObjectName,
                accountId,
                accountName,
                null,
                null,
                null,
                null,
                null,
                null
                );

            #endregion

            #region Email Platform Admins

            EmailManager.Send(
                Settings.Endpoints.Emails.PlatformEmailAddresses,
                Settings.Endpoints.Emails.FromPlatform,
                "Account Limitations",
                "Account Limitation Reached!",
                "<a href='http://" + Sahara.Core.Settings.Endpoints.URLs.PlatformDomain + "/account/" + accountId + "'><strong>" + accountName + "</strong></a> has attempted to go past a plan limitation on their maximum allowed " + limitationObjectName,
                true,
                true

            );

            #endregion


            return true;
        }

    }


    public static class PlatformAccountClosureHelper
    {

        /// <summary>
        /// Helper function to log when an account is closed AND send emails to alert platform admins for verification purposes
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="accountNameKey"></param>
        /// <param name="userEmail"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool LogAccountClosureAndAlertAdmins(string accountId, string accountName, string accountNameKey, string userName, string userId, string userEmail, bool isPaidAccount)
        {

            #region Log The Limitation

            PlatformLogManager.LogActivity(

                CategoryType.Account,
                ActivityType.Account_ClosureRequested,
                accountName + " has requested closure",
                accountName + " has closure request initiated by " + userName + "(" + userId + " | " + userEmail + ")",
                accountId,
                accountName,
                null,
                null,
                null,
                null,
                null,
                null
                );

            #endregion

            #region Email Platform Admins

            EmailManager.Send(
                Settings.Endpoints.Emails.PlatformEmailAddresses,
                Settings.Endpoints.Emails.FromPlatform,
                "Account Closure Request",
                accountName + " has requested account closure",
                "<a href='http://" + Sahara.Core.Settings.Endpoints.URLs.PlatformDomain + "/account/" + accountNameKey + "'><strong>" + accountName + "</strong></a> has been closed by " + userName + "(" + userId + " | " + userEmail + ")",
                true,
                true

            );

            #endregion


            return true;
        }

    }
}
