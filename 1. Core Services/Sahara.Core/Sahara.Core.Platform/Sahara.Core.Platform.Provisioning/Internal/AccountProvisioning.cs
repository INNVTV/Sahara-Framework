using System;
using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Platform.Provisioning.Sql.Scripts.Initialization;
using Sahara.Core.Logging.PlatformLogs.Helpers;

namespace Sahara.Core.Platform.Provisioning.Internal
{
    internal static class AccountProvisioning
    {
        internal static DataAccessResponseType GenerateAccountSchema(string accountID, string databaseName)
        {
            DataAccessResponseType response = new DataAccessResponseType();
            response.isSuccess = false;

            var SqlInitialization = new Initialization();

            try
            {
                /*======================================
                      GENERATE SCHEMA & SEED DATA
                ========================================*/

                //Run Schema for Account:
                //FYI: .sql Files must be saved as ANSI
                //FYI: .sql Files must be set as "Embedded Resource" & "CopyAlways" in Properties
                //Sql.Statements.InitializationStatements.RunSqlScript("Account_Data_Create.sql", Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(databaseName).ConnectionString, Sahara.Core.Common.SchemaNames.GuidToSchemaName(accountID));
                SqlInitialization.InitializeAccountProvision(Sahara.Core.Common.Methods.SchemaNames.AccountIdToSchemaName(accountID), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(databaseName).ConnectionString);


                response.isSuccess = true;
            }
            catch(Exception e)
            {
                response.isSuccess = false;
                response.ErrorMessage = e.Message;

                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to provision account: " + accountID + " into database: " + databaseName,
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

            }
            

            return response;

        }


    }
}
