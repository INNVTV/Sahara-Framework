using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace InventoryHawk.Account.Public.Api.BackgroundTasks
{
    public static class SalesAlerts
    {

        public static void SendSalesAlerts(string accountNameKey, string firstName = "", string lastName = "", string companyName = "", string phone = "", string email = "", string comments = "", string productName = "", string productId = "", string fullyQualifiedName = "", string locationPath = "", string origin = "", string ipAddress = "")
        {
            try
            {
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

                ThreadPool.QueueUserWorkItem(o =>

                    _sendSalesAlerts(accountNameKey, firstName, lastName, companyName, phone, email, comments, productName, productId, fullyQualifiedName, locationPath, origin, ipAddress)
            
            );

                #endregion
            }
            catch
            {

            }
            
        }

        internal static void _sendSalesAlerts(string accountNameKey, string firstName = "", string lastName = "", string companyName = "", string phone = "", string email = "", string comments = "", string productName = "", string productId = "", string fullyQualifiedName = "", string locationPath = "", string origin = "", string ipAddress = "")
        {
            try
            {
                #region Get Account Settings

                var accountSettings = DataAccess.AccountSettings.GetAccountSettings(accountNameKey);

                #endregion

                if (accountSettings.SalesSettings.UseSalesAlerts && accountSettings.SalesSettings.AlertEmails.Length > 0)
                {
                    #region Generate Email Alert Message & Subject (Based on parameters)

                    StringBuilder subject = new StringBuilder();
                    StringBuilder body = new StringBuilder();

                    subject.Append("New sales lead");

                    #region Names

                    if (firstName != "" && lastName != "")
                    {
                        //Subject
                        subject.Append(" from ");
                        subject.Append(firstName);
                        subject.Append(" ");
                        subject.Append(lastName);

                        //Body
                        body.Append("<b>From: </b>");
                        body.Append(firstName);
                        body.Append(" ");
                        body.Append(lastName);
                        body.Append("<br>");
                        body.Append("<br>");

                    }
                    else if (firstName != "")
                    {
                        //Subject
                        subject.Append(" from ");
                        subject.Append(firstName);

                        //Body
                        body.Append("<b>From: </b>");
                        body.Append(firstName);
                        body.Append("<br>");
                        body.Append("<br>");

                    }
                    else if (lastName != "")
                    {
                        //Subject
                        subject.Append(" from ");
                        subject.Append(lastName);

                        //Body
                        body.Append("<b>From: </b>");
                        body.Append(lastName);
                        body.Append("<br>");
                        body.Append("<br>");
                    }

                    #endregion

                    #region CompanyName

                    if (companyName != "")
                    {
                        //Body
                        body.Append("<b>Company: </b>");
                        body.Append(companyName);
                        body.Append("<br>");
                        body.Append("<br>");
                    }

                    #endregion


                    #region Product

                    if (productName != "")
                    {
                        //Subject
                        subject.Append(" for ");
                        subject.Append(productName);
                        body.Append("<br>");
                        body.Append("<br>");
                    }

                    if (productName != "" && fullyQualifiedName != "")
                    {
                        //Body
                        body.Append("<b>Product: </b>");
                        body.Append("<a href='http://");
                        body.Append(accountNameKey);
                        body.Append(".");
                        body.Append(CoreServices.PlatformSettings.Urls.AccountManagementDomain);
                        body.Append("/product/");
                        body.Append(fullyQualifiedName);
                        body.Append("'>");
                        body.Append(productName);
                        body.Append("</a>");
                        body.Append("<br>");
                        body.Append("<br>");
                    }
                    else if (productName != "")
                    {
                        //Body
                        body.Append("<b>Product: </b>");
                        body.Append(productName);
                        body.Append("<br>");
                        body.Append("<br>");
                    }

                    #endregion

                    #region Comments

                    else if (comments != "")
                    {
                        //Body
                        body.Append("<b>Comments: </b>");
                        body.Append(comments);
                        body.Append("<br>");
                        body.Append("<br>");
                    }

                    #endregion

                    #region Origin

                    else if (origin != "")
                    {
                        //Body
                        body.Append("<b>Origin: </b>");
                        body.Append(origin);
                        body.Append("<br>");
                        body.Append("<br>");
                    }

                    #endregion

                    #region IP

                    else if (ipAddress != "")
                    {
                        //Body
                        body.Append("<b>IP Address: </b>");
                        body.Append(ipAddress);
                        body.Append("<br>");
                        body.Append("<br>");
                    }

                    #endregion

                    #endregion

                    #region Send Email Alerts (v3 SendGrid API)

                    dynamic sg = new SendGridAPIClient(CoreServices.PlatformSettings.SendGrid.ApiKey);

                    Email from = new Email("alerts@inventoryhawk.com", "Sales Alert");

                    var personalization = new Personalization();
                    foreach (var emailAddress in accountSettings.SalesSettings.AlertEmails)
                    {
                        Email to = new Email(emailAddress);
                        personalization.AddTo(to);
                    }

                    //Mail mail = new Mail(from, subject, to, content);
                    Mail mail = new Mail();
                    mail.From = from;
                    mail.Subject = subject.ToString();
                    mail.Personalization = new List<Personalization>();
                    mail.Personalization.Add(personalization);

                    Content content = new Content("text/html", body.ToString());
                    mail.Contents = new List<Content>();
                    mail.Contents.Add(content);
                    

                    // Send the email.
                    try
                    {
                         var requestBody = mail.Get();

                         //dynamic response = await sg.client.mail.send.post(requestBody: requestBody);
                         dynamic response = sg.client.mail.send.post(requestBody: requestBody);
                         //dynamic d = response;
                        
                    }
                    catch (Exception e)
                    {
                        //Log the Exception ??
                        

                    }

                    #endregion

                    #region Send Email Alert(s) [Legacy]

                    /*

                    var myMessage = new SendGridMessage(); //.SendGrid.GetInstance();


                    // Add the message properties.
                    myMessage.From = new MailAddress("alerts@inventoryhawk.com", "Sales Alert");
                    //myMessage.Header.AddTo(recipients);
                    myMessage.AddTo(accountSettings.SalesSettings.AlertEmails);
                    myMessage.Subject = subject.ToString();
                    myMessage.Html = body.ToString();


                    // Create network credentials to access your SendGrid account.
                    var username = CoreServices.PlatformSettings.SendGrid.UserName;
                    var pswd = CoreServices.PlatformSettings.SendGrid.ApiKey;
                    var credentials = new NetworkCredential(username, pswd);

                    // Create a Web transport for sending email.
                    //var transportWeb = SendGridMail.Transport.Web.GetInstance(credentials);
                    //var sendGridMailWeb = SendGridMail.Web.GetInstance(credentials);
                    var transportWeb = new Web(credentials);

                    // Send the email.
                    try
                    {                       
                        //transportWeb.Deliver(myMessage);
                        //sendGridMailWeb.Deliver(myMessage);
                        transportWeb.Deliver(myMessage);                        
                    }
                    catch
                    {

                    }

                    */
                    #endregion
                }


            }
            catch (Exception e)
            {

            }
        }



    }
}