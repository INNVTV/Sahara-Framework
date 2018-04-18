using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Sahara.Core.Application.ApiKeys.Models;
using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.ApiKeys.Sql.Statements
{
    public static class InsertStatements
    {
        internal static DataAccessResponseType InsertApiKey(string sqlPartition, string schemaId, ApiKeyModel key)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();

            //SQL Statements =============================================================


            //INSERT =============================================================
            SqlStatement.Append("INSERT INTO  ");
            SqlStatement.Append(schemaId);
            SqlStatement.Append(".ApiKeys (");

            SqlStatement.Append("ApiKey,");
            SqlStatement.Append("Name,");
            SqlStatement.Append("Description,");
            SqlStatement.Append("CreatedDate");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@ApiKey, ");
            SqlStatement.Append("@Name, ");
            SqlStatement.Append("@Description, ");
            SqlStatement.Append("@CreatedDate");

            SqlStatement.Append(")");

            //CLOSE: Check Row Count ===========================================================
            //SqlStatement.Append(" END");
             
            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = SqlStatement.ToString();



            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@ApiKey", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@Name", SqlDbType.Text);
            sqlCommand.Parameters.Add("@Description", SqlDbType.Text);
            sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.DateTime);

            //Assign values
            sqlCommand.Parameters["@ApiKey"].Value = key.ApiKey;
            sqlCommand.Parameters["@Name"].Value = key.Name;
            sqlCommand.Parameters["@Description"].Value = key.Description;
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
                    "attempting to insert a new api key into SQL",
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
