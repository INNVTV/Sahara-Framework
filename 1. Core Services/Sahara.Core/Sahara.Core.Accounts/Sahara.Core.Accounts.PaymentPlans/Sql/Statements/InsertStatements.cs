using Sahara.Core.Common.ResponseTypes;
using Sahara.Core.Logging.PlatformLogs.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Sahara.Core.Accounts.PaymentPlans.Sql.Statements
{
    internal static class InsertStatements
    {
        public static DataAccessResponseType InsertPaymentPlan(string paymentPlanName,
            bool visibile, decimal monthlyRate, int maxUsers, int maxCategorizationsPerSet, int maxProductsPerSet, int maxProperties, int maxValuesPerProperty, int maxTags,
            bool allowSalesLeads, int monthlySupportHours, bool allowLocationData, bool allowCustomOrdering, bool allowThemes, bool allowImageEnhancements, int maxImageGroups, int maxImageFormats, int maxImageGalleries, int maxImagesPerGallery)
        {
            DataAccessResponseType response = new DataAccessResponseType();

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statement =============================================================
            SqlStatement.Append("INSERT INTO PaymentPlans (");

            SqlStatement.Append("PaymentPlanName,");
            SqlStatement.Append("MonthlyRate,");
            SqlStatement.Append("Visible, ");

            SqlStatement.Append("MaxUsers,");

            SqlStatement.Append("MaxCategorizationsPerSet,");
            SqlStatement.Append("MaxProductsPerSet,");

            SqlStatement.Append("MaxImageGroups,");
            SqlStatement.Append("MaxImageFormats,");
            SqlStatement.Append("MaxImageGalleries,");
            SqlStatement.Append("MaxImagesPerGallery,");

            SqlStatement.Append("MaxTags,");
            SqlStatement.Append("MaxProperties,");
            SqlStatement.Append("MaxValuesPerProperty,");

            SqlStatement.Append("AllowSalesLeads,");
            SqlStatement.Append("MonthlySupportHours,");
            //SqlStatement.Append("BasicSupport,");
            //SqlStatement.Append("EnhancedSupport,");

            SqlStatement.Append("AllowLocationData,");
            SqlStatement.Append("AllowCustomOrdering,");

            SqlStatement.Append("AllowThemes,");
            SqlStatement.Append("AllowImageEnhancements");

            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection

            SqlStatement.Append("@PaymentPlanName, ");
            SqlStatement.Append("@MonthlyRate, ");
            SqlStatement.Append("@Visible, ");

            SqlStatement.Append("@MaxUsers, ");


            //SqlStatement.Append("@MaxCategorizations,");
            SqlStatement.Append("@MaxCategorizationsPerSet,");
            //SqlStatement.Append("@MaxProducts,");
            SqlStatement.Append("@MaxProductsPerSet,");


            SqlStatement.Append("@MaxImageGroups,");
            SqlStatement.Append("@MaxImageFormats,");
            SqlStatement.Append("@MaxImageGalleries,");
            SqlStatement.Append("@MaxImagesPerGallery,");

            SqlStatement.Append("@MaxTags,");
            SqlStatement.Append("@MaxProperties, ");
            SqlStatement.Append("@MaxValuesPerProperty, ");


            SqlStatement.Append("@AllowSalesLeads,");
            SqlStatement.Append("@MonthlySupportHours,");
            //SqlStatement.Append("@BasicSupport,");
            //SqlStatement.Append("@EnhancedSupport,");

            SqlStatement.Append("@AllowLocationData,");
            SqlStatement.Append("@AllowCustomOrdering,");

            SqlStatement.Append("@AllowThemes,");
            SqlStatement.Append("@AllowImageEnhancements");

            SqlStatement.Append(")");

            //SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);
            SqlCommand sqlCommand = Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection.CreateCommand();
			sqlCommand.CommandText = SqlStatement.ToString();
			


            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@PaymentPlanName", SqlDbType.Text);
            sqlCommand.Parameters.Add("@MonthlyRate", SqlDbType.Decimal);
            sqlCommand.Parameters.Add("@Visible", SqlDbType.Bit);

            sqlCommand.Parameters.Add("@MaxUsers", SqlDbType.Int);


            //sqlCommand.Parameters.Add("@MaxCategorizations", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxCategorizationsPerSet", SqlDbType.Int);
            //sqlCommand.Parameters.Add("@MaxProducts", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxProductsPerSet", SqlDbType.Int);


            /*
            sqlCommand.Parameters.Add("@MaxCategories", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxSubcategories", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxSubsubcategories", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxSubsubsubcategories", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxProductsPerSet", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxSubcategoriesPerSet", SqlDbType.Int);
            */

            sqlCommand.Parameters.Add("@MaxImageGroups", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxImageFormats", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxImageGalleries", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxImagesPerGallery", SqlDbType.Int);


            sqlCommand.Parameters.Add("@MaxTags", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxProperties", SqlDbType.Int);
            sqlCommand.Parameters.Add("@MaxValuesPerProperty", SqlDbType.Int);


            sqlCommand.Parameters.Add("@AllowSalesLeads", SqlDbType.Bit);
            sqlCommand.Parameters.Add("@MonthlySupportHours", SqlDbType.Int);
            //sqlCommand.Parameters.Add("@BasicSupport", SqlDbType.Bit);
            //sqlCommand.Parameters.Add("@EnhancedSupport", SqlDbType.Bit);

            sqlCommand.Parameters.Add("@AllowLocationData", SqlDbType.Bit);
            sqlCommand.Parameters.Add("@AllowCustomOrdering", SqlDbType.Bit);

            sqlCommand.Parameters.Add("@AllowThemes", SqlDbType.Bit);
            sqlCommand.Parameters.Add("@AllowImageEnhancements", SqlDbType.Bit);

            //Assign values
            sqlCommand.Parameters["@PaymentPlanName"].Value = paymentPlanName;
            sqlCommand.Parameters["@MonthlyRate"].Value = monthlyRate;
            sqlCommand.Parameters["@Visible"].Value = visibile;

            sqlCommand.Parameters["@MaxUsers"].Value = maxUsers;

            //sqlCommand.Parameters["@MaxCategorizations"].Value = maxCategorizations;
            sqlCommand.Parameters["@MaxCategorizationsPerSet"].Value = maxCategorizationsPerSet;
            //sqlCommand.Parameters["@MaxProducts"].Value = maxProducts;
            sqlCommand.Parameters["@MaxProductsPerSet"].Value = maxProductsPerSet;

            /*
            sqlCommand.Parameters["@MaxCategories"].Value = maxCategories;
            sqlCommand.Parameters["@MaxSubcategories"].Value = maxSubcategories;
            sqlCommand.Parameters["@MaxSubsubcategories"].Value = maxSubsubcategories;
            sqlCommand.Parameters["@MaxSubsubsubcategories"].Value = maxSubsubsubcategories;
            sqlCommand.Parameters["@MaxProductsPerSet"].Value = MaxProductsPerSet;
            sqlCommand.Parameters["@MaxSubcategoriesPerSet"].Value = MaxSubcategoriesPerSet;
            */

            sqlCommand.Parameters["@MaxImageGroups"].Value = maxImageGroups;
            sqlCommand.Parameters["@MaxImageFormats"].Value = maxImageFormats;
            sqlCommand.Parameters["@MaxImageGalleries"].Value = maxImageGalleries;
            sqlCommand.Parameters["@MaxImagesPerGallery"].Value = maxImagesPerGallery;

            sqlCommand.Parameters["@MaxTags"].Value = maxTags;
            sqlCommand.Parameters["@MaxProperties"].Value = maxProperties;
            sqlCommand.Parameters["@MaxValuesPerProperty"].Value = maxValuesPerProperty;


            sqlCommand.Parameters["@AllowSalesLeads"].Value = allowSalesLeads;
            sqlCommand.Parameters["@MonthlySupportHours"].Value = monthlySupportHours;
            //sqlCommand.Parameters["@BasicSupport"].Value = basicSupport;
            //sqlCommand.Parameters["@EnhancedSupport"].Value = enhancedSupport;

            sqlCommand.Parameters["@AllowLocationData"].Value = allowLocationData;
            sqlCommand.Parameters["@AllowCustomOrdering"].Value = allowCustomOrdering;

            sqlCommand.Parameters["@AllowThemes"].Value = allowThemes;
            sqlCommand.Parameters["@AllowImageEnhancements"].Value = allowImageEnhancements;

            int insertAccountResult = 0;

            sqlCommand.Connection.OpenWithRetry();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQueryWithRetry(); // returns Int indicating number of rows affected
                if(insertAccountResult > 0)
                {
                    response.isSuccess = true;
                }
            }
            catch (Exception e)
            {
                //Log exception and email platform admins
                PlatformExceptionsHelper.LogExceptionAndAlertAdmins(
                    e,
                    "attempting to insert a payment plan into SQL",
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
