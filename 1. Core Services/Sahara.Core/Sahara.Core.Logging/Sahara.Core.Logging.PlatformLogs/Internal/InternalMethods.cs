using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Sahara.Core.Logging.PlatformLogs.TableEntities;
using Sahara.Core.Logging.PlatformLogs.Types;
using SendGrid;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using SendGrid.Helpers.Mail;

namespace Sahara.Core.Logging.PlatformLogs
{
    internal static class Internal
    {
        #region Write to Logs

        internal static void WritePlatformLog(CategoryType categoryType, ActivityType activityType, string description, string details = null, string accountId = null, string accountName = null, string userId = null, string userName = null, string userEmail = null, string ipAddress = null, string origin = null, string serializedObject = null)
        {
            CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            //Gather up all the entities into a list for our parallel task to execute in a ForEach
            List<Object> entityTypes = new List<object>();


            //Create an instance of each entity type and pass in associated CloudTableClient & TableName
            PlatformLogTableEntity_ByTime logTableEntity_Time = new PlatformLogTableEntity_ByTime(cloudTableClient, "platformactivitylogbytime");
            entityTypes.Add(logTableEntity_Time);

            PlatformLogTableEntity_ByCategory logTableEntity_Category = new PlatformLogTableEntity_ByCategory(cloudTableClient, "platformactivitylogbycategory");
            entityTypes.Add(logTableEntity_Category);

            PlatformLogTableEntity_ByActivity logTableEntity_Activity = new PlatformLogTableEntity_ByActivity(cloudTableClient, "platformactivitylogbyactivity");
            entityTypes.Add(logTableEntity_Activity);

            if (accountId != null)
            {
                PlatformLogTableEntity_ByAccountID logTableEntity_AccountID = new PlatformLogTableEntity_ByAccountID(cloudTableClient, "platformactivitylogbyaccountid");
                entityTypes.Add(logTableEntity_AccountID);
            }

            if (userId != null)
            {
                PlatformLogTableEntity_ByUserID logTableEntity_UserID = new PlatformLogTableEntity_ByUserID(cloudTableClient, "platformactivitylogbyuserid");
                entityTypes.Add(logTableEntity_UserID);
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
                    (obj as IPlatformLogTableEntity).Category = categoryType.ToString();
                    (obj as IPlatformLogTableEntity).Activity = activityType.ToString();
                    (obj as IPlatformLogTableEntity).Description = description.ToString();
                    (obj as IPlatformLogTableEntity).Details = details;

                    (obj as IPlatformLogTableEntity).UserID = userId;
                    (obj as IPlatformLogTableEntity).UserName = userName;
                    (obj as IPlatformLogTableEntity).UserEmail = userEmail;

                    (obj as IPlatformLogTableEntity).IPAddress = ipAddress;
                    (obj as IPlatformLogTableEntity).Origin = origin;

                    (obj as IPlatformLogTableEntity).AccountID = accountId;
                    (obj as IPlatformLogTableEntity).AccountName = accountName;

                    (obj as IPlatformLogTableEntity).Object = serializedObject;


                    //create an insert operation for each entity, assign to designated CloudTable, and add to our list of tasks:
                    TableOperation operation = TableOperation.Insert((obj as TableEntity));
                    (obj as IPlatformLogTableEntity).cloudTable.Execute(operation);
                });
            }
            catch (Exception e)
            {

                //Email platform admins about this exception
                #region Email Platform Admins About Exception

                try
                {
                    /*
                    * 
                    * Using SendGrid Library is not possible in this one case due to circular reference issues
                    * 
                    */

                    #region Create Parameter String

                    var parametersString = new StringBuilder();

                    parametersString.Append("(");

                    try
                    {
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

                        if (!String.IsNullOrEmpty(accountId))
                        {
                            parametersString.Append(accountId);
                            parametersString.Append(", ");
                        }
                        else
                        {
                            parametersString.Append("null");
                            parametersString.Append(", ");
                        }

                        if (!String.IsNullOrEmpty(accountName))
                        {
                            parametersString.Append(accountName);
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

                    dynamic sg = new SendGridAPIClient(Settings.Services.SendGrid.Account.APIKey);

                    Email from = new Email(Settings.Endpoints.Emails.FromExceptions, "Platform Exception");
                    string subject = "Exception Alert! (From: Platform Logger)";

                    var personalization = new Personalization();
                    foreach (var email in Settings.Endpoints.Emails.PlatformEmailAddresses)
                    {
                        Email to = new Email(email);
                        personalization.AddTo(to);
                    }

                    //Mail mail = new Mail(from, subject, to, content);
                    Mail mail = new Mail();
                    mail.From = from;
                    mail.Subject = subject;
                    mail.Personalization = new List<Personalization>();
                    mail.Personalization.Add(personalization);




                    //var myMessage = new SendGridMessage();

                    //myMessage.From = new MailAddress(Settings.Endpoints.Emails.FromExceptions, "Platform Exception");
                    //myMessage.AddTo(Settings.Endpoints.Emails.PlatformEmailAddresses);
                    //myMessage.Subject = "Exception Alert! (From: Platform Logger)";

                    //myMessage.Html =

                    var body =
                        "Exception location: <b>" + System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "</b>" +
                        "<br/><br/>" +
                        "Exception occurred while attempting to write to the platform log..." +
                        "<br/><br/><b>" +
                        e.Message +
                        "</b><br/><br/>" +
                        "Log Parameters:<br/><br/><b>" +
                        parametersString.ToString() +
                        "</b><br/><br/>" +
                        JsonConvert.SerializeObject(e) +
                        "<br/><br/>";

                    Content content = new Content("text/html", body);

                    //var username = Settings.Services.SendGrid.Account.UserName;
                    //var pswd = Settings.Services.SendGrid.Account.APIKey;
                    //var credentials = new NetworkCredential(username, pswd);

                    //var transportWeb = new Web(credentials);
                    //transportWeb.Deliver(myMessage);

                    mail.Contents = new List<Content>();
                    mail.Contents.Add(content);

                    var requestBody = mail.Get();

                    //dynamic response = await sg.client.mail.send.post(requestBody: requestBody);
                    dynamic response = sg.client.mail.send.post(requestBody: requestBody);
                    //dynamic d = response;
                }
                catch
                {

                }



                #endregion


            }

        }

        #endregion

        #region Get Logs

        #region Get Entire Platform Log By Time

        /// <summary>
        /// Gets ALL Categories and Activities for the Platform by time
        /// </summary>
        /// <param name="PlatformId"></param>
        /// <param name="maxRecords"></param>
        /// <returns></returns>
        internal static IEnumerable<PlatformLogTableEntity> GetPlatformLog(int maxRecords)
        {

            string _logFullName = "platformactivitylogbytime";

            CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;
            
            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();

            if (maxRecords > 0)
            {
                TableQuery<PlatformLogTableEntity> query = new TableQuery<PlatformLogTableEntity>().Take(maxRecords);

                return cloudTable.ExecuteQuery(query);
            }
            else
            {
                return null;
            }

        }

        #endregion

        #region Get an Entire Category by Time

        internal static IEnumerable<PlatformLogTableEntity> GetPlatformLogByCategory(CategoryType categoryType, int maxRecords)
        {

            string _logFullName = "platformactivitylogbycategory";

            CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;
            
            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();

            if (maxRecords > 0)
            {
                TableQuery<PlatformLogTableEntity> query = new TableQuery<PlatformLogTableEntity>().Take(maxRecords).
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

        internal static IEnumerable<PlatformLogTableEntity> GetPlatformLogByActivity(ActivityType activityType, int maxRecords)
        {

            string _logFullName = "platformactivitylogbyactivity";

            CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;
            
            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();

            if (maxRecords > 0)
            {
                TableQuery<PlatformLogTableEntity> query = new TableQuery<PlatformLogTableEntity>().Take(maxRecords).
                    Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, activityType.ToString()));

                return cloudTable.ExecuteQuery(query);
            }
            else
            {
                return null;
            }


        }

        #endregion

        #region Get an Entire Platform Users Log by Time

        internal static IEnumerable<PlatformLogTableEntity> GetPlatformLogByUser(string userId, int maxRecords)
        {

            string _logFullName = "platformactivitylogbyuserid";

            CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;
            
            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();

            if (maxRecords > 0)
            {
                TableQuery<PlatformLogTableEntity> query = new TableQuery<PlatformLogTableEntity>().Take(maxRecords).
                    Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

                return cloudTable.ExecuteQuery(query);
            }
            else
            {
                return null;
            }


        }

        #endregion

        #region Get an Entire Account Log by Time

        internal static IEnumerable<PlatformLogTableEntity> GetPlatformLogByAccount(string accountId, int maxRecords)
        {

            string _logFullName = "platformactivitylogbyaccountid";

            CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();

            if (maxRecords > 0)
            {
                TableQuery<PlatformLogTableEntity> query = new TableQuery<PlatformLogTableEntity>().Take(maxRecords).
                    Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, accountId));

                return cloudTable.ExecuteQuery(query);
            }
            else
            {
                return null;
            }


        }

        #endregion

        #endregion

        #region Clear Logs
        /*
        internal static void ClearLogs()
        {
            CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            IEnumerable<CloudTable> tables = cloudTableClient.ListTables();

            foreach (CloudTable table in tables)
            {
                try
                {
                    table.Delete();
                }
                catch(Exception e)
                {
                    //response.isSuccess = false;

                    PlatformLogManager.LogActivity(
                        CategoryType.Error,
                        ActivityType.Error_Exception,
                        "Table(s) Deletion Failed for platform during clear command",
                        "Please delete all tables for platform manually",
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString(),
                        JsonConvert.SerializeObject(e, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                    );
                }

            }

            //return response;
        }
        */
        #endregion

    }
}
