using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sahara.Core.Platform.Users.Sql.Statements
{
    public static class DeleteStatements
    {

        public static bool DeleteUser(string id)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("DELETE FROM PlatformUsers ");
            sqlStatement.Append("WHERE Id=@id");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection.CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();
			


            sqlCommand.Parameters.Add("@id", SqlDbType.NVarChar);

            sqlCommand.Parameters["@id"].Value = id;

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();

            if (result == 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }


        public static bool DeleteUserRoles(string id)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("DELETE FROM PlatformUsersInRoles ");
            sqlStatement.Append("WHERE UserId=@id");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection.CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();
			


            sqlCommand.Parameters.Add("@id", SqlDbType.NVarChar);

            sqlCommand.Parameters["@id"].Value = id;

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();

            if (result >= 0) //<-- 0 because user may not have any roles
            {
                isSuccess = true;
            }

            return isSuccess;
        }

    }
}
