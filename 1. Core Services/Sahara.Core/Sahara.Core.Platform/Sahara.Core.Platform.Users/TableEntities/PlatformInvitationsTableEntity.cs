using System;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Platform.Users.Internal;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Platform.Users.TableEntities
{
    public class PlatformInvitationsTableEntity : TableEntity
    {
        public PlatformInvitationsTableEntity()
        {
            //Generate Invitation Key
            InvitationKey = Guid.NewGuid().ToString();

            //Create the cloudtable instance and  name for the entity operate against:
            var cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            cloudTable = cloudTableClient.GetTableReference(PlatformInvitationsManager.PlatformInvitationsTableName);
            cloudTable.CreateIfNotExists();

            //Ordered by Newest at top
            RowKey = string.Format("{0:d19}+{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
        }

        public string InvitationKey
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }

        public CloudTable cloudTable { get; set; }

    }
}
