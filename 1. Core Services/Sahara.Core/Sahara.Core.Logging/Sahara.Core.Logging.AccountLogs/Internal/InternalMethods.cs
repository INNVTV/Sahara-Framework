using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Logging.AccountLogs.TableEntities;
using Sahara.Core.Logging.AccountLogs.Types;
using System.Text;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Logging.AccountLogs
{
    internal static class Internal
    {
        #region Write to Logs

        internal static void WriteAccountLog(string accountId, string storagePartition, CategoryType categoryType, ActivityType activityType, string description, string details = null, string userId = null, string userName = null, string userEmail = null, string ipAddress = null, string origin = null, string objectId = null, string objectName = null, string serializedObject = null)
        {
            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            //Gather up all the entities into a list for our parallel task to execute in a ForEach
            List<Object> entityTypes = new List<object>();


            //Create an instance of each entity type and pass in associated CloudTableClient & TableName
            AccountLogTableEntity_ByTime logTableEntity_Time = new AccountLogTableEntity_ByTime(cloudTableClient, Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + "activitylogbytime");
            entityTypes.Add(logTableEntity_Time);

            AccountLogTableEntity_ByCategory logTableEntity_Category = new AccountLogTableEntity_ByCategory(cloudTableClient, Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + "activitylogbycategory");
            entityTypes.Add(logTableEntity_Category);
            
            AccountLogTableEntity_ByActivity logTableEntity_Activity = new AccountLogTableEntity_ByActivity(cloudTableClient, Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) +  "activitylogbyactivity");
            entityTypes.Add(logTableEntity_Activity);

            if (userId != null)
            {
                AccountLogTableEntity_ByUserID logTableEntity_UserID = new AccountLogTableEntity_ByUserID(cloudTableClient, Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + "activitylogbyuserid");
                entityTypes.Add(logTableEntity_UserID);
            }

            if (objectId != null)
            {
                AccountLogTableEntity_ByObjectID logTableEntity_ObjectID = new AccountLogTableEntity_ByObjectID(cloudTableClient, Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + "activitylogbyobjectid");
                entityTypes.Add(logTableEntity_ObjectID);
            }
                      
            try
            {        
                Parallel.ForEach(entityTypes, obj =>
                {
                    
                    #region Trace Statements

                    //Display the id of the thread for each parallel instance to verifiy prallelism
                    //Trace.TraceInformation("Current thread ID: " + Thread.CurrentThread.ManagedThreadId);

                    #endregion

                    //Transform the LogItem into each corresponding table entity type for insert execution into logs
                    (obj as IAccountLogTableEntity).Category = categoryType.ToString();
                    (obj as IAccountLogTableEntity).Activity = activityType.ToString();
                    (obj as IAccountLogTableEntity).Description = description;
                    (obj as IAccountLogTableEntity).Details = details;

                    (obj as IAccountLogTableEntity).UserID = userId;
                    (obj as IAccountLogTableEntity).UserName = userName;
                    (obj as IAccountLogTableEntity).UserEmail = userEmail;

                    (obj as IAccountLogTableEntity).ObjectID = objectId;
                    (obj as IAccountLogTableEntity).ObjectName = objectName;

                    (obj as IAccountLogTableEntity).IPAddress = ipAddress;
                    (obj as IAccountLogTableEntity).Origin = origin;

                    (obj as IAccountLogTableEntity).Object = serializedObject;

                    //Create table for entity if not exists
                    //(obj as IAccountLogTableEntity).cloudTable.CreateIfNotExists();

                    //create an insert operation for each entity, assign to designated CloudTable, and add to our list of tasks:
                    TableOperation operation = TableOperation.Insert((obj as TableEntity));
                    (obj as IAccountLogTableEntity).cloudTable.Execute(operation);
                });
            }
            catch (Exception e)
            {
                //Email platform admins about this exception
                #region Email Platform Admins About Exception

                    #region Create Parameter String

                    var parametersString = new StringBuilder();

                    parametersString.Append("(");

                    try
                    {
                        parametersString.Append(accountId);
                        parametersString.Append(", ");

                        parametersString.Append(categoryType.ToString());
                        parametersString.Append(", ");

                        parametersString.Append(activityType.ToString());
                        parametersString.Append(", ");

                        parametersString.Append(description);
                        parametersString.Append(", ");

                        if (!String.IsNullOrEmpty(details))
                        {
                            parametersString.Append(details);
                            parametersString.Append(", ");
                        }
                        else
                        {
                            parametersString.Append("null");
                            parametersString.Append(", ");
                        }


                        if (!String.IsNullOrEmpty(userId))
                        {
                            parametersString.Append(userId);
                            parametersString.Append(", ");
                        }
                        else
                        {
                            parametersString.Append("null");
                            parametersString.Append(", ");
                        }

                        if (!String.IsNullOrEmpty(userName))
                        {
                            parametersString.Append(userName);
                            parametersString.Append(", ");
                        }
                        else
                        {
                            parametersString.Append("null");
                            parametersString.Append(", ");
                        }


                        if (!String.IsNullOrEmpty(userEmail))
                        {
                            parametersString.Append(userEmail);
                            parametersString.Append(", ");
                        }
                        else
                        {
                            parametersString.Append("null");
                            parametersString.Append(", ");
                        }

                        if (!String.IsNullOrEmpty(ipAddress))
                        {
                            parametersString.Append(ipAddress);
                            parametersString.Append(", ");
                        }
                        else
                        {
                            parametersString.Append("null");
                            parametersString.Append(", ");
                        }

                        if (!String.IsNullOrEmpty(origin))
                        {
                            parametersString.Append(origin);
                            parametersString.Append(", ");
                        }
                        else
                        {
                            parametersString.Append("null");
                            parametersString.Append(", ");
                        }

                        if (!String.IsNullOrEmpty(objectId))
                        {
                            parametersString.Append(objectId);
                            parametersString.Append(", ");
                        }
                        else
                        {
                            parametersString.Append("null");
                            parametersString.Append(", ");
                        }

                        if (!String.IsNullOrEmpty(objectName))
                        {
                            parametersString.Append(objectName);
                            parametersString.Append(", ");
                        }
                        else
                        {
                            parametersString.Append("null");
                            parametersString.Append(", ");
                        }


                        if (!String.IsNullOrEmpty(serializedObject))
                        {
                            parametersString.Append(serializedObject);
                            //parametersString.Append(", ");
                        }
                        else
                        {
                            parametersString.Append("null");
                            //parametersString.Append(", ");
                        }

                        parametersString.Append(")");
                    }
                    catch
                    {
                        parametersString.Append(" ...Error parsing next parameter -----> )");
                    }


                    #endregion


                #endregion

                //Log exception and email platform admins
                
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to write to an account activity log. Parameters: " + parametersString,
                    System.Reflection.MethodBase.GetCurrentMethod(),
                    accountId
                );
            }

        }

        #endregion

        #region Get Logs

        #region Get Entire Account Log By Time

        /// <summary>
        /// Gets ALL Categories and Activities for the account by time
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="maxRecords"></param>
        /// <returns></returns>
        internal static IEnumerable<AccountLogTableEntity> GetAccountLog(string accountId, string storagePartition, int maxRecords)
        {

            string _logFullName = Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + "activitylogbytime";

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();

            if (maxRecords > 0)
            {
                TableQuery<AccountLogTableEntity> query = new TableQuery<AccountLogTableEntity>().Take(maxRecords);

                return cloudTable.ExecuteQuery(query);
            }
            else
            {
                return null;
            }
            
        }

        #endregion

        #region Get an Entire Category by Time

        internal static IEnumerable<AccountLogTableEntity> GetAccountLogByCategory(string accountId, string storagePartition, CategoryType categoryType, int maxRecords)
        {

            string _logFullName = Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + "activitylogbycategory";

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();

            if (maxRecords > 0)
            {
                TableQuery<AccountLogTableEntity> query = new TableQuery<AccountLogTableEntity>().Take(maxRecords).
                    Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, categoryType.ToString()));

                return cloudTable.ExecuteQuery(query);
            }
            else
            {
                return null;
            }

            
        }

        #endregion

        #region Get an Entire Activity by Time

        internal static IEnumerable<AccountLogTableEntity> GetAccountLogByActivity(string accountId, string storagePartition, ActivityType activityType, int maxRecords)
        {

            string _logFullName = Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + "activitylogbyactivity";

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();

            if (maxRecords > 0)
            {
                TableQuery<AccountLogTableEntity> query = new TableQuery<AccountLogTableEntity>().Take(maxRecords).
                    Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, activityType.ToString()));

                return cloudTable.ExecuteQuery(query);
            }
            else
            {
                return null;
            }

            
        }

        #endregion

        #region Get an Entire Users Log by Time

        internal static IEnumerable<AccountLogTableEntity> GetAccountLogByUser(string accountId, string storagePartition, string userId, int maxRecords)
        {

            string _logFullName = Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + "activitylogbyuserid";

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();

            if (maxRecords > 0)
            {
                TableQuery<AccountLogTableEntity> query = new TableQuery<AccountLogTableEntity>().Take(maxRecords).
                    Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

                return cloudTable.ExecuteQuery(query);
            }
            else
            {
                return null;
            }

            
        }

        #endregion

        #region Get an Entire Object Log by Time

        internal static IEnumerable<AccountLogTableEntity> GetAccountLogByObject(string accountId, string storagePartition, string objectId, int maxRecords)
        {

            string _logFullName = Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + "activitylogbyobjectid";

            //CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();
            CloudTableClient cloudTableClient = Settings.Azure.Storage.GetStoragePartitionAccount(storagePartition).CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();

            if (maxRecords > 0)
            {
                TableQuery<AccountLogTableEntity> query = new TableQuery<AccountLogTableEntity>().Take(maxRecords).
                    Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, objectId));

                return cloudTable.ExecuteQuery(query);
            }
            else
            {
                return null;
            }

            
        }

        #endregion

        #endregion

    }
}
