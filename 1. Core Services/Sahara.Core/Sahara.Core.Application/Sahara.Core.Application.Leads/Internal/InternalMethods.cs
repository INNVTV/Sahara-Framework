using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Sahara.Core.Application.Leads.Models;
using Sahara.Core.Application.Leads.TableEntities;
using Sahara.Core.Common.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Leads
{
    /*Account Admin now deals with SALES LEADS DIRECTLY

    internal static class Internal
    {

        
        #region Get

        internal static IEnumerable<SalesLeadTableEntity> GetSalesLeads(string accountId, string label, int skip, int take)
        {

            string _logFullName = Common.Methods.SchemaNames.AccountIdToTableStorageName(accountId) + "Leads" + label.Replace("-", "").Replace(" ", "");

            CloudTableClient cloudTableClient = Settings.Azure.Storage.StorageConnections.AccountsStorage.CreateCloudTableClient();

            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;


            CloudTable cloudTable = cloudTableClient.GetTableReference(_logFullName);

            cloudTable.CreateIfNotExists();


             TableQuery<SalesLeadTableEntity> query = new TableQuery<SalesLeadTableEntity>().Take(take);
             
             return cloudTable.ExecuteQuery(query);

        }

        #endregion

        #region Update

        internal static DataAccessResponseType UpdateSalesLead(string accountId, SalesLeadTableEntity salesLeadTableEntity)
        {
            return new DataAccessResponseType();
        }

        #endregion

        #region Move

        internal static DataAccessResponseType MoveSalesLead(string accountId, string partitionKey, string rowKey, string labelFrom, string labelTo)
        {
            return new DataAccessResponseType();
        }

        #endregion
        
        
    }
    
    */
}
