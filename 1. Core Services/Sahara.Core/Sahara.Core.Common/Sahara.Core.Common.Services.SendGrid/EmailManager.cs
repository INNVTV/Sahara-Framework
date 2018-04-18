using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using SendGrid;
using Sahara.Core.Common.ResponseTypes;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace Sahara.Core.Common.Services.SendGrid
{
    public static class EmailManager
    {

        public static void Send(List<string> recipients, string fromEmail, string fromName, string subject, string body, bool isHtml, bool isImportant = false)
        {

            dynamic sg = new SendGridAPIClient(Settings.Services.SendGrid.Account.APIKey);

            Email from = new Email(fromEmail, fromName);

            var personalization = new Personalization();
            foreach (var email in recipients)
            {
                if(email.ToLower() == "platformadmin@[Config_PlatformEmail]")
                {
                    //on recipients with LISTS we always BCC platformAdmin in order to keep that email hidden from the account users
                    Email bcc = new Email(email);
                    personalization.Bccs = new List<Email>();
                    personalization.Bccs.Add(bcc);
                }
                else
                {
                    Email to = new Email(email);
                    personalization.AddTo(to);
                }
            }


            //Mail mail = new Mail(from, subject, to, content);
            Mail mail = new Mail();
            mail.From = from;
            mail.Subject = subject;
            mail.Personalization = new List<Personalization>();
            mail.Personalization.Add(personalization);

            if (isHtml)
            {
                Content content = new Content("text/html", body);
                mail.Contents = new List<Content>();
                mail.Contents.Add(content);
            }
            else
            {
                Content content = new Content("text/plain", body);
                mail.Contents = new List<Content>();
                mail.Contents.Add(content);
            }






            // Send the email.
            try
            {
                if (Settings.Services.SendGrid.Account.Active) //<-- We check to see if email service is inactive due to load testing to avoid extra service charges
                {
                    var requestBody = mail.Get();

                    //dynamic response = await sg.client.mail.send.post(requestBody: requestBody);
                    dynamic response = sg.client.mail.send.post(requestBody: requestBody);
                    //dynamic d = response;
                }
            }
            catch (Exception e)
            {
                //Log the Exception
                PlatformLogManager.LogActivity(
                    CategoryType.Error,
                    ActivityType.Error_SendGrid,
                    e.Message,
                    "An error occured when attempting to use SendGrid"
                    );

                //Log the Exception
                PlatformLogManager.LogActivity(
                        CategoryType.Error,
                        ActivityType.Error_Exception,
                        e.Message,
                        "An exception occured when attempting to use SendGrid",
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name,
                        JsonConvert.SerializeObject(e)
                    );

            }

        }

        public static void Send(string recipient, string fromEmail, string fromName, string subject, string body, bool isHtml, bool isImportant = false)
        {
            dynamic sg = new SendGridAPIClient(Settings.Services.SendGrid.Account.APIKey);

            Email from = new Email(fromEmail, fromName);

            var personalization = new Personalization();
            Email to = new Email(recipient);
            personalization.AddTo(to);

            //Mail mail = new Mail(from, subject, to, content);
            Mail mail = new Mail();
            mail.From = from;
            mail.Subject = subject;
            mail.Personalization = new List<Personalization>();
            mail.Personalization.Add(personalization);

            if (isHtml)
            {
                Content content = new Content("text/html", body);
                mail.Contents = new List<Content>();
                mail.Contents.Add(content);
            }
            else
            {
                Content content = new Content("text/plain", body);
                mail.Contents = new List<Content>();
                mail.Contents.Add(content);
            }






            // Send the email.
            try
            {
                if (Settings.Services.SendGrid.Account.Active) //<-- We check to see if email service is inactive due to load testing to avoid extra service charges
                {
                    var requestBody = mail.Get();

                    //dynamic response = await sg.client.mail.send.post(requestBody: requestBody);
                    dynamic response = sg.client.mail.send.post(requestBody: requestBody);
                    //dynamic d = response;
                }
            }
            catch (Exception e)
            {
                //Log the Exception
                PlatformLogManager.LogActivity(
                    CategoryType.Error,
                    ActivityType.Error_SendGrid,
                    e.Message,
                    "An error occured when attempting to use SendGrid"
                    );

                //Log the Exception
                PlatformLogManager.LogActivity(
                        CategoryType.Error,
                        ActivityType.Error_Exception,
                        e.Message,
                        "An exception occured when attempting to use SendGrid",
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name,
                        JsonConvert.SerializeObject(e)
                    );

            }
        }

    }
}
