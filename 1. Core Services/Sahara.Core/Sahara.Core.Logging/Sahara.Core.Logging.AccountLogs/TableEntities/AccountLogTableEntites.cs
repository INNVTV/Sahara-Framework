using System;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Sahara.Core.Logging.AccountLogs.TableEntities
{

    #region Interface

    /// <summary>
    /// Interface that allows for greater brevity in our methods, specifically the Parallel.For loop for writing to Azure Storage.
    /// </summary>
    internal interface IAccountLogTableEntity
    {

        string Category { get; set; }
        string Activity { get; set; }

        string Description { get; set; } //<-- User friendly description
        string Details { get; set; } //<-- Additional details, associated id's etc...

        string UserID { get; set; }
        string UserEmail { get; set; }
        string UserName { get; set; }

        string ObjectID { get; set; }
        string ObjectName { get; set; }

        string IPAddress { get; set; } //<-- Add if you intend to track ip/location of caller
        string Origin { get; set; }

        string Object { get; set; }

        CloudTable cloudTable { get; set; }

    }

    #endregion

    #region Base Class

    /// <summary>
    /// Base class for all LogTableEntity Types
    /// </summary>
    abstract class AccountLogTableEntity_Base : TableEntity, IAccountLogTableEntity
    {
        public AccountLogTableEntity_Base(CloudTableClient cloudTableClient, string tableName)
        {
            //Create and set retry policy
            //IRetryPolicy exponentialRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 4);
            IRetryPolicy linearRetryPolicy = new LinearRetry(TimeSpan.FromSeconds(1), 3);
            cloudTableClient.DefaultRequestOptions.RetryPolicy = linearRetryPolicy;

            //Create the cloudtable instance and  name for the entity operate against:
            cloudTable = cloudTableClient.GetTableReference(tableName);
            cloudTable.CreateIfNotExists();
        }

        // Abstract properties (properties that are used for partition keys on LogTableEntity_ types)
        public abstract string Category { get; set; }
        public abstract string Activity { get; set; }
        public abstract string UserID { get; set; }
        public abstract string ObjectID { get; set; }

        // Base Properties
        public string Description { get; set; }
        public string Details { get; set; }   
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string ObjectName { get; set; }
        public string IPAddress { get; set; } //<-- Add if you intend to track ip/location of caller
        public string Origin { get; set; }

        public string Object { get; set; }

        public CloudTable cloudTable { get; set; }
    }

    #endregion

    #region Table Entities

    internal class AccountLogTableEntity_ByTime : AccountLogTableEntity_Base
    {
        public AccountLogTableEntity_ByTime(CloudTableClient cloudTableClient, string tableName)
            : base(cloudTableClient, tableName)
        {
            PartitionKey = string.Format("{0:d19}+{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
        }

        public override string Category
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public override string Activity { get; set; }
        public override string UserID { get; set; }
        public override string ObjectID { get; set; }

    }

    internal class AccountLogTableEntity_ByCategory : AccountLogTableEntity_Base
    {
        public AccountLogTableEntity_ByCategory(CloudTableClient cloudTableClient, string tableName)
            : base(cloudTableClient, tableName)
        {
            RowKey = string.Format("{0:d19}+{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
        }

        public override string Category
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public override string Activity { get; set; }
        public override string UserID { get; set; }
        public override string ObjectID { get; set; }

    }

    internal class AccountLogTableEntity_ByActivity : AccountLogTableEntity_Base
    {
        public AccountLogTableEntity_ByActivity(CloudTableClient cloudTableClient, string tableName)
            : base(cloudTableClient, tableName)
        {
            RowKey = string.Format("{0:d19}+{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
        }

        public override string Activity
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public override string Category { get; set; }
        public override string UserID { get; set; }
        public override string ObjectID { get; set; }

    }

    internal class AccountLogTableEntity_ByUserID : AccountLogTableEntity_Base
    {
        public AccountLogTableEntity_ByUserID(CloudTableClient cloudTableClient, string tableName)
            : base(cloudTableClient, tableName)
        {
            RowKey = string.Format("{0:d19}+{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
        }

        public override string UserID
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public override string Category { get; set; }
        public override string Activity { get; set; }
        public override string ObjectID { get; set; }
    }

    internal class AccountLogTableEntity_ByObjectID : AccountLogTableEntity_Base
    {
        public AccountLogTableEntity_ByObjectID(CloudTableClient cloudTableClient, string tableName)
            : base(cloudTableClient, tableName)
        {
            RowKey = string.Format("{0:d19}+{1}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks, Guid.NewGuid().ToString("N"));
        }

        public override string ObjectID
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        public override string Category { get; set; }
        public override string Activity { get; set; }
        public override string UserID { get; set; }

    }


    #endregion
   
}
