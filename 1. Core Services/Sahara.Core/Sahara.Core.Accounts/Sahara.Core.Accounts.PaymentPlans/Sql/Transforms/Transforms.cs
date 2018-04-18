using Sahara.Core.Accounts.PaymentPlans.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Accounts.PaymentPlans.Sql.Transforms
{
    internal static class Transforms
    {
        public static PaymentPlan DataReader_to_PaymentPlan(SqlDataReader reader)
        {
            PaymentPlan paymentPlan = new PaymentPlan();

            //paymentPlan.PaymentPlanID = (int)reader["PaymentPlanID"];
            paymentPlan.PaymentPlanName = (String)reader["PaymentPlanName"];

            paymentPlan.MonthlyRate = (Decimal)reader["MonthlyRate"];

            paymentPlan.MaxUsers = (int)reader["MaxUsers"];

            paymentPlan.SearchPlan = (string)reader["SearchPlan"];

            //paymentPlan.MaxCategorizations = (int)reader["MaxCategorizations"];
            paymentPlan.MaxCategorizationsPerSet = (int)reader["MaxCategorizationsPerSet"];
            paymentPlan.MaxProducts = (int)reader["MaxProducts"];
            paymentPlan.MaxProductsPerSet = (int)reader["MaxProductsPerSet"];

            //Derived from PerSet Values

            #region THE MATH BEHIND THE MADNESS

            /*

            		int maxCatsPerSet = 3;
		            int maxProdsPerSet = 6;
		
		
		            int catCount = (((maxCatsPerSet * maxCatsPerSet) * maxCatsPerSet)) * maxCatsPerSet;
		            int prdCount = ((((maxCatsPerSet * maxCatsPerSet) * maxCatsPerSet) * maxProdsPerSet)*maxCatsPerSet);
		
		            int catCount2 = catCount + ((maxCatsPerSet * maxCatsPerSet) * maxCatsPerSet);
		            int catCount3 = catCount2 + ((maxCatsPerSet * maxCatsPerSet));
		            int totalCatCount = catCount3 + maxCatsPerSet;
		
		
		            Console.WriteLine(catCount);
		            Console.WriteLine(catCount2);
		            Console.WriteLine(catCount3);
		            Console.WriteLine(totalCatCount);
		
		            Console.WriteLine(prdCount);
		

            */

            #endregion


            //catCount is the total bottom level of cats if an account uses the deepest levels of subsubsubcategorizations to their maximum benefit
            int catCount = (((paymentPlan.MaxCategorizationsPerSet * paymentPlan.MaxCategorizationsPerSet) * paymentPlan.MaxCategorizationsPerSet)) * paymentPlan.MaxCategorizationsPerSet;
            int catCountLevel2 = catCount + ((paymentPlan.MaxCategorizationsPerSet * paymentPlan.MaxCategorizationsPerSet) * paymentPlan.MaxCategorizationsPerSet);
            int catCountLevel3 = catCountLevel2 + ((paymentPlan.MaxCategorizationsPerSet * paymentPlan.MaxCategorizationsPerSet));
            int totalCatCount = catCountLevel3 + paymentPlan.MaxCategorizationsPerSet;

            paymentPlan.MaxCategorizations = totalCatCount; // (((paymentPlan.MaxCategorizationsPerSet * paymentPlan.MaxCategorizationsPerSet) * paymentPlan.MaxCategorizationsPerSet)) * paymentPlan.MaxCategorizationsPerSet; //(paymentPlan.MaxCategorizationsPerSet * paymentPlan.MaxCategorizationsPerSet);

            //We now derive this from a hard number and use both this as well as productsperset to limit an accounts product count
            //paymentPlan.MaxProducts = (((paymentPlan.MaxCategorizationsPerSet * paymentPlan.MaxCategorizationsPerSet) * paymentPlan.MaxCategorizationsPerSet) * paymentPlan.MaxProductsPerSet) * paymentPlan.MaxCategorizationsPerSet;  //(paymentPlan.MaxCategorizations * paymentPlan.MaxProductsPerSet);

            //paymentPlan.MaxProducts = (int)reader["MaxProducts"];
            //paymentPlan.MaxCategories = (int)reader["MaxCategories"];

            //paymentPlan.MaxProductsPerCategory = (int)reader["MaxProductsPerCategory"];
            //paymentPlan.MaxProductsPerSubcategory = (int)reader["MaxProductsPerSubcategory"];
            //paymentPlan.MaxProductsPerSubsubcategory = (int)reader["MaxProductsPerSubsubcategory"];
            //paymentPlan.MaxProductsPerSubsubsubcategory = (int)reader["MaxProductsPerSubsubsubcategory"];

            //paymentPlan.MaxSubcategoriesPerSet = (int)reader["MaxSubcategoriesPerSet"];
            //paymentPlan.MaxSubcategories = (int)reader["MaxSubcategories"];
            //paymentPlan.MaxSubsubcategoriesPerSet = (int)reader["MaxSubsubcategoriesPerSet"];
            //paymentPlan.MaxSubsubsubcategoriesPerSet = (int)reader["MaxSubsubsubcategoriesPerSet"];

            paymentPlan.MaxProperties = (int)reader["MaxProperties"];
            paymentPlan.MaxValuesPerProperty = (int)reader["MaxValuesPerProperty"];
            paymentPlan.MaxTags = (int)reader["MaxTags"];
            paymentPlan.AllowImageEnhancements = (bool)reader["AllowImageEnhancements"];
            paymentPlan.AllowLocationData = (bool)reader["AllowLocationData"];

            paymentPlan.MaxImageGroups = (int)reader["MaxImageGroups"];
            paymentPlan.MaxImageFormats = (int)reader["MaxImageFormats"];
            paymentPlan.MaxImageGalleries = (int)reader["MaxImageGalleries"];
            paymentPlan.MaxImagesPerGallery = (int)reader["MaxImagesPerGallery"];

            paymentPlan.AllowThemes = (bool)reader["AllowThemes"];
            paymentPlan.AllowSalesLeads = (bool)reader["AllowSalesLeads"];
            paymentPlan.AllowCustomOrdering = (bool)reader["AllowCustomOrdering"];

            paymentPlan.MonthlySupportHours = (int)reader["MonthlySupportHours"];

            //paymentPlan.BasicSupport = (bool)reader["BasicSupport"];
           // paymentPlan.EnhancedSupport = (bool)reader["EnhancedSupport"];

            paymentPlan.Visible = (bool)reader["Visible"];


            ///paymentPlan.MaxSubcategories = (paymentPlan.MaxCategories * paymentPlan.MaxSubcategoriesPerSet);
            //paymentPlan.MaxSubsubcategories = (paymentPlan.MaxSubcategories * paymentPlan.MaxSubsubcategoriesPerSet);
            //paymentPlan.MaxSubsubsubcategories = (paymentPlan.MaxSubsubcategories * paymentPlan.MaxSubsubsubcategoriesPerSet);

            //paymentPlan.MaxProductsInAllSubcategories = (paymentPlan.MaxSubcategories * paymentPlan.MaxProductsPerSubcategory);
            //paymentPlan.MaxProductsInAllSubsubcategories = (paymentPlan.MaxSubsubcategories * paymentPlan.MaxProductsPerSubsubcategory);
            //paymentPlan.MaxProductsInAllSubsubsubcategories = (paymentPlan.MaxSubsubsubcategories * paymentPlan.MaxProductsPerSubsubsubcategory);

            //var maxProducts = Math.Max(paymentPlan.MaxProductsInAllSubcategories, paymentPlan.MaxProductsInAllSubsubcategories);
            //maxProducts = Math.Max(maxProducts, paymentPlan.MaxProductsInAllSubsubsubcategories);

            //paymentPlan.MaxProductsInAccount = maxProducts;


            return paymentPlan;
        }

        public static PaymentFrequency DataReader_to_PaymentFrequency(SqlDataReader reader)
        {
            PaymentFrequency paymentFrequency = new PaymentFrequency();

            paymentFrequency.PaymentFrequencyMonths = (int)reader["PaymentFrequencyMonths"];
            paymentFrequency.PaymentFrequencyName = (String)reader["PaymentFrequencyName"];
            paymentFrequency.PriceBreak = (Decimal)reader["PriceBreak"];
            paymentFrequency.Interval = (String)reader["Interval"];
            paymentFrequency.IntervalCount = (int)reader["IntervalCount"];
            return paymentFrequency;
        }
    }
}
