CREATE TABLE [dbo].[PlatformUsers] (

    [Id]					NVARCHAR(128)			NOT NULL,
    [UserName]				NVARCHAR(256),
    [PasswordHash]			NVARCHAR(max),
    [SecurityStamp]			NVARCHAR(max),
    [FirstName]				NVARCHAR(max),
    [LastName]				NVARCHAR(max),
    [Active]				BIT					    Default '1',
	[CreatedDate]			DATETIME				NOT NULL,
    [Discriminator]			NVARCHAR(128)			NULL,
	[Photo]					NVARCHAR(180)			NULL,
	[TimeZone]				NVARCHAR(75)			NULL, -- Future Iterations will allow for system to detect platform and/or user timezone and note the zone so that UTC times cab be localized for the user and/or platform.

	-- New columns for Identity 2.0
	[Email]				   NVARCHAR(256)			Default '',
	[EmailConfirmed]       BIT						Default '1',  -- We ignore ths and use 'Verified'
	[PhoneNumber]          NVARCHAR (MAX)			NULL,
    [PhoneNumberConfirmed] BIT						NOT NULL,
	[TwoFactorEnabled]     BIT						NOT NULL,
    [LockoutEndDateUtc]    DATETIME					NULL,
    [LockoutEnabled]       BIT						NOT NULL,
    [AccessFailedCount]    INT						NOT NULL,
	

    CONSTRAINT [PK_dbo.PlatformUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[PlatformUsers]([UserName] ASC)
GO