using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
namespace Sahara.Core.Accounts.PaymentPlans.Models
{
    [Serializable]
    [DataContract]
    public class PaymentPlan
    {
        #region Refactoring notes
        /*
            -- Adding a new "AllowRegistration" bool to the PaymentPlan object (commented out below) will allow for validation of selected plans coming in from users*
            * for scenarios where users can choose a plan while signing up to avoid passing in ID's for plans such as "Unlimited" which must be approved by a Platform Admin
        */
        #endregion

        //[DataMember]
        //public int PaymentPlanID { get; set; }

        //[DataMember]
        //public string DocumentPartitionTierID { get; set; }

        [DataMember]
        public string PaymentPlanName { get; set; } // PaymentPlanName is the PlanID

        [DataMember]
        public string SearchPlan { get; set; } // Free, Basic or Standard

        [DataMember]
        public decimal MonthlyRate { get; set; }

        [DataMember]
        public List<AlternateRate> AlternateRates { get; set; } // Used as a helper to describe alternate rates to users

        [DataMember]
        public bool Visible { get; set; }


        //[DataMember]
        //public List<String> StripeVarients { get; set; }

        #region Plan Limitations

        [DataMember]
        public int MaxCategorizations { get; set; }
        [DataMember]
        public int MaxCategorizationsPerSet { get; set; }

        [DataMember]
        public int MaxProducts { get; set; }
        [DataMember]
        public int MaxProductsPerSet { get; set; }
        


        [DataMember]
        public int MaxUsers { get; set; }

        [DataMember]
        public int MonthlySupportHours { get; set; }

        //[DataMember]
        //public int MaxCategories { get; set; }


        //[DataMember]
        //public int MaxProductsPerCategory { get; set; }
        //[DataMember]
        //public int MaxProductsPerSubcategory { get; set; }
        //[DataMember]
        //public int MaxProductsPerSubsubcategory { get; set; }
        //[DataMember]
        //public int MaxProductsPerSubsubsubcategory { get; set; }

        //[DataMember]
        //public int MaxProductsInAllCategories { get; set; }
        //[DataMember]
        //public int MaxProductsInAllSubcategories { get; set; }
        //[DataMember]
        //public int MaxProductsInAllSubsubcategories { get; set; }
        //[DataMember]
        //public int MaxProductsInAllSubsubsubcategories { get; set; }

        //[DataMember]
        //public int MaxProductsInAccount { get; set; } //< Highest numb from larget of above, checked against count




        //[DataMember]
        //public int MaxSubcategoriesPerSet { get; set; }
        //[DataMember]
        //public int MaxSubsubcategoriesPerSet { get; set; }
        //[DataMember]
        //public int MaxSubsubsubcategoriesPerSet { get; set; }
        //[DataMember]
        //public int MaxSubcategories { get; set; }
        //[DataMember]
        //public int MaxSubsubcategories { get; set; }
        //[DataMember]
        //public int MaxSubsubsubcategories { get; set; }



        [DataMember]
        public int MaxProperties { get; set; }
        [DataMember]
        public int MaxValuesPerProperty { get; set; }
        [DataMember]
        public int MaxTags { get; set; }
        [DataMember]
        public bool AllowThemes { get; set; }
        [DataMember]
        public bool AllowSalesLeads { get; set; }
        [DataMember]
        public bool AllowLocationData { get; set; }
        [DataMember]
        public bool AllowImageEnhancements { get; set; }

        [DataMember]
        public int MaxImageGroups { get; set; }
        [DataMember]
        public int MaxImageFormats { get; set; }
        [DataMember]
        public int MaxImageGalleries { get; set; }
        [DataMember]
        public int MaxImagesPerGallery { get; set; }

        [DataMember]
        public bool AllowCustomOrdering { get; set; }

        //[DataMember]
        //public bool BasicSupport { get; set; }
        //[DataMember]
        //public bool EnhancedSupport { get; set; }

        //[See refactoring notes above]
        //[DataMember]
        //public bool AllowRegistration { get; set; }

        #endregion

    }
}
