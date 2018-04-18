using System.Data;
using System.Data.SqlClient;
using System.Text;
using Sahara.Core.Common.ResponseTypes;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Platform.Users.Sql.Statements
{
    public static class UpdateStatements
    {
        public static DataAccessResponseType UpdateUserActiveState(string userId, bool isActive)
        {

            var response = new DataAccessResponseType();

            response.isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PlatformUsers ");
            sqlStatement.Append("SET Active = @isActive");
            sqlStatement.Append(" WHERE id = @id");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection.CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();
			

            sqlCommand.Parameters.Add("@isActive", SqlDbType.Bit);
            sqlCommand.Parameters.Add("@id", SqlDbType.NVarChar);

            sqlCommand.Parameters["@isActive"].Value = isActive;
            sqlCommand.Parameters["@id"].Value = userId;


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result == 1)
            {
                response.isSuccess = true;
            }

            return response;

        }

        internal static bool UpdateUserFullName(string userId, string firstName, string lastName)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PlatformUsers ");

            sqlStatement.Append("SET FirstName = '");
            sqlStatement.Append(firstName);
            sqlStatement.Append("'");

            sqlStatement.Append(", LastName = '");
            sqlStatement.Append(lastName);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE Id = '");
            sqlStatement.Append(userId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection.CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result == 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static bool UpdateUserEmail(string UserID, string email)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PlatformUsers ");

            sqlStatement.Append("SET Email = '");
            sqlStatement.Append(email);
            sqlStatement.Append("'");

            sqlStatement.Append(", UserName = '");
            sqlStatement.Append(email);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE Id = '");
            sqlStatement.Append(UserID);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection.CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result == 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static bool UpdateUserPhoto(string userId, string imageId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE PlatformUsers ");

            sqlStatement.Append("SET Photo = '");
            sqlStatement.Append(imageId);
            sqlStatement.Append("'");


            sqlStatement.Append(" WHERE Id = '");
            sqlStatement.Append(userId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.PlatformSqlConnection.CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result == 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }
    }
}
