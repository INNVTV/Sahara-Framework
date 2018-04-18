using Sahara.Core.Application.Categorization.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Application.Categorization.Sql.Statements
{
    internal static class InsertStatements
    {
        internal static DataAccessResponseType InsertCategory(string sqlPartition, string schemaId, CategoryModel category, int maxAllowed)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statements =============================================================

            //Check Row Count ===========================================================
            //SqlStatement.Append("DECLARE @ObjectCount INT ");
            SqlStatement.Append("SET @ObjectCount = (SELECT COUNT(*) ");
            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category) ");
            SqlStatement.Append("IF @ObjectCount < '");
            SqlStatement.Append(maxAllowed);
            SqlStatement.Append("' ");
            SqlStatement.Append("BEGIN ");

            //GET MaxOrderBy =============================================================
            //If the highest OrderBy is '0' we insert next as '0' (Alphabetical order) otherwise we +1 the OrderID so the newest categegorization item 
            SqlStatement.Append("DECLARE @MaxOrderBy INT ");
            SqlStatement.Append("SET @MaxOrderBy = (SELECT MAX(OrderID) FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category) ");
            SqlStatement.Append("IF(@MaxOrderBy > 0) ");
            SqlStatement.Append("BEGIN ");
            SqlStatement.Append("SET @MaxOrderBy = @MaxOrderBy + 1 ");
            SqlStatement.Append("END ");

            SqlStatement.Append("IF(@MaxOrderBy IS NULL) ");
            SqlStatement.Append("BEGIN ");
            SqlStatement.Append("SET @MaxOrderBy = 0 ");
            SqlStatement.Append("END ");

            //INSERT =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category (");

            SqlStatement.Append("CategoryID,");
            SqlStatement.Append("CategoryName,");
            SqlStatement.Append("CategoryNameKey,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("OrderID, ");
            SqlStatement.Append("Visible");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@CategoryID, ");
            SqlStatement.Append("@CategoryName, ");
            SqlStatement.Append("@CategoryNameKey, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@MaxOrderBy, ");
            SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //CLOSE: Check Row Count ===========================================================
            SqlStatement.Append(" END");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();



            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@CategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@CategoryName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@CategoryNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            //sqlCommand.Parameters.Add("@OrderID", SqlDbType.Int);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            sqlCommand.Parameters["@CategoryID"].Value = category.CategoryID;
            sqlCommand.Parameters["@CategoryName"].Value = category.CategoryName;
            sqlCommand.Parameters["@CategoryNameKey"].Value = category.CategoryNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            //sqlCommand.Parameters["@OrderID"].Value = category.OrderID;
            sqlCommand.Parameters["@Visible"].Value = category.Visible;

            // Add output parameters
            SqlParameter objectCount = sqlCommand.Parameters.Add("@ObjectCount", SqlDbType.Int);
            objectCount.Direction = ParameterDirection.Output;

            int insertAccountResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if (insertAccountResult > 0)
                {
                    response.isSuccess = true;
                }
                else
                {
                    if ((int)objectCount.Value >= maxAllowed)
                    {
                        return new DataAccessResponseType
                        {
                            isSuccess = false,
                            ErrorMessage = "Your plan does not allow for more than " + maxAllowed + " categories. Please upgrade to increase your limits."
                            //ErrorMessage = "You have reached the maximum amount of categories for your account. Please upgrade your plan or contact support to increase your limits."
                        };
                    }
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert a application category into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }

        internal static DataAccessResponseType InsertSubcategory(string sqlPartition, string schemaId, SubcategoryModel subcategory, int maxAllowed)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //SQL Statements =============================================================

            //Check Row Count ===========================================================
            //SqlStatement.Append("DECLARE @ObjectCount INT ");
            SqlStatement.Append("SET @ObjectCount = (SELECT COUNT(*) ");
            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory WHERE CategoryID='");
            SqlStatement.Append(subcategory.CategoryID);
            SqlStatement.Append("') ");
            SqlStatement.Append("IF @ObjectCount < '");
            SqlStatement.Append(maxAllowed);
            SqlStatement.Append("' ");
            SqlStatement.Append("BEGIN ");

            //GET MaxOrderBy =============================================================
            //If the highest OrderBy is '0' we insert next as '0' (Alphabetical order) otherwise we +1 the OrderID so the newest categegorization item 
            SqlStatement.Append("DECLARE @MaxOrderBy INT ");
            SqlStatement.Append("SET @MaxOrderBy = (SELECT MAX(OrderID) FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory WHERE CategoryID='");
            SqlStatement.Append(subcategory.CategoryID);
            SqlStatement.Append("') ");
            SqlStatement.Append("IF(@MaxOrderBy > 0) ");
            SqlStatement.Append("BEGIN ");
            SqlStatement.Append("SET @MaxOrderBy = @MaxOrderBy + 1 ");
            SqlStatement.Append("END ");

            SqlStatement.Append("IF(@MaxOrderBy IS NULL) ");
            SqlStatement.Append("BEGIN ");
            SqlStatement.Append("SET @MaxOrderBy = 0 ");
            SqlStatement.Append("END ");

            //INSERT =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory (");

            SqlStatement.Append("CategoryID,");
            SqlStatement.Append("SubcategoryID,");
            SqlStatement.Append("SubcategoryName,");
            SqlStatement.Append("SubcategoryNameKey,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("OrderID, ");
            SqlStatement.Append("Visible");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@CategoryID, ");
            SqlStatement.Append("@SubcategoryID, ");
            SqlStatement.Append("@SubcategoryName, ");
            SqlStatement.Append("@SubcategoryNameKey, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@MaxOrderBy, ");
            SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //CLOSE: Check Row Count ===========================================================
            SqlStatement.Append(" END");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();




            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@CategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubcategoryName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@SubcategoryNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            //sqlCommand.Parameters.Add("@OrderID", SqlDbType.Int);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            sqlCommand.Parameters["@CategoryID"].Value = subcategory.CategoryID;
            sqlCommand.Parameters["@SubcategoryID"].Value = subcategory.SubcategoryID;
            sqlCommand.Parameters["@SubcategoryName"].Value = subcategory.SubcategoryName;
            sqlCommand.Parameters["@SubcategoryNameKey"].Value = subcategory.SubcategoryNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            //sqlCommand.Parameters["@OrderID"].Value = subcategory.OrderID;
            sqlCommand.Parameters["@Visible"].Value = subcategory.Visible;

            // Add output parameters
            SqlParameter objectCount = sqlCommand.Parameters.Add("@ObjectCount", SqlDbType.Int);
            objectCount.Direction = ParameterDirection.Output;

            int insertAccountResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if (insertAccountResult > 0)
                {
                    response.isSuccess = true;
                }
                else
                {
                    if ((int)objectCount.Value >= maxAllowed)
                    {
                        return new DataAccessResponseType
                        {
                            isSuccess = false,
                            ErrorMessage = "Your plan does not allow for more than " + maxAllowed + " categories per set. Please upgrade to increase your limits."
                            //ErrorMessage = "You have reached the maximum amount of categories for your account. Please upgrade your plan or contact support to increase your limits."
                        };
                    }
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert a application subcategory into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }

        internal static DataAccessResponseType InsertSubsubcategory(string sqlPartition, string schemaId, SubsubcategoryModel subsubcategory, int maxAllowed)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //SQL Statements =============================================================

            //Check Row Count ===========================================================
            //SqlStatement.Append("DECLARE @ObjectCount INT ");
            SqlStatement.Append("SET @ObjectCount = (SELECT COUNT(*) ");
            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory WHERE SubcategoryID='");
            SqlStatement.Append(subsubcategory.SubcategoryID);
            SqlStatement.Append("') ");
            SqlStatement.Append("IF @ObjectCount < '");
            SqlStatement.Append(maxAllowed);
            SqlStatement.Append("' ");
            SqlStatement.Append("BEGIN ");

            //GET MaxOrderBy =============================================================
            //If the highest OrderBy is '0' we insert next as '0' (Alphabetical order) otherwise we +1 the OrderID so the newest categegorization item 
            SqlStatement.Append("DECLARE @MaxOrderBy INT ");
            SqlStatement.Append("SET @MaxOrderBy = (SELECT MAX(OrderID) FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory WHERE SubcategoryID='");
            SqlStatement.Append(subsubcategory.SubcategoryID);
            SqlStatement.Append("') ");
            SqlStatement.Append("IF(@MaxOrderBy > 0) ");
            SqlStatement.Append("BEGIN ");
            SqlStatement.Append("SET @MaxOrderBy = @MaxOrderBy + 1 ");
            SqlStatement.Append("END ");

            SqlStatement.Append("IF(@MaxOrderBy IS NULL) ");
            SqlStatement.Append("BEGIN ");
            SqlStatement.Append("SET @MaxOrderBy = 0 ");
            SqlStatement.Append("END ");

            //INSERT =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory (");

            //SqlStatement.Append("CategoryID,");
            SqlStatement.Append("SubcategoryID,");
            SqlStatement.Append("SubsubcategoryID,");
            SqlStatement.Append("SubsubcategoryName,");
            SqlStatement.Append("SubsubcategoryNameKey,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("OrderID, ");
            SqlStatement.Append("Visible");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            //SqlStatement.Append("@CategoryID, ");
            SqlStatement.Append("@SubcategoryID, ");
            SqlStatement.Append("@SubsubcategoryID, ");
            SqlStatement.Append("@SubsubcategoryName, ");
            SqlStatement.Append("@SubsubcategoryNameKey, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@MaxOrderBy, ");
            SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //CLOSE: Check Row Count ===========================================================
            SqlStatement.Append(" END");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();




            //Using parameterized queries to protect against injection
            //sqlCommand.Parameters.Add("@CategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubsubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubsubcategoryName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@SubsubcategoryNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            //sqlCommand.Parameters.Add("@OrderID", SqlDbType.Int);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            //sqlCommand.Parameters["@CategoryID"].Value = subsubcategory.CategoryID;
            sqlCommand.Parameters["@SubcategoryID"].Value = subsubcategory.SubcategoryID;
            sqlCommand.Parameters["@SubsubcategoryID"].Value = subsubcategory.SubsubcategoryID;
            sqlCommand.Parameters["@SubsubcategoryName"].Value = subsubcategory.SubsubcategoryName;
            sqlCommand.Parameters["@SubsubcategoryNameKey"].Value = subsubcategory.SubsubcategoryNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            //sqlCommand.Parameters["@OrderID"].Value = subsubcategory.OrderID;
            sqlCommand.Parameters["@Visible"].Value = subsubcategory.Visible;

            // Add output parameters
            SqlParameter objectCount = sqlCommand.Parameters.Add("@ObjectCount", SqlDbType.Int);
            objectCount.Direction = ParameterDirection.Output;

            int insertAccountResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if (insertAccountResult > 0)
                {
                    response.isSuccess = true;
                }
                else
                {
                    if ((int)objectCount.Value >= maxAllowed)
                    {
                        return new DataAccessResponseType
                        {
                            isSuccess = false,
                            ErrorMessage = "Your plan does not allow for more than " + maxAllowed + " categories per set. Please upgrade to increase your limits."
                            //ErrorMessage = "You have reached the maximum amount of subsubcategories for this subcateory. Please upgrade your plan or contact support to increase your limits."
                        };
                    }
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert a application subsubcategory into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }

        internal static DataAccessResponseType InsertSubsubsubcategory(string sqlPartition, string schemaId, SubsubsubcategoryModel subsubsubcategory, int maxAllowed)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statements =============================================================

            //Check Row Count ===========================================================
            //SqlStatement.Append("DECLARE @ObjectCount INT ");
            SqlStatement.Append("SET @ObjectCount = (SELECT COUNT(*) ");
            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory WHERE SubsubcategoryID='");
            SqlStatement.Append(subsubsubcategory.SubsubcategoryID);
            SqlStatement.Append("') ");
            SqlStatement.Append("IF @ObjectCount < '");
            SqlStatement.Append(maxAllowed);
            SqlStatement.Append("' ");
            SqlStatement.Append("BEGIN ");

            //GET MaxOrderBy =============================================================
            //If the highest OrderBy is '0' we insert next as '0' (Alphabetical order) otherwise we +1 the OrderID so the newest categegorization item 
            SqlStatement.Append("DECLARE @MaxOrderBy INT ");
            SqlStatement.Append("SET @MaxOrderBy = (SELECT MAX(OrderID) FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory WHERE SubsubcategoryID='");
            SqlStatement.Append(subsubsubcategory.SubsubcategoryID);
            SqlStatement.Append("') ");
            SqlStatement.Append("IF(@MaxOrderBy > 0) ");
            SqlStatement.Append("BEGIN ");
            SqlStatement.Append("SET @MaxOrderBy = @MaxOrderBy + 1 ");
            SqlStatement.Append("END ");

            SqlStatement.Append("IF(@MaxOrderBy IS NULL) ");
            SqlStatement.Append("BEGIN ");
            SqlStatement.Append("SET @MaxOrderBy = 0 ");
            SqlStatement.Append("END ");

            //INSERT =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory (");

            //SqlStatement.Append("CategoryID,");
            //SqlStatement.Append("SubcategoryID,");
            SqlStatement.Append("SubsubcategoryID,");
            SqlStatement.Append("SubsubsubcategoryID,");
            SqlStatement.Append("SubsubsubcategoryName,");
            SqlStatement.Append("SubsubsubcategoryNameKey,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("OrderID, ");
            SqlStatement.Append("Visible");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            //SqlStatement.Append("@CategoryID, ");
            //SqlStatement.Append("@SubcategoryID, ");
            SqlStatement.Append("@SubsubcategoryID, ");
            SqlStatement.Append("@SubsubsubcategoryID, ");
            SqlStatement.Append("@SubsubsubcategoryName, ");
            SqlStatement.Append("@SubsubsubcategoryNameKey, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@MaxOrderBy, ");
            SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //CLOSE: Check Row Count ===========================================================
            SqlStatement.Append(" END");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();




            //Using parameterized queries to protect against injection
            //sqlCommand.Parameters.Add("@CategoryID", SqlDbType.UniqueIdentifier);
            //sqlCommand.Parameters.Add("@SubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubsubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubsubsubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubsubsubcategoryName", SqlDbType.NVarChar);
            sqlCommand.Parameters.Add("@SubsubsubcategoryNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            //sqlCommand.Parameters.Add("@OrderID", SqlDbType.Int);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            //sqlCommand.Parameters["@CategoryID"].Value = subsubcategory.CategoryID;
            //sqlCommand.Parameters["@SubcategoryID"].Value = subsubcategory.SubcategoryID;
            sqlCommand.Parameters["@SubsubcategoryID"].Value = subsubsubcategory.SubsubcategoryID;
            sqlCommand.Parameters["@SubsubsubcategoryID"].Value = subsubsubcategory.SubsubsubcategoryID;
            sqlCommand.Parameters["@SubsubsubcategoryName"].Value = subsubsubcategory.SubsubsubcategoryName;
            sqlCommand.Parameters["@SubsubsubcategoryNameKey"].Value = subsubsubcategory.SubsubsubcategoryNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            //sqlCommand.Parameters["@OrderID"].Value = subsubsubcategory.OrderID;
            sqlCommand.Parameters["@Visible"].Value = subsubsubcategory.Visible;

            // Add output parameters
            SqlParameter objectCount = sqlCommand.Parameters.Add("@ObjectCount", SqlDbType.Int);
            objectCount.Direction = ParameterDirection.Output;

            int insertAccountResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if (insertAccountResult > 0)
                {
                    response.isSuccess = true;
                }
                else
                {
                    if((int)objectCount.Value >= maxAllowed)
                    {
                        return new DataAccessResponseType
                        {
                            isSuccess = false,
                            ErrorMessage = "Your plan does not allow for more than " + maxAllowed + " categories per set. Please upgrade to increase your limits."
                            //ErrorMessage = "You have reached the maximum amount of subsubsubcategories for this subsubcateory. Please upgrade your plan or contact support to increase your limits."
                        };
                    }
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert a application subsubsubcategory into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }

        /*
        public static DataAccessResponseType InsertCategory(string sqlPartition, string schemaId, CategoryModel category)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statement =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category (");

            SqlStatement.Append("CategoryID,");
            SqlStatement.Append("CategoryName,");
            SqlStatement.Append("CategoryNameKey,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("OrderID, ");
            SqlStatement.Append("Visible");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@CategoryID, ");
            SqlStatement.Append("@CategoryName, ");
            SqlStatement.Append("@CategoryNameKey, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@OrderID, ");
            SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();
			


            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@CategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@CategoryName", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CategoryNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            sqlCommand.Parameters.Add("@OrderID", SqlDbType.Int);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            sqlCommand.Parameters["@CategoryID"].Value = category.CategoryID;
            sqlCommand.Parameters["@CategoryName"].Value = category.CategoryName;
            sqlCommand.Parameters["@CategoryNameKey"].Value = category.CategoryNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            sqlCommand.Parameters["@OrderID"].Value = category.OrderID;
            sqlCommand.Parameters["@Visible"].Value = category.Visible;

            int insertAccountResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if(insertAccountResult > 0)
                {
                    response.isSuccess = true;
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert a application category into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }

        public static DataAccessResponseType InsertSubcategory(string sqlPartition, string schemaId, SubcategoryModel subcategory)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statement =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory (");

            SqlStatement.Append("CategoryID,");
            SqlStatement.Append("SubcategoryID,");
            SqlStatement.Append("SubcategoryName,");
            SqlStatement.Append("SubcategoryNameKey,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("OrderID, ");
            SqlStatement.Append("Visible");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@CategoryID, ");
            SqlStatement.Append("@SubcategoryID, ");
            SqlStatement.Append("@SubcategoryName, ");
            SqlStatement.Append("@SubcategoryNameKey, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@OrderID, ");
            SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();




            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@CategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubcategoryName", SqlDbType.Text);
            sqlCommand.Parameters.Add("@SubcategoryNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            sqlCommand.Parameters.Add("@OrderID", SqlDbType.Int);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            sqlCommand.Parameters["@CategoryID"].Value = subcategory.CategoryID;
            sqlCommand.Parameters["@SubcategoryID"].Value = subcategory.SubcategoryID;
            sqlCommand.Parameters["@SubcategoryName"].Value = subcategory.SubcategoryName;
            sqlCommand.Parameters["@SubcategoryNameKey"].Value = subcategory.SubcategoryNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            sqlCommand.Parameters["@OrderID"].Value = subcategory.OrderID;
            sqlCommand.Parameters["@Visible"].Value = subcategory.Visible;

            int insertAccountResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if (insertAccountResult > 0)
                {
                    response.isSuccess = true;
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert a application subcategory into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }

        public static DataAccessResponseType InsertSubsubcategory(string sqlPartition, string schemaId, SubsubcategoryModel subsubcategory)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statement =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory (");

            //SqlStatement.Append("CategoryID,");
            SqlStatement.Append("SubcategoryID,");
            SqlStatement.Append("SubsubcategoryID,");
            SqlStatement.Append("SubsubcategoryName,");
            SqlStatement.Append("SubsubcategoryNameKey,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("OrderID, ");
            SqlStatement.Append("Visible");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            //SqlStatement.Append("@CategoryID, ");
            SqlStatement.Append("@SubcategoryID, ");
            SqlStatement.Append("@SubsubcategoryID, ");
            SqlStatement.Append("@SubsubcategoryName, ");
            SqlStatement.Append("@SubsubcategoryNameKey, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@OrderID, ");
            SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();




            //Using parameterized queries to protect against injection
            //sqlCommand.Parameters.Add("@CategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubsubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubsubcategoryName", SqlDbType.Text);
            sqlCommand.Parameters.Add("@SubsubcategoryNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            sqlCommand.Parameters.Add("@OrderID", SqlDbType.Int);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            //sqlCommand.Parameters["@CategoryID"].Value = subsubcategory.CategoryID;
            sqlCommand.Parameters["@SubcategoryID"].Value = subsubcategory.SubcategoryID;
            sqlCommand.Parameters["@SubsubcategoryID"].Value = subsubcategory.SubsubcategoryID;
            sqlCommand.Parameters["@SubsubcategoryName"].Value = subsubcategory.SubsubcategoryName;
            sqlCommand.Parameters["@SubsubcategoryNameKey"].Value = subsubcategory.SubsubcategoryNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            sqlCommand.Parameters["@OrderID"].Value = subsubcategory.OrderID;
            sqlCommand.Parameters["@Visible"].Value = subsubcategory.Visible;

            int insertAccountResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if (insertAccountResult > 0)
                {
                    response.isSuccess = true;
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert a application subsubcategory into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }

        public static DataAccessResponseType InsertSubsubsubcategory(string sqlPartition, string schemaId, SubsubsubcategoryModel subsubsubcategory)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statement =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory (");

            //SqlStatement.Append("CategoryID,");
            //SqlStatement.Append("SubcategoryID,");
            SqlStatement.Append("SubsubcategoryID,");
            SqlStatement.Append("SubsubsubcategoryID,");
            SqlStatement.Append("SubsubsubcategoryName,");
            SqlStatement.Append("SubsubsubcategoryNameKey,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("OrderID, ");
            SqlStatement.Append("Visible");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            //SqlStatement.Append("@CategoryID, ");
            //SqlStatement.Append("@SubcategoryID, ");
            SqlStatement.Append("@SubsubcategoryID, ");
            SqlStatement.Append("@SubsubsubcategoryID, ");
            SqlStatement.Append("@SubsubsubcategoryName, ");
            SqlStatement.Append("@SubsubsubcategoryNameKey, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@OrderID, ");
            SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();




            //Using parameterized queries to protect against injection
            //sqlCommand.Parameters.Add("@CategoryID", SqlDbType.UniqueIdentifier);
            //sqlCommand.Parameters.Add("@SubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubsubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubsubsubcategoryID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@SubsubsubcategoryName", SqlDbType.Text);
            sqlCommand.Parameters.Add("@SubsubsubcategoryNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            sqlCommand.Parameters.Add("@OrderID", SqlDbType.Int);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            //sqlCommand.Parameters["@CategoryID"].Value = subsubcategory.CategoryID;
            //sqlCommand.Parameters["@SubcategoryID"].Value = subsubcategory.SubcategoryID;
            sqlCommand.Parameters["@SubsubcategoryID"].Value = subsubsubcategory.SubsubcategoryID;
            sqlCommand.Parameters["@SubsubsubcategoryID"].Value = subsubsubcategory.SubsubsubcategoryID;
            sqlCommand.Parameters["@SubsubsubcategoryName"].Value = subsubsubcategory.SubsubsubcategoryName;
            sqlCommand.Parameters["@SubsubsubcategoryNameKey"].Value = subsubsubcategory.SubsubsubcategoryNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            sqlCommand.Parameters["@OrderID"].Value = subsubsubcategory.OrderID;
            sqlCommand.Parameters["@Visible"].Value = subsubsubcategory.Visible;

            int insertAccountResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if (insertAccountResult > 0)
                {
                    response.isSuccess = true;
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert a application subsubsubcategory into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }
        */
    }
}
