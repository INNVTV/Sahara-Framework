using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Sahara.Core.Application.ApiKeys.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.ApiKeys.Sql.Statements
{
    public static class SelectStatements
    {
        internal static List<ApiKeyModel> SelectApiKeysList(string sqlPartition, string schemaId)
        {
            var keys = new List<ApiKeyModel>();

            StringBuilder SqlStatement = new StringBuilder();

            //-- First set of results --

            SqlStatement.Append("SELECT * FROM ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".ApiKeys Order By Name ASC");

            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            SqlDataReader reader = sqlCommand.ExecuteReaderWithRetry();

            while (reader.Read())
            {
                keys.Add(Transforms.DataReader_to_ApiKeyModel(reader));
            }

            sqlCommand.Connection.Close();

            return keys;
        }
    }
}
