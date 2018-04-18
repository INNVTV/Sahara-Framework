using System.Data.SqlClient;
using Sahara.Core.Infrastructure.Azure.Models.Database;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;

namespace Sahara.Core.Platform.Initialization.Sql.Scripts
{
    internal static class Create
    {

        public static bool CreateDatabase(DatabaseModel database, ReliableSqlConnection sqlConnection)
        {
            bool response = false;

            SqlCommand sqlCommand = new SqlCommand();

            //Drop databases (if exists)
            try
            {
                //sqlCommand = new SqlCommand("Drop Database " + database.DatabaseName, sqlConnection);
                sqlCommand = sqlConnection.CreateCommand();
			    sqlCommand.CommandText = "Drop Database " + database.DatabaseName;


                sqlCommand.Connection.OpenWithRetry();
                sqlCommand.ExecuteNonQueryWithRetry();
                sqlCommand.Connection.Close();
            }
            catch
            {
                sqlCommand.Connection.Close();
            }

            try
            {
                //Create Database =============================================================
                //sqlCommand = new SqlCommand("Create Database " + database.DatabaseName + "(EDITION='" + database.DatabaseEdition + "', MAXSIZE=" + database.DatabaseSize + ")", sqlConnection);
                sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = "Create Database " + database.DatabaseName + "(EDITION='" + database.DatabaseTier + "', MAXSIZE=" + database.DatabaseSize + ")";

                sqlCommand.CommandTimeout = 90; //<-- We increase timeout to 90 seconds on this important procedure

                sqlCommand.Connection.OpenWithRetry();
                sqlCommand.ExecuteNonQueryWithRetry();
                sqlCommand.Connection.Close();

                response = true;
            }
            catch
            {
                sqlCommand.Connection.Close();
                response = false;
            }

            return response;
        }

        /*
        private static void executeNonQueryStatement(string statement, ReliableSqlConnection sqlConnection)
        {
            if (statement != "")
            {

                //SqlCommand sqlCommandGenerateTables = new SqlCommand(statement, sqlConnection);
                SqlCommand sqlCommandGenerateTables = sqlConnection.CreateCommand();
                sqlCommandGenerateTables.CommandText = statement;
			    

                try
                {

                    sqlCommandGenerateTables.Connection.OpenWithRetry();
                    sqlCommandGenerateTables.ExecuteNonQueryWithRetry();
                    sqlCommandGenerateTables.Connection.Close();
                }
                catch
                {
                    //Try again (ADO.NET Connection Pooling may require a retry)
                    sqlCommandGenerateTables.Connection.Close();

                    sqlCommandGenerateTables.Connection.OpenWithRetry();
                    sqlCommandGenerateTables.ExecuteNonQueryWithRetry();
                    sqlCommandGenerateTables.Connection.Close();
                }
            }

        }*/

    }
}
