using System;
using System.Data.SqlClient;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Platform.Deprovisioning.Sql.Statements
{
    internal static class StoredProcedures
    {
        internal static DataAccessResponseType DestroySchema(string schemaName, string databasePartitionName)
       {
           var response = new DataAccessResponseType();

            //SqlCommand sqlCommand = new SqlCommand("DestroySchema", Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(databasePartitionName));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(databasePartitionName).CreateCommand();
		    sqlCommand.CommandText = "DestroySchema";
		    

           try
           {
               sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;

               sqlCommand.Parameters.Add(new SqlParameter("@SchemaName", schemaName));
               sqlCommand.Parameters.Add(new SqlParameter("@WorkTest", 'w'));

               sqlCommand.Connection.OpenWithRetry();

               sqlCommand.ExecuteNonQueryWithRetry();

               sqlCommand.Connection.Close();

               response.isSuccess = true;
               response.SuccessMessage = "Schema '" + schemaName + "', and all associated object have been destroyed on '" + databasePartitionName + "'";
           }
           catch(Exception e)
           {
               //Log exception and email platform admins
               PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                   e,
                   "attempting to destroy for: " + schemaName,
                   System.Reflection.MethodBase.GetCurrentMethod()
               );

               PlatformLogManager.LogActivity(
                        CategoryType.ManualTask,
                        ActivityType.ManualTask_SQL,
                        "Stored Procedure 'DestroySchema' Failed on the '" + databasePartitionName + "' partition for schema '" + schemaName + "'",
                        sqlCommand.ToString(),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        System.Reflection.MethodBase.GetCurrentMethod().ToString()
                    );

               response.isSuccess = false;
               response.ErrorMessage = e.Message;
           }

           return response;
       }
    }
}
