using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Accounts.Themes.TableEntities
{
    public class ThemeTableEntity : TableEntity
    {
        public ThemeTableEntity()
        {
        }

        public ThemeTableEntity(string themeName)
        {

            RowKey = themeName;
            PartitionKey = Common.Methods.ObjectNames.ConvertToObjectNameKey(themeName);

            //Create the cloudtable instance and  name for the entity operate against:
            var cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            cloudTable = cloudTableClient.GetTableReference("themes");
            cloudTable.CreateIfNotExists();   
        }

        public string ThemeNameKey
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public string ThemeName
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string ColorBackground { get; set; }
        public string ColorBackgroundGradientTop { get; set; }
        public string ColorBackgroundGradientBottom { get; set; }
        public string ColorShadow { get; set; }
        public string ColorHighlight { get; set; }
        public string ColorForeground { get; set; }
        public string ColorOverlay { get; set; }
        public string ColorTrim { get; set; }

        public string Font { get; set; }

        public CloudTable cloudTable { get; set; }

    }
}
