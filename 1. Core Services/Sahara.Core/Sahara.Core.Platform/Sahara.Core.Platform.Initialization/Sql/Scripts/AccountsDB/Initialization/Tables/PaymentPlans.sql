CREATE TABLE [PaymentPlans] (

	--[PaymentPlanID]			INT						NOT NULL PRIMARY KEY NONCLUSTERED,
    [PaymentPlanName]		NVARCHAR (80)			COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL PRIMARY KEY NONCLUSTERED,

	-- Plan Cost Per Month

	[MonthlyRate]			DECIMAL(8, 2)			NOT NULL, -- '0' if Trial or Free


	-- Plan Limitations

	[MaxUsers]							INT				NOT NULL,

	--[MaxCategorizations]				INT				NOT NULL, -- Now derived from max categorizations per set count
	[MaxCategorizationsPerSet]			INT				NOT NULL,

	[MaxProducts]						INT				NOT NULL, -- No longer derived from max categories x maxproducts per set count, we now have 2 layers of defense for product limitations
	[MaxProductsPerSet]					INT				NOT NULL, 


	[MonthlyStorage]					INT				NOT NULL DEFAULT 0, -- In MB
	[MonthlyCDN]						INT				NOT NULL DEFAULT 0, -- In MB
	[MonthlyTransactional]				INT				NOT NULL DEFAULT 0, -- In MB



	--[MaxCategories]							INT				NOT NULL,
	--[MaxSubcategoriesPerSet]					INT				NOT NULL, 
	--[MaxSubsubcategoriesPerSet]				INT				NOT NULL, 
	--[MaxSubsubsubcategoriesPerSet]			INT				NOT NULL,
	--[MaxProductsPerCategory]					INT				NOT NULL,
	--[MaxProductsPerSubcategory]				INT				NOT NULL,
	--[MaxProductsPerSubsubcategory]			INT				NOT NULL,
	--[MaxProductsPerSubsubsubcategory]			INT				NOT NULL,

	[MaxProperties]						INT				NOT NULL,
	[MaxValuesPerProperty]				INT				NOT NULL, --Used for swatches too
	[MaxTags]							INT				NOT NULL,
	[MaxImageGroups]					INT				NOT NULL, 
	[MaxImageFormats]					INT				NOT NULL,
	[MaxImageGalleries]					INT				NOT NULL,  
	[MaxImagesPerGallery]				INT				NOT NULL,
	  
	[AllowThemes]						BIT				NOT NULL DEFAULT 0,
	[AllowImageEnhancements]			BIT				NOT NULL DEFAULT 0, 
	[AllowSalesLeads]					BIT				NOT NULL DEFAULT 0,
	[AllowLocationData]					BIT				NOT NULL DEFAULT 0,
	[AllowCustomOrdering]				BIT				NOT NULL DEFAULT 0,
	[MonthlySupportHours]				INT				NOT NULL DEFAULT 0, 
	--[EnhancedSupport]					BIT				NOT NULL DEFAULT 0, 

	[SearchPlan]						NVarChar(20)    NOT NULL, 

	-- Plan Visibility

	[Visible]				BIT						NOT NULL DEFAULT 1, -- Show or hide as an option to users
	--[AllowRegistration]				BIT						NOT NULL DEFAULT 1, -- Allows this plan to be publicly registered by users for scenarios where users can choose a plan while signing up
)
GO

--ID is constraied to be unique & clustered
CREATE UNIQUE CLUSTERED INDEX [KeyIndex] ON [dbo].[PaymentPlans] ([PaymentPlanName])
GO



-- Default behaviour is to start out all accounts with a limited trial account and prompt user for payment once trial ends or a limitation is reached.
-- With some refactoring you can start them directly with a paying plan by passing a planid parameter to the Registration site and having the registration site pass in the planID along with C.C. info for Stripe on Plans with a MonthlyRate > 0
-- and adding Credit Card capture to the Registration site if a plan with a MonthlyRate above 0 is selected -->

