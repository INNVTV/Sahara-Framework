using System.Collections.Generic;
using System.Threading;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Logging.PlatformLogs.Models;

namespace Sahara.Core.Logging.PlatformLogs
{
    public static class PlatformLogManager
    {

        #region Log Methods

        public static void LogActivity(CategoryType categoryType, ActivityType activityType, string description, string details = null, string accountId = null, string accountName = null, string userID = null, string userName = null, string userEmail = null, string ipAddress = null, string origin = null, string serializedObject = null) 
        {
            try
            {
                #region Fire & Forget on a background thread (More overhead)
                /*
                System.Threading.ThreadStart threadStart = delegate {

                    //Log the activity
                    PrivateMethods.WritePlatformLog(activity, PlatformId, description, details, userID, userDisplayName, userEmail, itemType, itemID, itemName, ipAddress);

                    //Log the activity into the "transaction" log (optional) <---Remove if having a master Transaction log is not desired (will limit the amount of logs the system stores)
                    //If removing, be sure to also remove the TransactionActivityTypes & remove "Transaction" from the PlatformLogName                
                    TransactionLogType transaction = new TransactionLogType(activity.Activity);
                    PrivateMethods.WritePlatformLog(transaction, PlatformId, description, details, userID, userDisplayName, userEmail, itemType, itemID, itemName, ipAddress);


                };
                System.Threading.Thread thread = new System.Threading.Thread(threadStart);
                thread.IsBackground = true;
                thread.Start();
                 */
                #endregion

                #region Fire & Forget using ThreadPool.QueueUserWorkItem (Managed threads at the ready)

                /*
                ThreadPool.QueueUserWorkItem(o =>
                {
                    //Log the activity
                
                    Internal.WritePlatformLog(PlatformId, categoryType, activityType, description, details, userID, userName, userEmail, objectId, objectName);

                    //Log the activity into the "transaction" log (optional) <---Remove if having a master Transaction log is not desired (will limit the amount of logs the system stores)
                    //If removing, be sure to also remove the TransactionActivityTypes & remove "Transaction" from the PlatformLogName                
                    //TransactionLogType transaction = new TransactionLogType(activity.Activity);
                    //Internal.WritePlatformLog(transaction, PlatformId, description, details, userID, userDisplayName, userEmail, itemType, itemID, itemName, ipAddress);
                });
             
                 */

                #endregion

                #region  Fire & Forget using QueueBackgroundWorkItem

                #region QBWI Details

                /* QueueBackgroundWorkItem overview
                 * 
                 * QueueBackgroundWorkItem to reliably schedule and run background processes in ASP.NET
                 * 
                 * QBWI schedules a task which can run in the background, independent of any request.
                 * This differs from a normal ThreadPool work item in that ASP.NET automatically keeps track of how many work items registered through this API are currently running,
                 * and the ASP.NET runtime will try to delay AppDomain shutdown until these work items have finished executing.
                 * 
                 * QueueBackgroundWorkItem (QBWI). This was specifically added to enable ASP.NET apps to reliably run short-lived background tasks.
                 * 
                 * QBWI will register the work with ASP.NET. When ASP.NET has to recycle, it will notify the background work (by setting a CancellationToken)
                 * and will then wait up to 30 seconds for the work to complete. If the background work doesn’t complete in that time frame, the work will mysteriously disappear.
                 * 
                 * REQUIRES .Net Framework 4.5.2 !!!!! Make sure you install this version of the framework locally AND on your CloudServices
                 * 
                 */

                #endregion

                //SWITCH BACK ONCE .NET 4.5.2 is available for Azure.....
                //HostingEnvironment.QueueBackgroundWorkItem(ct => 
                ThreadPool.QueueUserWorkItem(o =>

                    Internal.WritePlatformLog(categoryType, activityType, description, details, accountId, accountName, userID, userName, userEmail, ipAddress, origin, serializedObject)

                    );



                #endregion
            }
            catch
            {

            }

        }

        #endregion

        #region Get Methods

        public static List<PlatformActivityLog> GetPlatformLog(int maxRecords)
        {
            return Transformations.TransformPlatformLogTableEntitiesToPlatformActivityLogs(Internal.GetPlatformLog(maxRecords));
        }
        public static List<PlatformActivityLog> GetPlatformLogByCategory(CategoryType categoryType, int maxRecords)
        {
            return Transformations.TransformPlatformLogTableEntitiesToPlatformActivityLogs(Internal.GetPlatformLogByCategory(categoryType, maxRecords));
        }
        public static List<PlatformActivityLog> GetPlatformLogByActivity(ActivityType activityType, int maxRecords)
        {
            return Transformations.TransformPlatformLogTableEntitiesToPlatformActivityLogs(Internal.GetPlatformLogByActivity(activityType, maxRecords));
        }
        public static List<PlatformActivityLog> GetPlatformLogByAccount(string accountId, int maxRecords)
        {
            return Transformations.TransformPlatformLogTableEntitiesToPlatformActivityLogs(Internal.GetPlatformLogByAccount(accountId, maxRecords));
        }
        public static List<PlatformActivityLog> GetPlatformLogByUser(string userId, int maxRecords)
        {
            return Transformations.TransformPlatformLogTableEntitiesToPlatformActivityLogs(Internal.GetPlatformLogByUser(userId, maxRecords));
        }

        #endregion

        #region Clear Methods

        /*
        public static void ClearLogs()
        {
            //Internal.ClearLogs();
        }
        */
        #endregion

    }


}
