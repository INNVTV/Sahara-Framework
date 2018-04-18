using Sahara.Core.Application.Categorization.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Categorization.Sql.Transforms
{
    internal static class Transforms
    {
        public static CategoryModel DataReader_to_CategoryModel(SqlDataReader reader)
        {
            CategoryModel category = new CategoryModel();

            category.CategoryID = (Guid)reader["CategoryID"];
            category.CategoryName = (String)reader["CategoryName"];
            category.CategoryNameKey = (String)reader["CategoryNameKey"];
            category.LocationPath = "";
            category.FullyQualifiedName = (String)reader["CategoryNameKey"];
            category.OrderID = (int)reader["OrderID"];
            category.Visible = (bool)reader["Visible"];
            category.CreatedDate = (DateTime)reader["CreatedDate"];
            category.Description = (String)reader["Description"];
            

            return category;
        }

        public static CategoryParentModel DataReader_to_CategoryParentModel(SqlDataReader reader)
        {
            CategoryParentModel category = new CategoryParentModel();

            category.CategoryID = (Guid)reader["CategoryID"];
            category.CategoryName = (String)reader["CategoryName"];
            category.CategoryNameKey = (String)reader["CategoryNameKey"];
            category.LocationPath = "";
            category.FullyQualifiedName = (String)reader["CategoryNameKey"];
            category.Visible = (bool)reader["Visible"];
            return category;
        }

        public static SubcategoryListModel DataReader_to_SubcategoryModel_List(SqlDataReader reader, string categoryNameKey)
        {
            SubcategoryListModel subcategory = new SubcategoryListModel();

            subcategory.SubcategoryID = (Guid)reader["SubcategoryID"];
            subcategory.SubcategoryName = (String)reader["SubcategoryName"];
            subcategory.SubcategoryNameKey = (String)reader["SubcategoryNameKey"];
            subcategory.OrderID = (int)reader["OrderID"];
            subcategory.Visible = (bool)reader["Visible"];

            subcategory.LocationPath = categoryNameKey;
            subcategory.FullyQualifiedName = categoryNameKey + "/" + subcategory.SubcategoryNameKey;

            return subcategory;
        }

        public static SubcategoryModel DataReader_to_SubcategoryModel(SqlDataReader reader)
        {
            SubcategoryModel subcategory = new SubcategoryModel();

            subcategory.Category = new CategoryParentModel
            {
                CategoryID = (Guid)reader["CategoryID"],
                CategoryName = (String)reader["CategoryName"],
                CategoryNameKey = (String)reader["CategoryNameKey"],
                FullyQualifiedName = (String)reader["CategoryNameKey"],
                LocationPath = "/"

            };

            subcategory.SubcategoryID = (Guid)reader["SubcategoryID"];
            subcategory.CategoryID = (Guid)reader["CategoryID"];
            subcategory.SubcategoryName = (String)reader["SubcategoryName"];
            subcategory.SubcategoryNameKey = (String)reader["SubcategoryNameKey"];
            subcategory.OrderID = (int)reader["OrderID"];
            subcategory.Visible = (bool)reader["Visible"];
            subcategory.CreatedDate = (DateTime)reader["CreatedDate"];
            subcategory.Description = (String)reader["Description"];


            return subcategory;
        }

        public static SubsubcategoryListModel DataReader_to_SubsubcategoryModel_List(SqlDataReader reader, string categoryNameKey, string subcategoryNameKey)
        {
            SubsubcategoryListModel subsubcategory = new SubsubcategoryListModel();

            subsubcategory.SubsubcategoryID = (Guid)reader["SubsubcategoryID"];
            subsubcategory.SubsubcategoryName = (String)reader["SubsubcategoryName"];
            subsubcategory.SubsubcategoryNameKey = (String)reader["SubsubcategoryNameKey"];
            subsubcategory.OrderID = (int)reader["OrderID"];
            subsubcategory.Visible = (bool)reader["Visible"];

            subsubcategory.LocationPath = categoryNameKey + "/" + subcategoryNameKey;
            subsubcategory.FullyQualifiedName = subsubcategory.LocationPath + "/" + subsubcategory.SubsubcategoryNameKey;

            return subsubcategory;
        }

        public static SubsubcategoryModel DataReader_to_SubsubcategoryModel(SqlDataReader reader)
        {
            SubsubcategoryModel subsubcategory = new SubsubcategoryModel();

            subsubcategory.SubsubcategoryID = (Guid)reader["SubsubcategoryID"];
            subsubcategory.SubcategoryID = (Guid)reader["SubcategoryID"];
            subsubcategory.CategoryID = (Guid)reader["CategoryID"];
            subsubcategory.SubsubcategoryName = (String)reader["SubsubcategoryName"];
            subsubcategory.SubsubcategoryNameKey = (String)reader["SubsubcategoryNameKey"];
            subsubcategory.OrderID = (int)reader["OrderID"];
            subsubcategory.Visible = (bool)reader["Visible"];
            subsubcategory.CreatedDate = (DateTime)reader["CreatedDate"];
            subsubcategory.Description = (String)reader["Description"];

            subsubcategory.Category = new CategoryParentModel
            {
                CategoryID = (Guid)reader["CategoryID"],
                CategoryName = (String)reader["CategoryName"],
                CategoryNameKey = (String)reader["CategoryNameKey"],
                FullyQualifiedName = (String)reader["CategoryNameKey"],
                LocationPath = "/"

            };

            subsubcategory.Subcategory = new SubcategoryParentModel
            {
                SubcategoryID = (Guid)reader["SubcategoryID"],
                SubcategoryName = (String)reader["SubcategoryName"],
                SubcategoryNameKey = (String)reader["SubcategoryNameKey"],
                FullyQualifiedName = subsubcategory.Category.CategoryNameKey + "/" + (String)reader["SubcategoryNameKey"],
                LocationPath = subsubcategory.Category.CategoryNameKey
            };

            subsubcategory.LocationPath = subsubcategory.Category.CategoryNameKey + "/" + subsubcategory.Subcategory.SubcategoryNameKey;
            subsubcategory.FullyQualifiedName = subsubcategory.LocationPath + "/" + subsubcategory.SubsubcategoryNameKey;

            return subsubcategory;
        }

        public static SubsubsubcategoryListModel DataReader_to_SubsubsubcategoryModel_List(SqlDataReader reader, string categoryNameKey, string subcategoryNameKey, string subsubcategoryNameKey)
        {
            SubsubsubcategoryListModel subsubsubcategory = new SubsubsubcategoryListModel();

            subsubsubcategory.SubsubsubcategoryID = (Guid)reader["SubsubsubcategoryID"];
            subsubsubcategory.SubsubsubcategoryName = (String)reader["SubsubsubcategoryName"];
            subsubsubcategory.SubsubsubcategoryNameKey = (String)reader["SubsubsubcategoryNameKey"];
            subsubsubcategory.OrderID = (int)reader["OrderID"];
            subsubsubcategory.Visible = (bool)reader["Visible"];

            subsubsubcategory.LocationPath = categoryNameKey + "/" + subcategoryNameKey + "/" + subsubcategoryNameKey;
            subsubsubcategory.FullyQualifiedName = subsubsubcategory.LocationPath + "/" + subsubsubcategory.SubsubsubcategoryNameKey;

            return subsubsubcategory;
        }

        public static SubsubsubcategoryModel DataReader_to_SubsubsubcategoryModel(SqlDataReader reader)
        {
            SubsubsubcategoryModel subsubsubcategory = new SubsubsubcategoryModel();

            subsubsubcategory.SubsubsubcategoryID = (Guid)reader["SubsubsubcategoryID"];
            subsubsubcategory.SubsubcategoryID = (Guid)reader["SubsubcategoryID"];
            subsubsubcategory.SubcategoryID = (Guid)reader["SubcategoryID"];
            subsubsubcategory.CategoryID = (Guid)reader["CategoryID"];
            subsubsubcategory.SubsubsubcategoryName = (String)reader["SubsubsubcategoryName"];
            subsubsubcategory.SubsubsubcategoryNameKey = (String)reader["SubsubsubcategoryNameKey"];
            subsubsubcategory.Description = (String)reader["Description"];

            subsubsubcategory.OrderID = (int)reader["OrderID"];
            subsubsubcategory.Visible = (bool)reader["Visible"];
            subsubsubcategory.CreatedDate = (DateTime)reader["CreatedDate"];

            subsubsubcategory.Category = new CategoryParentModel
            {
                CategoryID = (Guid)reader["CategoryID"],
                CategoryName = (String)reader["CategoryName"],
                CategoryNameKey = (String)reader["CategoryNameKey"],
                FullyQualifiedName = (String)reader["CategoryNameKey"],
                LocationPath = "/"
            };

            subsubsubcategory.Subcategory = new SubcategoryParentModel
            {
                SubcategoryID = (Guid)reader["SubcategoryID"],
                SubcategoryName = (String)reader["SubcategoryName"],
                SubcategoryNameKey = (String)reader["SubcategoryNameKey"],
                FullyQualifiedName = subsubsubcategory.Category.CategoryNameKey + "/" + (String)reader["SubcategoryNameKey"],
                LocationPath = subsubsubcategory.Category.CategoryNameKey
            };

            subsubsubcategory.Subsubcategory = new SubsubcategoryParentModel
            {
                SubsubcategoryID = (Guid)reader["SubsubcategoryID"],
                SubsubcategoryName = (String)reader["SubsubcategoryName"],
                SubsubcategoryNameKey = (String)reader["SubsubcategoryNameKey"],
                FullyQualifiedName = subsubsubcategory.Category.CategoryNameKey + "/" + subsubsubcategory.Subcategory.SubcategoryNameKey +  "/" + (String)reader["SubsubcategoryName"],
                LocationPath = subsubsubcategory.Category.CategoryNameKey + "/" + subsubsubcategory.Subcategory.SubcategoryNameKey
            };

            subsubsubcategory.LocationPath = subsubsubcategory.Category.CategoryNameKey + "/" + subsubsubcategory.Subcategory.SubcategoryNameKey + "/" + subsubsubcategory.Subsubcategory.SubsubcategoryNameKey;
            subsubsubcategory.FullyQualifiedName = subsubsubcategory.LocationPath + "/" + subsubsubcategory.SubsubsubcategoryNameKey;

            return subsubsubcategory;
        }

    }
}
