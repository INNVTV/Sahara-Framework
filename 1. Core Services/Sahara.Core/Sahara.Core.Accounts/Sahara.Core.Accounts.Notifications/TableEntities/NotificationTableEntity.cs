using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sahara.Core.Accounts.Notifications.Models;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Accounts.Notifications.TableEntities
{
    public class NotificationTableEntity : TableEntity
    {

        //NOTIFICATIONS OFF BY DEFAULT. UNCOMMENT TO TURN ON (Requires USERS table storage account)

        /*

        public NotificationTableEntity()
        {
        }

        public NotificationTableEntity(string userId, string notificationType)
        {

            PartitionKey = NotificationStatus.Unread.ToString();
            //Expired = false;
            ExpirationMinutes = 0;

            CreatedDateTime = DateTime.UtcNow;

            //Create the cloudtable instance and  name for the entity operate against:
            var cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.UsersStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            cloudTable = cloudTableClient.GetTableReference(Sahara.Core.Common.Methods.SchemaNames.UserIdToTableStorageName(userId) + "Notifications" + notificationType);
            cloudTable.CreateIfNotExists();

            //Ordered by Newest at top
            RowKey = string.Format("{0:d19}-{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
            
        }

        public string Status
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string Message { get; set; }
        //public bool Expired { get; set; }

        public DateTime ExpirationDateTime { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public double ExpirationMinutes { get; set; } 

        public CloudTable cloudTable { get; set; }
        
        */
    }
}
