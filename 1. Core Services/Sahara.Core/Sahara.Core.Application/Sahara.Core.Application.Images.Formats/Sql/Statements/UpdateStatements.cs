using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sahara.Core.Application.Images.Formats.Sql.Statements
{
    /* Depricated
    internal static class UpdateStatements
    {
        internal static bool UpdateImageFormatVisibleState(string sqlPartition, string schemaId, string imageFormatId, bool isVisible)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".ImageFormat ");

            sqlStatement.Append("SET Visible = '");
            sqlStatement.Append(isVisible);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE ImageFormatID = '");
            sqlStatement.Append(imageFormatId);
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

        internal static bool UpdateImageFormatGalleryState(string sqlPartition, string schemaId, string imageFormatId, bool isGallery)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".ImageFormat ");

            sqlStatement.Append("SET Gallery = '");
            sqlStatement.Append(isGallery);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE ImageFormatID = '");
            sqlStatement.Append(imageFormatId);
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

        internal static bool UpdateImageFormatListingState(string sqlPartition, string schemaId, string imageFormatId, bool isListing)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".ImageFormat ");

            sqlStatement.Append("SET Listing = '");
            sqlStatement.Append(isListing);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE ImageFormatID = '");
            sqlStatement.Append(imageFormatId);
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

    }*/
}
