using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace InventoryHawk.Account.Public.Api.Controllers
{
    public class InfrastructureController : Controller
    {
        [Route("infrastructure/RefreshPlatformSettings")]
        public ActionResult RefreshPlatformSettings()
        {

            #region Communicate with CoreServices and get updated subset of static settings for this client to work with

            var platformSettingsServiceClient = new PlatformSettingsService.PlatformSettingsServiceClient(); // <-- We only use PlatformSettingsServiceClient in EnviornmentSettings because it is ONLY used at application startup:

            try
            {

                platformSettingsServiceClient.Open();
                var platformSettingsResult = platformSettingsServiceClient.GetCorePlatformSettings(Common.SharedClientKey);

                CoreServices.PlatformSettings = platformSettingsResult;

                //Close the connections
                WCFManager.CloseConnection(platformSettingsServiceClient);

                return Content("<b style='color:darkgreen'>Settings refreshed!</b>");
            }
            catch (Exception e)
            {
                #region Manage Exception

                string exceptionMessage = e.Message.ToString();

                var currentMethod = System.Reflection.MethodBase.GetCurrentMethod();
                string currentMethodString = currentMethod.DeclaringType.FullName + "." + currentMethod.Name;

                // Abort the connections & manage the exceptions
                WCFManager.CloseConnection(platformSettingsServiceClient, exceptionMessage, currentMethodString);

                #endregion

                platformSettingsServiceClient.Close();

                return Content("<b style='color:red'>" + e.ToString() + "</b>");

            }

            #endregion


        }

        [Route("infrastructure/StaticPartitionsList")]
        public ActionResult StaticPartitionsList()
        {
            StringBuilder str = new StringBuilder();

            str.Append("<b><u>Search Partitions</u></b><br/>");

            foreach(var partition in CoreServices.PlatformSettings.SearchParitions)
            {
                str.Append(partition.Name + "<br/>");
            }

            str.Append("<br/><br/><b><u>Storage Partitions</u></b><br>");

            foreach (var partition in CoreServices.PlatformSettings.StorageParitions)
            {
                str.Append(partition.Name + "<br/>");
            }

            str.Append("<br/><br/>(Static)");

            return Content(str.ToString());

        }

        [Route("infrastructure/StaticPartition/{partitionType}/{partitionName}")]
        public ActionResult StaticPartition(string partitionType, string partitionName)
        {
            bool staticallyAvailable = true;

            switch (partitionType.ToLower())
            {
                case "search":

                    #region Get Search Partition

                    if (CoreServices.PlatformSettings.SearchParitions == null || CoreServices.PlatformSettings.SearchParitions.ToList().Count == 0)
                    {
                        //No Search Partitions Available in Static List, refresh list from Core Services
                        Common.RefreshPlatformSettings();
                        staticallyAvailable = false;
                    }

                    PlatformSettingsService.SearchPartition searchPartition = null;

                    searchPartition = CoreServices.PlatformSettings.SearchParitions.FirstOrDefault(partition => partition.Name == partitionName);

                    

                    if (searchPartition == null)
                    {
                        //May be a new partition, refresh platform setting and try again
                        Common.RefreshPlatformSettings();
                        searchPartition = CoreServices.PlatformSettings.SearchParitions.FirstOrDefault(partition => partition.Name == partitionName);
                        staticallyAvailable = false;
                    }

                    if(searchPartition != null)
                    {
                        return Content(searchPartition.Name + " | Plan: '" + searchPartition.Plan + "' | TenantCount:" + searchPartition.TenantCount + " | MaxTenants:" + searchPartition.MaxTenants + "<br/><br/>Statically Available: " + staticallyAvailable);
                    }
                    else
                    {
                        return Content("null");
                    }
                    

                #endregion


                case "storage":

                    #region Get Storage Partition

                    if (CoreServices.PlatformSettings.StorageParitions == null || CoreServices.PlatformSettings.StorageParitions.ToList().Count == 0)
                    {
                        //No StorageParitions Partitions Available in Static List, refresh list from Core Services
                        Common.RefreshPlatformSettings();
                        staticallyAvailable = false;
                    }

                    PlatformSettingsService.StoragePartition storagePartition = null;

                    storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == partitionName);


                    if (storagePartition == null)
                    {
                        //May be a new partition, refresh platform setting and try again
                        Common.RefreshPlatformSettings();
                        storagePartition = CoreServices.PlatformSettings.StorageParitions.FirstOrDefault(partition => partition.Name == partitionName);
                        staticallyAvailable = false;
                    }

                    if (storagePartition != null)
                    {
                        return Content(storagePartition.Name + " | TenantCount:" + storagePartition.TenantCount + " | MaxTenants:" + storagePartition.MaxTenants + "<br/><br/>Statically Available: " + staticallyAvailable);
                    }
                    else
                    {
                        return Content("null");
                    }

                   

                #endregion

                default:
                    return null;
            }

        }

        [Route("infrastructure/StaticPartitionsList/Refresh")]
        public ActionResult StaticPartitionsListWithCheck()
        {
            Common.RefreshPlatformSettings();

            StringBuilder str = new StringBuilder();

            str.Append("<b><u>Search Partitions</u></b><br/>");

            foreach (var partition in CoreServices.PlatformSettings.SearchParitions)
            {
                str.Append(partition.Name + "<br/>");
            }

            str.Append("<br/><br/><b><u>Search Partitions</u></b><br>");

            foreach (var partition in CoreServices.PlatformSettings.StorageParitions)
            {
                str.Append(partition.Name + "<br/>");
            }

            str.Append("<br/><br/>(After refresh)");

            return Content(str.ToString());

        }
    }
}