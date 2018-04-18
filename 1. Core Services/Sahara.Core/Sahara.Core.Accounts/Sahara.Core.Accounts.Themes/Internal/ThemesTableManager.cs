using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Accounts.Themes.TableEntities;
using Sahara.Core.Common.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.Themes.Internal
{
    internal static class ThemesTableManager
    {

        internal static DataAccessResponseType StoreTheme(ThemeTableEntity themeTableEntity)
        {
            var response = new DataAccessResponseType();

            try
            {

                TableOperation operation = TableOperation.Insert((themeTableEntity as TableEntity));
                themeTableEntity.cloudTable.Execute(operation);

                response.isSuccess = true;

                return response;
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                /*
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to store notifications into table storage for: " + userId,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );
                */

                response.isSuccess = true;
                response.ErrorMessage = e.Message;

                return response;
            }

        }

        internal static IEnumerable<ThemeTableEntity> GetThemes()
        {
            CloudTableClient cloudTableClient = Sahara.Core.Settings.Azure.Storage.StorageConnections.PlatformStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            CloudTable cloudTable = cloudTableClient.GetTableReference("themes");

            cloudTable.CreateIfNotExists();

            TableQuery<ThemeTableEntity> query = new TableQuery<ThemeTableEntity>();

            var themeEntities = cloudTable.ExecuteQuery(query);

            return themeEntities;

        }

    }
}
