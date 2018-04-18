-- Moved to Table Storage

/*CREATE TABLE [FailedPaymentAttempts] (

	--Each recent failed attempt is tracked per AccountID.
	--We track failed attempts in order to lock accounts after X number of failed attempts (as dicated by application settings)
	--An account that is locked passed X number of days (as dicated by application settings) will be deleted by the Custodian
	--Once a payment is finally processed this table is cleared for that AccountID resulting in a count of 0 failed payment attempts
	
	[TransactionID]			UNIQUEIDENTIFIER		NOT NULL PRIMARY KEY NONCLUSTERED, --Each payment attempt is given a unique transaction id by the application

	[AccountID]				UNIQUEIDENTIFIER		NOT NULL, -- Foriegn Key ()
    [AccountName]			NVARCHAR (80)			COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    [AccountNameKey]		NVARCHAR (80)			COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,

	[ErrorMessage]			NVARCHAR (Max)			COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ErrorCode]				NVARCHAR (120)			COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

	[TransactionAmount]		DECIMAL					NOT NULL,
	[TransactionDate]		DATETIME				NOT NULL,

    
)


GO

--TransactionID is constraied to be unique (created as a GUID dueing payment processing attempt by the application)
CREATE UNIQUE INDEX [KeyIndex] ON [dbo].[FailedPaymentAttempts] ([TransactionID])
GO

--We CLUSTER on AccountID to improve grouping: 
CREATE CLUSTERED INDEX [AccountIDClusteredIndex] ON [dbo].[FailedPaymentAttempts] ([AccountID])
GO

--We create a composite index on TransactionDate & AccountID to improve order by calls on TransactionDate with an AccountID Cluster: 
CREATE INDEX [TransactionDateClusteredIndex] ON [dbo].[FailedPaymentAttempts] ([AccountID], [TransactionDate])
GO*/