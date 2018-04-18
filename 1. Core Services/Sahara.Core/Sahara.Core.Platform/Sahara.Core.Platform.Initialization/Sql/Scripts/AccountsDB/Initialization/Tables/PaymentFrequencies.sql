CREATE TABLE [PaymentFrequencies] (

	[PaymentFrequencyMonths]	INT						NOT NULL PRIMARY KEY NONCLUSTERED, --Stripe PlanID = PaymantPlanName + PaymentFrequencyNameKey (sm-1, sm-12, lg-1, lg-12 Etc...)
    [PaymentFrequencyName]		NVARCHAR (80)			COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,

	[Interval]					NVARCHAR (80)			COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,

	[IntervalCount]				INT						NOT NULL,

	[PriceBreak]				DECIMAL(3, 2)			NOT NULL, -- Percentage: Allows you to offer price breaks on extended commitments (yearly, etc)


    
)
GO

--ID is constraied to be unique & clustered
CREATE UNIQUE CLUSTERED INDEX [KeyIndex] ON [dbo].[PaymentFrequencies] ([PaymentFrequencyMonths])
GO

