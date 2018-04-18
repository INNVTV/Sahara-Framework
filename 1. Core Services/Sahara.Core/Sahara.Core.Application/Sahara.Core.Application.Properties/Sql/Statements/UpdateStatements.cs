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
    internal static class UpdateStatements
    {
        internal static bool UpdatePropertyListingState(string sqlPartition, string schemaId, string propertyId, bool listingState)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Property ");

            sqlStatement.Append("SET Listing = '");
            sqlStatement.Append(listingState);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PropertyID = '");
            sqlStatement.Append(propertyId);
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

        internal static bool UpdatePropertyDetailsState(string sqlPartition, string schemaId, string propertyId, bool detailsState)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Property ");

            sqlStatement.Append("SET Details = '");
            sqlStatement.Append(detailsState);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PropertyID = '");
            sqlStatement.Append(propertyId);
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

        internal static bool UpdatePropertyFacetableState(string sqlPartition, string schemaId, string propertyId, bool isFacetable)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Property ");

            sqlStatement.Append("SET Facetable = '");
            sqlStatement.Append(isFacetable);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PropertyID = '");
            sqlStatement.Append(propertyId);
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

        internal static bool UpdatePropertySortableState(string sqlPartition, string schemaId, string propertyId, bool isSortable)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Property ");

            sqlStatement.Append("SET Sortable = '");
            sqlStatement.Append(isSortable);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PropertyID = '");
            sqlStatement.Append(propertyId);
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

        internal static bool UpdatePropertyAppendableState(string sqlPartition, string schemaId, string propertyId, bool isAppendable)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Property ");

            sqlStatement.Append("SET Appendable = '");
            sqlStatement.Append(isAppendable);
            sqlStatement.Append("'");

            sqlStatement.Append(" WHERE PropertyID = '");
            sqlStatement.Append(propertyId);
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

        internal static bool UpdatePropertyFacetInterval(string sqlPartition, string schemaId, string propertyId, int newFacetInterval)
        {
            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Property ");

            sqlStatement.Append("SET FacetInterval = @FacetInterval");

            sqlStatement.Append(" WHERE PropertyID = '");
            sqlStatement.Append(propertyId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();




            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@FacetInterval", SqlDbType.Int);

            sqlCommand.Parameters["@FacetInterval"].Value = newFacetInterval;

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static bool UpdatePropertySymbol(string sqlPartition, string schemaId, string propertyId, string symbol)
        {
            bool isSuccess = false;
            StringBuilder sqlStatement = new StringBuilder();
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();

            if (String.IsNullOrEmpty(symbol) || symbol == "" || symbol == " " || symbol == "  " || symbol == "   " || symbol == "    " || symbol == "     " || symbol == "     " || symbol == "      " || symbol == "       " || symbol == "        ")
            {
                //SQL Statement =============================================================
                sqlStatement.Append("UPDATE  ");
                sqlStatement.Append(schemaId);
                sqlStatement.Append(".Property ");

                sqlStatement.Append("SET Symbol = NULL");

                sqlStatement.Append(" WHERE PropertyID = '");
                sqlStatement.Append(propertyId);
                sqlStatement.Append("'");
            }
            else
            {
                //SQL Statement =============================================================
                sqlStatement.Append("UPDATE  ");
                sqlStatement.Append(schemaId);
                sqlStatement.Append(".Property ");

                sqlStatement.Append("SET Symbol = @Symbol");

                sqlStatement.Append(" WHERE PropertyID = '");
                sqlStatement.Append(propertyId);
                sqlStatement.Append("'");

                sqlCommand.Parameters.Add("@Symbol", SqlDbType.NVarChar);
                sqlCommand.Parameters["@Symbol"].Value = symbol;
            }

            sqlCommand.CommandText = sqlStatement.ToString();
            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static bool UpdatePropertyNumericDescriptor(string sqlPartition, string schemaId, string propertyId, string numericDescriptor)
        {
            bool isSuccess = false;
            StringBuilder sqlStatement = new StringBuilder();
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();

            if (String.IsNullOrEmpty(numericDescriptor) || numericDescriptor == "" || numericDescriptor == " " || numericDescriptor == "  " || numericDescriptor == "   " || numericDescriptor == "    " || numericDescriptor == "     " || numericDescriptor == "     " || numericDescriptor == "      " || numericDescriptor == "       " || numericDescriptor == "        ")
            {
                //SQL Statement =============================================================
                sqlStatement.Append("UPDATE  ");
                sqlStatement.Append(schemaId);
                sqlStatement.Append(".Property ");

                sqlStatement.Append("SET NumericDescriptor = NULL");

                sqlStatement.Append(" WHERE PropertyID = '");
                sqlStatement.Append(propertyId);
                sqlStatement.Append("'");
            }
            else
            {
                //SQL Statement =============================================================
                sqlStatement.Append("UPDATE  ");
                sqlStatement.Append(schemaId);
                sqlStatement.Append(".Property ");

                sqlStatement.Append("SET NumericDescriptor = @NumericDescriptor");

                sqlStatement.Append(" WHERE PropertyID = '");
                sqlStatement.Append(propertyId);
                sqlStatement.Append("'");

                sqlCommand.Parameters.Add("@NumericDescriptor", SqlDbType.NVarChar);
                sqlCommand.Parameters["@NumericDescriptor"].Value = numericDescriptor;
            }

            sqlCommand.CommandText = sqlStatement.ToString();
            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static bool UpdatePropertySymbolPlacement(string sqlPartition, string schemaId, string propertyId, string symbolPlacement)
        {
            if (symbolPlacement == "leading" || symbolPlacement == "trailing")
            {
                //Do nothing
            }
            else
            {
                return false;
            }

            bool isSuccess = false;

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE  ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Property ");

            sqlStatement.Append("SET SymbolPlacement = @SymbolPlacement");

            sqlStatement.Append(" WHERE PropertyID = '");
            sqlStatement.Append(propertyId);
            sqlStatement.Append("'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();




            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@SymbolPlacement", SqlDbType.NVarChar);

            sqlCommand.Parameters["@SymbolPlacement"].Value = symbolPlacement;

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result >= 1)
            {
                isSuccess = true;
            }

            return isSuccess;
        }

        internal static DataAccessResponseType SetFeaturedProperties(string sqlPartition, string schemaId, Dictionary<string, int> featuredOrderingDictionary)
        {
            var response = new DataAccessResponseType();

            StringBuilder sqlStatement = new StringBuilder();


            //FIRST we set ALL to 0
            sqlStatement.Append("UPDATE ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Property ");
            sqlStatement.Append("SET FeaturedID = '0'; ");

            //THEN we apply order id to all

            //SQL Statement =============================================================
            foreach (KeyValuePair<string, int> ordering in featuredOrderingDictionary)
            {
                try
                {
                    
                    sqlStatement.Append("UPDATE  ");
                    sqlStatement.Append(schemaId);
                    sqlStatement.Append(".Property ");

                    sqlStatement.Append("SET FeaturedID = '");
                    sqlStatement.Append(ordering.Value);
                    sqlStatement.Append("' ");

                    sqlStatement.Append("WHERE PropertyID = '");
                    sqlStatement.Append(ordering.Key);
                    sqlStatement.Append("'; ");
                }
                catch (Exception e)
                {
                    return new DataAccessResponseType { isSuccess = false, ErrorMessage = "An error occurred while looping through the dictionary: " + e.Message };
                }
            }

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();


            if (result > 0)
            {
                response.isSuccess = true;
            }

            return response;
        }


        internal static DataAccessResponseType ResetFeaturedProperties(string sqlPartition, string schemaId)
        {
            var response = new DataAccessResponseType();

            StringBuilder sqlStatement = new StringBuilder();

            //SQL Statement =============================================================
            sqlStatement.Append("UPDATE ");
            sqlStatement.Append(schemaId);
            sqlStatement.Append(".Property ");
            sqlStatement.Append("SET FeaturedID = '0'");

            //SqlCommand sqlCommand = new SqlCommand(sqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition));
            SqlCommand sqlCommand = Sahara.Core.Settings.Azure.Databases.DatabaseConnections.DatabasePartitionSqlConnection(sqlPartition).CreateCommand();
            sqlCommand.CommandText = sqlStatement.ToString();

            sqlCommand.Connection.OpenWithRetry();
            int result = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected

            sqlCommand.Connection.Close();

            if (result > 0)
            {
                response.isSuccess = true;
            }

            return response;
        }
    }
}
