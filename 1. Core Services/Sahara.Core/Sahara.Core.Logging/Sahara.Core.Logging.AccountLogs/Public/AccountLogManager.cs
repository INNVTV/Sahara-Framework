using System.Collections.Generic;
using System.Threading;
using Sahara.Core.Logging.AccountLogs.Types;
using Sahara.Core.Logging.AccountLogs.Models;
//using System.Web.Hosting;

namespace Sahara.Core.Logging.AccountLogs
{
    public static class AccountLogManager
    {
        #region Log Methods

        public static void LogActivity(string accountId, string storagePartition, CategoryType categoryType, ActivityType activityType, string description, string details = null, string userID = null, string userName = null, string userEmail = null, string ipAddress = null, string origin = null, string objectId = null, string objectName = null, string serializedObject = null) 
        {
            if(!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(storagePartition)) //<-- Make sure an accountId and storagePartition are included!!!
            {
                try
                {
                    #region Fire & Forget on a background thread (More overhead)
                    /*
                    System.Threading.ThreadStart threadStart = delegate {

                        //Log the activity
                        PrivateMethods.WriteAccountLog(activity, accountId, description, details, userID, userDisplayName, userEmail, itemType, itemID, itemName, ipAddress);

                        //Log the activity into the "transaction" log (optional) <---Remove if having a master Transaction log is not desired (will limit the amount of logs the system stores)
                        //If removing, be sure to also remove the TransactionActivityTypes & remove "Transaction" from the AccountLogName                
                        TransactionLogType transaction = new TransactionLogType(activity.Activity);
                        PrivateMethods.WriteAccountLog(transaction, accountId, description, details, userID, userDisplayName, userEmail, itemType, itemID, itemName, ipAddress);


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
                
                        Internal.WriteAccountLog(accountId, categoryType, activityType, description, details, userID, userName, userEmail, objectId, objectName);

                        //Log the activity into the "transaction" log (optional) <---Remove if having a master Transaction log is not desired (will limit the amount of logs the system stores)
                        //If removing, be sure to also remove the TransactionActivityTypes & remove "Transaction" from the AccountLogName                
                        //TransactionLogType transaction = new TransactionLogType(activity.Activity);
                        //Internal.WriteAccountLog(transaction, accountId, description, details, userID, userDisplayName, userEmail, itemType, itemID, itemName, ipAddress);
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

                        Internal.WriteAccountLog(accountId, storagePartition, categoryType, activityType, description, details, userID, userName, userEmail, ipAddress, origin, objectId, objectName, serializedObject)
                
                        );

            

                    #endregion
                }
                catch
                {

                }

            }

        }

        #endregion

        #region Get Methods

        public static List<AccountActivityLog> GetAccountLog(string accountId, string storagePartition, int maxRecords)
        {
            return Transformations.TransformAccountLogTableEntitiesToAccountActivityLogs(Internal.GetAccountLog(accountId, storagePartition, maxRecords));
        }
        public static List<AccountActivityLog> GetAccountLogByCategory(string accountId, string storagePartition, CategoryType categoryType, int maxRecords)
        {
            return Transformations.TransformAccountLogTableEntitiesToAccountActivityLogs(Internal.GetAccountLogByCategory(accountId, storagePartition, categoryType, maxRecords));
        }
        public static List<AccountActivityLog> GetAccountLogByActivity(string accountId, string storagePartition, ActivityType activityType, int maxRecords)
        {
            return Transformations.TransformAccountLogTableEntitiesToAccountActivityLogs(Internal.GetAccountLogByActivity(accountId, storagePartition, activityType, maxRecords));
        }
        public static List<AccountActivityLog> GetAccountLogByUser(string accountId, string storagePartition, string userId, int maxRecords)
        {
            return Transformations.TransformAccountLogTableEntitiesToAccountActivityLogs(Internal.GetAccountLogByUser(accountId, storagePartition, userId, maxRecords));
        }
        public static List<AccountActivityLog> GetAccountLogByObject(string accountId, string storagePartition, string objectId, int maxRecords)
        {
            return Transformations.TransformAccountLogTableEntitiesToAccountActivityLogs(Internal.GetAccountLogByObject(accountId, storagePartition, objectId, maxRecords));
        }
        #endregion

    }
}
