using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.Images.Formats.Sql.Statements
{
    internal static class BoolStatements
    {
        /* Depricated in favor of pulling a copy of the entire format
        internal static bool IsImageFormatDeletionAllowed(string sqlPartition, string schemaId, string imageFormatId)
        {
            bool allowDeletion = false;

            StringBuilder SqlStatement = new StringBuilder();

            //First set of results (groups)
            SqlStatement.Append("SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".ImageFormat ");
            SqlStatement.Append("WHERE ImageFormatID = '");
            SqlStatement.Append(imageFormatId);
            SqlStatement.Append("'");

            //var sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                allowDeletion = (Boolean)reader["AllowDeletion"];
            }

            sqlCommand.Connection.Close();

            return allowDeletion;
        }*/
    }
}
