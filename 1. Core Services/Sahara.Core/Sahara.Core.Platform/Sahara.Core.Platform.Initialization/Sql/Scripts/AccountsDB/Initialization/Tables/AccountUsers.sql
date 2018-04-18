CREATE TABLE [dbo].[AccountUsers] (

    [Id]					NVARCHAR (128)			NOT NULL,
    [UserName]				NVARCHAR (256),			-- email_accountId
	[Email]					NVARCHAR (256),
    [PasswordHash]			NVARCHAR (max),
    [SecurityStamp]			NVARCHAR (max),
    [FirstName]				NVARCHAR (max),
    [LastName]				NVARCHAR (max),
    [Active]				BIT						Default '1',
	[AccountOwner]			BIT						Default '0',
    [CreatedDate]			DATETIME				NOT NULL,
	[AccountID]				UNIQUEIDENTIFIER,
    [AccountName]			NVARCHAR (max),			-- Set to update on trigger
    [AccountNameKey]		NVARCHAR (max),			-- Set to update on trigger
    [Discriminator]			NVARCHAR(128)			NULL,
	[LastActivityDate]		DATETIME				NULL,
	[Photo]					NVARCHAR(180)			NULL,
	[TimeZone]				NVARCHAR(75)			NULL, -- Future Iterations will allow for system to detect account and/or user timezone and note the zone so that UTC times cab be localized for the user and/or account.

	-- New columns for Identity 2.0
	[EmailConfirmed]		BIT						Default '1',  -- We ignore ths as we use the invitation system
	[PhoneNumber]			NVARCHAR (MAX)			NULL,
    [PhoneNumberConfirmed]	BIT						NOT NULL,
	[TwoFactorEnabled]		BIT						NOT NULL,
    [LockoutEndDateUtc]		DATETIME				NULL,
    [LockoutEnabled]		BIT						NOT NULL,
    [AccessFailedCount]		INT						NOT NULL,
	
	--Optional for certain scenarios:

	--[LoginName]			[nvarchar](max),
	--[DisplayName]			[nvarchar](max),

    CONSTRAINT [PK_dbo.AccountUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AccountUsers]([UserName] ASC)
GO