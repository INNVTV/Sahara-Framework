using Sahara.Core.Platform.Partitioning.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Platform.Partitioning.Sql
{
    internal static class Transforms
    {

        #region SQL Partition Objects

        public static SqlSchemaLog DataReader_to_SqlSchemaLog(SqlDataReader reader)
        {
            SqlSchemaLog sqlSchemaLog = new SqlSchemaLog();

            sqlSchemaLog.Version = String.Format("{0:0.0}", (decimal)reader["Version"]);
            sqlSchemaLog.Description = (String)reader["Description"];
            sqlSchemaLog.InstallDate = (DateTime)reader["InstallDate"];

            return sqlSchemaLog;
        }

        #endregion


    }
}
