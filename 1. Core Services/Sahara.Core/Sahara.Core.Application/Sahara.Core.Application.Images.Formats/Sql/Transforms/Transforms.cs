
using Sahara.Core.Application.Images.Formats.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Formats.Sql
{
    public static class Transforms
    {
        public static ImageFormatGroupTypeModel DataReader_to_ImageFormatGroupTypeModel(SqlDataReader reader)
        {
            ImageFormatGroupTypeModel ImageFormatGroupTypeModel = new ImageFormatGroupTypeModel();

            ImageFormatGroupTypeModel.ImageFormatGroupTypeID = (Guid)reader["ImageGroupTypeID"];
            ImageFormatGroupTypeModel.ImageFormatGroupTypeName = (String)reader["ImageGroupTypeName"];
            ImageFormatGroupTypeModel.ImageFormatGroupTypeNameKey = (String)reader["ImageGroupTypeNameKey"];
            
            return ImageFormatGroupTypeModel;
        }

        public static ImageFormatGroupModel DataReader_to_ImageGroupModel(SqlDataReader reader)
        {
            ImageFormatGroupModel imageGroupModel = new ImageFormatGroupModel();

            imageGroupModel.ImageFormatGroupTypeNameKey = (String)reader["ImageGroupTypeNameKey"];

            imageGroupModel.ImageFormatGroupID = (Guid)reader["ImageGroupID"];
            imageGroupModel.ImageFormatGroupName = (String)reader["ImageGroupName"];
            imageGroupModel.ImageFormatGroupNameKey = (String)reader["ImageGroupNameKey"];

            imageGroupModel.AllowDeletion = (bool)reader["AllowDeletion"];

            imageGroupModel.Visible = (bool)reader["Visible"];

            imageGroupModel.ImageFormats = new List<ImageFormatModel>();

            return imageGroupModel;
        }

        public static ImageFormatModel DataReader_to_ImageFormatModel(SqlDataReader reader)
        {
            ImageFormatModel imageFormatModel = new ImageFormatModel();

            imageFormatModel.ImageFormatGroupNameKey = (String)reader["ImageGroupNameKey"];
            imageFormatModel.ImageFormatGroupTypeNameKey = (String)reader["ImageGroupTypeNameKey"];

            imageFormatModel.ImageFormatID = (Guid)reader["ImageFormatID"];
            imageFormatModel.ImageFormatName = (String)reader["ImageFormatName"];
            imageFormatModel.ImageFormatNameKey = (String)reader["ImageFormatNameKey"];

            imageFormatModel.Height = (int)reader["Height"];
            imageFormatModel.Width = (int)reader["Width"];

            imageFormatModel.OrderID = (int)reader["OrderID"];

            imageFormatModel.Listing = (bool)reader["Listing"];
            imageFormatModel.Gallery = (bool)reader["Gallery"];
            imageFormatModel.Visible = (bool)reader["Visible"];
            imageFormatModel.AllowDeletion = (bool)reader["AllowDeletion"];

            return imageFormatModel;
        }
    }
}
