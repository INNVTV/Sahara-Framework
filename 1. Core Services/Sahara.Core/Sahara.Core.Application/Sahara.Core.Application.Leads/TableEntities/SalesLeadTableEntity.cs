using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Leads.TableEntities
{
    /* Account Admin now deals with SALES LEADS DIRECTLY 

    public class SalesLeadTableEntity : TableEntity
    {
        public SalesLeadTableEntity()
        {

        }

        //public SalesLeadTableEntity(CloudTableClient cloudTableClient, string tableName)
        //{
        //PartitionKey = string.Format("{0:d19}+{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));

        ////Create and set retry policy
        //IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
        //cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

        ////Create the cloudtable instance and  name for the entity operate against:
        //cloudTable = cloudTableClient.GetTableReference(tableName);
        //cloudTable.CreateIfNotExists();
        //}

        public string LeadID
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        

        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string FullyQualifiedName { get; set; }
        public string LocationPath { get; set; }

        public string Comments { get; set; }
        public string Notes { get; set; }

        public string IPAddress { get; set; } //<-- Add if you intend to track ip/location of caller
        public string Origin { get; set; }

        public string Object { get; set; } //<-- Available to use to store small json/.net objects

        //public CloudTable cloudTable { get; set; }
    }

    */
}
