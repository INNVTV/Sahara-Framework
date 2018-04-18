CREATE TABLE [Accounts] (

	[AccountID]					UNIQUEIDENTIFIER		NOT NULL PRIMARY KEY NONCLUSTERED,
    [AccountName]				NVARCHAR (80)			COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [AccountNameKey]			NVARCHAR (80)			COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,

	[PhoneNumber]				NVARCHAR (80)			NULL,

	[StripeCustomerID]			NVARCHAR (180)			NULL, -- If null, indicates that the user has not made any monetary transactions yet
	[StripeSubscriptionID]		NVARCHAR (180)			NULL, -- If null, indicates that a payed subscription has not been created yet. Subscription contains an PlanId (AKA PaymentPlanName-PaymentFrequencyName)
	[StripeCardID]				NVARCHAR (180)			NULL, -- If null, indicates that the user has not made any monetary transactions yet and/or does not have a card on file

	[PaymentPlanName]			NVARCHAR (80)			NULL DEFAULT 'Unprovisioned', -- Foreign Key (Default is '0' for Unprovisioned accounts
	[PaymentFrequencyMonths]	INT						NULL DEFAULT 0, -- Foreign Key

    [Active]					BIT						NOT NULL DEFAULT 0, -- Is currently active (Applied once provisioning request has been started by the platform admins)
	
	[AccountEndDate]			DATETIME				NULL,  -- Used when a closed account has a remainder of it's subscription balance left. The Pro-rated is used to figure out account end date. Custodian will check daily to purge accounts. Can also be used to set a day to close an account that needs to be purged.
	[ClosureApproved]			BIT						NOT NULL DEFAULT 0, -- Paid Accounts must first be approved by a PlatformAdmin before Custodian closes the account. This allows you to verify a cloasure verbally or archive data before custodian processes the closure.
	[Closed]					BIT						NOT NULL DEFAULT 0, -- Account is closed, will be deprovisioned by Custodian Worker on next run

	--[Locked]					BIT						NOT NULL DEFAULT 0, -- Account is locked if true, usually for failed payment
	[LockedDate]				DATETIME				NULL, -- Date that account was locked, NULL if unlocked. Account will be deleted by Custodian after X number of days

	-- PARTITIONING ----

	--[DocumentPartition]			NVARCHAR (140)			NULL, -- Will be [AccountNameKey]
	[SearchPartition]			NVARCHAR (140)			NULL,
	[StoragePartition]		    NVARCHAR (140)			NULL,
    [SqlPartition]				NVARCHAR (120)			NULL, 	

	-- PROVIIONING ----

    [Provisioned]				BIT						NOT NULL DEFAULT 0, 
    [ProvisionedDate]			DATETIME				NULL, 
	
	[Logo]						NVARCHAR(38)			NULL,
	[TimeZone]					NVARCHAR(75)			NULL, -- Future Iterations will allow for system to detect account and/or user timezone and note the zone so that UTC times cab be localized for the user and/or account.	
	[CardExpiration]			DATETIME				NULL, -- Used for dunning purposes by The Custodian. Set & Updated with card updates
	[Credits]					INT						NOT NULL DEFAULT 0, -- Amount of money available to spend on services outside of typical monthly subscription fees or immediate micro transactions. Can be removed or refactored.

	--Delinquent represents BOTH customer.delinquent & subscription.status
	[Delinquent]				BIT						NOT NULL DEFAULT 0, -- If true, indicates that a Charge with an associated invoice has failed and the account is currently in the dunning process. Usially a credit card needs to be updated. Managed by TWO Stripe WebHooks: 1. customer.updated(delinquent=true) event and 2. customer.subscription.updated(status=past_due/active) event
	[CreatedDate]				DATETIME				NOT NULL,

)
GO

--ID & Name are constraied to be unique (created as a GUID and registered by the application)
CREATE UNIQUE INDEX [KeyIndex] ON [dbo].[Accounts] ([AccountID])
GO

CREATE UNIQUE INDEX [NameIndex] ON [dbo].[Accounts] ([AccountName])
GO

--NameKey is constraied to be unique
--NameKey will be the hot query, it's unique, can be used to search by routes and as a subdomain so we CLUSTER our index on this: 
CREATE UNIQUE CLUSTERED INDEX [NameKeyClusteredIndex] ON [dbo].[Accounts] ([AccountNameKey])
GO

