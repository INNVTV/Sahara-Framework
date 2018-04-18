using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Sahara.Core.Application.Properties.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties.Sql.Statements
{
    internal static class SelectStatements
    {
        internal static List<PropertyTypeModel> SelectPropertyTypeList()
        {
            var propertyTypes = new List<PropertyTypeModel>();

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT * FROM PropertyType ORDER BY OrderID Asc");

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                propertyTypes.Add(Transforms.DataReader_to_PropertyTypeModel(reader));
            }

            sqlCommand.Connection.Close();

            return propertyTypes;
        }


        internal static int SelectPropertyCount(string sqlPartition, string schemaId)
        {
            int response = 0;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT COUNT(*) FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Property");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            response = (int)sqlCommand.ExecuteScalarWithRetry();

            sqlCommand.Connection.Close();

            return response;
        }

        internal static PropertyModel SelectProperty(string sqlPartition, string schemaId, string propertyName)
        {
            PropertyModel property = null;

            StringBuilder SqlStatement = new StringBuilder();

            //-- First set of results --

            SqlStatement.Append("SELECT Top 1 * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Property ");
            SqlStatement.Append("Where PropertyNameKey = '");
            SqlStatement.Append(Common.Methods.ObjectNames.ConvertToObjectNameKey(propertyName));
            SqlStatement.Append("'; ");

            //-- Second set of results --

            SqlStatement.Append("SELECT PropertyValue.PropertyValueID, PropertyValue.PropertyValueName, PropertyValue.PropertyValueNameKey, PropertyValue.PropertyID, PropertyValue.OrderID, PropertyValue.Visible, PropertyValue.CreatedDate FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".PropertyValue ");
            SqlStatement.Append("Left Outer Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Property ");
            SqlStatement.Append("ON Property.PropertyID = PropertyValue.PropertyID ");
            SqlStatement.Append("Where PropertyNameKey = '");
            SqlStatement.Append(Common.Methods.ObjectNames.ConvertToObjectNameKey(propertyName));
            SqlStatement.Append("' ORDER BY PropertyValue.OrderID Asc, PropertyValue.PropertyValueNameKey Asc; ");

            //-- Third set of results --

            SqlStatement.Append("SELECT PropertySwatch.PropertySwatchID, PropertySwatch.PropertySwatchLabel, PropertySwatch.PropertySwatchNameKey, PropertySwatch.PropertyID, PropertySwatch.PropertySwatchImage, PropertySwatch.PropertySwatchImageMedium, PropertySwatch.PropertySwatchImageSmall, PropertySwatch.OrderID, PropertySwatch.Visible, PropertySwatch.CreatedDate FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".PropertySwatch ");
            SqlStatement.Append("Left Outer Join ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Property ");
            SqlStatement.Append("ON Property.PropertyID = PropertySwatch.PropertyID ");
            SqlStatement.Append("Where PropertyNameKey = '");
            SqlStatement.Append(Common.Methods.ObjectNames.ConvertToObjectNameKey(propertyName));
            SqlStatement.Append("' ORDER BY PropertySwatch.OrderID Asc, PropertySwatch.PropertySwatchNameKey Asc");
            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                property = Transforms.DataReader_to_PropertyModel(reader);
            }

            if(property != null)
            {
                property.Values = new List<PropertyValueModel>();

                if (reader.NextResult()) //<-- Move to Values results (if any exist)
                {
                    while (reader.Read())
                    {
                        property.Values.Add(Transforms.DataReader_to_PropertyValueModel(reader));
                    }
                }

                property.Swatches = new List<PropertySwatchModel>();

                if (reader.NextResult()) //<-- Move to Swatches results (if any exist)
                {
                    while (reader.Read())
                    {
                        property.Swatches.Add(Transforms.DataReader_to_PropertySwatchModel(reader));
                    }
                }
            }

            sqlCommand.Connection.Close();

            return property;
        }

        /*
        internal static PropertyValueModel SelectPropertyValue(string sqlPartition, string schemaId, string propertyId, string propertyValueName)
        {
            var propertyValue = new PropertyValueModel();

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT Top 1 * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".PropertyValue ");
            SqlStatement.Append("Where PropertyID = '");
            SqlStatement.Append(propertyId);
            SqlStatement.Append("' ");
            SqlStatement.Append("And PropertyValueNameKey = '");
            SqlStatement.Append(Common.Methods.ObjectNames.ConvertToObjectNameKey(propertyValueName));
            SqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                propertyValue = Transforms.DataReader_to_PropertyValueModel(reader);
            }

            sqlCommand.Connection.Close();

            return propertyValue;
        }*/

        internal static List<PropertyModel> SelectPropertyList(string sqlPartition, string schemaId)
        {
            var properties = new List<PropertyModel>();
            var propertyValues = new List<PropertyValueModel>(); //<-- Placeholder for all values
            var propertySwatches = new List<PropertySwatchModel>(); //<-- Placeholder for all swatches

            StringBuilder SqlStatement = new StringBuilder();

            //-- First set of results --

            SqlStatement.Append("SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Property");

            SqlStatement.Append(" ORDER BY OrderID Asc, PropertyName Asc");

            //-- Second set of results --

            SqlStatement.Append("; SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".PropertyValue ");
            SqlStatement.Append(" ORDER BY OrderID Asc, PropertyValueName Asc");

            //-- Third set of results --

            SqlStatement.Append("; SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".PropertySwatch ");
            SqlStatement.Append(" ORDER BY OrderID Asc, PropertySwatchLabel Asc");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                properties.Add(Transforms.DataReader_to_PropertyModel(reader));
            }


            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    propertyValues.Add(Transforms.DataReader_to_PropertyValueModel(reader));
                }
            }

            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    propertySwatches.Add(Transforms.DataReader_to_PropertySwatchModel(reader));
                }
            }

            sqlCommand.Connection.Close();

            //After closing the connection we loop through and assign values & swatches to properties
            foreach(PropertyModel property in properties)
            {
                if(property.PropertyTypeNameKey == "predefined")
                {
                    property.Values = new List<PropertyValueModel>();

                    foreach (PropertyValueModel value in propertyValues)
                    {
                        if (property.PropertyID == value.PropertyID)
                        {
                            //Assign it to the property
                            property.Values.Add(value);                        
                        }
                    }
                }

                if (property.PropertyTypeNameKey == "swatch")
                {
                    property.Swatches = new List<PropertySwatchModel>();

                    foreach (PropertySwatchModel swatch in propertySwatches)
                    {
                        if (property.PropertyID == swatch.PropertyID)
                        {
                            //Assign it to the property
                            property.Swatches.Add(swatch);
                        }
                    }
                }

            }

            return properties;
        }

        /*
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

        

        internal static CategoryModel SelectCategoryById(string sqlPartition, string schemaId, string categoryId)
        {
            CategoryModel category = null;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT TOP 1 * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Category WHERE CategoryID = '");
            SqlStatement.Append(categoryId);
            SqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                category = Transforms.Transforms.DataReader_to_CategoryModel(reader);
            }

            sqlCommand.Connection.Close();

            return category;
        }

        internal static CategoryModel SelectCategoryByName(string sqlPartition, string schemaId, string categoryName)
        {
            CategoryModel category = null;

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT TOP 1 * FROM ");
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
                category = Transforms.Transforms.DataReader_to_CategoryModel(reader);
            }

            sqlCommand.Connection.Close();

            return category;
        }
        */
    }
}
