using Sahara.Core.Logging.PlatformLogs;
using Sahara.Core.Logging.PlatformLogs.Types;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Collections.Generic;
using Sahara.Core.Common.ResponseTypes;

namespace Sahara.Core.Application.Categorization.Sql.Statements
{
    public static class UpdateStatements
    {

        #region Category

        internal static bool UpdateCategoryName(string sqlPartition, string schemaId, string categoryId, string newCategoryName)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Category ");

            sqlStatement.Append("SET CategoryName = @CategoryName");
            sqlStatement.Append(", CategoryNameKey = @CategoryNameKey");

            sqlStatement.Append(" WHERE CategoryID = '");
            sqlStatement.Append(categoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();
			



            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@CategoryName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@CategoryNameKey", SqlDbType.NVarChar);


            sqlCommand.Parameters["@CategoryName"].Value = newCategoryName;
            sqlCommand.Parameters["@CategoryNameKey"].Value = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(newCategoryName);


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static bool UpdateCategoryVisibleState(string sqlPartition, string schemaId, string categoryId, bool isVisible)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Category ");

            sqlStatement.Append("SET Visible = '");
            sqlStatement.Append(isVisible);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE CategoryID = '");
            sqlStatement.Append(categoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
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

        internal static DataAccessResponseType ReorderCategories(string sqlPartition, string schemaId, Dictionary<string, int> categoryOrderingDictionary)
        {
            var response = new DataAccessResponseType();

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            foreach (KeyValuePair<string, int> ordering in categoryOrderingDictionary)
            {
                try
                {
                    sqlStatement.Append("UPDATE  ");
                    sqlStatement.Append(schemaId);
                    sqlStatement.Append(".Category ");

                    sqlStatement.Append("SET OrderID = '");
                    sqlStatement.Append(ordering.Value);
                    sqlStatement.Append("' ");

                    sqlStatement.Append("WHERE CategoryID = '");
                    sqlStatement.Append(ordering.Key);
                    sqlStatement.Append("'; ");
                }
                catch(Exception e)
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "An error occurred while looping through the dictionary: " + e.Message };
                }
            }

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result > 0)
            {
                response.isSuccess = true;
            }

            return response;
        }

        internal static DataAccessResponseType ResetCategoryOrdering(string sqlPartition, string schemaId)
        {
            var response = new DataAccessResponseType();

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Category ");
            sqlStatement.Append("SET OrderID = '0'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();

            if (result > 0)
            {
                response.isSuccess = true;
            }

            return response;
        }

        internal static bool UpdateCategoryDescription(string sqlPartition, string schemaId, string categoryId, string newDescription)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Category ");

            sqlStatement.Append("SET Description = @Description");

            sqlStatement.Append(" WHERE CategoryID = '");
            sqlStatement.Append(categoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();


            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@Description", SqlDbType.NVarChar);

            sqlCommand.Parameters["@Description"].Value = newDescription;

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

        internal static bool UpdateSubcategoryName(string sqlPartition, string schemaId, string subcategoryId, string newSubcategoryName)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subcategory ");

            sqlStatement.Append("SET SubcategoryName = @SubcategoryName");
            sqlStatement.Append(", SubcategoryNameKey = @SubcategoryNameKey");

            sqlStatement.Append(" WHERE SubcategoryID = '");
            sqlStatement.Append(subcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = sqlStatement.ToString();
			



            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@SubcategoryName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@SubcategoryNameKey", SqlDbType.NVarChar);


            sqlCommand.Parameters["@SubcategoryName"].Value = newSubcategoryName;
            sqlCommand.Parameters["@SubcategoryNameKey"].Value = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(newSubcategoryName);


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static bool UpdateSubcategoryVisibleState(string sqlPartition, string schemaId, string subcategoryId, bool isVisible)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subcategory ");

            sqlStatement.Append("SET Visible = '");
            sqlStatement.Append(isVisible);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE SubcategoryID = '");
            sqlStatement.Append(subcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
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

        internal static DataAccessResponseType ReorderSubcategories(string sqlPartition, string schemaId, Dictionary<string, int> subcategoryOrderingDictionary)
        {
            var response = new DataAccessResponseType();

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            foreach (KeyValuePair<string, int> ordering in subcategoryOrderingDictionary)
            {
                try
                {
                    sqlStatement.Append("UPDATE  ");
                    sqlStatement.Append(schemaId);
                    sqlStatement.Append(".Subcategory ");

                    sqlStatement.Append("SET OrderID = '");
                    sqlStatement.Append(ordering.Value);
                    sqlStatement.Append("' ");

                    sqlStatement.Append("WHERE SubcategoryID = '");
                    sqlStatement.Append(ordering.Key);
                    sqlStatement.Append("'; ");
                }
                catch (Exception e)
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "An error occurred while looping through the dictionary: " + e.Message };
                }
            }

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result > 0)
            {
                response.isSuccess = true;
            }

            return response;
        }

        internal static DataAccessResponseType ResetSubcategoryOrdering(string sqlPartition, string schemaId, string categoryId)
        {
            var response = new DataAccessResponseType();

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subcategory ");
            sqlStatement.Append("SET OrderID = '0' ");
            sqlStatement.Append("WHERE CategoryID = '");
            sqlStatement.Append(categoryId);
            sqlStatement.Append("'; ");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();

            if (result > 0)
            {
                response.isSuccess = true;
            }

            return response;
        }

        internal static bool UpdateSubcategoryDescription(string sqlPartition, string schemaId, string subcategoryId, string newDescription)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subcategory ");

            sqlStatement.Append("SET Description = @Description");

            sqlStatement.Append(" WHERE SubcategoryID = '");
            sqlStatement.Append(subcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();


            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@Description", SqlDbType.NVarChar);

            sqlCommand.Parameters["@Description"].Value = newDescription;

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

        internal static bool UpdateSubsubcategoryName(string sqlPartition, string schemaId, string subsubcategoryId, string newSubsubcategoryName)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subsubcategory ");

            sqlStatement.Append("SET SubsubcategoryName = @SubsubcategoryName");
            sqlStatement.Append(", SubsubcategoryNameKey = @SubsubcategoryNameKey");

            sqlStatement.Append(" WHERE SubsubcategoryID = '");
            sqlStatement.Append(subsubcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();




            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@SubsubcategoryName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@SubsubcategoryNameKey", SqlDbType.NVarChar);


            sqlCommand.Parameters["@SubsubcategoryName"].Value = newSubsubcategoryName;
            sqlCommand.Parameters["@SubsubcategoryNameKey"].Value = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(newSubsubcategoryName);


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static bool UpdateSubsubcategoryVisibleState(string sqlPartition, string schemaId, string subsubcategoryId, bool isVisible)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subsubcategory ");

            sqlStatement.Append("SET Visible = '");
            sqlStatement.Append(isVisible);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE SubsubcategoryID = '");
            sqlStatement.Append(subsubcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
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

        internal static DataAccessResponseType ReorderSubsubcategories(string sqlPartition, string schemaId, Dictionary<string, int> subsubcategoryOrderingDictionary)
        {
            var response = new DataAccessResponseType();

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            foreach (KeyValuePair<string, int> ordering in subsubcategoryOrderingDictionary)
            {
                try
                {
                    sqlStatement.Append("UPDATE  ");
                    sqlStatement.Append(schemaId);
                    sqlStatement.Append(".Subsubcategory ");

                    sqlStatement.Append("SET OrderID = '");
                    sqlStatement.Append(ordering.Value);
                    sqlStatement.Append("' ");

                    sqlStatement.Append("WHERE SubsubcategoryID = '");
                    sqlStatement.Append(ordering.Key);
                    sqlStatement.Append("'; ");
                }
                catch (Exception e)
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "An error occurred while looping through the dictionary: " + e.Message };
                }
            }

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result > 0)
            {
                response.isSuccess = true;
            }

            return response;
        }

        internal static DataAccessResponseType ResetSubsubcategoryOrdering(string sqlPartition, string schemaId, string subcategoryId)
        {
            var response = new DataAccessResponseType();

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subsubcategory ");
            sqlStatement.Append("SET OrderID = '0' ");
            sqlStatement.Append("WHERE SubcategoryID = '");
            sqlStatement.Append(subcategoryId);
            sqlStatement.Append("'; ");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();

            if (result > 0)
            {
                response.isSuccess = true;
            }

            return response;
        }

        internal static bool UpdateSubsubcategoryDescription(string sqlPartition, string schemaId, string subsubcategoryId, string newDescription)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subsubcategory ");

            sqlStatement.Append("SET Description = @Description");

            sqlStatement.Append(" WHERE SubsubcategoryID = '");
            sqlStatement.Append(subsubcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();


            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@Description", SqlDbType.NVarChar);

            sqlCommand.Parameters["@Description"].Value = newDescription;

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

        internal static bool UpdateSubsubsubcategoryName(string sqlPartition, string schemaId, string subsubsubcategoryId, string newSubsubsubcategoryName)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subsubsubcategory ");

            sqlStatement.Append("SET SubsubsubcategoryName = @SubsubsubcategoryName");
            sqlStatement.Append(", SubsubsubcategoryNameKey = @SubsubsubcategoryNameKey");

            sqlStatement.Append(" WHERE SubsubsubcategoryID = '");
            sqlStatement.Append(subsubsubcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();




            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@SubsubsubcategoryName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@SubsubsubcategoryNameKey", SqlDbType.NVarChar);


            sqlCommand.Parameters["@SubsubsubcategoryName"].Value = newSubsubsubcategoryName;
            sqlCommand.Parameters["@SubsubsubcategoryNameKey"].Value = Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(newSubsubsubcategoryName);


            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static bool UpdateSubsubsubcategoryVisibleState(string sqlPartition, string schemaId, string subsubsubcategoryId, bool isVisible)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subsubsubcategory ");

            sqlStatement.Append("SET Visible = '");
            sqlStatement.Append(isVisible);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE SubsubsubcategoryID = '");
            sqlStatement.Append(subsubsubcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
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

        internal static DataAccessResponseType ReorderSubsubsubcategories(string sqlPartition, string schemaId, Dictionary<string, int> subsubsubcategoryOrderingDictionary)
        {
            var response = new DataAccessResponseType();

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            foreach (KeyValuePair<string, int> ordering in subsubsubcategoryOrderingDictionary)
            {
                try
                {
                    sqlStatement.Append("UPDATE  ");
                    sqlStatement.Append(schemaId);
                    sqlStatement.Append(".Subsubsubcategory ");

                    sqlStatement.Append("SET OrderID = '");
                    sqlStatement.Append(ordering.Value);
                    sqlStatement.Append("' ");

                    sqlStatement.Append("WHERE SubsubsubcategoryID = '");
                    sqlStatement.Append(ordering.Key);
                    sqlStatement.Append("'; ");
                }
                catch (Exception e)
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "An error occurred while looping through the dictionary: " + e.Message };
                }
            }

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result > 0)
            {
                response.isSuccess = true;
            }

            return response;
        }

        internal static DataAccessResponseType ResetSubsubsubcategoryOrdering(string sqlPartition, string schemaId, string subsubcategoryId)
        {
            var response = new DataAccessResponseType();

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subsubsubcategory ");
            sqlStatement.Append("SET OrderID = '0' ");
            sqlStatement.Append("WHERE SubsubcategoryID = '");
            sqlStatement.Append(subsubcategoryId);
            sqlStatement.Append("'; ");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();

            if (result > 0)
            {
                response.isSuccess = true;
            }

            return response;
        }

        internal static bool UpdateSubsubsubcategoryDescription(string sqlPartition, string schemaId, string subsubsubcategoryId, string newDescription)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Subsubsubcategory ");

            sqlStatement.Append("SET Description = @Description");

            sqlStatement.Append(" WHERE SubsubsubcategoryID = '");
            sqlStatement.Append(subsubsubcategoryId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();


            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@Description", SqlDbType.NVarChar);

            sqlCommand.Parameters["@Description"].Value = newDescription;

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