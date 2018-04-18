using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Sahara.Core.Application.Images.Formats.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Formats.Sql.Statements
{
    public static class InsertStatements
    {
        internal static DataAccessResponseType InsertImageGroup(string sqlPartition, string schemaId, ImageFormatGroupModel imageGroup)//, int maxAllowed) //<-- Removed, now checked above AFTER removing non custom types
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statements =============================================================

            //Check Row Count ===========================================================
            //SqlStatement.Append("DECLARE @ObjectCount INT ");
            /*
            SqlStatement.Append("SET @ObjectCount = (SELECT COUNT(*) ");
            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".ImageGroup) ");
            SqlStatement.Append("IF @ObjectCount < '");
            SqlStatement.Append(maxAllowed);
            SqlStatement.Append("' ");
            SqlStatement.Append("BEGIN ");
            */

            //INSERT =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".ImageGroup (");

            SqlStatement.Append("ImageGroupTypeNameKey,");
            SqlStatement.Append("ImageGroupID,");
            SqlStatement.Append("ImageGroupName,");
            SqlStatement.Append("ImageGroupNameKey, ");
            SqlStatement.Append("CreatedDate");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@ImageGroupTypeNameKey, ");
            SqlStatement.Append("@ImageGroupID, ");
            SqlStatement.Append("@ImageGroupName, ");
            SqlStatement.Append("@ImageGroupNameKey, ");
            SqlStatement.Append("@CreatedDate");

            SqlStatement.Append(")");

            //CLOSE: Check Row Count ===========================================================
            //SqlStatement.Append(" END");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();



            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@ImageGroupTypeNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@ImageGroupID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@ImageGroupName", SqlDbType.Text);
            sqlCommand.Parameters.Add("@ImageGroupNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);

            //Assign values
            sqlCommand.Parameters["@ImageGroupTypeNameKey"].Value = imageGroup.ImageFormatGroupTypeNameKey;
            sqlCommand.Parameters["@ImageGroupID"].Value = imageGroup.ImageFormatGroupID;
            sqlCommand.Parameters["@ImageGroupName"].Value = imageGroup.ImageFormatGroupName;
            sqlCommand.Parameters["@ImageGroupNameKey"].Value = imageGroup.ImageFormatGroupNameKey;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;


            // Add output parameters
            //SqlParameter objectCount = sqlCommand.Parameters.Add("@ObjectCount", SqlDbType.Int);
            //objectCount.Direction = ParameterDirection.Output;

            int insertResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if (insertResult > 0)
                {
                    response.isSuccess = true;
                }
                else
                {
                    /*
                    if ((int)objectCount.Value >= maxAllowed)
                    {
                        return new DataAccessResponseType
                        {
                            isSuccess = false,
                            ErrorMessage = "Your plan does not allow for more than " + maxAllowed + " image groups. Please upgrade to increase your limits."
                            //ErrorMessage = "You have reached the maximum amount of categories for your account. Please upgrade your plan or contact support to increase your limits."
                        };
                    }*/
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert an image group into SQL",
                    System.Reflection.MethodBase.GetCurrentMethod()
                );

                response.isSuccess = false;
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();

            return response;
        }

        internal static DataAccessResponseType InsertImageFormat(string sqlPartition, string schemaId, ImageFormatModel imageFormat)//, int maxAllowed) //<-- Removed, now checked above AFTER removing non custom types
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statements =============================================================

            //Check Row Count ===========================================================
            //SqlStatement.Append("DECLARE @ObjectCount INT ");
            /*SqlStatement.Append("SET @ObjectCount = (SELECT COUNT(*) ");
            SqlStatement.Append("FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".ImageFormat) ");
            SqlStatement.Append("IF @ObjectCount < '");
            SqlStatement.Append(maxAllowed);
            SqlStatement.Append("' ");
            SqlStatement.Append("BEGIN ");*/

            //INSERT =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".ImageFormat (");

            SqlStatement.Append("ImageGroupTypeNameKey,");
            SqlStatement.Append("ImageGroupNameKey, ");
            SqlStatement.Append("ImageFormatID,");
            SqlStatement.Append("ImageFormatName,");
            SqlStatement.Append("ImageFormatNameKey,");
            SqlStatement.Append("Height,");
            SqlStatement.Append("Width,");
            SqlStatement.Append("Gallery,");
            SqlStatement.Append("Listing,");
            SqlStatement.Append("CreatedDate");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@ImageGroupTypeNameKey, ");
            SqlStatement.Append("@ImageGroupNameKey, ");
            SqlStatement.Append("@ImageFormatID, ");
            SqlStatement.Append("@ImageFormatName, ");
            SqlStatement.Append("@ImageFormatNameKey, ");
            SqlStatement.Append("@Height, ");
            SqlStatement.Append("@Width, ");
            SqlStatement.Append("@Gallery, ");
            SqlStatement.Append("@Listing, ");
            SqlStatement.Append("@CreatedDate");

            SqlStatement.Append(")");

            //CLOSE: Check Row Count ===========================================================
            //SqlStatement.Append(" END");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();



            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@ImageGroupTypeNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@ImageGroupNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@ImageFormatID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@ImageFormatName", SqlDbType.Text);
            sqlCommand.Parameters.Add("@ImageFormatNameKey", SqlDbType.Text);
            sqlCommand.Parameters.Add("@Height", SqlDbType.Int);
            sqlCommand.Parameters.Add("@Width", SqlDbType.Int);
            sqlCommand.Parameters.Add("@Gallery", SqlDbType.Bit);
            sqlCommand.Parameters.Add("@Listing", SqlDbType.Bit);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);

            //Assign values
            sqlCommand.Parameters["@ImageGroupTypeNameKey"].Value = imageFormat.ImageFormatGroupTypeNameKey;
            sqlCommand.Parameters["@ImageGroupNameKey"].Value = imageFormat.ImageFormatGroupNameKey;
            sqlCommand.Parameters["@ImageFormatID"].Value = imageFormat.ImageFormatID;
            sqlCommand.Parameters["@ImageFormatName"].Value = imageFormat.ImageFormatName;
            sqlCommand.Parameters["@ImageFormatNameKey"].Value = imageFormat.ImageFormatNameKey;
            sqlCommand.Parameters["@Height"].Value = imageFormat.Height;
            sqlCommand.Parameters["@Width"].Value = imageFormat.Width;
            sqlCommand.Parameters["@Gallery"].Value = imageFormat.Gallery;
            sqlCommand.Parameters["@Listing"].Value = imageFormat.Listing;
            sqlCommand.Parameters["@CreatedDate"].Value = DateTime.UtcNow;


            // Add output parameters
            //SqlParameter objectCount = sqlCommand.Parameters.Add("@ObjectCount", SqlDbType.Int);
            //objectCount.Direction = ParameterDirection.Output;

            int insertResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if (insertResult > 0)
                {
                    response.isSuccess = true;
                }
                else
                {
                    /*
                    if ((int)objectCount.Value >= maxAllowed)
                    {
                        return new DataAccessResponseType
                        {
                            isSuccess = false,
                            ErrorMessage = "Your plan does not allow for more than " + maxAllowed + " image formats. Please upgrade to increase your limits."
                            //ErrorMessage = "You have reached the maximum amount of categories for your account. Please upgrade your plan or contact support to increase your limits."
                        };
                    }*/
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert an image format into SQL",
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
