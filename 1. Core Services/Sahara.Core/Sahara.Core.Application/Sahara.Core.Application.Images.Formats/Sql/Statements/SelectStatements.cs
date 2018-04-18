using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Sahara.Core.Application.Images.Formats.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Formats.Sql.Statements
{
    internal static class SelectStatements
    {
        internal static List<ImageFormatGroupTypeModel> SelectImageGroupTypeList()
        {
            var imageGroupTypes = new List<ImageFormatGroupTypeModel>();

            StringBuilder SqlStatement = new StringBuilder();

            SqlStatement.Append("SELECT * FROM ImageGroupType ORDER BY ImageGroupTypeName Asc");

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();


            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                imageGroupTypes.Add(Transforms.DataReader_to_ImageFormatGroupTypeModel(reader));
            }

            sqlCommand.Connection.Close();

            return imageGroupTypes;
        }

        internal static List<ImageFormatGroupModel> SelectImageFormatsByGroupType(string sqlPartition, string schemaId, string imageGroupTypeNameKey)
        {
            var imageGroups = new List<ImageFormatGroupModel>();
            var imageFormats = new List<ImageFormatModel>();

            StringBuilder SqlStatement = new StringBuilder();

            //First set of results (groups)
            SqlStatement.Append("SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".ImageGroup ");
            SqlStatement.Append("WHERE ImageGroupTypeNameKey = '");
            SqlStatement.Append(imageGroupTypeNameKey);
            SqlStatement.Append("'; ");

            //Second set of results (formats)
            SqlStatement.Append("SELECT f.*, g.ImageGroupName, g.ImageGroupID FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".ImageGroup g ");
            SqlStatement.Append("LEFT JOIN ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".ImageFormat f  ");
            SqlStatement.Append("ON f.ImageGroupNameKey = g.ImageGroupNameKey ");
            SqlStatement.Append("WHERE g.ImageGroupTypeNameKey = '");
            SqlStatement.Append(imageGroupTypeNameKey);
            SqlStatement.Append("' AND f.ImageGroupTypeNameKey = '");
            SqlStatement.Append(imageGroupTypeNameKey);
            SqlStatement.Append("' Order By f.ImageGroupNameKey, f.ImageFormatName, f.OrderID;");


            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();
            
            while (reader.Read())
            {
                imageGroups.Add(Transforms.DataReader_to_ImageGroupModel(reader));
            }


            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    imageFormats.Add(Transforms.DataReader_to_ImageFormatModel(reader));
                }
            }

            sqlCommand.Connection.Close();



            //Assign each format to the proper group before returning results
            foreach(ImageFormatModel format in imageFormats)
            {
                foreach(ImageFormatGroupModel group in imageGroups)
                {
                    if (format.ImageFormatGroupNameKey == group.ImageFormatGroupNameKey)
                    {
                        group.ImageFormats.Add(format);
                    }
                }
            }

            return imageGroups;
        }

    }
}
