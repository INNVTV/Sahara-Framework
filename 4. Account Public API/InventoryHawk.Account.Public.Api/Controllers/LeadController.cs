using InventoryHawk.Account.Public.Api.Models.TableEntities.Leads;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    public class LeadController : Controller
    {
        #region  Fiddler Settings

        /*
        [POST]
        Content-Type: application/x-www-form-urlencoded

        RequestBody:
        firstName=Han&lastName=Solo&comments=Let's blow this thing and head home&companyName=The Smugglers Den
        */

        #endregion

        [Route("lead")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        [HttpPost]
        public bool Lead(string firstName = "", string lastName = "", string companyName = "", string phone = "", string email = "", string comments = "", string productName = "", string productId = "", string fullyQualifiedName = "", string locationPath = "", string origin = "", string ipAddress = "", string userId = "", string userName = "")
        {
            bool isSuccess = false;

            #region Disregard if all important inputs are null

            if(firstName == "" && lastName == "" && companyName == "" && phone == "" && email == "" && comments == "")
            {
                return false;
            }

            #endregion

            #region Scrub the data (Truncate EXCEPTIONALLY long strings)

            if (firstName.Length > 25)
            {
                firstName = firstName.Substring(0, 25);
            }
            if (lastName.Length > 25)
            {
                lastName = lastName.Substring(0, 25);
            }
            if (companyName.Length > 60)
            {
                companyName = companyName.Substring(0, 60);
            }
            if (comments.Length > 600)
            {
                comments = comments.Substring(0, 600);
            }
            if (phone.Length > 35)
            {
                phone = phone.Substring(0, 35);
            }
            if (email.Length > 60)
            {
                email = email.Substring(0, 60);
            }
            if (locationPath.Length > 300)
            {
                locationPath = locationPath.Substring(0, 300);
            }
            if (fullyQualifiedName.Length > 400)
            {
                fullyQualifiedName = fullyQualifiedName.Substring(0, 400);
            }
            if (origin.Length > 35)
            {
                origin = origin.Substring(0, 35);
            }
            if (ipAddress.Length > 120)
            {
                ipAddress = ipAddress.Substring(0, 120);
            }
            if (productName.Length > 240)
            {
                productName = productName.Substring(0, 240);
            }
            if (productId.Length > 120)
            {
                productId = productId.Substring(0, 120);
            }
            #endregion

            #region Get account information

            //Get the subdomain (if exists) for the api call
            string accountNameKey = Common.GetSubDomain(Request.Url);

            var account = Common.GetAccountObject(accountNameKey);

            if(account == null)
            {
                return false;
            }

            #endregion

            #region confirm account plan allows for leads and accout settings has leads ON

            if(account.PaymentPlan.AllowSalesLeads == false)
            {
                return false;
            }

            #endregion

            #region Generate Leads Table Name

            var newLeadsTableName = "acc" + account.AccountID.ToString().Replace("-", "").ToLower() + "LeadsNew";

            #endregion

            #region Connect to table storage (Legacy)

            /*
            CloudStorageAccount storageAccount;
            // Credentials are from centralized CoreServiceSettings
            StorageCredentials storageCredentials = new StorageCredentials(CoreServices.PlatformSettings.Storage.AccountName, CoreServices.PlatformSettings.Storage.AccountKey);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;
            */

            #endregion


            #region Connect to table storage partition

            #region Get Storage Partition

            if (CoreServices.PlatformSettings.StorageParitions == null || CoreServices.PlatformSettings.StorageParitions.ToList().Count == 0)
            {
                //No Storage Partitions Available in Static List, refresh list from Core Services
                Common.RefreshPlatformSettings();
            }

            //Get the storage partition for this account
            var storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);

            if (storagePartition == null)
            {
                //May be a new partition, refresh platform setting and try again
                Common.RefreshPlatformSettings();
                storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == account.StoragePartition);
            }

            #endregion

            CloudStorageAccount storageAccount;
            StorageCredentials storageCredentials = new StorageCredentials(storagePartition.Name, storagePartition.Key);
            storageAccount = new CloudStorageAccount(storageCredentials, false);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();

            //Create and set retry policy for logging
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 4);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


            #endregion



            #region Create TableEntity for Lead

            SalesLeadTableEntity salesLead = new SalesLeadTableEntity(cloudTableClient, newLeadsTableName);

            salesLead.FirstName = firstName;
            salesLead.LastName = lastName;
            salesLead.CompanyName = companyName;
            
            salesLead.Origin = origin;
            salesLead.Phone = phone;
            salesLead.Email = email;

            salesLead.Comments = comments;

            salesLead.ProductID = productId;
            salesLead.ProductName = productName;
            salesLead.IPAddress = ipAddress;
            salesLead.FullyQualifiedName = fullyQualifiedName;
            salesLead.LocationPath = locationPath;

            salesLead.UserID = userId;
            salesLead.UserName = userName;

            #endregion

            #region Insert Entity

            //create an insert operation for each entity, assign to designated CloudTable, and add to our list of tasks:
            TableOperation operation = TableOperation.Insert(salesLead);
            var tableResult = salesLead.cloudTable.Execute(operation);

            if(tableResult.HttpStatusCode == 204)
            {
                isSuccess = true;
            }

            #endregion

            #region Send email alerts (If ON) using BackgroundTask

            if(isSuccess)
            {
                BackgroundTasks.SalesAlerts.SendSalesAlerts(accountNameKey, firstName, lastName, companyName, phone, email, comments, productName, productId, fullyQualifiedName, locationPath, origin, ipAddress);               
            }

            #endregion

            return isSuccess;
        }

    }
}
