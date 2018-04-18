using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Sahara.Core.Application.Properties.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Properties.Sql.Statements
{
    internal static class InsertStatements
    {
        public static DataAccessResponseType InsertProperty(string sqlPartition, string schemaId, PropertyModel property)
        {
            bool facetable = property.Facetable;

            if (property.PropertyTypeNameKey == "datetime")
            {
                //Default setting for dateime facetability is false
                facetable = false;
            }

            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statement =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".Property (");

            SqlStatement.Append("PropertyTypeNameKey,");
            SqlStatement.Append("PropertyID,");
            SqlStatement.Append("PropertyName,");
            SqlStatement.Append("PropertyNameKey,");
            SqlStatement.Append("SearchFieldName,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("Facetable");
            //SqlStatement.Append("Visible");


            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@PropertyTypeNameKey, ");
            SqlStatement.Append("@PropertyID, ");
            SqlStatement.Append("@PropertyName, ");
            SqlStatement.Append("@PropertyNameKey, ");
            SqlStatement.Append("@SearchFieldName, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@Facetable");
            //SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();



            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@PropertyTypeNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@PropertyID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@PropertyName", SqlDbType.Text);
            sqlCommand.Parameters.Add("@PropertyNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@SearchFieldName", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            sqlCommand.Parameters.Add("@Facetable", SqlDbType.Bit);
            //sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            sqlCommand.Parameters["@PropertyTypeNameKey"].Value = property.PropertyTypeNameKey;
            sqlCommand.Parameters["@PropertyID"].Value = property.PropertyID;
            sqlCommand.Parameters["@PropertyName"].Value = property.PropertyName;
            sqlCommand.Parameters["@PropertyNameKey"].Value = property.PropertyNameKey;
            sqlCommand.Parameters["@SearchFieldName"].Value = property.SearchFieldName;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            sqlCommand.Parameters["@Facetable"].Value = facetable;
            //sqlCommand.Parameters["@Visible"].Value = property.Visible;

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
                    "attempting to insert a property into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }

        public static DataAccessResponseType InsertPropertyValue(string sqlPartition, string schemaId, PropertyValueModel propertyValue)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statement =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".PropertyValue (");

            SqlStatement.Append("PropertyID,");
            SqlStatement.Append("PropertyValueID,");
            SqlStatement.Append("PropertyValueName,");
            SqlStatement.Append("PropertyValueNameKey,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("Visible");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@PropertyID, ");
            SqlStatement.Append("@PropertyValueID, ");
            SqlStatement.Append("@PropertyValueName, ");
            SqlStatement.Append("@PropertyValueNameKey, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();




            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@PropertyID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@PropertyValueID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@PropertyValueName", SqlDbType.Text);
            sqlCommand.Parameters.Add("@PropertyValueNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            sqlCommand.Parameters["@PropertyID"].Value = propertyValue.PropertyID;
            sqlCommand.Parameters["@PropertyValueID"].Value = propertyValue.PropertyValueID;
            sqlCommand.Parameters["@PropertyValueName"].Value = propertyValue.PropertyValueName;
            sqlCommand.Parameters["@PropertyValueNameKey"].Value = propertyValue.PropertyValueNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            sqlCommand.Parameters["@Visible"].Value = propertyValue.Visible;

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
                    "attempting to insert a property value into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }

        public static DataAccessResponseType InsertPropertySwatch(string sqlPartition, string schemaId, PropertySwatchModel propertySwatch)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statement =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".PropertySwatch (");

            SqlStatement.Append("PropertyID,");
            SqlStatement.Append("PropertySwatchID,");
            SqlStatement.Append("PropertySwatchImage,");
            SqlStatement.Append("PropertySwatchImageMedium,");
            SqlStatement.Append("PropertySwatchImageSmall,");
            SqlStatement.Append("PropertySwatchLabel,");
            SqlStatement.Append("PropertySwatchNameKey,");
            SqlStatement.Append("CreatedDate, ");
            SqlStatement.Append("Visible");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@PropertyID, ");
            SqlStatement.Append("@PropertySwatchID, ");
            SqlStatement.Append("@PropertySwatchImage, ");
            SqlStatement.Append("@PropertySwatchImageMedium, ");
            SqlStatement.Append("@PropertySwatchImageSmall, ");
            SqlStatement.Append("@PropertySwatchLabel, ");
            SqlStatement.Append("@PropertySwatchNameKey, ");
            SqlStatement.Append("@CreatedDate, ");
            SqlStatement.Append("@Visible");

            SqlStatement.Append(")");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();




            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@PropertyID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@PropertySwatchID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@PropertySwatchImage", SqlDbType.Text);
            sqlCommand.Parameters.Add("@PropertySwatchImageMedium", SqlDbType.Text);
            sqlCommand.Parameters.Add("@PropertySwatchImageSmall", SqlDbType.Text);
            sqlCommand.Parameters.Add("@PropertySwatchLabel", SqlDbType.Text);
            sqlCommand.Parameters.Add("@PropertySwatchNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            //Assign values
            sqlCommand.Parameters["@PropertyID"].Value = propertySwatch.PropertyID;
            sqlCommand.Parameters["@PropertySwatchID"].Value = propertySwatch.PropertySwatchID;
            sqlCommand.Parameters["@PropertySwatchImage"].Value = propertySwatch.PropertySwatchImage;
            sqlCommand.Parameters["@PropertySwatchImageMedium"].Value = propertySwatch.PropertySwatchImageMedium;
            sqlCommand.Parameters["@PropertySwatchImageSmall"].Value = propertySwatch.PropertySwatchImageSmall;
            sqlCommand.Parameters["@PropertySwatchLabel"].Value = propertySwatch.PropertySwatchLabel;
            sqlCommand.Parameters["@PropertySwatchNameKey"].Value = propertySwatch.PropertySwatchNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;
            sqlCommand.Parameters["@Visible"].Value = propertySwatch.Visible;

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
                    "attempting to insert a property value into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }
    }
}
