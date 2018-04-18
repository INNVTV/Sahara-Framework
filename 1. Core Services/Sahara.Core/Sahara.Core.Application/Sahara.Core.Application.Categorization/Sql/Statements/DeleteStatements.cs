using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Application.Categorization.Sql.Statements
{
    public static class DeleteStatements
    {

        #region Category

        internal static bool DeleteCategory(string sqlPartition, string schemaId, string categoryId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement (Delete All Associated Subcategories) =============================================================
            //sqlStatement.Append("DELETE FROM ");
            //sqlStatement.Append(schemaId);
            //sqlStatement.Append(".Subcategory ");

            //sqlStatement.Append(" WHERE CategoryID = '");
            //sqlStatement.Append(categoryId);
            //sqlStatement.Append("'");

            //END STATEMENT =============================================================
            //sqlStatement.Append("; ");

            //SQL Statement (Delete The Category) =============================================================
            sqlStatement.Append("DELETE FROM ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Category ");

            sqlStatement.Append(" WHERE CategoryID = '");
            sqlStatement.Append(categoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();

            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        #endregion

        #region Subcategory

        internal static bool DeleteSubcategory(string sqlPartition, string schemaId, string subcategoryId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("DELETE FROM ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subcategory ");

            sqlStatement.Append(" WHERE SubcategoryID = '");
            sqlStatement.Append(subcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();




            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        #endregion

        #region Subsubcategory

        internal static bool DeleteSubsubcategory(string sqlPartition, string schemaId, string subsubcategoryId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("DELETE FROM ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subsubcategory ");

            sqlStatement.Append(" WHERE SubsubcategoryID = '");
            sqlStatement.Append(subsubcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();




            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        #endregion

        #region Subsubsubcategory

        internal static bool DeleteSubsubsubcategory(string sqlPartition, string schemaId, string subsubsubcategoryId)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("DELETE FROM ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subsubsubcategory ");

            sqlStatement.Append(" WHERE SubsubsubcategoryID = '");
            sqlStatement.Append(subsubsubcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();




            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        #endregion
    }
}