using System;
using System.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Platform.Users.Sql.Statements
{
    internal static class SelectStatements
    {
        public static int SelectSuperAdminCount()
        {

            int response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT COUNT(*) AS Count FROM PlatformUsers INNER JOIN PlatformUsersInRoles ON PlatformUsers.Id = PlatformUsersInRoles.UserId INNER JOIN PlatformUserRoles ON PlatformUserRoles.Id = PlatformUsersInRoles.RoleId Where PlatformUserRoles.Name = '" + Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin + "'";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response = Convert.ToInt32(reader["Count"]);
            }

            sqlCommand.Connection.Close();

            return response;
        }

        public static int SelectActiveSuperAdminCount()
        {

            int response = 0;

            //SQL Statement =============================================================
            string SqlStatement =
                "SELECT COUNT(*) AS Count FROM PlatformUsers INNER JOIN PlatformUsersInRoles ON PlatformUsers.Id = PlatformUsersInRoles.UserId INNER JOIN PlatformUserRoles ON PlatformUserRoles.Id = PlatformUsersInRoles.RoleId Where PlatformUserRoles.Name = '" + Sahara.Core.Settings.Platform.Users.Authorization.Roles.SuperAdmin + "' AND PlatformUsers.Active = '1'";

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement, Sahara.Core.Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                response = Convert.ToInt32(reader["Count"]);
            }

            sqlCommand.Connection.Close();

            return response;
        }
    }
}
