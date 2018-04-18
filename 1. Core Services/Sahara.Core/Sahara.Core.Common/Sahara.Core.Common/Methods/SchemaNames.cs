using System;

namespace Sahara.Core.Common.Methods
{
    public static class SchemaNames
    {

        ///Removed in order to use accountnamekey 
        /// <summary>
        /// Convert an AccountID into it's designated table name
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public static string AccountIdToTableStorageName(string accountId)
        {
            return "acc" + accountId.Replace("-", "").ToLower(); //<-- TS requires alpha numberic first characters
        }


        /// <summary>
        /// Convert a UserID into it's designated table name
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string UserIdToTableStorageName(string userId)
        {
            return "usr" + userId.Replace("-", "").ToLower(); //<-- TS requires alpha numberic first characters
        }


        public static string AccountIdToSchemaName(string accountId)
        {
            return Settings.Platform.Partitioning.AccountPartitionSchemaDesignation + accountId.Replace("-", "").ToLower();
        }

        /// <summary>
        /// Convert a schema name back into the AccountID it represents
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        ///
        //public static Guid FromAccountSchemaName(string schemaName)
        //{
            //string guid = schemaName.Substring(Settings.Platform.Partitioning.AccountPartitionSchemaDesignation.Length, schemaName.Length - 1);
            //return new Guid(guid);
        //}

    }

}
