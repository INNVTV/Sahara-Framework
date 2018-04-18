using Sahara.Core.Application.Categorization.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;


namespace Sahara.Core.Application.Categorization.Sql.Statements
{
    internal static class SelectStatements
    {
        #region Categories

        /*
        internal static int SelectCategoryCount(string sqlPartition, string schemaId)
        {
            int response = 0;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT COUNT(*) FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            response = (int)sqlCommand.ExecuteScalarWithRetry();

            sqlCommand.Connection.Close();

            return response;
        }*/

        internal static bool CategoryNameExistsElsewhere(string sqlPartition, string schemaId, string categoryId, string nameToCheck)
        {
            bool nameExistsElsewhere = false;
            List<Guid> Ids = new List<Guid>();

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT CategoryID FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category WHERE CategoryNameKey = '");
            SqlStatement.Append(Common.Methods.ObjectNames.ConvertToObjectNameKey(nameToCheck));
            SqlStatement.Append("'");

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                Ids.Add((Guid)reader["CategoryID"]);
            }

            foreach (Guid id in Ids)
            {
                if (id.ToString().ToLower() != categoryId.ToLower())
                {
                    nameExistsElsewhere = true;
                }
            }

            sqlCommand.Connection.Close();

            return nameExistsElsewhere;

        }

        internal static string SelectCategoryIdByName(string sqlPartition, string schemaId, string categoryName)
        {
            string id = String.Empty;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT Top 1 CategoryID FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category WHERE CategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName));
            SqlStatement.Append("'");


            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                id = reader["CategoryID"].ToString();
            }


            sqlCommand.Connection.Close();

            return id;
        }

        internal static List<CategoryModel> SelectCategoryList(string sqlPartition, string schemaId, bool includeHidden)
        {
            var categories = new List<CategoryModel>();

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category");

            if (includeHidden == false)
            {
                SqlStatement.Append(" WHERE Visible = '1'");
            }

            SqlStatement.Append(" ORDER BY OrderID Asc, CategoryName Asc");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                categories.Add(Transforms.Transforms.DataReader_to_CategoryModel(reader));
            }

            sqlCommand.Connection.Close();

            return categories;
        }

        internal static CategoryModel SelectCategoryById(string sqlPartition, string schemaId, string categoryId, bool includeHiddenSubcategories = true)
        {
            CategoryModel category = null;

            StringBuilder SqlStatement = new StringBuilder();

            //-- First set of results --

            SqlStatement.Append("SELECT TOP 1 * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category WHERE CategoryID = '");
            SqlStatement.Append(categoryId);
            SqlStatement.Append("'");

            //-- Second set of results --

            SqlStatement.Append("; SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory WHERE CategoryID = '");
            SqlStatement.Append(categoryId);
            SqlStatement.Append("'");

            if (includeHiddenSubcategories == false)
            {
                SqlStatement.Append(" AND Visible = '1'");
            }

            SqlStatement.Append(" ORDER BY OrderID Asc, SubcategoryName Asc");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                category = Transforms.Transforms.DataReader_to_CategoryModel(reader);
            }

            if(category != null)
            {
                category.Subcategories = new List<SubcategoryListModel>();

                if (reader.NextResult()) //<-- Move to Subcategory results (if any exist)
                {
                    while (reader.Read())
                    {
                        category.Subcategories.Add(Transforms.Transforms.DataReader_to_SubcategoryModel_List(reader, category.CategoryNameKey));
                    }
                }
            }

            sqlCommand.Connection.Close();

            return category;
        }

        internal static CategoryModel SelectCategoryByName(string sqlPartition, string schemaId, string categoryName, bool includeHiddenSubcategories = true)
        {
            CategoryModel category = null;

            StringBuilder SqlStatement = new StringBuilder();

            //-- First set of results --

            SqlStatement.Append("SELECT TOP 1 * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category WHERE CategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName));
            SqlStatement.Append("'");

            //-- Second set of results --

            SqlStatement.Append("; SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory ");
            SqlStatement.Append("LEFT OUTER JOIN ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category ");
            SqlStatement.Append("ON Category.CategoryID = Subcategory.CategoryID WHERE CategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName));
            SqlStatement.Append("'");

            if (includeHiddenSubcategories == false)
            {
                SqlStatement.Append(" AND Subcategory.Visible = '1'");
            }

            SqlStatement.Append(" ORDER BY Subcategory.OrderID Asc, Subcategory.SubcategoryName Asc");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                category = Transforms.Transforms.DataReader_to_CategoryModel(reader);
            }

            if(category != null)
            {
                category.Subcategories = new List<SubcategoryListModel>();

                if (reader.NextResult()) //<-- Move to Subcategory results (if any exist)
                {
                    while (reader.Read())
                    {
                        category.Subcategories.Add(Transforms.Transforms.DataReader_to_SubcategoryModel_List(reader, category.CategoryNameKey));
                    }
                }
            }

            sqlCommand.Connection.Close();

            return category;
        }

        #endregion

        #region Subcategories
        /*
        internal static int SelectSubcategoryCount(string sqlPartition, string schemaId)
        {
            int response = 0;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT COUNT(*) FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            response = (int)sqlCommand.ExecuteScalarWithRetry();

            sqlCommand.Connection.Close();

            return response;
        }
        */
        internal static bool SubcategoryNameExistsElsewhere(string sqlPartition, string schemaId, string categotyId, string subcategoryId, string nameToCheck)
        {
            bool nameExistsElsewhere = false;
            List<Guid> Ids = new List<Guid>();
            
            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT SubcategoryID FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory WHERE SubcategoryNameKey = '");
            SqlStatement.Append(Common.Methods.ObjectNames.ConvertToObjectNameKey(nameToCheck));
            SqlStatement.Append("' ");
            SqlStatement.Append("And CategoryID = '");
            SqlStatement.Append(categotyId);
            SqlStatement.Append("'");

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while(reader.Read())
            {
                Ids.Add((Guid)reader["SubcategoryID"]);
            }

            foreach(Guid id in Ids)
            {
                if(id.ToString().ToLower() != subcategoryId.ToLower())
                {
                    nameExistsElsewhere = true;
                }
            }

            sqlCommand.Connection.Close();

            return nameExistsElsewhere;

        }

        internal static SubcategoryModel SelectSubcategoryById(string sqlPartition, string schemaId, string subcategoryId, bool includeHidden)
        {
            SubcategoryModel subcategory = null;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT TOP 1 sub.SubcategoryID, sub.SubcategoryName, sub.SubcategoryNameKey, sub.Visible, sub.OrderID, sub.CreatedDate, sub.Description, cat.CategoryID, cat.CategoryName, cat.CategoryNameKey FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory sub ");
            SqlStatement.Append("Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category cat ON cat.CategoryID = sub.CategoryID ");
            SqlStatement.Append("WHERE sub.SubcategoryID = '");
            SqlStatement.Append(subcategoryId);
            SqlStatement.Append("'");

            //-- Second set of results (Associated Category) --

            SqlStatement.Append("; SELECT TOP 1 * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category ");
            SqlStatement.Append("Left Outer Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory ");
            SqlStatement.Append("ON Category.CategoryID = Subcategory.CategoryID ");
            SqlStatement.Append("WHERE SubcategoryID = '");
            SqlStatement.Append(subcategoryId);
            SqlStatement.Append("'");

            //-- Third set of results (Associated Subsubcategories) --

            SqlStatement.Append("; SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory ");
            SqlStatement.Append("WHERE SubcategoryID = '");
            SqlStatement.Append(subcategoryId);
            SqlStatement.Append("'");
            if (includeHidden == false)
            {
                SqlStatement.Append(" AND Visible = '1'");
            }

            SqlStatement.Append(" ORDER BY OrderID Asc, SubsubcategoryName Asc");

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                subcategory = Transforms.Transforms.DataReader_to_SubcategoryModel(reader);
            }

            if(subcategory != null)
            {
                subcategory.Category = new CategoryParentModel();

                if (reader.NextResult()) //<-- Move to Category results (if any exist)
                {
                    while (reader.Read())
                    {
                        subcategory.Category = Transforms.Transforms.DataReader_to_CategoryParentModel(reader);

                        subcategory.LocationPath = subcategory.Category.CategoryNameKey;
                        subcategory.FullyQualifiedName = subcategory.LocationPath + "/" + subcategory.SubcategoryNameKey;
                    }
                }

                if (reader.NextResult()) //<-- Move to Subsubcategory results (if any exist)
                {
                    subcategory.Subsubcategories = new List<SubsubcategoryListModel>();

                    while (reader.Read())
                    {
                        subcategory.Subsubcategories.Add(Transforms.Transforms.DataReader_to_SubsubcategoryModel_List(reader, subcategory.Category.CategoryNameKey, subcategory.SubcategoryNameKey));
                    }
                }
            }

            sqlCommand.Connection.Close();

            return subcategory;
        }

        internal static SubcategoryModel SelectSubcategoryByNames(string sqlPartition, string schemaId, string categoryName, string subcategoryName, bool includeHiddenSubsubcategories)
        {
            SubcategoryModel subcategory = null;

            StringBuilder SqlStatement = new StringBuilder();

            //-- First set of results (Subcategory) --

            SqlStatement.Append("SELECT TOP 1 * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory ");
            SqlStatement.Append(" LEFT OUTER JOIN ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category ");
            SqlStatement.Append("ON Category.CategoryID = Subcategory.CategoryID WHERE CategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND SubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryName));
            SqlStatement.Append("'");

            //-- Second set of results (Associated Category) --

            SqlStatement.Append("; SELECT TOP 1 * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category WHERE CategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName));
            SqlStatement.Append("'");

            //-- Get Subcategory ID As Variable --

            SqlStatement.Append("; DECLARE @subcatid AS uniqueidentifier ");
            SqlStatement.Append("SELECT @subcatid = (SELECT TOP 1 Subcategory.SubcategoryID FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory ");
            SqlStatement.Append(" LEFT OUTER JOIN ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category ");
            SqlStatement.Append("ON Category.CategoryID = Subcategory.CategoryID WHERE CategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND SubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryName));
            SqlStatement.Append("') ");
            SqlStatement.Append("SELECT @subcatid ");

            //-- Third set of results (Associated Subsubcategories) --
            SqlStatement.Append("; SELECT subsub.* FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory subsub ");
            SqlStatement.Append("Left Outer Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory ");
            SqlStatement.Append("On Subcategory.SubcategoryID = subsub.SubcategoryID ");
            SqlStatement.Append("Where Subcategory.SubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND subsub.SubcategoryID = @subcatid");
            if (includeHiddenSubsubcategories == false)
            {
                SqlStatement.Append(" AND subsub.Visible = '1'");
            }

            SqlStatement.Append(" ORDER BY subsub.OrderID Asc, subsub.SubsubcategoryName Asc");

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                subcategory = Transforms.Transforms.DataReader_to_SubcategoryModel(reader);
            }

            if(subcategory != null)
            {
                subcategory.Category = new CategoryParentModel();

                if (reader.NextResult()) //<-- Move to Category results (if any exist)
                {
                    while (reader.Read())
                    {
                        subcategory.Category = Transforms.Transforms.DataReader_to_CategoryParentModel(reader);

                        subcategory.LocationPath = subcategory.Category.CategoryNameKey;
                        subcategory.FullyQualifiedName = subcategory.LocationPath + "/" + subcategory.SubcategoryNameKey;
                    }
                }

                if (reader.NextResult()) //<-- Move to SubcategoryID results
                {
                    //Skip this, not used
                }

                if (reader.NextResult()) //<-- Move to Subsubcategory results (if any exist)
                {
                    subcategory.Subsubcategories = new List<SubsubcategoryListModel>();

                    while (reader.Read())
                    {
                        subcategory.Subsubcategories.Add(Transforms.Transforms.DataReader_to_SubsubcategoryModel_List(reader, subcategory.Category.CategoryNameKey, subcategory.SubcategoryNameKey));
                    }
                }
            }

            sqlCommand.Connection.Close();

            return subcategory;
        }

        #endregion

        #region Subsubcategories

        /*
        internal static int SelectSubsubcategoryCount(string sqlPartition, string schemaId)
        {
            int response = 0;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT COUNT(*) FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            response = (int)sqlCommand.ExecuteScalarWithRetry();

            sqlCommand.Connection.Close();

            return response;
        }
        */

        internal static bool SubsubcategoryNameExistsElsewhere(string sqlPartition, string schemaId, string subcategoryId, string subsubcategoryId, string nameToCheck)
        {
            bool nameExistsElsewhere = false;
            List<Guid> Ids = new List<Guid>();

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT SubsubcategoryID FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory WHERE SubsubcategoryNameKey = '");
            SqlStatement.Append(Common.Methods.ObjectNames.ConvertToObjectNameKey(nameToCheck));
            SqlStatement.Append("' ");
            SqlStatement.Append("And SubcategoryID = '");
            SqlStatement.Append(subcategoryId);
            SqlStatement.Append("'");

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                Ids.Add((Guid)reader["SubsubcategoryID"]);
            }

            foreach (Guid id in Ids)
            {
                if (id.ToString().ToLower() != subsubcategoryId.ToLower())
                {
                    nameExistsElsewhere = true;
                }
            }

            sqlCommand.Connection.Close();

            return nameExistsElsewhere;

        }


        internal static SubsubcategoryModel SelectSubsubcategoryById(string sqlPartition, string schemaId, string subsubcategoryId, bool includeHidden)
        {
            SubsubcategoryModel subsubcategory = null;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT TOP 1 ");
            SqlStatement.Append("subsub.SubsubcategoryID, subsub.SubsubcategoryName, subsub.SubsubcategoryNameKey, subsub.Visible, subsub.OrderID, subsub.CreatedDate, subsub.Description, sub.SubcategoryID, sub.SubcategoryName, sub.SubcategoryNameKey, cat.CategoryID, cat.CategoryName, cat.CategoryNameKey ");
            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory subsub ");
            SqlStatement.Append("Inner Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory sub ");
            SqlStatement.Append("ON sub.SubcategoryID = subsub.SubcategoryID ");
            SqlStatement.Append("Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category cat ");
            SqlStatement.Append("ON cat.CategoryID = sub.CategoryID ");
            SqlStatement.Append("WHERE SubsubcategoryID = '");
            SqlStatement.Append(subsubcategoryId);
            SqlStatement.Append("'");

            //-- Second set of results (Associated Subsubsubategories) --
            SqlStatement.Append("; SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory ");
            SqlStatement.Append("WHERE SubsubcategoryID = '");
            SqlStatement.Append(subsubcategoryId);
            SqlStatement.Append("'");
            if (includeHidden == false)
            {
                SqlStatement.Append(" AND Visible = '1'");
            }

            SqlStatement.Append(" ORDER BY OrderID Asc, SubsubsubcategoryName Asc");


            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                subsubcategory = Transforms.Transforms.DataReader_to_SubsubcategoryModel(reader);

                if (reader.NextResult()) //<-- Move to Subsubsubcategory results (if any exist)
                {
                    subsubcategory.Subsubsubcategories = new List<SubsubsubcategoryListModel>();

                    while (reader.Read())
                    {
                        subsubcategory.Subsubsubcategories.Add(Transforms.Transforms.DataReader_to_SubsubsubcategoryModel_List(reader, subsubcategory.Category.CategoryNameKey, subsubcategory.Subcategory.SubcategoryNameKey, subsubcategory.SubsubcategoryNameKey));
                    }
                }
            }

            sqlCommand.Connection.Close();

            return subsubcategory;
        }

        internal static SubsubcategoryModel SelectSubsubcategoryByNames(string sqlPartition, string schemaId, string categoryName, string subcategoryName, string subsubcategoryName, bool includeHiddenSubsubsubcategories)
        {
            SubsubcategoryModel subsubcategory = null;

            StringBuilder SqlStatement = new StringBuilder();

            //-- First set of results (Subsubcategory) --

            SqlStatement.Append("SELECT TOP 1 ");
            SqlStatement.Append("subsub.SubsubcategoryID, subsub.SubsubcategoryName, subsub.SubsubcategoryNameKey, subsub.Visible, subsub.OrderID, subsub.CreatedDate, subsub.Description, sub.SubcategoryID, sub.SubcategoryName, sub.SubcategoryNameKey, cat.CategoryID, cat.CategoryName, cat.CategoryNameKey ");
            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory subsub ");
            SqlStatement.Append("Inner Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory sub ");
            SqlStatement.Append("ON sub.SubcategoryID = subsub.SubcategoryID ");
            SqlStatement.Append("Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category cat ");
            SqlStatement.Append("ON cat.CategoryID = sub.CategoryID ");
            SqlStatement.Append("WHERE SubsubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND SubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND CategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName));
            SqlStatement.Append("'");


            //-- Get Subsubcategory ID As Variable --

            SqlStatement.Append("; DECLARE @subsubcatid AS uniqueidentifier ");
            SqlStatement.Append("SELECT @subsubcatid = (SELECT TOP 1 subsub.SubsubcategoryID FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory subsub ");
            SqlStatement.Append("Inner Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory sub ");
            SqlStatement.Append("ON sub.SubcategoryID = subsub.SubcategoryID ");
            SqlStatement.Append("Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category cat ");
            SqlStatement.Append("ON cat.CategoryID = sub.CategoryID ");
            SqlStatement.Append("WHERE SubsubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND SubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND CategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName));
            SqlStatement.Append("') ");
            SqlStatement.Append("SELECT @subsubcatid ");


            //-- Second set of results (Associated Subsubsubategories) --
            SqlStatement.Append("; SELECT subsubsub.* FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory subsubsub ");
            SqlStatement.Append("Left Outer Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory ");
            SqlStatement.Append("On Subsubcategory.SubsubcategoryID = subsubsub.SubsubcategoryID ");
            SqlStatement.Append("Where Subsubcategory.SubsubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND subsubsub.SubsubcategoryID = @subsubcatid");
            if (includeHiddenSubsubsubcategories == false)
            {
                SqlStatement.Append(" AND subsubsub.Visible = '1'");
            }

            SqlStatement.Append(" ORDER BY subsubsub.OrderID Asc, subsubsub.SubsubsubcategoryName Asc");

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                subsubcategory = Transforms.Transforms.DataReader_to_SubsubcategoryModel(reader);
            }

            if (subsubcategory != null)
            {
                if (reader.NextResult()) //<-- Move to SubsubcategoryID results
                {
                    //Skip this, not used
                }

                if (reader.NextResult()) //<-- Move to Subsubsubcategory results (if any exist)
                {
                    subsubcategory.Subsubsubcategories = new List<SubsubsubcategoryListModel>();

                    while (reader.Read())
                    {
                        subsubcategory.Subsubsubcategories.Add(Transforms.Transforms.DataReader_to_SubsubsubcategoryModel_List(reader, subsubcategory.Category.CategoryNameKey, subsubcategory.Subcategory.SubcategoryNameKey, subsubcategory.SubsubcategoryNameKey));
                    }
                }
            }

            sqlCommand.Connection.Close();

            return subsubcategory;
        }

        #endregion

        #region Subsubsubcategories

        /*
        internal static int SelectSubsubsubcategoryCount(string sqlPartition, string schemaId)
        {
            int response = 0;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT COUNT(*) FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            response = (int)sqlCommand.ExecuteScalarWithRetry();

            sqlCommand.Connection.Close();

            return response;
        }
        */

        internal static bool SubsubsubcategoryNameExistsElsewhere(string sqlPartition, string schemaId, string subsubcategoryId, string nameToCheck)
        {
            bool nameExistsElsewhere = false;
            List<Guid> Ids = new List<Guid>();

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT SubsubsubcategoryID FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory WHERE SubsubsubcategoryNameKey = '");
            SqlStatement.Append(Common.Methods.ObjectNames.ConvertToObjectNameKey(nameToCheck));
            SqlStatement.Append("' ");
            SqlStatement.Append("And SubsubcategoryID = '");
            SqlStatement.Append(subsubcategoryId);
            SqlStatement.Append("'");

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                Ids.Add((Guid)reader["SubsubsubcategoryID"]);
            }

            foreach (Guid id in Ids)
            {
                if (id.ToString().ToLower() != subsubcategoryId.ToLower())
                {
                    nameExistsElsewhere = true;
                }
            }

            sqlCommand.Connection.Close();

            return nameExistsElsewhere;

        }


        internal static SubsubsubcategoryModel SelectSubsubsubcategoryById(string sqlPartition, string schemaId, string subsubsubcategoryId)
        {
            SubsubsubcategoryModel subsubsubcategory = null;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT TOP 1 ");
            SqlStatement.Append("subsubsub.SubsubsubcategoryID, subsubsub.SubsubsubcategoryName, subsubsub.SubsubsubcategoryNameKey, subsubsub.Visible, subsubsub.OrderID, subsubsub.CreatedDate, subsubsub.Description, subsub.SubsubcategoryID, subsub.SubsubcategoryName, subsub.SubsubcategoryNameKey, sub.SubcategoryID, sub.SubcategoryName, sub.SubcategoryNameKey , cat.CategoryID, cat.CategoryName, cat.CategoryNameKey ");

            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory subsubsub ");

            SqlStatement.Append("Inner Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory subsub ");
            SqlStatement.Append("ON subsub.SubsubcategoryID = subsubsub.SubsubcategoryID ");

            SqlStatement.Append("Inner Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory sub ");
            SqlStatement.Append("ON sub.SubcategoryID = subsub.SubcategoryID ");
            SqlStatement.Append("Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category cat ");
            SqlStatement.Append("ON cat.CategoryID = sub.CategoryID ");
            SqlStatement.Append("WHERE SubsubsubcategoryID = '");
            SqlStatement.Append(subsubsubcategoryId);
            SqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                subsubsubcategory = Transforms.Transforms.DataReader_to_SubsubsubcategoryModel(reader);
            }

            sqlCommand.Connection.Close();

            return subsubsubcategory;
        }

        internal static SubsubsubcategoryModel SelectSubsubsubcategoryByNames(string sqlPartition, string schemaId, string categoryName, string subcategoryName, string subsubcategoryName, string subsubsubcategoryName)
        {
            SubsubsubcategoryModel subsubsubcategory = null;

            StringBuilder SqlStatement = new StringBuilder();

            //-- First set of results (Subsubcategory) --
            SqlStatement.Append("SELECT TOP 1 ");
            SqlStatement.Append("subsubsub.SubsubsubcategoryID, subsubsub.SubsubsubcategoryName, subsubsub.SubsubsubcategoryNameKey, subsubsub.Visible, subsubsub.OrderID, subsubsub.CreatedDate, subsubsub.Description, subsub.SubsubcategoryID, subsub.SubsubcategoryName, subsub.SubsubcategoryNameKey, sub.SubcategoryID, sub.SubcategoryName, sub.SubcategoryNameKey , cat.CategoryID, cat.CategoryName, cat.CategoryNameKey ");

            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory subsubsub ");

            SqlStatement.Append("Inner Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory subsub ");
            SqlStatement.Append("ON subsub.SubsubcategoryID = subsubsub.SubsubcategoryID ");

            SqlStatement.Append("Inner Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory sub ");
            SqlStatement.Append("ON sub.SubcategoryID = subsub.SubcategoryID ");

            SqlStatement.Append("Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category cat ");
            SqlStatement.Append("ON cat.CategoryID = sub.CategoryID ");

            SqlStatement.Append("WHERE SubsubsubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubsubcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND SubsubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND SubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND CategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName));
            SqlStatement.Append("'");

            /*
            SqlStatement.Append("SELECT TOP 1 ");
            SqlStatement.Append("subsubsub.SubsubsubcategoryID, subsubsub.SubsubsubcategoryName, subsubsub.SubsubsubcategoryNameKey, subsubsub.Visible, subsubsub.OrderID, subsubsub.CreatedDate, subsub.SubcategoryID, subsub.SubcategoryName, subsub.SubcategoryNameKey, sub.CategoryID, sub.CategoryName, sub.CategoryNameKey , cat.CategoryID, cat.CategoryName, cat.CategoryNameKey ");
            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory subsubsub ");
            SqlStatement.Append("Inner Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory subsub ");
            SqlStatement.Append("Inner Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory sub ");
            SqlStatement.Append("ON sub.SubcategoryID = subsub.SubcategoryID ");
            SqlStatement.Append("Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category cat ");
            SqlStatement.Append("ON cat.CategoryID = sub.CategoryID ");
            SqlStatement.Append("WHERE SubsubsubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubsubcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND SubsubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subsubcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND SubcategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(subcategoryName));
            SqlStatement.Append("' ");
            SqlStatement.Append("AND CategoryNameKey = '");
            SqlStatement.Append(Sahara.Core.Common.Methods.ObjectNames.ConvertToObjectNameKey(categoryName));
            SqlStatement.Append("'");
            */

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                subsubsubcategory = Transforms.Transforms.DataReader_to_SubsubsubcategoryModel(reader);
            }

            sqlCommand.Connection.Close();

            return subsubsubcategory;
        }

        #endregion

        #region Shared

        
        internal static int SelectCategorizationCount(string sqlPartition, string schemaId)
        {
            int response = 0;

            StringBuilder SqlStatement = new StringBuilder();



            SqlStatement.Append("SELECT SUM(amount) FROM (");

            //1.
            SqlStatement.Append("SELECT COUNT(*) amount FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category");

            SqlStatement.Append(" UNION ALL ");

            //2.
            SqlStatement.Append("SELECT COUNT(*) amount FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subcategory");

            SqlStatement.Append(" UNION ALL ");

            //3.
            SqlStatement.Append("SELECT COUNT(*) amount FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubcategory");

            SqlStatement.Append(" UNION ALL ");

            //4.
            SqlStatement.Append("SELECT COUNT(*) amount FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Subsubsubcategory");

            SqlStatement.Append(") AS value");



            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();


            response = (int)sqlCommand.ExecuteScalarWithRetry();


            sqlCommand.Connection.Close();

            return response;
        }

        #endregion
    }
}
