using System.Data.SqlClient;
using Sahara.Core.Infrastructure.Azure.Models.Database;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Platform.Partitioning.Sql.Scripts.Create
{
    internal static class Create
    {
        
        public static bool CreateDatabase(DatabaseModel database)
        {
            bool response = false;

            ///SqlCommand sqlCommand;

            //Create Database =============================================================
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection.CreateCommand();
            sqlCommand.CommandText = "Create Database " + database.DatabaseName + "(EDITION='" + database.DatabaseTier + "', MAXSIZE=" + database.DatabaseSize + ")";

            sqlCommand.CommandTimeout = 90; //<-- We increase timeout to 90 seconds on this important procedure

            sqlCommand.Connection.OpenWithRetry();
            sqlCommand.ExecuteNonQueryWithRetry();
            sqlCommand.Connection.Close();

            response = true;

            return response;
        }

        /*
        private static void executeNonQueryStatement(string statement, SqlConnection sqlConnection)
        {
            if (statement != "")
            {

                SqlCommand sqlCommandGenerateTables = new SqlCommand(statement, sqlConnection);

                try
                {

                    sqlCommandGenerateTables.Connection.Open();
                    sqlCommandGenerateTables.ExecuteNonQuery();
                    sqlCommandGenerateTables.Connection.Close();
                }
                catch
                {
                    //Try again (ADO.NET Connection Pooling may require a retry)
                    sqlCommandGenerateTables.Connection.Close();

                    sqlCommandGenerateTables.Connection.Open();
                    sqlCommandGenerateTables.ExecuteNonQuery();
                    sqlCommandGenerateTables.Connection.Close();
                }
            }
       
        } */
         
    }
}
