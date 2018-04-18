using System;
using System.Data.SqlClient;

namespace Sahara.Core.Platform.Initialization.Sql.Statements
{
    public static class VerificationStatements
    {

        public static bool DatabaseExists(string databaseName)
        {
            bool exists = false;

            string SqlStatement =
                //"SELECT name FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = '" + databaseName + "' OR name = '" + databaseName + "')";
                 "IF EXISTS (SELECT name FROM master.sys.databases WHERE name = N'" + databaseName + "') SELECT 'true' ELSE SELECT 'false'";

            SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), new SqlConnection(Sahara.Core.Settings.Azure.Databases.DatabaseConnections.MasterSqlConnection.ConnectionString));
            sqlCommand.Connection.Open();
            exists = Convert.ToBoolean(sqlCommand.ExecuteScalar());

            sqlCommand.Connection.Close();

            return exists;
        }

        public static bool TableExists(string tableName, string connectionString)
        {
            bool exists = false;

            string SqlStatement =
                "IF OBJECT_ID ('dbo." + tableName + "') IS NOT NULL SELECT 'true' ELSE SELECT 'false'";

            SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), new SqlConnection(connectionString));
            sqlCommand.Connection.Open();
            exists = Convert.ToBoolean(sqlCommand.ExecuteScalar());

            sqlCommand.Connection.Close();

            return exists;
        }

    }
}
