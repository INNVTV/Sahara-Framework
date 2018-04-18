using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sahara.Core.Platform.Initialization.Sql.Statements
{
    internal static class SelectStatements
    {
        internal static List<string> GetAllPlatformDatabases()
        {
            var Databases = new List<string>();

            var sqlStatement = "SELECT name from sys.databases where name != 'master'";
            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection.CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                Databases.Add(reader["name"].ToString());
            }

            sqlCommand.Connection.Close();


            return Databases;

        }
    }
}
